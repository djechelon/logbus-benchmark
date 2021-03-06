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

namespace It.Unina.Dis.Logbus.Configuration
{
    /// <remarks/>
    [XmlInclude(typeof (LoggerDefinition))]
    [XmlInclude(typeof (InboundChannelDefinition))]
    [XmlInclude(typeof (LogCollectorDefinitionBase))]
    [XmlInclude(typeof (LogbusCollectorDefinition))]
    [XmlInclude(typeof (ForwarderDefinition))]
    [GeneratedCode("xsd", "4.0.30319.1")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://www.dis.unina.it/logbus-ng/configuration/3.0")]
    public abstract class TypeAndParamBase
    {
        private KeyValuePair[] paramField;

        private string typeField;

        /// <remarks/>
        [XmlElement("param")]
        public KeyValuePair[] param
        {
            get { return paramField; }
            set { paramField = value; }
        }

        /// <remarks/>
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string type
        {
            get { return typeField; }
            set { typeField = value; }
        }
    }
}