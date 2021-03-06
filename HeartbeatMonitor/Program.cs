﻿using System.Threading;
using It.Unina.Dis.Logbus.Clients;
using It.Unina.Dis.Logbus.Filters;
using System;
using It.Unina.Dis.Logbus;
namespace HeartbeatMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This program monitors only heartbeats coming from any entity in the system.");
            Console.WriteLine("Press CTRL+C when you are finished.");

            PropertyFilter idFilt = new PropertyFilter
                                        {
                                             comparison = ComparisonOperator.eq,
                                             propertyName = Property.MessageID,
                                             value = "HEARTBEAT"
                                         };
            SeverityFilter debugFilter = new SeverityFilter { comparison = ComparisonOperator.eq, severity = SyslogSeverity.Debug };
            
            using (ILogClient client = ClientHelper.CreateUnreliableClient(idFilt & debugFilter))
            {
                client.MessageReceived += client_MessageReceived;
                client.Start();
                Thread.Sleep(Timeout.Infinite);
            }
        }

        static void client_MessageReceived(object sender, SyslogMessageEventArgs e)
        {
            SyslogAttributes attrs = e.Message.GetAdvancedAttributes();
            Console.WriteLine("Got heartbeat from ({0}|{1}|{2})", e.Message.Host,
                              e.Message.ProcessID ?? e.Message.ApplicationName, attrs.LogName);
        }
    }
}
