using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using Starksoft.Cryptography.OpenPGP;
using System.IO;
using System.Net;
using System.Collections.Specialized;

namespace PasteBin
{

    public class Pastie
    {
        string pastebin_login_url = "http://pastebin.com/api/api_login.php";
        string pastebin_post_url = "http://pastebin.com/api/api_post.php";
        string pastebin_dev_key = "3c2d6c8eaad4c8a1838a4c31a74af893";
        string _pastebinUserKey = null;

        public string SendViaPasteBin(string body, string subject)
        {

            if (string.IsNullOrEmpty(subject))
            {
                subject = "Note " + DateTime.Now.ToLongDateString() + ":" + DateTime.Now.ToShortTimeString();
            }

            string api_paste_format = "csharp";
            string userk = LogInToPasteBin();

            NameValueCollection values = new NameValueCollection();
            values.Add("api_dev_key", pastebin_dev_key);
            values.Add("api_option", "paste");
            values.Add("api_paste_code", body);
            values.Add("api_paste_private", "1");
            values.Add("api_paste_name", subject);
            values.Add("api_paste_expire_date", "10M");
            values.Add("api_paste_format", api_paste_format);
            values.Add("api_user_key", userk);

            using (WebClient wc = new WebClient())
            {

                byte[] respBytes = wc.UploadValues(pastebin_post_url, values);
                string respString = Encoding.UTF8.GetString(respBytes);

                Uri valid = null;
                if (Uri.TryCreate(respString, UriKind.Absolute, out valid))
                {
                    return valid.ToString();
                }
                else
                {
                    return respString;
                }
            }
        }

        string LogInToPasteBin()
        {

            if (_pastebinUserKey != null)
                return _pastebinUserKey;

            string userName = "SomeoneWeird";
            string pwd = "";

            NameValueCollection values = new NameValueCollection();
            values.Add("api_dev_key", pastebin_dev_key);
            values.Add("api_user_name", userName);
            values.Add("api_user_password", pwd);

            using (WebClient wc = new WebClient())
            {
                byte[] respBytes = wc.UploadValues(pastebin_login_url, values);
                string resp = Encoding.UTF8.GetString(respBytes);
                if (resp.Contains("Bad API request"))
                {
                    Console.Write("Error:" + resp);
                    return null;
                }
                _pastebinUserKey = resp;
            }
            return _pastebinUserKey;
        }
    }

}