using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Clients;
using System.Diagnostics;
using It.Unina.Dis.Logbus.RemoteLogbus;
using It.Unina.Dis.Logbus.Filters;
using System.Threading;

namespace UselessMonitor
{
    /// <summary>
    /// This program allocates a number of "fake" channels on Logbus just to increase Logbus
    /// CPU usage
    /// 
    /// Parameter: number of channels to create
    /// </summary>
    class Program
    {
        static string[] chan_ids;
        static IChannelManagement chan_manager;

        static void Main(string[] args)
        {
            chan_manager = ClientHelper.CreateChannelManager();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Usage: UselessMonitor NUMBER_OF_CHANNELS");
                Environment.Exit(0);
            }

            int chans;
            if (!int.TryParse(args[0], out chans))
            {
                Console.WriteLine("Invalid channel number");
                Environment.Exit(1);
            }

            if (chans < 1)
            {
                Console.WriteLine("Please, at least one...");
                Environment.Exit(1);
            }

            chan_ids = new string[chans];
            for (int i = 0; i < chans; i++)
            {
                string chan_id = string.Format("UM_{0}_{1}", Process.GetCurrentProcess().Id, i);
                ChannelCreationInformation info = new ChannelCreationInformation()
                {
                    coalescenceWindow = 0,
                    description = "Automatic null channel",
                    filter = new FalseFilter(),
                    id = chan_id,
                    title = "Auto"
                };
                chan_ids[i] = chan_id;
                chan_manager.CreateChannel(info);
            }

            Thread.Sleep(Timeout.Infinite);

            Console.WriteLine("Created {0} channels to disturb Logbus. Exit when you're done", chans);
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            foreach (string id in chan_ids)
                chan_manager.DeleteChannel(id);
        }
    }
}
