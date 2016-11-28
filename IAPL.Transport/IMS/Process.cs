using System;
using System.IO; 
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport.IMS
{
    public class Process
    {
        public struct IMSReportDetails
        {
            public string _countrycode;
            public string _country;
            public string _imsdataseq;
            public double _sizeKB;
            public DateTime _datesent;
            public string _commentresolution;
            public string _datafilerecvd;
            public string _transactiondate;
            public int _recordcount;
            public double _imsextractionvalue;
            public string _sendstatus;
            
            public string CountryCode
            {   
                get {  return _countrycode;  }
                set { this._countrycode = value; }
            }

            public string Country
            {   
                get {  return _country;  }
                set { this._country = value; }
            }

            public string IMSDataSeq
            {   
                get {  return _imsdataseq;  }
                set { this._imsdataseq = value; }
            }

            public double SizeKB
            {   
                get {  return _sizeKB;  }
                set { this._sizeKB = value; }
            }

            public DateTime DateSent
            {   
                get {  return _datesent;  }
                set { this._datesent = value; }
            }

            public string _ERS_Issue;
            public string ERS_Issue
            {
                get { return _ERS_Issue; }
                set { this._ERS_Issue = value; }
            }

            public string _IMSSR_Issue;
            public string IMSSR_Issue
            {
                get { return _IMSSR_Issue; }
                set { this._IMSSR_Issue = value; }
            }

            public string CommentResolution
            {   
                get {  return _commentresolution;  }
                set { this._commentresolution = value; }
            }

            public string DataFileRecvd
            {   
                get {  return _datafilerecvd;  }
                set { this._datafilerecvd = value; }
            }

            public string TransactionDate
            {   get {  return _transactiondate;  }
                set { this._transactiondate = value; }
            }

            public int RecordCount
            {   
                get {  return _recordcount;  }
                set { this._recordcount = value; }
            }

            public double IMSExtractionValue
            {   
                get {  return _imsextractionvalue;  }
                set { this._imsextractionvalue = value; }
            }

            public string SendStatus
            {
                get { return _sendstatus; }
                set { this._sendstatus = value; }
            }
        }

        IAPL.Transport.Util.Utility Util = new IAPL.Transport.Util.Utility();
        IAPL.Transport.Transactions.ServerDetails SDetails = new IAPL.Transport.Transactions.ServerDetails();
        
        public static string IMSFileExt = IAPL.Transport.Configuration.Config.GetAppSettingsValue("IMSFileExt", "ZIP").ToLower();
        public static string IMSLogExt = IAPL.Transport.Configuration.Config.GetAppSettingsValue("IMSLogExt", "LOG").ToLower();
        public static string IMSTerminatorExt = IAPL.Transport.Configuration.Config.GetAppSettingsValue("IMSTerminatorExt", "ZZZ").ToLower();
        public static string IMSIncompleteFolder = IAPL.Transport.Configuration.Config.GetAppSettingsValue("IncompleteFolder", "IMSIncomplete");

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Get the list of IMS files to process
        public System.Collections.Hashtable GetIMSFiles(System.Collections.Hashtable IMSFiles,
                                                        out System.Collections.Hashtable IMSListOfZipFiles,
                                                        out System.Collections.Hashtable IMSIncompleteFiles,
                                                        out System.Collections.Hashtable IMSCompleteCountriesName,
                                                        out System.Collections.Hashtable IMSIncompleteCountriesName)
        {
            string Filename = "";
            string ExtName = "";

            int TerminatorCtr = 0;
            int LogCtr = 0;
            int IMSCtr = 0;
            System.Collections.Hashtable ListOfTerminator = new System.Collections.Hashtable();
            System.Collections.Hashtable ListOfLogs = new System.Collections.Hashtable();
            System.Collections.Hashtable ZipFiles = new System.Collections.Hashtable();
            //ListOfZipFiles = null;            

            foreach (System.Collections.DictionaryEntry file in IMSFiles)
            {
                Filename = SDetails.getFileNameOnly(file.Value.ToString());
                ExtName = Filename.Substring(Filename.LastIndexOf(".") + 1);
                Filename = Filename.ToLower();
                ExtName = ExtName.ToLower();
                // Get list of Terminator

                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                //if (ExtName.Equals(IMSTerminatorExt.ToLower()))

                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                if (string.Equals(ExtName, IMSTerminatorExt, StringComparison.OrdinalIgnoreCase))
                {
                    TerminatorCtr++;
                    ListOfTerminator.Add("file" + TerminatorCtr.ToString(), Filename);
                }

                // Get list of Logs
                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                //if (ExtName.Equals(IMSLogExt.ToLower()))

                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                if (string.Equals(ExtName, IMSLogExt, StringComparison.OrdinalIgnoreCase))
                {
                    LogCtr++;
                    ListOfLogs.Add("file" + LogCtr.ToString(), Filename);
                }

                // Get list of IMS Zip files
                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                //if (ExtName.Equals(IMSFileExt.ToLower()))

                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                if (string.Equals(ExtName, IMSFileExt, StringComparison.OrdinalIgnoreCase))
                {
                    IMSCtr++;
                    ZipFiles.Add("file" + IMSCtr.ToString(), Filename);
                }
            }

            // ------------------------------------------------------------------------------------------

            // Filter files
            int CompleteFileCtr = 0;
            int IncompleteFileCtr = 0;
            int ZipFileCtr = 0;
            int CompleteCountriesNameCtr = 0;
            int IncompleteCountriesNameCtr = 0;
            System.Collections.Hashtable ValidZipFiles = new System.Collections.Hashtable();
            System.Collections.Hashtable CompleteFiles = new System.Collections.Hashtable();
            System.Collections.Hashtable IncompleteFiles = new System.Collections.Hashtable();
            System.Collections.Hashtable CompleteCountriesName = new System.Collections.Hashtable();
            System.Collections.Hashtable IncompleteCountriesName = new System.Collections.Hashtable();
            foreach (System.Collections.DictionaryEntry TerminatorFile in ListOfTerminator)
            {
                string terminatorfile = TerminatorFile.Value.ToString();
                string TermFilename = terminatorfile.Substring(0,terminatorfile.LastIndexOf("."));

                // Check if Terminator filename is found in Log list
                bool LogFound = false;
                foreach (System.Collections.DictionaryEntry LogFile in ListOfLogs)
                {
                    string logfile = LogFile.Value.ToString();
                    string VerifyLogFilename = TermFilename + "." + IMSLogExt;
                    if (logfile.ToUpper() == VerifyLogFilename.ToUpper())
                    {
                        LogFound = true;
                        break;
                    }
                }

                // Check if Terminator filename is found in Zip list
                bool ZipFound = false;
                if (LogFound)
                {
                    foreach (System.Collections.DictionaryEntry ZipFile in ZipFiles)
                    {
                        string zipfile = ZipFile.Value.ToString();
                        string VerifyZipFilename = TermFilename + "." + IMSFileExt;
                        if (zipfile.ToUpper() == VerifyZipFilename.ToUpper())
                        {
                            ZipFound = true;
                            break;
                        }
                    }
                }

                Filename = terminatorfile;
                string ThisFilename = Filename.Substring(0, 7);
                string CountryCode = Filename.Substring(0, 2);
                string VersionNo = Filename.Substring(3, 7);

                if (LogFound && ZipFound)
                {
                    // Add to list of files to pull from source (Complete)
                    CompleteFileCtr++; CompleteFiles.Add("file" + CompleteFileCtr.ToString(), Filename);
                    CompleteFileCtr++; CompleteFiles.Add("file" + CompleteFileCtr.ToString(), Filename.Replace(IMSTerminatorExt, IMSLogExt));
                    CompleteFileCtr++; CompleteFiles.Add("file" + CompleteFileCtr.ToString(), Filename.Replace(IMSTerminatorExt, IMSFileExt));

                    // List Valid Zip files
                    ZipFileCtr++; ValidZipFiles.Add("file" + ZipFileCtr.ToString(), Filename.Replace(IMSTerminatorExt, IMSFileExt));

                    // Get filename and put to list of CompleteCountriesName
                    CompleteCountriesNameCtr++; CompleteCountriesName.Add("file" + ZipFileCtr.ToString(), Filename.Substring(0, 7));                    
                }
                else
                {
                    // List down Incomplete files
                    IncompleteFileCtr++; IncompleteFiles.Add("file" + IncompleteFileCtr.ToString(), Filename);
                    if (LogFound) { IncompleteFileCtr++; IncompleteFiles.Add("file" + IncompleteFileCtr.ToString(), Filename.Replace(IMSTerminatorExt, IMSLogExt)); }
                    if (ZipFound) { IncompleteFileCtr++; IncompleteFiles.Add("file" + IncompleteFileCtr.ToString(), Filename.Replace(IMSTerminatorExt, IMSFileExt)); }

                    // Get filename and put to list of IncompleteCountriesName
                    IncompleteCountriesNameCtr++; IncompleteCountriesName.Add("file" + (IncompleteCountriesName.Count + 1), Filename.Substring(0, 7));
                }
            }
            IMSListOfZipFiles = ValidZipFiles;
            IMSIncompleteFiles = IncompleteFiles;
            IMSCompleteCountriesName=CompleteCountriesName;
            IMSIncompleteCountriesName=IncompleteCountriesName;

            return CompleteFiles;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    
        // **********************************************************
        // Project: ISG12043
        // Developer: Alrazen Estrella
        // Date: Oct. 10, 2008

        public static bool IMSSendEmailWhenNoFilesFound(string ThreadName,
                                                        IAPL.Transport.Transactions.MessageDetails mDetails,
                                                        bool IMSNoFilesFound)
        {
            bool success = true;

            IAPL.Transport.IMS.Process.IMSReportDetails IMSDetailsStruct = new IAPL.Transport.IMS.Process.IMSReportDetails();
            System.Collections.Hashtable IMSData = new System.Collections.Hashtable();

            // Get the country list from the database
            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();
            db.GetIMSCountries(IAPL.Transport.Util.ConstantVariables.SP_GetIMSDetails, "IMSCountries");
            if (db.ErrorMessage.Trim().Length > 0)
            {
                success = false;
                IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-ProcessIMS()", "Database error: " + db.ErrorMessage);
            }
            else
            {
                System.Data.DataTable dTable = db.CommonDataTable;
                if (dTable == null)
                { success = false; }
                else
                {
                    // Set initial value
                    foreach (System.Data.DataRow row in dTable.Rows)
                    {
                        IMSDetailsStruct.Country = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["CountryName"]);
                        IMSDetailsStruct.CountryCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["Code"]);
                        IMSDetailsStruct.ERS_Issue = "NO FILE RECEIVED";
                        IMSDetailsStruct.CommentResolution = "NO FILE RECEIVED";


                        //IMSDetailsStruct.DataFileRecvd = "";
                        //IMSDetailsStruct.TransactionDate = "";
                        //IMSDetailsStruct.RecordCount = 0;
                        //IMSDetailsStruct.IMSExtractionValue = 0;


                        IMSData.Add(IMSDetailsStruct.CountryCode, IMSDetailsStruct);
                    }
                }

                //Gerard Jan 12 2009
              
                System.Data.DataTable _dt = db.GetLastSendDate(mDetails.MessageCode, mDetails.ERP, mDetails.Principal);
                IMSMailStatus _mailStatus = new IMSMailStatus();
                if (_dt.Rows.Count == 0)
                {
                    Console.WriteLine("IMSSendEmailWhenNoFilesFound-- Initial Run -- updating tables");
                    //first run no entries found create an entry
                    db.SaveIMSMailStatus(mDetails.MessageCode, mDetails.Principal, mDetails.ERP);
                    _dt = db.GetLastSendDate(mDetails.MessageCode, mDetails.ERP, mDetails.Principal);
                    //_lastSendDate = Convert.ToDateTime(_dt.Rows[0][0]);
                    _mailStatus.LastSendDate = Convert.ToDateTime(_dt.Rows[0][0]);
                    _mailStatus.MailSent = Convert.ToBoolean(_dt.Rows[0][1]);

                    Console.WriteLine("IMSSendEmailWhenNoFilesFound Mail is being sent. -- Exception Report sent");
                    //Send both reports
                    
                    //Email Summary Report
                    //success = PerformEmailProcess(1, "", "", ThreadName, IMSData, mDetails, true);
                    // Email Exception Report 
                    success = PerformEmailProcess(2, "", "", ThreadName, IMSData, mDetails, true);
                    //Update mail sent
                    db.SaveIMSMailStatus(mDetails.MessageCode, mDetails.Principal, mDetails.ERP);

                }
                else
                {
                    Console.WriteLine("IMSSendEmailWhenNoFilesFound -- Entry Found -- Proceeding with mail send logic.");
                    //entry found
                    _mailStatus.LastSendDate = Convert.ToDateTime(_dt.Rows[0][0]);
                    _mailStatus.MailSent = Convert.ToBoolean(_dt.Rows[0][1]);

                    if (_mailStatus.MailSent == false)
                    {
                        Console.WriteLine("IMSSendEmailWhenNoFilesFound Mail is being sent. -- Exception Report Sent --");
                        //Send both reports

                        //Email Summary Report
                        //success = PerformEmailProcess(1, "", "", ThreadName, IMSData, mDetails, true);
                        // Email Exception Report Summary
                        success = PerformEmailProcess(2, "", "", ThreadName, IMSData, mDetails, true);
                        //Update mail sent
                        db.SaveIMSMailStatus(mDetails.MessageCode, mDetails.Principal, mDetails.ERP);
                    }
                    else
                    {
                        if (_mailStatus.LastSendDate.ToShortDateString() == DateTime.Now.ToShortDateString())
                        {
                            //Do Nothing
                            Console.WriteLine("IMSSendEmailWhenNoFilesFound Mail already Sent today");
                        }
                        else
                        {
                            //update the mail status then send emails.
                            Console.WriteLine("IMSSendEmailWhenNoFilesFound - new day -- updating imsMailStatus and sending Exception Report mail");
                            //Email Summary Report
                            //success = PerformEmailProcess(1, "", "", ThreadName, IMSData, mDetails, true);
                            // Email Exception Report Summary
                            success = PerformEmailProcess(2, "", "", ThreadName, IMSData, mDetails, true);
                            db.SaveIMSMailStatus(mDetails.MessageCode, mDetails.Principal, mDetails.ERP);
                        }
                    }

                }

                //Mail hasn't been sent so send emails
                

                // Delete Process from Temp table
                db.DeleteIMSProcess(mDetails.IMSProcessId);
                if (db.ErrorMessage.Trim().Length > 0)
                {
                    success = false;
                    IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-DeleteIMSFileProcessed()", "Database error: " + db.ErrorMessage);
                }

            }
            return success;
        }
        
        
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public static bool ProcessIMS(string ThreadName,
                                      string IncFolder,
                                      System.Collections.Hashtable IMSCompleteCountriesName, 
                                      System.Collections.Hashtable IMSIncompleteCountriesName,
                                      IAPL.Transport.Transactions.MessageDetails mDetails,
                                      System.Collections.Hashtable FTPFileFoundSentList,
                                      out System.Collections.Hashtable DetailsForIMS)
        {
            bool success = true;
            DetailsForIMS = new System.Collections.Hashtable(); 
            if (mDetails.IndividualProcess.Equals(1))
            {
                IAPL.Transport.IMS.Process.IMSReportDetails IMSDetailsStruct = new IAPL.Transport.IMS.Process.IMSReportDetails();
                System.Collections.Hashtable IMSData = new System.Collections.Hashtable();

                // Get the country list from the database
                IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();
                db.GetIMSCountries(IAPL.Transport.Util.ConstantVariables.SP_GetIMSDetails, "IMSCountries");
                if (db.ErrorMessage.Trim().Length > 0)
                {
                    success = false;
                    IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-ProcessIMS()", "Database error: " + db.ErrorMessage);
                }
                else
                {
                    System.Data.DataTable dTable = db.CommonDataTable;
                    if (dTable == null)
                    {   success = false;    }
                    else
                    {
                        // Set initial value
                        foreach (System.Data.DataRow row in dTable.Rows)
                        {
                            IMSDetailsStruct.CountryCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["Code"]);
                            IMSDetailsStruct.Country = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["CountryName"]);
                            IMSDetailsStruct.IMSDataSeq = "";
                            IMSDetailsStruct.SizeKB = 0;
                            IMSDetailsStruct.DateSent = System.DateTime.Now;
                            IMSDetailsStruct.ERS_Issue = "";
                            IMSDetailsStruct.IMSSR_Issue = "";
                            IMSDetailsStruct.CommentResolution = "";
                            IMSDetailsStruct.DataFileRecvd = "";
                            IMSDetailsStruct.TransactionDate = "";
                            IMSDetailsStruct.RecordCount = 0;
                            IMSDetailsStruct.IMSExtractionValue = 0;
                            IMSDetailsStruct.SendStatus = "";
                            IMSData.Add(IMSDetailsStruct.CountryCode, IMSDetailsStruct);
                        }
                    }

                    // Process CompleteCountriesName
                    foreach (System.Collections.DictionaryEntry file in (System.Collections.Hashtable)IMSCompleteCountriesName)
                    {
                        string filename = file.Value.ToString();
                        string CountryCode = filename.Substring(0,3);

                        // ------------- Get Struct inside Hashtable then edit value(s) ---------------------
                        IMSReportDetails IMS = (IMSReportDetails)IMSData[CountryCode.ToUpper()];

                        // Version No. (both reports)
                        IMS.IMSDataSeq = filename.Substring(3);

                        // File size in KB (both reports)
                        string IMSFolderPath = mDetails.BackupFolder + @"\" + mDetails.IMSFolder;
                        long FileSize = IAPL.Transport.Util.Utility.GetFileSize_Local(IMSFolderPath + @"\" + filename + "." + Process.IMSFileExt);
                        IMS.SizeKB = IAPL.Transport.Util.Utility.ConvertByteToKB(FileSize);

                        // Date Sent (both reports)
                        IMS.DateSent = System.DateTime.Now;

                        // Open file and parse 
                        string filereadbuf;
                        string IMSLogfile = IMSFolderPath + @"\" + filename + "." + Process.IMSLogExt;
                        int Size = (int)IAPL.Transport.Util.Utility.GetFileSize_Local(IMSLogfile);
                        char[] buf = new char[Size];
                        StreamReader sr = new StreamReader(new FileStream(IMSLogfile, FileMode.Open, FileAccess.Read));
                        int retval = sr.ReadBlock(buf, 0, Size); 
                        filereadbuf = new string(buf); 
                        sr.Close();
                        string[] ParsedData = IAPL.Transport.Util.Utility.ParseString(filereadbuf);

                        // Issue (both reports)
                        IMS.ERS_Issue = ParsedData[2].ToString();
                        IMS.IMSSR_Issue = ParsedData[2].ToString();


                        // Comment/Resolution (IMS Summary Report)
                        IMS.CommentResolution = ParsedData[3].ToString();


                        // Data File Received (IMS Summary Report)
                        if (FTPFileFoundSentList[CountryCode] != null)
                            IMS.DataFileRecvd = "Yes";
                        else
                            IMS.DataFileRecvd = "No File";


                        // Transaction Date (IMS Summary Report)
                        IMS.TransactionDate = ParsedData[4].ToString();


                        // Record Count (IMS Summary Report)
                        IMS.RecordCount = Convert.ToInt32(ParsedData[5].ToString());


                        // IMS Extraction Value (IMS Summary Report)
                        IMS.IMSExtractionValue = Convert.ToDouble(ParsedData[6].ToString());

                        // Send Status (Exception Report Summary)
                        if (IMS.ERS_Issue.Equals(string.Empty))
                        { IMS.SendStatus = "SUCCESS"; }
                        else
                        { IMS.SendStatus = IMS.ERS_Issue; }

                        // ----------------------------------------------------------------------------------

                        // Update Hashtable with edited value(s)
                        IMSData[CountryCode] = IMS; 
                    }
                }
                
                // Temp
                string fileName = "";
                string desFilePath = "";

                // Email Exception Report Summary
                success = PerformEmailProcess(1, fileName, desFilePath, ThreadName, IMSData, mDetails, false);

                // Email IMS Summary Report
                success = PerformEmailProcess(2, fileName, desFilePath, ThreadName, IMSData, mDetails, false);

                // Delete Processed files from database
                db.DeleteIMSFileProcessed(mDetails.IMSProcessId);
                if (db.ErrorMessage.Trim().Length > 0)
                {
                    success = false;
                    IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-DeleteIMSFileProcessed()", "Database error: " + db.ErrorMessage);
                }
                else
                {
                    // Delete Process from Temp table
                    db.DeleteIMSProcess(mDetails.IMSProcessId);
                    if (db.ErrorMessage.Trim().Length > 0)
                    {
                        success = false;
                        IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-DeleteIMSFileProcessed()", "Database error: " + db.ErrorMessage);
                    }
                }
            }
            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private static bool PerformEmailProcess(int Report,
                                                string fileName, 
                                                string desFilePath, 
                                                string ThreadName,
                                                System.Collections.Hashtable DetailsForIMS,
                                                IAPL.Transport.Transactions.MessageDetails mDetails,
                                                bool IMSNoFilesFound)
        {
            IAPL.Transport.Transactions.EmailTransaction emailTrans = new IAPL.Transport.Transactions.EmailTransaction();
            emailTrans.IMSNoFilesFound = IMSNoFilesFound;
            bool success = emailTrans.StartProcessIMS(Report, fileName, desFilePath, ThreadName, DetailsForIMS, mDetails);
            return success;
        }

        // **********************************************************


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    }

    public class IMSMailStatus
    {
        public bool _mailSent;
        public DateTime _lastSendDate;

        public bool MailSent
        {
            get { return _mailSent; }
            set { _mailSent = value; }
        }

        public DateTime LastSendDate
        {
            get { return _lastSendDate; }
            set { _lastSendDate = value; }
        }
    }
}
