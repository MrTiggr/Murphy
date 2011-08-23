/*
Events Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
	public class Events : Plugin {

		#region " Constructor/Destructor "
		public Events(Bot bot)
			: base(bot) {
			Bot.OnJoin += new JoinEventHandler(Bot_OnJoin);
			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
		}
		#endregion

		#region " Events "
		void DeleteOldEvents(List<EventInfo> eventInfos) {
			List<EventInfo> l = new List<EventInfo>();
			foreach (EventInfo e in eventInfos)
				if (e.Date < DateTime.Now)
					l.Add(e);
			if (l.Count > 0) {
				foreach (EventInfo e in l)
					eventInfos.Remove(e);
				Save(eventInfos);
			}
		}

		void Remove(string nick, EventInfo e) {
			e.There.Remove(nick);
			e.MaybeThere.Remove(nick);
			e.NotThere.Remove(nick);
		}

		#region " Load/Save (Serialization) "
		public void Save(List<EventInfo> eventInfos) {
			eventInfos.Sort(new EventInfoComparer());

			StreamWriter f = new StreamWriter("Data\\Events.xml", false);
			new XmlSerializer(typeof(List<EventInfo>)).Serialize(f, eventInfos);
			f.Close();
		}

		public List<EventInfo> Load() {
			try {
				FileStream f = new FileStream("Data\\Events.xml", FileMode.Open);
				List<EventInfo> eventInfos = (List<EventInfo>)new XmlSerializer(typeof(List<EventInfo>)).Deserialize(f);
				f.Close();
				return eventInfos;
			} catch (Exception e) {
				Console.WriteLine("# " + e.Message);
				return new List<EventInfo>();
			}
		}
		#endregion

		#region " EventInfoComparer "
		class EventInfoComparer : IComparer<EventInfo> {
			public int Compare(EventInfo a, EventInfo b) {
				return a.Date.CompareTo(b.Date);
			}

			public bool Equals(EventInfo a, EventInfo b) {
				return a.Equals(b);
			}

			public int GetHashCode(EventInfo a) {
				return a.GetHashCode();
			}
		}
		#endregion

		#region " EventInfo Class "
		[Serializable]
		public class EventInfo {

			public EventInfo() {
				there = new List<string>();
				maybeThere = new List<string>();
				notThere = new List<string>();
			}

			public EventInfo(DateTime date, string text) {
				this.date = date;
				this.text = text;
				there = new List<string>();
				maybeThere = new List<string>();
				notThere = new List<string>();
			}

			List<string> there;
			public List<string> There {
				get {
					return there;
				}
				set {
					there = value;
				}
			}

			List<string> maybeThere;
			public List<string> MaybeThere {
				get {
					return maybeThere;
				}
				set {
					maybeThere = value;
				}
			}

			List<string> notThere;
			public List<string> NotThere {
				get {
					return notThere;
				}
				set {
					notThere = value;
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
		}
		#endregion
		#endregion

		#region " Event Handles "
		void Bot_OnJoin(Network n, Irc.JoinEventArgs e) {
			List<EventInfo> eventInfos = Load();
			DeleteOldEvents(eventInfos);
			int i = eventInfos.Count;
			if (i==1)
				n.SendMessage(Murphy.Irc.SendType.Notice, e.Who, "There is 1 upcoming event.");
			else
				n.SendMessage(Murphy.Irc.SendType.Notice, e.Who, "There are " + i + " upcoming events.");
		}


		void Bot_OnMessage(Network n, Irc.IrcEventArgs e) {


			if (IsMatch("^events \\?$", e.Data.Message)) {
				AnswerWithNotice(n, e, FormatBold("Use of Events plugin:"));
				AnswerWithNotice(n, e, FormatItalic("list events") + " - Lists details of all upcoming events.");
				AnswerWithNotice(n, e, FormatItalic("add <nick> to [<number>]") + " - Adds <nick> as there to the specified event (You may use 'me' instead of <nick>).");
				AnswerWithNotice(n, e, FormatItalic("add <nick> not to [<number>]") + " - Adds <nick> as not there to the specified event (You may use 'me' instead of <nick>).");
				AnswerWithNotice(n, e, FormatItalic("add <nick> maybe to [<number>]") + " - Adds <nick> as maybe there to the specified event (You may use 'me' instead of <nick>).");
				AnswerWithNotice(n, e, FormatItalic("remove <nick> from [<number>]") + " - Removes <nick> from the specified event (You may use 'me' instead of <nick>).");
				AnswerWithNotice(n, e, FormatItalic("add event <day>.<month>.<year> <hours>:<minutes> <text>") + " - Adds the specified event.");
				AnswerWithNotice(n, e, FormatItalic("edit event [<number>] <day>.<month>.<year> <hours>:<minutes> <text>") + " - Edits the specified event, but leaves the there/not there/maybe there nicknames intact.");
				AnswerWithNotice(n, e, FormatItalic("remove event [<number>]") + " - Removes the entire specified event.");
				AnswerWithNotice(n, e, FormatItalic("clear event [<number>]") + " - Removes all the there/not there/maybe there nicknames from the specified event, but leaves the event itself intact.");
			}
			else if (IsMatch("^list events$", e.Data.Message)) {
				List<EventInfo> eventInfos = Load();
				DeleteOldEvents(eventInfos);
				int i = 0;
				foreach (EventInfo eventinfo in eventInfos) {
					string there = "";
					foreach (string s in eventinfo.There)
						there += s + ", ";
					if (there.Length > 0)
						there = there.Substring(0, there.Length - 2);
					string maybeThere = "";
					foreach (string s in eventinfo.MaybeThere)
						maybeThere += s + ", ";
					if (maybeThere.Length > 0)
						maybeThere = maybeThere.Substring(0, maybeThere.Length - 2);
					string notThere = "";
					foreach (string s in eventinfo.NotThere)
						notThere += s + ", ";
					if (notThere.Length > 0)
						notThere = notThere.Substring(0, notThere.Length - 2);

					AnswerWithNotice(n, e, FormatBold("[" + Format(i) + "]") + " - " + FormatBold(eventinfo.Text) + " - scheduled for " + eventinfo.Date.ToLongDateString() + " " + eventinfo.Date.ToShortTimeString());
					AnswerWithNotice(n, e, "there: " + FormatItalic(there) + " - maybe there: " + FormatItalic(maybeThere) + " - not there: " + FormatItalic(notThere));
					i++;
				}
				if (i <= 0)
					AnswerWithNotice(n, e, "There are no upcoming events.");
				return;
			}
			else if (IsMatch("^add event (?<day>\\d{1,2})\\.(?<month>\\d{1,2})\\.(?<year>\\d{4}) (?<hour>\\d{1,2}):(?<minute>\\d{1,2}) (?<text>.*)$", e.Data.Message)) {
				DateTime d = new DateTime(int.Parse(Matches["year"].ToString()), int.Parse(Matches["month"].ToString()), int.Parse(Matches["day"].ToString()), int.Parse(Matches["hour"].ToString()), int.Parse(Matches["minute"].ToString()), 0);
				List<EventInfo> eventInfos = Load();
				eventInfos.Add(new EventInfo(d, Matches["text"].ToString()));
				Save(eventInfos);
				AnswerWithNotice(n, e, "I added the event.");
				return;
			}
			else if (IsMatch("^add (?<nick>\\w*) to \\[(?<event>\\d{1,3})\\]$", e.Data.Message)) {
				List<EventInfo> eventInfos = Load();
				int i=int.Parse(Matches["event"].ToString());
				if (i >= eventInfos.Count) {
					AnswerWithNotice(n, e, "There is no such event.");
					return;
				}
				EventInfo eventinfo = eventInfos[i];
				string nick = Matches["nick"].ToString();
				if (nick.ToLower() == "me")
					nick = e.Data.Nick;
				Remove(nick, eventinfo);
				eventinfo.There.Add(nick);
				Save(eventInfos);
				AnswerWithNotice(n, e, "I added " + FormatItalic(nick) + " as " + FormatItalic("there") + ".");
				return;
			}
			else if (IsMatch("^add (?<nick>\\w*) not to \\[(?<event>\\d{1,3})\\]$", e.Data.Message)) {
				List<EventInfo> eventInfos = Load();
				int i = int.Parse(Matches["event"].ToString());
				if (i >= eventInfos.Count) {
					AnswerWithNotice(n, e, "There is no such event.");
					return;
				}
				EventInfo eventinfo = eventInfos[i];
				string nick = Matches["nick"].ToString();
				if (nick.ToLower() == "me")
					nick = e.Data.Nick;
				Remove(nick, eventinfo);
				eventinfo.NotThere.Add(nick);
				Save(eventInfos);
				AnswerWithNotice(n, e, "I added " + FormatItalic(nick) + " as " + FormatItalic("not there") + ".");
				return;
			}
			else if (IsMatch("^add (?<nick>\\w*) maybe to \\[(?<event>\\d{1,3})\\]$", e.Data.Message)) {
				List<EventInfo> eventInfos = Load();
				int i = int.Parse(Matches["event"].ToString());
				if (i >= eventInfos.Count) {
					AnswerWithNotice(n, e, "There is no such event.");
					return;
				}
				EventInfo eventinfo = eventInfos[i];
				string nick = Matches["nick"].ToString();
				if (nick.ToLower() == "me")
					nick = e.Data.Nick;
				Remove(nick, eventinfo);
				eventinfo.MaybeThere.Add(nick);
				Save(eventInfos);
				AnswerWithNotice(n, e, "I added " + FormatItalic(nick) + " as " + FormatItalic("maybe there") + ".");
				return;
			}
			else if (IsMatch("^remove (?<nick>\\w*) from \\[(?<event>\\d{1,3})\\]$", e.Data.Message)) {
				string nick = Matches["nick"].ToString();
				if (nick.ToLower() == "me")
					nick = e.Data.Nick;
				List<EventInfo> eventInfos = Load();
				int i = int.Parse(Matches["event"].ToString());
				if (i >= eventInfos.Count) {
					AnswerWithNotice(n, e, "There is no such event.");
					return;
				}
				Remove(nick, eventInfos[i]);
				Save(eventInfos);
				AnswerWithNotice(n, e, "I removed " + FormatItalic(nick) + ".");
				return;
			}
			else if (IsMatch("^remove event \\[(?<event>\\d{1,3})\\]$", e.Data.Message)) {
				List<EventInfo> eventInfos = Load();
				int i = int.Parse(Matches["event"].ToString());
				if (i >= eventInfos.Count) {
					AnswerWithNotice(n, e, "There is no such event.");
					return;
				}
				eventInfos.RemoveAt(i);
				Save(eventInfos);
				AnswerWithNotice(n, e, "I removed the event.");
				return;
			}
			else if (IsMatch("^clear event \\[(?<event>\\d{1,3})\\]$", e.Data.Message)) {
				List<EventInfo> eventInfos = Load();
				int i = int.Parse(Matches["event"].ToString());
				if (i >= eventInfos.Count) {
					AnswerWithNotice(n, e, "There is no such event.");
					return;
				}
				EventInfo eventinfo = eventInfos[i];
				eventinfo.There.Clear();
				eventinfo.MaybeThere.Clear();
				eventinfo.NotThere.Clear();
				Save(eventInfos);
				AnswerWithNotice(n, e, "I cleared the event.");
				return;
			}
			else if (IsMatch("^edit event \\[(?<event>\\d{1,3})\\] (?<day>\\d{1,2})\\.(?<month>\\d{1,2})\\.(?<year>\\d{4}) (?<hour>\\d{1,2}):(?<minute>\\d{1,2}) (?<text>.*)$", e.Data.Message)) {
				List<EventInfo> eventInfos = Load();
				int i = int.Parse(Matches["event"].ToString());
				if (i >= eventInfos.Count) {
					AnswerWithNotice(n, e, "There is no such event.");
					return;
				}
				EventInfo eventinfo = eventInfos[i];
				DateTime d = new DateTime(int.Parse(Matches["year"].ToString()), int.Parse(Matches["month"].ToString()), int.Parse(Matches["day"].ToString()), int.Parse(Matches["hour"].ToString()), int.Parse(Matches["minute"].ToString()), 0);
				eventinfo.Date = d;
				eventinfo.Text = Matches["text"].ToString();
				Save(eventInfos);
				AnswerWithNotice(n, e, "I edited the event.");
				return;
			}

		}
		#endregion

	}
}
