/*
Murphy: The #bitcoin-police IRC Bot
Copyright (C) 2011 #bitcoin-police - [Bitcrafted by TIGGR 2011] 

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using Engine;

namespace Murphy {
	public class Bot : IDisposable
    {

        #region Member Variables
        private static List<User> loggedin = new List<User>();
        private static List<String> admins = new List<String>();

        public string BOT_CONTROL_SEQ = "#";
        #endregion

        #region " Constructor/Destructor/Dispose "
        public Bot() {
			#region " Header "
			Console.WriteLine("Murphy: The #bitcoin-police IRC Bot  - v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + " - [http://www.bitcoinpolice.org]");
			Console.WriteLine("(c) 2011 Murphy, The Bitcoin-Police IRC Bot - Bitcrafted By TIGGR");
			Console.WriteLine("===============================================================================");
			Console.WriteLine("Murphy: The #bitcoin-police IRC Bot comes with absolutely no warranty.");
			Console.WriteLine("This is free software, and you are welcome to redistribute it under certain");
			Console.WriteLine("conditions. See the enclosed copy of the General Public License for details.");
			Console.WriteLine("===============================================================================");
			#endregion

			#region " Load Configuration "
			configuration = new XmlDocument();
			configuration.Load("Configuration.xml");

            //Load the Admins from config file
            foreach (XmlElement elem in configuration["Murphy"]["Config"].GetElementsByTagName("admin"))
            {
                admins.Add(elem.InnerText);
            }

            BOT_CONTROL_SEQ = configuration["Murphy"]["Config"].Attributes["ControlSeq"].Value;
            

			foreach (XmlElement e in configuration.GetElementsByTagName("Network")) {
				Network n = new Network();
				networks.Add(n);
				n.Name = e.Attributes["Name"].Value;
				n.Nickname = e.Attributes["Nickname"].Value;
				n.Realname = e.Attributes["Realname"].Value;
				n.Username = e.Attributes["Username"].Value;
				if (e.HasAttribute("Password")) {
					n.UsePassword = true;
					n.Password = e.Attributes["Password"].Value;
				}
				else
					n.UsePassword = false;
				n.Port = int.Parse(e.Attributes["Port"].Value);
				n.SendDelay = int.Parse(e.Attributes["SendDelay"].Value);

				foreach (XmlElement f in e.GetElementsByTagName("Server"))
					n.Servers.Add(f.Attributes["Address"].Value);

				foreach (XmlElement f in e.GetElementsByTagName("Channel"))
					n.Channels.Add(f.Attributes["Name"].Value);


				n.OnBan += new Irc.BanEventHandler(OnBanHandler);
				n.OnChannelAction += new Irc.ActionEventHandler(OnChannelActionHandler);
				n.OnChannelActiveSynced += new Irc.IrcEventHandler(OnChannelActiveSyncedHandler);
				n.OnChannelMessage += new Irc.IrcEventHandler(OnChannelMessageHandler);
				n.OnChannelModeChange += new Irc.IrcEventHandler(OnChannelModeChangeHandler);
				n.OnChannelNotice += new Irc.IrcEventHandler(OnChannelNoticeHandler);
				n.OnChannelPassiveSynced += new Irc.IrcEventHandler(OnChannelPassiveSyncedHandler);
				n.OnConnected += new EventHandler(OnConnectedHandler);
				n.OnConnecting += new EventHandler(OnConnectingHandler);
				n.OnConnectionError += new EventHandler(OnConnectionErrorHandler);
				n.OnCtcpReply += new Irc.IrcEventHandler(OnCtcpReplyHandler);
				n.OnCtcpRequest += new Irc.IrcEventHandler(OnCtcpRequestHandler);
				n.OnDehalfop += new Irc.DehalfopEventHandler(OnDehalfopHandler);
				n.OnDeop += new Irc.DeopEventHandler(OnDeopHandler);
				n.OnDevoice += new Irc.DevoiceEventHandler(OnDevoiceHandler);
				n.OnDisconnected += new EventHandler(OnDisconnectedHandler);
				n.OnDisconnecting += new EventHandler(OnDisconnectingHandler);
				n.OnError += new Irc.ErrorEventHandler(OnErrorHandler);
				n.OnErrorMessage += new Irc.IrcEventHandler(OnErrorMessageHandler);
				n.OnHalfop += new Irc.HalfopEventHandler(OnHalfopHandler);
				n.OnInvite += new Irc.InviteEventHandler(OnInviteHandler);
				n.OnJoin += new Irc.JoinEventHandler(OnJoinHandler);
				n.OnKick += new Irc.KickEventHandler(OnKickHandler);
				n.OnModeChange += new Irc.IrcEventHandler(OnModeChangeHandler);
				n.OnMotd += new Irc.MotdEventHandler(OnMotdHandler);
				n.OnNames += new Irc.NamesEventHandler(OnNamesHandler);
				n.OnNickChange += new Irc.NickChangeEventHandler(OnNickChangeHandler);
				n.OnOp += new Irc.OpEventHandler(OnOpHandler);
				n.OnPart += new Irc.PartEventHandler(OnPartHandler);
				n.OnPing += new Irc.PingEventHandler(OnPingHandler);
				n.OnQueryAction += new Irc.ActionEventHandler(OnQueryActionHandler);
				n.OnQueryMessage += new Irc.IrcEventHandler(OnQueryMessageHandler);
				n.OnQueryNotice += new Irc.IrcEventHandler(OnQueryNoticeHandler);
				n.OnQuit += new Irc.QuitEventHandler(OnQuitHandler);
				n.OnRawMessage += new Irc.IrcEventHandler(OnRawMessageHandler);
				n.OnReadLine += new Irc.ReadLineEventHandler(OnReadLineHandler);
				n.OnRegistered += new EventHandler(OnRegisteredHandler);
				n.OnTopic += new Irc.TopicEventHandler(OnTopicHandler);
				n.OnTopicChange += new Irc.TopicChangeEventHandler(OnTopicChangeHandler);
				n.OnUnban += new Irc.UnbanEventHandler(OnUnbanHandler);
				n.OnUserModeChange += new Irc.IrcEventHandler(OnUserModeChangeHandler);
				n.OnVoice += new Irc.VoiceEventHandler(OnVoiceHandler);
				n.OnWho += new Irc.WhoEventHandler(OnWhoHandler);
				n.OnWriteLine += new Irc.WriteLineEventHandler(OnWriteLineHandler);
			}
			#endregion

			#region " Load Plugins "
			object[] o ={ this };
			foreach (System.IO.FileInfo f in new System.IO.DirectoryInfo("Plugins").GetFiles())
				if (f.Extension == ".dll") {
					Console.WriteLine("Loading Plugins from Assembly '" + f.Name + "' ...");
					Assembly a = System.Reflection.Assembly.LoadFile(f.FullName);
					foreach (Type t in a.GetTypes())
						if (t.BaseType == typeof(Plugin)) {
							Plugin p = (Plugin)Activator.CreateInstance(t, o);
							plugins.Add(p);
						}
				}
			Console.WriteLine("===============================================================================");
			#endregion
		}

		~Bot() {
			Dispose();
		}

		public void Dispose() {
			DisconnectAll();
		}
		#endregion

		#region " Connect/Disconnect "
		public void ConnectAll() {
			foreach (Network n in Networks)
				n.Connect();
		}

		public void DisconnectAll() {
			foreach (Network n in Networks)
				n.Disconnect();
		}
		#endregion

		#region " Methods "



        public void LoginUser(User user)
        {
            if (!loggedin.Contains(user))
            {
                loggedin.Add(user);
            }
        }

        public void LogoutUser(User user)
        {
            if (loggedin.Contains(user))
            {
                loggedin.Remove(user);
            }
        }

        public bool isLoggedIn(string nick)
        {
            foreach(User user in loggedin){
                if(user.nick.ToLower()==nick.ToLower()){
                    return true;
                }
            }
            return false;
        }

		public Network GetNetworkByName(string name) {
			foreach (Network n in networks)
				if (n.Name == name)
					return n;
			throw new NetworkNotFoundException();
		}

		public void SaveConfiguration() {
			configuration.Save("Configuration.xml");
		}
		#endregion

		#region " Properties "

        public static bool isAdmin(string nick)
        {

            foreach (string s in admins)
            {
                if (s == nick)
                    return true;
            }
            return false;
        }

        public static List<User> LoggedInUsers
        {
            get { return Bot.loggedin; }
        }

		XmlDocument configuration;
		public XmlElement Configuration {
			get {
				return configuration["Murphy"];
			}
		}

		List<Network> networks = new List<Network>();
		public List<Network> Networks {
			get {
				return networks;
			}
		}

		List<Plugin> plugins = new List<Plugin>();
		public List<Plugin> Plugins {
			get {
				return plugins;
			}
		}
		#endregion

		#region " Global Event Handles "
		void OnBanHandler(object sender, Irc.BanEventArgs e) {
			if (OnBan != null)
				OnBan((Network)sender, e);
		}

		void OnChannelActionHandler(object sender, Irc.ActionEventArgs e) {
			if (OnChannelAction != null)
				OnChannelAction((Network)sender, e);
		}

		void OnChannelActiveSyncedHandler(object sender, Irc.IrcEventArgs e) {
			if (OnChannelActiveSynced != null)
				OnChannelActiveSynced((Network)sender, e);
		}

		void OnChannelMessageHandler(object sender, Irc.IrcEventArgs e) {
			if (OnChannelMessage != null)
				OnChannelMessage((Network)sender, e);
		}

		void OnChannelModeChangeHandler(object sender, Irc.IrcEventArgs e) {
			if (OnChannelModeChange != null)
				OnChannelModeChange((Network)sender, e);
		}

		void OnChannelNoticeHandler(object sender, Irc.IrcEventArgs e) {
			if (OnChannelNotice != null)
				OnChannelNotice((Network)sender, e);
		}

		void OnChannelPassiveSyncedHandler(object sender, Irc.IrcEventArgs e) {
			if (OnChannelPassiveSynced != null)
				OnChannelPassiveSynced((Network)sender, e);
		}

		void OnConnectedHandler(object sender, EventArgs e) {
			if (OnConnected != null)
				OnConnected((Network)sender, e);
		}

		void OnConnectingHandler(object sender, EventArgs e) {
			if (OnConnecting != null)
				OnConnecting((Network)sender, e);
		}

		void OnConnectionErrorHandler(object sender, EventArgs e) {
			if (OnConnectionError != null)
				OnConnectionError((Network)sender, e);
		}

		void OnCtcpReplyHandler(object sender, Irc.IrcEventArgs e) {
			if (OnCtcpReply != null)
				OnCtcpReply((Network)sender, e);
		}

		void OnCtcpRequestHandler(object sender, Irc.IrcEventArgs e) {
			if (OnCtcpRequest != null)
				OnCtcpRequest((Network)sender, e);
		}

		void OnDehalfopHandler(object sender, Irc.DehalfopEventArgs e) {
			if (OnDehalfop != null)
				OnDehalfop((Network)sender, e);
		}

		void OnDeopHandler(object sender, Irc.DeopEventArgs e) {
			if (OnDeop != null)
				OnDeop((Network)sender, e);
		}

		void OnDevoiceHandler(object sender, Irc.DevoiceEventArgs e) {
			if (OnDevoice != null)
				OnDevoice((Network)sender, e);
		}

		void OnDisconnectedHandler(object sender, EventArgs e) {
			if (OnDisconnected != null)
				OnDisconnected((Network)sender, e);
		}

		void OnDisconnectingHandler(object sender, EventArgs e) {
			if (OnDisconnecting != null)
				OnDisconnecting((Network)sender, e);
		}

		void OnErrorHandler(object sender, Irc.ErrorEventArgs e) {
			if (OnError != null)
				OnError((Network)sender, e);
		}

		void OnErrorMessageHandler(object sender, Irc.IrcEventArgs e) {
			if (OnErrorMessage != null)
				OnErrorMessage((Network)sender, e);
		}

		void OnHalfopHandler(object sender, Irc.HalfopEventArgs e) {
			if (OnHalfop != null)
				OnHalfop((Network)sender, e);
		}

		void OnInviteHandler(object sender, Irc.InviteEventArgs e) {
			if (OnInvite != null)
				OnInvite((Network)sender, e);
		}

		void OnJoinHandler(object sender, Irc.JoinEventArgs e) {
			if (OnJoin != null)
				OnJoin((Network)sender, e);
		}

		void OnKickHandler(object sender, Irc.KickEventArgs e) {
			if (OnKick != null)
				OnKick((Network)sender, e);
		}

		void OnModeChangeHandler(object sender, Irc.IrcEventArgs e) {
			if (OnModeChange != null)
				OnModeChange((Network)sender, e);
		}

		void OnMotdHandler(object sender, Irc.MotdEventArgs e) {
			if (OnMotd != null)
				OnMotd((Network)sender, e);
		}

		void OnNamesHandler(object sender, Irc.NamesEventArgs e) {
			if (OnNames != null)
				OnNames((Network)sender, e);
		}

		void OnNickChangeHandler(object sender, Irc.NickChangeEventArgs e) {
			if (OnNickChange != null)
				OnNickChange((Network)sender, e);
		}

		void OnOpHandler(object sender, Irc.OpEventArgs e) {
			if (OnOp != null)
				OnOp((Network)sender, e);
		}

		void OnPartHandler(object sender, Irc.PartEventArgs e) {
			if (OnPart != null)
				OnPart((Network)sender, e);
		}

		void OnPingHandler(object sender, Irc.PingEventArgs e) {
			if (OnPing != null)
				OnPing((Network)sender, e);
		}

		void OnQueryActionHandler(object sender, Irc.ActionEventArgs e) {
			if (OnQueryAction != null)
				OnQueryAction((Network)sender, e);
		}

		void OnQueryMessageHandler(object sender, Irc.IrcEventArgs e) {
			if (OnQueryMessage != null)
				OnQueryMessage((Network)sender, e);
		}

		void OnQueryNoticeHandler(object sender, Irc.IrcEventArgs e) {
			if (OnQueryNotice != null)
				OnQueryNotice((Network)sender, e);
		}

		void OnQuitHandler(object sender, Irc.QuitEventArgs e) {
			if (OnQuit != null)
				OnQuit((Network)sender, e);
		}

		void OnRawMessageHandler(object sender, Irc.IrcEventArgs e) {
			if (OnRawMessage != null)
				OnRawMessage((Network)sender, e);
		}

		void OnReadLineHandler(object sender, Irc.ReadLineEventArgs e) {
			if (OnReadLine != null)
				OnReadLine((Network)sender, e);
		}

		void OnRegisteredHandler(object sender, EventArgs e) {
			if (OnRegistered != null)
				OnRegistered((Network)sender, e);
		}

		void OnTopicHandler(object sender, Irc.TopicEventArgs e) {
			if (OnTopic != null)
				OnTopic((Network)sender, e);
		}

		void OnTopicChangeHandler(object sender, Irc.TopicChangeEventArgs e) {
			if (OnTopicChange != null)
				OnTopicChange((Network)sender, e);
		}

		void OnUnbanHandler(object sender, Irc.UnbanEventArgs e) {
			if (OnUnban != null)
				OnUnban((Network)sender, e);
		}

		void OnUserModeChangeHandler(object sender, Irc.IrcEventArgs e) {
			if (OnUserModeChange != null)
				OnUserModeChange((Network)sender, e);
		}

		void OnVoiceHandler(object sender, Irc.VoiceEventArgs e) {
			if (OnVoice != null)
				OnVoice((Network)sender, e);
		}

		void OnWhoHandler(object sender, Irc.WhoEventArgs e) {
			if (OnWho != null)
				OnWho((Network)sender, e);
		}

		void OnWriteLineHandler(object sender, Irc.WriteLineEventArgs e) {
			if (OnWriteLine != null)
				OnWriteLine((Network)sender, e);
		}
		#endregion

		#region " Events "
		public event EventHandler OnRegistered;
		public event PingEventHandler OnPing;
		public event IrcEventHandler OnRawMessage;
		public event ErrorEventHandler OnError;
		public event IrcEventHandler OnErrorMessage;
		public event JoinEventHandler OnJoin;
		public event NamesEventHandler OnNames;
		public event PartEventHandler OnPart;
		public event QuitEventHandler OnQuit;
		public event KickEventHandler OnKick;
		public event InviteEventHandler OnInvite;
		public event BanEventHandler OnBan;
		public event UnbanEventHandler OnUnban;
		public event OpEventHandler OnOp;
		public event DeopEventHandler OnDeop;
		public event HalfopEventHandler OnHalfop;
		public event DehalfopEventHandler OnDehalfop;
		public event VoiceEventHandler OnVoice;
		public event DevoiceEventHandler OnDevoice;
		public event WhoEventHandler OnWho;
		public event MotdEventHandler OnMotd;
		public event TopicEventHandler OnTopic;
		public event TopicChangeEventHandler OnTopicChange;
		public event NickChangeEventHandler OnNickChange;
		public event IrcEventHandler OnModeChange;
		public event IrcEventHandler OnUserModeChange;
		public event IrcEventHandler OnChannelModeChange;
		public event IrcEventHandler OnChannelMessage;
		public event ActionEventHandler OnChannelAction;
		public event IrcEventHandler OnChannelNotice;
		public event IrcEventHandler OnChannelActiveSynced;
		public event IrcEventHandler OnChannelPassiveSynced;
		public event IrcEventHandler OnQueryMessage;
		public event ActionEventHandler OnQueryAction;
		public event IrcEventHandler OnQueryNotice;
		public event IrcEventHandler OnCtcpRequest;
		public event IrcEventHandler OnCtcpReply;
		public event ReadLineEventHandler OnReadLine;
		public event WriteLineEventHandler OnWriteLine;
		public event EventHandler OnConnecting;
		public event EventHandler OnConnected;
		public event EventHandler OnDisconnecting;
		public event DisconnectedEventHandler OnDisconnected;
		public event EventHandler OnConnectionError;
		#endregion

	}

	#region " Delegates "
	public delegate void IrcEventHandler(Network network, Irc.IrcEventArgs e);
	public delegate void ActionEventHandler(Network network, Irc.ActionEventArgs e);
	public delegate void ErrorEventHandler(Network network, Irc.ErrorEventArgs e);
	public delegate void PingEventHandler(Network network, Irc.PingEventArgs e);
	public delegate void KickEventHandler(Network network, Irc.KickEventArgs e);
	public delegate void JoinEventHandler(Network network, Irc.JoinEventArgs e);
	public delegate void NamesEventHandler(Network network, Irc.NamesEventArgs e);
	public delegate void PartEventHandler(Network network, Irc.PartEventArgs e);
	public delegate void InviteEventHandler(Network network, Irc.InviteEventArgs e);
	public delegate void OpEventHandler(Network network, Irc.OpEventArgs e);
	public delegate void DeopEventHandler(Network network, Irc.DeopEventArgs e);
	public delegate void HalfopEventHandler(Network network, Irc.HalfopEventArgs e);
	public delegate void DehalfopEventHandler(Network network, Irc.DehalfopEventArgs e);
	public delegate void VoiceEventHandler(Network network, Irc.VoiceEventArgs e);
	public delegate void DevoiceEventHandler(Network network, Irc.DevoiceEventArgs e);
	public delegate void BanEventHandler(Network network, Irc.BanEventArgs e);
	public delegate void UnbanEventHandler(Network network, Irc.UnbanEventArgs e);
	public delegate void TopicEventHandler(Network network, Irc.TopicEventArgs e);
	public delegate void TopicChangeEventHandler(Network network, Irc.TopicChangeEventArgs e);
	public delegate void NickChangeEventHandler(Network network, Irc.NickChangeEventArgs e);
	public delegate void QuitEventHandler(Network network, Irc.QuitEventArgs e);
	public delegate void WhoEventHandler(Network network, Irc.WhoEventArgs e);
	public delegate void MotdEventHandler(Network network, Irc.MotdEventArgs e);
	public delegate void ReadLineEventHandler(Network network, Irc.ReadLineEventArgs e);
	public delegate void WriteLineEventHandler(Network network, Irc.WriteLineEventArgs e);
	public delegate void DisconnectedEventHandler(Network network, EventArgs e);
	#endregion
}
