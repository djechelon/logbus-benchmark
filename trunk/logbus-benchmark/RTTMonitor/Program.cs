using System;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Collectors;
using It.Unina.Dis.Logbus.Loggers;
using It.Unina.Dis.Logbus.Clients;
using It.Unina.Dis.Logbus.Filters;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Globalization;

namespace RTTMonitor
{
    /// <summary>
    /// This program estimates the Round Trip Time for log messages sent to Logbus
    /// </summary>
    class Program
    {
        static ILogCollector _target;
        static ILogClient _source;
        static ILog _log;
        static readonly AutoResetEvent _ar = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            Console.WriteLine("This program periodically measures the RTT for log messages sent by here");

            PropertyFilter hostFilter = new PropertyFilter
                                            {
                                                comparison = ComparisonOperator.eq,
                                                value = Dns.GetHostName(),
                                                propertyName = Property.Host
                                            };
            PropertyFilter pidFilter = new PropertyFilter
                                           {
                                               comparison = ComparisonOperator.eq,
                                               value =
                                                   Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture),
                                               propertyName = Property.ProcessID
                                           };
            PropertyFilter idFilter = new PropertyFilter
                                          {
                                              value = "RTT",
                                              comparison = ComparisonOperator.eq,
                                              propertyName = Property.MessageID
                                          };

            _target = CollectorHelper.CreateCollector("udp");
            _source = ClientHelper.CreateDefaultClient(hostFilter & pidFilter & idFilter);
            _log = LoggerHelper.GetLogger("Logfile");

            _source.MessageReceived += source_MessageReceived;
            _source.Start();

            while (true)
            {
                DateTime curTimestamp = DateTime.UtcNow;
                double millis = (curTimestamp - DateTime.Today).TotalMilliseconds;
                SyslogMessage message = new SyslogMessage(SyslogFacility.Local3, SyslogSeverity.Notice, millis.ToString(CultureInfo.InvariantCulture))
                {
                    MessageId = "RTT"
                };

                _target.SubmitMessage(message);

                if (!_ar.WaitOne(5000))
                {
                    Console.WriteLine("RTT sent at {0} lost", curTimestamp);
                    _log.Warning("RTT sent at {0} lost", curTimestamp);
                    continue;
                }

                Thread.Sleep(3000);
            }
        }

        static void source_MessageReceived(object sender, SyslogMessageEventArgs e)
        {
            double curMillis = (DateTime.UtcNow - DateTime.Today).TotalMilliseconds;
            double milliseconds = double.Parse(e.Message.Text, CultureInfo.InvariantCulture);

            double rtt = Math.Round(curMillis - milliseconds, 3);
            Console.WriteLine("Current RTT value: {0,3}ms", rtt);

            _log.Debug("RTT: {0,3}", rtt.ToString(CultureInfo.InvariantCulture));

            _ar.Set();
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _source.Dispose();
        }
    }
}
