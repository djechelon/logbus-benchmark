using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using It.Unina.Dis.Logbus;
using It.Unina.Dis.Logbus.Loggers;

namespace SyslogSource
{
    class Program
    {

        static void Main(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Console.WriteLine("Usage: SyslogSource NUMBER_OF_MESSAGES TIMEOUT");
                Environment.Exit(0);
            }

            int messages, timeout;
            if (!int.TryParse(args[0], out messages))
            {
                Console.WriteLine("Invalid number of messages");
                Environment.Exit(1);
            }
            if (!int.TryParse(args[1], out timeout))
            {
                Console.WriteLine("Invalid timeout");
                Environment.Exit(1);
            }

            Console.WriteLine("Ready to send {0} messages with {1}ms timeout, ETA {2} seconds", messages, timeout, (messages * timeout) / 1000);
            Run(messages, timeout);
            Console.WriteLine("Completed");
        }

        /// <summary>
        /// Sends LOTS of Syslog messages to the default target.
        /// The first message is marked with MSGID "RESET" and Text equal to the number of messages that will be sent
        /// All other messages have MSGID "BENCH" and Text as incrementing counter
        /// Final message has MSGID "END"
        /// 
        /// First the program sends RESET, then waits 3 seconds to make sure (really sure) that server received that
        /// message for first
        /// 
        /// After RESET and timeout, all other messages are sent one after another
        /// 
        /// After the final message, there are 5 seconds of wait and the final END message
        /// </summary>
        /// <param name="messages">Number of messages to send</param>
        /// <param name="timeout">Timeout between messages</param>
        private static void Run(int messages, int timeout)
        {
            ILogCollector server = LoggerHelper.CreateDefaultCollector();
            SyslogMessage start_msg = new SyslogMessage()
            {
                Timestamp = DateTime.Now,
                Facility = SyslogFacility.Local4,
                Severity = SyslogSeverity.Notice,
                Host = "bench-host",
                MessageId = "RESET",
                Text = messages.ToString()
            };
            server.SubmitMessage(start_msg);

            System.Threading.Thread.Sleep(3000);

            for (int i = 0; i < messages; i++)
            {
                SyslogMessage message = new SyslogMessage()
                {
                    Timestamp = DateTime.Now,
                    Facility = SyslogFacility.Local4,
                    Severity = SyslogSeverity.Notice,
                    Host = "bench-host",
                    MessageId = "BENCH",
                    Text = i.ToString()
                };

                server.SubmitMessage(message);
                if (timeout > 0) System.Threading.Thread.Sleep(timeout);
            }

            System.Threading.Thread.Sleep(5000);

            SyslogMessage end_msg = new SyslogMessage()
            {
                Timestamp = DateTime.Now,
                Facility = SyslogFacility.Local4,
                Severity = SyslogSeverity.Notice,
                Host = "bench-host",
                MessageId = "END"
            };
            server.SubmitMessage(end_msg);

        }
    }
}
