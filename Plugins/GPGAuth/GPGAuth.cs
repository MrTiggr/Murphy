#region Using directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Starksoft.Cryptography.OpenPGP;
using System.Text;
using System.Data;
using PasteBin;
using System.Xml;
using Engine;
using System.Diagnostics;
using EmailSender;
#endregion

namespace Murphy.Plugins
{
    class GPGAuth : Plugin
    {
        #region member variables

        public string BOT_CONTROL_SEQ = "#";
        private SQLiteDatabase db = new SQLiteDatabase();
        private static GnuPG gpg = new GnuPG();
        private static List<User> loggedin = new List<User>();
        private string gpgpath;
        #endregion

        #region Constructor
        public GPGAuth(Bot bot)
            : base(bot)
        {
            //Set the default pathe GPG on win7
            gpg.BinaryPath = gpgpath = Bot.Configuration["Plugins"]["GPGAuth"].Attributes["binaryPath"].Value;
            BOT_CONTROL_SEQ = Bot.Configuration["Config"].Attributes["ControlSeq"].Value;

            //Hook Up the Bot event Handlers
            Bot.OnChannelMessage += new IrcEventHandler(Bot_OnMessage);
            Bot.OnQueryMessage += new IrcEventHandler(Bot_OnMessage);
        }
        #endregion

        #region Bot Event Handlers

        /// <summary>
        /// Event that is triggered whenever a channel message is recieved by the bot
        /// </summary>
        /// <param name="n">The Network upon which the message occured</param>
        /// <param name="e">The message event and data itself</param>
        void Bot_OnMessage(Network n, Irc.IrcEventArgs e)
        {
            string mes = string.Empty;

            if (e.Data.Message.StartsWith(BOT_CONTROL_SEQ))
            {
                try
                {
                    mes = e.Data.Message.Substring(1).Split(' ')[0];
                }
                catch (Exception ex)
                {
                    Answer(n, e, "An error occurred, please try again.");
                }

                bool isadmin = Bot.isAdmin(e.Data.Nick);
                
                //Process Admin Commands
                if(isadmin)
                {
                    switch (mes)
                    {
                        case "rollcall":
                            rollcall(n, e);
                            isadmin = true;
                            break;
                    }
                }

                //Process Normal Commands
                if (!isadmin)
                {
                    switch (mes)
                    {
                        case "register":
                            register(n, e);
                            break;
                        case "eauth":
                            login(n, e, false);
                            break;
                        case "leauth":
                            login(n, e, true);
                            break;
                        case "everify":
                            loginconf(n, e);
                            break;
                        case "ident":
                            ident(n, e);
                            break;
                        case "bizident":
                            bizident(n, e);
                            break;
                        case "logout":
                            logout(n, e);
                            break;
                        case "email":
                            emailogin(n, e);
                            break;
                        case "setemail":
                            setemail(n, e);
                            break;
                        case "login":
                            help(n, e);
                            break;
                        default:
                            //Answer(n, e, "I do not know that command, " + e.Data.Nick);
                            break;
                    }
                }
            }
        }



