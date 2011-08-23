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
using System.Threading;
using Murphy.Irc;

namespace Murphy {
	public class Network : IrcClient, IDisposable {

		#region " Constructor/Destructor/Dispose "
		public Network() {}

		~Network() {
			Dispose();
		}

		public void Dispose() {
			Disconnect();
		}
		#endregion

		#region " Connect/Disconnect "
		Thread listenThread;
		public void Connect() {
			try {
				base.SendDelay = sendDelay;
				base.ActiveChannelSyncing = true;
				base.Connect(servers.ToArray(), port);
				if (usePassword)
					base.Login(nickname, realname, 0, username, password);
				else
					base.Login(nickname, realname, 0, username);

				foreach (string channel in channels)
					base.RfcJoin(channel);

				listenThread = new Thread(new ThreadStart(base.Listen));
				listenThread.Name = "ListenThreadFor" + name;
				listenThread.Start();
			} catch (Exception e) {
				Console.WriteLine("# " + e.Message);
			}
		}

		public new void Disconnect() {
			RfcQuit("TTFN - TaTa For Now!");
			base.Disconnect();
		}
		#endregion

		#region " Properties "
		string name;
		public string Name {
			get {
				return name;
			}
			set {
				this.name = value;
			}
		}

		string nickname;
		public new string Nickname {
			get {
				return base.Nickname;
			}
			set {
				if (IsConnected)
					RfcNick(value);
				nickname = value;
			}
		}

		string realname;
		public new string Realname {
			get {
				return realname;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("The real name cannot be changed while the Bot is connected.");
				this.realname = value;
			}
		}

		string username;
		public new string Username {
			get {
				return username;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("The username cannot be changed while the Bot is connected.");
				this.username = value;
			}
		}

		string password;
		public new string Password {
			get {
				return password;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("The password cannot be changed while the Bot is connected.");
				this.password = value;
			}
		}

		bool usePassword;
		public bool UsePassword {
			get {
				return usePassword;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("This property cannot be changed while the Bot is connected.");
				this.usePassword = value;
			}
		}

		List<string> servers = new List<string>();
		public List<string> Servers {
			get {
				return servers;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("The list of servers cannot be changed while the Bot is connected.");
				this.servers = value;
			}
		}

		List<string> channels = new List<string>();
		public List<string> Channels {
			get {
				return channels;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("The list of channels cannot be changed while the Bot is connected.");
				this.channels = value;
			}
		}

		int port;
		public new int Port {
			get {
				return port;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("The port cannot be changed while the Bot is connected.");
				this.port = value;
			}
		}

		int sendDelay;
		public new int SendDelay {
			get {
				return sendDelay;
			}
			set {
				if (IsConnected)
					throw new InvalidOperationWhileConnectedException("The send delay cannot be changed while the Bot is connected.");
				this.sendDelay = value;
			}
		}

		#endregion

	}
}
