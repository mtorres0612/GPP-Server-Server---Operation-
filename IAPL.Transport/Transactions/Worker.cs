/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Added ProcessFiles method for serialization
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;
using IAPL.Transport.Operation;
using IAPL.Transport.Data;
using IAPL.Transport.Configuration;
using IAPL.Transport.Util;
using System.Xml.Serialization;
using System.Threading;            // Alrazen Estrella || ISG12152 

namespace IAPL.Transport.Transactions
{
    public class Worker
    {
        private string _IMSProcessId;
        public string IMSProcessId
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMddhhmmss") + "IMS";
            }
            set
            {
                _IMSProcessId = value;
            }
        }

        private string _imsfolder;
        public string imsfolder
        {
            get
            {
                return DateTime.Now.ToString("yyyyMMdd") + "IMS";
            }
            set
            {
                _imsfolder = value;
            }
        }

        private static object thislock = new object();

        #region local variables
        bool _mailSent = false;
        List<SentDetail_BO> _SentDetails = new List<SentDetail_BO>();
        /// <summary>
        /// test
        /// </summary>
        IAPL.Transport.Transactions.MessageDetails msgDetails = new MessageDetails();
        IAPL.Transport.Transactions.ServerDetails srcServerDetails = new ServerDetails();
        IAPL.Transport.Transactions.ServerDetails desServerDetails = new ServerDetails();
        IAPL.Transport.Operation.Network netOperation = new IAPL.Transport.Operation.Network();
        IAPL.Transport.Operation.Http _http = new IAPL.Transport.Operation.Http();

        // ********************************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        // Date: July 23, 2008

        IAPL.Transport.Transactions.NetTransaction NetTrans = new IAPL.Transport.Transactions.NetTransaction();
        System.Collections.Hashtable ExtractedFiles = new System.Collections.Hashtable();
        System.Collections.Hashtable ThreadZipFolders = new System.Collections.Hashtable();
        private Hashtable fitefiles = new Hashtable();

        //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
        //private static int CtrFilesProcessed = 1;

        //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
        private static int CtrFilesProcessed = 0;

        //private static bool CounterUpdateDone = false;

        private int totalfiles = 0;
        public int TotalFiles
        {
            get { return this.totalfiles; }
            set { this.totalfiles = value; }
        }

        // ********************************************************************************

        // *********************************************************************
        // Project: ISG12043
        // Developer: Alrazen Estrella
        // Date: September 22, 2008

        IAPL.Transport.Data.DbTransaction DBTrans = new IAPL.Transport.Data.DbTransaction();

        private Hashtable FTPFileFoundSentList = null;
        private Hashtable FTPFileNotFoundSentList = null;
        private Hashtable listofzipfiles = new Hashtable();
        private Hashtable listofincompletefiles = new Hashtable();
        private Hashtable listofcompletecountriesname = new Hashtable();
        private Hashtable listofincompletecountriesname = new Hashtable();
        private Hashtable DetailsForIMS = null;

        private Hashtable imsdetails = new Hashtable();
        IAPL.Transport.IMS.Process IMSProcess = new IAPL.Transport.IMS.Process();

        private Hashtable _NamedConventionFiles = new Hashtable();
        // *********************************************************************

        string threadName = "";
        string fileName = "";
        List<string> files = new List<string>();
        private int transferDelayToDestination = 1500;
        private int maxAttemptOnFailed = 5;

        //LENIN - ISG11597 - ADD - 11-27-2007
        private Hashtable fileNames = new Hashtable();
        /// <summary>
        /// Managing FITE File Names
        /// </summary>
        /// <remarks>dsfsdf</remarks>
        private string _FITEFileName = "";

        #endregion

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        #region constructors
        public Worker(
            string threadName,
            string fileName,
            IAPL.Transport.Transactions.MessageDetails mDetails,
            IAPL.Transport.Transactions.ServerDetails srcDetails,
            IAPL.Transport.Transactions.ServerDetails desDetails,
            int transferDelay,
            int maxAttempt)
        {
            this.threadName                 = threadName;
            this.fileName                   = fileName;
            this.transferDelayToDestination = transferDelay;
            this.maxAttemptOnFailed         = maxAttempt;

            // message settings
            this.msgDetails.ApplicationCode            = mDetails.ApplicationCode;
            this.msgDetails.BackupFolder               = mDetails.BackupFolder;
            this.msgDetails.CountryCode                = mDetails.CountryCode;
            this.msgDetails.EndDate                    = mDetails.EndDate;
            this.msgDetails.ERP                        = mDetails.ERP;
            this.msgDetails.MessageCode                = mDetails.MessageCode;
            this.msgDetails.MessageFileDestinationID   = mDetails.MessageFileDestinationID;
            this.msgDetails.MessageFileSourceID        = mDetails.MessageFileSourceID;
            this.msgDetails.Principal                  = mDetails.Principal;
            this.msgDetails.ProcessLogID               = mDetails.ProcessLogID;
            this.msgDetails.SourceFile                 = mDetails.SourceFile;
            this.msgDetails.SourceFileMask             = mDetails.SourceFileMask;
            this.msgDetails.SourceFolder               = mDetails.SourceFolder;
            this.msgDetails.StartDate                  = mDetails.StartDate;
            this.msgDetails.SupplierID                 = mDetails.SupplierID;
            this.msgDetails.SupplierName               = mDetails.SupplierName;
            this.msgDetails.TechnicalErrorDescription  = mDetails.TechnicalErrorDescription;
            this.msgDetails.TradingCode                = mDetails.TradingCode;
            this.msgDetails.TradingName                = mDetails.TradingName;
            this.msgDetails.TransDescription           = mDetails.TransDescription;
            this.msgDetails.SetSendSuccessNotification = mDetails.SendSuccessNotification.ToString();

            //LENIN - ISG11597 - ADD - 11-27-2007
            this.msgDetails.FITEFileMask            = mDetails.FITEFileMask;
            this.msgDetails.SetZippingFunctionality = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.IsZIP);
            this.msgDetails.ZIPPassword             = mDetails.ZIPPassword;
            this.msgDetails.Retention               = mDetails.Retention;

            // ********************************************
            // Developer: Alrazen Estrella
            // Project: ISG12152
            // Date: July 17, 2008

            this.msgDetails.SetZipSource       = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.IsZIPSource);
            this.msgDetails.SetFilesSentSingle = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.FilesSentSingle);
            this.msgDetails.SetFilesSentBatch  = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.FilesSentBatch);
            this.msgDetails.ProcessCode        = mDetails.ProcessCode;

            // ********************************************

            // ********************************************
            // Developer: Alrazen Estrella
            // Project: ISG12128
            // Date: September 4, 2008

            this.msgDetails.SetFileConvertionFlag = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.FileConvertionFlag);
            this.msgDetails.SourceCodePage        = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString(mDetails.SourceCodePage));
            this.msgDetails.DestinationCodePage   = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString(mDetails.DestinationCodePage));

            // ********************************************

            // **********************************************************
            // Developer: Alrazen Estrella
            // Date: September 29, 2008
            // Project: ISG12043

            this.msgDetails.MsetFilePickupDelay = Convert.ToInt32(mDetails.MsetFilePickupDelay);
            this.msgDetails.IndividualProcess   = Convert.ToInt32(mDetails.IndividualProcess);
            this.msgDetails.MsgManualRunFlag    = Convert.ToBoolean(mDetails.MsgManualRunFlag);
            this.msgDetails.MsgStartTime        = mDetails.MsgStartTime;
            this.msgDetails.MsgEndTime          = mDetails.MsgEndTime;
            this.msgDetails.MsetBatchRun        = Convert.ToBoolean(mDetails.MsetBatchRun);
            this.msgDetails.MsetBatchTime       = mDetails.MsetBatchTime;
            this.msgDetails.MsetRuntime         = Convert.ToBoolean(mDetails.MsetRuntime);
            this.msgDetails.MsetStartTime       = mDetails.MsetStartTime;
            this.msgDetails.MsetEndTime         = mDetails.MsgEndTime;
            this.msgDetails.MsetInterval        = Convert.ToInt32(mDetails.MsetInterval);
            this.msgDetails.IMSBatchRun         = Convert.ToBoolean(mDetails.IMSBatchRun);
            this.msgDetails.IMSFolder           = mDetails.IMSFolder;
            this.msgDetails.IMSProcessId        = mDetails.IMSProcessId;
            this.msgDetails.CrashStatus         = mDetails.CrashStatus;

            // **********************************************************

            //// source server info

            //NEW CODE SR#33117 Ccenriquez / Capad -- November 16, 2009
            this.srcServerDetails.TotalFiles = srcDetails.TotalFiles;

            this.srcServerDetails.DateValue            = srcDetails.DateValue;
            this.srcServerDetails.EmailAddressBCC      = srcDetails.EmailAddressBCC;
            this.srcServerDetails.EmailAddressCC       = srcDetails.EmailAddressCC;
            this.srcServerDetails.EmailAddressFrom     = srcDetails.EmailAddressFrom;
            this.srcServerDetails.EmailAddressTo       = srcDetails.EmailAddressTo;
            this.srcServerDetails.EmailBody            = srcDetails.EmailBody;
            this.srcServerDetails.EmailSubject         = srcDetails.EmailSubject;
            this.srcServerDetails.ErrorMessage         = "";
            this.srcServerDetails.FileCounter          = srcDetails.FileCounter;
            this.srcServerDetails.FileDirection        = srcDetails.FileDirection;
            this.srcServerDetails.FileNamingConvention = srcDetails.FileNamingConvention;
            this.srcServerDetails.FileNamingExtension  = srcDetails.FileNamingExtension;
            this.srcServerDetails.MessageAction        = srcDetails.MessageAction;
            this.srcServerDetails.MessageCode          = srcDetails.MessageCode;
            this.srcServerDetails.OrigSourceFileName   = srcDetails.OrigSourceFileName;
            this.srcServerDetails.ServerAddress        = srcDetails.ServerAddress;
            this.srcServerDetails.ServerFolder         = srcDetails.ServerFolder;
            this.srcServerDetails.ServerPassword       = srcDetails.ServerPassword;
            this.srcServerDetails.ServerUserName       = srcDetails.ServerUserName;
            this.srcServerDetails.TransmissionType     = srcDetails.TransmissionType;
            this.srcServerDetails.FileNameDateFormat   = srcDetails.FileNameDateFormat;
            this.srcServerDetails.ZipCopytoDestination = srcDetails.ZipCopytoDestination;
            // destination server info

            //NEW CODE SR#33117 Ccenriquez / Capad -- November 16, 2009
            this.desServerDetails.TotalFiles = srcDetails.TotalFiles;

            this.desServerDetails.DateValue            = desDetails.DateValue;
            this.desServerDetails.EmailAddressBCC      = desDetails.EmailAddressBCC;
            this.desServerDetails.EmailAddressCC       = desDetails.EmailAddressCC;
            this.desServerDetails.EmailAddressFrom     = desDetails.EmailAddressFrom;
            this.desServerDetails.EmailAddressTo       = desDetails.EmailAddressTo;
            this.desServerDetails.EmailBody            = desDetails.EmailBody;
            this.desServerDetails.EmailSubject         = desDetails.EmailSubject;
            this.desServerDetails.ErrorMessage         = "";
            this.desServerDetails.FileCounter          = desDetails.FileCounter;
            this.desServerDetails.FileDirection        = desDetails.FileDirection;
            this.desServerDetails.FileNamingConvention = desDetails.FileNamingConvention;
            this.desServerDetails.FileNamingExtension  = desDetails.FileNamingExtension;
            this.desServerDetails.MessageAction        = desDetails.MessageAction;
            this.desServerDetails.MessageCode          = desDetails.MessageCode;
            this.desServerDetails.OrigSourceFileName   = desDetails.OrigSourceFileName;
            this.desServerDetails.ServerAddress        = desDetails.ServerAddress;
            this.desServerDetails.ServerFolder         = desDetails.ServerFolder;
            this.desServerDetails.ServerPassword       = desDetails.ServerPassword;
            this.desServerDetails.ServerUserName       = desDetails.ServerUserName;
            this.desServerDetails.TransmissionType     = desDetails.TransmissionType;
            this.desServerDetails.FileNameDateFormat   = desDetails.FileNameDateFormat;

            //Jun Roxas 3.2015
            //Zoetis custom port enhancement HPQC12493
            this.desServerDetails.ServerPort = desDetails.ServerPort;

            //Jun Roxas 3.4.2015
            //add ability to use temporary file terminator extension during FTP upload
            this.desServerDetails.IsUseFileTempExtension = desDetails.IsUseFileTempExtension;
            this.desServerDetails.FileTempExtension      = desDetails.FileTempExtension;
            this.desServerDetails.ZipCopytoDestination   = desDetails.ZipCopytoDestination;
        }
        #endregion

        public Worker(
            string threadName,
            List<string> files,
            IAPL.Transport.Transactions.MessageDetails mDetails,
            IAPL.Transport.Transactions.ServerDetails srcDetails,
            IAPL.Transport.Transactions.ServerDetails desDetails,
            int transferDelay,
            int maxAttempt)
        {
            this.threadName                 = threadName;
            this.files                      = files;
            this.transferDelayToDestination = transferDelay;
            this.maxAttemptOnFailed         = maxAttempt;

            // message settings
            this.msgDetails.ApplicationCode            = mDetails.ApplicationCode;
            this.msgDetails.BackupFolder               = mDetails.BackupFolder;
            this.msgDetails.CountryCode                = mDetails.CountryCode;
            this.msgDetails.EndDate                    = mDetails.EndDate;
            this.msgDetails.ERP                        = mDetails.ERP;
            this.msgDetails.MessageCode                = mDetails.MessageCode;
            this.msgDetails.MessageFileDestinationID   = mDetails.MessageFileDestinationID;
            this.msgDetails.MessageFileSourceID        = mDetails.MessageFileSourceID;
            this.msgDetails.Principal                  = mDetails.Principal;
            this.msgDetails.ProcessLogID               = mDetails.ProcessLogID;
            this.msgDetails.SourceFile                 = mDetails.SourceFile;
            this.msgDetails.SourceFileMask             = mDetails.SourceFileMask;
            this.msgDetails.SourceFolder               = mDetails.SourceFolder;
            this.msgDetails.StartDate                  = mDetails.StartDate;
            this.msgDetails.SupplierID                 = mDetails.SupplierID;
            this.msgDetails.SupplierName               = mDetails.SupplierName;
            this.msgDetails.TechnicalErrorDescription  = mDetails.TechnicalErrorDescription;
            this.msgDetails.TradingCode                = mDetails.TradingCode;
            this.msgDetails.TradingName                = mDetails.TradingName;
            this.msgDetails.TransDescription           = mDetails.TransDescription;
            this.msgDetails.SetSendSuccessNotification = mDetails.SendSuccessNotification.ToString();

            //LENIN - ISG11597 - ADD - 11-27-2007
            this.msgDetails.FITEFileMask            = mDetails.FITEFileMask;
            this.msgDetails.SetZippingFunctionality = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.IsZIP);
            this.msgDetails.ZIPPassword             = mDetails.ZIPPassword;
            this.msgDetails.Retention               = mDetails.Retention;

            // ********************************************
            // Developer: Alrazen Estrella
            // Project: ISG12152
            // Date: July 17, 2008

            this.msgDetails.SetZipSource       = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.IsZIPSource);
            this.msgDetails.SetFilesSentSingle = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.FilesSentSingle);
            this.msgDetails.SetFilesSentBatch  = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.FilesSentBatch);
            this.msgDetails.ProcessCode        = mDetails.ProcessCode;

            // ********************************************

            // ********************************************
            // Developer: Alrazen Estrella
            // Project: ISG12128
            // Date: September 4, 2008

            this.msgDetails.SetFileConvertionFlag = IAPL.Transport.Util.CommonTools.ValueToString(mDetails.FileConvertionFlag);
            this.msgDetails.SourceCodePage        = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString(mDetails.SourceCodePage));
            this.msgDetails.DestinationCodePage   = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString(mDetails.DestinationCodePage));

            // ********************************************

            // **********************************************************
            // Developer: Alrazen Estrella
            // Date: September 29, 2008
            // Project: ISG12043

            this.msgDetails.MsetFilePickupDelay = Convert.ToInt32(mDetails.MsetFilePickupDelay);
            this.msgDetails.IndividualProcess   = Convert.ToInt32(mDetails.IndividualProcess);
            this.msgDetails.MsgManualRunFlag    = Convert.ToBoolean(mDetails.MsgManualRunFlag);
            this.msgDetails.MsgStartTime        = mDetails.MsgStartTime;
            this.msgDetails.MsgEndTime          = mDetails.MsgEndTime;
            this.msgDetails.MsetBatchRun        = Convert.ToBoolean(mDetails.MsetBatchRun);
            this.msgDetails.MsetBatchTime       = mDetails.MsetBatchTime;
            this.msgDetails.MsetRuntime         = Convert.ToBoolean(mDetails.MsetRuntime);
            this.msgDetails.MsetStartTime       = mDetails.MsetStartTime;
            this.msgDetails.MsetEndTime         = mDetails.MsgEndTime;
            this.msgDetails.MsetInterval        = Convert.ToInt32(mDetails.MsetInterval);
            this.msgDetails.IMSBatchRun         = Convert.ToBoolean(mDetails.IMSBatchRun);
            this.msgDetails.IMSFolder           = mDetails.IMSFolder;
            this.msgDetails.IMSProcessId        = mDetails.IMSProcessId;
            this.msgDetails.CrashStatus         = mDetails.CrashStatus;
            this.msgDetails.MsgMonday           = mDetails.MsgMonday;
            this.msgDetails.MsgTuesday          = mDetails.MsgTuesday;
            this.msgDetails.MsgWednesday        = mDetails.MsgWednesday;
            this.msgDetails.MsgThursday         = mDetails.MsgThursday;
            this.msgDetails.MsgFriday           = mDetails.MsgFriday;
            this.msgDetails.MsgSaturday         = mDetails.MsgSaturday;
            this.msgDetails.MsgSunday           = mDetails.MsgSunday;

            // **********************************************************

            //// source server info

            //NEW CODE SR#33117 Ccenriquez / Capad -- November 16, 2009
            this.srcServerDetails.TotalFiles = srcDetails.TotalFiles;

            this.srcServerDetails.DateValue            = srcDetails.DateValue;
            this.srcServerDetails.EmailAddressBCC      = srcDetails.EmailAddressBCC;
            this.srcServerDetails.EmailAddressCC       = srcDetails.EmailAddressCC;
            this.srcServerDetails.EmailAddressFrom     = srcDetails.EmailAddressFrom;
            this.srcServerDetails.EmailAddressTo       = srcDetails.EmailAddressTo;
            this.srcServerDetails.EmailBody            = srcDetails.EmailBody;
            this.srcServerDetails.EmailSubject         = srcDetails.EmailSubject;
            this.srcServerDetails.ErrorMessage         = "";
            this.srcServerDetails.FileCounter          = srcDetails.FileCounter;
            this.srcServerDetails.FileDirection        = srcDetails.FileDirection;
            this.srcServerDetails.FileNamingConvention = srcDetails.FileNamingConvention;
            this.srcServerDetails.FileNamingExtension  = srcDetails.FileNamingExtension;
            this.srcServerDetails.MessageAction        = srcDetails.MessageAction;
            this.srcServerDetails.MessageCode          = srcDetails.MessageCode;
            this.srcServerDetails.OrigSourceFileName   = srcDetails.OrigSourceFileName;
            this.srcServerDetails.ServerAddress        = srcDetails.ServerAddress;
            this.srcServerDetails.ServerFolder         = srcDetails.ServerFolder;
            this.srcServerDetails.ServerPassword       = srcDetails.ServerPassword;
            this.srcServerDetails.ServerUserName       = srcDetails.ServerUserName;
            this.srcServerDetails.TransmissionType     = srcDetails.TransmissionType;
            this.srcServerDetails.FileNameDateFormat   = srcDetails.FileNameDateFormat;

            // destination server info

            //NEW CODE SR#33117 Ccenriquez / Capad -- November 16, 2009
            this.desServerDetails.TotalFiles = srcDetails.TotalFiles;

            this.desServerDetails.DateValue            = desDetails.DateValue;
            this.desServerDetails.EmailAddressBCC      = desDetails.EmailAddressBCC;
            this.desServerDetails.EmailAddressCC       = desDetails.EmailAddressCC;
            this.desServerDetails.EmailAddressFrom     = desDetails.EmailAddressFrom;
            this.desServerDetails.EmailAddressTo       = desDetails.EmailAddressTo;
            this.desServerDetails.EmailBody            = desDetails.EmailBody;
            this.desServerDetails.EmailSubject         = desDetails.EmailSubject;
            this.desServerDetails.ErrorMessage         = "";
            this.desServerDetails.FileCounter          = desDetails.FileCounter;
            this.desServerDetails.FileDirection        = desDetails.FileDirection;
            this.desServerDetails.FileNamingConvention = desDetails.FileNamingConvention;
            this.desServerDetails.FileNamingExtension  = desDetails.FileNamingExtension;
            this.desServerDetails.MessageAction        = desDetails.MessageAction;
            this.desServerDetails.MessageCode          = desDetails.MessageCode;
            this.desServerDetails.OrigSourceFileName   = desDetails.OrigSourceFileName;
            this.desServerDetails.ServerAddress        = desDetails.ServerAddress;
            this.desServerDetails.ServerFolder         = desDetails.ServerFolder;
            this.desServerDetails.ServerPassword       = desDetails.ServerPassword;
            this.desServerDetails.ServerUserName       = desDetails.ServerUserName;
            this.desServerDetails.TransmissionType     = desDetails.TransmissionType;
            this.desServerDetails.FileNameDateFormat   = desDetails.FileNameDateFormat;

            //Jun Roxas 3.2015
            //Zoetis custom port enhancement HPQC12493
            this.desServerDetails.ServerPort = desDetails.ServerPort;

            //Jun Roxas 3.4.2015
            //add ability to use temporary file terminator extension during FTP upload
            this.desServerDetails.IsUseFileTempExtension = desDetails.IsUseFileTempExtension;
            this.desServerDetails.FileTempExtension = desDetails.FileTempExtension;
        }
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        #region methods

        #region processFile
        public void ProcessFile()
        {
            string errorMessage = "";

            IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + " is running. ");
            IAPL.Transport.Transactions.EmailTransaction emailTrans = null;

            #region get orig filename
            if (srcServerDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK)
            {
                srcServerDetails.OrigSourceFileName = srcServerDetails.getFileNameOnly(fileName);
                desServerDetails.OrigSourceFileName = srcServerDetails.getFileNameOnly(fileName);
                fileName = srcServerDetails.getFileNameOnly(fileName);
            }
            else
            {
                srcServerDetails.OrigSourceFileName = fileName;
                desServerDetails.OrigSourceFileName = fileName;
            }
            #endregion



            //log to db first
            #region insert dbtransaction

            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            //sourcefilelog table
            System.Collections.Hashtable hTable = new System.Collections.Hashtable();
            hTable.Add("@trdpCode", msgDetails.TradingCode);
            hTable.Add("@msgCode", msgDetails.MessageCode);
            hTable.Add("@erpID", msgDetails.ERP);
            hTable.Add("@sflgIsCountrySetup", "1");
            hTable.Add("@sflgFileType", "1");
            hTable.Add("@sflgSourceFilename", srcServerDetails.GetSourceFile());
            hTable.Add("@sflgDestinationFilename", desServerDetails.GetDestinationFile(desServerDetails.ServerFolder));

            msgDetails.SourceFile = fileName;
            msgDetails.SourceFolder = srcServerDetails.ServerFolder;

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
            hTable2.Add("@prlgDescription", "FILETRANSFER - on-going....");
            hTable2.Add("@prlgTechnicalErrDesc", "no error");
            hTable2.Add("@prlgSourceParent", "");
            hTable2.Add("@prlgSourceParentCount", "0");
            hTable2.Add("@prlgSourceChild", "0");
            hTable2.Add("@prlgSourceChildCount", "0");
            db.InsertTransactionLog(hTable, hTable2);

            #endregion
            #region Lenin
            //LENIN - ISG11597 - ADD - 11-27-2007
            if (msgDetails.FITEFileMask.Trim() == String.Empty)
            {
                // NO FILE MASKING

                //get files from the source server           
                errorMessage = transferNow(fileName, threadName, srcServerDetails, msgDetails, string.Empty);
            }
            else
            {
                // WITH FILE MASKING

                //get files from the source server
                string zipfilename = srcServerDetails.GenFileName();
                if (msgDetails.ProcessCode.Equals(5))
                {
                    errorMessage = transferNow(fileName, threadName, srcServerDetails, msgDetails, zipfilename);
                }
                else
                {
                    errorMessage = transferNow(_FITEFileName, threadName, srcServerDetails, msgDetails, zipfilename);
                    fileName = zipfilename;
                }
            }
            #endregion
            if (errorMessage.Trim().Length < 1)
            {
                IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + " has been moved to backup folder.");

                int countAttempt = 0;
                errorMessage = "first try";
                if (desServerDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP)
                {
                    errorMessage = transferNow(fileName, threadName, desServerDetails, msgDetails, string.Empty);
                }
                else
                {
                    while (errorMessage.Trim().Length > 0 && countAttempt < this.maxAttemptOnFailed)
                    {
                        //SR#33117 Ccenriquez / Capad -- November 13, 2009
                        desServerDetails.CountSendAttempt = countAttempt;

                        errorMessage = transferNow(fileName, threadName, desServerDetails, msgDetails, string.Empty);

                        if (errorMessage.Trim().Length > 0)
                        {
                            IAPL.Transport.Util.TextLogger.LogError("tab", "(Thread " + threadName +
                                ") Error: " + errorMessage);

                            IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "",
                                "Thread " + threadName +
                                " Transfer to destination server failed! Attempt number " + Convert.ToString(countAttempt + 1) + "...");

                            // cause to delay the transfer of file to destination server to avoid file locking
                            System.Threading.Thread.Sleep(this.transferDelayToDestination);
                        }

                        countAttempt++;
                    }
                }

                // Perform Email notification
                if (errorMessage.Trim().Length < 1)
                {
                    #region Success
                    IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + " has been moved from backup folder to destination server.");

                    db.UpdateTransactionLog(true, "FILETRANSFER - successful.", ""); // change parameter to reflect the successful trans                    

                    // send email notification
                    if (desServerDetails.TransmissionType != IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL)
                    {
                        if (msgDetails.SendSuccessNotification == true) // send only if messagesettings.MsetSendSuccessNotification == true
                        {
                            msgDetails.ProcessLogID = db.GetProcessLogID;
                            msgDetails.TransDescription = "FILETRANSFER - successful.";
                            msgDetails.TechnicalErrorDescription = "";
                            msgDetails.EndDate = DateTime.Now.ToString();

                            emailTrans = new EmailTransaction(msgDetails);
                            emailTrans.DestinationFolder = desServerDetails.GetSourceFolder("");

                            if (msgDetails.ProcessCode.Equals(4) | msgDetails.ProcessCode.Equals(6))
                            {
                                emailTrans.SourceFile = msgDetails.SourceFiles;                 // Bug fix | ISG12128

                                //OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                                //emailTrans.OutputFile = msgDetails.ActualOutputFile;

                                //NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                                emailTrans.OutputFile = desServerDetails.DesFileName;
                            }
                            else if (msgDetails.ProcessCode.Equals(7))
                            {
                                //STEP 1 GENERATE THE BASE HTML

                                List<string> _failed = new List<string>();
                                List<string> _sent = new List<string>();


                                for (int i = 0; i < _SentDetails.Count; i++)
                                {
                                    //Compare this to 

                                    if (emailTrans.HTTPSendingFailed(msgDetails.MessageCode, msgDetails.ERP, _SentDetails[i].FileName))//fileNames["file" + (i + 1)].ToString()))
                                    {
                                        _failed.Add(_SentDetails[i].FileName);
                                    }
                                    else
                                    {
                                        _sent.Add(_SentDetails[i].FileName);

                                    }

                                }

                                //Remove Batch file from List
                                _sent.Remove(msgDetails.SourceFile);
                                _failed.Remove(msgDetails.SourceFile);



                                Data.DbTransaction _db = new IAPL.Transport.Data.DbTransaction();
                                System.Data.DataTable _dt = _db.GetEmailInfo(msgDetails.MessageCode, msgDetails.ERP);

                                string _xsltPath = string.Empty;
                                if (_dt.Rows.Count != 0)
                                {

                                    desServerDetails.EmailAddressBCC = _dt.Rows[0]["emldIntEmailAddrBCC"].ToString();
                                    desServerDetails.EmailAddressCC = _dt.Rows[0]["emldIntEmailAddrCC"].ToString();
                                    desServerDetails.EmailAddressFrom = _dt.Rows[0]["emldIntEmailAddrFROM"].ToString();
                                    desServerDetails.EmailAddressTo = _dt.Rows[0]["emldIntEmailAddrTO"].ToString();
                                    desServerDetails.EmailSubject = _dt.Rows[0]["emldEmailSubject"].ToString();
                                    _xsltPath = _dt.Rows[0]["emldXSLTPath"].ToString();
                                }
                                //success
                                _mailSent = emailTrans.GenerateHTML(_sent, _failed, msgDetails.BackupFolder, _xsltPath, desServerDetails, msgDetails, _SentDetails, false);

                            }
                            else
                            {
                                emailTrans.SourceFile = fileName;

                                //OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                                //emailTrans.OutputFile = desServerDetails.GenFileName();

                                //NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                                emailTrans.OutputFile = desServerDetails.DesFileName;
                            }

                            emailTrans.SourceFolder = srcServerDetails.GetSourceFolder("");


                            // Do not send Notification for the Terminator file under Condition 5
                            bool PerformEmail = true;

                            string fileExt = fileName.Substring(fileName.LastIndexOf(".") + 1);

                            string terminatorExt = msgDetails.FITEFileMask.Substring(msgDetails.FITEFileMask.LastIndexOf(".") + 1);

                            //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                            //if (msgDetails.ProcessCode.Equals(5) &&
                            //    fileExt.Equals(terminatorExt))

                            //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                            if (msgDetails.ProcessCode.Equals(5) &&
                                string.Equals(fileExt, terminatorExt, StringComparison.OrdinalIgnoreCase))
                            { PerformEmail = false; }

                            if (msgDetails.ProcessCode.Equals(7))
                            { //mail already sent
                                if (!_mailSent)
                                {
                                    errorMessage = threadName + " [\'" + fileName + "\' file has been backup on " + msgDetails.BackupFolder + " folder but failed on transmisson to " +
                          desServerDetails.ServerAddress + " server!] " + errorMessage;
                                    IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", errorMessage);

                                    //log to db
                                    db.UpdateTransactionLog(false, "FILETRANSFER - failed.", errorMessage);

                                    // send email notification 
                                    msgDetails.ProcessLogID = db.GetProcessLogID;
                                    msgDetails.TransDescription = "FILETRANSFER - failed.";
                                    msgDetails.TechnicalErrorDescription = errorMessage;
                                    emailTrans = new EmailTransaction(msgDetails);
                                    emailTrans.DestinationFolder = desServerDetails.GetDestinationFolder(desServerDetails.ServerFolder);
                                    emailTrans.SourceFile = srcServerDetails.GetSourceFile();

                                    //OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                                    //emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);

                                    //NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                                    if (desServerDetails.TransmissionType == ConstantVariables.TransmissionMode.HTTP)
                                        emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);
                                    else
                                        emailTrans.OutputFile = desServerDetails.DesFileName;

                                    if (!emailTrans.SendEmailNotification(fileName, threadName, false))
                                    {
                                        string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailTrans.ErrorMessage);
                                        if (errorList.Length >= 2)
                                        {
                                            IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[Thread: " + threadName + "] " + errorList[1]);
                                        }
                                        else
                                        {
                                            IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                                                "[Thread: " + threadName + "] " + emailTrans.ErrorMessage);
                                        }
                                    }
                                    _mailSent = true;
                                }
                            }
                            else
                            {
                                if (PerformEmail && (!emailTrans.SendEmailNotification(fileName, threadName, true)))
                                {
                                    string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailTrans.ErrorMessage);
                                    if (errorList.Length >= 2)
                                    {
                                        IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[Thread: " + threadName + "] " + errorList[1]);
                                    }
                                    else
                                    {
                                        IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                                            "[Thread: " + threadName + "] " + emailTrans.ErrorMessage);
                                    }
                                }
                            }
                        }
                    }
                }
                    #endregion
                else
                { // failed to transmit the file
                    //log to isErrorTable

                    //log to textfile
                    if (msgDetails.ProcessCode.Equals(7))
                    {

                    }
                    else
                    {
                        errorMessage = threadName + " [\'" + fileName + "\' file has been backup on " + msgDetails.BackupFolder + " folder but failed on transmisson to " +
                      desServerDetails.ServerAddress + " server!] " + errorMessage;
                        IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", errorMessage);

                        //log to db
                        db.UpdateTransactionLog(false, "FILETRANSFER - failed.", errorMessage);

                        //BEGIN SR#33117 Ccenriquez -- Dec 15, 2009
                        //// send email notification 
                        //msgDetails.ProcessLogID = db.GetProcessLogID;
                        //msgDetails.TransDescription = "FILETRANSFER - failed.";
                        //msgDetails.TechnicalErrorDescription = errorMessage;
                        //emailTrans = new EmailTransaction(msgDetails);
                        //emailTrans.DestinationFolder = desServerDetails.GetDestinationFolder(desServerDetails.ServerFolder);
                        //emailTrans.SourceFile = srcServerDetails.GetSourceFile();

                        ////OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                        ////emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);

                        ////NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                        //if (desServerDetails.TransmissionType == ConstantVariables.TransmissionMode.HTTP)
                        //    emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);
                        //else
                        //    emailTrans.OutputFile = desServerDetails.DesFileName;

                        //if (!emailTrans.SendEmailNotification(fileName, threadName, false))
                        //{
                        //    string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailTrans.ErrorMessage);
                        //    if (errorList.Length >= 2)
                        //    {
                        //        IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[Thread: " + threadName + "] " + errorList[1]);
                        //    }
                        //    else
                        //    {
                        //        IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                        //            "[Thread: " + threadName + "] " + emailTrans.ErrorMessage);
                        //    }
                        //}
                        //END SR#33117 Ccenriquez -- Dec 15, 2009
                    }

                }
            }
            else
            {
                #region AL
                //OLDCODE : SR#33041 : 10.30.2009 : Peng / Cez
                //errorMessage = threadName + " [Failed to backup the file " + fileName + " from " +
                //    srcServerDetails.ServerAddress + " to " +
                //    desServerDetails.ServerAddress + " server!] " + errorMessage;

                //NEWCODE : SR#33041 : 10.30.2009 : Peng / Cez
                errorMessage = threadName + " [Failed to backup the file " + fileName + " from " +
                    srcServerDetails.ServerAddress + " to " +
                    msgDetails.BackupFolder + " server!] " + errorMessage;


                IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", errorMessage);
                db.UpdateTransactionLog(false, "FILETRANSFER - failed.", errorMessage); // change parameter to reflect the failed trans

                // send email notification 
                msgDetails.ProcessLogID = db.GetProcessLogID;
                msgDetails.TransDescription = "FILETRANSFER - failed.";
                msgDetails.TechnicalErrorDescription = errorMessage;
                emailTrans = new EmailTransaction(msgDetails);

                emailTrans.DestinationFolder = desServerDetails.GetDestinationFolder(desServerDetails.ServerFolder);
                emailTrans.SourceFile = srcServerDetails.GetSourceFile();

                //OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                //emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);

                //NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                if (desServerDetails.TransmissionType == ConstantVariables.TransmissionMode.HTTP)
                    emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);
                else
                    emailTrans.OutputFile = desServerDetails.DesFileName;

                if (!emailTrans.SendEmailNotification(fileName, threadName, false))
                {
                    string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailTrans.ErrorMessage);
                    if (errorList.Length >= 2)
                    {
                        IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[Thread: " + threadName + "] " + errorList[1]);
                    }
                    else
                    {
                        IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                            "[Thread: " + threadName + "] " + emailTrans.ErrorMessage);
                    }
                }
                #endregion
            }

            #region // SR 33041 october 19, 2009 -- start

            //lock (IAPL.Transport.Util.GlobalVariables.htMessageCodes)
            lock (thislock)
            {
                if (IAPL.Transport.Util.GlobalVariables.htMessageCodes.ContainsKey(msgDetails.MessageCode))
                {
                    int filesToProcess = Convert.ToInt32(IAPL.Transport.Util.GlobalVariables.htMessageCodes[msgDetails.MessageCode]);
                    if (--filesToProcess > 0)
                        IAPL.Transport.Util.GlobalVariables.htMessageCodes[msgDetails.MessageCode] = filesToProcess;
                    else
                        IAPL.Transport.Util.GlobalVariables.htMessageCodes.Remove(msgDetails.MessageCode); //if MessageCode has no more Thread/File to process, remove it from htMessageCodes
                }
            }
            #endregion // SR 33041 october 19, 2009 -- start


            IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + " - done. ");

            if (db != null)
                db.Dispose();
            if (emailTrans != null)
                emailTrans.Dispose();

            dispose();
        }

        #endregion

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // *************************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        // Date: July 21, 2008
        private string DoProcess(string fileName, string threadName, IAPL.Transport.Transactions.ServerDetails sDetails, IAPL.Transport.Transactions.MessageDetails mDetails, string zipfilename, out bool success)
        {
            lock (thislock)
            {
                msgDetails.ZipCopyToDestination = sDetails.ZipCopytoDestination;
                success = true;
                string errMessage = "";
                IAPL.Transport.Transactions.FtpTransaction ftp = new FtpTransaction(sDetails);
                IAPL.Transport.Transactions.SftpTransaction sftp = new SftpTransaction(sDetails);
                IAPL.Transport.Transactions.EmailTransaction emailTrans = new EmailTransaction(sDetails, mDetails);
                IAPL.Transport.Transactions.NetTransaction netTrans2 = new NetTransaction(sDetails);

                System.Collections.Hashtable DumpFiles = new System.Collections.Hashtable();
                System.Collections.Hashtable TerminatorFiles = new System.Collections.Hashtable();

                // Create Temporary folder 
                string tempFolder = mDetails.BackupFolder.Substring(0, mDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempfolderforzip", "temp");
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempFolder))
                { System.IO.Directory.CreateDirectory(tempFolder); }

                // Create Dump folder 
                string DumpFolder = tempFolder + @"\Dump";
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(DumpFolder))
                { System.IO.Directory.CreateDirectory(DumpFolder); }

                string Temp_OrigSourceFileName = "";

                msgDetails.IMSFolder = imsfolder;

                // Get IMS Process Code
                if (mDetails.IndividualProcess.Equals(1) &&
                    (msgDetails.ProcessCode.Equals(1) ||
                    msgDetails.ProcessCode.Equals(2)) &&
                    msgDetails.ZipCopyToDestination.Equals(0))
                {
                    if (mDetails.CrashStatus.Equals(false))
                        mDetails.IMSProcessId = IMSProcessId;
                }

                if (msgDetails.ProcessCode.Equals(1))
                // -----------------------------------------------------------------------------------------------------
                #region Process Condition 1
                {
                    if (msgDetails.ZipCopyToDestination.Equals(0))
                    {
                        #region RECEIVE - TRANSMISSION

                        // Put current value of OrigSourceFilename to temp variable
                        Temp_OrigSourceFileName = srcServerDetails.OrigSourceFileName;

                        if (!mDetails.CrashStatus)
                        {
                            // Copy file from Source to Backup folder depending on which Transmission Mode to use
                            switch (sDetails.TransmissionType)
                            {
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                    #region FTP
                                    // Copy file from FTP Source to Local Backup folder 
                                    success = ftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                    if (success)
                                    {
                                        // If IMS, then set the Process Id and Status
                                        if (mDetails.IndividualProcess.Equals(1))
                                        {
                                            if (mDetails.CrashStatus.Equals(false))
                                                DBTrans.SetIMSProcessId(mDetails.MessageCode, mDetails.ERP, mDetails.Principal, mDetails.IMSProcessId);
                                        }
                                        success = PerformCondition1_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails);
                                    }

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                    #region SFTP
                                    // Copy file from FTP Source to Local Backup folder 
                                    success = sftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                    if (success)
                                    {
                                        // If IMS, then set Process to true
                                        if (mDetails.IndividualProcess.Equals(1))
                                        {
                                            if (mDetails.CrashStatus.Equals(false))
                                                DBTrans.SetIMSProcessId(mDetails.MessageCode, mDetails.ERP, mDetails.Principal, mDetails.IMSProcessId);
                                        }
                                        success = PerformCondition1_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails);
                                    }

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                    #region Network
                                    success = NetWorkCopyFilesFromSourceToDumpFolder(fileNames, mDetails, netTrans2, out DumpFiles, out TerminatorFiles, out DumpFolder);
                                    if (success)
                                    { success = PerformCondition1_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }
                                    break;
                                    #endregion
                            }
                        }

                        // Return the previous OrigSourceFilename value
                        srcServerDetails.OrigSourceFileName = Temp_OrigSourceFileName;

                        // Set Destination Flag
                        msgDetails.ZipCopyToDestination = 1;

                        #endregion
                    }
                    else
                    {
                        #region SEND - TRANSMISSION
                        if (success)
                        {
                            // Save current OrigSourceFilename value to Temp variable then set a new value                        
                            Temp_OrigSourceFileName = desServerDetails.OrigSourceFileName;

                            // Copy Zip file from Backup to Destination folder depending on the Transmission mode to use
                            switch (sDetails.TransmissionType)
                            {
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                    #region FTP
                                    // Check if IMS
                                    if (msgDetails.IndividualProcess.Equals(1))
                                        success = DoFTPSend(sDetails, listofzipfiles, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);   // If IMS
                                    else
                                        success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);        // If Not IMS

                                    //Edited New
                                    // Put failed to upload files to Failed Folder
                                    if (!FTPFileNotFoundSentList.Count.Equals(0))
                                    {
                                        // If IMS
                                        if (msgDetails.IndividualProcess.Equals(1))
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder + @"\" + msgDetails.IMSFolder);
                                        else
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder);
                                    }


                                    if (msgDetails.IndividualProcess.Equals(1))
                                    {
                                        // Perform IMS Process
                                        string IncompleteFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOf("Backup")) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                                        DetailsForIMS = new Hashtable();
                                        success = IAPL.Transport.IMS.Process.ProcessIMS(threadName, IncompleteFolder, ListOfCompleteCountriesName, ListOfIncompleteCountriesName, msgDetails, FTPFileFoundSentList, out DetailsForIMS);
                                    }

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                    #region SFTP
                                    // Check if IMS
                                    if (msgDetails.IndividualProcess.Equals(1))
                                        success = DoFTPSend(sDetails, listofzipfiles, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);   // If IMS
                                    else
                                        success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);        // If Not IMS

                                    //Edited New
                                    // Put failed to upload files to Failed Folder
                                    if (!FTPFileNotFoundSentList.Count.Equals(0))
                                    {
                                        // If IMS
                                        if (msgDetails.IndividualProcess.Equals(1))
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder + @"\" + msgDetails.IMSFolder);
                                        else
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder);
                                    }

                                    if (msgDetails.IndividualProcess.Equals(1))
                                    {
                                        // Perform IMS Process
                                        string IncompleteFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOf("Backup")) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                                        DetailsForIMS = new Hashtable();
                                        success = IAPL.Transport.IMS.Process.ProcessIMS(threadName, IncompleteFolder, ListOfCompleteCountriesName, ListOfIncompleteCountriesName, msgDetails, FTPFileFoundSentList, out DetailsForIMS);
                                    }

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                    #region Network
                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    Hashtable fileNamesClone = (Hashtable)fileNames.Clone();

                                    foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    {
                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = desServerDetails.GenFileName();
                                        else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = file.Key.ToString();
                                        else
                                            desServerDetails.DesFileName = file.Value.ToString();

                                        desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(file.Value.ToString());

                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                            desServerDetails.IncrementCounter();

                                        success = netTrans2.StartProcess(desServerDetails.OrigSourceFileName, threadName, mDetails.BackupFolder, desServerDetails);

                                        if (success)
                                        {
                                            if (fileNamesClone.ContainsValue(file.Value.ToString()))
                                                fileNamesClone.Remove(file.Key.ToString());
                                        }
                                        else
                                        {
                                            //This will be used when trying to send the file again
                                            fileNamesClone.Remove(file.Key);
                                            fileNamesClone.Add(desServerDetails.DesFileName, file.Value);
                                        }
                                    }

                                    fileNames = fileNamesClone;

                                    if (fileNames.Count > 0)
                                        errMessage = "NETWORK-StartProcess()|one or more files have not been transmitted successfully to destination";
                                    //END SR#33117 Ccenriquez / Capad -- November 17, 2009

                                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    //foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    //{
                                    //    desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(file.Value.ToString());
                                    //    success = netTrans2.StartProcess(desServerDetails.OrigSourceFileName, threadName, mDetails.BackupFolder, desServerDetails);
                                    //}
                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                                    #region Email
                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 19, 2009
                                    Hashtable fileNamesCloneEmail = (Hashtable)fileNames.Clone();

                                    foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    {
                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = desServerDetails.GenFileName();
                                        else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = file.Key.ToString();
                                        else
                                            desServerDetails.DesFileName = file.Value.ToString();

                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                            desServerDetails.IncrementCounter();

                                        success = PerformEmailProcess(file.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                        if (success)
                                        {
                                            if (fileNamesCloneEmail.ContainsValue(file.Value.ToString()))
                                                fileNamesCloneEmail.Remove(file.Key.ToString());
                                        }
                                        else
                                        {
                                            //This will be used when trying to send the file again
                                            fileNamesCloneEmail.Remove(file.Key);
                                            fileNamesCloneEmail.Add(desServerDetails.DesFileName, file.Value);
                                        }
                                    }

                                    fileNames = fileNamesCloneEmail;

                                    if (fileNames.Count > 0)
                                        errMessage = "EMAIL-StartProcess()|one or more files have not been transmitted successfully to destination";
                                    //END SR#33117 Ccenriquez / Capad -- November 19, 2009


                                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009
                                    //foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    //{
                                    //    success = PerformEmailProcess(file.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                    //    if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
                                    //    { desServerDetails.IncrementCounter(); }
                                    //}
                                    break;
                                    #endregion
                            }

                            //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 : added if success
                            //when retry to send to destination due to an error it will not go to RECIEVE process again!
                            if (success)
                            {
                                // Return the previous value of OrigSourceFilename
                                desServerDetails.OrigSourceFileName = Temp_OrigSourceFileName;

                                // UnSet Destination Flag
                                msgDetails.ZipCopyToDestination = 0;
                            }
                        }
                        #endregion
                    }
                }
                #endregion
                // -----------------------------------------------------------------------------------------------------

                else if (msgDetails.ProcessCode.Equals(2))

                // -----------------------------------------------------------------------------------------------------
                #region Process Condition 2
                {
                    // Perform Zip with password

                    if (msgDetails.ZipCopyToDestination.Equals(0))
                    {
                        #region RECEIVE - TRANSMISSION

                        // Create Temporary folder for zipping
                        tempFolder = mDetails.BackupFolder.Substring(0, mDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempfolderforzip", "temp");
                        if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempFolder))
                        { System.IO.Directory.CreateDirectory(tempFolder); }

                        string FileMasked = mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1);

                        // Copy Zip file from source to Temp folder depending on which Transmission Mode to use
                        switch (sDetails.TransmissionType)
                        {
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                #region FTP
                                // Copy file from FTP Source to Dump folder 
                                success = ftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                {
                                    // If IMS, then set Process to true
                                    if (mDetails.IndividualProcess.Equals(1))
                                    {
                                        if (mDetails.CrashStatus.Equals(false))
                                            DBTrans.SetIMSProcessId(mDetails.MessageCode, mDetails.ERP, mDetails.Principal, mDetails.IMSProcessId);
                                    }
                                    success = PerformCondition2_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails);
                                }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                #region SFTP
                                // Copy file from SFTP Source to Dump folder 
                                success = sftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                {
                                    // If IMS, then set Process to true
                                    if (mDetails.IndividualProcess.Equals(1))
                                    {
                                        if (mDetails.CrashStatus.Equals(false))
                                            DBTrans.SetIMSProcessId(mDetails.MessageCode, mDetails.ERP, mDetails.Principal, mDetails.IMSProcessId);
                                    }
                                    success = PerformCondition2_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails);
                                }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                #region Network
                                DumpFolder = "";
                                success = NetWorkCopyFilesFromSourceToDumpFolder(fileNames, mDetails, netTrans2, out DumpFiles, out TerminatorFiles, out DumpFolder);
                                if (success)
                                { success = PerformCondition2_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }

                                break;
                                #endregion
                        }

                        // Set Destination Flag
                        msgDetails.ZipCopyToDestination = 1;

                        #endregion
                    }
                    else
                    {
                        #region SEND - TRANSMISSION
                        // Save current OrigSourceFilename value to Temp variable then set a new value                        
                        Temp_OrigSourceFileName = desServerDetails.OrigSourceFileName;

                        // Copy Zip file from Backup to Destination folder depending on the Transmission mode to use
                        switch (sDetails.TransmissionType)
                        {
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                #region FTP
                                if (msgDetails.IndividualProcess.Equals(1))
                                    success = DoFTPSend(sDetails, listofzipfiles, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);   // If IMS
                                else
                                    success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);        // If Not IMS

                                //Edited New
                                // Put failed to upload files to Failed Folder
                                if (!FTPFileNotFoundSentList.Count.Equals(0))
                                {
                                    // If IMS
                                    if (msgDetails.IndividualProcess.Equals(1))
                                        PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder + @"\" + msgDetails.IMSFolder);
                                    else
                                        PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder);
                                }

                                if (msgDetails.IndividualProcess.Equals(1))
                                {
                                    // Perform IMS Process
                                    string IncompleteFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOf("Backup")) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                                    DetailsForIMS = new Hashtable();
                                    success = IAPL.Transport.IMS.Process.ProcessIMS(threadName, IncompleteFolder, ListOfCompleteCountriesName, ListOfIncompleteCountriesName, msgDetails, FTPFileFoundSentList, out DetailsForIMS);
                                }

                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                #region SFTP

                                if (msgDetails.IndividualProcess.Equals(1))
                                    success = DoFTPSend(sDetails, listofzipfiles, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);   // If IMS
                                else
                                    success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);        // If Not IMS

                                //Edited New
                                // Put failed to upload files to Failed Folder
                                if (!FTPFileNotFoundSentList.Count.Equals(0))
                                {
                                    // If IMS
                                    if (msgDetails.IndividualProcess.Equals(1))
                                        PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder + @"\" + msgDetails.IMSFolder);
                                    else
                                        PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder);
                                }

                                if (msgDetails.IndividualProcess.Equals(1))
                                {
                                    // Perform IMS Process
                                    string IncompleteFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOf("Backup")) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                                    DetailsForIMS = new Hashtable();
                                    success = IAPL.Transport.IMS.Process.ProcessIMS(threadName, IncompleteFolder, ListOfCompleteCountriesName, ListOfIncompleteCountriesName, msgDetails, FTPFileFoundSentList, out DetailsForIMS);
                                }

                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                #region Network
                                //BEGIN SR#33117 Ccenriquez / Capad -- November 17, 2009
                                Hashtable fileNamesClone = (Hashtable)fileNames.Clone();

                                foreach (DictionaryEntry file in (Hashtable)fileNames)
                                {
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = file.Key.ToString();
                                    else
                                        desServerDetails.DesFileName = file.Value.ToString();

                                    desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(file.Value.ToString());

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    success = netTrans2.StartProcess(desServerDetails.OrigSourceFileName, threadName, mDetails.BackupFolder, desServerDetails);

                                    if (success)
                                    {
                                        if (fileNamesClone.ContainsValue(file.Value.ToString()))
                                            fileNamesClone.Remove(file.Key.ToString());
                                    }
                                    else
                                    {
                                        //This will be used when trying to send the file again
                                        fileNamesClone.Remove(file.Key);
                                        fileNamesClone.Add(desServerDetails.DesFileName, file.Value);
                                    }
                                }

                                fileNames = fileNamesClone;

                                if (fileNames.Count > 0)
                                    errMessage = "NETWORK-StartProcess()|one or more files have not been transmitted successfully to destination";
                                //END SR#33117 Ccenriquez / Capad -- November 17, 2009

                                //OLD CODE SR#33117 Ccenriquez / Capad -- November 17, 2009
                                //foreach (DictionaryEntry file in (Hashtable)fileNames)
                                //{
                                //    desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(file.Value.ToString());
                                //    success = netTrans2.StartProcess(desServerDetails.OrigSourceFileName, threadName, mDetails.BackupFolder, desServerDetails);
                                //}
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                                #region Email
                                //BEGIN SR#33117 Ccenriquez / Capad -- November 19, 2009
                                Hashtable fileNamesCloneEmail = (Hashtable)fileNames.Clone();

                                foreach (DictionaryEntry file in (Hashtable)fileNames)
                                {
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = file.Key.ToString();
                                    else
                                        desServerDetails.DesFileName = file.Value.ToString();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    success = PerformEmailProcess(file.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                    if (success)
                                    {
                                        if (fileNamesCloneEmail.ContainsValue(file.Value.ToString()))
                                            fileNamesCloneEmail.Remove(file.Key.ToString());
                                    }
                                    else
                                    {
                                        //This will be used when trying to send the file again
                                        fileNamesCloneEmail.Remove(file.Key);
                                        fileNamesCloneEmail.Add(desServerDetails.DesFileName, file.Value);
                                    }
                                }

                                fileNames = fileNamesCloneEmail;

                                if (fileNames.Count > 0)
                                    errMessage = "EMAIL-StartProcess()|one or more files have not been transmitted successfully to destination";
                                //END SR#33117 Ccenriquez / Capad -- November 19, 2009

                                //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009
                                //foreach (DictionaryEntry file in (Hashtable)fileNames)
                                //{
                                //    success = PerformEmailProcess(file.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                //    if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
                                //    { desServerDetails.IncrementCounter(); }
                                //}
                                break;
                                #endregion
                        }

                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 : added if success
                        //when retry to send to destination due to an error it will not go to RECIEVE process again!
                        if (success)
                        {
                            // Return the previous value of OrigSourceFilename
                            desServerDetails.OrigSourceFileName = Temp_OrigSourceFileName;

                            // UnSet Destination Flag
                            msgDetails.ZipCopyToDestination = 0;
                        }
                        #endregion
                    }
                }
                #endregion
                // -----------------------------------------------------------------------------------------------------

                else if (msgDetails.ProcessCode.Equals(3))

                // -----------------------------------------------------------------------------------------------------
                #region Process Condition 3
                {
                    // Unzip zip file then send raw files

                    string FileMaskedExt = mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1);

                    tempFolder = mDetails.BackupFolder.Substring(0, mDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempfolderforzip", "temp");
                    string tempZipFolder = tempFolder + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempzipfolder", "tempzip");
                    if (msgDetails.ZipCopyToDestination.Equals(0))
                    {
                        #region RECEIVE - TRANSMISSION

                        // Create Temporary directory 
                        if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempFolder))
                        { System.IO.Directory.CreateDirectory(tempFolder); }

                        // Create Temporary Unzip directory for zipping                        
                        if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempZipFolder))
                        { System.IO.Directory.CreateDirectory(tempFolder); }

                        // Copy Zip file from source to Temp folder depending on which Transmission Mode to use

                        switch (sDetails.TransmissionType)
                        {
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                #region FTP
                                // Copy file from FTP Source to Dump folder 
                                success = ftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                { success = PerformCondition3_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                #region SFTP
                                // Copy file from SFTP Source to Dump folder 
                                success = sftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                { success = PerformCondition3_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                #region Network
                                DumpFolder = "";
                                success = NetWorkCopyFilesFromSourceToDumpFolder(fileNames, mDetails, netTrans2, out DumpFiles, out TerminatorFiles, out DumpFolder);
                                if (success)
                                {
                                    success = PerformCondition3_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails);
                                }

                                break;
                                #endregion
                        }

                        // Set Destination Flag
                        msgDetails.ZipCopyToDestination = 1;

                        #endregion
                    }
                    else
                    {
                        #region SEND - TRANSMISSION

                        // Copy zip file from Backup to Destination folder
                        switch (sDetails.TransmissionType)
                        {
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                #region FTP

                                FTPFileFoundSentList = new Hashtable();
                                FTPFileNotFoundSentList = null;
                                for (int ctr = 1; ctr <= 3; ctr++)
                                {
                                    Hashtable FileToProcess = new Hashtable();
                                    if (ctr.Equals(1))
                                    { FileToProcess = ExtractedFiles; }
                                    else
                                    { FileToProcess = FTPFileNotFoundSentList; }

                                    FTPFileNotFoundSentList = new Hashtable();

                                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 
                                    //success = ftp.MoveAllFilesFromListToFTP(desServerDetails.ServerFolder, FileToProcess);

                                    //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 
                                    success = ftp.MoveAllFilesFromListToFTP(desServerDetails.ServerFolder, FileToProcess, msgDetails);

                                    // Delete the files in the Thread folder
                                    foreach (DictionaryEntry file in (Hashtable)FTPFileFoundSentList)
                                    {
                                        if (System.IO.File.Exists(file.Value.ToString()))
                                        { System.IO.File.Delete(file.Value.ToString()); }
                                    }

                                    if (FTPFileNotFoundSentList.Count.Equals(0))
                                    { break; }
                                }

                                // Put failed to upload files to Failed Folder
                                if (!FTPFileNotFoundSentList.Count.Equals(0))
                                { PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, ""); }

                                //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 - clear extractedfiles only when successful!!!
                                if (success)
                                    ExtractedFiles.Clear();

                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                #region SFTP

                                FTPFileFoundSentList = new Hashtable();
                                FTPFileNotFoundSentList = null;
                                for (int ctr = 1; ctr <= 3; ctr++)
                                {
                                    Hashtable FileToProcess = new Hashtable();
                                    if (ctr.Equals(1))
                                    { FileToProcess = ExtractedFiles; }
                                    else
                                    { FileToProcess = FTPFileNotFoundSentList; }

                                    FTPFileNotFoundSentList = new Hashtable();

                                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
                                    //success = sftp.MoveAllFilesFromListToFTP(desServerDetails.ServerFolder, FileToProcess);

                                    //NEW CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
                                    success = sftp.MoveAllFilesFromListToSFTP(desServerDetails.ServerFolder, FileToProcess, msgDetails);

                                    // Delete the files in the Thread folder
                                    foreach (DictionaryEntry file in (Hashtable)FTPFileFoundSentList)
                                    {
                                        if (System.IO.File.Exists(file.Value.ToString()))
                                        { System.IO.File.Delete(file.Value.ToString()); }
                                    }

                                    if (FTPFileNotFoundSentList.Count.Equals(0))
                                    { break; }
                                }

                                // Put failed to upload files to Failed Folder
                                if (!FTPFileNotFoundSentList.Count.Equals(0))
                                { PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, ""); }

                                //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 - clear extractedfiles only when successful!!!
                                if (success)
                                    ExtractedFiles.Clear();

                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                #region Network
                                string TempOrigSourceFileName = desServerDetails.OrigSourceFileName;

                                //BEGIN SR#33117 Ccenriquez / Capad -- November 17, 2009
                                Hashtable ExtractedFilesClone = (Hashtable)ExtractedFiles.Clone();

                                foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                {
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = ExtractFiles.Key.ToString();
                                    else
                                        desServerDetails.DesFileName = ExtractFiles.Value.ToString();

                                    desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(ExtractFiles.Value.ToString());

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    success = netTrans2.backupFileFrom(ExtractFiles.Value.ToString(), desServerDetails.ServerAddress + @"\" + desServerDetails.DesFileName);

                                    if (success)
                                    {
                                        if (ExtractedFilesClone.ContainsValue(ExtractFiles.Value.ToString()))
                                            ExtractedFilesClone.Remove(ExtractFiles.Key.ToString());
                                    }
                                    else
                                    {
                                        //This will be used when trying to send the file again
                                        ExtractedFilesClone.Remove(ExtractFiles.Key);
                                        ExtractedFilesClone.Add(desServerDetails.DesFileName, ExtractFiles.Value);
                                    }
                                }

                                desServerDetails.OrigSourceFileName = TempOrigSourceFileName;

                                ExtractedFiles = ExtractedFilesClone;

                                if (ExtractedFiles.Count > 0)
                                    errMessage = "NETWORK-StartProcess()|one or more files have not been transmitted successfully to destination";
                                //END SR#33117 Ccenriquez / Capad -- November 17, 2009

                                //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
                                //string TempOrigSourceFileName = desServerDetails.OrigSourceFileName;
                                //foreach (DictionaryEntry file in (Hashtable)ExtractedFiles)
                                //{
                                //    desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(file.Value.ToString());
                                //    netTrans2.backupFileFrom(file.Value.ToString(),
                                //                             desServerDetails.ServerAddress + @"\" + desServerDetails.GenFileName());

                                //    if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
                                //    { desServerDetails.IncrementCounter(); }
                                //}
                                //desServerDetails.OrigSourceFileName = TempOrigSourceFileName;
                                //ExtractedFiles.Clear();
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                                #region Email
                                //BEGIN SR#33117 Ccenriquez / Capad -- November 17, 2009
                                Hashtable ExtractedFilesCloneEmail = (Hashtable)ExtractedFiles.Clone();

                                foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                {
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = ExtractFiles.Key.ToString();
                                    else
                                        desServerDetails.DesFileName = ExtractFiles.Value.ToString();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    success = PerformEmailProcess(ExtractFiles.Value.ToString(), tempZipFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                    if (success)
                                    {
                                        if (ExtractedFilesCloneEmail.ContainsValue(ExtractFiles.Value.ToString()))
                                            ExtractedFilesCloneEmail.Remove(ExtractFiles.Key.ToString());
                                    }
                                    else
                                    {
                                        //This will be used when trying to send the file again
                                        ExtractedFilesCloneEmail.Remove(ExtractFiles.Key);
                                        ExtractedFilesCloneEmail.Add(desServerDetails.DesFileName, ExtractFiles.Value);
                                    }
                                }

                                ExtractedFiles = ExtractedFilesCloneEmail;

                                if (ExtractedFiles.Count > 0)
                                    errMessage = "EMAIL-StartProcess()|one or more files have not been transmitted successfully to destination";
                                //END SR#33117 Ccenriquez / Capad -- November 17, 2009

                                //foreach (DictionaryEntry file in (Hashtable)ExtractedFiles)
                                //{
                                //    success = PerformEmailProcess(file.Value.ToString(), tempZipFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                //    if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
                                //    { desServerDetails.IncrementCounter(); }
                                //}
                                break;
                                #endregion
                        }

                        //DON'T DELETE THE ZIP FOLDER YET IF THERE IS AN ERROR!!! SR#33117 Ccenriquez / Capad -- November 17, 2009
                        if (ExtractedFiles.Count == 0)
                        {
                            // Delete Temporary Thread Zip folder
                            foreach (DictionaryEntry ThreadFolder in (Hashtable)ThreadZipFolders)
                            {
                                //BUG FIXED ERROR ADDED IF EXISTS CONDITION SR#33117 Ccenriquez / Capad -- November 17, 2009
                                if (Directory.Exists(ThreadFolder.Value.ToString()))
                                    System.IO.Directory.Delete(ThreadFolder.Value.ToString(), true);
                            }

                            // UnSet Destination Flag
                            msgDetails.ZipCopyToDestination = 0;
                        }

                        #endregion
                    }
                }
                #endregion
                // -----------------------------------------------------------------------------------------------------

                if (msgDetails.ProcessCode.Equals(5))

                // -----------------------------------------------------------------------------------------------------
                #region Process Condition 5
                {
                    // Move files
                    switch (sDetails.TransmissionType)
                    {
                        case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                            #region FTP
                            if (msgDetails.ZipCopyToDestination.Equals(0))
                            {
                                #region RECEIVE - TRANSMISSION
                                string TerminatorToInclude = "";
                                if (!msgDetails.FITEFileMask.Equals(string.Empty))
                                { TerminatorToInclude = fileName.Substring(0, fileName.LastIndexOf(".")) + msgDetails.FITEFileMask.Substring(msgDetails.FITEFileMask.LastIndexOf(".")); }

                                success = ftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, TerminatorToInclude, DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                {
                                    netTrans2.ProcessCode = msgDetails.ProcessCode;
                                    success = PerformCondition5_Recv(DumpFolder + @"\" + fileName, netTrans2, mDetails);
                                }
                                msgDetails.ZipCopyToDestination = 1;
                                #endregion
                            }
                            else
                            {
                                #region SEND - TRANSMISSION
                                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                //if (fileName.Substring(fileName.LastIndexOf(".") + 1) != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1))

                                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                if (fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower() != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1).ToLower())
                                {
                                    // Set Terminator Flag
                                    if (!mDetails.FITEFileMask.Equals(string.Empty))
                                    { ftp.WithTerminator = true; }
                                    else
                                    { ftp.WithTerminator = false; }

                                    // Set Single Flag
                                    netTrans2.FileSentSingle = msgDetails.FilesSentSingle;

                                    //SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.FileNamingConvention == string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();
                                    //else
                                    //{
                                    //    //WARNING!!! THIS IS TO RECREATE THE SCENARIO TO THE PROD
                                    //    IAPL.Transport.Util.TextLogger.LogError("POTENTIAL FILE COUNTER OVERLAPPING: ", " MessageCode--->" + sDetails.MessageCode +
                                    //            " desServerDetails.DesFileName--->" + desServerDetails.DesFileName +
                                    //            " sDetails.CountSendAttempt--->" + sDetails.CountSendAttempt.ToString() +
                                    //            " Details.FileNamingConvention--->" + sDetails.FileNamingConvention +
                                    //            " sDetails.FileNamingConvention.IndexOf(\"<CTR>\", StringComparison.OrdinalIgnoreCase)--->" + sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase).ToString());                                                
                                    //}

                                    // Copy file from Backup to Destination folder 
                                    success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);

                                    // Put failed to upload files to Failed Folder
                                    if (!FTPFileNotFoundSentList.Count.Equals(0))
                                    { PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder); }

                                    ////BEGIN SR#33117 Ccenriquez / Capad -- November 16, 2009
                                    //if (sDetails.FileNamingConvention == string.Empty)
                                    //    desServerDetails.DesFileName = fileName;

                                    //if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                    //{
                                    //    desServerDetails.FileCounter += CtrFilesProcessed;
                                    //    desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    //    desServerDetails.IncrementCounter();

                                    //    if (++CtrFilesProcessed >= sDetails.TotalFiles)
                                    //        CtrFilesProcessed = 0; //reset counter
                                    //}
                                    ////END SR#33117 Ccenriquez / Capad -- November 16, 2009
                                }

                                //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 : added if success
                                //when retry to send to destination due to an error it will not go to RECIEVE process again!
                                if (success)
                                    msgDetails.ZipCopyToDestination = 0;
                                #endregion
                            }
                            break;
                            #endregion

                        case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                            #region SFTP
                            if (msgDetails.ZipCopyToDestination.Equals(0))
                            {
                                #region RECEIVE - TRANSMISSION
                                string TerminatorToInclude = "";
                                if (!msgDetails.FITEFileMask.Equals(string.Empty))
                                { TerminatorToInclude = fileName.Substring(0, fileName.LastIndexOf(".")) + msgDetails.FITEFileMask.Substring(msgDetails.FITEFileMask.LastIndexOf(".")); }

                                // Copy file from SFTP Source to Dump folder 
                                success = sftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, TerminatorToInclude, DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                {
                                    netTrans2.ProcessCode = msgDetails.ProcessCode;
                                    success = PerformCondition5_Recv(DumpFolder + @"\" + fileName, netTrans2, mDetails);
                                }
                                msgDetails.ZipCopyToDestination = 1;
                                #endregion
                            }
                            else
                            {
                                #region SEND - TRANSMISSION
                                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                //if (fileName.Substring(fileName.LastIndexOf(".") + 1) != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1))

                                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                if (fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower() != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1).ToLower())
                                {
                                    // Set Terminator Flag
                                    if (!mDetails.FITEFileMask.Equals(string.Empty))
                                    { sftp.WithTerminator = true; }
                                    else
                                    { sftp.WithTerminator = false; }

                                    // Set Single Flag
                                    netTrans2.FileSentSingle = msgDetails.FilesSentSingle;

                                    //SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.FileNamingConvention == string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    // Copy file from Backup to Destination folder 
                                    success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);

                                    // Put failed to upload files to Failed Folder
                                    if (!FTPFileNotFoundSentList.Count.Equals(0))
                                    { PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder); }
                                }

                                //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 : added if success
                                //when retry to send to destination due to an error it will not go to RECIEVE process again!
                                if (success)
                                    msgDetails.ZipCopyToDestination = 0;
                                #endregion
                            }
                            break;
                            #endregion

                        case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                            #region Network
                            if (msgDetails.ZipCopyToDestination.Equals(0))
                            {
                                #region RECEIVE - TRANSMISSION
                                // Send file to Dump folder
                                DumpFolder = "";
                                bool Terminator = false;
                                success = NetWorkCopyTheFileFromSourceToDumpFolder(fileName, mDetails, netTrans2, out DumpFolder, out Terminator);
                                if (success)
                                {
                                    //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                    //if (fileName.Substring(fileName.LastIndexOf(".") + 1) != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1))

                                    //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                    if (fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower() != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1).ToLower())
                                    {
                                        netTrans2.ProcessCode = msgDetails.ProcessCode;
                                        success = PerformCondition5_Recv(DumpFolder + @"\" + fileName, netTrans2, mDetails);
                                    }
                                }
                                msgDetails.ZipCopyToDestination = 1;
                                #endregion
                            }
                            else
                            {
                                #region SEND - TRANSMISSION
                                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                //if (fileName.Substring(fileName.LastIndexOf(".") + 1) != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1))

                                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                                if (fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower() != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1).ToLower())
                                {
                                    // Set Terminator Flag
                                    if (!mDetails.FITEFileMask.Equals(string.Empty))
                                    { netTrans2.WithTerminator = true; }
                                    else
                                    { netTrans2.WithTerminator = false; }

                                    // Set Single Flag
                                    netTrans2.FileSentSingle = msgDetails.FilesSentSingle;

                                    //SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.FileNamingConvention == string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    // Copy file from Backup to Destination folder 
                                    success = netTrans2.StartProcess(fileName, threadName, mDetails.BackupFolder, desServerDetails);

                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 13, 2009
                                    //if (sDetails.FileNamingConvention == string.Empty)
                                    //    desServerDetails.DesFileName = fileName;

                                    //if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                    //{
                                    //    desServerDetails.FileCounter += CtrFilesProcessed;
                                    //    desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    //    desServerDetails.IncrementCounter();

                                    //    if (++CtrFilesProcessed >= sDetails.TotalFiles)
                                    //        CtrFilesProcessed = 0; //reset counter
                                    //}
                                    //END SR#33117 Ccenriquez / Capad -- November 13, 2009
                                }

                                //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 : added if success
                                //when retry to send to destination due to an error it will not go to RECIEVE process again!
                                if (success)
                                    msgDetails.ZipCopyToDestination = 0;
                                #endregion
                            }
                            break;
                            #endregion

                        case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                            #region Email
                            //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                            //if (fileName.Substring(fileName.LastIndexOf(".") + 1) != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1))

                            //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                            if (fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower() != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1).ToLower())
                            {
                                //SR#33117 Ccenriquez / Capad -- November 17, 2009
                                if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                    desServerDetails.DesFileName = desServerDetails.GenFileName();
                                else if (sDetails.FileNamingConvention == string.Empty)
                                    desServerDetails.DesFileName = desServerDetails.GenFileName();

                                if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                    desServerDetails.IncrementCounter();

                                success = PerformEmailProcess(fileName, mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                //SR#33117 Ccenriquez / Capad -- November 17, 2009
                                //if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
                                //{
                                //    desServerDetails.IncrementCounter();
                                //}
                            }
                            break;
                            #endregion
                        case IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP:

                            #region HTTP
                            if (msgDetails.ZipCopyToDestination.Equals(0))
                            {

                                //PLACE HOLDER
                            }
                            else
                            {
                                //SEND
                                #region SEND - TRANSMISSION
                                success = _http.POSTXML(sDetails.ServerAddress, mDetails.BackupFolder + "\\" + fileName, sDetails.ServerUserName, sDetails.ServerPassword, mDetails.FITEFileMask);
                                msgDetails.ZipCopyToDestination = 0;

                                #endregion
                            }
                            break;
                            #endregion
                    }
                }
                #endregion
                // -----------------------------------------------------------------------------------------------------

                else if (msgDetails.ProcessCode.Equals(4) ||
                         msgDetails.ProcessCode.Equals(6))

                // -----------------------------------------------------------------------------------------------------
                #region Process Condition 4 & 6
                {
                    if (msgDetails.ZipCopyToDestination.Equals(0))
                    {
                        #region RECEIVE - TRANSMISSION

                        switch (sDetails.TransmissionType)
                        {
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                #region FTP
                                // Copy file from FTP Source to Dump folder 
                                success = ftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                { success = PerformCondition4And6_Recv(DumpFolder, tempFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                #region SFTP
                                // Copy file from FTP Source to Dump folder 
                                success = sftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                if (success)
                                { success = PerformCondition4And6_Recv(DumpFolder, tempFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                #region Network
                                DumpFolder = "";
                                success = NetWorkCopyFilesFromSourceToDumpFolder(fileNames, mDetails, netTrans2, out DumpFiles, out TerminatorFiles, out DumpFolder);
                                if (success)
                                { success = PerformCondition4And6_Recv(DumpFolder, tempFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }
                                break;
                                #endregion
                        }

                        msgDetails.ZipCopyToDestination = 1;

                        #endregion
                    }
                    else
                    {
                        #region SEND - TRANSMISSION

                        Temp_OrigSourceFileName = desServerDetails.OrigSourceFileName;
                        desServerDetails.OrigSourceFileName = mDetails.ZipFileToCopy;

                        // Copy zip file from Backup to Destination folder
                        switch (sDetails.TransmissionType)
                        {
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                #region FTP
                                if (mDetails.FilesSentBatch)
                                {
                                    //SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.FileNamingConvention == string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();
                                    //else
                                    //{
                                    //    //WARNING!!! THIS IS TO RECREATE THE SCENARIO TO THE PROD
                                    //    IAPL.Transport.Util.TextLogger.LogError("POTENTIAL FILE COUNTER OVERLAPPING: ", " MessageCode--->" + sDetails.MessageCode +
                                    //            " desServerDetails.DesFileName--->" + desServerDetails.DesFileName +
                                    //            " sDetails.CountSendAttempt--->" + sDetails.CountSendAttempt.ToString() +
                                    //            " Details.FileNamingConvention--->" + sDetails.FileNamingConvention +
                                    //            " sDetails.FileNamingConvention.IndexOf(\"<CTR>\", StringComparison.OrdinalIgnoreCase)--->" + sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase).ToString());
                                    //}

                                    success = ftp.StartProcess(desServerDetails.OrigSourceFileName, fileNames, listofincompletefiles, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);

                                    ////BEGIN SR#33117 Ccenriquez / Capad -- November 16, 2009
                                    //if (sDetails.FileNamingConvention == string.Empty)
                                    //    desServerDetails.DesFileName = fileName;

                                    //if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                    //{
                                    //    desServerDetails.FileCounter += CtrFilesProcessed;
                                    //    desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    //    desServerDetails.IncrementCounter();

                                    //    if (++CtrFilesProcessed >= sDetails.TotalFiles)
                                    //        CtrFilesProcessed = 0; //reset counter
                                    //}
                                    ////END SR#33117 Ccenriquez / Capad -- November 16, 2009
                                }
                                else
                                {
                                    string tempOrigSourceFileName = desServerDetails.OrigSourceFileName;

                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 13, 2009
                                    Hashtable ExtractedFilesClone = (Hashtable)ExtractedFiles.Clone();

                                    foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                    {
                                        //SR#33117 Ccenriquez / Capad -- November 12, 2009
                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = desServerDetails.GenFileName();
                                        else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = ExtractFiles.Key.ToString();
                                        else
                                            desServerDetails.DesFileName = ExtractFiles.Value.ToString();

                                        string FileToUpload = ExtractFiles.Value.ToString();
                                        desServerDetails.OrigSourceFileName = FileToUpload;

                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                            desServerDetails.IncrementCounter();

                                        success = DoFTPSend(sDetails, ExtractedFiles, FileToUpload, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);

                                        if (success)
                                        {
                                            if (ExtractedFilesClone.ContainsValue(ExtractFiles.Value.ToString()))
                                                ExtractedFilesClone.Remove(ExtractFiles.Key.ToString());
                                        }
                                        else
                                        {
                                            //This will be used when trying to send the file again
                                            ExtractedFilesClone.Remove(ExtractFiles.Key);
                                            ExtractedFilesClone.Add(desServerDetails.DesFileName, ExtractFiles.Value);
                                        }

                                        // Put failed to upload files to Failed Folder
                                        if (!FTPFileNotFoundSentList.Count.Equals(0))
                                        { PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder); }
                                    }

                                    ExtractedFiles = ExtractedFilesClone;

                                    if (ExtractedFiles.Count > 0)
                                        errMessage = "FTP-StartProcess()|one or more files have not been transmitted successfully to destination";
                                    else
                                        desServerDetails.OrigSourceFileName = tempOrigSourceFileName;
                                    //END SR#33117 Ccenriquez / Capad -- November 12, 2009
                                }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                #region SFTP
                                if (mDetails.FilesSentBatch)
                                {
                                    //SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.FileNamingConvention == string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    success = sftp.StartProcess(desServerDetails.OrigSourceFileName, fileNames, listofincompletefiles, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                }
                                else
                                {
                                    string tempOrigSourceFileName = desServerDetails.OrigSourceFileName;

                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 13, 2009
                                    Hashtable ExtractedFilesClone = (Hashtable)ExtractedFiles.Clone();

                                    foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                    {
                                        //SR#33117 Ccenriquez / Capad -- November 12, 2009
                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = desServerDetails.GenFileName();
                                        else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = ExtractFiles.Key.ToString();
                                        else
                                            desServerDetails.DesFileName = ExtractFiles.Value.ToString();

                                        string FileToUpload = ExtractFiles.Value.ToString();
                                        desServerDetails.OrigSourceFileName = FileToUpload;

                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                            desServerDetails.IncrementCounter();

                                        success = DoFTPSend(sDetails, ExtractedFiles, FileToUpload, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);

                                        if (success)
                                        {
                                            if (ExtractedFilesClone.ContainsValue(ExtractFiles.Value.ToString()))
                                                ExtractedFilesClone.Remove(ExtractFiles.Key.ToString());
                                        }
                                        else
                                        {
                                            //This will be used when trying to send the file again
                                            ExtractedFilesClone.Remove(ExtractFiles.Key);
                                            ExtractedFilesClone.Add(desServerDetails.DesFileName, ExtractFiles.Value);
                                        }

                                        // Put failed to upload files to Failed Folder
                                        if (!FTPFileNotFoundSentList.Count.Equals(0))
                                        { PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder); }
                                    }

                                    ExtractedFiles = ExtractedFilesClone;

                                    if (ExtractedFiles.Count > 0)
                                        errMessage = "SFTP-StartProcess()|one or more files have not been transmitted successfully to destination";
                                    else
                                        desServerDetails.OrigSourceFileName = tempOrigSourceFileName;
                                    //END SR#33117 Ccenriquez / Capad -- November 12, 2009
                                }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                #region Network
                                if (mDetails.FilesSentBatch)
                                {
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.FileNamingConvention == string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    success = netTrans2.StartProcess(desServerDetails.OrigSourceFileName, threadName, mDetails.BackupFolder, desServerDetails);

                                    ////BEGIN SR#33117 Ccenriquez / Capad -- November 16, 2009
                                    //if (sDetails.FileNamingConvention == string.Empty)
                                    //    desServerDetails.DesFileName = fileName;

                                    //if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                    //{
                                    //    desServerDetails.FileCounter += CtrFilesProcessed;
                                    //    desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    //    desServerDetails.IncrementCounter();

                                    //    if (++CtrFilesProcessed >= sDetails.TotalFiles)
                                    //        CtrFilesProcessed = 0; //reset counter
                                    //}
                                    ////END SR#33117 Ccenriquez / Capad -- November 16, 2009
                                }
                                else
                                {
                                    string tempOrigSourceFileName = desServerDetails.OrigSourceFileName;

                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 13, 2009
                                    Hashtable ExtractedFilesClone = (Hashtable)ExtractedFiles.Clone();

                                    foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                    {
                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = desServerDetails.GenFileName();
                                        else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = ExtractFiles.Key.ToString();
                                        else
                                            desServerDetails.DesFileName = ExtractFiles.Value.ToString();

                                        string FileToUpload = ExtractFiles.Value.ToString();
                                        desServerDetails.OrigSourceFileName = FileToUpload;

                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                            desServerDetails.IncrementCounter();

                                        success = netTrans2.StartProcess(ExtractFiles.Value.ToString(), threadName, mDetails.BackupFolder, desServerDetails);

                                        if (success)
                                        {
                                            if (ExtractedFilesClone.ContainsValue(ExtractFiles.Value.ToString()))
                                                ExtractedFilesClone.Remove(ExtractFiles.Key.ToString());
                                        }
                                        else
                                        {
                                            //This will be used when trying to send the file again
                                            ExtractedFilesClone.Remove(ExtractFiles.Key);
                                            ExtractedFilesClone.Add(desServerDetails.DesFileName, ExtractFiles.Value);
                                        }
                                    }

                                    ExtractedFiles = ExtractedFilesClone;

                                    if (ExtractedFiles.Count > 0)
                                        errMessage = "Network-StartProcess()|one or more files have not been transmitted successfully to destination";
                                    else
                                        desServerDetails.OrigSourceFileName = tempOrigSourceFileName;
                                    //END SR#33117 Ccenriquez / Capad -- November 13, 2009
                                }
                                break;
                                #endregion

                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                                #region Email

                                //OLD CODE SR#33117 Ccenriquez / Capad -- November 17, 2009
                                //foreach (DictionaryEntry file in (Hashtable)ExtractedFiles)
                                //{
                                //    success = PerformEmailProcess(file.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                //    if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
                                //    { desServerDetails.IncrementCounter(); }
                                //}

                                //BEGIN SR#33117 Ccenriquez / Capad -- November 17, 2009
                                Hashtable ExtractedFilesClone1 = (Hashtable)ExtractedFiles.Clone();

                                foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                {
                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = desServerDetails.GenFileName();
                                    else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                        desServerDetails.DesFileName = ExtractFiles.Key.ToString();
                                    else
                                        desServerDetails.DesFileName = ExtractFiles.Value.ToString();

                                    if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                        desServerDetails.IncrementCounter();

                                    success = PerformEmailProcess(ExtractFiles.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                    if (success)
                                    {
                                        if (ExtractedFilesClone1.ContainsValue(ExtractFiles.Value.ToString()))
                                            ExtractedFilesClone1.Remove(ExtractFiles.Key.ToString());
                                    }
                                    else
                                    {
                                        //This will be used when trying to send the file again
                                        ExtractedFilesClone1.Remove(ExtractFiles.Key);
                                        ExtractedFilesClone1.Add(desServerDetails.DesFileName, ExtractFiles.Value);
                                    }
                                }

                                ExtractedFiles = ExtractedFilesClone1;

                                if (ExtractedFiles.Count > 0)
                                    errMessage = "Email-StartProcess()|one or more files have not been transmitted successfully to destination";
                                //END SR#33117 Ccenriquez / Capad -- November 13, 2009
                                break;
                                #endregion
                        }

                        // Delete Thread Zip folder
                        foreach (System.Collections.DictionaryEntry ThreadFolder in ThreadZipFolders)
                        {
                            System.IO.Directory.Delete(ThreadFolder.Value.ToString(), true);
                        }

                        //COMMENT CODE BELOW - SR#33117 Ccenriquez / Capad -- November 5, 2009
                        //ExtractedFiles.Clear();

                        //SR#33117 Ccenriquez / Capad -- November 5, 2009
                        if (mDetails.FilesSentBatch)
                        {
                            if (success)
                            {
                                ExtractedFiles.Clear();
                                ThreadZipFolders.Clear();

                                // Return original value of OrigSourceFileName
                                desServerDetails.OrigSourceFileName = Temp_OrigSourceFileName;

                                // Set Destination Flag
                                msgDetails.ZipCopyToDestination = 0;
                            }
                        }
                        else
                        {
                            if (ExtractedFiles.Count == 0)
                            {
                                ThreadZipFolders.Clear();

                                // Return original value of OrigSourceFileName
                                desServerDetails.OrigSourceFileName = Temp_OrigSourceFileName;

                                // Set Destination Flag
                                msgDetails.ZipCopyToDestination = 0;
                            }
                        }

                        #endregion
                    }
                }
                #endregion
                // -----------------------------------------------------------------------------------------------------
                //            }
                if (msgDetails.ProcessCode.Equals(7))
                // -----------------------------------------------------------------------------------------------------
                #region Process Condition 7
                {
                    if (msgDetails.ZipCopyToDestination.Equals(0))
                    {
                        #region RECEIVE - TRANSMISSION

                        // Put current value of OrigSourceFilename to temp variable
                        Temp_OrigSourceFileName = srcServerDetails.OrigSourceFileName;

                        if (!mDetails.CrashStatus)
                        {
                            // Copy file from Source to Backup folder depending on which Transmission Mode to use
                            switch (sDetails.TransmissionType)
                            {
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                    #region FTP
                                    // Copy file from FTP Source to Local Backup folder 
                                    success = ftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                    success = PerformConditionHTTPSequence(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails, sDetails);//PerformCondition1_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails);

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                    #region SFTP
                                    // Copy file from FTP Source to Local Backup folder 
                                    success = sftp.StartProcess(fileName, fileNames, listofincompletefiles, threadName, mDetails.SourceFolder, mDetails.BackupFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                                    if (success)
                                    {
                                        // If IMS, then set Process to true
                                        if (mDetails.IndividualProcess.Equals(1))
                                        {
                                            if (mDetails.CrashStatus.Equals(false))
                                                DBTrans.SetIMSProcessId(mDetails.MessageCode, mDetails.ERP, mDetails.Principal, mDetails.IMSProcessId);
                                        }
                                        success = PerformCondition1_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails);
                                    }

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                    #region Network
                                    success = NetWorkCopyFilesFromSourceToDumpFolder(fileNames, mDetails, netTrans2, out DumpFiles, out TerminatorFiles, out DumpFolder);
                                    if (success)
                                    { success = PerformCondition1_Recv(DumpFolder, DumpFiles, TerminatorFiles, netTrans2, mDetails); }
                                    break;
                                    #endregion
                            }
                        }

                        // Return the previous OrigSourceFilename value
                        srcServerDetails.OrigSourceFileName = Temp_OrigSourceFileName;

                        // Set Destination Flag
                        msgDetails.ZipCopyToDestination = 1;

                        #endregion
                    }
                    else
                    {
                        #region SEND - TRANSMISSION
                        if (success)
                        {
                            // Save current OrigSourceFilename value to Temp variable then set a new value                        
                            Temp_OrigSourceFileName = desServerDetails.OrigSourceFileName;

                            // Copy Zip file from Backup to Destination folder depending on the Transmission mode to use
                            switch (sDetails.TransmissionType)
                            {
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                    #region FTP
                                    // Check if IMS
                                    if (msgDetails.IndividualProcess.Equals(1))
                                        success = DoFTPSend(sDetails, listofzipfiles, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);   // If IMS
                                    else
                                        success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);        // If Not IMS

                                    // Put failed to upload files to Failed Folder
                                    if (!FTPFileNotFoundSentList.Count.Equals(0))
                                    {
                                        // If IMS
                                        if (msgDetails.IndividualProcess.Equals(1))
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder + @"\" + msgDetails.IMSFolder);
                                        else
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder);
                                    }


                                    if (msgDetails.IndividualProcess.Equals(1))
                                    {
                                        // Perform IMS Process
                                        string IncompleteFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOf("Backup")) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                                        DetailsForIMS = new Hashtable();
                                        success = IAPL.Transport.IMS.Process.ProcessIMS(threadName, IncompleteFolder, ListOfCompleteCountriesName, ListOfIncompleteCountriesName, msgDetails, FTPFileFoundSentList, out DetailsForIMS);
                                    }

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                    #region SFTP

                                    // Check if IMS
                                    if (msgDetails.IndividualProcess.Equals(1))
                                        success = DoFTPSend(sDetails, listofzipfiles, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);   // If IMS
                                    else
                                        success = DoFTPSend(sDetails, fileNames, fileName, threadName, mDetails.BackupFolder, desServerDetails.ServerFolder, msgDetails.ProcessCode, mDetails.FilesSentSingle, fitefiles, "", DumpFolder, out DumpFiles, out TerminatorFiles, out FTPFileFoundSentList, out FTPFileNotFoundSentList, ref errMessage);        // If Not IMS

                                    // Put failed to upload files to Failed Folder
                                    if (!FTPFileNotFoundSentList.Count.Equals(0))
                                    {
                                        // If IMS
                                        if (msgDetails.IndividualProcess.Equals(1))
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder + @"\" + msgDetails.IMSFolder);
                                        else
                                            PutFailedToUploadFilesToFailedFolder(FTPFileNotFoundSentList, mDetails.BackupFolder);
                                    }

                                    if (msgDetails.IndividualProcess.Equals(1))
                                    {
                                        // Perform IMS Process
                                        string IncompleteFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOf("Backup")) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                                        DetailsForIMS = new Hashtable();
                                        success = IAPL.Transport.IMS.Process.ProcessIMS(threadName, IncompleteFolder, ListOfCompleteCountriesName, ListOfIncompleteCountriesName, msgDetails, FTPFileFoundSentList, out DetailsForIMS);
                                    }

                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                    #region Network
                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    Hashtable fileNamesClone = (Hashtable)fileNames.Clone();

                                    foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    {
                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = desServerDetails.GenFileName();
                                        else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = file.Key.ToString();
                                        else
                                            desServerDetails.DesFileName = file.Value.ToString();

                                        desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(file.Value.ToString());

                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                            desServerDetails.IncrementCounter();

                                        success = netTrans2.StartProcess(desServerDetails.OrigSourceFileName, threadName, mDetails.BackupFolder, desServerDetails);

                                        if (success)
                                        {
                                            if (fileNamesClone.ContainsValue(file.Value.ToString()))
                                                fileNamesClone.Remove(file.Key.ToString());
                                        }
                                        else
                                        {
                                            //This will be used when trying to send the file again
                                            fileNamesClone.Remove(file.Key);
                                            fileNamesClone.Add(desServerDetails.DesFileName, file.Value);
                                        }
                                    }

                                    fileNames = fileNamesClone;

                                    if (fileNames.Count > 0)
                                        errMessage = "NETWORK-StartProcess()|one or more files have not been transmitted successfully to destination";
                                    //END SR#33117 Ccenriquez / Capad -- November 17, 2009

                                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 17, 2009
                                    //foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    //{
                                    //    desServerDetails.OrigSourceFileName = desServerDetails.getFileNameOnly(file.Value.ToString());
                                    //    success = netTrans2.StartProcess(desServerDetails.OrigSourceFileName, threadName, mDetails.BackupFolder, desServerDetails);
                                    //}
                                    break;
                                    #endregion

                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                                    #region Email
                                    //BEGIN SR#33117 Ccenriquez / Capad -- November 19, 2009
                                    Hashtable fileNamesCloneEmail = (Hashtable)fileNames.Clone();

                                    foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    {
                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = desServerDetails.GenFileName();
                                        else if (sDetails.CountSendAttempt > 0 && sDetails.FileNamingConvention != string.Empty)
                                            desServerDetails.DesFileName = file.Key.ToString();
                                        else
                                            desServerDetails.DesFileName = file.Value.ToString();

                                        if (sDetails.CountSendAttempt == 0 && sDetails.FileNamingConvention != string.Empty && sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                                            desServerDetails.IncrementCounter();

                                        success = PerformEmailProcess(file.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                        if (success)
                                        {
                                            if (fileNamesCloneEmail.ContainsValue(file.Value.ToString()))
                                                fileNamesCloneEmail.Remove(file.Key.ToString());
                                        }
                                        else
                                        {
                                            //This will be used when trying to send the file again
                                            fileNamesCloneEmail.Remove(file.Key);
                                            fileNamesCloneEmail.Add(desServerDetails.DesFileName, file.Value);
                                        }
                                    }

                                    fileNames = fileNamesCloneEmail;

                                    if (fileNames.Count > 0)
                                        errMessage = "EMAIL-StartProcess()|one or more files have not been transmitted successfully to destination";
                                    //END SR#33117 Ccenriquez / Capad -- November 19, 2009

                                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009
                                    //foreach (DictionaryEntry file in (Hashtable)fileNames)
                                    //{
                                    //    success = PerformEmailProcess(file.Value.ToString(), mDetails.BackupFolder, tempFolder, emailTrans, mDetails, out errMessage);

                                    //    if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
                                    //    { desServerDetails.IncrementCounter(); }
                                    //}
                                    break;
                                    #endregion
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP:
                                    success = SendHTTP(sDetails, mDetails, fileNames);
                                    msgDetails.ZipCopyToDestination = 0;

                                    break;
                            }

                            // Return the previous value of OrigSourceFilename
                            desServerDetails.OrigSourceFileName = Temp_OrigSourceFileName;

                            // UnSet Destination Flag
                            msgDetails.ZipCopyToDestination = 0;
                        }
                        #endregion
                    }
                }
                #endregion

                #region Increment Counter

                //BEGIN SR#33117 Ccenriquez / Capad -- November 17, 2009
                if (sDetails.CountSendAttempt == 0 &&
                    sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1 &&
                    sDetails.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND &&
                    !desServerDetails.FileNamingConvention.Equals(string.Empty) &&
                    sDetails.TransmissionType != IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP &&
                    (mDetails.FITEFileMask.ToString().Trim() == string.Empty || fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower() != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1).ToLower()))
                {
                    //EXCLUDE THIS ON MSG FILE COUNTER UPDATE : any transmission to FTP : zip source : w/ fite : zip facility
                    if ((msgDetails.ProcessCode == 4) || (msgDetails.ProcessCode == 5) || (msgDetails.ProcessCode == 6) ||
                       (msgDetails.ProcessCode == 1 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK) ||
                       (msgDetails.ProcessCode == 2 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK) ||
                       (msgDetails.ProcessCode == 3 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK) ||
                       (msgDetails.ProcessCode == 7 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK) ||
                       (msgDetails.ProcessCode == 1 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL) ||
                       (msgDetails.ProcessCode == 2 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL) ||
                       (msgDetails.ProcessCode == 3 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL) ||
                       (msgDetails.ProcessCode == 7 && sDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL))
                    {
                        DBTrans.UpdateMsgCounter(desServerDetails.MessageCode, desServerDetails.FileCounter);
                    }
                    //else
                    //{
                    //    //WARNING!!! THIS IS TO RECREATE THE SCENARIO TO THE PROD
                    //    IAPL.Transport.Util.TextLogger.LogError("POTENTIAL FILE COUNTER OVERLAPPING: ",
                    //            " MessageCode--->" + sDetails.MessageCode +
                    //            " desServerDetails.DesFileName--->" + desServerDetails.DesFileName +
                    //            " sDetails.CountSendAttempt--->" + sDetails.CountSendAttempt.ToString() +
                    //            " Details.FileNamingConvention--->" + sDetails.FileNamingConvention +
                    //            " sDetails.FileNamingConvention.IndexOf(\"<CTR>\", StringComparison.OrdinalIgnoreCase)--->" + sDetails.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase).ToString());
                    //}
                }

                #region Requeue Logic
                if (sDetails.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND &&
                    sDetails.TransmissionType != IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP &&
                    sDetails.CountSendAttempt >= 4)
                {
                    string requeueFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("RequeueFolder", "requeue");

                    try
                    {
                        RequeueFile theRequeueFile = new RequeueFile();

                        if (msgDetails.ProcessCode == 1 || msgDetails.ProcessCode == 2 || msgDetails.ProcessCode == 7)
                        {
                            if (sDetails.TransmissionType != IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP &&
                                    sDetails.TransmissionType != IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP)
                            {
                                foreach (System.Collections.DictionaryEntry file in fileNames)
                                    theRequeueFile.SaveRequeueFile(file.Value.ToString(), file.Key.ToString(), msgDetails, srcServerDetails, desServerDetails);
                            }
                        }
                        else if (msgDetails.ProcessCode == 3)
                        {
                            if (sDetails.TransmissionType != IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP &&
                                    sDetails.TransmissionType != IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP)
                            {
                                foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                    theRequeueFile.SaveRequeueFile(ExtractFiles.Value.ToString(), ExtractFiles.Key.ToString(), msgDetails, srcServerDetails, desServerDetails);
                            }
                        }
                        else if (msgDetails.ProcessCode == 4 || msgDetails.ProcessCode == 6)
                        {
                            if (msgDetails.FilesSentBatch)
                            {
                                theRequeueFile.SaveRequeueFile(desServerDetails.DesFileName, desServerDetails.DesFileName, msgDetails, srcServerDetails, desServerDetails);
                            }
                            else
                            {
                                foreach (System.Collections.DictionaryEntry ExtractFiles in ExtractedFiles)
                                    theRequeueFile.SaveRequeueFile(ExtractFiles.Value.ToString(), ExtractFiles.Key.ToString(), msgDetails, srcServerDetails, desServerDetails);
                            }
                        }
                        else if (msgDetails.ProcessCode == 5)
                        {
                            theRequeueFile.SaveRequeueFile(fileName, desServerDetails.DesFileName, msgDetails, srcServerDetails, desServerDetails);
                        }

                        //console.write requeue sucess
                        //log requeue sucess
                        errMessage = threadName + " [\'" + fileName + "\' file has been requeued on " + requeueFolder + " folder but failed on transmisson to " + desServerDetails.ServerAddress + " server!] " + errMessage;
                        IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", errMessage);
                    }
                    catch
                    {
                        //console.write requeue failed
                        //log requeue failed
                        errMessage = threadName + " [\'" + fileName + "\' file has been backup on " + msgDetails.BackupFolder + " folder but failed on requeue to " + requeueFolder + " server!] " + errMessage;
                        IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", errMessage);
                    }
                }
                #endregion
                //END SR#33117 Ccenriquez / Capad -- November 17, 2009

                ////SR#33117 Ccenriquez / Capad -- November 11, 2009 - note: don't include FITE in FileCounter!!!
                //if (fileName.Substring(fileName.LastIndexOf(".") + 1) != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1))
                //{
                //    int FinalCounter = 0;
                //    //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //    //if ((!msgDetails.ZipCopyToDestination.Equals(0)) & (!desServerDetails.FileNamingConvention.Equals(string.Empty)))

                //    //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //    if (!desServerDetails.IsRetrySendToDes && !desServerDetails.FileNamingConvention.Equals(string.Empty))
                //    {
                //        if (msgDetails.ProcessCode.Equals(5))
                //        {
                //            #region Process Code 5
                //            //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //            //if (CtrFilesProcessed.Equals(TotalFiles))

                //            //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //            if (CtrFilesProcessed.Equals(TotalFiles - fitefiles.Count))
                //            {
                //                // If Single Process
                //                if (mDetails.FilesSentSingle)
                //                {
                //                    // With Terminator (Case 1)
                //                    if (!msgDetails.FITEFileMask.Equals(string.Empty))
                //                    {
                //                        //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                        //if (msgDetails.SourceFileMask.Equals("*.*"))
                //                        //{
                //                        //    FinalCounter = desServerDetails.FileCounter + (fileNames.Count - fitefiles.Count);
                //                        //}
                //                        //else
                //                        //{
                //                        //    FinalCounter = desServerDetails.FileCounter + fileNames.Count;
                //                        //}

                //                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                        FinalCounter = desServerDetails.FileCounter;

                //                        IAPL.Transport.Util.Utility.UpdateFilenameConventionCounter(desServerDetails.MessageCode, FinalCounter.ToString());

                //                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                        ++CtrFilesProcessed;

                //                        //COMMENT CODE BELOW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009 - note : file counter will be incremented also during retry file send
                //                        //CtrFilesProcessed = 0;
                //                    }

                //                }
                //                else  // Batch Process                          
                //                {
                //                    // With Terminator (Case 2)
                //                    if (!msgDetails.FITEFileMask.Equals(string.Empty))
                //                    {
                //                        //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                        //FinalCounter = desServerDetails.FileCounter + (fileNames.Count - 1);

                //                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                        FinalCounter = desServerDetails.FileCounter;
                //                        IAPL.Transport.Util.Utility.UpdateFilenameConventionCounter(desServerDetails.MessageCode,
                //                                                                                    FinalCounter.ToString());
                //                        CtrFilesProcessed = 0;
                //                    }
                //                }
                //            }
                //            //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //            //else
                //            //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //            else if (CtrFilesProcessed < (TotalFiles - fitefiles.Count))
                //            {
                //                CtrFilesProcessed++;
                //            }
                //            #endregion
                //        }
                //        else
                //        {
                //            if (msgDetails.ProcessCode.Equals(3))
                //            {
                //                #region Process Code 3
                //                FinalCounter = desServerDetails.FileCounter + ExtractedFiles.Count;
                //                #endregion
                //            }
                //            else if (msgDetails.ProcessCode.Equals(4) | msgDetails.ProcessCode.Equals(6))
                //            {
                //                #region Process Code 4 & 6
                //                // Single
                //                if (mDetails.FilesSentSingle)
                //                {
                //                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                    //FinalCounter = desServerDetails.FileCounter + (fileNames.Count - fitefiles.Count);

                //                    //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                    FinalCounter = desServerDetails.FileCounter;
                //                }
                //                else
                //                {
                //                    // Batch
                //                    // Case 2 & 4
                //                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                    //FinalCounter = desServerDetails.FileCounter + 1;

                //                    //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                    FinalCounter = desServerDetails.FileCounter;
                //                }
                //                #endregion
                //            }
                //            else
                //            {
                //                //OLD CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                //FinalCounter = desServerDetails.FileCounter + fileNames.Count;

                //                //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009
                //                FinalCounter = desServerDetails.FileCounter;
                //            }
                //            IAPL.Transport.Util.Utility.UpdateFilenameConventionCounter(desServerDetails.MessageCode, FinalCounter.ToString());
                //        }
                //    }
                //}
                #endregion

                #region Set Error message
                // Check for the error message
                switch (sDetails.TransmissionType)
                {
                    case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                        //OLD CODE SR#33117 Ccenriquez / Capad -- November 5, 2009 
                        //errMessage = ftp.ErrorMessage;

                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 5, 2009 
                        if (errMessage.Trim().Length == 0)
                            errMessage = ftp.ErrorMessage;
                        break;

                    case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                        //OLD CODE SR#33117 Ccenriquez / Capad -- November 5, 2009 
                        //errMessage = netTrans2.ErrorMessage;

                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 5, 2009 
                        if (errMessage.Trim().Length == 0)
                            errMessage = sftp.ErrorMessage;
                        break;

                    case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:

                        //OLD CODE SR#33117 Ccenriquez / Capad -- November 9, 2009 
                        //errMessage = netTrans2.ErrorMessage;

                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 9, 2009 
                        if (errMessage.Trim().Length == 0)
                            errMessage = netTrans2.ErrorMessage;

                        break;

                    case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                        errMessage = netTrans2.ErrorMessage;
                        break;

                    case IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP:
                        errMessage = _http.ErrorMessage;
                        break;

                }
                return errMessage;
                #endregion
            }
        }

        // *************************************************************************

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        #region transfernow
        private string transferNow(string fileName, string threadName, IAPL.Transport.Transactions.ServerDetails sDetails, IAPL.Transport.Transactions.MessageDetails mDetails, string zipfilename)
        {
            bool success = true;
            string errMessage = "";

            // *************************************************************************
            // Developer: Alrazen Estrella
            // Project: ISG12152
            // Date: July 21, 2008

            string tempFolder = mDetails.BackupFolder.Substring(0, mDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempfolderforzip", "temp");

            // *************************************************************************


            switch (sDetails.TransmissionType)
            {
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    #region FTP
                    // ************************************************************************************************
                    // Developer: Alrazen Estrella
                    // Project: ISG12152
                    // Date: July 21, 2008

                    errMessage = DoProcess(fileName, threadName, sDetails, mDetails, zipfilename, out success);

                    // ************************************************************************************************

                    break;
                    #endregion

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    #region SFTP
                    // ************************************************************************************************
                    // Developer: Alrazen Estrella
                    // Project: ISG12152
                    // Date: July 21, 2008

                    IAPL.Transport.Transactions.SftpTransaction sftp = new SftpTransaction(sDetails);
                    errMessage = DoProcess(fileName, threadName, sDetails, mDetails, zipfilename, out success);

                    // ************************************************************************************************

                    //SR#33117 Ccenriquez / Capad -- November 9, 2009
                    //if (!success)
                    //    errMessage = sftp.ErrorMessage;
                    break;
                    #endregion

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                    #region Email
                    // ************************************************************************************************
                    // Developer: Alrazen Estrella
                    // Project: ISG12152
                    // Date: August 8, 2008

                    if (sDetails.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
                    {
                        errMessage = "MessageTransaction-transferNow()|Email are used for SEND tranmission type only!";
                        success = false;
                    }
                    else
                    {
                        errMessage = DoProcess(fileName, threadName, sDetails, mDetails, zipfilename, out success);
                    }

                    // ************************************************************************************************

                    break;
                    #endregion

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                    #region Network
                    // ************************************************************************************************
                    // Developer: Alrazen Estrella
                    // Project: ISG12152
                    // Date: July 21, 2008

                    errMessage = DoProcess(fileName, threadName, sDetails, mDetails, zipfilename, out success);

                    // ************************************************************************************************

                    //SR#33117 Ccenriquez / Capad -- November 9, 2009
                    //if (!success)
                    //    errMessage = NetTrans.ErrorMessage;

                    break;
                    #endregion
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP:
                    #region HTTP
                    errMessage = DoProcess(fileName, threadName, sDetails, mDetails, zipfilename, out success);
                    break;
                    #endregion

            }
            return errMessage;
        }

        #endregion

        #region dispose
        private void dispose()
        {
            if (this.msgDetails != null)
            {
                this.msgDetails = null;
            }
            if (this.srcServerDetails != null)
            {
                this.srcServerDetails = null;
            }
            if (this.desServerDetails != null)
            {
                this.desServerDetails = null;
            }
        }
        #endregion

        #endregion

        #region properties < File Terminator and Zipping Functionality >
        //LENIN - ISG11597 - ADD - 11-207-2007
        /// <summary>
        /// test
        /// </summary>
        /// <remarks>test</remarks>
        /// <value>test</value>
        public Hashtable FileNames
        {
            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    fileNames.Add(file.Key, file.Value);
                }
            }
        }

        public string FITEFileName
        {
            get
            {
                return this._FITEFileName;
            }
            set
            {
                this._FITEFileName = value;
            }
        }
        #endregion

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ***********************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12043
        // Date: Oct. 7, 2008

        public Hashtable ListOfZipFiles
        {
            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    listofzipfiles.Add(file.Key, file.Value);
                }
            }
        }

        public Hashtable ListOfIncompleteFiles
        {
            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    listofincompletefiles.Add(file.Key, file.Value);
                }
            }
        }

        public Hashtable ListOfCompleteCountriesName
        {
            get
            {
                return this.listofcompletecountriesname;
            }

            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    listofcompletecountriesname.Add(file.Key, file.Value);
                }
            }
        }

        public Hashtable ListOfIncompleteCountriesName
        {
            get
            {
                return this.listofincompletecountriesname;
            }

            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    listofincompletecountriesname.Add(file.Key, file.Value);
                }
            }
        }

        public Hashtable IMSDetails
        {
            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    imsdetails.Add(file.Key, file.Value);
                }
            }
        }

        public Hashtable NamedConventionFiles
        {
            get
            { return this._NamedConventionFiles; }

            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    _NamedConventionFiles.Add(file.Key, file.Value);
                }
            }
        }

        // ***********************************************************************

        // ***********************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        // Date: 07/07/08

        public Hashtable FiteFiles
        {
            set
            {
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    fitefiles.Add(file.Key, file.Value);
                }
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Create Guid Zip folder specifically per zip for Unzipping
        private string CreateGuidFolder(string DefaultFolder)
        {
            string GuidFolder = DefaultFolder + @"\" + Guid.NewGuid().ToString();
            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(GuidFolder))
            { System.IO.Directory.CreateDirectory(GuidFolder); }
            return GuidFolder;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Create folder
        private bool CreateFolder(string FolderPath)
        {
            bool success = true;
            try
            {
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(FolderPath))
                { System.IO.Directory.CreateDirectory(FolderPath); }
            }
            catch (Exception ex)
            {
                success = false;
                throw ex;
            }
            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Create GUID folder under TempZip folder
        private bool CreateTempZipGuidFolder(IAPL.Transport.Transactions.MessageDetails mDetails,
                                             out string tempZipGuidFolder)
        {
            bool success = true;
            tempZipGuidFolder = "";

            try
            {
                string tempFolder = mDetails.BackupFolder.Substring(0, mDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempfolderforzip", "temp");
                string tempZipFolder = tempFolder + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempzipfolder", "tempzip");

                // Create Temporary folder 
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempFolder))
                { System.IO.Directory.CreateDirectory(tempFolder); }

                // Create Temporary Zip folder for zip process
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempZipFolder))
                { System.IO.Directory.CreateDirectory(tempFolder); }

                // Create GUID folder for unzipping
                tempZipGuidFolder = CreateGuidFolder(tempZipFolder);
            }
            catch
            { success = true; }

            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Combine 2 Hashtables for output
        private System.Collections.Hashtable CombineHashtables(System.Collections.Hashtable HashTable1,
                                                               System.Collections.Hashtable HashTable2)
        {
            System.Collections.Hashtable HashTablesContent = new Hashtable();
            int Ctr = 0;
            foreach (DictionaryEntry file1 in (Hashtable)HashTable1)
            {
                Ctr++;
                HashTablesContent.Add("file" + Ctr, file1.Value);
            }

            foreach (DictionaryEntry file2 in (Hashtable)HashTable2)
            {
                Ctr++;
                HashTablesContent.Add("file" + Ctr, file2.Value);
            }

            return HashTablesContent;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Combine 2 Hashtables for output
        private System.Collections.Hashtable CombineIMSFTPFoundFilesHashtables(System.Collections.Hashtable HashTable1,
                                                                               System.Collections.Hashtable HashTable2)
        {
            System.Collections.Hashtable HashTablesContent = new Hashtable();
            foreach (DictionaryEntry file1 in (Hashtable)HashTable1)
            {
                string CountryCode = file1.Value.ToString().Substring(0, 3).ToUpper();
                HashTablesContent.Add(CountryCode, file1.Value);
            }

            foreach (DictionaryEntry file2 in (Hashtable)HashTable2)
            {
                string CountryCode = file2.Value.ToString().Substring(0, 3).ToUpper();
                HashTablesContent.Add(CountryCode, file2.Value);
            }

            return HashTablesContent;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Put a password in a zip file
        private bool PutPasswordToZipfile(string tempFolder,
                                          string filetoprocess,
                                          string ZipPassword,
                                          out string errMessage)
        {
            bool success = true;
            errMessage = "";
            string ZipName = "";
            try
            {
                // Create TempZip folder
                string tempZipFolder = tempFolder + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempzipfolder", "tempzip");
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempZipFolder))
                { System.IO.Directory.CreateDirectory(tempFolder); }

                // Create Guid Zip folder specifically per zip for Unzipping
                string tempThreadZipFolder = tempZipFolder + @"\" + Guid.NewGuid().ToString();
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempThreadZipFolder))
                { System.IO.Directory.CreateDirectory(tempThreadZipFolder); }

                // Extract zip file from Temp to Temporary Zip folder
                string ZipFileInTemp = tempFolder + @"\" + srcServerDetails.getFileNameOnly(filetoprocess);
                IAPL.Transport.Util.Utility.ExtractAllFilesInZip(ZipFileInTemp, tempThreadZipFolder);

                // Zip raw files in the Thread Zip folder, then dump to Backup folder
                string Zipfile = srcServerDetails.getFileNameOnly(filetoprocess);
                try
                {
                    IAPL.Transport.Util.Utility.ZIPAll(Zipfile,
                                                       tempThreadZipFolder,
                                                       msgDetails.BackupFolder,
                                                       ZipPassword, out ZipName);
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                    success = false;
                }

                // Delete Thread Zip folder
                System.IO.Directory.Delete(tempThreadZipFolder, true);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                success = false;
            }
            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private bool PutFailedToUploadFilesToFailedFolder(Hashtable FilesToMove,
                                                          string SourceFolder)
        {
            bool success = true;
            try
            {
                string FailedFoldername = IAPL.Transport.Configuration.Config.GetAppSettingsValue("failedtouploadfolder", "IMSSendFailed");
                string RootFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\";
                string FailedFolder = RootFolder + FailedFoldername;

                // Create Failed Folder 
                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(FailedFolder))
                { System.IO.Directory.CreateDirectory(FailedFoldername); }

                string SourceFilePath = "";
                foreach (DictionaryEntry file in (Hashtable)FilesToMove)
                {
                    if (SourceFolder.Equals(string.Empty))
                    { SourceFilePath = file.Value.ToString(); }
                    else
                    { SourceFilePath = SourceFolder + @"\" + file.Value.ToString(); }

                    string DestinationFilePath = FailedFolder + @"\" + desServerDetails.getFileNameOnly(SourceFilePath);
                    System.IO.File.Copy(SourceFilePath, DestinationFilePath, true);
                }
            }
            catch
            {
                success = false;
            }
            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // FTP Sending process
        // Send files to FTP Destination, then check FTP if file exist after sending
        // if file(s) not found, try up to 3x to upload it to FTP destination
        private bool DoFTPSend(IAPL.Transport.Transactions.ServerDetails sDetails,
                               Hashtable ProcessTheseFiles,
                               string fileName,
                               string threadName,
                               string BackupFolder,
                               string ServerFolder,
                               int ProcessCode,
                               bool FilesSentSingle,
                               Hashtable fitefiles,
                               string IncludeTermInCondition5,
                               string DumpFolder,
                               out Hashtable dumpfiles,
                               out Hashtable TerminatorFiles,
                               out Hashtable FTPFileFoundSentList,
                               out Hashtable FTPFileNotFoundSentList,
                               ref string errorMessage)
        {
            bool success = false;
            bool FileFound = false;
            //bool FileSizeEqual = false;
            dumpfiles = new Hashtable(); ;
            TerminatorFiles = new Hashtable(); ;
            FTPFileFoundSentList = new Hashtable();
            FTPFileNotFoundSentList = null;

            IAPL.Transport.Transactions.FtpTransaction ftp = new FtpTransaction(sDetails);
            IAPL.Transport.Transactions.SftpTransaction sftp = new SftpTransaction(sDetails);

            Hashtable ReturnFTPFileFoundSentList = new Hashtable();

            string SourceFolder = "";
            if (msgDetails.IndividualProcess.Equals(1))
                // 20160212 MDO 
                // Causing error on FTP upload file due to double backslashe "\\"                
                // SourceFolder = BackupFolder + @"\" + msgDetails.IMSFolder;

                // Handle the excess backslash
                SourceFolder = BackupFolder + (BackupFolder.Substring(BackupFolder.Length - 1, 1) == "\\" ? "" : @"\") + msgDetails.IMSFolder;
            else
                SourceFolder = BackupFolder;

            bool Sent = false;
            for (int ctr = 1; ctr <= 3; ctr++)
            {
                Hashtable FileToProcess = new Hashtable();
                if (ctr.Equals(1))
                { FileToProcess = ProcessTheseFiles; }
                else
                { FileToProcess = FTPFileNotFoundSentList; }

                FTPFileNotFoundSentList = new Hashtable();
                if (!Sent)
                {
                    switch (sDetails.TransmissionType)
                    {
                        case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                            #region FTP
                            //OLD CODE SR#33117 Ccenriquez / Capad -- November 12, 2009
                            //if (ftp.StartProcess(fileName, FileToProcess, listofincompletefiles, threadName, SourceFolder, ServerFolder, ProcessCode, FilesSentSingle, fitefiles, IncludeTermInCondition5, DumpFolder, msgDetails, out dumpfiles, out TerminatorFiles))
                            //{
                            //    if (msgDetails.IndividualProcess.Equals(1))
                            //    {
                            //        // If IMS
                            //        ReturnFTPFileFoundSentList.Clear();
                            //        success = ftp.CheckIMSSentFilesFromFTPServer(msgDetails, FileToProcess, ctr, out ReturnFTPFileFoundSentList, out FTPFileNotFoundSentList);

                            //        if (ReturnFTPFileFoundSentList.Count > 0)
                            //            ftp.FTPFileFoundSentList = CombineIMSFTPFoundFilesHashtables(ftp.FTPFileFoundSentList, ReturnFTPFileFoundSentList);
                            //    }
                            //}

                            //NEW CODE SR#33117 Ccenriquez / Capad -- November 12, 2009
                            success = ftp.StartProcess(fileName, FileToProcess, listofincompletefiles, threadName, SourceFolder, ServerFolder, ProcessCode, FilesSentSingle, fitefiles, IncludeTermInCondition5, DumpFolder, msgDetails, out dumpfiles, out TerminatorFiles);
                            if (success)
                            {
                                if (msgDetails.IndividualProcess.Equals(1))
                                {
                                    // If IMS
                                    ReturnFTPFileFoundSentList.Clear();
                                    success = ftp.CheckIMSSentFilesFromFTPServer(msgDetails, FileToProcess, ctr, out ReturnFTPFileFoundSentList, out FTPFileNotFoundSentList);

                                    if (ReturnFTPFileFoundSentList.Count > 0)
                                        ftp.FTPFileFoundSentList = CombineIMSFTPFoundFilesHashtables(ftp.FTPFileFoundSentList, ReturnFTPFileFoundSentList);
                                }

                                //BEGIN SR#34056 CCEnriquez -- November 26, 2009 - check to make sure if the file is really present in the destination folder
                                IAPL.Transport.Operation.FTP ftpOps = new IAPL.Transport.Operation.FTP(sDetails.ServerAddress, sDetails.ServerUserName, sDetails.ServerPassword, sDetails.ServerPort);
                                if (ftpOps.Connect())
                                {
                                    FileFound = ftpOps.FileExist((sDetails.DesFileName == string.Empty) ? fileName : sDetails.DesFileName, ServerFolder);

                                    //Jun Roxas 3.4.2015 rename uploaded file
                                    if (FileFound)
                                    {
                                        //check if fileTempExtension enabled and value present
                                        if (desServerDetails.IsUseFileTempExtension && desServerDetails.FileTempExtension.Length > 0)
                                        {
                                            string oldFileName = ServerFolder + "/" + ((sDetails.DesFileName == string.Empty) ? fileName : sDetails.DesFileName);
                                            string newFileName = oldFileName.Substring(0, oldFileName.Length - desServerDetails.FileTempExtension.Length);
                                            ftpOps.Rename(oldFileName, newFileName);
                                            FileFound = ftpOps.FileExist(newFileName, string.Empty);
                                        }
                                    }
                                }
                                //END SR#34056 CCEnriquez -- November 26, 2009 - check to make sure if the file is really present in the destination folder
                            }
                            break;
                            #endregion

                        case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                            #region SFTP
                            //success = sftp.StartProcess(fileName, FileToProcess, listofincompletefiles, threadName, BackupFolder, ServerFolder, ProcessCode, FilesSentSingle, fitefiles, IncludeTermInCondition5, DumpFolder, msgDetails, out dumpfiles, out TerminatorFiles);
                            //sftp.CheckSentFilesFromFTPServer(ProcessTheseFiles, msgDetails, ctr, out FTPFileFoundSentList, out FTPFileNotFoundSentList);

                            //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009    
                            //if (sftp.StartProcess(fileName, FileToProcess, listofincompletefiles, threadName, SourceFolder, ServerFolder, ProcessCode, FilesSentSingle, fitefiles, IncludeTermInCondition5, DumpFolder, msgDetails, out dumpfiles, out TerminatorFiles))

                            //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009  
                            success = sftp.StartProcess(fileName, FileToProcess, listofincompletefiles, threadName, SourceFolder, ServerFolder, ProcessCode, FilesSentSingle, fitefiles, IncludeTermInCondition5, DumpFolder, msgDetails, out dumpfiles, out TerminatorFiles);

                            if (success)
                            {
                                if (msgDetails.IndividualProcess.Equals(1))
                                {
                                    // If IMS
                                    ReturnFTPFileFoundSentList.Clear();
                                    success = sftp.CheckIMSSentFilesFromFTPServer(msgDetails, FileToProcess, ctr, out ReturnFTPFileFoundSentList, out FTPFileNotFoundSentList);

                                    if (ReturnFTPFileFoundSentList.Count > 0)
                                        sftp.FTPFileFoundSentList = CombineIMSFTPFoundFilesHashtables(sftp.FTPFileFoundSentList, ReturnFTPFileFoundSentList);
                                }

                                //BEGIN SR#34056 CCEnriquez -- November 26, 2009 - check to make sure if the file is really present in the destination folder
                                IAPL.Transport.Operation.SFTP sftpOps = new IAPL.Transport.Operation.SFTP(sDetails.ServerAddress, sDetails.ServerUserName, sDetails.ServerPassword, sDetails.ServerPort);
                                if (sftpOps.Connect())
                                {
                                    FileFound = sftpOps.FileExist((sDetails.DesFileName == string.Empty) ? fileName : sDetails.DesFileName, ServerFolder);

                                    //Jun Roxas 3.5.2015 rename uploaded file
                                    if (FileFound)
                                    {
                                        //check if fileTempExtension enabled and value present
                                        if (desServerDetails.IsUseFileTempExtension && desServerDetails.FileTempExtension.Length > 0)
                                        {
                                            string oldFileName = ServerFolder + "/" + ((sDetails.DesFileName == string.Empty) ? fileName : sDetails.DesFileName);
                                            string newFileName = oldFileName.Substring(0, oldFileName.Length - desServerDetails.FileTempExtension.Length);
                                            sftpOps.Rename(oldFileName, newFileName);
                                            FileFound = sftpOps.FileExist(newFileName, string.Empty);
                                        }
                                    }
                                }
                                //END SR#34056 CCEnriquez -- November 26, 2009 - check to make sure if the file is really present in the destination folder
                            }

                            break;
                            #endregion
                    }

                    if (FileFound || (FTPFileNotFoundSentList.Count.Equals(0)))
                    { break; }

                }
            }

            switch (sDetails.TransmissionType)
            {
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    FTPFileFoundSentList = ftp.FTPFileFoundSentList;
                    //SR#33117 Ccenriquez / Capad -- November 6, 2009 
                    errorMessage = ftp.ErrorMessage;
                    break;

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    //SR#33117 Ccenriquez / Capad -- November 6, 2009 
                    FTPFileFoundSentList = sftp.FTPFileFoundSentList;
                    errorMessage = sftp.ErrorMessage;
                    break;
            }

            // If not IMS
            if (!msgDetails.IndividualProcess.Equals(1) && (!fileName.Equals(string.Empty)))
            {
                string CountryCode = "";
                CountryCode = fileName.Substring(0, fileName.LastIndexOf("."));

                if (FileFound)
                {
                    FTPFileFoundSentList.Add(CountryCode, fileName);
                }
                else
                {
                    FTPFileNotFoundSentList.Add(CountryCode, fileName);
                }
            }

            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Perform Emailing process
        private bool PerformEmailProcess(string file,
                                         string SourcePath,
                                         string tempFolder,
                                         IAPL.Transport.Transactions.EmailTransaction emailTrans,
                                         IAPL.Transport.Transactions.MessageDetails mDetails,
                                         out string errMessage)
        {
            emailTrans.DestinationFolder = desServerDetails.GetSourceFolder("");
            emailTrans.SourceFile = fileName;

            //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
            //emailTrans.OutputFile = desServerDetails.GenFileName();

            //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
            emailTrans.OutputFile = desServerDetails.DesFileName;

            emailTrans.SourceFolder = srcServerDetails.GetSourceFolder("");

            bool success = true;
            errMessage = "";

            string DestinationPath = "";
            if (mDetails.ProcessCode == 3)
            { DestinationPath = file.Substring(0, file.LastIndexOf("\\")); }
            else
            { DestinationPath = mDetails.BackupFolder; }
            string DestinationFolder = DestinationPath;

            string Destinationfile = "";
            if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
            {
                // Copy Named convention file to temp
                string Sourcefile = DestinationPath + @"\" + desServerDetails.getFileNameOnly(file);
                Destinationfile = tempFolder + @"\" + emailTrans.OutputFile;
                if (File.Exists(Destinationfile))
                { File.Delete(Destinationfile); }
                File.Copy(Sourcefile, Destinationfile);
                DestinationFolder = tempFolder;
            }
            else
            { Destinationfile = DestinationFolder + @"\" + desServerDetails.getFileNameOnly(file); }

            System.Threading.Thread.Sleep(this.transferDelayToDestination);
            success = emailTrans.StartProcess(Destinationfile, threadName, DestinationFolder);

            if (!desServerDetails.FileNamingConvention.Equals(string.Empty))
            {
                if (File.Exists(Destinationfile))
                { File.Delete(Destinationfile); }
            }

            if (!success) errMessage = emailTrans.ErrorMessage;
            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Move files in the Hashtable list in source to destination 
        private bool CopyFilesInListToFolder(System.Collections.Hashtable FileListToMove,
                                             string DumpFolder,
                                             string DestinationFolder,
                                             IAPL.Transport.Transactions.NetTransaction netTrans2,
                                             string IMSFolder,
                                             string IMSRunType,
                                             DateTime IMSDate)
        {
            bool success = true;
            try
            {
                string filetocopy = "";
                foreach (DictionaryEntry FileToCopy in (Hashtable)FileListToMove)
                {
                    if (msgDetails.IndividualProcess.Equals(1))
                        filetocopy = DumpFolder + @"\" + FileToCopy.Value.ToString();       // If IMS
                    else
                        filetocopy = FileToCopy.Value.ToString();                           // If Not IMS

                    success = netTrans2.backupFileFrom(filetocopy, DestinationFolder + @"\" + srcServerDetails.getFileNameOnly(filetocopy));
                    if (!success)
                        break;

                    string FileExt = filetocopy.Substring(filetocopy.LastIndexOf(".") + 1).ToLower();
                    if (msgDetails.IndividualProcess.Equals(1) && FileExt.ToLower() == "zip")
                    {
                        string Filename = srcServerDetails.getFileNameOnly(filetocopy);
                        string IMSCountryCode = Filename.Substring(0, 3);
                        string IMSVersionNo = Filename.Substring(3, 4);
                        DBTrans.SaveIMSFileProcessed(msgDetails.IMSProcessId, msgDetails.MessageCode, msgDetails.ERP, msgDetails.Principal, IMSCountryCode, IMSVersionNo, IMSFolder, IMSRunType, IMSDate);
                    }
                }
            }
            catch
            {
                success = false;
            }
            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Dump files from source folder to dump folder
        private bool NetWorkCopyFilesFromSourceToDumpFolder(System.Collections.Hashtable fileNames,
                                                           IAPL.Transport.Transactions.MessageDetails mDetails,
                                                           IAPL.Transport.Transactions.NetTransaction netTrans2,
                                                           out System.Collections.Hashtable DumpFiles,
                                                           out System.Collections.Hashtable TerminatorFiles,
                                                           out string DumpFolder)
        {
            DumpFiles = new Hashtable();
            TerminatorFiles = new Hashtable();

            bool success = false;

            // Create Temporary folder 
            string tempFolder = mDetails.BackupFolder.Substring(0, mDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempfolderforzip", "temp");
            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempFolder))
            { System.IO.Directory.CreateDirectory(tempFolder); }

            // Create Dump folder 
            DumpFolder = tempFolder + @"\Dump";
            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(DumpFolder))
            { System.IO.Directory.CreateDirectory(DumpFolder); }

            string TerminatorExtName = mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1);

            int ctr1 = 0;
            int ctr2 = 0;
            foreach (DictionaryEntry file in (Hashtable)fileNames)
            {
                string FileToProcess = file.Value.ToString();

                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                //if (FileToProcess.Substring(FileToProcess.LastIndexOf(".") + 1) != TerminatorExtName)

                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                if (FileToProcess.Substring(FileToProcess.LastIndexOf(".") + 1).ToLower() != TerminatorExtName.ToLower())
                {
                    // Save filename to global variable OrigSourcefilename
                    srcServerDetails.OrigSourceFileName = srcServerDetails.getFileNameOnly(FileToProcess);

                    // Copy files from Source to Dump folder
                    success = netTrans2.StartProcess(srcServerDetails.OrigSourceFileName, threadName, DumpFolder, desServerDetails);

                    ctr1++;
                    DumpFiles.Add("file" + ctr1, DumpFolder + @"\" + srcServerDetails.getFileNameOnly(FileToProcess));

                    // Perform Terminator copy process
                    if (success)
                    {
                        // With Terminator
                        if (mDetails.FITEFileMask != "")
                        {
                            // Copy Terminator file from Source to Dump folder ( For Single )
                            if (mDetails.FilesSentSingle)
                            {
                                string FiteMaskFile = srcServerDetails.OrigSourceFileName.Replace(srcServerDetails.OrigSourceFileName.Substring(srcServerDetails.OrigSourceFileName.LastIndexOf(".") + 1),
                                                                                                  mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1));
                                success = netTrans2.backupFileFrom(srcServerDetails.ServerAddress + @"\" + FiteMaskFile,
                                                                   DumpFolder + @"\" + FiteMaskFile);
                                if (success)
                                {
                                    ctr2++;
                                    TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + FiteMaskFile);
                                }
                            }
                        }
                    }
                    else
                    { break; }
                }
            }

            // Copy Terminator file from Source to Dump folder ( For Batch )
            if ((success) & (mDetails.FITEFileMask != "") & (mDetails.FilesSentBatch))
            {
                success = netTrans2.backupFileFrom(FITEFileName,
                                                   DumpFolder + @"\" + srcServerDetails.getFileNameOnly(FITEFileName));

                ctr2++;
                TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + srcServerDetails.getFileNameOnly(FITEFileName));

            }
            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // Dump files from source folder to dump folder
        private bool NetWorkCopyTheFileFromSourceToDumpFolder(string Filename,
                                                              IAPL.Transport.Transactions.MessageDetails mDetails,
                                                              IAPL.Transport.Transactions.NetTransaction netTrans2,
                                                              out string DumpFolder,
                                                              out bool Terminator)
        {
            bool success = false;
            Terminator = false;

            // Create Temporary folder 
            string tempFolder = mDetails.BackupFolder.Substring(0, mDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("tempfolderforzip", "temp");
            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(tempFolder))
            { System.IO.Directory.CreateDirectory(tempFolder); }

            // Create Dump folder 
            DumpFolder = tempFolder + @"\Dump";
            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(DumpFolder))
            { System.IO.Directory.CreateDirectory(DumpFolder); }

            // Save filename to global variable OrigSourcefilename
            srcServerDetails.OrigSourceFileName = srcServerDetails.getFileNameOnly(Filename);

            // Check if file is a Terminator
            string Destination = "";

            //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
            //if (fileName.Substring(fileName.LastIndexOf(".") + 1) != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1))

            //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
            if (fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower() != mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1).ToLower())
            {
                // Not Terminator
                Destination = DumpFolder;
            }
            else
            {
                // Terminator
                Destination = mDetails.BackupFolder;
                Terminator = true;
            }

            // Copy file from Source to Destination (Dump/Backup) folder
            success = netTrans2.StartProcess(srcServerDetails.OrigSourceFileName, threadName, Destination, desServerDetails);

            return success;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // ====================================================================================================
        // Main process for condition 1


        private bool SendHTTP(ServerDetails sDetails, MessageDetails mDetails, Hashtable fileNames)
        {
            bool success = true;
            System.Data.DataTable _dt = null;
            _http = new IAPL.Transport.Operation.Http();
            IAPL.Transport.Data.DbTransaction _GetHTTP = new IAPL.Transport.Data.DbTransaction();

            try
            {
                bool _err = false;
                List<string> _SortedList = new List<string>();
                for (int i = 1; i < fileNames.Count + 1; i++)
                {
                    _SortedList.Add(fileNames["file" + i].ToString());
                }

                _SortedList.Sort();


                #region AfterSort
                for (int i = 0; i < _SortedList.Count; i++)//fileNames.Count + 1; i++)
                {
                    if (!_err)
                    {
                        _dt = _GetHTTP.GetHTTPProcess(mDetails.TradingCode, sDetails.MessageCode, mDetails.Principal, mDetails.SupplierID, _SortedList[i].ToString());//fileNames["file" + i].ToString());

                        if (_dt.Rows.Count != 0)
                        {
                            //Send File via HTTP
                            Console.WriteLine("SENDING FILE:" + _dt.Rows[0]["Path"].ToString());
                            success = _http.POSTXML(sDetails.ServerAddress, _dt.Rows[0]["Path"].ToString(), sDetails.ServerUserName, sDetails.ServerPassword, mDetails.FITEFileMask);
                            //IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + "FILE SENT: " + fileNames[i].ToString());
                            if (success)
                            {


                                if (!_SortedList[i].ToString().Equals(mDetails.SourceFile))
                                {
                                    SentDetail_BO _sent = new SentDetail_BO();
                                    _sent.DateSent = DateTime.Now.ToShortDateString() + " : " + DateTime.Now.ToLongTimeString();
                                    _sent.FileName = _SortedList[i].ToString();//_dt.Rows[0]["Path"].ToString();
                                    _SentDetails.Add(_sent);

                                }
                                //DO CLEAN UP of GUID FOLDER AND FILES.
                                #region Clean up

                                success = _GetHTTP.DeleteHTTPProcess(mDetails.TradingCode, sDetails.MessageCode, mDetails.Principal, mDetails.SupplierID, _SortedList[i].ToString());//fileNames["file" + i].ToString());
                                DeleteFile(_dt.Rows[0]["Path"].ToString());
                                if (i == _SortedList.Count - 1)//fileNames.Count)
                                {
                                    DeleteGUIDDirectory(_dt.Rows[0]["GUID"].ToString());
                                }
                                #endregion

                            }
                            else
                            {


                                #region HTTP RETRIES
                                int a = 0;
                                //Update schema info
                                _GetHTTP.UpdateHTTPProcess(sDetails.ServerAddress, sDetails.ServerUserName, sDetails.ServerPassword, _dt.Rows[0]["Path"].ToString());
                                do
                                {
                                    success = _http.POSTXML(sDetails.ServerAddress, _dt.Rows[0]["Path"].ToString(), sDetails.ServerUserName, sDetails.ServerPassword, mDetails.FITEFileMask);
                                    if (!success)
                                    {
                                        IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + _http.ErrorMessage + "RETRY ATTEMP #" + a);
                                        //Sending FAILED
                                        a++;

                                        if (a >= 3)
                                        {
                                            _err = true;
                                            success = false;
                                            string _guid = Path.GetDirectoryName(_dt.Rows[0]["Path"].ToString());

                                            _GetHTTP.UpdateAllHTTPProcess(sDetails.ServerAddress, sDetails.ServerUserName, sDetails.ServerPassword, _guid);
                                            //1. select all files in httpprocess with the same guid as _dt.Rows[0]["Path"].ToString()
                                            //Sending failed 3x -- add entry to database.

                                            //Update ProcessLog that we have an error.
                                            //Update Email Notification about the error
                                            i = _SortedList.Count + 1;


                                            //GERARD EMAIL
                                            //SEND TECHNICAL EMAIL

                                            IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + _http.ErrorMessage + "Final attempt - HttpProcess Updated");
                                            EmailTransaction _failedMail = new EmailTransaction();
                                            //bool _a = emailTrans.GenerateHTML(_sent, _failed, msgDetails.BackupFolder, _xsltPath, desServerDetails, msgDetails, _SentDetails, false);
                                            //bool techEmail = _failedMail.GenerateHTML(null, null, msgDetails.BackupFolder, null, desServerDetails, msgDetails, _SentDetails, true);

                                            //_http.ErrorMessage// = "File Sending Failed: " + sDetails.ServerAddress;
                                            break;
                                        }

                                    }
                                    else
                                    {
                                        //SENT SUCCESSFULLY
                                        //DO CLEANUP
                                        #region Clean up
                                        a = 4;
                                        _GetHTTP.DeleteHTTPProcess(mDetails.TradingCode, sDetails.MessageCode, mDetails.Principal, mDetails.SupplierID, _SortedList[i].ToString());
                                        DeleteFile(_dt.Rows[0]["Path"].ToString());
                                        if (i == _SortedList.Count - 1)
                                        {
                                            DeleteGUIDDirectory(_dt.Rows[0]["GUID"].ToString());
                                        }
                                        #endregion
                                    }

                                }
                                while (a < 3);
                                #endregion
                                //RETRIES
                            }
                        }
                    }

                }
                #endregion
                #region BEFORE SORTING
                //for (int i = 1; i < fileNames.Count + 1; i++)
                //{
                //    if (!_err)
                //    {
                //        _dt = _GetHTTP.GetHTTPProcess(mDetails.TradingCode, sDetails.MessageCode, mDetails.Principal, mDetails.SupplierID, fileNames["file" + i].ToString());

                //        if (_dt.Rows.Count != 0)
                //        {
                //            //Send File via HTTP
                //            Console.WriteLine("SENDING FILE:" + _dt.Rows[0]["Path"].ToString());
                //            success = _http.POSTXML(sDetails.ServerAddress, _dt.Rows[0]["Path"].ToString(), sDetails.ServerUserName, sDetails.ServerPassword, mDetails.FITEFileMask);
                //            //IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + "FILE SENT: " + fileNames[i].ToString());
                //            if (success)
                //            {
                //                //DO CLEAN UP of GUID FOLDER AND FILES.
                //                #region Clean up

                //                success = _GetHTTP.DeleteHTTPProcess(mDetails.TradingCode, sDetails.MessageCode, mDetails.Principal, mDetails.SupplierID, fileNames["file" + i].ToString());
                //                DeleteFile(_dt.Rows[0]["Path"].ToString());
                //                if (i == fileNames.Count)
                //                {
                //                    DeleteGUIDDirectory(_dt.Rows[0]["GUID"].ToString());
                //                }
                //                #endregion

                //            }
                //            else
                //            {


                //                #region HTTP RETRIES
                //                int a = 0;
                //                //Update schema info
                //                _GetHTTP.UpdateHTTPProcess(sDetails.ServerAddress, sDetails.ServerUserName, sDetails.ServerPassword, _dt.Rows[0]["Path"].ToString());
                //                do
                //                {
                //                    success = _http.POSTXML(sDetails.ServerAddress, _dt.Rows[0]["Path"].ToString(), sDetails.ServerUserName, sDetails.ServerPassword, mDetails.FITEFileMask);
                //                    if (!success)
                //                    {
                //                        IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + _http.ErrorMessage + "RETRY ATTEMP #" + a);
                //                        //Sending FAILED
                //                        a++;

                //                        if (a >= 3)
                //                        {
                //                            _err = true;
                //                            success = false;
                //                            string _guid = Path.GetDirectoryName(_dt.Rows[0]["Path"].ToString());

                //                            _GetHTTP.UpdateAllHTTPProcess(sDetails.ServerAddress, sDetails.ServerUserName, sDetails.ServerPassword, _guid);
                //                            //1. select all files in httpprocess with the same guid as _dt.Rows[0]["Path"].ToString()
                //                            //Sending failed 3x -- add entry to database.

                //                            //Update ProcessLog that we have an error.
                //                            //Update Email Notification about the error
                //                            i = fileNames.Count + 1;

                //                            //SEND TECHNICAL EMAIL

                //                            IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Thread " + threadName + _http.ErrorMessage + "Final attempt - HttpProcess Updated");


                //                            //_http.ErrorMessage// = "File Sending Failed: " + sDetails.ServerAddress;
                //                            break;
                //                        }

                //                    }
                //                    else
                //                    {
                //                        //SENT SUCCESSFULLY
                //                        //DO CLEANUP
                //                        #region Clean up
                //                        a = 4;
                //                        _GetHTTP.DeleteHTTPProcess(mDetails.TradingCode, sDetails.MessageCode, mDetails.Principal, mDetails.SupplierID, fileNames["file" + i].ToString());
                //                        DeleteFile(_dt.Rows[0]["Path"].ToString());
                //                        if (i == fileNames.Count)
                //                        {
                //                            DeleteGUIDDirectory(_dt.Rows[0]["GUID"].ToString());
                //                        }
                //                        #endregion
                //                    }

                //                }
                //                while (a < 3);
                //                #endregion
                //                //RETRIES
                //            }
                //        }
                //    }

                //}
                #endregion
                //success = _http.POSTXML(sDetails.ServerAddress, mDetails.BackupFolder + "\\" + fileName, sDetails.ServerUserName, sDetails.ServerPassword, mDetails.FITEFileMask);
            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }
            return success;
        }


        private void DoHTTPRetries()
        {

        }

        private void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);

                //   System.IO.File.Delete(DumpFolder + @"\" + FilenameToCopy);
            }

        }

        private void DeleteGUIDDirectory(string path)
        {
            if (IAPL.Transport.Util.CommonTools.DirectoryExist(path))
            { System.IO.Directory.Delete(path, true); }
        }
        private bool PerformConditionHTTPSequence(string DumpFolder, Hashtable DumpFiles, Hashtable TerminatorFiles, IAPL.Transport.Transactions.NetTransaction netTrans2, IAPL.Transport.Transactions.MessageDetails mDetails, IAPL.Transport.Transactions.ServerDetails sDetails)
        {
            //GERARD
            //COPY FILES TO LOCAL DUMP FOLDER AND SAVE INFO TO DATABASE.
            //EXECUTE HTTP MODULE TO SEND EACH FILE 
            //DUMP FOLDER + BACKUP FOLDER + TEMP + GUID ID
            bool success = false;
            try
            {
                System.Collections.Hashtable FilesToProcess = new System.Collections.Hashtable();
                FilesToProcess = fileNames;

                string TempFolder = CreateGuidFolder(DumpFolder + @"\HTTP");

                string _source = "";
                string _dest = "";

                string _bDest = "";
                //For Loop on the hashtable


                //LOOP INSERT TO DATBASE ALL FILES ON HASTABLE
                IAPL.Transport.Data.DbTransaction _saveProcess = new IAPL.Transport.Data.DbTransaction();
                for (int i = 1; i < FilesToProcess.Count + 1; i++)
                {

                    _bDest = mDetails.BackupFolder + "\\" + FilesToProcess["file" + i];
                    //backup source 

                    //netTrans2.StartProcess(FilesToProcess["file"+i], 
                    _source = DumpFolder + "\\" + FilesToProcess["file" + i];
                    _dest = TempFolder + "\\" + FilesToProcess["file" + i];


                    //CREATE ISG BACKUP
                    success = netTrans2.CopyFileFrom(_source, _bDest);
                    //
                    //Dump the files to GUID Folder
                    success = netTrans2.backupFileFrom(_source, _dest);


                    success = _saveProcess.SaveHTTPProcess(mDetails.TradingCode, sDetails.MessageCode, mDetails.Principal, mDetails.SourceFile, mDetails.SupplierID, FilesToProcess["file" + i].ToString(), TempFolder, _dest);


                }


            }
            catch (Exception ex)
            {
                string a = ex.Message;
            }
            return success;

        }
        private bool PerformCondition1_Recv(string DumpFolder,
                                            System.Collections.Hashtable DumpFiles,
                                            System.Collections.Hashtable TerminatorFiles,
                                            IAPL.Transport.Transactions.NetTransaction netTrans2,
                                            IAPL.Transport.Transactions.MessageDetails mDetails)
        {
            bool success = true;
            try
            {
                string DestinationFolder = "";
                System.Collections.Hashtable FilesToProcess = new System.Collections.Hashtable();

                string IMSFolder = "";
                DateTime IMSDate = DateTime.Now;
                string IMSRunType = "";
                if (mDetails.IndividualProcess.Equals(1))       // If IMS
                {
                    FilesToProcess = fileNames;

                    if (msgDetails.IMSBatchRun)
                        IMSRunType = "B";
                    else
                        IMSRunType = "M";

                    // Create IMS Folder
                    IMSFolder = imsfolder;
                    DestinationFolder = msgDetails.BackupFolder + @"\" + IMSFolder;
                    success = CreateFolder(DestinationFolder);
                }
                else                                            // Not IMS   
                {
                    FilesToProcess = DumpFiles;
                    DestinationFolder = msgDetails.BackupFolder;
                }


                if (mDetails.FileConvertionFlag)
                {
                    // Convert Files
                    foreach (DictionaryEntry FilesToCopy in (Hashtable)FilesToProcess)
                    {
                        string FilenameToCopy = srcServerDetails.getFileNameOnly(FilesToCopy.Value.ToString());

                        string IMSCountryCode = "";
                        string IMSVersionNo = "";
                        if (mDetails.IndividualProcess.Equals(1))
                        {
                            IMSCountryCode = FilenameToCopy.Substring(0, 3);
                            IMSVersionNo = FilenameToCopy.Substring(3, 4);
                        }

                        if (FilenameToCopy.Substring(FilenameToCopy.LastIndexOf(".") + 1).ToLower().Equals("zip"))
                        {
                            // Create Temp Zip Guid folder for zip processing
                            string tempZipGuidFolder = "";
                            success = CreateTempZipGuidFolder(mDetails, out tempZipGuidFolder);
                            string strDestinationFolder = tempZipGuidFolder;

                            // -----------------------------------------------------------------
                            // ISG12043 | Alrazen Estrella | Oct. 7, 2008
                            // Check Zip Validity
                            bool ZipValid = true;
                            if (mDetails.IndividualProcess.Equals(1))
                            { ZipValid = IAPL.Transport.Util.Utility.ValidateZipFile(DumpFolder + @"\" + FilenameToCopy).Equals(true); }
                            // -----------------------------------------------------------------

                            if (ZipValid)
                            {
                                // Extract zip file from Dump to TempZip GUID folder
                                IAPL.Transport.Util.Utility.ExtractAllFilesInZip(DumpFolder + @"\" + FilenameToCopy, tempZipGuidFolder);

                                // *************** PERFORM CONVERT PROCESS ****************
                                // Create Convert Folder
                                string ConvertFolder = tempZipGuidFolder + @"\" + "Convert";
                                success = CreateFolder(ConvertFolder);

                                foreach (string FilesToConvert in System.IO.Directory.GetFiles(tempZipGuidFolder))
                                {
                                    string FilenameToConvert = @"\" + srcServerDetails.getFileNameOnly(FilesToConvert);
                                    success = IAPL.Transport.Util.Utility.FileConvert(FilesToConvert,
                                                                                      ConvertFolder + FilenameToConvert,
                                                                                      mDetails.SourceCodePage, mDetails.DestinationCodePage);
                                }
                                // ********************************************************

                                // Zip raw files, send to Backup folder
                                string ZipName = "";
                                IAPL.Transport.Util.Utility.ZIPAll(FilenameToCopy,
                                                                   ConvertFolder,
                                                                   DestinationFolder,
                                                                   "",
                                                                   out ZipName);

                                // Copy Terminator files from Dump to Backup folder
                                if (mDetails.FilesSentSingle)
                                {
                                    if (!mDetails.FITEFileMask.Equals(string.Empty))
                                    {
                                        string SourceExtName = FilenameToCopy.Substring(FilenameToCopy.LastIndexOf("."));
                                        string TerminatorName = mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf("."));
                                        string TerminatorFile = FilenameToCopy.Replace(SourceExtName, TerminatorName);
                                        success = netTrans2.backupFileFrom(DumpFolder + @"\" + TerminatorFile,
                                                                           DestinationFolder + @"\" + TerminatorFile);
                                    }
                                }

                                // Delete Dump files
                                System.IO.File.Delete(DumpFolder + @"\" + FilenameToCopy);

                                if (mDetails.IndividualProcess.Equals(1))
                                    DBTrans.SaveIMSFileProcessed(mDetails.IMSProcessId, msgDetails.MessageCode, msgDetails.ERP, msgDetails.Principal, IMSCountryCode, IMSVersionNo, IMSFolder, IMSRunType, IMSDate);

                            }

                            // Delete TempZipGuid Folder
                            if (IAPL.Transport.Util.CommonTools.DirectoryExist(tempZipGuidFolder))
                            { System.IO.Directory.Delete(tempZipGuidFolder, true); }
                        }
                        else
                        {
                            // If file is not ZIP, not Terminator, and if IMS Process to use

                            //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                            //if (mDetails.IndividualProcess.Equals(1) && (!FilenameToCopy.Substring(FilenameToCopy.LastIndexOf(".")+1).Equals(IAPL.Transport.IMS.Process.IMSTerminatorExt)))

                            //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                            if (mDetails.IndividualProcess.Equals(1) && (!FilenameToCopy.Substring(FilenameToCopy.LastIndexOf(".") + 1).ToLower().Equals(IAPL.Transport.IMS.Process.IMSTerminatorExt.ToLower())))
                            {
                                success = netTrans2.backupFileFrom(DumpFolder + @"\" + FilenameToCopy,
                                                                   DestinationFolder + @"\" + FilenameToCopy);
                            }
                        }
                    }

                    // Copy Terminator files from Dump to Backup folder
                    if (mDetails.FilesSentBatch)
                    {
                        if (!mDetails.FITEFileMask.Equals(string.Empty))
                        {
                            foreach (DictionaryEntry TFile in (Hashtable)TerminatorFiles)
                            {
                                success = netTrans2.backupFileFrom(TFile.Value.ToString(),
                                                                   DestinationFolder + @"\" + srcServerDetails.getFileNameOnly(TFile.Value.ToString()));
                            }
                        }
                    }
                }
                else
                {
                    // No file convertion
                    if (TerminatorFiles.Count > 0)
                        success = CopyFilesInListToFolder(CombineHashtables(FilesToProcess, TerminatorFiles), DumpFolder, DestinationFolder, netTrans2, IMSFolder, IMSRunType, IMSDate);
                    else
                        success = CopyFilesInListToFolder(FilesToProcess, DumpFolder, DestinationFolder, netTrans2, IMSFolder, IMSRunType, IMSDate);
                }
            }
            catch
            { success = false; }
            return success;
        }
        // ====================================================================================================


        // ====================================================================================================
        // Main process for condition 2
        private bool PerformCondition2_Recv(string DumpFolder,
                                            System.Collections.Hashtable DumpFiles,
                                            System.Collections.Hashtable TerminatorFiles,
                                            IAPL.Transport.Transactions.NetTransaction netTrans2,
                                            IAPL.Transport.Transactions.MessageDetails mDetails)
        {
            bool success = true;
            bool ZipValid = true;       // ISG12043 | Alrazen Estrella | Oct. 8, 2008
            try
            {
                System.Collections.Hashtable FilesToProcess = new System.Collections.Hashtable();

                string DestinationFolder = "";
                string IMSFolder = "";
                DateTime IMSDate = DateTime.Now;
                string IMSRunType = "";
                if (mDetails.IndividualProcess.Equals(1))       // If IMS
                {
                    FilesToProcess = fileNames;

                    if (msgDetails.IMSBatchRun)
                        IMSRunType = "B";
                    else
                        IMSRunType = "M";

                    // Create IMS Folder
                    IMSFolder = imsfolder;
                    DestinationFolder = msgDetails.BackupFolder + @"\" + IMSFolder;
                    success = CreateFolder(DestinationFolder);
                }
                else                                            // Not IMS   
                {
                    FilesToProcess = DumpFiles;
                    DestinationFolder = msgDetails.BackupFolder;
                }

                foreach (DictionaryEntry FilesToCopy in (Hashtable)FilesToProcess)
                {
                    string FilenameToCopy = srcServerDetails.getFileNameOnly(FilesToCopy.Value.ToString());
                    string FileExt = FilenameToCopy.Substring(FilenameToCopy.LastIndexOf(".") + 1).ToLower();
                    if (FileExt.Equals("zip"))
                    {
                        // Zip file

                        string IMSCountryCode = "";
                        string IMSVersionNo = "";
                        if (mDetails.IndividualProcess.Equals(1))
                        {
                            IMSCountryCode = FilenameToCopy.Substring(0, 3);
                            IMSVersionNo = FilenameToCopy.Substring(3, 4);
                        }

                        // Create Temp Zip Guid folder for zip processing
                        string tempZipGuidFolder = "";
                        success = CreateTempZipGuidFolder(mDetails, out tempZipGuidFolder);

                        // -----------------------------------------------------------------
                        // ISG12043 | Alrazen Estrella | Oct. 7, 2008
                        // Check Zip Validity
                        if (mDetails.IndividualProcess.Equals(1))
                        { ZipValid = IAPL.Transport.Util.Utility.ValidateZipFile(DumpFolder + @"\" + FilenameToCopy).Equals(true); }
                        // -----------------------------------------------------------------

                        if (ZipValid)
                        {
                            // Extract zip file from Dump to TempZip GUID folder
                            IAPL.Transport.Util.Utility.ExtractAllFilesInZip(DumpFolder + @"\" + FilenameToCopy, tempZipGuidFolder);

                            string RawFilesPath = tempZipGuidFolder;

                            // *************** PERFORM CONVERT PROCESS ****************
                            if (mDetails.FileConvertionFlag)
                            {
                                // Create Convert Folder
                                string ConvertFolder = tempZipGuidFolder + @"\" + "Convert";
                                success = CreateFolder(ConvertFolder);

                                foreach (string FilesToConvert in System.IO.Directory.GetFiles(tempZipGuidFolder))
                                {
                                    string FilenameToConvert = @"\" + srcServerDetails.getFileNameOnly(FilesToConvert);
                                    success = IAPL.Transport.Util.Utility.FileConvert(FilesToConvert,
                                                                                      ConvertFolder + FilenameToConvert,
                                                                                      mDetails.SourceCodePage, mDetails.DestinationCodePage);
                                }
                                RawFilesPath = ConvertFolder;
                            }
                            // ********************************************************

                            // Zip raw files, send to Backup folder
                            string ZipName = "";
                            IAPL.Transport.Util.Utility.ZIPAll(FilenameToCopy,
                                                               RawFilesPath,
                                                               DestinationFolder,
                                                               mDetails.ZIPPassword,
                                                               out ZipName);

                            // Copy Terminator files from Dump to Backup folder
                            if (!mDetails.IndividualProcess.Equals(1))
                            {
                                // If not IMS
                                if (mDetails.FilesSentSingle)
                                {
                                    if (!mDetails.FITEFileMask.Equals(string.Empty))
                                    {
                                        string SourceExtName = FilenameToCopy.Substring(FilenameToCopy.LastIndexOf("."));
                                        string TerminatorName = mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf("."));
                                        string TerminatorFile = FilenameToCopy.Replace(SourceExtName, TerminatorName);
                                        success = netTrans2.backupFileFrom(DumpFolder + @"\" + TerminatorFile,
                                                                           DestinationFolder + @"\" + TerminatorFile);
                                    }
                                }
                            }

                            // Delete Dump file
                            System.IO.File.Delete(DumpFolder + @"\" + FilenameToCopy);

                            // Save zip files to IMS Process table
                            if (mDetails.IndividualProcess.Equals(1))
                                DBTrans.SaveIMSFileProcessed(mDetails.IMSProcessId, msgDetails.MessageCode, msgDetails.ERP, msgDetails.Principal, IMSCountryCode, IMSVersionNo, IMSFolder, IMSRunType, IMSDate);
                        }

                        // Delete TempZipGuid Folder
                        if (IAPL.Transport.Util.CommonTools.DirectoryExist(tempZipGuidFolder))
                        { System.IO.Directory.Delete(tempZipGuidFolder, true); }
                    }
                    else
                    {
                        // If Not Zip file
                        success = netTrans2.backupFileFrom(DumpFolder + @"\" + FilenameToCopy,
                                                           DestinationFolder + @"\" + FilenameToCopy);
                    }
                }

                if ((ZipValid) && (!mDetails.IndividualProcess.Equals(1)))
                {
                    // If Zip is valid and Not IMS

                    // Copy Terminator files from Dump to Backup folder
                    if (mDetails.FilesSentBatch)
                    {
                        if (!mDetails.FITEFileMask.Equals(string.Empty))
                        {
                            foreach (DictionaryEntry TFile in (Hashtable)TerminatorFiles)
                            {
                                success = netTrans2.backupFileFrom(TFile.Value.ToString(),
                                                                   DestinationFolder + @"\" + srcServerDetails.getFileNameOnly(TFile.Value.ToString()));
                            }
                        }
                    }
                }
            }
            catch
            { success = false; }
            return success;
        }
        // ====================================================================================================

        // ====================================================================================================
        // Main process for condition 3
        private bool PerformCondition3_Recv(string DumpFolder,
                                            System.Collections.Hashtable DumpFiles,
                                            System.Collections.Hashtable TerminatorFiles,
                                            IAPL.Transport.Transactions.NetTransaction netTrans2,
                                            IAPL.Transport.Transactions.MessageDetails mDetails)
        {
            bool success = true;
            try
            {
                foreach (DictionaryEntry FilesToCopy in (Hashtable)DumpFiles)
                {
                    string FilenameToCopy = srcServerDetails.getFileNameOnly(FilesToCopy.Value.ToString());

                    // Create Temp Zip Guid folder for zip processing
                    string tempZipGuidFolder = "";
                    success = CreateTempZipGuidFolder(mDetails, out tempZipGuidFolder);

                    // Store TempZipGuid folders to memory (Thread)
                    int ctr = ThreadZipFolders.Count + 1;
                    ThreadZipFolders.Add("file" + ctr.ToString(), tempZipGuidFolder);

                    // Extract zip file from Dump to TempZip GUID folder
                    IAPL.Transport.Util.Utility.ExtractAllFilesInZip(DumpFolder + @"\" + FilenameToCopy, tempZipGuidFolder);

                    string RawFilesPath = tempZipGuidFolder;

                    // *************** PERFORM CONVERT PROCESS ****************
                    if (mDetails.FileConvertionFlag)
                    {
                        // Create Convert Folder
                        string ConvertFolder = tempZipGuidFolder + @"\" + "Convert";
                        success = CreateFolder(ConvertFolder);

                        foreach (string FilesToConvert in System.IO.Directory.GetFiles(tempZipGuidFolder))
                        {
                            string FilenameToConvert = @"\" + srcServerDetails.getFileNameOnly(FilesToConvert);
                            success = IAPL.Transport.Util.Utility.FileConvert(FilesToConvert,
                                                                              ConvertFolder + FilenameToConvert,
                                                                              mDetails.SourceCodePage, mDetails.DestinationCodePage);
                        }
                        RawFilesPath = ConvertFolder;
                    }
                    // ********************************************************

                    // List all the extracted files
                    int i = ExtractedFiles.Count;
                    foreach (string ExtractFiles in System.IO.Directory.GetFiles(RawFilesPath))
                    {
                        i++;
                        ExtractedFiles.Add("file" + i.ToString(), RawFilesPath + @"\" + srcServerDetails.getFileNameOnly(ExtractFiles));
                    }

                    // Copy Zip file from Dump to Backup folder then Delete the zip in the Dump folder
                    success = netTrans2.backupFileFrom(DumpFolder + @"\" + FilenameToCopy,
                                                       mDetails.BackupFolder + @"\" + FilenameToCopy);


                    // Copy Terminator files from Dump to Backup folder
                    if (mDetails.FilesSentSingle)
                    {
                        if (!mDetails.FITEFileMask.Equals(string.Empty))
                        {
                            string SourceExtName = FilenameToCopy.Substring(FilenameToCopy.LastIndexOf("."));
                            string TerminatorName = mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf("."));
                            string TerminatorFile = FilenameToCopy.Replace(SourceExtName, TerminatorName);
                            success = netTrans2.backupFileFrom(DumpFolder + @"\" + TerminatorFile,
                                                               mDetails.BackupFolder + @"\" + TerminatorFile);
                        }
                    }
                }

                // Copy Terminator files from Dump to Backup folder
                if (mDetails.FilesSentBatch)
                {
                    if (!mDetails.FITEFileMask.Equals(string.Empty))
                    {
                        foreach (DictionaryEntry TFile in (Hashtable)TerminatorFiles)
                        { success = netTrans2.backupFileFrom(TFile.Value.ToString(), mDetails.BackupFolder + @"\" + srcServerDetails.getFileNameOnly(TFile.Value.ToString())); }
                    }
                }
            }
            catch
            { success = false; }
            return success;
        }

        // ====================================================================================================

        // ====================================================================================================
        // Main process for condition 4 and 6
        private bool PerformCondition4And6_Recv(string DumpFolder, string tempFolder,
                                                System.Collections.Hashtable DumpFiles,
                                                System.Collections.Hashtable TerminatorFiles,
                                                IAPL.Transport.Transactions.NetTransaction netTrans2,
                                                IAPL.Transport.Transactions.MessageDetails mDetails)
        {
            bool success = true;
            try
            {
                string Terminator = mDetails.FITEFileMask.Substring(mDetails.FITEFileMask.LastIndexOf(".") + 1);

                // Set Zip Password
                string ZipPWD = "";
                if (msgDetails.ProcessCode.Equals(6))
                { ZipPWD = msgDetails.ZIPPassword; }

                // Get Zip filename
                //string tempThreadZipFolder = "";
                if (mDetails.FilesSentBatch)
                {
                    if (srcServerDetails.FileNamingConvention != "")
                    {
                        // Get the given Filename Convention
                        msgDetails.ZipFileToCopy = srcServerDetails.GenFileName();
                    }
                    else
                    {
                        // If no given Filename Convention, get the 1st filename
                        msgDetails.ZipFileToCopy = "";
                    }
                }

                // If file terminator is not empty  
                if ((mDetails.SourceFile != string.Empty) && (mDetails.SourceFileMask != "*.*"))
                {
                    foreach (System.Collections.DictionaryEntry file in fitefiles)
                    {
                        int fileNamesCount = fileNames.Count + 1;
                        fileNames.Add("file" + fileNamesCount.ToString(), file.Value.ToString());
                    }
                }

                // Create Temp Zip Guid folder for zip processing
                string tempZipGuidFolder = "";
                success = CreateTempZipGuidFolder(mDetails, out tempZipGuidFolder);

                string ZipName = "";
                string RawFilesPath = tempZipGuidFolder;
                foreach (DictionaryEntry FilesToCopy in (Hashtable)DumpFiles)
                {
                    string FilenameToCopy = srcServerDetails.getFileNameOnly(FilesToCopy.Value.ToString());

                    // ===============================================
                    // Bug fix | ISG12128
                    if (msgDetails.SourceFiles.Equals(string.Empty))
                    { msgDetails.SourceFiles = FilenameToCopy; }
                    else
                    { msgDetails.SourceFiles = msgDetails.SourceFiles + ", " + FilenameToCopy; }
                    // ===============================================

                    if (mDetails.FileConvertionFlag)
                    {
                        // *************** PERFORM CONVERT PROCESS ****************
                        // Create Convert Folder
                        string ConvertFolder = tempZipGuidFolder + @"\" + "Convert";
                        success = CreateFolder(ConvertFolder);

                        // Convert file from dump then save to TempZipGuid/Convert folder
                        string FilenameToConvert = @"\" + FilenameToCopy;
                        success = IAPL.Transport.Util.Utility.FileConvert(FilesToCopy.Value.ToString(),
                                                                          ConvertFolder + FilenameToConvert,
                                                                          mDetails.SourceCodePage, mDetails.DestinationCodePage);

                        RawFilesPath = ConvertFolder;
                        // ********************************************************

                        // Delete File in Dump folder
                        System.IO.File.Delete(DumpFolder + @"\" + FilenameToCopy);
                    }
                    else
                    {
                        // Copy raw file from Dump to TempZipGuid folder
                        success = netTrans2.backupFileFrom(FilesToCopy.Value.ToString(),
                                                           tempZipGuidFolder + @"\" + FilenameToCopy);
                    }

                    if (mDetails.FilesSentSingle)
                    {
                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 11, 2009 note: this will prevent the bug inside the block!
                        if (Terminator != string.Empty)
                        {
                            // Copy Terminator file from Dump to Backup folder
                            string TerminatorFilename = FilenameToCopy.Replace(FilenameToCopy.Substring(FilenameToCopy.LastIndexOf(".") + 1), Terminator);
                            success = netTrans2.backupFileFrom(DumpFolder + @"\" + TerminatorFilename,
                                                               msgDetails.BackupFolder + @"\" + TerminatorFilename);
                        }

                        // Zip raw file from TempZipGuid folder to Backup folder
                        string Zipfile = FilenameToCopy.Substring(0, FilenameToCopy.LastIndexOf(".") + 1) + "zip";
                        IAPL.Transport.Util.Utility.ZipCreate(RawFilesPath, msgDetails.BackupFolder + @"\" + Zipfile, ZipPWD);

                        // Add to Extracted Files
                        int ECtr = ExtractedFiles.Count + 1;
                        ExtractedFiles.Add("file" + ECtr.ToString(), Zipfile);

                        // Delete File zipped in TempZipGuid folder
                        System.IO.File.Delete(RawFilesPath + @"\" + FilenameToCopy);

                        // Save the filename as Outputfile for output in Email notification
                        mDetails.ActualOutputFile = srcServerDetails.getFileNameOnly(Zipfile);
                    }
                }

                if (mDetails.FilesSentBatch)
                {
                    // Zip all files
                    IAPL.Transport.Util.Utility.ZIPAll(msgDetails.ZipFileToCopy,
                                                       RawFilesPath,
                                                       msgDetails.BackupFolder,
                                                       ZipPWD,
                                                       out ZipName);
                    int ECtr = ExtractedFiles.Count + 1;
                    ExtractedFiles.Add("file" + ECtr.ToString(), ZipName);

                    // Copy Terminator from Dump to Backup folder
                    foreach (DictionaryEntry TerminatorFile in (Hashtable)TerminatorFiles)
                    {
                        string TerminatorFileToProcess = srcServerDetails.getFileNameOnly(TerminatorFile.Value.ToString());
                        success = netTrans2.backupFileFrom(DumpFolder + @"\" + TerminatorFileToProcess,
                                                           msgDetails.BackupFolder + @"\" + TerminatorFileToProcess);
                    }
                    // Save the filename as Outputfile for output in Email notification
                    mDetails.ActualOutputFile = srcServerDetails.getFileNameOnly(ZipName);
                }

                if (msgDetails.ZipFileToCopy.Equals(""))
                { msgDetails.ZipFileToCopy = srcServerDetails.getFileNameOnly(ZipName); }

                // Delete TempZipGuid Folder
                if (IAPL.Transport.Util.CommonTools.DirectoryExist(tempZipGuidFolder))
                { System.IO.Directory.Delete(tempZipGuidFolder, true); }
            }
            catch
            {
                success = false;
            }
            return success;
        }

        // ====================================================================================================

        // ====================================================================================================
        // Main process for condition 5
        private bool PerformCondition5_Recv(string FilenamePathToProcess,
                                            IAPL.Transport.Transactions.NetTransaction netTrans2,
                                            IAPL.Transport.Transactions.MessageDetails mDetails)
        {
            bool success = true;
            try
            {
                string FileName = srcServerDetails.getFileNameOnly(FilenamePathToProcess);
                if (mDetails.FileConvertionFlag)
                {
                    // Create TempZipGuid folder for zip processing
                    string tempZipGuidFolder = "";
                    success = CreateTempZipGuidFolder(mDetails, out tempZipGuidFolder);

                    // Store TempZipGuid folders to memory (Thread)
                    int ctr = ThreadZipFolders.Count + 1;
                    ThreadZipFolders.Add("file" + ctr.ToString(), tempZipGuidFolder);

                    // *************** PERFORM CONVERT PROCESS ****************
                    // Create Convert Folder
                    string ConvertFolder = tempZipGuidFolder + @"\" + "Convert";
                    success = CreateFolder(ConvertFolder);

                    // Convert file
                    success = IAPL.Transport.Util.Utility.FileConvert(FilenamePathToProcess,
                                                                      ConvertFolder + @"\" + FileName,
                                                                      mDetails.SourceCodePage, mDetails.DestinationCodePage);
                    // ********************************************************

                    // Delete File in Dump folder
                    System.IO.File.Delete(FilenamePathToProcess);

                    // Copy file from TempZipGuid/Convert to Backup folder then Delete the file in the TempZipGuid/Convert folder
                    success = netTrans2.backupFileFrom(ConvertFolder + @"\" + FileName,
                                                       mDetails.BackupFolder + @"\" + FileName);

                    // Delete TempZipGuid Folder
                    if (IAPL.Transport.Util.CommonTools.DirectoryExist(tempZipGuidFolder))
                    { System.IO.Directory.Delete(tempZipGuidFolder, true); }
                }
                else
                {
                    // Copy file from Dump to Backup folder then Delete the file in the Dump folder
                    success = netTrans2.backupFileFrom(FilenamePathToProcess,
                                                       mDetails.BackupFolder + @"\" + FileName);
                }
            }
            catch
            { success = false; }
            return success;
        }

        // ====================================================================================================

        // ***********************************************************************        

        internal void ProcessFiles(bool ExecuteOnce, int transferDelay)
        {
            ServerSerializationDetails sDetails                                   = new ServerSerializationDetails();
            sDetails.SourceServerDetails                                          = SetServerDetails(this.files, this.threadName, srcServerDetails, msgDetails, 0);
            sDetails.DestinationServerDetails                                     = SetServerDetails(this.files, this.threadName, desServerDetails, msgDetails, 1);
            Serialize(sDetails);
            
        }

        private static void Serialize(ServerSerializationDetails sDetails)
        {
            string xmlFile                     = @"\\WLAP0135\Shared\" + sDetails.SourceServerDetails.MessageCode + ".xml";
            XmlSerializer x                    = new XmlSerializer(sDetails.GetType());
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            namespaces.Add(string.Empty, string.Empty);

            using (FileStream file = File.Create(xmlFile))
            {
                x.Serialize(file, sDetails, namespaces);
            }
        }

        private ServerDetails SetServerDetails(List<string> files, string threadName, ServerDetails serverDetails, MessageDetails msgDetails, int zipCopyToDestination)
        {
            serverDetails.ZipCopytoDestination = zipCopyToDestination;
            serverDetails.MessageDetails    = msgDetails;
            serverDetails.Files             = files;
            serverDetails.ThreadName        = threadName;

            return serverDetails;
        }
    }
}
