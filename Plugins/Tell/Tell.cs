/*
Tell Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
	public class Tell : Plugin {

		#region " Constructor/Destructor "
		public Tell(Bot bot)
			: base(bot) {
			Bot.OnChannelMessage+=new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnJoin += new JoinEventHandler(Bot_OnJoin);
		}
		#endregion

		#region " Tell "
		void Check(Network network, Irc.JoinEventArgs e) {
			List<TellInfo> l = Load();
			List<TellInfo> tmp = new List<TellInfo>();
			foreach (TellInfo t in l)
				if (t.Target == e.Data.Nick && t.Network == network.Name) {
					tmp.Add(t);
					Answer(network, e, FormatBold(e.Data.Nick) + ", on " + FormatBold(t.Date.ToLongDateString()) + " " + FormatBold(t.Date.ToLongTimeString()) + " " + FormatBold(t.Name) + " wanted to tell you " + FormatBold(t.Text) + ".");
				}
			foreach (TellInfo t in tmp)
				l.Remove(t);
			if (tmp.Count > 0)
				Save(l);
		}


		#region " Load/Save (Serialization) "
		public void Save(List<TellInfo> l) {
			StreamWriter f = new StreamWriter("Data\\Tell.xml", false);
			new XmlSerializer(typeof(List<TellInfo>)).Serialize(f, l);
			f.Close();
		}

		public List<TellInfo> Load() {
			List<TellInfo> l;
			try {
				FileStream f = new FileStream("Data\\Tell.xml", FileMode.Open);
				l = (List<TellInfo>)new XmlSerializer(typeof(List<TellInfo>)).Deserialize(f);
				f.Close();
			} catch (Exception e) {
				Console.WriteLine("# " + e.Message);
				l = new List<TellInfo>();
			}
			return l;
		}
		#endregion

		#region " TellInfo Class "
		[Serializable]
		public class TellInfo {

			public TellInfo() {}

			string target;
			public string Target {
				get {
					return target;
				}
				set {
					target = value;
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

			string name;
			public string Name {
				get {
					return name;
				}
				set {
					name = value;
				}
			}
		}
		#endregion
		#endregion

		#region " Event Handles "
		void Bot_OnMessage(Network network, Irc.IrcEventArgs e) {

			if (IsMatch("^tell \\?$", e.Data.Message)) {
				AnswerWithNotice(network, e, FormatBold("Use of Tell plugin:"));
				AnswerWithNotice(network, e, FormatItalic("tell <recipient> <message>") + " - Tells <recipient> the <message> the next time he joins.");
			}
			else if (IsMatch("^tell (?<target>.*?) (?<message>.*)$", e.Data.Message)) {
				List<TellInfo> l = Load();
				TellInfo t = new TellInfo();
				t.Date = DateTime.Now;
				t.Name = e.Data.Nick;
				t.Network = network.Name;
				t.Target = Matches["target"].ToString();
				t.Text = Matches["message"].ToString();
				l.Add(t);
				Save(l);
				AnswerWithNotice(network, e, "I'll tell your message.");
			}
		}

		void Bot_OnJoin(Network network, Irc.JoinEventArgs e) {
			Check(network, e);
		}
		#endregion
	}
}
