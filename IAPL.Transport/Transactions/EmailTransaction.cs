using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
//Gerard Start
using System.Xml.Xsl;
using System.Xml;
//Gerard end
using System.Web;

namespace IAPL.Transport.Transactions
{
    class EmailTransaction
    {
        private IAPL.Transport.Transactions.ServerDetails serverInformation = null;
        private IAPL.Transport.Transactions.MessageDetails messageInformation = null;
        private string errorMessage = "";

        public string SourceFile = "";
        public string OutputFile = "";
        public string DestinationFolder = "";
        public string SourceFolder = "";

        #region constructors
        public EmailTransaction() { 
        
        }

        public EmailTransaction(IAPL.Transport.Transactions.ServerDetails transferInfo, IAPL.Transport.Transactions.MessageDetails messageInfo)
        {
            this.serverInformation = transferInfo;
            this.messageInformation = messageInfo;
        }

        public EmailTransaction(IAPL.Transport.Transactions.MessageDetails messageInfo)
        {
            this.serverInformation = new ServerDetails();
            this.messageInformation = messageInfo;
        }

        #endregion

        #region properties
        public string ErrorMessage {
            get { 
                return this.errorMessage;
            }
            set {
                this.errorMessage = value;
            }
        }

        #endregion


        private bool _IMSNoFilesFound = false;
        public bool IMSNoFilesFound
        {
            get { return _IMSNoFilesFound; }
            set { this._IMSNoFilesFound = value; }
        }



        #region methods

        #region set processlog id
        public void SetProcessLogID(string prlgID) {
            messageInformation.ProcessLogID = prlgID;
        }
        #endregion

        #region SendEmail
        private void SendEmail(object data)
        {
            ArrayList list = (ArrayList)data;
            string to = (string)list[0];
            string from = (string)list[1];
            string subject = (string)list[2];
            string smtpServer = (string)list[3];
            string message = (string)list[4];
            string threadName = (string)list[5];

            //System.Console.WriteLine("Thread {0} is running ", threadName);
            IAPL.Transport.Operation.Email email = new IAPL.Transport.Operation.Email(from, to, 2, smtpServer);
            email.Send(subject, false, message, null);
        }
        #endregion

