/*
Eightball Plugin for The #bitcoin-police IRC Bot - Murphy [http://www.bitcoinpolice.org]
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
	public class Eightball : Plugin {

		#region " Constructor/Destructor "
		Random r = new Random();
		string[] answers = new string[] { "No", "Yes", "Never ever", "All signs point to yes", "All signs point to no", "Definitely", "I'm not quite sure about that", "I don't think so", "I think so", "When hell freezes over", "What a silly question", "I don't want to say anything about this serious matter", "Uhm, I don't want to get in trouble" };
		public Eightball(Bot bot)
			: base(bot) {
			Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
			Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
		}
		#endregion

		#region " Event handles "
		void Bot_OnMessage(Network n, Irc.IrcEventArgs e) {
			if (IsMatch("^Eightball \\?$", e.Data.Message)) {
				AnswerWithNotice(n, e, FormatBold("Use of Eightball plugin:"));
				AnswerWithNotice(n, e, FormatItalic("<Botname> <your question, at least 7 characters long>?") + " - The bot will answer.");
			}
			else if (IsMatch("^" + n.Nickname + " .{7,}\\?\\s*$", e.Data.Message))
				Answer(n, e, answers[r.Next(answers.Length)] + ", " + e.Data.Nick + ".");
		}
		#endregion
	}
}