        protected void help(Network n, Murphy.Irc.IrcEventArgs e)
        {
            AnswerWithNotice(n, e, FormatBold(String.Format("_______________________________________________________________________________________", BOT_CONTROL_SEQ)));
            AnswerWithNotice(n, e, FormatBold(String.Format("                           IDENTITY VERIFICATION COMMANDS                ", BOT_CONTROL_SEQ)));
            AnswerWithNotice(n, e, String.Format("   " + FormatBold(FormatUnderlined("Commands:")), BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}help") + " - Shows this help screen", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}register <BusinessNick> <GpGKey>") + " - registers a business nick as having the GpG key specified", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}eauth <?BusinessNick>") + " - authenticates you as the (optional) nick by creating a One-Time pawword for you to decrypt with your GpG key", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}setemail <email>") + " - sets the registered email for your logged in nick", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}email") + " - performs a pseudo-2-factor login via email", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}bizident <?name>") + " - Shows the login details for (optional) <name> or the requestor", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}logout") + " - logs your identity out of the system", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, FormatBold(String.Format("_______________________________________________________________________________________", BOT_CONTROL_SEQ)));
        }

        /// <summary>
        /// Register a Business/GpG Key in the database 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="e"></param>
        protected void register(Network n, Irc.IrcEventArgs e)
        {
            if (IsMatch("^register (?<name>.*?) (?<gpgkey>.*)$", e.Data.Message.Substring(BOT_CONTROL_SEQ.Length)))
            {
                if ((Matches["gpgkey"].Value.Length == 16) || (Matches["gpgkey"].Value.Length == 8))
                {
                    Dictionary<String, String> account = new Dictionary<string, string>();
                    account.Add("name", Matches["name"].Value);
                    account.Add("gpgkey", Matches["gpgkey"].Value);
                    db.Insert("accounts", account);
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = gpgpath + "gpg.exe";
                    startInfo.Arguments= " --keyserver pgp.mit.edu --recv-key 0x" + Matches["gpgkey"].Value;
                    Process.Start(startInfo);
                    Answer(n, e, e.Data.Nick + ": Successfully registered business: " + FormatBold(Matches["name"].Value) + " with GPG key: " + FormatBold(Matches["gpgkey"].Value));
                    Answer(n, e, FormatColor(FormatBold("PLEASE BE PATIENT WE ARE DOWNLOADING YOUR KEY FROM pgp.mit.edu KEYSERVERS. THIS MAY TAKE UP TO A MINUTE!"),IrcColor.Purple));
                   
                }
                else
                {
                    Answer(n, e, FormatBold(e.Data.Nick + ": Invalid GPG key id."));
                }
            }
            else
            {
                Answer(n, e, FormatBold(String.Format("Register Command Usage: {0}register <businessname> <gpgkey>", BOT_CONTROL_SEQ)));
            }
        }

        /// <summary>
        /// Returns a list of currently logged in users via Notify
        /// Only Logged in admins can perform this task
        /// </summary>
        /// <param name="n"></param>
        /// <param name="e"></param>
        protected void rollcall(Network n, Irc.IrcEventArgs e)
        {
            if(Bot.isLoggedIn(e.Data.Nick)){
                foreach (User user in Bot.LoggedInUsers)
                {
                    AnswerWithNotice(n, e, FormatBold(String.Format("User {0} is logged in as {1}", user.nick, user.kid)));            
                }
            }
            else{
                AnswerWithNotice(n, e, FormatBold(String.Format("RollCall can only be performed by a logged in administrator. Command Usage: {0}rollcall", BOT_CONTROL_SEQ)));            
            }
        }

        protected void emailogin(Network n, Irc.IrcEventArgs e)
        {
            string name = "";
            if (IsMatch("^emailauth (?<name>.*?)$", e.Data.Message.Substring(BOT_CONTROL_SEQ.Length)))
            {
                name = (Matches["name"].Value.Length > 16) ? Matches["name"].Value.Substring(0, 16) : Matches["name"].Value;
            }
            else
            {
                name = e.Data.Nick;
            }


            System.Data.DataTable accounts;
            String query = "SELECT id \"ID\", name \"NAME\", email \"EMAIL\", gpgkey \"key\" FROM accounts;";
            accounts = db.GetDataTable(query);
            int id = 0;
            bool noemail = true;
            foreach (DataRow account in accounts.Rows)
            {
                id++;
                if (account["NAME"] as string == name && account["EMAIL"].ToString().Length > 0)
                {
                    Answer(n, e, FormatItalic(e.Data.Nick + ": .. sending #email, please be patient.."));
                    gpg.Recipient = account["key"] as string;
                    MemoryStream unencrypted = new MemoryStream(Encoding.ASCII.GetBytes(name + ":" + DateTime.Now.Ticks + "\n"));
                    MemoryStream encrypted = new MemoryStream();
                    gpg.Encrypt(unencrypted, encrypted);
                    Dictionary<String, String> data = new Dictionary<String, String>();
                    data.Add("verify", StreamToString(unencrypted));
                    db.Update("accounts", data, String.Format("id = {0}", id));
                    GmailSender g = new GmailSender("MrTiggr@BitcoinPolice.org", "LAcIYB<4;=~zAbW{}7Tdjk,kW!Nq.~-C");
                    g.Send("MrTiggr@BitcoinPolice.org", account["EMAIL"].ToString(), "#bitcoin-police gpg login request for: " + name, StreamToString(encrypted));
                    Answer(n, e, e.Data.Nick + ": An Email has been sent to your registered email address. Decrypt the contents using your GpG Key and then send IRC message " + FormatBold("#everify <decodedctext>") + " to login.");
                    noemail = false;
                }
            }
            if (noemail)
            {
                Answer(n, e, e.Data.Nick + ": No registered email for your nick. To register an email; login normally then issue IRC command " + FormatBold("#setemail <emailaddress>"));
                Answer(n, e, FormatBold("Once you have registered an email address, you may perform a two-factor login at any time by typing #email"));
            }
        }


        protected void setemail(Network n, Irc.IrcEventArgs e)
        {
            if (Bot.isLoggedIn(e.Data.Nick))
            {
                string name = "";
                if (IsMatch("^setemail (?<email>.*?)$", e.Data.Message.Substring(BOT_CONTROL_SEQ.Length)))
                {
                    if ((Matches["email"].Value.IndexOf("@") > 1))
                    {
                        name = e.Data.Nick;
                        System.Data.DataTable accounts;
                        String query = "SELECT id \"ID\", name \"NAME\", email \"EMAIL\", gpgkey \"key\" FROM accounts;";
                        accounts = db.GetDataTable(query);

                        foreach (DataRow account in accounts.Rows)
                        {
                            if (account["NAME"] as string == name)
                            {

                                Dictionary<String, String> data = new Dictionary<String, String>();
                                data.Add("email", Matches["email"].Value);
                                db.Update("accounts", data, String.Format("id = {0}", account["ID"]));
                                GmailSender g = new GmailSender("MrTiggr@BitcoinPolice.org", "LAcIYB<4;=~zAbW{}7Tdjk,kW!Nq.~-C");
                                g.Send("MrTiggr@BitcoinPolice.org",  Matches["email"].Value, "#bitcoin-police email confirmation for: " + name, "Confirming your email address.");
                                Answer(n, e, e.Data.Nick + ": Your email address has been registered. An Email has been sent to your registered email address.");

                            }
                        }
                    }
                    else
                    {
                        Answer(n, e, e.Data.Nick + ": INVALID EMAIL ADDRESS!");
                    }
                }

            }
            else
            {
                AnswerWithNotice(n, e, FormatBold(String.Format("You may only set the email for your user if you are logged in. Command Usage: {0}setemail <email>", BOT_CONTROL_SEQ)));
            }
        }

        /// <summary>
        /// Login As a registered User.
        /// This is the first step in a two step login and returns a nonce (One-Time code) fo rthe user to decrypt with their known key.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="e"></param>
        /// <param name="wget"></param>
        protected void login(Network n, Irc.IrcEventArgs e, bool wget)
        {

            string name = "";
            if (IsMatch("^.?eauth (?<name>.*?)$", e.Data.Message.Substring(BOT_CONTROL_SEQ.Length)))
            {
                name = (Matches["name"].Value.Length > 16) ? Matches["name"].Value.Substring(0, 16) : Matches["name"].Value;
            }
            else
            {
                name = e.Data.Nick;
            }

            
            System.Data.DataTable accounts;
            String query = "SELECT id \"ID\", name \"NAME\", email \"EMAIL\", gpgkey \"key\" FROM accounts;";
            accounts = db.GetDataTable(query);
            int id = 0;
            foreach (DataRow account in accounts.Rows)
            {
                id++;
                if (account["NAME"] as string == name)
                {
                    gpg.Recipient = account["key"] as string;
                    MemoryStream unencrypted = new MemoryStream(Encoding.ASCII.GetBytes(name + ":" + DateTime.Now.Ticks + "\n"));
                    MemoryStream encrypted = new MemoryStream();
                    gpg.Encrypt(unencrypted, encrypted);


                    Dictionary<String, String> data = new Dictionary<String, String>();
                    data.Add("verify", StreamToString(unencrypted));
                    db.Update("accounts", data, String.Format("id = {0}", id));

                    Pastie p = new Pastie();
                    string pastie = p.SendViaPasteBin(StreamToString(encrypted), "#bitcoin-police gpg login request for: " + name);
                    string[] paste = pastie.Split('/');
                    if (wget)
                    {
                        Answer(n, e, e.Data.Nick + ": wget -qO " + FormatBold("http://pastebin.com/raw.php?i=" + paste[3]) + " | gpg --decrypt");
                    }
                    else
                    {
                        Answer(n, e, e.Data.Nick + ": Request here: "+ FormatBold("http://pastebin.com/raw.php?i=" + paste[3]));
                    }

                }
            }
        }

        /// <summary>
        /// Confirm identity of logging in user
        /// </summary>
        /// <param name="n"></param>
        /// <param name="e"></param>
        protected void loginconf(Network n, Irc.IrcEventArgs e)
        {
            if (IsMatch("^everify (?<nonce>.*?)$", e.Data.Message.Substring(BOT_CONTROL_SEQ.Length)))
            {
                string[] temp = Matches["nonce"].Value.Split(':');
                string name = temp[0];
                SQLiteDatabase db = new SQLiteDatabase();
                System.Data.DataTable accounts;
                String query = "SELECT id \"ID\", name \"NAME\", email \"EMAIL\", gpgkey \"key\", verify \"VERIFY\" FROM accounts;";
                accounts = db.GetDataTable(query);
                bool registered = false;
                int id = 0;
                foreach (DataRow account in accounts.Rows)
                {
                    id++;
                    if (account["NAME"] as string == name)
                    {
                        Answer(n, e, "Name from verify string:   " + name);
                        Answer(n, e, "Verify string from db:     " + account["VERIFY"].ToString());
                        Answer(n, e, "Verify string from irc:    " + Matches["nonce"].Value);

                        if (account["VERIFY"].ToString().ToLower() == Matches["nonce"].Value.ToLower() + "\n")
                        {
                            User u = new User();
                            u.nick = e.Data.Nick;
                            u.user = name;
                            u.kid = account["key"] as string;
                            loggedin.Add(u);

                            //LOGIN THE USER AT THE BOT LEVEL SO OTHER PLUGINS CAN SEE
                            Bot.LoginUser(u);

                            //JUST ANOTHER WAY TO REPLY TO THE USER
                            Answer(n, e, e.Data.Nick + ": You are now logged in.");
                            registered = true;
                        }
                        else
                        {
                            Answer(n, e, e.Data.Nick + ": Invalid login.");
                        }
                    }
                }

                if (!registered)
                {
                    Answer(n, e, e.Data.Nick + ": Your not registered.");
                }
            }
        }
        /// <summary>
        /// IDENT command help
        /// </summary>
        /// <param name="n"></param>
        /// <param name="e"></param>
        protected void ident(Network n, Irc.IrcEventArgs e)
        {
            Answer(n, e, e.Data.Nick + String.Format(": Use {0}bizident to identify a business, or ;;ident to identify a person.", BOT_CONTROL_SEQ));
        }
        /// <summary>
        /// Identify the business the user is logged in as
        /// </summary>
        /// <param name="n"></param>
        /// <param name="e"></param>
        protected void bizident(Network n, Irc.IrcEventArgs e)
        {
            string name = "";
            if (IsMatch("^bizident (?<name>.*?)$", e.Data.Message.Substring(BOT_CONTROL_SEQ.Length)))
            {
                name = (Matches["name"].Value.Length > 16) ? Matches["name"].Value.Substring(0, 16) : Matches["name"].Value;
            }
            else
            {
                name = e.Data.Nick;
            }
            bool lin = false;

            foreach (User s in loggedin)
            {
                if (s.nick == name)
                {
                    Answer(n, e, ((e.Data.Nick==name) ?  "You are logged in as " : e.Data.Nick + ":" + name + " is logged in as ") + FormatBold(s.user) +" with GpG Key of " + FormatBold(s.kid));
                    lin = true;
                }

            }
            if (!lin)
            {
                Answer(n, e, e.Data.Nick + ": you are not logged in.");
            }
        }
        /// <summary>
        /// Log the user out
        /// </summary>
        /// <param name="n"></param>
        /// <param name="e"></param>
        protected void logout(Network n, Irc.IrcEventArgs e)
        {
            int num = 0;
            bool lot = Bot.isLoggedIn(e.Data.Nick);

            if (lot)
            {
                try
                {
                    foreach (User s in loggedin)
                    {
                        if (s.nick == e.Data.Nick)
                        {
                            loggedin.RemoveAt(num);
                            Bot.LogoutUser(s);
                            Answer(n, e, e.Data.Nick + ": you are now logged out");
                            lot = true;
                        }
                        num++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Answer(n, e, e.Data.Nick + ": you are not logged in.");
            }

        }

        public static string StreamToString(MemoryStream ms)
        {
            ms.Seek(0, SeekOrigin.Begin);
            byte[] jsonBytes = new byte[ms.Length];
            ms.Read(jsonBytes, 0, (int)ms.Length);
            return Encoding.UTF8.GetString(jsonBytes);
        }

    }
        #endregion
}

