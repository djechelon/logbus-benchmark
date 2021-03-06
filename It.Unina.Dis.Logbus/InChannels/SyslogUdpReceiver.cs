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
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using It.Unina.Dis.Logbus.Utils;

namespace It.Unina.Dis.Logbus.InChannels
{
    /// <summary>
    /// Collects Syslog messages over UDP unicast channels
    /// </summary>
    /// <remarks>Implements RFC5426</remarks>
    internal class SyslogUdpReceiver :
        ReceiverBase
    {

        private int _receivedMessages, _parseErrors;

        #region Constructor
        public SyslogUdpReceiver()
        {
            ReceiveBufferSize = -1;

            MessageReceived += delegate
                                   {
                                       Interlocked.Increment(ref _receivedMessages);
                                   };

            ParseError += delegate
                              {
                                  Interlocked.Increment(ref _parseErrors);
                              };
        }

        public SyslogUdpReceiver(int port)
            : this()
        {
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port", port, "Port must be in the range of 0-65535");
            Port = port;
        }
        #endregion

        /// <summary>
        /// Default port to listen
        /// </summary>
        public const int DEFAULT_PORT = 514;

        /// <summary>
        /// Default number of worker threads
        /// </summary>
        public const int WORKER_THREADS = 4;

        private Thread[] _listenerThreads, _parserThreads;


        private IFifoQueue<byte[]>[] _byteQueues;
        private int _currentQueue;
        private bool _listen;

        /// <summary>
        /// Port to listen on
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Interface to listen on
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the receive buffer size of UDP
        /// </summary>
        public int ReceiveBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the UdpClient to use
        /// </summary>
        protected UdpClient Client { get; set; }

        /// <summary>
        /// Implements IRunnable.Start
        /// </summary>
        protected override void OnStart()
        {
            if (Port == 0)
            {
                Port = DEFAULT_PORT;
            }

            Client = InitClient();

            _listen = true;
            _listenerThreads = new Thread[WORKER_THREADS];
            _parserThreads = new Thread[WORKER_THREADS];
            _byteQueues = new IFifoQueue<byte[]>[WORKER_THREADS];
            _currentQueue = int.MinValue;
            for (int i = 0; i < WORKER_THREADS; i++)
            {
                _byteQueues[i] = new FastFifoQueue<byte[]>(16384);

                _listenerThreads[i] = new Thread(ListenerLoop)
                                          {
                                              Name = string.Format("SyslogUdpReceiver[{1}].ListenerLoop[{0}]", i, ToString()),
                                              IsBackground = true,
                                              Priority = ThreadPriority.AboveNormal
                                          };
                _listenerThreads[i].Start();

                _parserThreads[i] = new Thread(ParserLoop)
                                        {
                                            Name = string.Format("SyslogUdpReceiver[{1}].ParserLoop[{0}]", i, ToString()),
                                            IsBackground = true
                                        };
                _parserThreads[i].Start(i);
            }

        }

        /// <summary>
        /// Performs stop operations on UDP receiver
        /// </summary>
        protected override void OnStop()
        {
            _listen = false;
            try
            {
                Client.Close(); //Trigger SocketException if thread is blocked into listening
                for (int i = 0; i < WORKER_THREADS; i++)
                    _listenerThreads[i].Join();
                _listenerThreads = null;
            }
            catch
            {
            } //Really nothing?

            try
            {
                for (int i = 0; i < WORKER_THREADS; i++)
                    _parserThreads[i].Interrupt();
                for (int i = 0; i < WORKER_THREADS; i++)
                    _parserThreads[i].Join();
                _parserThreads = null;
            }
            catch
            {
            }
        }

        protected virtual UdpClient InitClient()
        {

            IPEndPoint localEp = IpAddress == null
                                 ? new IPEndPoint(IPAddress.Any, Port)
                                 : new IPEndPoint(IPAddress.Parse(IpAddress), Port);

            Socket clientSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ExclusiveAddressUse = true,
            };

            if (ReceiveBufferSize >= 0) clientSock.ReceiveBufferSize = ReceiveBufferSize;

            clientSock.Bind(localEp);
            return new UdpClient { Client = clientSock };
        }

