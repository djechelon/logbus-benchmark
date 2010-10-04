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
        private static volatile int _id = 0;

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
                _id++;
                DateTime curTimestamp = DateTime.UtcNow;
                double millis = (curTimestamp - DateTime.Today).TotalMilliseconds;
                string text = _id.ToString(CultureInfo.InvariantCulture) + "_" + millis.ToString(CultureInfo.InvariantCulture);
                SyslogMessage message = new SyslogMessage(SyslogFacility.Local3, SyslogSeverity.Notice, text)
                {
                    MessageId = "RTT"
                };

                _target.SubmitMessage(message);

                if (!_ar.WaitOne(10000))
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
            try
            {
                string[] split = e.Message.Text.Split('_');
                int msgId = int.Parse(split[0]);
                if (msgId != _id)
                {
                    _log.Warning("Old message received");
                    Console.WriteLine("Warning: old message received. It was already dropped");
                    return;
                }

                double curMillis = (DateTime.UtcNow - DateTime.Today).TotalMilliseconds;
                double milliseconds = double.Parse(split[1], CultureInfo.InvariantCulture);

                double rtt = Math.Round(curMillis - milliseconds, 3);
                Console.WriteLine("Current RTT value: {0,3}ms", rtt);

                _log.Debug("RTT: {0,3}", rtt.ToString(CultureInfo.InvariantCulture));
                _ar.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex);
            }
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _source.Dispose();
            Console.WriteLine("Program terminated");
        }
    }
}
