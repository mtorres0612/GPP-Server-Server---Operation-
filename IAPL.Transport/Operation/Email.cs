/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Removed dependency to Rebex
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
//using Rebex.Net;
//using Rebex.Mail;
//using Rebex.Mime.Headers;
using System.Net.Mail;
using System.Collections;

namespace IAPL.Transport.Operation
{
    class Email
    {
        #region local variables
        private string to = "";
        private string from = "";
        private string cc = "";
        private string bcc = "";
        private string subject = "";
        private MailPriority priority = MailPriority.Normal;
        private string smtpServer = "";
        private string errorMessage = "";
        #endregion

        #region constructors
        public Email(string from, string to, int priority, string smtp)
        {
            this.From = from;
            this.To = to;
            //this.Subject = subject;

            switch (priority)
            {
                case 1: this.Priority = MailPriority.High; break;
                case 2: this.Priority = MailPriority.Normal; break;
                case 3: this.Priority = MailPriority.Low; break;
            }

            this.SmtpServer = smtp;
        }

        public Email(string from, string to, string cc, string bcc, string subject, int priority, string smtp)
        {
            this.From = from;
            this.To = to;
            this.CC = cc;
            this.BCC = bcc;
            this.Subject = subject;

            switch (priority)
            {
                case 1: this.Priority = MailPriority.High; break;
                case 2: this.Priority = MailPriority.Normal; break;
                case 3: this.Priority = MailPriority.Low; break;
            }

            this.SmtpServer = smtp;
        }
        #endregion

        #region Properties
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
            }
        }

        public MailPriority Priority
        {
            get
            {
                return priority;
            }
            set
            {
                priority = value;
            }
        }

        public string To
        {
            get
            {
                return to;
            }
            set
            {
                to = value;
            }
        }

        public string CC
        {
            get
            {
                return this.cc;
            }
            set
            {
                this.cc = value;
            }
        }

        public string BCC
        {
            get
            {
                return this.bcc;
            }
            set
            {
                this.bcc = value;
            }
        }

        public string From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
            }
        }

        public string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                subject = value;
            }
        }

        public string SmtpServer
        {
            get
            {
                return smtpServer;
            }
            set
            {
                smtpServer = value;
            }
        }

        #endregion

        #region Methods

        #region send message w/ attachment and w/ subject parameter
        public bool Send(string subject, bool isHTML, string message, object attachment)
        {
            // Editted New
            ArrayList attachmentList = (ArrayList)attachment;
            this.Subject = subject;
            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient(this.SmtpServer);

            bool success = true;
            try
            {
                // and set its properties to desired values 
                mail.From = new MailAddress(this.From);
                mail.To.Add(this.To);
                if (this.CC.Trim().Length > 0)
                {
                    mail.CC.Add(this.CC);
                }

                if (this.BCC.Trim().Length > 0)
                {
                    mail.Bcc.Add(this.BCC);
                }

                mail.Subject = subject;


                //if (isHTML)
                //{
                //    mail.BodyHtml = message;
                //}
                //else {
                //    mail.BodyText = message;
                //}

                mail.Body = message;

                for (int i = 0; i < attachmentList.Count; i++)
                {
                    mail.Attachments.Add(
                        new Attachment(attachmentList[i].ToString())
                    );
                }

                IAPL.Transport.Util.TextLogger.Log("Email", "Performing email...");
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "Email-Send()|" + ex.Message.ToString();
                IAPL.Transport.Util.TextLogger.Log("Email", "Email process failed.");
            }
            if (success)
                IAPL.Transport.Util.TextLogger.Log("Email", "Email process successful.");

            return success;
        }
        #endregion

        #region send message w/ attachment
        public bool Send(bool isHTML, string message, object attachment)
        {
            ArrayList attachmentList = (ArrayList)attachment;
            MailMessage mail = new MailMessage();
            SmtpClient smtpClient = new SmtpClient(this.SmtpServer);
            bool success = true;
            try
            {
                // and set its properties to desired values 
                mail.From = new MailAddress(this.From);
                mail.To.Add(this.To);
                if (this.CC.Trim().Length > 0)
                {
                    mail.CC.Add(this.CC);
                }

                if (this.BCC.Trim().Length > 0)
                {
                    mail.Bcc.Add(this.BCC);
                }

                mail.Subject = subject;


                //if (isHTML)
                //{
                //    mail.BodyHtml = message;
                //}
                //else
                //{
                //    mail.BodyText = message;

                //}

                mail.Body = message;
                mail.IsBodyHtml = isHTML;

                if (attachmentList != null)
                {
                    for (int i = 0; i < attachmentList.Count; i++)
                    {
                        mail.Attachments.Add(
                            new Attachment(attachmentList[i].ToString())
                        );
                    }
                }

                IAPL.Transport.Util.TextLogger.Log("Email", "Performing email...");
                Console.WriteLine("Email", "Performing email... on " + this.SmtpServer);
                //Console.WriteLine(mail.BodyHtml);
                mail.Subject = this.Subject;
                smtpClient.Send(mail);
                Console.WriteLine("Done sending...");
                IAPL.Transport.Util.TextLogger.Log("Email", "Email sending complete.");
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "Email-Send()|" + ex.Message.ToString();
                IAPL.Transport.Util.TextLogger.Log("Email", "Email process failed.");
            }

            if (success)
                IAPL.Transport.Util.TextLogger.Log("Email", "Email process successful.");

            return success;
        }
        #endregion

        #endregion

    }
}
