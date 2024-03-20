using AE.Net.Mail;
using System;
using System.Configuration;

namespace ExtractPosData
{
    class xexatouch
    {
        static ImapClient ic;
        string MailId = "napavalleyinventory@outlook.com";
        string Password = "Lgofer12#";
        string ExaTouchDiffmail = ConfigurationManager.AppSettings["ExaTouchDiffmail"];

        public xexatouch(int storeid)
        {
            if (ExaTouchDiffmail.Contains(storeid.ToString()))
            {
                MailId = "nineandhiltonmarketinventory@outlook.com";
            }
            for (int i = 1; i < i + 1; i++)
            {
                ic = new ImapClient("outlook.office365.com", MailId, Password, AuthMethods.Login, 993, true);
                ic.SelectMailbox("INBOX");
                var Email = ic.GetMessage(0);
                if (Email.Attachments.Count>0)
                {
                    ic.DeleteMessage(Email);
                    Console.WriteLine(Email.Subject + " " + "MESSAGE DELETED FROM INBOX");
                }
                                
            }
        }
    }
}
