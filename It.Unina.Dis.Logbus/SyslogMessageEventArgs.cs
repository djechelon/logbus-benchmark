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

namespace It.Unina.Dis.Logbus
{
    /// <summary>
    /// Supports events related to Syslog messages
    /// </summary>
    public class SyslogMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Syslog message
        /// </summary>
        public SyslogMessage Message { get; set; }

        /// <remarks/>
        public SyslogMessageEventArgs()
        {
        }

        /// <remarks/>
        public SyslogMessageEventArgs(SyslogMessage msg)
        {
            Message = msg;
        }
    }
}