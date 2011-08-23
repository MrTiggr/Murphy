/*
Seen Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
using System.IO;
using System.Xml.Serialization;
#endregion

namespace Murphy.Plugins {
	public class Seen : Plugin {

		#region " Constructor/Destructor "
		List<SeenInfo> l;
		public Seen(Bot bot)
			: base(bot) {
			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnJoin += new JoinEventHandler(Bot_OnJoin);
			Bot.OnPart += new PartEventHandler(Bot_OnPart);
			Bot.OnQuit += new QuitEventHandler(Bot_OnQuit);
			Bot.OnNickChange += new NickChangeEventHandler(Bot_OnNickChange);
			l=Load();
		}
		#endregion

		#region " Seen "
		SeenInfo FindName(string network, string name, List<SeenInfo> l) {
			foreach (SeenInfo i in l)
				if (i.Network == network && i.Names.Contains(name))
					return i;
			return null;
		}

		SeenInfo FindIdent(string network, string ident, List<SeenInfo> l) {
			foreach (SeenInfo i in l)
				if (i.Ident == ident && i.Network == network)
					return i;
			return null;
		}

		void NewSeen(string network, string nick, string ident, string text) {
			SeenInfo i = FindIdent(network, ident, l);
			if (i == null) {
				i = new SeenInfo();
				i.Date = DateTime.Now;
				i.Ident = ident;
				i.Names = new List<string>();
				i.Network = network;
				i.Text = text;
				l.Add(i);
			}
			else {
				i.Date = DateTime.Now;
				i.Text = text;
			}

			if (!i.Names.Contains(nick))
				i.Names.Add(nick);

			Save(l);
		}

		#region " Load/Save (Serialization) "
		public void Save(List<SeenInfo> l) {
			try {
				StreamWriter f = new StreamWriter("Data\\Seen.xml", false);
				new XmlSerializer(typeof(List<SeenInfo>)).Serialize(f, l);
				f.Close();
			} catch (Exception e) {
				Console.WriteLine("#" + e.Message);
			}
		}

		public List<SeenInfo> Load() {
			List<SeenInfo> l;
			try {
				FileStream f = new FileStream("Data\\Seen.xml", FileMode.Open);
				l = (List<SeenInfo>)new XmlSerializer(typeof(List<SeenInfo>)).Deserialize(f);
				f.Close();
			} catch (Exception e) {
				Console.WriteLine("#" + e.Message);
				l = new List<SeenInfo>();
			}
			return l;
		}
		#endregion

		#region " SeenInfo Class "
		[Serializable]
		public class SeenInfo {

			public SeenInfo() {
			}

			List<string> names = new List<string>();
			public List<string> Names {
				get {
					return names;
				}
				set {
					names = value;
				}
			}

			string network;
			public string Network {
				get {
					return network;
				}
				set {
					network = value;
				}
			}

			DateTime date;
			public DateTime Date {
				get {
					return date;
				}
				set {
					date = value;
				}
			}

			string text;
			public string Text {
				get {
					return text;
				}
				set {
					text = value;
				}
			}

			string ident;
			public string Ident {
				get {
					return ident;
				}
				set {
					ident = value;
				}
			}
		}
		#endregion
		#endregion

		#region " Event Handles "
		void Bot_OnMessage(Network network, Irc.IrcEventArgs e) {
			if (IsMatch("^seen \\?$", e.Data.Message)) {
				AnswerWithNotice(network, e, FormatBold("Use of Seen plugin:"));
				AnswerWithNotice(network, e, FormatItalic("seen <nick>") + " - Displays information when the Bot last saw <nick>.");
			}
			else if (IsMatch("^seen (?<nick>.*)$", e.Data.Message)) {
				SeenInfo i = FindName(network.Name, Matches["nick"].ToString(), l);
				if (i == null)
					Answer(network, e, "I never saw " + Matches["nick"].ToString() + " before.");
				else if (i.Ident == e.Data.Ident)
					Answer(network, e, "Looking for yourself, eh?");
				else {
					string hour = "hours";
					string minute = "minutes";
					TimeSpan t = (TimeSpan)(DateTime.Now - i.Date);
					if (t.TotalHours == 1)
						hour = "hour";
					if (t.Minutes == 1)
						minute = "minute";
					Answer(network, e, "I saw " + Matches["nick"].ToString() + " " + Convert.ToInt16(t.TotalHours).ToString() + " " + hour + " and " + t.Minutes.ToString() + " " + minute + " ago, " + i.Text + ".");
				}
			}
			else
				NewSeen(network.Name, e.Data.Nick, e.Data.Ident, "on " + e.Data.Channel + ", saying " + e.Data.Message);
		}

		void Bot_OnJoin(Network network, Irc.JoinEventArgs e) {
			NewSeen(network.Name, e.Data.Nick, e.Data.Ident, "joining " + e.Data.Channel);
		}

		void Bot_OnPart(Network network, Irc.PartEventArgs e) {
			NewSeen(network.Name, e.Data.Nick, e.Data.Ident, "leaving " + e.Data.Channel);
		}

		void Bot_OnQuit(Network network, Irc.QuitEventArgs e) {
			NewSeen(network.Name, e.Data.Nick, e.Data.Ident, "quitting IRC (" + e.Data.Message + ")");
		}

		void Bot_OnNickChange(Network network, Irc.NickChangeEventArgs e) {
			NewSeen(network.Name, e.Data.Nick, e.Data.Ident, "changing his nick from " + e.OldNickname + " to " + e.NewNickname);
		}
		#endregion

	}
}
