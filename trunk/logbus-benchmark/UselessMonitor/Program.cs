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
        private static string[] _chanIds;
        private static IChannelManagement _chanManager;
        private static ILogClient[] _clients;

        static void Main(string[] args)
        {
            _chanManager = ClientHelper.CreateChannelManager();

            //Console.CancelKeyPress += Console_CancelKeyPress;

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

            _chanIds = new string[chans];
            _clients = new ILogClient[chans];

            try
            {
                for (int i = 0; i < chans; i++)
                {
                    string chanId = string.Format("UM_{0}_{1}", Process.GetCurrentProcess().Id, i);
                    ChannelCreationInformation info = new ChannelCreationInformation
                    {
                        coalescenceWindow = 0,
                        description = "Automatic null channel",
                        filter = new TrueFilter(),
                        id = chanId,
                        title = "Auto"
                    };
                    _chanIds[i] = chanId;
                    _chanManager.CreateChannel(info);

                    _clients[i] = ClientHelper.CreateUnreliableClient(_chanIds[i]);
                    _clients[i].Start();


                }
                Console.WriteLine("Created {0} channels to disturb Logbus. Press ENTER when you're done", chans);

                Console.ReadLine();
            }
            finally
            {
                for (int i = 0; i < chans; i++)
                {
                    _clients[i].Dispose();
                    _clients[i] = null;

                    _chanManager.DeleteChannel(_chanIds[i]);
                }
            }

        }

        /* Waiting for Mono CTRL+C bug to be fixed
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            foreach (string id in _chanIds)
                _chanManager.DeleteChannel(id);
        }*/
    }
}
