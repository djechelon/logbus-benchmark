/*
 *                  Logbus-ng project
 *    ©2010 Logbus Reasearch Team - Some rights reserved
 *
 *  Created by:
 *      Vittorio Alfieri - vitty85@users.sourceforge.net
 *      Antonio Anzivino - djechelon@users.sourceforge.net
 *
 *  Based on the research project "Logbus" by
 *
 *  Dipartimento di Informatica e Sistemistica
 *  University of Naples "Federico II"
 *  via Claudio, 21
 *  80121 Naples, Italy
 *
 *  Software is distributed under Microsoft Reciprocal License
 *  Documentation under Creative Commons 3.0 BY-SA License
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.Net;

namespace It.Unina.Dis.Logbus
{
    /// <summary>
    /// A syslog (RFC 5424) message
    /// </summary>
    /// <remarks>Currently, it can be serialized only into RFC5424 standard</remarks>
    [Serializable]
    public class SyslogMessage
    {

        /// <summary>
        ///	Facility that generated the message 
        /// </summary>
        public SyslogFacility Facility { get; set; }

        /// <summary>
        ///	Severity level of the message 
        /// </summary>
        public SyslogSeverity Severity { get; set; }

        /// <summary>
        ///	Time when the message was generated, if available
        /// </summary>
        /// <remarks>This field should be always set at the UTC time</remarks>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Time offset between Timestamp field and effective time stamp
        /// </summary>
        public TimeSpan? TimeOffset { get; set; }

        /// <summary>
        /// Local time according to time zone info
        /// </summary>
        public DateTime? LocalTimestamp
        {
            get
            {
                if (Timestamp == null) return null;
                if (TimeOffset == null) return Timestamp;
                return Timestamp + TimeOffset;
            }
        }

        /// <summary>
        ///	Hostname that generated the message, if available 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        ///	Application that generated the message, if available 
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///	ID of process that generated the message, if available 
        /// </summary>
        public string ProcessID { get; set; }

        /// <summary>
        ///	Application-specific ID of the message, if available 
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        ///	Additional information generated by the source 
        /// </summary>
        public IDictionary<string, IDictionary<string, string>> Data { get; set; }

        /// <summary>
        ///	Human-readable text 
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Initializes a new instance of SyslogMessage
        /// </summary>
        public SyslogMessage()
        { }

        /// <summary>
        /// Initializes a new instance of SyslogMessage with common fields
        /// </summary>
        /// <param name="timestamp">Time message was generated</param>
        /// <param name="host">Host that generated the message</param>
        /// <param name="facility">Facility of the message</param>
        /// <param name="level">Severity of message</param>
        /// <param name="text">Text message</param>
        [Obsolete("You should use SyslogMessage(string host, SyslogFacility facility, SyslogSeverity level, string text)", false)]
        public SyslogMessage(DateTime? timestamp, string host, SyslogFacility facility, SyslogSeverity level, string text)
            : this()
        {
            Timestamp = timestamp;
            Host = host;
            Facility = facility;
            Severity = level;
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of SyslogMessage with common fields
        /// </summary>
        /// <param name="host">Host that generated the message</param>
        /// <param name="facility">Facility of the message</param>
        /// <param name="level">Severity of message</param>
        /// <param name="text">Text message</param>
        public SyslogMessage(string host, SyslogFacility facility, SyslogSeverity level, string text)
            : this()
        {
            Timestamp = DateTime.UtcNow;
            TimeOffset = DateTime.Now - Timestamp;
            Host = host;
            Facility = facility;
            Severity = level;
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of SyslogMessage and auto-completes some parameters
        /// </summary>
        /// <param name="facility">Facility of the message</param>
        /// <param name="severity">Severity of message</param>
        /// <param name="text">Text message</param>
        public SyslogMessage(SyslogFacility facility, SyslogSeverity severity, string text)
            : this()
        {
            Facility = facility;
            Severity = severity;
            Text = text;
            Timestamp = DateTime.UtcNow;
            TimeOffset = DateTime.Now - Timestamp;
            Host = Dns.GetHostName();
            ProcessID = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
            ApplicationName = Process.GetCurrentProcess().ProcessName;
        }

        /// <summary>
        /// Adjustes timestamp for real-time applications detecting possible bogus timestamps
        /// </summary>
        /// <remarks>Apply this method only if you are sure the message is received real-time.
        /// Old log messages must not be adjusted. Time difference for bogus timestamp is set to 30 days</remarks>
        public void AdjustTimestamp()
        {
            if (Timestamp == null || Math.Abs((DateTime.UtcNow - Timestamp.Value).TotalDays) > 30)
            {
                Timestamp = DateTime.UtcNow;
                TimeOffset = DateTime.Now - DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Retrieves advanced Syslog attributes, if available
        /// </summary>
        /// <returns></returns>
        public SyslogAttributes GetAdvancedAttributes()
        {
            SyslogAttributes ret = new SyslogAttributes();

            if (Data != null)
            {
                const string KEY = "CallerData@" + Loggers.SimpleLogImpl.ENTERPRISE_ID;
                if (Data.ContainsKey(KEY))
                {
                    IDictionary<string, string> callerData = Data[KEY];
                    if (callerData.ContainsKey("ClassName")) ret.ClassName = callerData["ClassName"];
                    if (callerData.ContainsKey("MethodName")) ret.MethodName = callerData["MethodName"];
                    if (callerData.ContainsKey("ModuleName")) ret.ModuleName = callerData["ModuleName"];
                    if (callerData.ContainsKey("LogName")) ret.LogName = callerData["LogName"];
                }

                if (Data.ContainsKey("timeQuality"))
                {
                    IDictionary<string, string> timequality = Data["timeQuality"];
                    if (timequality.ContainsKey("tzKnown") && timequality["tzKnown"] == "1") ret.TimeZoneKnown = true;
                    if (timequality.ContainsKey("isSynced") && timequality["isSynced"] == "1") ret.TimeSynchronized = true;
                    if (ret.TimeSynchronized && timequality.ContainsKey("syncAccuracy")) long.TryParse(timequality["syncAccuracy"], NumberStyles.Integer, CultureInfo.InvariantCulture, out ret.TimeSyncAccuracy);
                }

                if (Data.ContainsKey("origin"))
                {
                    IDictionary<string, string> origin = Data["origin"];
                    if (origin.ContainsKey("ip")) IPAddress.TryParse(origin["ip"], out ret.OriginIpAddress);
                    if (origin.ContainsKey("enterpriseId")) ret.EnterpriseId = origin["enterpriseId"];
                    if (origin.ContainsKey("software")) ret.OriginatorSoftware = origin["software"];
                    if (origin.ContainsKey("swVersion")) ret.OriginatorSoftwareVersion = origin["swVersion"];
                }
            }

            return ret;
        }

        #region Conversion

        private int PriVal
        {
            get
            {
                return (int)Facility * 8 + (int)Severity;
            }
        }

        /// <summary>
        ///	Converts the object into RFC5424 UTF-8 binary representation 
        /// </summary>
        /// <returns>
        /// An array of <see cref="System.Byte"/> containing the UTF-8 representation
        /// </returns>
        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(ToRfc5424String());
        }

        /// <summary>
        /// Converts the object into RFC3164 string representation 
        /// </summary>
        /// <returns></returns>
        [Obsolete("You should avoid to use RFC 3164 as it's ambiguous and outdated")]
        public string ToRfc3164String()
        {
            StringBuilder ret = new StringBuilder();

            const string SPACE = @" ";

            //Encode Prival
            ret.AppendFormat("<{0}>", PriVal.ToString(CultureInfo.InvariantCulture));

            //Encode datetime
            //If unknown use local time
            ret.Append((Timestamp.HasValue) ? Timestamp.Value.ToString("MMM  dd HH:mm:ss", CultureInfo.InvariantCulture) : DateTime.Now.ToString("MMM  dd HH:mm:ss", CultureInfo.InvariantCulture));
            ret.Append(SPACE);

            //Encode hostname
            ret.Append(Host);
            ret.Append(SPACE);

            //Start BODY with TAG/AppName
            ret.Append(ApplicationName);
            if (ProcessID != null) ret.AppendFormat("[{0}]:", ProcessID);
            ret.Append(SPACE);

            ret.Append(Text);

            return ret.ToString();
        }

        /// <summary>
        ///	Converts the object into RFC5424 UTF-8 string representation 
        /// </summary>
        /// <returns>
        /// </returns>
        public string ToRfc5424String()
        {
            StringBuilder ret = new StringBuilder();

            const string NILVALUE = @"-";
            const string SPACE = @" ";

            //Prival+version
            ret.AppendFormat("<{0}>1", PriVal.ToString(CultureInfo.InvariantCulture));
            ret.Append(SPACE);

            //Timestamp
            const string TIMESTAMP_FORMAT = @"yyyy-MM-dd\THH:mm:ss";
            ret.Append((Timestamp == null) ? NILVALUE : Timestamp.Value.ToString(TIMESTAMP_FORMAT, CultureInfo.InvariantCulture));
            if (TimeOffset == null) ret.Append('Z');
            else
            {
                const string FORMAT = "{0}{1:D2}:{2:D2}";
                char sign = (TimeOffset.Value.Hours > 0) ? '+' : '-';
                ret.Append(string.Format(FORMAT, sign, Math.Abs(TimeOffset.Value.Hours), TimeOffset.Value.Minutes));
            }
            ret.Append(SPACE);

            //Hostname
            ret.Append((Host == null) ? NILVALUE : Format5424(Host, 255));
            ret.Append(SPACE);

            //AppName
            ret.Append((ApplicationName == null) ? NILVALUE : Format5424(ApplicationName, 48));
            ret.Append(SPACE);

            //procName
            ret.Append((ProcessID == null) ? NILVALUE : Format5424(ProcessID, 128));
            ret.Append(SPACE);

            //msgID
            ret.Append((MessageId == null) ? NILVALUE : Format5424(MessageId, 32));
            ret.Append(SPACE);

            //Structured Data			
            if (Data == null)
                ret.Append(NILVALUE);
            else
            {
                foreach (KeyValuePair<string, IDictionary<string, string>> kvp in Data)
                {
                    ret.Append(ToStringData(kvp.Key, kvp.Value));
                }
            }

            //Text
            if (Text != null)
            {
                byte[] bom = Encoding.UTF8.GetPreamble();
                ret.Append(SPACE);
                ret.Append(Encoding.UTF8.GetString(bom));
                ret.Append(Text);
            }


            return ret.ToString();
        }

        private static string Format5424(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input)) throw new ArgumentNullException("input");
            StringBuilder output = new StringBuilder();
            int len = Math.Min(maxLength, input.Length);
            char[] buffer = input.ToCharArray();

            for (int i = 0; i < len; i++)
                if (buffer[i] >= 32 && buffer[i] <= 126) output.Append(buffer[i]);
                else len++;

            return output.ToString();
        }

        /// <summary>
        ///	Converts the object into RFC5424 UTF-8 string representation 
        /// </summary>
        public override string ToString()
        {
            return ToRfc5424String();
        }

        private static string ToStringData(string key, IDictionary<string, string> data)
        {
            StringBuilder ret = new StringBuilder();

            ret.Append('[');
            ret.Append(key);

            List<string> elements = new List<string>();
            foreach (KeyValuePair<string, string> kvp in data)
            {
                elements.Add(string.Format(@"{0}=""{1}""", kvp.Key, Escape(kvp.Value, new char[] { '"', '\\', ']' })));
            }

            if (elements.Count > 0)
            {
                ret.Append(' ');
                ret.Append(string.Join(" ", elements.ToArray()));
            }

            ret.Append(']');
            return ret.ToString();
        }

        private static string Escape(string input, char[] specialChars)
        {
            StringBuilder ret = new StringBuilder();
            foreach (char c in input)
            {
                foreach (char comp in specialChars)
                {
                    if (c == comp)
                    {
                        ret.Append('\\');
                        break;
                    }
                }
                ret.Append(c);
            }

            return ret.ToString();
        }

        /// <summary>
        /// Converts a Windows EventLog entry to Syslog
        /// </summary>
        /// <param name="eventLogEntry">Log entry to convert</param>
        /// <returns>Correspondant Syslog message</returns>
        public static explicit operator SyslogMessage(EventLogEntry eventLogEntry)
        {
            SyslogMessage message = new SyslogMessage
                                        {
                                            Timestamp = eventLogEntry.TimeGenerated,
                                            Host = eventLogEntry.MachineName,
                                            Text = eventLogEntry.Message
                                        };


            //No "official" matching between Windows and Syslog severities exist.
            //We are choosing by our discretion, but we plan to align to what other
            //developers do, if we find more information about this
            switch (eventLogEntry.EntryType)
            {
                case EventLogEntryType.Error:
                    {
                        message.Severity = SyslogSeverity.Alert;
                        break;
                    }
                case EventLogEntryType.FailureAudit:
                    {
                        message.Severity = SyslogSeverity.Error;
                        break;
                    }
                case EventLogEntryType.Information:
                    {
                        message.Severity = SyslogSeverity.Info;
                        break;
                    }
                case EventLogEntryType.SuccessAudit:
                    {
                        message.Severity = SyslogSeverity.Notice;
                        break;
                    }
                case EventLogEntryType.Warning:
                    {
                        message.Severity = SyslogSeverity.Warning;
                        break;
                    }
            }

            //Windows Event Log messages do not distinguish between facilities
            //but Windows uses more than one Event Log, actually distinguishing facilities.
            //One could assign the facility value depending on the log that originated the message
            //http://code.google.com/p/eventlog-to-syslog/source/browse/trunk/4.0/syslog.h
            //chooses facility 3 (System daemons)
            message.Facility = SyslogFacility.System;
            message.ApplicationName = eventLogEntry.Source;
            message.MessageId = eventLogEntry.InstanceId.ToString();

            return message;
        }

        #endregion

        #region Parsing
        /// <summary>
        /// Parses Syslog messages according to RFC3164
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private static SyslogMessage Parse3164(string payload)
        {
            SyslogMessage ret = new SyslogMessage();
            try
            {
                int pointer = 1;
                String newPayload = payload.Substring(pointer);

                //Calculate prival = Facility*8 + Severity...
                String prival = newPayload.Split('>')[0];
                Int32 severity = 0;
                ret.Facility = (SyslogFacility)Math.DivRem(Int32.Parse(prival), 8, out severity);
                ret.Severity = (SyslogSeverity)severity;
                pointer += prival.Length + 1;

                //Calculate Timestamp...
                newPayload = payload.Substring(pointer);
                Int32 month = GetMonthByName(newPayload.Substring(0, 3));
                pointer += 4;
                newPayload = payload.Substring(pointer);

                Int32 day = 0;
                day = Int32.Parse(newPayload[0] == ' ' ? newPayload.Substring(1, 1) : newPayload.Substring(0, 2));
                pointer += 3;
                newPayload = payload.Substring(pointer);

                String timestamp = newPayload.Split(' ')[0];
                Int32 hour = Int32.Parse(timestamp.Split(':')[0]);
                Int32 minute = Int32.Parse(timestamp.Split(':')[1]);
                Int32 sec = Int32.Parse(timestamp.Split(':')[2]);
                ret.Timestamp = new DateTime(DateTime.Today.Year, month, day, hour, minute, sec, 0);
                pointer += timestamp.Length + 1;

                //Calculate HostIP...
                newPayload = payload.Substring(pointer);
                ret.Host = newPayload.Split(' ')[0];
                pointer += ret.Host.Length + 1;

                //Calculate AppName...
                newPayload = payload.Substring(pointer);
                String temp = newPayload.Split(' ')[0];
                if (temp.Contains("["))
                {
                    ret.ApplicationName = temp.Split('[')[0];
                    ret.ProcessID = temp.Split('[')[1].Split(']')[0];
                }
                else
                {
                    ret.ApplicationName = temp;
                    ret.ProcessID = null;
                }
                pointer += temp.Length + 1;

                //Calculate MessageID...
                ret.MessageId = null;

                //Calculate StructuredData...
                ret.Data = null;

                //Calculate Msg if present...
                if (pointer >= payload.Length)
                    ret.Text = null;
                else
                {
                    newPayload = payload.Substring(pointer);
                    if (newPayload.EndsWith("\r\n")) newPayload = newPayload.Substring(0, newPayload.Length - 2);
                    else if (newPayload.EndsWith("\n")) newPayload = newPayload.Substring(0, newPayload.Length - 1);
                    ret.Text = newPayload;
                }
                //Return the SyslogMessage...
                return ret;
            }
            catch (FormatException ex)
            {
                ex.Data.Add("Payload", payload);
                throw;
            }
            catch (Exception e)
            {
                FormatException ex = new FormatException("Message not in Syslog format", e);
                ex.Data.Add("Payload", payload);
                throw ex;
            }
        }

        /// <summary>
        /// Parses Syslog messages according to RFC5424
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private static SyslogMessage Parse5424(string payload)
        {
            SyslogMessage ret = new SyslogMessage();
            try
            {
                int pointer = 1;
                String newPayload = payload.Substring(pointer);

                //Calculate prival = Facility*8 + Severity...
                String prival = newPayload.Split('>')[0];
                Int32 severity = 0;
                ret.Facility = (SyslogFacility)Math.DivRem(Int32.Parse(prival), 8, out severity);
                ret.Severity = (SyslogSeverity)severity;
                pointer += prival.Length + 1;

                //Calculate Version...
                newPayload = payload.Substring(pointer);
                string version = newPayload.Substring(0, 1);
                if (version != "1")
                {
                    NotSupportedException ex = new NotSupportedException("Only Syslog version 1 is supported");
                    ex.Data["version"] = version;
                    throw ex;
                }
                pointer += 2;

                //Calculate Timestamp...
                newPayload = payload.Substring(pointer);
                String timestamp = newPayload.Split(' ')[0];

                if (timestamp != "-")
                {
                    String[] elem = timestamp.Split('T');

                    Int32 year = Int32.Parse(elem[0].Split('-')[0]);
                    Int32 month = Int32.Parse(elem[0].Split('-')[1]);
                    Int32 day = Int32.Parse(elem[0].Split('-')[2]);

                    String[] elem2;
                    Int32 fusoH = 0;
                    Int32 fusoM = 0;
                    if (elem[1].Contains("-"))
                    {
                        elem2 = elem[1].Split('-');
                        fusoH = Int32.Parse(elem2[1].Split(':')[0]) * -1;
                        fusoM = Int32.Parse(elem2[1].Split(':')[1]) * -1;
                    }
                    else if (elem[1].Contains("+"))
                    {
                        elem2 = elem[1].Split('+');
                        fusoH = Int32.Parse(elem2[1].Split(':')[0]);
                        fusoM = Int32.Parse(elem2[1].Split(':')[1]);
                    }
                    else
                    {
                        elem2 = elem[1].Split('Z');
                    }
                    Int32 hour = Int32.Parse(elem2[0].Split(':')[0]);
                    Int32 minute = Int32.Parse(elem2[0].Split(':')[1]);
                    Int32 sec = Int32.Parse(elem2[0].Split(':')[2].Split('.')[0]);
                    string msecStr = elem2[0].Split(':')[2];
                    Int32 msec = 0;
                    if (msecStr.IndexOf('.') > -1)
                        msec = Int32.Parse(msecStr.Split('.')[1].Substring(0, 3));

                    //UTC or reference time
                    ret.Timestamp = new DateTime(year, month, day, hour, minute, sec, msec);

                    //Time zone offset
                    if (fusoH == 0 && fusoM == 0) ret.TimeOffset = null;
                    else ret.TimeOffset = new TimeSpan(fusoH, fusoM, 0);
                }
                else
                {
                    ret.Timestamp = null;
                    ret.TimeOffset = null;
                }
                pointer += timestamp.Length + 1;

                //Calculate HostIP...
                newPayload = payload.Substring(pointer);
                ret.Host = (newPayload.Split(' ')[0] == "-") ? null : newPayload.Split(' ')[0];
                pointer += (ret.Host == null) ? 2 : ret.Host.Length + 1;

                //Calculate AppName...
                newPayload = payload.Substring(pointer);
                ret.ApplicationName = (newPayload.Split(' ')[0] == "-") ? null : newPayload.Split(' ')[0];
                pointer += (ret.ApplicationName == null) ? 2 : ret.ApplicationName.Length + 1;

                //Calculate ProcID...
                newPayload = payload.Substring(pointer);
                ret.ProcessID = (newPayload.Split(' ')[0] == "-") ? null : newPayload.Split(' ')[0];
                pointer += (ret.ProcessID == null) ? 2 : ret.ProcessID.Length + 1;

                //Calculate MessageID...
                newPayload = payload.Substring(pointer);
                ret.MessageId = (newPayload.Split(' ')[0] == "-") ? null : newPayload.Split(' ')[0];
                pointer += (ret.MessageId == null) ? 2 : ret.MessageId.Length + 1;

                //Calculate StructuredData...
                newPayload = payload.Substring(pointer);
                String structuredData = "[";
                if (newPayload.StartsWith("-"))
                {
                    ret.Data = null;
                }
                else
                {
                    for (int j = 1; j < newPayload.Length - 1; j++)
                    {
                        structuredData += newPayload[j];
                        if (newPayload[j - 1] == '"' && newPayload[j] == ']' && newPayload[j + 1] == ' ')
                            break;
                    }
                    if (!structuredData.EndsWith("]"))
                        structuredData += ']';
                    ret.Data = new Dictionary<string, IDictionary<string, string>>();
                    structuredData = structuredData.Replace("\\[", "@*°");
                    structuredData = structuredData.Replace("\\]", "çà§");
                    String[] elementi = structuredData.Split('[', ']');
                    for (int i = 0; i < elementi.Length; i++)
                    {
                        if (elementi[i].Length == 0)
                            continue;
                        String[] subElem = elementi[i].Split(' ');
                        String key = subElem[0];
                        Dictionary<string, string> values = new Dictionary<string, string>();
                        for (int k = 1; k < subElem.Length; k++)
                        {
                            String[] subSubElem = subElem[k].Split('=');
                            subSubElem[0] = subSubElem[0].Replace("@*°", "\\[");
                            subSubElem[0] = subSubElem[0].Replace("çà§", "\\]");
                            subSubElem[1] = subSubElem[1].Replace("@*°", "\\[");
                            subSubElem[1] = subSubElem[1].Replace("çà§", "\\]");
                            values.Add(subSubElem[0], subSubElem[1].Substring(1, subSubElem[1].Length - 2));
                        }
                        ret.Data.Add(key, values);
                    }
                }
                pointer += structuredData.Length + 1;

                //Calculate Msg if present...
                if (pointer >= payload.Length)
                    ret.Text = null;
                else
                {
                    newPayload = payload.Substring(pointer);
                    if (newPayload.EndsWith("\r\n")) newPayload = newPayload.Substring(0, newPayload.Length - 2);
                    else if (newPayload.EndsWith("\n")) newPayload = newPayload.Substring(0, newPayload.Length - 1);
                    byte[] BOM = { 0xef, 0xbb, 0xbf }, utf8String = Encoding.UTF8.GetBytes(newPayload);

                    if (BOM[0] == utf8String[0] && BOM[1] == utf8String[1] && BOM[2] == utf8String[2])
                        ret.Text = Encoding.UTF8.GetString(utf8String, 3, utf8String.Length - 3); //Cut BOM
                    else
                        ret.Text = newPayload;
                }
                //Return the SyslogMessage...
                return ret;
            }
            catch (FormatException ex)
            {
                ex.Data.Add("Payload", payload);
                throw;
            }
            catch (Exception e)
            {
                FormatException ex = new FormatException("Message not in Syslog format", e);
                ex.Data.Add("Payload", payload);
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">Argument is null</exception>
        /// <exception cref="FormatException">Message is not in Syslog format</exception>
        public static SyslogMessage Parse(string payload)
        {
            if (string.IsNullOrEmpty(payload)) throw new ArgumentNullException("payload");

            if (payload[0] != '<')
                //RFC5424 messages ALL start with LT, so this is either a No-PRI RFC3164 message or completely invalid message
                return Parse3164(payload);
            //Message begins with LT, so there must be GT
            else if (char.IsDigit(payload[payload.IndexOf('>') + 1]))
                //After GT there is a number. It should be the Version number of RFC5424
                return Parse5424(payload);
            else if (char.IsLetter(payload[payload.IndexOf('>') + 1]))
            {
                //It should be the name of a month. Trying with 3164
                return Parse3164(payload);
            }
            else
                //Definitely bad message
                throw new FormatException("The message is not formatted into any supported Syslog format");
        }

        /// <summary>
        /// Parses a Syslog message from binary payload
        /// </summary>
        /// <param name="payload">Binary payload to parse</param>
        /// <returns>A new instance of SyslogMessage</returns>
        /// <exception cref="System.ArgumentNullException">Argument is null</exception>
        /// <exception cref="FormatException">Message is not in Syslog format</exception>
        public static SyslogMessage Parse(byte[] payload)
        {
            return Parse(System.Text.Encoding.UTF8.GetString(payload));
        }

        private static Int32 GetMonthByName(String month)
        {
            Int32 mon = 1;
            switch (month)
            {
                case "Jan":
                    mon = 1;
                    break;
                case "Feb":
                    mon = 2;
                    break;
                case "Mar":
                    mon = 3;
                    break;
                case "Apr":
                    mon = 4;
                    break;
                case "May":
                    mon = 5;
                    break;
                case "Jun":
                    mon = 6;
                    break;
                case "Jul":
                    mon = 7;
                    break;
                case "Aug":
                    mon = 8;
                    break;
                case "Sep":
                    mon = 9;
                    break;
                case "Oct":
                    mon = 10;
                    break;
                case "Nov":
                    mon = 11;
                    break;
                case "Dec":
                    mon = 12;
                    break;
            }

            return mon;
        }

        #endregion
    }
}
