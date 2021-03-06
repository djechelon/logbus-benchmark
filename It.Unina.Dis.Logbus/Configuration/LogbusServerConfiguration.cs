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
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Serialization;
using It.Unina.Dis.Logbus.Filters;

namespace It.Unina.Dis.Logbus.Configuration
{
    /// <remarks/>
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://www.dis.unina.it/logbus-ng/configuration/3.0")]
    [XmlRoot("logbus-server", Namespace = "http://www.dis.unina.it/logbus-ng/configuration/3.0", IsNullable = false)]
    public class LogbusServerConfiguration : XmlnsSupport
    {
        private FilterBase corefilterField;
        private CustomFiltersConfiguration customfiltersField;
        private ForwarderDefinition[] forwardtoField;
        private InboundChannelDefinition[] inchannelsField;
        private string outChannelFactoryTypeField;

        private OutputTransportsConfiguration outtransportsField;

        private PluginDefinition[] pluginsField;

        private WebServerConfiguration webserverField;

        /// <remarks/>
        [XmlArray("in-channels", IsNullable = true)]
        [XmlArrayItem("in-channel", IsNullable = false)]
        public InboundChannelDefinition[] inchannels
        {
            get { return inchannelsField; }
            set { inchannelsField = value; }
        }

        /// <remarks/>
        [XmlElement("custom-filters")]
        public CustomFiltersConfiguration customfilters
        {
            get { return customfiltersField; }
            set { customfiltersField = value; }
        }

        /// <remarks/>
        [XmlElement("out-transports")]
        public OutputTransportsConfiguration outtransports
        {
            get { return outtransportsField; }
            set { outtransportsField = value; }
        }

        /// <remarks/>
        [XmlElement("core-filter", IsNullable = true)]
        public FilterBase corefilter
        {
            get { return corefilterField; }
            set { corefilterField = value; }
        }

        /// <remarks/>
        [XmlArrayItem("plugin", IsNullable = false)]
        public PluginDefinition[] plugins
        {
            get { return pluginsField; }
            set { pluginsField = value; }
        }

        /// <remarks/>
        [XmlArray("forward-to")]
        [XmlArrayItem("forwarder", IsNullable = false)]
        public ForwarderDefinition[] forwardto
        {
            get { return forwardtoField; }
            set { forwardtoField = value; }
        }

        /// <remarks/>
        public WebServerConfiguration webserver
        {
            get { return webserverField; }
            set { webserverField = value; }
        }

        /// <remarks/>
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string outChannelFactoryType
        {
            get { return outChannelFactoryTypeField; }
            set { outChannelFactoryTypeField = value; }
        }
    }
}