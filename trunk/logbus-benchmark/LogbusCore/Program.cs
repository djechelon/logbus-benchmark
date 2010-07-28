using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using System.Collections.Specialized;
using System.Threading;

namespace LogbusCore
{
    class Program
    {
        static bool[] messages_rcvd;

        static void Main(string[] args)
        {
            ILogBus logbus = LogbusSingletonHelper.Instance;

            logbus.MessageReceived += new SyslogMessageEventHandler(logbus_MessageReceived);

            logbus.Start();

            Console.WriteLine("Server is active. Status will be printed on this console. Run SysglogSource with appropriate parameters to proceed with the benchmark");

            Thread.Sleep(Timeout.Infinite);
        }

        static void logbus_MessageReceived(object sender, SyslogMessageEventArgs e)
        {
            if (e.Message.MessageId == "BENCH")
            {
                int msgid = int.Parse(e.Message.Text);
                messages_rcvd[msgid] = true;
            }
            else if (e.Message.MessageId == "RESET")
            {
                Console.WriteLine("Preparing to receive {0} messages", e.Message.Text);
                int msgnum = int.Parse(e.Message.Text);
                messages_rcvd = new bool[msgnum];
            }
            else if (e.Message.MessageId == "END")
            {
                Console.WriteLine("Client stopped sending messages");
                Console.WriteLine("Counting how many dropped...");
                Console.WriteLine();

                int dropped = 0;
                foreach (bool rcvd in messages_rcvd)
                    if (!rcvd) dropped++;

                Console.WriteLine("Dropped {0} messages", dropped);
            }
        }
    }
}
