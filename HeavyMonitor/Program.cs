using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Clients;
using It.Unina.Dis.Logbus.Loggers;
using System.Threading;

namespace HeavyMonitor
{
    /// <summary>
    /// This program tries to overload Logbus by subscribing many and many times to the same channel
    /// in order to increase outbound traffic
    /// </summary>
    class Program
    {
        const int CLIENTS_NUMBER = 50;
        static ILogClient[] client_array;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            string chan_name;
            int clients = CLIENTS_NUMBER;

            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Usage: HeavyMonitor CHANNEL_ID [NUM_CLIENTS]");
                Environment.Exit(1);
            }

            chan_name = args[0];
            if (args.Length > 1)
            {
                clients = int.Parse(args[1]);
            }

            client_array = new ILogClient[clients];
            for (int i = 0; i < clients; i++)
            {
                client_array[i] = ClientHelper.CreateDefaultClient(chan_name);
                client_array[i].Start();
            }

            Console.WriteLine("Subscribed {0} clients to channel {1}. Press CTRL+C when you're done.", clients, chan_name);

            Thread.Sleep(Timeout.Infinite);
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            foreach (ILogClient client in client_array)
                client.Dispose();
        }
    }
}
