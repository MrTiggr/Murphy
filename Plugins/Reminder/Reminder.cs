/*
Reminder Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
using System.Text.RegularExpressions;
#endregion

namespace Murphy.Plugins {
	public class Reminder : Plugin, IDisposable {

		#region " Constructor/Destructor "
		Thread t;
		public Reminder(Bot bot)
			: base(bot) {
			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
		}

		~Reminder() {
			Dispose();
		}

		public void Dispose() {
			t.Abort();
		}
		#endregion

		#region " Remind "
		void StartThread() {
			if (t == null || t.ThreadState != ThreadState.Running) {
				t = new Thread(new ThreadStart(DoRemind));
				t.Start();
			}
		}

		void DoRemind() {
			List<RemindInfo> l = Load();
			while (l.Count > 0) {
				List<RemindInfo> tmp = new List<RemindInfo>();
				foreach (RemindInfo i in l) {
					if (i.Date < DateTime.Now) {
						Network network = Bot.GetNetworkByName(i.Network);
						if (network != null) {
							if (i.IsPrivate)
								network.SendMessage(Murphy.Irc.SendType.Message, i.User, i.User + ", time's up! " + i.Message);
							else
								network.SendMessage(Murphy.Irc.SendType.Message, i.Channel, i.User + ", time's up! " + i.Message);
						}
						tmp.Add(i);
					}
				}
				foreach (RemindInfo i in tmp)
					l.Remove(i);
				if (tmp.Count > 0)
					Save(l);
				Thread.Sleep(10000);
			}
		}

		#region " Load/Save (Serialization) "
		public void Save(List<RemindInfo> l) {
			StreamWriter f = new StreamWriter("Data\\Reminder.xml", false);
			new XmlSerializer(typeof(List<RemindInfo>)).Serialize(f, l);
			f.Close();
		}

		public List<RemindInfo> Load() {
			List<RemindInfo> l;
			try {
				FileStream f = new FileStream("Data\\Reminder.xml", FileMode.Open);
				l = (List<RemindInfo>)new XmlSerializer(typeof(List<RemindInfo>)).Deserialize(f);
				f.Close();
			} catch (Exception e) {
				Console.WriteLine("# " + e.Message);
				l = new List<RemindInfo>();
			}
			return l;
		}
		#endregion

		#region " RemindInfo Class "
		[Serializable]
		public class RemindInfo {

			public RemindInfo() {
			}

			bool isPrivate;
			public bool IsPrivate {
				get {
					return isPrivate;
				}
				set {
					isPrivate = value;
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

			string network;
			public string Network {
				get {
					return network;
				}
				set {
					network = value;
				}
			}

			string message;
			public string Message {
				get {
					return message;
				}
				set {
					message = value;
				}
			}

			string user;
			public string User {
				get {
					return user;
				}
				set {
					user = value;
				}
			}

			string channel;
			public string Channel {
				get {
					return channel;
				}
				set {
					channel = value;
				}
			}
		}
		#endregion
		#endregion

		#region " Event Handles "
		void Bot_OnMessage(Network network, Irc.IrcEventArgs e) {

			if (IsMatch("^reminder \\?$", e.Data.Message)) {
				AnswerWithNotice(network, e, FormatBold("Use of Reminder plugin:"));
				AnswerWithNotice(network, e, FormatItalic("remind me in <minutes> <message>") + " - Reminds you in <minutes> minutes.");
				AnswerWithNotice(network, e, FormatItalic("remind me at <hours>:<minutes> <message>") + " - Reminds you at the given time.");
			}
			else if (IsMatch("^remind me in (?<minutes>\\d{1,3}) (?<message>.*)$", e.Data.Message)) {
				List<RemindInfo> l = Load();
				RemindInfo i = new RemindInfo();
				i.Network = network.Name;
				i.Channel = e.Data.Channel;
				i.User = e.Data.Nick;
				i.Message = Matches["message"].ToString();
				i.Date = DateTime.Now.AddMinutes(int.Parse(Matches["minutes"].ToString()));
				i.IsPrivate = e.Data.Type == Irc.ReceiveType.QueryMessage;
				l.Add(i);
				Save(l);
				StartThread();
				AnswerWithNotice(network, e, "You will be reminded.");
			}
			else if (IsMatch("^remind me at (?<hours>\\d{1,2}):(?<minutes>\\d{1,2}) (?<message>.*)$",e.Data.Message)) {
				List<RemindInfo> l = Load();
				RemindInfo i = new RemindInfo();
				i.Network = network.Name;
				i.Channel = e.Data.Channel;
				i.User = e.Data.Nick;
				i.Message = Matches["message"].ToString();
				i.Date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, int.Parse(Matches["hours"].ToString()), int.Parse(Matches["minutes"].ToString()), 0);
				if (i.Date < DateTime.Now)
					i.Date = i.Date.AddDays(1);
				i.IsPrivate = e.Data.Type == Irc.ReceiveType.QueryMessage;
				l.Add(i);
				Save(l);
				StartThread();
				AnswerWithNotice(network, e, "You will be reminded.");
			}
		}
		#endregion
	}
}
