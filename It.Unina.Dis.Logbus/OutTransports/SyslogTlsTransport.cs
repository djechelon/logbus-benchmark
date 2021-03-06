﻿/*
 *                  Logbus-ng project
 *    ©2010 Logbus Reasearch Team - Some rights reserved
 *
 *  Created by:
 *      Vittorio Alfieri - vitty85@users.sourceforge.net
 *      Antonio Anzivino - djechelon@users.sourceforge.net
 *
 *  Based on the research project "Logbus" by
 *
 *  Dipartimento di Informatica e Sistemistica
 *  University of Naples "Federico II"
 *  via Claudio, 21
 *  80121 Naples, Italy
 *
 *  Software is distributed under Microsoft Reciprocal License
 *  Documentation under Creative Commons 3.0 BY-SA License
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using It.Unina.Dis.Logbus.Loggers;
using It.Unina.Dis.Logbus.Utils;
using System.Net;

namespace It.Unina.Dis.Logbus.OutTransports
{
    internal class SyslogTlsTransport
        : IOutboundTransport, ILogSupport
    {
        private readonly Dictionary<string, TlsClient> _clients;
        private bool _disposed;
        private readonly ReaderWriterLock _listLock;
        private readonly Thread _worker;
        private readonly IFifoQueue<SyslogMessage> _queue;
        private readonly Timer _tmrCleaner, _tmrStatistics;
        private long _messagesSent;

        private const int DEFAULT_JOIN_TIMEOUT = 5000;
        private const int TIMER_CYCLE_TIMEOUT = 60000;

        #region Constructor/Destructor

        public SyslogTlsTransport(X509Certificate2 serverCert, bool validateClientCert)
        {
            _clients = new Dictionary<string, TlsClient>();
            _listLock = new ReaderWriterLock();
            ServerCertificate = serverCert;
            ValidateClientCertificate = validateClientCert;
            _queue = new FastFifoQueue<SyslogMessage>(2048);
            _worker = new Thread(DispatchLoop)
                          {
                              Name = "SyslogTlsTransport.DispatchLoop",
                              IsBackground = true
                          };
            _worker.Start();

            _tmrCleaner = new Timer(CleanupClients, null, TIMER_CYCLE_TIMEOUT, TIMER_CYCLE_TIMEOUT);
            _tmrStatistics = new Timer(LogStatistics, null, TIMER_CYCLE_TIMEOUT, TIMER_CYCLE_TIMEOUT);
        }

        ~SyslogTlsTransport()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            GC.SuppressFinalize(this);

            _disposed = true;

            _worker.Interrupt();
            _tmrCleaner.Dispose();
            _tmrStatistics.Dispose();

            if (disposing)
            {
                //We need to be the only one thread into here
                _listLock.AcquireWriterLock(Timeout.Infinite);
                try
                {
                    foreach (KeyValuePair<string, TlsClient> kvp in _clients)
                    {
                        try
                        {
                            kvp.Value.Client.Close();
                        }
                        catch
                        {
                        } //Don't care
                    }
                    _clients.Clear();
                }
                finally
                {
                    _listLock.ReleaseWriterLock();
                }
            }
            _worker.Join(DEFAULT_JOIN_TIMEOUT);
        }

        #endregion

        /// <summary>
        /// Certificate for TLS client
        /// </summary>
        public X509Certificate2 ServerCertificate { get; private set; }

        /// <summary>
        /// Whether to validate the SSL server certificate of new clients
        /// </summary>
        public bool ValidateClientCertificate { get; private set; }

        #region IOutboundTransport Membri di

        public int SubscribedClients
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                _listLock.AcquireReaderLock(DEFAULT_JOIN_TIMEOUT);
                try
                {
                    return _clients.Count;
                }
                finally
                {
                    _listLock.ReleaseReaderLock();
                }
            }
        }

        /// <summary>
        /// Implements IOutboundTransport.SubscribeClient
        /// </summary>
        /// <remarks>
        /// Input instructions: (both required)
        /// <list>
        /// <item><c>host</c>: host name to connect to and validate certificate for</item>
        /// <item><c>port</c>: port number to use</item>
        /// <item><c>ip</c> (optional): overrides host, which is still required for certificate validation</item>
        /// </list>
        /// 
        /// Output instructions: none
        /// </remarks>
        public string SubscribeClient(IEnumerable<KeyValuePair<string, string>> inputInstructions,
                                      out IEnumerable<KeyValuePair<string, string>> outputInstructions)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (inputInstructions == null) throw new ArgumentNullException("inputInstructions");

            string host = null, portno = null;
            int port;
            IPAddress ipOverride = null;

            foreach (KeyValuePair<string, string> kvp in inputInstructions)
            {
                switch (kvp.Key)
                {
                    case "host":
                        {
                            host = kvp.Value;
                            break;
                        }
                    case "port":
                        {
                            portno = kvp.Value;
                            break;
                        }
                    case "ip":
                        {
                            if (!IPAddress.TryParse(kvp.Value, out ipOverride))
                                throw new TransportException("Invalid IP address specified");
                            break;
                        }
                }
            }

            //Parameter error detection
            if (host == null) throw new TransportException("Host name not specified");
            if (portno == null) throw new TransportException("Port number not specified");
            if (!int.TryParse(portno, out port)) throw new TransportException("Invalid TCP port number");
            if (port < 1 || port > 65534) throw new TransportException("Invalid TCP port number");

            outputInstructions = new Dictionary<string, string>();

            try
            {
                TcpClient newTcpClient;

                if (ipOverride == null)
                    newTcpClient = new TcpClient(host, port);
                else
                {
                    newTcpClient = new TcpClient(ipOverride.AddressFamily) { NoDelay = true, SendBufferSize = 65536 };
                    newTcpClient.Connect(ipOverride, port);
                }

                SslStream sslStream = new SslStream(newTcpClient.GetStream(), false, RemoteCertificateValidation,
                                                    LocalCertificateSelection);

                sslStream.AuthenticateAsClient(host);

                TlsClient newClient = new TlsClient(newTcpClient, sslStream);
                string id = newClient.GetHashCode().ToString();

                _listLock.AcquireWriterLock(DEFAULT_JOIN_TIMEOUT);
                try
                {
                    _clients.Add(id, newClient);
                    return id;
                }
                finally
                {
                    _listLock.ReleaseWriterLock();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Unable to subscribe new TLS client");
                Log.Debug("Error details: {0}", ex);
                throw new TransportException("Unable to subscribe client", ex);
            }
        }

        /// <summary>
        /// Implements IOutboundTransport.RequiresRefresh
        /// </summary>
        public bool RequiresRefresh
        {
            get { return false; }
        }

        /// <summary>
        /// Implements IOutboundTransport.SubscriptionTtl
        /// </summary>
        public long SubscriptionTtl
        {
            get { throw new NotSupportedException("This transport doesn't support time to live"); }
        }

        /// <summary>
        /// Implements IOutboundTransport.RefreshClient
        /// </summary>
        public void RefreshClient(string clientId)
        {
            throw new NotSupportedException("This transport doesn't support refreshing subscriptions");
        }

        /// <summary>
        /// Implements IOutboundTransport.UnsubscribeClient
        /// </summary>
        public void UnsubscribeClient(string clientId)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            if (string.IsNullOrEmpty(clientId)) throw new ArgumentNullException("clientId");

            TlsClient client = null;

            _listLock.AcquireReaderLock(DEFAULT_JOIN_TIMEOUT);
            try
            {
                if (!_clients.ContainsKey(clientId)) throw new TransportException("Client not subscribed to transport");
                client = _clients[clientId];

                //Remove
                LockCookie ck = _listLock.UpgradeToWriterLock(DEFAULT_JOIN_TIMEOUT);
                try
                {
                    _clients.Remove(clientId);
                }
                finally
                {
                    _listLock.DowngradeFromWriterLock(ref ck);
                }
            }
            finally
            {
                _listLock.ReleaseReaderLock();
                if (client != null)
                {
                    try
                    {
                        client.Stream.Flush();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        client.Dispose();
                    }
                }
            }
        }

        #endregion

        #region ILogCollector Membri di

        public void SubmitMessage(SyslogMessage message)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _queue.Enqueue(message);
        }

        #endregion

        #region IDisposable Membri di

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
        }

        #endregion

        #region ILogSupport Membri di

        public ILog Log { private get; set; }

        #endregion

        private void DispatchLoop()
        {
            try
            {
                while (true)
                {
                    SyslogMessage[] msgs = _queue.Flush();
                    if (msgs == null || msgs.Length == 0) msgs = new[] { _queue.Dequeue() };

                    byte[] data;

                    using (MemoryStream ms = new MemoryStream(8192))
                    {
                        foreach (SyslogMessage msg in msgs)
                        {
                            byte[] payload = Encoding.UTF8.GetBytes(msg.ToRfc5424String());
                            foreach (char c in payload.Length.ToString(CultureInfo.InvariantCulture))
                                ms.WriteByte((byte)c);

                            ms.WriteByte((byte)' ');

                            ms.Write(payload, 0, payload.Length);
                            _messagesSent++;
                        }
                        data = ms.ToArray();
                    }

                    //Waiting for pending writes

                    //Critical section. Obtain a snapshot of list and release lock ASAP
                    TlsClient[] clients;
                    _listLock.AcquireReaderLock(DEFAULT_JOIN_TIMEOUT);
                    try
                    {
                        clients = new TlsClient[_clients.Count];
                        _clients.Values.CopyTo(clients, 0);
                    }
                    finally
                    {
                        _listLock.ReleaseReaderLock();
                    }

                    foreach (TlsClient client in clients)
                    {
                        if (client.AsyncResult != null)
                        {
                            try
                            {
                                client.Stream.EndWrite(client.AsyncResult);
                                client.AsyncResult = null;
                            }
                            catch (ObjectDisposedException) { }
                            catch (IOException ex)
                            {
                                Log.Warning("Unable to send paylod to TLS client {0}", client.Client.Client.RemoteEndPoint.ToString());
                                Log.Debug("Error details: {0}", ex.Message);
                            }
                        }
                    }

                    foreach (TlsClient client in clients)
                    {
                        try
                        {
                            /* Asynchronously writing data to buffer.
                             * Once writing is completed, the callback signals the AutoResetEvent
                             * and once all clients signal the AutoResetEvent, new data will be written.
                             * Meanwhile, TlsClient encodes new data to send
                             */
                            client.AsyncResult = client.Stream.BeginWrite(data, 0, data.Length, null, null);
                        }
                        catch (IOException ex)
                        {
                            Log.Warning("Unable to send paylod to TLS client {0}", client.Client.Client.RemoteEndPoint.ToString());
                            Log.Debug("Error details: {0}", ex.Message);
                        }
                        catch (ObjectDisposedException) { }
                    }
                }
            }
            catch (ThreadInterruptedException) { }
            catch (Exception ex)
            {
                Log.Error("Failed TLS cycle in SyslogTlsTransport");
                Log.Debug("Error details: {0}", ex.Message);
                Dispose();
            }
        }

        private void CleanupClients(object state)
        {
            List<string> toRemove = new List<string>();

            /* Scan for dead clients
             * According to MSDN, a client will be detected to be dead
             * only after a failed I/O.
             * If no messages run on channel, lots of dead clients won't be
             * discovered. This is why we force flushing the stream: if client
             * is dead, IOException is triggered and Connected is set to false
             */
            _listLock.AcquireReaderLock(DEFAULT_JOIN_TIMEOUT);
            try
            {
                foreach (KeyValuePair<string, TlsClient> kvp in _clients)
                {
                    try
                    {
                        kvp.Value.Stream.Flush();
                    }
                    catch (IOException)
                    {
                    } //Flush did its real job

                    if (!kvp.Value.Client.Connected) toRemove.Add(kvp.Key);
                }

                if (toRemove.Count > 0)
                {
                    LockCookie ck = _listLock.UpgradeToWriterLock(DEFAULT_JOIN_TIMEOUT);
                    try
                    {
                        foreach (string id in toRemove)
                        {
                            try
                            {
                                TlsClient client = _clients[id];
                                _clients.Remove(id);
                                client.Dispose();
                            }
                            catch (KeyNotFoundException ex)
                            {
                                //Strange
                                Log.Debug("Error occurred when purging dead TLS client: {0}", ex.Message);
                            }
                        }
                    }
                    finally
                    {
                        _listLock.DowngradeFromWriterLock(ref ck);
                    }
                }
            }
            finally
            {
                _listLock.ReleaseReaderLock();
            }
            if (toRemove.Count > 0)
                Log.Notice("TLS transport {0} cleaned up {1} dead clients", GetHashCode(), toRemove.Count);
        }

        private bool RemoteCertificateValidation(Object sender, X509Certificate certificate, X509Chain chain,
                                                 SslPolicyErrors sslPolicyErrors)
        {
            if (!ValidateClientCertificate) return true;

            throw new NotImplementedException("No validation logic yet");
        }

        private X509Certificate LocalCertificateSelection(Object sender, string targetHost,
                                                          X509CertificateCollection localCertificates,
                                                          X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return ServerCertificate;
        }

        private void LogStatistics(object state)
        {
            Log.Debug("During the last minute TLS transport {0} sent {1} messages. {2} messages in queue.",
                GetHashCode(), _messagesSent, _queue.Count);
            _messagesSent = 0;
        }

        /// <summary>
        /// Support class for TLS clients
        /// </summary>
        private class TlsClient
            : IDisposable
        {
            public TlsClient(TcpClient client, SslStream stream)
            {
                Client = client;
                Stream = stream;
            }

            public readonly TcpClient Client;

            public readonly SslStream Stream;

            public IAsyncResult AsyncResult;

            #region IDisposable Membri di

            public void Dispose()
            {
                Client.Close();
            }

            #endregion
        }
    }
}