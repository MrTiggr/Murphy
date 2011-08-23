/*
GoogleSearch Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
	public class GoogleSearch : Plugin {

		#region " Constructor/Destructor "
		public GoogleSearch(Bot bot)
			: base(bot) {
			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
		}
		#endregion

		#region " Event Handles "
		void Bot_OnMessage(Network n, Irc.IrcEventArgs e) {
			if (IsMatch("^googlesearch \\?$", e.Data.Message)) {
				AnswerWithNotice(n, e, FormatBold("Use of GoogleSearch plugin:"));
				AnswerWithNotice(n, e, FormatItalic("google <search term>") + " - Searches google for <search term> and lists the first two results.");
				AnswerWithNotice(n, e, FormatItalic("google <number of results> <search term>") + " - Searches google for <search term> and lists the first <number of results> (maximum is 10) results.");
			}
			else if (IsMatch("^google ((?<count>\\d{1,2}) )?(?<term>.*)$", e.Data.Message)) {
				int count = 2;
				if (Matches["count"].Length > 0)
					count = int.Parse(Matches["count"].ToString());
				if (count > 10)
					count = 10;

				try {
					Google.Google.GoogleSearchService s = new Google.Google.GoogleSearchService();
					Google.Google.GoogleSearchResult results = s.doGoogleSearch(Bot.Configuration["Plugins"]["GoogleSearch"].Attributes["Key"].Value, Matches["term"].ToString(), 0, count, false, "", false, "", "", "");

					if (count > results.resultElements.Length)
						count = results.resultElements.Length;

					if (count > 0) {
						Answer(n, e, FormatBold(count.ToString() + " results for " + FormatItalic(Matches["term"].ToString()) + "."));
						for (int i = 0; i < count; i++) {
							string result = results.resultElements[i].URL + " (" + FormatBold(results.resultElements[i].title) + " - " + results.resultElements[i].snippet + ")";
							result = result.Replace("<b>", "").Replace("</b>", "").Replace("<br>", "").Replace("&#39;", "'").Replace("&amp;", "&");
							Answer(n, e, result);
						}
					}
					else
						Answer(n, e, "No results found for " + FormatItalic(Matches["term"].ToString()) + ".");
				} catch (Exception ex) {
					Answer(n, e, "Error in GoogleSearch plugin: " + FormatItalic(ex.Message));
				}
			}
		}
		#endregion

	}
}
