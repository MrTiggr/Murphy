using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Murphy;
using System.Data;

namespace BusinessRatings
{
    /// <summary>
    /// Business Ratings and Reputation plugin. 
    /// 
    /// This plugin collects business rating information and performs a trust rating algorithm based 
    /// upon the information provided as verifying the user's identity.
    /// 
    /// </summary>
    public class Reputation : Plugin
    {

        #region member variables
        public string BOT_CONTROL_SEQ = "#";
        private SQLiteDatabase db = new SQLiteDatabase();
        #endregion

        #region Constructor

        public Reputation(Bot bot)
            : base(bot)
        {
            BOT_CONTROL_SEQ = Bot.Configuration["Config"].Attributes["ControlSeq"].Value;

            //Hook Up the Bot event Handlers
            Bot.OnChannelMessage += new IrcEventHandler(Bot_OnChannelMessage);
            Bot.OnQueryMessage += new IrcEventHandler(Bot_OnPrivateMessage);
        }
        #endregion

        #region Bot Event Handlers

        void Bot_OnChannelMessage(Network n, Murphy.Irc.IrcEventArgs e)
        {
            if (e.Data.Message.StartsWith(BOT_CONTROL_SEQ))
            {
                string mes = e.Data.Message.Substring(1).Split(' ')[0].ToLower();
                bool isadmin = false;

                //Process Admin Commands
                if (Bot.isAdmin(e.Data.Nick))
                {
                    switch (mes)
                    {
                        case "ping":

                            isadmin = true;
                            break;
                    }
                }

                //Process Normal Commands
                if (!isadmin)
                {
                    switch (mes)
                    {
                        case "help":
                            help(n, e);
                            break;
                        case "infotypes":
                            infotypes(n, e);
                            break;
                        case "set":
                            Answer(n, e, "OOPS!, " + e.Data.Nick +" you really should do that in private! (try adding /msg to the beginning)");
                            break;
                        default:

                            //Answer(n, e, "I do not know that command, " + e.Data.Nick);
                            break;
                    }
                }
            }
        }

        void Bot_OnPrivateMessage(Network n, Murphy.Irc.IrcEventArgs e)
        {
            if (e.Data.Message.StartsWith(BOT_CONTROL_SEQ))
            {
                string mes = e.Data.Message.Substring(1).Split(' ')[0].ToLower();
                bool isadmin = false;

                //Process Admin Commands
                if (Bot.isAdmin(e.Data.Nick))
                {
                    switch (mes)
                    {
                        case "ping":

                            isadmin = true;
                            break;
                    }
                }

                //Process Normal Commands
                if (!isadmin)
                {
                    switch (mes)
                    {
                        case "set":
                            validate(n, e);
                            break;
                        default:
                            //Answer(n, e, "I do not know that command, " + e.Data.Nick);
                            break;
                    }
                }
            }
        }
        #endregion

        #region command handlers

