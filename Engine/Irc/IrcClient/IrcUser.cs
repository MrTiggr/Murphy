/**
 * $Id: IrcUser.cs 152 2005-01-09 20:15:04Z meebey $
 * $URL: svn://svn.qnetp.net/smartirc/SmartIrc4net/tags/0.3.5/src/IrcClient/IrcUser.cs $
 * $Rev: 152 $
 * $Author: meebey $
 * $Date: 2005-01-09 21:15:04 +0100 (Sun, 09 Jan 2005) $
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2004 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
 * 
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System.Collections.Specialized;

namespace Murphy.Irc
{
    /// <summary>
    ///
    /// </summary>
    public class IrcUser
    {
        private IrcClient _IrcClient;
        private string    _Nick     = null;
        private string    _Ident    = null;
        private string    _Host     = null;
        private string    _Realname = null;
        private bool      _IsIrcOp  = false;
        private bool      _IsAway   = false;
        private string    _Server   = null;
        private int       _HopCount = -1;

        internal IrcUser(string nickname, IrcClient ircclient)
        {
            _IrcClient = ircclient;
            _Nick      = nickname;
        }

#if LOG4NET
        ~IrcUser()
        {
            Logger.ChannelSyncing.Debug("IrcUser ("+Nick+") destroyed");
        }
#endif

        public string Nick
        {
            get {
                return _Nick;
            }
            set {
                _Nick = value;
            }
        }

        public string Ident
        {
            get {
                return _Ident;
            }
            set {
                _Ident = value;
            }
        }

        public string Host
        {
            get {
                return _Host;
            }
            set {
                _Host = value;
            }
        }

        public string Realname
        {
            get {
                return _Realname;
            }
            set {
                _Realname = value;
            }
        }

        public bool IsIrcOp
        {
            get {
                return _IsIrcOp;
            }
            set {
                _IsIrcOp = value;
            }
        }

        public bool IsAway
        {
            get {
                return _IsAway;
            }
            set {
                _IsAway = value;
            }
        }

        public string Server
        {
            get {
                return _Server;
            }
            set {
                _Server = value;
            }
        }

        public int HopCount
        {
            get {
                return _HopCount;
            }
            set {
                _HopCount = value;
            }
        }

        public string[] JoinedChannels
        {
            get {
                Channel          channel;
                string[]         result;
                string[]         channels       = _IrcClient.GetChannels();
                StringCollection joinedchannels = new StringCollection();
                foreach (string channelname in channels) {
                    channel = _IrcClient.GetChannel(channelname);
                    if (channel.UnsafeUsers.ContainsKey(_Nick)) {
                        joinedchannels.Add(channelname);
                    }
                }

                result = new string[joinedchannels.Count];
                joinedchannels.CopyTo(result, 0);
                return result;
                //return joinedchannels;
            }
        }
    }
}
