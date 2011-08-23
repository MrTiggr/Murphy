/*
Murphy: The #bitcoin-police IRC Bot
Copyright (C) 2005 Hannes Sachsenhofer

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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
#endregion

namespace Murphy {
	partial class frmMain : Form {

		delegate void EmptyDelegate();
		Bot bot;
		System.IO.StringWriter writer;
		System.Threading.Thread thread;


		public frmMain() {
			InitializeComponent();
		}


		private void SystrayIcon_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				if (Visible)
					Hide();
				else {
					Show();
					Activate();
					txtLog.SelectionStart = txtLog.Text.Length;
					txtLog.ScrollToCaret();
				}
			}
		}


		private void btnClose_Click(object sender, EventArgs e) {
			Application.Exit();
		}


		private void frmMain_Load(object sender, EventArgs e) {
			writer = new System.IO.StringWriter();
			Console.SetOut(writer);

			Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
			FormClosing += new FormClosingEventHandler(frmMain_FormClosing);
			txtLog.TextChanged += new EventHandler(txtLog_TextChanged);
			txtMessage.KeyDown += new KeyEventHandler(txtMessage_KeyDown);

			bot = new Bot();
			bot.OnRawMessage+=new IrcEventHandler(Bot_OnRawMessage);

			bot.ConnectAll();

			thread = new System.Threading.Thread(new System.Threading.ThreadStart(Log));
			thread.Start();
		}


		void Bot_OnRawMessage(Network network, Irc.IrcEventArgs e) {
			Console.WriteLine(e.Data.RawMessage);
		}


		void Log() {
			while (true) {
				System.Threading.Thread.Sleep(1000);
				UpdateLog();
			}
		}


		void UpdateLog() {
			if (InvokeRequired) {
				BeginInvoke(new EmptyDelegate(UpdateLog), new object[] { });
				return;
			}
			string s = writer.ToString();
			if (s.Length > 10000)
				s = s.Substring(s.Length - 10000);
			s = s.Trim();
			if (txtLog.Text != s)
				txtLog.Text = s;
		}


		void Application_ApplicationExit(object sender, EventArgs e) {
			thread.Abort();
			bot.DisconnectAll();
			Console.Out.Close();
		}


		void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
			this.Visible = false;
		}


		void txtLog_TextChanged(object sender, EventArgs e) {
			txtLog.Focus();
			txtLog.SelectionStart = txtLog.Text.Length;
			txtLog.SelectionLength = 0;
			txtLog.ScrollToCaret();
		}


		void txtMessage_KeyDown(object sender, KeyEventArgs e) {
			if (e.KeyCode == Keys.Enter) {
				bot.Networks[0].SendMessage(Murphy.Irc.SendType.Message,bot.Networks[0].Channels[0], txtMessage.Text);
				txtMessage.Text = "";
			}
		}

	}
}