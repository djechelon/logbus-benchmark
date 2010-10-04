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

namespace It.Unina.Dis.Logbus.Wrappers
{
    /// <summary>
    /// Proxy for IChannelManagement
    /// </summary>
    public sealed class ChannelManagementTie
        : IChannelManagement
    {

        /// <summary>
        /// Initializes ChannelManagementTie with proxied object
        /// </summary>
        /// <param name="targetInstance">Object to be proxied</param>
        /// <exception cref="System.ArgumentNullException">targetInstance is null</exception>
        public ChannelManagementTie(IChannelManagement targetInstance)
        {
            if (targetInstance == null) throw new System.ArgumentNullException("targetInstance");
            target = targetInstance;
        }

        private IChannelManagement target;

        #region IChannelManagement Membri di

        /// <remarks/>
        public string[] ListChannels()
        {
            return target.ListChannels();
        }

        /// <remarks/>
        public void CreateChannel(It.Unina.Dis.Logbus.RemoteLogbus.ChannelCreationInformation description)
        {
            target.CreateChannel(description);
        }

        /// <remarks/>
        public It.Unina.Dis.Logbus.RemoteLogbus.ChannelInformation GetChannelInformation(string id)
        {
            return target.GetChannelInformation(id);
        }

        /// <remarks/>
        public void DeleteChannel(string id)
        {
            target.DeleteChannel(id);
        }

        #endregion
    }
}