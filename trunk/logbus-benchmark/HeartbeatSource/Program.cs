using It.Unina.Dis.Logbus.Loggers;
using System.Threading;
using System;
namespace HeartbeatSource
{
    class Program
    {
        static void Main(string[] args)
        {
            uint entities = 1;
            Console.WriteLine("This program periodically sends heartbeats to Logbus.");
            if (args.Length > 0)
            {
                if (uint.TryParse(args[0], out entities)) Console.WriteLine("Using {0} entities for this process", entities);
                else
                {
                    Console.WriteLine("Usage: HeartbeatSource [ENTITIES]");
                    Console.WriteLine("\tENTITIES:\tNumber of entities that this program will simulate");
                    Environment.Exit(1);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Activating {0} entities. This may take a while as the program waits a little time between activations...");
            ILog[] logs = new ILog[entities];

            Random rand = new Random();
            for (int i = 0; i < entities; i++)
            {
                logs[i] = LoggerHelper.GetLogger("HbEntity-" + i.ToString());
                Thread.Sleep(rand.Next(3000));
            }

            Console.WriteLine();
            Console.WriteLine("Program running. No status will be reported here.");
            Console.WriteLine("Press CTRL+C when you want to stop.");
            Thread.Sleep(Timeout.Infinite);
        }


    }
}