        #region getEmailDistributionListSendAttachment
        private bool getEmailDistributionListSendAttachment(bool isSuccessful, string fileName, string threadName, string desFilePath)
        {
            bool success = true;

            IAPL.Transport.Transactions.ServerDetails emailDetails = new ServerDetails();

            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            db.emailDistributionList(false, messageInformation.MessageCode, messageInformation.ERP, 0);

            System.Data.DataTable dTable = db.CommonDataTable;
            string emailSMTP = IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationsmtp", "");

            if (dTable != null)
            {
                foreach (System.Data.DataRow row in dTable.Rows)
                {
                    //IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationsender", "")

                    IAPL.Transport.Operation.Email email = new IAPL.Transport.Operation.Email(
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrFROM"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrTO"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrCC"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrBCC"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldEmailSubject"]),
                        2,
                        emailSMTP);

                    string mesgBody = this.readNoticationMessageBody(fileName, isSuccessful,
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldXSLTPath"]));

                    ArrayList list = new ArrayList();

                    fileName = this.serverInformation.GetNetworkSourceFile(desFilePath, fileName);
                    list.Add(fileName);

                    // temporarily set to save as TEXT for the sake of testing....
                    //success = email.Send(true, mesgBody, list);

                    success = email.Send(false, mesgBody, list);
                    if (!success)
                    {
                        this.ErrorMessage = email.ErrorMessage;
                    }

                    //emailDetails.EmailSubject = row["emldEmailSubject"].ToString();

                }
            }
            else
            {
                success = false;
            }

            //return emailDetails;
            return success;
        }
        #endregion


        #region Get Email Distribution List for IMS
//        private bool getEmailDistributionListSendAttachment(bool isSuccessful, string fileName, string threadName, string desFilePath)        
        private bool getIMSEmailDistributionList(int Report,
                                                 string fileName,
                                                 string desFilePath,
                                                 string ThreadName, 
                                                 System.Collections.Hashtable DetailsForIMS,
                                                 IAPL.Transport.Transactions.MessageDetails mDetails)
        {
            bool success = true;

            IAPL.Transport.Transactions.ServerDetails emailDetails = new ServerDetails();

            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            db.emailDistributionList(true, mDetails.MessageCode, mDetails.ERP, Report);

            System.Data.DataTable dTable = db.CommonDataTable;
            string emailSMTP = IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationsmtp", "");

            if (dTable != null)
            {
                foreach (System.Data.DataRow row in dTable.Rows)
                {
                    IAPL.Transport.Operation.Email email = new IAPL.Transport.Operation.Email(
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["IMSemldIntEmailAddrFROM"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["IMSemldIntEmailAddrTO"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["IMSemldIntEmailAddrCC"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["IMSemldIntEmailAddrBCC"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["IMSemldEmailSubject"]),
                        2,
                        emailSMTP);

                    string mesgBody = this.IMSreadNoticationMessageBody(Report, success,
                                                                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["IMSemldXSLTPath"]),
                                                                        DetailsForIMS,
                                                                        mDetails.IMSBatchRun);

                    // Email this!
                    //success = email.Send(true, mesgBody, list);
                    success = email.Send(false, mesgBody, null);
                    if (!success) { this.ErrorMessage = email.ErrorMessage; }                    
                }
            }
            else
            {
                success = false;
            }

            //return emailDetails;
            return success;
        }
        #endregion


        #region StartProcess (send file as attachment to email as destination)
        public bool StartProcess(string fileName, string threadName, string desFilePath) {
            bool success = true;

            success = getEmailDistributionListSendAttachment(true, fileName, threadName, desFilePath);
            
            return success;
        }
        #endregion

        #region Send Email for IMS Reports
        public bool StartProcessIMS(int Report, 
                                    string fileName,
                                    string desFilePath,
                                    string ThreadName, 
                                    System.Collections.Hashtable DetailsForIMS,
                                    IAPL.Transport.Transactions.MessageDetails mDetails)
        {
            bool success = true;
            success = getIMSEmailDistributionList(Report, 
                                                  fileName,
                                                  desFilePath, 
                                                  ThreadName, 
                                                  DetailsForIMS,
                                                  mDetails);
            return success;
        }
        #endregion




        #region getEmailDistributionListEmailNotification
        private bool getEmailDistributionListEmailNotification(bool isSuccessful, string fileName, string threadName)
        {
            bool success = true;

            IAPL.Transport.Transactions.ServerDetails emailDetails = new ServerDetails();

            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            if (!isSuccessful)
            {
                 db.emailDistributionList(false,IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationmessagecode", "TECHNICAL_NOTIFICATION"),
                    IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationerp", "IAPL"),0);
            }
            else
            {
                db.emailDistributionList(false,messageInformation.MessageCode, messageInformation.ERP,0);
            }

            System.Data.DataTable dTable = db.CommonDataTable;
            string emailSMTP = IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationsmtp", "");

            if (dTable != null)
            {
                foreach (System.Data.DataRow row in dTable.Rows)
                {
                    //IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationsender", "")

                    IAPL.Transport.Operation.Email email = new IAPL.Transport.Operation.Email(
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrFROM"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrTO"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrCC"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrBCC"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldEmailSubject"]),
                        2,
                        emailSMTP);

                    string mesgBody = this.readNoticationMessageBody(fileName, isSuccessful,
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldXSLTPath"]));

                    ArrayList list = new ArrayList();

                    //fileName = this.serverInformation.GetNetworkSourceFile(desFilePath, fileName);
                    //list.Add(fileName);

                    IAPL.Transport.Util.TextLogger.LogSetting("EmailContent [Subject: " + email.Subject + "]", mesgBody);

                    //IAPL.Transport.Util.TextLogger.Log("-->EmailContent [Subject: " + email.Subject + "]", mesgBody);

                    success = email.Send(true, mesgBody, list);
                    if (!success)
                    {
                        this.ErrorMessage = email.ErrorMessage + " (" + email.SmtpServer + ")";
                    }
                    //emailDetails.EmailSubject = row["emldEmailSubject"].ToString();
                }
            }
            else {
                success = false;
            }

            //return emailDetails;
            return success;
        }
        #endregion

        #region emailNotification
        public bool SendEmailNotification(string fileName, string threadName, bool isSuccessful)
        {
            bool success = true;

            success = getEmailDistributionListEmailNotification(isSuccessful, fileName, threadName);
       
            return success;
        }
        #endregion

        #region readMessageBody
        private string readMessageBody( string emailContent, string fileName) {
            StringBuilder mesgBody = new StringBuilder();

            //mesgBody = this.readXSLTFile(xsltFile);
            mesgBody.Append(emailContent);

            mesgBody.Replace("$DateTime", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
            mesgBody.Replace("$InterchangeNo", "");
            mesgBody.Replace("$DocumentNo", this.serverInformation.FileCounter.ToString());
            mesgBody.Replace("$OutputFileName", fileName);
            mesgBody.Replace("$DestinationFolder", "");
            mesgBody.Replace("$SupplierName", "");
            mesgBody.Replace("$ERPID", this.messageInformation.ERP);
            mesgBody.Replace("$Subject", this.serverInformation.EmailSubject);
            mesgBody.Replace("$Environment", "");
            //Gerard start
            //return convertXSLTToHTML(mesgBody.ToString());            
            //Gerard end
            return mesgBody.ToString();
        }
        #endregion

        //Gerard start
        #region ConvertXSLT To HTML
        private string convertXSLTToHTML(string xsltSource, string xmlString)
        {
            XmlDocument _xmlOutput = new XmlDocument();
            this.ErrorMessage = "";
            try
            {
                #region old
                XslTransform objxslt = new XslTransform();
                //System.Xml.Xsl.XslCompiledTransform objxslt = new XslCompiledTransform();
                //XslCompiledTransform objxslt = new XslCompiledTransform();
                objxslt.Load(xsltSource);
                //objxslt.Load(templatepath + "employee.xslt"); //xslttemplatename);
                XmlDocument xmldoc = new XmlDocument();
                //xmldoc.AppendChild(xmldoc.CreateElement("DocumentRoot"));
                xmldoc.LoadXml(xmlString);
                XmlReader _xmlReader;
                _xmlReader = objxslt.Transform(xmldoc, new XsltArgumentList());
                _xmlOutput.Load(_xmlReader);
                #endregion
                //XslCompiledTransform objxslt = new XslCompiledTransform();
                //XmlDocument xmldoc = new XmlDocument();
                //xmldoc.LoadXml(xmlString);
                //using (XmlWriter writer = _xmlOutput.CreateNavigator().AppendChild())//doc.CreateNavigator().AppendChild())
                //{
                //    objxslt.Transform(xmldoc, (XsltArgumentList)null, writer);
                //}
              }
            catch (Exception ex) {
                this.ErrorMessage = "EmailTransaction-convertXSLTToHTML()|" + ex.Message.ToString();
            }

            return _xmlOutput.OuterXml;            
        }


        #endregion
        //Gerard end

        private string returnHtmlData(string mesg) {

            return System.Web.HttpUtility.HtmlEncode(mesg);
        }

        #region readNoticationMessageBody
        //OLD CODE SR#33117 Ccenriquez / Capad -- November 5, 2009
        //private string readNoticationMessageBody(string fileName, bool isSuccessful, string xsltFile)
        
        //NEW CODE SR#33117 Ccenriquez / Capad -- November 5, 2009
        public string readNoticationMessageBody(string fileName, bool isSuccessful, string xsltFile)
        {
            string result = "";

            IAPL.Transport.Util.XmlData xmlData = new IAPL.Transport.Util.XmlData();
            this.ErrorMessage = "";
            try
            {
                xmlData.CreateMainTag("NewDataSet");
                xmlData.AddElement("NewDataSet", "Table", "");
                //xmlData.CreateMainTag("Table");

                xmlData.AddElement("NewDataSet", "Subject", this.returnHtmlData( this.serverInformation.EmailSubject));

                //OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                //xmlData.AddElement("NewDataSet", "DateTime", this.returnHtmlData(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")));

                //NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                xmlData.AddElement("NewDataSet", "DateTime", this.returnHtmlData(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")));

                xmlData.AddElement("NewDataSet", "Environment", this.returnHtmlData(IAPL.Transport.Configuration.Config.GetAppSettingsValue("environment", "development")));
                xmlData.AddElement("NewDataSet", "OutputFileName", this.returnHtmlData(this.OutputFile));
                xmlData.AddElement("NewDataSet", "DestinationFolder", this.returnHtmlData(this.DestinationFolder));
                xmlData.AddElement("NewDataSet", "SupplierName", this.returnHtmlData(messageInformation.SupplierName));
                xmlData.AddElement("NewDataSet", "SourceFileName", this.returnHtmlData(this.SourceFile));
                xmlData.AddElement("NewDataSet", "SourceFolder", this.returnHtmlData(this.SourceFolder));
                xmlData.AddElement("NewDataSet", "ERPID", this.returnHtmlData(this.messageInformation.ERP));

                if (!isSuccessful)
                {
                    xmlData.AddElement("Table", "ProcesslogID", this.returnHtmlData(this.messageInformation.ProcessLogID));
                    xmlData.AddElement("Table", "CustomerID", "");
                    xmlData.AddElement("Table", "prlgProcessSource", "");
                    xmlData.AddElement("Table", "prlgStartDate", this.returnHtmlData(this.messageInformation.StartDate));
                    xmlData.AddElement("Table", "prlgEndDate", this.returnHtmlData(this.messageInformation.EndDate));
                    xmlData.AddElement("Table", "ERPID", this.returnHtmlData(this.messageInformation.ERP));

                    if (messageInformation.TechnicalErrorDescription.Trim().Length > 0)
                    {
                        xmlData.AddElement("Table", "prlgIsSuccess", "0");
                    }
                    else
                    {
                        xmlData.AddElement("Table", "prlgIsSuccess", "1");
                    }
                    xmlData.AddElement("Table", "apluCode", this.returnHtmlData(this.messageInformation.ApplicationCode));
                    xmlData.AddElement("Table", "prlgDescription", this.returnHtmlData(this.messageInformation.TransDescription));
                    xmlData.AddElement("Table", "prlgTechnicalErrDesc", this.returnHtmlData(this.messageInformation.TechnicalErrorDescription));
                    xmlData.AddElement("Table", "prlgSourceParentCount", "");
                    xmlData.AddElement("Table", "prlgSourceParent", "");
                    xmlData.AddElement("Table", "prlgSourceChild", "0");
                    xmlData.AddElement("Table", "prlgSourceChildCount", "0");
                    xmlData.AddElement("Table", "prlgAddDate", this.returnHtmlData(this.messageInformation.StartDate));
                }

                result = convertXSLTToHTML(xsltFile, xmlData.GetXmlData);
            }
            catch (Exception ex) {
                result = "";
                this.ErrorMessage = ex.Message.ToString();
            }           

            return result;
        }
        #endregion


        private IAPL.Transport.Util.XmlData IMSRepBuildXML(int Report, 
                                                           IAPL.Transport.Util.XmlData xmlData,
                                                           IAPL.Transport.IMS.Process.IMSReportDetails IMS)
        {
            xmlData.AddElement("NewDataSet", "Details", string.Empty);
            xmlData.AddElement("Details", "Country", this.returnHtmlData(IMS.Country));
            xmlData.AddElement("Details", "CountryCode", this.returnHtmlData(IMS.CountryCode));
            xmlData.AddElement("Details", "IMSDataSeq", this.returnHtmlData(IMS.IMSDataSeq));
            
            if (IMS.SizeKB.Equals(0))
                xmlData.AddElement("Details", "SizeKB", "");
            else
                xmlData.AddElement("Details", "SizeKB", this.returnHtmlData(Convert.ToDouble(IMS.SizeKB).ToString()));

            if (!IMSNoFilesFound)
            {
                if (!IMS.IMSDataSeq.Equals(string.Empty))
                {
                    DateTime dt = IMS.DateSent;
                    xmlData.AddElement("Details", "ISGDateSent", this.returnHtmlData(dt.ToString("MMMM dd, yyyy")));
                }
                else
                {
                    xmlData.AddElement("Details", "ISGDateSent", "");
                }
            }
            else
            {
                xmlData.AddElement("Details", "ISGDateSent", "");
            }

            switch (Report)
            {
                case 1:         // Exception Report summary
                    xmlData.AddElement("Details", "SendStatus", this.returnHtmlData(Convert.ToString(IMS.SendStatus)));
                    xmlData.AddElement("Details", "Issue", this.returnHtmlData(IMS.ERS_Issue));
                    xmlData.AddElement("Details", "Resolution", "");
                    xmlData.AddElement("Details", "IMSSingaporeComment", "");
                    xmlData.AddElement("Details", "ISGComment", "");
                    break;

                case 2:
                    xmlData.AddElement("Details", "Issue", this.returnHtmlData(IMS.IMSSR_Issue));
                    xmlData.AddElement("Details", "CommentResolution", this.returnHtmlData(IMS.CommentResolution));
                    xmlData.AddElement("Details", "DataFileRcvd", this.returnHtmlData(Convert.ToString(IMS.DataFileRecvd)));
                    xmlData.AddElement("Details", "TransactionDate", this.returnHtmlData(Convert.ToString(IMS.TransactionDate)));

                    if (IMS.SizeKB.Equals(0))
                        xmlData.AddElement("Details", "RecordCount", "");
                    else
                        xmlData.AddElement("Details", "RecordCount", this.returnHtmlData(Convert.ToString(IMS.RecordCount)));

                    if (IMS.SizeKB.Equals(0))
                        xmlData.AddElement("Details", "IMSExtractionValue", "");
                    else
                        xmlData.AddElement("Details", "IMSExtractionValue", this.returnHtmlData(Convert.ToString(IMS.IMSExtractionValue)));

                    break;
            }
            return xmlData;
        }


        #region IMSreadNoticationMessageBody
        private string IMSreadNoticationMessageBody(int Report, bool isSuccessful, string xsltFile, System.Collections.Hashtable DetailsForIMS, bool IMSBatchRun)
        {
            string result = "";

            IAPL.Transport.Util.XmlData xmlData = new IAPL.Transport.Util.XmlData();
            this.ErrorMessage = "";
            try
            {
                xmlData.CreateMainTag("NewDataSet");
                DateTime DataStatusReportDate = System.DateTime.Now;
                xmlData.AddElement("NewDataSet", "IMSDataStatusReportDate", this.returnHtmlData(DataStatusReportDate.ToString("MMMM dd, yyyy") + " (" + DataStatusReportDate.DayOfWeek + ")"));

                int ctr = 0;
                //do sorting

                //DetailsForIMS = (Hashtable)IAPL.Transport.Util.Utility.SortedHashTable(DetailsForIMS);
                foreach (DictionaryEntry IMSData in (Hashtable)DetailsForIMS)
                {
                    ctr++;
                    IAPL.Transport.IMS.Process.IMSReportDetails IMS = (IAPL.Transport.IMS.Process.IMSReportDetails)DetailsForIMS[IMSData.Key.ToString()];

                    switch (Report)
                    {
                        case 1:         // Exception Report summary
                            if (IMSBatchRun)
                                xmlData = IMSRepBuildXML(Report, xmlData, IMS);
                            else
                            {
                                if (!IMS.IMSDataSeq.Equals(string.Empty))
                                    xmlData = IMSRepBuildXML(Report, xmlData, IMS);
                            }
                            break;

                        case 2:         // IMS Summary Report
                            if (IMSBatchRun)
                                xmlData = IMSRepBuildXML(Report, xmlData, IMS);
                            else
                            {
                                if (!IMS.IMSDataSeq.Equals(string.Empty))
                                    xmlData = IMSRepBuildXML(Report, xmlData, IMS);
                            }
                            break;
                    }
                }

                if (System.IO.File.Exists(xsltFile))
                {
                    result = convertXSLTToHTML(xsltFile, xmlData.GetXmlData);
                }
                else
                {
                    result = "";
                    this.ErrorMessage = xsltFile + " not found";
                }
                
            }
            catch (Exception ex)
            {
                result = "";
                this.ErrorMessage = ex.Message.ToString();
            }

            return result;
        }
        #endregion

        #region readXSLTFile
        private StringBuilder readXSLTFile(string fileName)
        {
            StringBuilder mesgBody = new StringBuilder();

            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    //This allows you to do one Read operation.
                    //Console.WriteLine(sr.ReadToEnd());
                    mesgBody.Append(sr.ReadToEnd());
                }

            }
            catch (Exception ex) {
                this.ErrorMessage = ex.Message.ToString();
            }

            return mesgBody;
        }
        #endregion

        #region Recurring Table
        public bool HTTPSendingFailed(string Msgcode, string Prncpal, string Filename)
        {
            Data.DbTransaction _db = new IAPL.Transport.Data.DbTransaction();
            return _db.CheckIFHTTPSendingFailed(Msgcode, Prncpal, Filename);

        }
        public bool GenerateHTML(List<string> Sent, List<string> Failed, string TempFolderEmail, string XSLTFile, ServerDetails sDetails, MessageDetails mDetails, List<SentDetail_BO> filesSent, bool isTechnicalError)
        {
            //Demo purposes
            
            //Preparation Code
            //1. Create Guid folder

            string _tempGUID = IAPL.Transport.Util.CommonTools.CreateGuidFolder(TempFolderEmail) + "\\";
            string _fileName = Path.GetFileName(XSLTFile);
            string _workingFile = _tempGUID + _fileName;
            //2. copy XSLTFile to Guid
            File.Copy(XSLTFile, _tempGUID + _fileName);

            //3. add sent files to file
            
            
            //Add the sent stuff
            #region SENT

            StringBuilder _sbHeader = new StringBuilder();
            StringBuilder _sbBody = new StringBuilder();

            //Sent Files
            for (int i = 0; i < Sent.Count; i++)
            {

                string a = Sent[i].ToString();
                string b = filesSent[i].FileName;
                string c = filesSent[i].DateSent;

                _sbHeader.AppendLine(AddEntry("ItemEntry", i, true, filesSent[i].DateSent, "SentFiles"));
                _sbBody.AppendLine(AddEntry("ItemEntry", i, false, filesSent[i].DateSent, "SentFiles"));
                //MessageBox.Show(_list[i]);
            }

            //READY THE LOOP FOR PENDING FILES
            for (int i = 0; i < Failed.Count; i++)
            {
                _sbHeader.AppendLine(AddEntry("PendingEntry", i, true, filesSent[i].DateSent, "PendingFiles"));
                _sbBody.AppendLine(AddEntry("PendingEntry", i, false, filesSent[i].DateSent, "PendingFiles"));
            }

            
            //XSLT FOR CLIENT HAS BEEN CREATED
            bool success = AppendToFile(_workingFile, "ItemEntry", _sbHeader, _tempGUID + "BBraunClient.xslt", _sbBody, true);
            //CREATE THE HTML EMAIL FOR THE CLIENT
            string toSend = CreateHTMLEmailForClient(false, _tempGUID + "BBraunClient.xslt", Sent, sDetails, mDetails, Failed);
            




            //SEND EMAIL
            if (toSend != "")
            {
                //Successful mailing
                Operation.Email _ToMail = new IAPL.Transport.Operation.Email(sDetails.EmailAddressFrom, sDetails.EmailAddressTo, sDetails.EmailAddressCC, sDetails.EmailAddressBCC, sDetails.EmailSubject, 3, Configuration.Config.GetAppSettingsValue("emailnotificationsmtp", ""));
                _ToMail.Send(true, toSend, null);

            }

            
          
            

            #endregion

            //Clean up
            Directory.Delete(_tempGUID, true);
           

            return true;
           
        }

        //REQUEUE OVERLOAD
        public bool GenerateHTML(List<string> Sent, List<string> Failed, string TempFolderEmail, string XSLTFile, List<SentDetail_BO> filesSent, HTTPProcessObject _httpProcess, bool isTechnicalError)
        {
            //Demo purposes

            //Preparation Code
            //1. Create Guid folder

            string _tempGUID = IAPL.Transport.Util.CommonTools.CreateGuidFolder(TempFolderEmail) + "\\";
            string _fileName = Path.GetFileName(XSLTFile);
            string _workingFile = _tempGUID + _fileName;
            //2. copy XSLTFile to Guid
            File.Copy(XSLTFile, _tempGUID + _fileName);

            //3. add sent files to file


            //Add the sent stuff
            #region SENT

            StringBuilder _sbHeader = new StringBuilder();
            StringBuilder _sbBody = new StringBuilder();

            //Sent Files
            for (int i = 0; i < Sent.Count; i++)
            {

                string a = Sent[i].ToString();
                string b = filesSent[i].FileName;
                string c = filesSent[i].DateSent;

                _sbHeader.AppendLine(AddEntry("ItemEntry", i, true, filesSent[i].DateSent, "SentFiles"));
                _sbBody.AppendLine(AddEntry("ItemEntry", i, false, filesSent[i].DateSent, "SentFiles"));
                //MessageBox.Show(_list[i]);
            }

            //READY THE LOOP FOR PENDING FILES
            for (int i = 0; i < Failed.Count; i++)
            {
                _sbHeader.AppendLine(AddEntry("PendingEntry", i, true, filesSent[i].DateSent, "PendingFiles"));
                _sbBody.AppendLine(AddEntry("PendingEntry", i, false, filesSent[i].DateSent, "PendingFiles"));
            }

            //if (isTechnicalError)
            //{
            //    string a = sDetails.ErrorMessage;
            //    string b = "";
            //}

            //XSLT FOR CLIENT HAS BEEN CREATED
            bool success = AppendToFile(_workingFile, "ItemEntry", _sbHeader, _tempGUID + "BBraunClient.xslt", _sbBody, true);
            //CREATE THE HTML EMAIL FOR THE CLIENT
            string toSend = CreateHTMLEmailForClient(false, _tempGUID + "BBraunClient.xslt", Sent,Failed, _httpProcess);





            //SEND EMAIL
            if (toSend != "")
            {
                //Successful mailing
                Operation.Email _ToMail = new IAPL.Transport.Operation.Email(_httpProcess.EmailFrom, _httpProcess.EmailTo,_httpProcess.EmailCC, _httpProcess.EmailBCC, _httpProcess.EmailSubject, 3, Configuration.Config.GetAppSettingsValue("emailnotificationsmtp", ""));
               // _ToMail.Send(true, toSend, null);

            }





            #endregion

            //Clean up
            Directory.Delete(_tempGUID, true);


            return true;

        }

        private string CreateHTMLEmailForClient(bool isSuccessful, string xsltFile, List<string> Sent, ServerDetails srvDetails, MessageDetails msgDetails, List<string> Pending)
        {
            string result = "";

            IAPL.Transport.Util.XmlData xmlData = new IAPL.Transport.Util.XmlData();
            // this.ErrorMessage = "";
            try
            {
                int _total = Sent.Count + Pending.Count;
                xmlData.CreateMainTag("NewDataSet");
                xmlData.AddElement("NewDataSet", "Table", "");
                //xmlData.CreateMainTag("Table");

                xmlData.AddElement("NewDataSet", "Subject", this.returnHtmlData(srvDetails.EmailSubject));//this.serverInformation.EmailSubject));
                xmlData.AddElement("NewDataSet", "DateTime", this.returnHtmlData(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")));
                xmlData.AddElement("NewDataSet", "SupplierName", this.returnHtmlData(msgDetails.SupplierName));//messageInformation.SupplierName));
                xmlData.AddElement("NewDataSet", "SourceFileName", this.returnHtmlData(Path.GetFileName(xsltFile)));//this.SourceFile));
                xmlData.AddElement("NewDataSet", "SourceFolder", this.returnHtmlData(msgDetails.SourceFolder));
                xmlData.AddElement("NewDataSet", "ERPID", this.returnHtmlData(msgDetails.ERP));//this.messageInformation.ERP));
                xmlData.AddElement("NewDataSet", "DestinationURL", this.returnHtmlData(srvDetails.ServerAddress));//this.messageInformation.ERP));
                xmlData.AddElement("NewDataSet", "Total", this.returnHtmlData(Sent.Count + " / " + _total ));//this.messageInformation.ERP));

                for (int i = 1; i < Sent.Count + 1; i++)
                {
                    xmlData.AddElement("NewDataSet", "ItemEntry" + (i - 1), this.returnHtmlData(Sent[i - 1].ToString()));//this.messageInformation.ERP));
                }

                for (int i = 1; i < Pending.Count + 1; i++)
                {
                    xmlData.AddElement("NewDataSet", "PendingEntry" + (i - 1), this.returnHtmlData(Sent[i - 1].ToString()));//this.messageInformation.ERP));
                }
                if (!isSuccessful)
                {
                    xmlData.AddElement("Table", "ProcesslogID", this.returnHtmlData("ProcessLOGID"));//this.messageInformation.ProcessLogID));
                    xmlData.AddElement("Table", "CustomerID", "");
                    xmlData.AddElement("Table", "prlgProcessSource", "");
                    xmlData.AddElement("Table", "prlgStartDate", this.returnHtmlData("DateToday"));//this.messageInformation.StartDate));
                    xmlData.AddElement("Table", "prlgEndDate", this.returnHtmlData("END DATE"));//this.messageInformation.EndDate));
                    xmlData.AddElement("Table", "ERPID", this.returnHtmlData("ANOTHER ERPID"));//this.messageInformation.ERP));


                    xmlData.AddElement("Table", "prlgIsSuccess", "0");

                    xmlData.AddElement("Table", "apluCode", this.returnHtmlData("APLU CODE"));//this.messageInformation.ApplicationCode));
                    xmlData.AddElement("Table", "prlgDescription", this.returnHtmlData("Description"));//this.messageInformation.TransDescription));
                    xmlData.AddElement("Table", "prlgTechnicalErrDesc", this.returnHtmlData("Technical Error Desc"));//this.messageInformation.TechnicalErrorDescription));
                    xmlData.AddElement("Table", "prlgSourceParentCount", "");
                    xmlData.AddElement("Table", "prlgSourceParent", "");
                    xmlData.AddElement("Table", "prlgSourceChild", "0");
                    xmlData.AddElement("Table", "prlgSourceChildCount", "0");
                    xmlData.AddElement("Table", "prlgAddDate", this.returnHtmlData("ADD DATE"));//this.messageInformation.StartDate));
                }

                result = convertXSLTToHTML(xsltFile, xmlData.GetXmlData);
            }
            catch (Exception ex)
            {
                string _result = ex.Message;
                Console.WriteLine("CreateHTMLEmailForClient: " + ex.Message);
                //this.ErrorMessage = ex.Message.ToString();
            }

            return result;
        }

        //REQUEUE OVERLOAD
        private string CreateHTMLEmailForClient(bool isSuccessful, string xsltFile, List<string> Sent, List<string> Pending, HTTPProcessObject _httpProcess)
        {
            string result = "";

            IAPL.Transport.Util.XmlData xmlData = new IAPL.Transport.Util.XmlData();
            // this.ErrorMessage = "";
            try
            {
                int _total = Sent.Count + Pending.Count;
                xmlData.CreateMainTag("NewDataSet");
                xmlData.AddElement("NewDataSet", "Table", "");
                //xmlData.CreateMainTag("Table");

                xmlData.AddElement("NewDataSet", "Subject", this.returnHtmlData(_httpProcess.EmailSubject));//this.serverInformation.EmailSubject));
                xmlData.AddElement("NewDataSet", "DateTime", this.returnHtmlData(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")));
                xmlData.AddElement("NewDataSet", "SupplierName", this.returnHtmlData(_httpProcess.SupplierName));//messageInformation.SupplierName));
                xmlData.AddElement("NewDataSet", "SourceFileName", this.returnHtmlData(Path.GetFileName(xsltFile)));//this.SourceFile));
                xmlData.AddElement("NewDataSet", "SourceFolder", this.returnHtmlData(_httpProcess.Guid));
                xmlData.AddElement("NewDataSet", "ERPID", this.returnHtmlData(_httpProcess.ERP));//this.messageInformation.ERP));
                xmlData.AddElement("NewDataSet", "DestinationURL", this.returnHtmlData(_httpProcess.URL));//this.messageInformation.ERP));
                xmlData.AddElement("NewDataSet", "Total", this.returnHtmlData(Sent.Count + " / " + _total));//this.messageInformation.ERP));

                for (int i = 1; i < Sent.Count + 1; i++)
                {
                    xmlData.AddElement("NewDataSet", "ItemEntry" + (i - 1), this.returnHtmlData(Sent[i - 1].ToString()));//this.messageInformation.ERP));
                }

                for (int i = 1; i < Pending.Count + 1; i++)
                {
                    xmlData.AddElement("NewDataSet", "PendingEntry" + (i - 1), this.returnHtmlData(Sent[i - 1].ToString()));//this.messageInformation.ERP));
                }
                if (!isSuccessful)
                {
                    xmlData.AddElement("Table", "ProcesslogID", this.returnHtmlData("ProcessLOGID"));//this.messageInformation.ProcessLogID));
                    xmlData.AddElement("Table", "CustomerID", "");
                    xmlData.AddElement("Table", "prlgProcessSource", "");
                    xmlData.AddElement("Table", "prlgStartDate", this.returnHtmlData("DateToday"));//this.messageInformation.StartDate));
                    xmlData.AddElement("Table", "prlgEndDate", this.returnHtmlData("END DATE"));//this.messageInformation.EndDate));
                    xmlData.AddElement("Table", "ERPID", this.returnHtmlData("ANOTHER ERPID"));//this.messageInformation.ERP));


                    xmlData.AddElement("Table", "prlgIsSuccess", "0");

                    xmlData.AddElement("Table", "apluCode", this.returnHtmlData("APLU CODE"));//this.messageInformation.ApplicationCode));
                    xmlData.AddElement("Table", "prlgDescription", this.returnHtmlData("Description"));//this.messageInformation.TransDescription));
                    xmlData.AddElement("Table", "prlgTechnicalErrDesc", this.returnHtmlData("Technical Error Desc"));//this.messageInformation.TechnicalErrorDescription));
                    xmlData.AddElement("Table", "prlgSourceParentCount", "");
                    xmlData.AddElement("Table", "prlgSourceParent", "");
                    xmlData.AddElement("Table", "prlgSourceChild", "0");
                    xmlData.AddElement("Table", "prlgSourceChildCount", "0");
                    xmlData.AddElement("Table", "prlgAddDate", this.returnHtmlData("ADD DATE"));//this.messageInformation.StartDate));
                }

                result = convertXSLTToHTML(xsltFile, xmlData.GetXmlData);
            }
            catch (Exception ex)
            {
                string _result = ex.Message;
                Console.WriteLine("CreateHTMLEmailForClient: " + ex.Message);
                //this.ErrorMessage = ex.Message.ToString();
            }

            return result;
        }
        private string AddEntry(string param, int _Count, bool isHeader, string DateSent, string Status)
        {
            string _return = string.Empty;
            if (isHeader)
            {
                //Param = Stylesheet parameter, Count is identifier
                return _return = "<xsl:param name='" + param + _Count + "' />";
            }
            else
            {
                
                StringBuilder _sb = new StringBuilder();
                
                
                

 _sb.AppendLine("<tr>");
_sb.AppendLine("<td bgcolor='#E8EEF7' style='background:#E8EEF7;padding:.75pt .75pt .75pt .75pt'>");
 _sb.AppendLine("<p class='MsoNormal'>");
 _sb.AppendLine("<font size='1' face='Arial'>");
 _sb.AppendLine("<span style='font-size:9.0pt;");
 _sb.AppendLine("font-family:Arial'>");
_sb.AppendLine("<xsl:value-of select='" + param + _Count + "' />");
//_sb.AppendLine("<o:p></o:p>");
_sb.AppendLine("</span>");
_sb.AppendLine("</font>");
_sb.AppendLine("</p>");
_sb.AppendLine("</td>");
_sb.AppendLine("<td bgcolor='#E8EEF7' style='background:#E8EEF7;padding:.75pt .75pt .75pt .75pt'>");
_sb.AppendLine("<p class='MsoNormal'>");
_sb.AppendLine("<font size='1' face='Arial'>");
_sb.AppendLine("<span style='font-size:9.0pt;");
_sb.AppendLine("font-family:Arial'>");

if (Status == "SentFiles")
{
    _sb.AppendLine("Sent Successfully");
}
else
{
    _sb.AppendLine("Pending");
}
//_sb.AppendLine("<o:p></o:p>");
_sb.AppendLine("</span>");
_sb.AppendLine("</font>");
_sb.AppendLine("</p>");
_sb.AppendLine("</td>");
_sb.AppendLine("<td bgcolor='#E8EEF7' style='background:#E8EEF7;padding:.75pt .75pt .75pt .75pt'>");
_sb.AppendLine("<p class='MsoNormal'>");
_sb.AppendLine("<font size='1' face='Arial'>");
_sb.AppendLine("<span style='font-size:9.0pt;");
_sb.AppendLine("font-family:Arial'>");
_sb.AppendLine(DateSent);
//_sb.AppendLine("<o:p></o:p>");
_sb.AppendLine("</span>");
_sb.AppendLine("</font>");
_sb.AppendLine("</p>");
_sb.AppendLine("</td>");
_sb.AppendLine("</tr>");

//_sb.Append(returnHtmlData(_sb.ToString()));
                return _sb.ToString();
            }


        }

        private bool AppendToFile(string source, string param, StringBuilder _sbHeader, string path, StringBuilder _sbBody, bool isSentFiles)
        {
            bool _ret = true;
            //File.Copy(source, path);
            try
            {
                if (isSentFiles)
                {

                    // Open a file for reading
StreamReader streamReader = File.OpenText(source);
// Now, read the entire file into a strin
string contents = streamReader.ReadToEnd();
streamReader.Close();

// Write the modification into the same fil
StreamWriter streamBody=File.CreateText(path);

string a = contents.Replace("<!--Start Sent Table-->", _sbBody.ToString());
string b = a.Replace("<!--Start Param Client-->", _sbHeader.ToString());

streamBody.Write(b);

streamBody.Close();
                    _ret = true;
                }
                else //TECHNICAL ERROR
                {
                    FileStream _fs = new FileStream(source, FileMode.Open);
                    StreamReader _sr = new StreamReader(_fs);

                    string temp = "";
                    string temp2 = "";
                    while ((temp = _sr.ReadLine()) != null)
                    {
                        if (temp == "\t<!--Start Table3 Technical-->")
                        {     temp2+= "<BR/>";
                              temp2 += "<table align='center' width='98%' border='0' cellpadding='3' cellspacing='1' class='Back'>";
                              temp2 += _sbBody.ToString();//temp2 += name + '\t' + lastname + '\t' + phone + '\t' + mail + '\t' + website + "\r\n";
                              temp2 += "</table>";
                        }
                        else if (temp == "\t<!--Start Param Technical-->")
                        {

                            temp2 += _sbHeader.ToString();
                        }
                        else
                        {
                            temp2 += (temp + "\r\n");
                        }


                    }
                    _sr.Close();
                    _fs.Close();

                    _fs = new FileStream(path, FileMode.Create);
                    StreamWriter sw = new StreamWriter(_fs);
                    sw.Write(temp2);
                    sw.Close();
                    _fs.Close();

                    _ret = true;
                }
                return _ret;
            }

            catch
            {
                //MessageBox.Show(ex.Message);
                return false;
            }

        }
        #endregion

        #region dispose
        public void Dispose() {
            this.messageInformation = null;
            this.serverInformation = null;
        }
        #endregion

        #endregion
    }    
}
