using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Loggers;
using It.Unina.Dis.Logbus.Clients;
using It.Unina.Dis.Logbus.Filters;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.IO;
using System.Globalization;

namespace RTTMonitor
{
    /// <summary>
    /// This program estimates the Round Trip Time for log messages sent to Logbus
    /// </summary>
    class Program
    {
        static ILogCollector target;
        static ILogClient source;
        static string filename = null;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            if (args != null && args.Length > 0)
            {
                filename = args[0];
            }

            Console.WriteLine("This program periodically measures the RTT for log messages sent by here");

            AndFilter rtt_filter = new AndFilter();
            PropertyFilter pid_filter = new PropertyFilter()
            {
                comparison = ComparisonOperator.eq,
                value = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture),
                propertyName = Property.ProcessID
            };
            PropertyFilter id_filter = new PropertyFilter()
            {
                value = "RTT",
                comparison = ComparisonOperator.eq,
                propertyName = Property.MessageID
            };

            rtt_filter.filter = new FilterBase[] { pid_filter, id_filter };

            target = LoggerHelper.CreateDefaultCollector();
            source = ClientHelper.CreateDefaultClient(rtt_filter);

            source.MessageReceived += new SyslogMessageEventHandler(source_MessageReceived);
            source.Start();

            while (true)
            {
                DateTime cur_timestamp = DateTime.Now;
                double millis = (cur_timestamp - DateTime.Today).TotalMilliseconds;
                SyslogMessage message = new SyslogMessage(SyslogFacility.Local3, SyslogSeverity.Notice, millis.ToString(CultureInfo.InvariantCulture))
                {
                    MessageId = "RTT"
                };

                target.SubmitMessage(message);

                Thread.Sleep(3000);
            }
        }

        static void source_MessageReceived(object sender, SyslogMessageEventArgs e)
        {
            double cur_millis =(DateTime.Now - DateTime.Today).TotalMilliseconds;
            double milliseconds = double.Parse(e.Message.Text,CultureInfo.InvariantCulture);

            double rtt = Math.Round(cur_millis - milliseconds, 3);
            Console.WriteLine("Current RTT value: {0}", rtt);

            if (filename != null)
                using (TextWriter tw = new StreamWriter(File.OpenWrite(filename)))
                    tw.WriteLine(rtt);
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            source.Dispose();
        }
    }
}
