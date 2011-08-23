/*
Reconnect Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
Copyright (C) 2005 Hannes Sachsenhofer [http://www.sachsenhofer.com]

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

#region Using directives
using System;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace Murphy.Plugins {
	public class Reconnect : Plugin {

		#region " Constructor/Destructor "
		public Reconnect(Bot bot)
			: base(bot) {
			Bot.OnDisconnected += new DisconnectedEventHandler(Bot_OnDisconnected);
			Bot.OnKick += new KickEventHandler(Bot_OnKick);
		}
		#endregion

		#region " Methods "
		Network n;
		void DoReconnect() {
			Network network = n;
			while (!network.IsConnected) {
				Thread.Sleep(30000);
				Console.WriteLine("Trying to reconnect to " + network.Name + ".");
				network.Connect();
			}
		}
		#endregion

		#region " Event handles "
		void Bot_OnDisconnected(Network network, EventArgs e) {
			Console.WriteLine("Disconnected from " + network.Name + ".");
			n = network;
			new Thread(new ThreadStart(DoReconnect)).Start();
		}

		void Bot_OnKick(Network network, Murphy.Irc.KickEventArgs e) {
			if (e.Data.Nick == network.Nickname) {
				Console.WriteLine("Rejoining " + e.Channel + " on " + network.Name + ".");
				network.RfcJoin(e.Channel);
			}
		}
		#endregion

	}
}
