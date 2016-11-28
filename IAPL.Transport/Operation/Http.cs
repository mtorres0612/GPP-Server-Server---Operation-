/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Added security protocol for TLS 1.2 Compliance
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace IAPL.Transport.Operation
{
    /// <summary>
    /// Class to Transfer XML Files via HTTP
    /// </summary>
    /// <remarks>uses WebRequest . net class to send POST messages.</remarks>
    class Http
    {
        private string userName = string.Empty;
        private string password = string.Empty;
        private string httpAddress = string.Empty;
        private string errorMessage = string.Empty;

        private IAPL.Transport.Transactions.ServerDetails serverInformation = null;
        //private string errorMessage = "";

        //private IAPL.Transport.Util.ConstantVariables.FileDirection fileDirection = IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE;
        //private object transferInfo = null;

        //public FtpTransaction(string fileDirection, object transerInfo) {
        public Http(IAPL.Transport.Transactions.ServerDetails transferInfo)
        {            
            this.serverInformation = transferInfo;
        }
        public Http()
        {

        }


        #region properties

        //public string ErrorMessage
        //{
        //    get
        //    {
        //        return this.errorMessage;
        //    }
        //    set
        //    {
        //        this.errorMessage = value;
        //    }
        //}

        #endregion
        

        #region Properties
        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        public string HTTPAddress
        {
            get
            {
                return httpAddress;
            }
            set
            {
                httpAddress = value;
            }
        }

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

        #region Methods

        private static bool AcceptAllCertificatePolicy(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public bool POSTXML(string Url, string File, string UserName, string Password, string Terminator)
        {
            System.Uri.CheckHostName(Url);
            bool success = false;
            try
            {
                //Password = Password;//IAPL.Transport.Util.Utility.Decrypt(Password.Trim());
                XmlDocument _xmlText = new XmlDocument();
                //GERARD FIX
                File = File.ToLower();

                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                //if (File.Substring(File.LastIndexOf(".") + 1) == "xml")

                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                if (File.Substring(File.LastIndexOf(".") + 1).ToLower() == "xml")
                {
                    _xmlText.Load(File);
                    ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertificatePolicy;
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;//TLS 1.2 COMPLIANCE
                    WebRequest _request = WebRequest.Create((Url));
                    _request.Credentials = new NetworkCredential(UserName, Password);
                    _request.Method = "POST";
                    _request.ContentType = "text/xml";
                    //_request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0; .NET CLR 1.1.4322)";
                    
                    
                    XmlTextWriter xw = new XmlTextWriter(_request.GetRequestStream(), Encoding.UTF8);
                    _xmlText.WriteTo(xw);
                    xw.Close();
                    WebResponse rep = _request.GetResponse();
                    rep.Close();
                }

                
                success = true;
                if (success)
                {
                    IAPL.Transport.Data.DbTransaction DBTrans = new IAPL.Transport.Data.DbTransaction();
                    DBTrans.UpdateTransactionLog(true, File + "Successfully sent. ", "");
                }
            }

            catch(Exception ex)
            {
                success = false;
                this.ErrorMessage = "HTTP-Send()|" + ex.Message.ToString();
                //
            }

            return success;
         }
        #endregion

        #endregion
    }
}
