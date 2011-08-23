/*
Quote Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
#endregion

namespace Murphy.Plugins {
	public class Quote : Plugin {

		#region " Constructor/Destructor "
		Random r = new Random();
		public Quote(Bot bot):base(bot) {
			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
		}
		#endregion

		#region " Quote "
		string GetQuote(string type, List<QuoteInfo> quotes) {
			return GetQuote(GetQuotes(type, quotes));
		}

		List<QuoteInfo> GetQuotes(string type, List<QuoteInfo> quotes) {
			List<QuoteInfo> l = new List<QuoteInfo>();
			foreach (QuoteInfo q in quotes)
				if (q.Type.ToLower() == type.ToLower())
					l.Add(q);
			return l;
		}

		string GetQuote(List<QuoteInfo> quotes) {
			if (quotes.Count <= 0)
				return "";
			return quotes[r.Next(0, quotes.Count)].Text;
		}

		#region " Load/Save (Serialization) "
		public void Save(List<QuoteInfo> quotes) {
			StreamWriter f = new StreamWriter("Data\\Quotes.xml", false);
			new XmlSerializer(typeof(List<QuoteInfo>)).Serialize(f, quotes);
			f.Close();
		}

		public List<QuoteInfo> Load() {
			List<QuoteInfo> quotes;
			try {
				FileStream f = new FileStream("Data\\Quotes.xml", FileMode.Open);
				quotes = (List<QuoteInfo>)new XmlSerializer(typeof(List<QuoteInfo>)).Deserialize(f);
				f.Close();
			} catch (Exception e) {
				Console.WriteLine("# " + e.Message);
				quotes = new List<QuoteInfo>();
			}
			return quotes;
		}
		#endregion

		#region " QuoteInfo "
		public class QuoteInfo {

			public QuoteInfo() { }

			public QuoteInfo(string type, string text) {
				this.type = type;
				this.text = text;
			}

			string type;
			public string Type {
				get {
					return type;
				}
				set {
					type = value;
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

		#region " Event handles "
		void Bot_OnMessage(Network n, Irc.IrcEventArgs e) {

			if (IsMatch("^quote \\?$", e.Data.Message)) {
				AnswerWithNotice(n, e, FormatBold("Use of Quote plugin:"));
				AnswerWithNotice(n, e, FormatItalic("quote") + " - Prints a random quote.");
				AnswerWithNotice(n, e, FormatItalic("quote <name>") + " - Prints a random quote from <name>.");
				AnswerWithNotice(n, e, FormatItalic("quote <name> [<number>]") + " - Prints the specified quote from <name>.");
				AnswerWithNotice(n, e, FormatItalic("add quote from <name> <quote>") + " - Adds the a quote from <name>.");
				AnswerWithNotice(n, e, FormatItalic("list quotes") + " - Lists the sources of all quotes.");
				AnswerWithNotice(n, e, FormatItalic("list quotes from <name>") + " - Lists all quotes of <name>.");
				AnswerWithNotice(n, e, FormatItalic("remove quote [<number>] from <name>") + " - Removes the specified quote from <name>.");
				AnswerWithNotice(n, e, FormatItalic("edit quote [<number>] from <name> <new quote>") + " - Changes the specified quote from <name> to <new quote>.");
			}
			else if (IsMatch("^quote$", e.Data.Message)) {
				string quote = GetQuote(Load());
				if (quote.Length > 0)
					Answer(n, e, quote);
				else
					AnswerWithNotice(n, e, "I'm sorry, but I don't know any quotes.");
			}
			else if (IsMatch("^quote (?<type>\\w*)$", e.Data.Message)) {
				string quote = GetQuote(Matches["type"].ToString(), Load());
				if (quote.Length > 0)
					Answer(n, e, quote);
				else
					AnswerWithNotice(n, e, "I'm sorry, but I don't know any quotes from " + FormatItalic(Matches["type"].ToString()) + ".");
			}
			else if (IsMatch("^quote (?<type>\\w*?) \\[(?<number>\\d{1,2})\\]$", e.Data.Message)) {
				List<QuoteInfo> quotes = GetQuotes(Matches["type"].ToString(), Load());
				int i = int.Parse(Matches["number"].ToString());
				if (quotes.Count >= i + 1)
					Answer(n, e, quotes[i].Text);
				else
					AnswerWithNotice(n, e, "I'm sorry, but this quote does not exist.");
			}
			else if (IsMatch("^add quote from (?<type>\\w*?) (?<text>.*)$", e.Data.Message)) {
				List<QuoteInfo> quotes = Load();
				quotes.Add(new QuoteInfo(Matches["type"].ToString(), Matches["text"].ToString()));
				Save(quotes);
				AnswerWithNotice(n, e, "I added this quote from " + FormatItalic(Matches["type"].ToString()) + ".");
			}
			else if (IsMatch("^list quotes$", e.Data.Message)) {
				List<QuoteInfo> quotes = Load();
				if (quotes.Count > 0) {
					List<string> names = new List<string>();
					foreach (QuoteInfo q in quotes)
						if (!names.Contains(q.Type.ToLower()))
							names.Add(q.Type.ToLower());
					AnswerWithNotice(n, e, FormatBold("The sources of all quotes:"));
					foreach (string s in names)
						AnswerWithNotice(n, e, s);
				}
				else
					AnswerWithNotice(n, e, "I'm sorry, but I don't know any quotes.");
			}
			else if (IsMatch("^list quotes from (?<type>\\w*?)$", e.Data.Message)) {
				List<QuoteInfo> quotes = GetQuotes(Matches["type"].ToString(), Load());
				if (quotes.Count > 0) {
					AnswerWithNotice(n, e, FormatBold("All Quotes from " + FormatItalic(Matches["type"].ToString()) + ":"));
					for (int i = 0; i < quotes.Count; i++)
						AnswerWithNotice(n, e, FormatBold("[" + Format(i) + "]") + " " + quotes[i].Text);
				}
				else
					AnswerWithNotice(n, e, "I'm sorry, but I don't know any quotes from " + FormatItalic(Matches["type"].ToString()) + ".");
			}
			else if (IsMatch("^remove quote \\[(?<number>\\d{1,3})\\] from (?<type>\\w*?)$", e.Data.Message)) {
				List<QuoteInfo> allQuotes = Load();
				List<QuoteInfo> quotes = GetQuotes(Matches["type"].ToString(), allQuotes);
				int i = int.Parse(Matches["number"].ToString());
				if (quotes.Count >= i + 1) {
					allQuotes.Remove(quotes[i]);
					Save(allQuotes);
					AnswerWithNotice(n, e, "I removed this quote from " + FormatItalic(Matches["type"].ToString()) + ".");
				}
				else
					AnswerWithNotice(n, e, "I'm sorry, but this quote does not exist.");
			}
			else if (IsMatch("^edit quote \\[(?<number>\\d{1,3})\\] from (?<type>\\w*?) (?<text>.*)$", e.Data.Message)) {
				List<QuoteInfo> allQuotes = Load();
				List<QuoteInfo> quotes = GetQuotes(Matches["type"].ToString(), allQuotes);
				int i = int.Parse(Matches["number"].ToString());
				if (quotes.Count >= i + 1) {
					quotes[i].Text = Matches["text"].ToString();
					Save(allQuotes);
					AnswerWithNotice(n, e, "I edited this quote from " + FormatItalic(Matches["type"].ToString()) + ".");
				}
				else
					AnswerWithNotice(n, e, "I'm sorry, but this quote does not exist.");
			}
		}
		#endregion
	}
}
