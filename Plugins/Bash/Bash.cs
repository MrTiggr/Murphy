/*
Bash Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
Copyright (C) 2005 Wolfgang Gottesheim, Hannes Sachsenhofer [http://www.sachsenhofer.com]

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
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
#endregion

namespace Murphy.Plugins {
	public class Bash : Plugin {

		#region " Constructor/Destructor "
		public Bash(Bot bot)
			: base(bot) {
			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
		}
		#endregion

		#region " Bash "
		Network n;
		Irc.IrcEventArgs e;
		void GetGermanBash() {
			Network n = this.n;
			Irc.IrcEventArgs e = this.e;

			HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create("http://german-bash.org/action/random/n/1");
			httpReq.Method = "GET";
			WebResponse httpRes = httpReq.GetResponse();
			StreamReader stream = new StreamReader(httpRes.GetResponseStream());
			string responseString = stream.ReadToEnd();
			stream.Close();
			httpRes.Close();

			Regex r = new Regex("\\<div class=\"zitat\"\\>(?<text>.*?)\\</div\\>",RegexOptions.Singleline);
			string text = r.Match(responseString).Groups["text"].ToString();
			r = new Regex("\\<a name=\"(?<number>\\d*)\"\\>\\</a\\>");
			string number = r.Match(responseString).Groups["number"].ToString();

			text = text.Replace("&lt;", "<");
			text = text.Replace("&gt;", ">");
			text = text.Replace("&quot;", "\"");
			text = text.Replace("<br />", "\r\n");
			text = text.Replace("&nbsp;", " ");
			text = text.Replace("&uuml;", "ü");
			text = text.Replace("&auml;", "ä");
			text = text.Replace("&ouml;", "ö");
			text = text.Replace("\r", "");
			text = text.Replace("\t", "");
			text = text.Trim();

			Answer(n, e, FormatBold("german-bash.org quote #" + number));
			foreach (string s in text.Split('\n'))
				if (s.Length > 0)
					Answer(n, e, s);
		}


		void GetBash() {
			Network n = this.n;
			Irc.IrcEventArgs e = this.e;
			HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create("http://bash.org/?random");
			httpReq.Method = "GET";
			WebResponse httpRes = httpReq.GetResponse();
			StreamReader stream = new StreamReader(httpRes.GetResponseStream());
			string responseString = stream.ReadToEnd();
			stream.Close();
			httpRes.Close();

			int start = Regex.Match(responseString, "class=\"qt\"").Index + 11;
			int end = Regex.Match(responseString, "</p>\n<p class=\"quote\">").Index;
			string cutstring = responseString.Substring(start, end - start);
			cutstring = cutstring.Replace("&lt;", "<");
			cutstring = cutstring.Replace("&gt;", ">");
			cutstring = cutstring.Replace("&quot;", "\"");
			cutstring = cutstring.Replace("<br />", "\r\n");
			cutstring = cutstring.Replace("&nbsp;", " ");
			cutstring = cutstring.Replace("\r", "");
			Match m = Regex.Match(responseString, "<a href=\".([0-9]{2,10})\" title");

			Answer(n, e, FormatBold("bash.org quote #" + m.Groups[1].Value));
			foreach (string s in cutstring.Split('\n'))
				if (s.Length > 0)
					Answer(n, e, s);
		}
		#endregion

		#region " Event handles "
		void Bot_OnMessage(Network network, Irc.IrcEventArgs e) {
			if (IsMatch("^bash \\?$", e.Data.Message)) {
				AnswerWithNotice(n, e, FormatBold("Use of Bash plugin:"));
				AnswerWithNotice(n, e, FormatItalic("bash") + " - Prints a random quote from http://www.bash.org.");
			}
			else if (IsMatch("^bash$", e.Data.Message)) {
				this.n = network;
				this.e = e;
				new System.Threading.Thread(new ThreadStart(GetBash)).Start();
			}
			else if (IsMatch("^german bash$", e.Data.Message)) {
				this.n = network;
				this.e = e;
				new System.Threading.Thread(new ThreadStart(GetGermanBash)).Start();
			}
		}
		#endregion
	}
}
