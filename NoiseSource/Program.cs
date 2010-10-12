using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Collectors;
using It.Unina.Dis.Logbus.Loggers;
using System.Threading;

namespace NoiseSource
{
    /// <summary>
    /// This program continuously sends garbage messages to Logbus in order to increase network traffic
    /// and Logbus CPU usage
    /// 
    /// Parameter: number of milliseconds between each messages
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Usage: NoiseSource ([MESSAGE_RATE]|\"inf\")");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("\tMESSAGE_RATE:\t number of messages sent per second");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Example: \tNoiseSource 1500");
                Console.WriteLine("sends 1500 messages per second");
                Console.WriteLine("\tNoiseSource inf");
                Console.WriteLine("sends infinite messages, flooding network socket");
                Environment.Exit(0);
            }
            int rate = 0;

            if (args[0] != "inf" && !int.TryParse(args[0], out rate) || rate == 0)
            {
                Console.WriteLine("Invalid rate");
                Environment.Exit(1);
            }
            long timeout = 10000000 / rate; //100ns timeout

            if (timeout == 0)
                Console.WriteLine("Ready to send infinite messages at infinite rate");
            else
                Console.WriteLine("Ready to send infinite messages at a rate of {0}/s", rate);

            ILogCollector server = CollectorHelper.CreateCollector();
            string host = System.Net.Dns.GetHostName();
            while (true)
            {
                SyslogMessage startMsg = new SyslogMessage
                {
                    Timestamp = DateTime.Now,
                    Facility = SyslogFacility.Printer,
                    Severity = SyslogSeverity.Debug,
                    Host = host,
                    MessageId = "NOISE",
                    Text = "The quick brown fox jumps over the lazy dog"
                };
                server.SubmitMessage(startMsg);

                if (rate > 0) Thread.Sleep(new TimeSpan(timeout));
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Program terminated");
        }
    }
}
