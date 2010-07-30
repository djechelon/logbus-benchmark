using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using System.Collections.Specialized;
using System.Threading;
using System.Collections;

namespace LogbusCore
{
    class Program
    {
        static Dictionary<string, BitArray> messages_rcvd;

        static void Main(string[] args)
        {
            messages_rcvd = new Dictionary<string, BitArray>();

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
                messages_rcvd[e.Message.Host][msgid] = true;
            }
            else if (e.Message.MessageId == "RESET")
            {
                Console.WriteLine("Preparing to receive {0} messages from {1}", e.Message.Text, e.Message.Host);
                int msgnum = int.Parse(e.Message.Text);
                messages_rcvd.Add(e.Message.Host, new BitArray(msgnum));
            }
            else if (e.Message.MessageId == "END")
            {
                Console.WriteLine("Client stopped sending messages");
                Console.WriteLine("Counting how many dropped...");
                Console.WriteLine();

                int dropped = 0;
                foreach (bool rcvd in messages_rcvd[e.Message.Host])
                    if (!rcvd) dropped++;

                messages_rcvd.Remove(e.Message.Host);

                Console.WriteLine("Dropped {0} messages for host {1}", dropped, e.Message.Host);
            }
        }
    }
}
