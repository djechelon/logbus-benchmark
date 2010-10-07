using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using It.Unina.Dis.Logbus;

namespace Syslog2Csv
{
    class Program
    {
        private const String OUTPUT_FILE = "output.csv";

        static void Main(string[] args)
        {
            try
            {
                // Devi passare un argomento...
                if (args.Length != 1)
                {
                    Console.WriteLine("Usage: Syslog2Csv.exe filepath(or wildcard expression)");
                    Environment.Exit(1);
                }

                // Capisce il percorso e le eventuali espressioni wildcard...
                String path = Path.GetDirectoryName(args[0]);
                String filename = Path.GetFileName(args[0]);
                if (String.IsNullOrEmpty(path) || String.IsNullOrEmpty(filename))
                    return;

                // Legge tutti i file di log passati:
                String[] files = Directory.GetFiles(path, filename);
                List<SyslogMessage> list = new List<SyslogMessage>();
                for (int i = 0; i < files.Length; i++)
                {
                    TextReader tr = File.OpenText(Path.Combine(path, files[i]));
                    String tmp = tr.ReadLine();
                    while (tmp != null)
                    {
                        list.Add(SyslogMessage.Parse(tmp));
                        tmp = tr.ReadLine();
                    }
                    tr.Close();
                }

                // Crea il file Csv a partire dalla lista caricata...
                if (File.Exists(OUTPUT_FILE))
                    File.Delete(OUTPUT_FILE);
                TextWriter tw = File.CreateText(OUTPUT_FILE);
                tw.WriteLine(@"Id;UtcTimestamp;Host;RTT");
                long index = 0, lost = 0;
                foreach (SyslogMessage message in list)
                {
                    if (message.Severity == SyslogSeverity.Debug)
                    {

                        String row = String.Join(";", new string[]
                                                          {
                                                              index.ToString(),
                                                              message.Timestamp.ToString(),
                                                              message.Host,
                                                              message.Text.Split(' ')[1]
                                                          });
                        index++;
                        tw.WriteLine(row);
                    }
                    else if (message.Severity == SyslogSeverity.Warning)
                    {
                        if (message.Text.Contains("lost"))
                            lost++;
                        else if (message.Text.Contains("dropped"))
                            lost--;
                    }
                }
                tw.Close();
                Console.WriteLine("There were {0} messages lost complexively", lost);
            }
            catch
            {
                Environment.Exit(1);
            }
        }

    }
}
