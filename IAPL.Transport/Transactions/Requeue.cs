using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

namespace IAPL.Transport.Transactions
{
    class Requeue
    {
        //Query the httpprocess database filter will be filetransactionid > 0
        //if found 
        //query it again and http.postcxml
        //do cleanup
        List<SentDetail_BO> _SentDetails = new List<SentDetail_BO>();

        bool _sentMail = false;
        bool _err = false;
        string success = string.Empty;
        string _errMessage = string.Empty;
        public void ProcessQueue()
        {
            

            
            //List<string> _errorGroups = _getDB.GetDistinctHTTPProcess();
            List<HTTPProcessObject> _list = GetProcessObjects();
            for (int i = 0; i < _list.Count; i++)
            {
                //SEND IT
                if (!_err)
                {
                    success = SendHTTP(_list[i].URL, _list[i].path, _list[i].Password, _list[i].SourceFile, _list[i].UserName);
                    if (success == "True")
                    {
                        SentDetail_BO _sent = new SentDetail_BO();
                        _sent.DateSent = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToLongTimeString();
                        _sent.FileName = _list[i].FileName;//_SortedList[i].ToString();//_dt.Rows[0]["Path"].ToString();
                        _SentDetails.Add(_sent);

                        DeleteHTTPProcess(_list[i]);
                        //
                        if (i == _list.Count)
                        {
                            Util.Utility.DeleteGUIDDirectory(_list[i].Guid);
                        }

                        //send the email
                        //update the process log
                        
                        SendSuccessEmail(_list[i]);
                        //SendSuccessEmail(_httpBO.MessageCode, _httpBO.Principal, _httpBO.SourceFile, _httpBO.URL, _httpBO.SupplierName, _httpBO.path + "requeue\\"); 
                    }
                    else 
                    {

                        //SEND TECHNICAL ERROR EMAIL

                        if (!_sentMail)
                        {
                            TechnicalErrorEmail(_errMessage, _list[i]);
                            _sentMail = true;
                        }
                        
                    }
                    //keep sending
                }
                else
                {
                    //close thread
                }
                //

            }



        }

        #region CleanUp
        private bool DeleteHTTPProcess(HTTPProcessObject _httpBO)
        {
            bool _ret = true;
            try
            {
                Data.DbTransaction _getDB = new IAPL.Transport.Data.DbTransaction();
                _getDB.DeleteHTTPProcess(_httpBO.TradingCode, _httpBO.MessageCode, _httpBO.Principal, _httpBO.SupplierID, _httpBO.FileName);

                Util.Utility.DeleteFile(_httpBO.path);
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                _ret = false;
            }
           return _ret;
        }
        private List<HTTPProcessObject> GetProcessObjects()
        {
            Data.DbTransaction _getDB = new IAPL.Transport.Data.DbTransaction();
            DataTable _dt = _getDB.GetHTTPProcessOrderByFileNameAndDate();
            List<HTTPProcessObject> _listHTPPProcess = new List<HTTPProcessObject>();
            for (int i = 0; i < _dt.Rows.Count; i++) 
            {
                 HTTPProcessObject _httpBO = new HTTPProcessObject();
                _httpBO = new HTTPProcessObject();
                _httpBO.URL = _dt.Rows[i]["URL"].ToString();
                _httpBO.path = _dt.Rows[i]["Path"].ToString();
                _httpBO.Password = _dt.Rows[i]["Password"].ToString();
                _httpBO.SourceFile = _dt.Rows[i]["SourceFile"].ToString();
                _httpBO.UserName = _dt.Rows[i]["UserName"].ToString();
                _httpBO.TradingCode = _dt.Rows[i]["tradingCode"].ToString();
                _httpBO.MessageCode = _dt.Rows[i]["msgCode"].ToString();
                _httpBO.Principal = _dt.Rows[i]["Prncpal"].ToString();
                _httpBO.SupplierID = _dt.Rows[i]["SupplierID"].ToString();
                _httpBO.FileName = _dt.Rows[i]["FileName"].ToString();
                _httpBO.Guid = _dt.Rows[i]["GUID"].ToString();
                _httpBO.SupplierName = _dt.Rows[i]["suppName"].ToString();
                _httpBO.ERP = _dt.Rows[i]["erp"].ToString();
                _httpBO.EmailSubject = _dt.Rows[i]["emldEmailSubject"].ToString();
                //
                _httpBO.EmailFrom = _dt.Rows[i]["emldIntEmailAddrFROM"].ToString();
                _httpBO.EmailTo = _dt.Rows[i]["emldIntEmailAddrTO"].ToString();
                _httpBO.EmailCC = _dt.Rows[i]["emldIntEmailAddrCC"].ToString();
                _httpBO.EmailBCC = _dt.Rows[i]["emldIntEmailAddrBCC"].ToString();
                _httpBO.EmailEXTTo = _dt.Rows[i]["emldExtEmailAddrTO"].ToString();
                _httpBO.EmailEXTCC = _dt.Rows[i]["emldExtEmailAddrCC"].ToString();
                _httpBO.EmailXSLTPath = _dt.Rows[i]["emldXSLTPath"].ToString();


                _listHTPPProcess.Add(_httpBO);
            }

            return _listHTPPProcess;
        }
        #endregion