        #region IConfigurable Membri di

        /// <summary>
        /// Implements IConfigurable.GetConfigurationParameter
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override string GetConfigurationParameter(string key)
        {
            if (Disposed) throw new ObjectDisposedException(GetType().FullName);
            switch (key)
            {
                case "ip":
                    return IpAddress;
                case "port":
                    return Port.ToString(CultureInfo.InvariantCulture);
                case "receiveBufferSize":
                    return ReceiveBufferSize.ToString(CultureInfo.InvariantCulture);
                default:
                    {
                        throw new NotSupportedException("Configuration parameter is not supported");
                    }
            }
        }

        /// <summary>
        /// Implements IConfigurable.SetConfigurationParameter
        /// </summary>
        public override void SetConfigurationParameter(string key, string value)
        {
            if (Disposed) throw new ObjectDisposedException(GetType().FullName);
            switch (key)
            {
                case "ip":
                    {
                        IpAddress = value;
                        break;
                    }
                case "port":
                    {
                        Port = int.Parse(value);
                        break;
                    }
                case "receiveBufferSize":
                    {
                        ReceiveBufferSize = int.Parse(value);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("Configuration parameter is not supported");
                    }
            }
        }

        #endregion

        private void ParserLoop(object queue)
        {
            int queueId = (int)queue;
            try
            {
                while (true)
                {
                    byte[] payload = _byteQueues[queueId].Dequeue();

                    try
                    {
                        SyslogMessage newMessage = SyslogMessage.Parse(payload);
                        ForwardMessage(newMessage);
                    }
                    catch (FormatException ex)
                    {
                        ParseErrorEventArgs e = new ParseErrorEventArgs(payload, ex, false);
                        OnParseError(e);
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
            finally
            {
                byte[][] finalMessages = _byteQueues[queueId].FlushAndDispose();
                if (finalMessages.GetLength(0) > 0)
                {
                    Log.Notice("Inbound channel {0} still needs to process {0} pending messages. Delaying stop.",
                               ToString(), finalMessages.GetLength(0));
                }
                foreach (byte[] payload in finalMessages)
                {
                    try
                    {
                        SyslogMessage newMessage = SyslogMessage.Parse(payload);
                        ForwardMessage(newMessage);
                    }
                    catch (FormatException ex)
                    {
                        ParseErrorEventArgs e = new ParseErrorEventArgs(payload, ex, false);
                        OnParseError(e);
                    }
                }
            }
        }

        private void ListenerLoop()
        {
            IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
            while (_listen)
            {
                try
                {
                    byte[] payload = Client.Receive(ref remoteEndpoint);

                    _byteQueues[
                        (((Interlocked.Increment(ref _currentQueue)) % WORKER_THREADS) + WORKER_THREADS) % WORKER_THREADS].
                        Enqueue(payload);
                }
                catch (SocketException)
                {
                    //We are closing, or an I/O error occurred
                    //if (Stopped) //Yes, we are closing
                    //return;
                    //else nothing yet
                }
                catch (Exception)
                {
                } //Really do nothing? Shouldn't we stop the service?
            }
        }

        public override string ToString()
        {
            return string.Format("SyslogUdpReceiver:{0}:{1}", IpAddress ?? "*", Port.ToString(CultureInfo.InvariantCulture));
        }

        protected override void LogStatistics()
        {
            string[] queuesStatus = new string[WORKER_THREADS];
            for (int i = 0; i < WORKER_THREADS; i++)
                queuesStatus[i] = _byteQueues[i].Count.ToString(CultureInfo.CurrentUICulture);
                
            
            Log.Debug("Status of {0}. Received during last minute: {1}. Parse errors: {2}. Buffer queues holding ({3}).",
                ToString(),
                Interlocked.Exchange(ref _receivedMessages, 0),
                Interlocked.Exchange(ref _parseErrors, 0),
                string.Join(",", queuesStatus)
                );

        }
    }
}