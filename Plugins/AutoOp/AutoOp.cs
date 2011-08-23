/*
AutoOp Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
using System.Xml;
#endregion

namespace Murphy.Plugins {
	public class AutoOp : Plugin {

		#region " Constructor/Destructor "
		public AutoOp(Bot bot)
			: base(bot) {
			Bot.OnJoin += new JoinEventHandler(Bot_OnJoin);
			Bot.OnDeop += new DeopEventHandler(Bot_OnDeop);
			Bot.OnOp += new OpEventHandler(Bot_OnOp);

			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
		}
		#endregion


		#region " Methods "
		void Op(Network n, string mask, string nick, string channel) {
			foreach (XmlElement elem in Bot.Configuration["Plugins"]["AutoOp"].GetElementsByTagName("Op")) {
				if (IsMatch(elem.InnerText, mask))
					n.Op(channel, nick);
			}
		}

		void DeOp(Network n, string mask, string nick, string channel) {
			foreach (XmlElement elem in Bot.Configuration["Plugins"]["AutoOp"].GetElementsByTagName("DeOp")) {
				if (IsMatch(elem.InnerText, mask))
					n.Deop(channel, nick);
			}
		}
		#endregion


		#region " Event handles "
		void Bot_OnMessage(Network n, Irc.IrcEventArgs e) {
			if (IsMatch("^autoop \\?$", e.Data.Message)) {
				AnswerWithNotice(n, e, FormatBold("Use of AutoOp plugin:"));
				AnswerWithNotice(n, e, "No remote commands available. All configuration has to be done manually in the Configuration.xml.");
			}
		}


		void Bot_OnJoin(Network n, Irc.JoinEventArgs e) {
			Op(n, e.Data.From, e.Data.Nick, e.Data.Channel);
		}


		void Bot_OnOp(Network n, Irc.OpEventArgs e) {
			DeOp(n, GetFullUser(n, e.Whom), e.Whom, e.Data.Channel);
		}


		void Bot_OnDeop(Network n, Murphy.Irc.DeopEventArgs e) {
			Op(n, GetFullUser(n, e.Whom), e.Whom, e.Data.Channel);
		}
		#endregion
	}
}
