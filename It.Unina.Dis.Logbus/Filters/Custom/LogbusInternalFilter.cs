﻿/*
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
using System.Diagnostics;
using System.Globalization;
using System.Net;
using It.Unina.Dis.Logbus.Design;

namespace It.Unina.Dis.Logbus.Filters.Custom
{
    /// <summary>
    /// Filters messages coming from the Logbus-ng server
    /// </summary>
    [CustomFilter("logbus-internal",
        Description = "Only messages internally generated by Logbus-ng core match this filter")]
    public class LogbusInternalFilter
        : ICustomFilter
    {
        private readonly string _logbusHost, _logbusPid;

        /// <summary>
        /// Initializes a new instance of LogbusInternalFilter
        /// </summary>
        public LogbusInternalFilter()
        {
            _logbusHost = Dns.GetHostName();
            _logbusPid = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
        }

        #region ICustomFilter Membri di

        IEnumerable<FilterParameter> ICustomFilter.Configuration
        {
            set { throw new NotSupportedException("LogbusInternalFilter doesn't support configuration"); }
        }

        #endregion

        #region IFilter Membri di

        bool IFilter.IsMatch(SyslogMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            return _logbusHost == message.Host && _logbusPid == message.ProcessID &&
                   message.Facility == SyslogFacility.Internally;
        }

        #endregion

        /// <summary>
        /// Converts this filter into the XML proxy representation
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static implicit operator FilterBase(LogbusInternalFilter filter)
        {
            return new CustomFilter {name = "logbus-internal"};
        }
    }
}