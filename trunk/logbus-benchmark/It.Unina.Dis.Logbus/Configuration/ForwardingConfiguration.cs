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

namespace It.Unina.Dis.Logbus.Configuration
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.dis.unina.it/logbus-ng/configuration")]
    [System.Xml.Serialization.XmlRootAttribute("forward-to", Namespace = "http://www.dis.unina.it/logbus-ng/configuration", IsNullable = false)]
    public partial class ForwardingConfiguration
    {

        private ForwarderDefinition[] forwarderField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("forwarder")]
        public ForwarderDefinition[] forwarder
        {
            get
            {
                return this.forwarderField;
            }
            set
            {
                this.forwarderField = value;
            }
        }
    }
}