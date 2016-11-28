/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Added properties for XML serialization
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace IAPL.Transport.Transactions
{
    public class MessageDetails
    {
        #region local variables
        
        private string erp = "";
        private string principal = "";
        private string messageCode = "";
        private string tradingCode = "";
        private string tradingName = "";
        private string countryCode = "";
        //private IAPL.Transport.Util.ConstantVariables.FileDirection fileDirection = IAPL.Transport.Util.ConstantVariables.FileDirection.NONE;

        //private string sourceFiles = "";
        private string sourceFileMask = "";

        private string messageFileDesID = "";
        private string messageFileSrcID = "";
        
        private string backupFolder = "";
        private string applicationCode = "";

        private string supplierID = "";
        private string supplierName = "";

        private string processLogID = "";
        private string transDescription = "";
        private string technicalErrorDescription = "";

        private string startDate = "";
        private string endDate = "";

        private string sourceFolder = "";
        private string sourceFile = "";
        private bool sendSuccessNotification = false;

        #region < New Variables for File Terminator and Zipping Functionality >
        //LENIN
        private string fiteMask = "";
        private bool isZIP = false;
        private string zipPassword = "";
        private string retention = "0";

        // ********************************************
        // Developer: Alrazen Estrella
        // Date: July 17, 2008
        // Project: ISG12152
        private bool iszipsource = false;
        private int processcode = 0;
        private int zipcopytodestination = 0;
        private string zipfiletocopy = "";
        private int ziponprocess = 0;

        private bool filessentsingle = true;
        private bool filessentbatch = false;

        private string actualsourceFile = "";
        private string actualoutputFile = "";
        
        // ********************************************

        // ********************************************
        // Developer: Alrazen Estrella
        // Date: September 4, 2008
        // Project: ISG12128

        private bool _fileconvertionflag = false;
        private int _sourcecodepage = 0;
        private int _destinationcodepage = 0;
        private string _SourceFiles = "";               // Bug fix

        // ********************************************

        // ********************************************
        // Developer: Alrazen Estrella
        // Date: September 25, 2008
        // Project: ISG12043

        private int _MsetFilePickupDelay = 0;
        private int _IndividualProcess = 0;
        private bool _MsgManualRunFlag = false;
        private DateTime _MsgStartTime;
        private DateTime _MsgEndTime;
        private bool _MsetBatchRun = false;
        private DateTime _MsetBatchTime;
        private bool _MsetRuntime = false;
        private DateTime _MsetStartTime;
        private DateTime _MsetEndTime;
        private int _MsetInterval = 0;
        private bool _IMSBatchRun = false;

        private bool _isUseTempExtension = false;
        private string _msgTempExtension = string.Empty;

        // ********************************************

        #endregion

        //SR#34273 Ccenriquez -- December 4, 2009
        private System.Int32 _MsetMaxThreadCount;

        #endregion

        #region constructors
        public MessageDetails() {}

        public MessageDetails(string srcFileMask, string backupFolder )
        {                        
            this.SourceFileMask = srcFileMask;
            this.BackupFolder = backupFolder;
        }

        #endregion

        #region properties

        public bool MsgMonday { get; set; }
        public bool MsgTuesday { get; set; }
        public bool MsgWednesday { get; set; }
        public bool MsgThursday { get; set; }
        public bool MsgFriday { get; set; }
        public bool MsgSaturday { get; set; }
        public bool MsgSunday { get; set; }

        public bool SendSuccessNotification 
        {
            get
            {
                return this.sendSuccessNotification;
            }            
        }

        public string SetSendSuccessNotification {
            set
            {
                if (value == null)
                    this.sendSuccessNotification = false;
                else
                {
                    if (value.Length < 1)
                        this.sendSuccessNotification = false;
                    else
                    {
                        if (value.Equals("1") || value.ToLower().Equals("true"))
                            this.sendSuccessNotification = true;
                        else
                            this.sendSuccessNotification = false;
                    }
                }
            }
        }

        public string SourceFile
        {
            get
            {
                return this.sourceFile;
            }
            set
            {
                this.sourceFile = value;
            }
        }

        public string SourceFolder {
            get
            {
                return this.sourceFolder;
            }
            set
            {
                this.sourceFolder = value;
            }
        }

        public string StartDate {
            get
            {
                return this.startDate;
            }
            set
            {
                this.startDate = value;
            }
        }

        public string EndDate
        {
            get
            {
                return this.endDate;
            }
            set
            {
                this.endDate = value;
            }
        }

        public string TransDescription
        {
            get {
                return this.transDescription;
            }
            set {
                this.transDescription = value;
            }
        }

        public string TechnicalErrorDescription
        {
            get
            {
                return this.technicalErrorDescription;
            }
            set
            {
                this.technicalErrorDescription = value;
            }
        }

        public string ProcessLogID
        {
            get
            {
                return this.processLogID;
            }
            set
            {
                this.processLogID = value;
            }
        }

        public string SupplierName
        {
            get
            {
                return this.supplierName;
            }
            set
            {
                this.supplierName = value;
            }
        }

        public string SupplierID
        {
            get
            {
                return this.supplierID;
            }
            set
            {
                this.supplierID = value;
            }
        }

        public string ApplicationCode {
            get {
                return this.applicationCode;
            }
            set {
                this.applicationCode = value;
            }
        }

        //public IAPL.Transport.Util.ConstantVariables.FileDirection FileDirectionType {
        //    get {
        //        return this.fileDirection;
        //    }
        //    set {
        //        this.fileDirection = value;
        //    }

        //}
              

        public string BackupFolder
        {
            get
            {
                return this.backupFolder;
            }
            set
            {
                this.backupFolder = value;
            }
        }

        public string SourceFileMask
        {
            get
            {
                return this.sourceFileMask;
            }
            set
            {
                this.sourceFileMask = value;
            }
        }        

        public string ERP { 
            get{
                return this.erp;
            }

            set {
                this.erp = value;
            }
        }

        public string Principal {
            get
            {
                return this.principal;
            }

            set
            {
                this.principal = value;
            }
        }

        public string MessageCode {
            get
            {
                return this.messageCode;
            }

            set
            {
                this.messageCode = value;
            }
        }

        public string TradingCode {
            get
            {
                return this.tradingCode;
            }

            set
            {
                this.tradingCode = value;
            }
        }

        public string TradingName {
            get
            {
                return this.tradingName;
            }

            set
            {
                this.tradingName = value;
            }
        }

        public string CountryCode
        {
            get
            {
                return this.countryCode;
            }

            set
            {
                this.countryCode = value;
            }
        }

        //public IAPL.Transport.Util.ConstantVariables.FileDirection FileDirectionMode
        //{
        //    get
        //    {
        //        return this.fileDirection;
        //    }

        //    set
        //    {
        //        this.fileDirection = value;
        //    }
        //}

        //public string SourceFiles
        //{
        //    get
        //    {
        //        return this.sourceFiles;
        //    }

        //    set
        //    {
        //        this.sourceFiles = value;
        //    }
        //}        

        public string MessageFileSourceID
        {
            get
            {
                return this.messageFileSrcID;
            }

            set
            {
                this.messageFileSrcID = value;
            }
        }

        public string MessageFileDestinationID
        {
            get
            {
                return this.messageFileDesID;
            }

            set
            {
                this.messageFileDesID = value;
            }
        }

        #region < New Properties for File Terminator and Zipping Functionality (ISG11597) 11-24-2007>
        //LENIN - ISG11597 - ADD - 11-24-2007 //
        public string FITEFileMask
        {
            get { return this.fiteMask; }
            set { this.fiteMask = value; }
        }

        public bool IsZIP
        {
            get { return this.isZIP; }
        }

        public string ZIPPassword
        {
            get { return this.zipPassword; }
            set { this.zipPassword = value; }
        }

        public string Retention
        {
            get { return this.retention; }
            set { this.retention = value; }
        }

        public string SetZippingFunctionality
        {
            set
            {
                if (value == null)
                    this.isZIP = false;
                else
                {
                    if (value.Length < 1)
                        this.isZIP = false;
                    else
                    {
                        if (value.Equals("1") || value.ToLower().Equals("true"))
                            this.isZIP = true;
                        else
                            this.isZIP = false;
                    }
                }
            }
        }
        #endregion


        // **************************************************
        // Developer: Alrazen Estrella
        // Date: July 17, 2008
        // Project: ISG12152

        public bool IsZIPSource
        {
            get { return this.iszipsource; }
        }

        public string SetZipSource
        {
            set
            {
                if (value == null)
                    this.iszipsource = false;
                else
                {
                    if (value.Length < 1)
                        this.iszipsource = false;
                    else
                    {
                        if (value.Equals("1") || value.ToLower().Equals("true"))
                            this.iszipsource = true;
                        else
                            this.iszipsource = false;
                    }
                }
            }
        }

        public int ProcessCode
        {
            get { return this.processcode; }
            set { this.processcode = value; }
        }

        public int ZipCopyToDestination
        {
            get { return this.zipcopytodestination; }
            set { this.zipcopytodestination = value; }
        }

        public string ZipFileToCopy
        {
            get { return this.zipfiletocopy; }
            set { this.zipfiletocopy = value; }
        }

        public int ZipOnProcess
        {
            get { return this.ziponprocess; }
            set { this.ziponprocess = value; }
        }

        public bool FilesSentSingle
        {
            get { return this.filessentsingle; }
        }

        public string SetFilesSentSingle
        {
            set
            {
                if (value == null)
                    this.filessentsingle = false;
                else
                {
                    if (value.Length < 1)
                        this.filessentsingle = false;
                    else
                    {
                        if (value.Equals("1") || value.ToLower().Equals("true"))
                            this.filessentsingle = true;
                        else
                            this.filessentsingle = false;
                    }
                }
            }
        }

        public bool FilesSentBatch
        {
            get { return this.filessentbatch; }
        }

        public string SetFilesSentBatch
        {
            set
            {
                if (value == null)
                    this.filessentbatch = false;
                else
                {
                    if (value.Length < 1)
                        this.filessentbatch = false;
                    else
                    {
                        if (value.Equals("1") || value.ToLower().Equals("true"))
                            this.filessentbatch = true;
                        else
                            this.filessentbatch = false;
                    }
                }
            }
        }

        public string ActualSourceFile
        {
            get
            {
                return this.actualsourceFile;
            }
            set
            {
                this.actualsourceFile = value;
            }
        }

        public string ActualOutputFile
        {
            get
            {
                return this.actualoutputFile;
            }
            set
            {
                this.actualoutputFile = value;
            }
        }

        // **************************************************

        // **************************************************
        // Developer: Alrazen Estrella
        // Date: September 4, 2008
        // Project: ISG12128

        public bool FileConvertionFlag
        {
            get { return this._fileconvertionflag; }
        }

        public string SetFileConvertionFlag
        {
            set
            {
                if (value == null)
                    this._fileconvertionflag = false;
                else
                {
                    if (value.Length < 1)
                        this._fileconvertionflag = false;
                    else
                    {
                        if (value.Equals("1") || value.ToLower().Equals("true"))
                            this._fileconvertionflag = true;
                        else
                            this._fileconvertionflag = false;
                    }
                }
            }
        }

        public int SourceCodePage
        {
            get { return this._sourcecodepage; }
            set { this._sourcecodepage = value; }
        }

        public int DestinationCodePage
        {
            get { return this._destinationcodepage; }
            set { this._destinationcodepage = value; }
        }

        public string SourceFiles                       // Bug fix
        {
            get
            {
                return this._SourceFiles;
            }
            set
            {
                this._SourceFiles = value;
            }
        }
        // **************************************************


        // **************************************************
        // Developer: Alrazen Estrella
        // Date: September 25, 2008
        // Project: ISG12043

        public int MsetFilePickupDelay
        {
            get { return this._MsetFilePickupDelay; }
            set { this._MsetFilePickupDelay = value; }
        }

        public int IndividualProcess
        {
            get { return this._IndividualProcess; }
            set { this._IndividualProcess = value; }
        }

        public bool MsgManualRunFlag
        {
            get { return this._MsgManualRunFlag; }
            set { this._MsgManualRunFlag = value; }
        }

        public DateTime MsgStartTime
        {
            get { return this._MsgStartTime; }
            set { this._MsgStartTime = value; }
        }

        public DateTime MsgEndTime
        {
            get { return this._MsgEndTime; }
            set { this._MsgEndTime = value; }
        }

        public bool MsetBatchRun
        {
            get { return this._MsetBatchRun; }
            set { this._MsetBatchRun = value; }
        }

        public DateTime MsetBatchTime
        {
            get { return this._MsetBatchTime; }
            set { this._MsetBatchTime = value; }
        }

        public bool MsetRuntime
        {
            get { return this._MsetRuntime; }
            set { this._MsetRuntime = value; }
        }

        public DateTime MsetStartTime
        {
            get { return this._MsetStartTime; }
            set { this._MsetStartTime = value; }
        }

        public DateTime MsetEndTime
        {
            get { return this._MsetEndTime; }
            set { this._MsetEndTime = value; }
        }

        public int MsetInterval
        {
            get { return this._MsetInterval; }
            set { this._MsetInterval = value; }
        }

        public bool IMSBatchRun
        {
            get { return this._IMSBatchRun; }
            set { this._IMSBatchRun = value; }
        }

        private string _IMSFolder = "";
        public string IMSFolder
        {
            get
            {   return this._IMSFolder; }
            set
            {   this._IMSFolder = value;    }
        }

        private string _IMSProcessId = "";
        public string IMSProcessId
        {
            get
            {
                return this._IMSProcessId;
            }
            set
            { this._IMSProcessId = value; }
        }

        private bool _CrashStatus = false;
        public bool CrashStatus
        {
            get
            {
                return this._CrashStatus;
            }
            set
            { this._CrashStatus = value; }
        }

        // **************************************************

        //SR#34273 Ccenriquez -- December 4, 2009
        public System.Int32 MsetMaxThreadCount
        {
            get
            {
                return _MsetMaxThreadCount;
            }
            set
            {
                _MsetMaxThreadCount = value;
            }
        }


        public bool IsUseTempExtension
        {
            get { return _isUseTempExtension; }
            set { _isUseTempExtension = value; }
        }

        public string MsgTempExtension
        {
            get { return _msgTempExtension; }
            set { _msgTempExtension = value; }
        }
        #endregion

    }
}
