using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Clients;
using It.Unina.Dis.Logbus.Filters;
using It.Unina.Dis.Logbus.Collectors;
using System.Threading;
using System.Runtime.InteropServices;

namespace LossCounter
{
    class Program
    {
        private static BitArray _messagesRcvd;
        private static int _received, _expected;
        private static readonly AutoResetEvent Stop = new AutoResetEvent(false);


        [DllImport("libc.so.6")]
        private extern static void nanosleep(ref timespec rqtp, ref timespec rmtp);

        internal struct timespec
        {
            public IntPtr tv_sec;
            public IntPtr tv_nsec;
        }

        private static void Nanosleep(int ns)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix) throw new PlatformNotSupportedException("Only in Mono/Unix");

            timespec rqtp = new timespec
                       {
                           tv_sec = new IntPtr(0),
                           tv_nsec = new IntPtr(ns)
                       };

            timespec rmtp = new timespec();

            nanosleep(ref rqtp, ref rmtp);
        }

        static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("Usage: LossCounter NUMBER_OF_MESSAGES RATE");
                Environment.Exit(0);
            }


            if (!int.TryParse(args[0], out _expected))
            {
                Console.WriteLine("Invalid number of messages");
                Environment.Exit(1);
            }
            int rate = 0;

            if (args[1] != "inf" && !int.TryParse(args[1], out rate) || rate == 0)
            {
                Console.WriteLine("Invalid rate");
                Environment.Exit(1);
            }


            int timeout = 1000000000 / rate; //1ns timeout
            if (timeout == 0)
                Console.WriteLine("Ready to send {0} messages at infinite rate", _expected);
            else
                Console.WriteLine("Ready to send {0} messages at a rate of {1}/s, timeout {2}ns", _expected, rate, timeout);

            Run(timeout);
            Console.WriteLine("Completed");
        }

        private static void Run(int timeout)
        {
            _messagesRcvd = new BitArray(_expected, false);
            _received = 0;
            string pid = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);

            ILogCollector server = CollectorHelper.CreateCollector();
            string host = Dns.GetHostName();
            PropertyFilter hostFilter = new PropertyFilter
            {
                comparison = ComparisonOperator.eq,
                value = host,
                propertyName = Property.Host
            };
            FacilityEqualsFilter facFilter = new FacilityEqualsFilter { facility = Facility.Local4 };
            PropertyFilter pidFilter = new PropertyFilter
            {
                comparison = ComparisonOperator.eq,
                value = pid,
                propertyName = Property.ProcessID
            };
            PropertyFilter idFilter = new PropertyFilter
            {
                value = "BENCH",
                comparison = ComparisonOperator.eq,
                propertyName = Property.MessageID
            };
            SeverityFilter sevFilter = new SeverityFilter
                                           {
                                               comparison = ComparisonOperator.eq,
                                               severity = Severity.Notice
                                           };
            using (ILogClient client = ClientHelper.CreateUnreliableClient(hostFilter & pidFilter & idFilter & sevFilter & facFilter))
            {
                client.MessageReceived += client_MessageReceived;
                client.Start();

                for (int i = 0; i < _expected; i++)
                {
                    if (timeout > 0) Nanosleep(timeout);
                    SyslogMessage message = new SyslogMessage
                                                {
                                                    Timestamp = DateTime.Now,
                                                    Facility = SyslogFacility.Local4,
                                                    Severity = SyslogSeverity.Notice,
                                                    Host = host,
                                                    ProcessID = pid,
                                                    MessageId = "BENCH",
                                                    Text = i.ToString()
                                                };

                    server.SubmitMessage(message);
                }
                Console.WriteLine("Waiting up to 3 minutes for messages to be all delivered back here...");
                if (Stop.WaitOne(new TimeSpan(0, 0, 3, 0)))
                    Console.WriteLine("Received all messaged");
                else
                    Console.WriteLine("Lost {0} messages", _expected - _received);
            }
        }

        static void client_MessageReceived(object sender, SyslogMessageEventArgs e)
        {
            try
            {
                _received++;
                int msgid = int.Parse(e.Message.Text);
                _messagesRcvd[msgid] = true;
            }
            catch { }

            if (_received == _expected) Stop.Set();

        }

    }
}