        private void SendSuccessEmail(HTTPProcessObject _httpProcess)//string _msgCode, string _erp, string _sourceFile, string _url, string suppName, string backupFolder)
        {
            List<string> _failed = new List<string>();
            List<string> _sent = new List<string>();
            EmailTransaction emailTrans = new EmailTransaction();
            ServerDetails desServerDetails = new ServerDetails();
            MessageDetails msgDetails = new MessageDetails();

            for (int i = 0; i < _SentDetails.Count; i++)
            {
                if (emailTrans.HTTPSendingFailed(_httpProcess.MessageCode, _httpProcess.ERP, _SentDetails[i].FileName))
                {
                    _failed.Add(_SentDetails[i].FileName);
                }
                else
                {
                    _sent.Add(_SentDetails[i].FileName);
                }
            }

            //Remove Batch file from List
            _sent.Remove(_httpProcess.SourceFile);
            _failed.Remove(_httpProcess.SourceFile);



            bool _a = emailTrans.GenerateHTML(_sent, _failed, _httpProcess.Guid + "requeue\\", _httpProcess.EmailXSLTPath, _SentDetails, _httpProcess, false);//, desServerDetails, msgDetails, _SentDetails, false);
        }
        private string SendHTTP(string Url, string FilePath, string Password, string terminator, string Username)
        {
            try 
            {
                Console.WriteLine("Requeue Sending: " + FilePath + " : " + Url);
                Operation.Http _http = new IAPL.Transport.Operation.Http();
                success = _http.POSTXML(Url, FilePath, Username, Util.Utility.Decrypt(Password), terminator).ToString();
               Console.WriteLine("Requeue Sending" + success + ": " + FilePath + " : " + Url);
               _errMessage = _http.ErrorMessage;
            }
            catch(Exception ex)
            {
                //string a = ex.Message;
                success = ex.Message;
                Console.WriteLine("Error sending: " + FilePath + " : " + Url);
                IAPL.Transport.Util.TextLogger.Log("HTTP", "Sending Failed");
                //LOG MESSAGE
            }

            return success;
        }