        protected void help(Network n, Murphy.Irc.IrcEventArgs e)
        {
            AnswerWithNotice(n, e, FormatBold(String.Format("_______________________________________________________________________________________", BOT_CONTROL_SEQ)));
            AnswerWithNotice(n, e, FormatBold(String.Format("                    W3LCOME TO THE BITCOIN REPUTATION MANAGEMENT SYSTEM                ", BOT_CONTROL_SEQ)));
            //AnswerWithNotice(n, e, String.Format("          ", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("   After registering your Business and GpG key with the system, you may login and manage the information used to provide a trust rating for your business.", BOT_CONTROL_SEQ));
            //AnswerWithNotice(n, e, FormatBold(String.Format("          ", BOT_CONTROL_SEQ)));
            AnswerWithNotice(n, e, String.Format("   " + FormatUnderlined("WHY REGISTER YOUR INFORMATION?"), BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("   Each piece of verifiable, identifying information that you provide is used by our system to generate an overall 'Trust Rating' for your business. The more information you provide, the more accurately the Trust Rating we calculate will represent you.", BOT_CONTROL_SEQ));
            //AnswerWithNotice(n, e, String.Format("   ", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("   Members of the Bitcoin community will be able to request your trust rating and view  a profile of your business that ONLY REVEALS which types of information you have or", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("   have not provided.  This, coupled with our Trust Rating (which also utilises infoavailable to the public such as Domain Registration) will allow users to decide their own trust ratings amongst businesses and peers.", BOT_CONTROL_SEQ));
            //AnswerWithNotice(n, e, String.Format("          ", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("   " + FormatBold(FormatUnderlined("Commands:")), BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}help") + " - Shows this help screen", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}set <INFOTYPE> <VALUE>") + " - registers a piece of info to your account of the type <INFOTYPE> ***PRIVATE MESSAGE", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}del <INFOTYPE>") + " - deletes a piece of info from your account of the type <INFOTYPE>", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}infotypes") + " - Lists all of the available information types accepted", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}profile") + " - Shows your complete profile to you via Private notice", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}profile <name>") + " - Shows the public profile for <name>", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, String.Format("    " + FormatBold("{0}rank <name>") + " - Calculates the Trust Ranking for <name>", BOT_CONTROL_SEQ));
            AnswerWithNotice(n, e, FormatBold(String.Format("_______________________________________________________________________________________", BOT_CONTROL_SEQ)));
        }

        protected void infotypes(Network n, Murphy.Irc.IrcEventArgs e)
        {
            DataTable types;
            String query = "SELECT id \"ID\", validation_type \"TYPE\", description \"DESCRIPTION\" FROM validation_type;";
            types = db.GetDataTable(query);

            AnswerWithNotice(n, e, FormatBold("____________________________ ACCEPTED INFORMATION TYPES ____________________________ "));
            AnswerWithNotice(n, e, String.Format("   <INFOTYPE>                  <DESCRIPTION>",BOT_CONTROL_SEQ));
            foreach (DataRow type in types.Rows)
            {
                AnswerWithNotice(n, e, String.Format("   " + FormatBold("{0}") + "                  {1}", type["TYPE"], type["DESCRIPTION"]));
            }
           
        }

        protected void validate(Network n, Murphy.Irc.IrcEventArgs e)
        {
            if (Bot.isLoggedIn(e.Data.Nick))
            {
                if (IsMatch("^set (?<infotype>.*?) (?<info>.*?)$", e.Data.Message.Substring(BOT_CONTROL_SEQ.Length)))
                {
                    //TODO! Check the InfoType is valid - for now accept invalis ones
                    DataRow user = getAccount(e.Data.Nick);
                    Dictionary<String, String> account = new Dictionary<string, string>();
                    account.Add("account_id", user["ID"].ToString());
                    account.Add("validation_type", Matches["infotype"].Value);
                    account.Add("validation", Matches["info"].Value);
                    db.Insert("validation", account);
                    AnswerWithNotice(n, e, String.Format("Your information for "+ FormatBold("{0}")+" was successfully set. A moderator will validate this information shortly. Your registered email will be used as a point of contact for the validation process.", Matches["infotype"].Value));
                }
                else
                {
                    AnswerWithNotice(n, e, FormatBold(String.Format("Command Usage: {0}set <INFOTYPE> <VALUE>", BOT_CONTROL_SEQ)));
                }
            }
            else
            {
                AnswerWithNotice(n, e, FormatBold(String.Format("{0}set can only be performed by a logged user. Command Usage: {0}set <INFOTYPE> <VALUE>", BOT_CONTROL_SEQ)));
            }
        }
        #endregion

        private DataRow getAccount(string nick)
        {
            System.Data.DataTable accounts;
            String query = "SELECT id \"ID\", name \"NAME\", email \"EMAIL\", gpgkey \"key\" FROM accounts;";
            accounts = db.GetDataTable(query);
            foreach (DataRow account in accounts.Rows)
            {
                if (account["NAME"] as string == nick)
                {
                    return account;
                }
            }
            return null;
        }
    }

    #region Model Classes
    /*
     * 
     * 
     * -- Describe VALIDATION_TYPE
CREATE TABLE "validation_type" (
    "ID" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    "validation_type" TEXT NOT NULL,
    "description" TEXT NOT NULL
)

     * 
         * -- Describe VALIDATION
        CREATE TABLE "validation" (
            "id" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
            "account_id" INTEGER NOT NULL,
            "validation_type" TEXT NOT NULL,
            "validation" TEXT NOT NULL,
            "verified" INTEGER NOT NULL DEFAULT (0)
        )*/
    /// <summary>
    /// A simple Atom of information assigned to an account as a form of verifying reputation/credentials
    /// </summary>
    public class Credential
    {
        private int _id;

        public int ID
        {
            get { return _id; }
            set { _id = value; }
        }
        private int _account_id;

        public int AccountID
        {
            get { return _account_id; }
            set { _account_id = value; }
        }
        private string _type;

        public string CredentialType
        {
            get { return _type; }
            set { _type = value; }
        }
        private string _value;

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private bool _verified;

        public bool IsVerified
        {
            get { return _verified; }
            set { _verified = value; }
        }
    }
    #endregion
}
