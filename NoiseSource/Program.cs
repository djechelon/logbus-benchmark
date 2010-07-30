﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Loggers;
using System.Threading;

namespace NoiseSource
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Usage: NoiseSource TIMEOUT");
                Environment.Exit(0);
            }

            int timeout;
            if (!int.TryParse(args[0], out timeout))
            {
                Console.WriteLine("Invalid timeout");
                Environment.Exit(1);
            }

            Console.WriteLine("Ready to send infinite messages with {0}ms timeout", timeout);

            ILogCollector server = LoggerHelper.CreateDefaultCollector();
            string host = System.Net.Dns.GetHostName();
            while (true)
            {
                SyslogMessage start_msg = new SyslogMessage()
                {
                    Timestamp = DateTime.Now,
                    Facility = SyslogFacility.Printer,
                    Severity = SyslogSeverity.Debug,
                    Host = host,
                    MessageId = "NOISE",
                    Text = "The quick brown fox jumps over the lazy dog"
                };
                server.SubmitMessage(start_msg);

                if (timeout > 0) Thread.Sleep(timeout);
            }
        }
    }
}