        private void CreateProcessLog(MessageDetails msgDetails, string _errMessage)
        {
            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            //sourcefilelog table
            System.Collections.Hashtable hTable = new System.Collections.Hashtable();
            hTable.Add("@trdpCode", msgDetails.TradingCode);
            hTable.Add("@msgCode", msgDetails.MessageCode);
            hTable.Add("@erpID", msgDetails.ERP);
            hTable.Add("@sflgIsCountrySetup", "1");
            hTable.Add("@sflgFileType", "1");
            hTable.Add("@sflgSourceFilename", msgDetails.SourceFile);//srcServerDetails.GetSourceFile());
            //hTable.Add("@sflgDestinationFilename", desServerDetails.GetFTPSourceFile(srcServerDetails.GenFileName()));
            hTable.Add("@sflgDestinationFilename", "N/A");

            //msgDetails.SourceFile = msgDetails.SourceFile;//fileName;
            //msgDetails.SourceFolder = msgDetails.SourceFolder;//srcServerDetails.ServerFolder;

            //processlog table
            System.Collections.Hashtable hTable2 = new System.Collections.Hashtable();
            hTable2.Add("@apluCode", msgDetails.ApplicationCode);
            hTable2.Add("@PRNCPL", msgDetails.Principal);
            hTable2.Add("@msgCode", msgDetails.MessageCode);
            hTable2.Add("@ERPID", msgDetails.ERP);
            hTable2.Add("@prlgCustID", "");
            hTable2.Add("@prlgProcessSource", "");
            msgDetails.StartDate = DateTime.Now.ToString();
            msgDetails.EndDate = DateTime.Now.ToString();
            hTable2.Add("@prlgStartDate", msgDetails.StartDate);
            hTable2.Add("@prlgEndDate", msgDetails.EndDate); // this should appear on the update method
            hTable2.Add("@prlgIsSuccess", "0"); // this should appear on the update method
            hTable2.Add("@prlgDescription", "REQUEUE SENDING FAILED");
            hTable2.Add("@prlgTechnicalErrDesc", _errMessage);
            hTable2.Add("@prlgSourceParent", "");
            hTable2.Add("@prlgSourceParentCount", "0");
            hTable2.Add("@prlgSourceChild", "0");
            hTable2.Add("@prlgSourceChildCount", "0");
            db.InsertTransactionLog(hTable, hTable2);
        }
        private void TechnicalErrorEmail(string errorMessage, HTTPProcessObject _httpObject)
        {
            string technicalError = errorMessage;
            Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();
            MessageDetails msgDetails = new MessageDetails();
            errorMessage = _httpObject.SourceFile + "[\'" + _httpObject.FileName + "\'file has been backup on " + _httpObject.Guid + "folder but failed on transmission to " +
                            _httpObject.URL + " Address!] " + errorMessage;

            //errorMessage = threadName + " [\'" + fileName + "\' file has been backup on " + msgDetails.BackupFolder + " folder but failed on transmisson to " +
            //desServerDetails.ServerAddress + " server!] " + errorMessage;
            
            IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", errorMessage);
          
            
            // send email notification 
            msgDetails.ProcessLogID = db.GetProcessLogID;
            msgDetails.TransDescription = "FILETRANSFER - failed.";
            msgDetails.TechnicalErrorDescription = errorMessage;
            //emailTrans = new EmailTransaction(msgDetails);



            msgDetails.ERP = _httpObject.ERP;
            msgDetails.MessageCode = _httpObject.MessageCode;
            msgDetails.SupplierName = _httpObject.SupplierName;
            msgDetails.StartDate = DateTime.Now.ToString();
            msgDetails.EndDate = DateTime.Now.ToString();
            msgDetails.TechnicalErrorDescription = technicalError;
            msgDetails.ApplicationCode = "Requeue";
            msgDetails.TransDescription = errorMessage;
            msgDetails.SourceFile = _httpObject.SourceFile;
            msgDetails.SourceFolder = _httpObject.Guid;
            msgDetails.Principal = _httpObject.Principal;
            msgDetails.TradingCode = _httpObject.TradingCode;

            
            //
            //CREATE PROCESSLOG
            //log to db
            CreateProcessLog(msgDetails, errorMessage);
            //db.UpdateTransactionLog(false, "FILETRANSFER - failed.", errorMessage);
            

            EmailTransaction emailtrans = new EmailTransaction(msgDetails);
            emailtrans.DestinationFolder = _httpObject.URL;
            emailtrans.SourceFile = _httpObject.path;
            emailtrans.OutputFile = "N/A";
            

            //emailTrans.DestinationFolder = desServerDetails.GetDestinationFolder(desServerDetails.ServerFolder);
            //emailTrans.SourceFile = srcServerDetails.GetSourceFile();
            //emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);

            //IAPL.Transport.Transactions.EmailTransaction emailTrans = new EmailTransaction(msgDetails);
            if (!emailtrans.SendEmailNotification(_httpObject.FileName, _httpObject.SourceFile, false))
            {
                string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailtrans.ErrorMessage);
                if (errorList.Length >= 2)
                {
                    IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[Thread: " + _httpObject.SourceFile + "] " + errorList[1]);
                }
                else
                {
                    IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                        "[Thread: " + _httpObject.SourceFile + "] " + emailtrans.ErrorMessage);
                }
            }
        }
    }
}
