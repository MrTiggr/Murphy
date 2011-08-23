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

#region Using directives
using System;
using System.Collections.Generic;
using System.Windows.Forms;
#endregion

namespace Murphy {
	static class Program {

		[STAThread]
		static void Main() {
			bot = new Bot();
			Application.Run(new frmMain());	
		}

		static Bot bot;
		public static Bot Bot {
			get {
				return bot;
			}
		}
	}
}