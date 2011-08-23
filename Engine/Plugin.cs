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

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Murphy {
	public abstract class Plugin {

		#region " Constructor "
		public Plugin(Bot bot) {
			this.bot = bot;
		}
		#endregion

		#region " Properties "
		Bot bot;
		protected Bot Bot {
			get {
				return bot;
			}
		}

		GroupCollection matches;
		public GroupCollection Matches {
			get {
				return matches;
			}

			set {
				matches = value;
			}
		} 


		#endregion

		#region " Methods "
		protected internal bool IsMatch(string pattern, string input) {
			Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
			if (r.IsMatch(input)) {
				matches = r.Match(input).Groups;
				return true;
			}
			else {
				matches = null;
				return false;
			}
		}


		protected internal static string Format(int i) {
			if (i >= 10)
				return i.ToString();
			else
				return "0" + i.ToString();
		}


		protected internal static string FormatBold(string s) {
			return "\u0002" + s + "\u0002";
		}


		protected internal static string FormatItalic(string s) {
			return "\u0016" + s + "\u0016";
		}


		protected internal static string FormatUnderlined(string s) {
			return "\u001F" + s + "\u001F";
		}


		protected internal static string FormatColor(string s, IrcColor foreground) {
			return "\u0003" + ((int)foreground).ToString() + s + "\u0003" +  ((int)foreground).ToString();
		}


		protected internal static string FormatColor(string s, IrcColor foreground, IrcColor background) {
			return "\u0003" + ((int)foreground).ToString() + "," + ((int)background).ToString() + s + "\u0003" + ((int)foreground).ToString() + "," + ((int)background).ToString();
		}


		protected internal static void Answer(Network n, Irc.IrcEventArgs e, string s) {
			if (e.Data.Type == Irc.ReceiveType.QueryMessage)
				n.SendMessage(Murphy.Irc.SendType.Message, e.Data.Nick, s);
			else
				n.SendMessage(Murphy.Irc.SendType.Message, e.Data.Channel, s);
		}

		protected internal static void Answer(Network n, Irc.JoinEventArgs e, string s) {
			n.SendMessage(Murphy.Irc.SendType.Message, e.Data.Channel, s);
		}


		protected internal static void AnswerWithNotice(Network n, Irc.IrcEventArgs e, string s) {
			n.SendMessage(Murphy.Irc.SendType.Notice, e.Data.Nick, s);
		}

        protected internal static void AnswerWithQuery(Network n, Irc.IrcEventArgs e, string s)
        {
            n.SendMessage(Murphy.Irc.SendType.Message, e.Data.Nick, s);
        }


		protected internal static string GetFullUser(Network n, string nick) {
			Irc.IrcUser u = n.GetIrcUser(nick);
			return u.Nick + "!" + u.Ident + "@" + u.Host;
		}
		#endregion
	}

	public enum IrcColor {
		White = 00,
		Black = 01,
		Blue = 02,
		Green = 03,
		LightRed = 04,
		Brown = 05,
		Purple = 06,
		Orange = 07,
		Yellow = 08,
		LightGreen = 09,
		Cyan = 10,
		LightCyan = 11,
		LightBlue = 12,
		Pink = 13,
		Grey = 14,
		LightGrey = 15
	};

}
