/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Added properties for XML serialization
 *  MNTORRES 09162016 : Added class ServerSerializationDetails
 *                    : Added property ZipCopyTodestination
 *  
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport.Transactions
{
    //OLD CODE SR#33117 Ccenriquez / Capad -- November 18, 2009
    //class ServerDetails

    //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009
    public class ServerDetails
    {
        #region localvariables
        //BEGIN SR#33117 Ccenriquez / Capad -- November 13, 2009
        private System.String _DesFileName = string.Empty;
        private System.Int32 _CountSendAttempt;
        private System.Int32 _TotalFiles;
        private int _zipCopytoDestination;


        public System.String DesFileName
        {
            get
            {
                return _DesFileName;
            }
            set
            {
                _DesFileName = value;
            }
        }
        
        public System.Int32 CountSendAttempt
        {
            get
            {
                return _CountSendAttempt;
            }
            set
            {
                _CountSendAttempt = value;
            }
        }
        
        public System.Int32 TotalFiles
        {
            get
            {
                return _TotalFiles;
            }
            set
            {
                _TotalFiles = value;
            }
        }
        //END SR#33117 Ccenriquez / Capad -- November 13, 2009

        public MessageDetails MessageDetails { get; set; }
        public List<string> Files { get; set; }
        public string ThreadName { get; set; }

        private string serverAddress = "";
        private string serverFolder = "";
        private string serverUserName = "";
        private string serverPassword = "";
        private IAPL.Transport.Util.ConstantVariables.FileDirection fileDirection = IAPL.Transport.Util.ConstantVariables.FileDirection.NONE;        
        private IAPL.Transport.Util.ConstantVariables.FileAction messageAction = IAPL.Transport.Util.ConstantVariables.FileAction.NONE;
        private IAPL.Transport.Util.ConstantVariables.TransmissionMode transmissionType = IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE;

        private string fileNamingConvention = "";
        private string fileNamingExtension = "";
        private string fileNameDateFormat = "";

        private DateTime dateValue = DateTime.Now;
        private Int32 fileCounter = 0;

        private string emailAddressFrom = "";
        private string emailAddressTo = "";
        private string emailAddressCC = "";
        private string emailAddressBCC = "";
        private string emailSubject = "";
        private string emailBody = "";

        private string errorMessage = "";

        private string messageCode = "";

        private string origSourceFileName = "";

        //private int serverPort = 0;

        #endregion

        #region constructors

        public ServerDetails() {
            setCounterValue("0");
            this.DateValue = DateTime.Now;
            this.FileTempExtension = string.Empty;
        }

        public ServerDetails(
            string sName, 
            string sRemoteFolder, 
            string sUserName, 
            string sPassword, 
            IAPL.Transport.Util.ConstantVariables.FileDirection sFileDirection, 
            string sLocalFolder, 
            string sFiles, 
            string sFileNamingConvention, 
            string msgCounter, 
            IAPL.Transport.Util.ConstantVariables.FileAction fileAction, 
            string transmissionType, 
            string mesgCode,
            int sPort,
            string sTempExtension
            ) 
        {
            this.ServerAddress = sName;
            this.ServerFolder = sRemoteFolder;
            this.ServerUserName = sUserName;
            this.ServerPassword = sPassword;
            this.FileDirection = sFileDirection;

            this.MessageAction = fileAction;            

            //this.LocalFolder = sLocalFolder;
            //this.SourceFileMask = sFiles;
            this.FileNamingConvention = sFileNamingConvention;

            setCounterValue(msgCounter);
            this.DateValue = DateTime.Now;

            this.TransmissionType = GetTransmissionType(transmissionType);

            //setCounterValue("0");
            //this.DateValue = DateTime.Now.ToString("MMddyyyy");

            this.MessageCode = mesgCode;
            this.ServerPort = sPort;
            this.FileTempExtension = sTempExtension;
        }

        public ServerDetails(
            string sName, 
            string sRemoteFolder, 
            string sUserName, 
            string sPassword, 
            string strFileDirection, 
            string sLocalFolder, 
            string sFiles, 
            string sFileNamingConvention, 
            string msgCounter, 
            IAPL.Transport.Util.ConstantVariables.FileAction fileAction, 
            string transmissionType, 
            string mesgCode,
            string sTempExtension)
        {
            this.ServerAddress = sName;
            this.ServerFolder = sRemoteFolder;
            this.ServerUserName = sUserName;
            this.ServerPassword = sPassword;
            strFileDirection = strFileDirection.ToUpper().Trim();

            if (strFileDirection.Equals("RECEIVE")) {
                this.FileDirection = IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE;
            }
            else if (strFileDirection.Equals("SEND"))
            {
                this.fileDirection = IAPL.Transport.Util.ConstantVariables.FileDirection.SEND;
            }
            else {
                this.fileDirection = IAPL.Transport.Util.ConstantVariables.FileDirection.NONE;
            }

            //this.LocalFolder = sLocalFolder;
            //this.SourceFileMask = sFiles;
            this.FileNamingConvention = sFileNamingConvention;
            setCounterValue(msgCounter);
            this.DateValue = DateTime.Now;

            this.TransmissionType = GetTransmissionType(transmissionType);
            this.MessageCode = mesgCode;
            this.FileTempExtension = sTempExtension;
        }

        public ServerDetails(
            IAPL.Transport.Util.ConstantVariables.FileDirection sFileDirection, 
            string sLocalFolder, 
            string sFiles, 
            string sFileNamingConvention, 
            string msgCounter, 
            IAPL.Transport.Util.ConstantVariables.FileAction fileAction, 
            string transmissionType, 
            string mesgCode,
            string portNum,
            string sTempExtension
            )
        {
            //this.ServerAddress = sName;
            //this.ServerFolder = sRemoteFolder;
            //this.ServerUserName = sUserName;
            //this.ServerPassword = sPassword;
            this.FileDirection = sFileDirection;

            this.MessageAction = fileAction;

            //this.LocalFolder = sLocalFolder;
            //this.SourceFileMask = sFiles;
            this.FileNamingConvention = sFileNamingConvention;

            setCounterValue(msgCounter);
            this.DateValue = DateTime.Now;

            this.TransmissionType = GetTransmissionType(transmissionType);
            this.MessageCode = mesgCode;
            int thisPortNum = 0;
            int.TryParse(portNum, out thisPortNum);
            this.ServerPort = thisPortNum;
            this.FileTempExtension = sTempExtension;
        }

        #endregion

        #region properties
        public string FileNameDateFormat {
            get
            {
                return this.fileNameDateFormat;
            }
            set
            {
                this.fileNameDateFormat = value;
            }
        }

        public string OrigSourceFileName
        {
            get
            {
                return this.origSourceFileName;
            }
            set
            {
                this.origSourceFileName = value;
            }
        }

        public int ServerPort { get; set; }

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

        public string ErrorMessage {
            get
            {
                return this.errorMessage;
            }
            set
            {
                this.errorMessage = value;
            }
        }

        public string FileNamingConvention
        {
            get
            {
                return this.fileNamingConvention;
            }
            set
            {
                this.fileNamingConvention = value;
            }
        }

        public string FileNamingExtension
        {
            get
            {
                return this.fileNamingExtension;
            }
            set
            {
                this.fileNamingExtension = value;
            }
        }

        public string FileTempExtension { get; set; }
        public bool IsUseFileTempExtension { get; set; }

        public DateTime DateValue
        {
            get
            {
                return this.dateValue;
            }
            set
            {
                this.dateValue = value;
            }
        }

        public Int32 FileCounter
        {
            get
            {
                return this.fileCounter;
            }
            set
            {
                this.fileCounter = value;
            }
        }

        public string ServerFolder
        {
            get
            {
                return this.serverFolder;
            }
            set
            {
                this.serverFolder = value;
            }
        }

        public IAPL.Transport.Util.ConstantVariables.TransmissionMode TransmissionType {
            get {
                return this.transmissionType;
            }
            set {
                this.transmissionType = value;
            }
        }

        public IAPL.Transport.Util.ConstantVariables.FileAction MessageAction {
            get {
                return this.messageAction;
            }
            set {
                this.messageAction = value;
            }
        }

        public string ServerAddress {
            get {
                return this.serverAddress;
            }
            set {
                this.serverAddress = value;
            }
        }
        public string ServerUserName
        {
            get
            {
                return this.serverUserName;
            }
            set
            {
                this.serverUserName = value;
            }
        }
        public string ServerPassword
        {
            get
            {
                return this.serverPassword;
            }
            set
            {
                this.serverPassword = value;
            }
        }

        public IAPL.Transport.Util.ConstantVariables.FileDirection FileDirection
        {
            get
            {
                return this.fileDirection;
            }
            set
            {
                this.fileDirection = value;
            }
        }

        public string EmailAddressTo
        {
            get
            {
                return this.emailAddressTo;
            }
            set
            {
                this.emailAddressTo = value;
            }
        }

        public string EmailAddressFrom
        {
            get
            {
                return this.emailAddressFrom;
            }
            set
            {
                this.emailAddressFrom = value;
            }
        }

        public string EmailAddressCC
        {
            get
            {
                return this.emailAddressCC;
            }
            set
            {
                this.emailAddressCC = value;
            }
        }

        public string EmailAddressBCC
        {
            get
            {
                return this.emailAddressBCC;
            }
            set
            {
                this.emailAddressBCC = value;
            }
        }

        public string EmailSubject
        {
            get
            {
                return this.emailSubject;
            }
            set
            {
                this.emailSubject = value;
            }
        }

        public string EmailBody
        {
            get
            {
                return this.emailBody;
            }
            set
            {
                this.emailBody = value;
            }
        }

        public int ZipCopytoDestination
        {
            get { return _zipCopytoDestination; }
            set { _zipCopytoDestination = value; }
        }


        #endregion

        #region methods

        #region GetTransmissionType
        public IAPL.Transport.Util.ConstantVariables.TransmissionMode GetTransmissionType(string transmissionType)
        {
            IAPL.Transport.Util.ConstantVariables.TransmissionMode retTransMode = IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE;
            switch (transmissionType.ToUpper().Trim())
            {
                case "FTP":
                    retTransMode = IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP;
                    break;
                case "SFTP":
                    retTransMode = IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP;
                    break;
                case "NETWORK":
                    retTransMode = IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK;
                    break;
                case "EMAIL":
                    retTransMode = IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL;
                    break;
                case "HTTP":
                    retTransMode = IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP;
                    break;
                default:
                    retTransMode = IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE;
                    break;
            }
            return retTransMode;
        }
        #endregion

        #region counter filenaming

        #region incrementcounter
        public void IncrementCounter()
        {
            if (this.FileCounter == 9999)
            {
                this.FileCounter = 1;
            }
            else
            {
                this.FileCounter++;
            }
        }
        #endregion

        #region setCounterValue
        private void setCounterValue(string msgCounter)
        {
            try
            {
                this.FileCounter = Convert.ToInt32(msgCounter);
                if (this.FileCounter == 0) {
                    this.FileCounter = 1;
                }
                else if (this.FileCounter > 9999) {
                    this.FileCounter = 1;
                }
            }
            catch
            {
                this.FileCounter = 1;
            }
        }
        #endregion

        #region getFileCounter
        private string getFileCounter()
        {
            return this.FileCounter.ToString().PadLeft(4, '0');
        }
        #endregion

        public string GenFileName()
        {
            string tempFileName = this.FileNamingConvention;

            if (tempFileName.Trim().Length > 0) // with filename convention
            {
                if (tempFileName.Trim().EndsWith("."))
                {
                    if (this.FileNamingExtension.Trim().Length > 0)
                    {
                        if (this.FileNamingExtension.Trim().StartsWith("."))
                        {
                            tempFileName = tempFileName.Trim() + this.FileNamingExtension.Replace(".", "");
                        }
                        else
                        {
                            tempFileName = tempFileName.Trim() + this.FileNamingExtension;
                        }
                    }
                }
                else {
                    if (this.FileNamingExtension.Trim().Length > 0)
                    {
                        if (this.FileNamingExtension.Trim().StartsWith("."))
                        {
                            tempFileName = tempFileName.Trim() + this.FileNamingExtension;
                        }
                        else
                        {
                            tempFileName = tempFileName.Trim() + "." + this.FileNamingExtension;
                        }
                    }
                }

                if (tempFileName.IndexOf("<CTR>") > -1)
                {
                    tempFileName = tempFileName.Replace("<CTR>", getFileCounter());
                }
                if (tempFileName.IndexOf("<DATE>") > -1)
                {
                    tempFileName = tempFileName.Replace("<DATE>", getDateValue());
                }
            }
            else {
                tempFileName = this.OrigSourceFileName;
            }

            if (this.FileTempExtension.ToString().Trim().Length > 0)
            {
                if (this.FileTempExtension.Substring(0, 1) != ".")
                    this.FileTempExtension = '.' + this.FileTempExtension;
                tempFileName += this.FileTempExtension;
            }

            return tempFileName;
        }

        private string getDateValue() {
            string retDate = "";
            try
            {
                //OLD CODE SR#33117 Ccenriquez / Capad -- December 07, 2009
                //retDate = this.DateValue.ToString(this.FileNameDateFormat);

                //NEW CODE SR#33117 Ccenriquez / Capad -- December 07, 2009
                System.Threading.Thread.Sleep(1000);

                retDate = DateTime.Now.ToString(this.FileNameDateFormat);
            }
            catch {
                retDate = "";
            }

            return retDate;
        }

        public string GetFTPSourceFile(string fileName) {
            if(!this.ServerFolder.Trim().StartsWith(@"/")){
                this.ServerFolder = @"/" + this.ServerFolder;
            }
            //else if (!this.serverFolder.Trim().StartsWith("//"))
            //{
            //    this.ServerFolder = "//" + this.ServerFolder;
            //}

            //if (this.serverFolder.Trim().StartsWith("////")) {
            //    this.ServerFolder = "//" + this.ServerFolder.Substring(1, this.ServerFolder.Length - 1);
            //}

            if (!this.ServerFolder.Trim().EndsWith(@"/"))
            {
                fileName = this.ServerFolder + @"/" + fileName;
            }
            else
            {
                fileName = this.ServerFolder + fileName;
            }

            return fileName;
        }

        //LENIN - ISG11957 - ADD - 11-27-2007
        public string GetFTPSourcePath()
        {
            if (!this.ServerFolder.Trim().StartsWith(@"/"))
            {
                this.ServerFolder = @"/" + this.ServerFolder;
            }
            //else if (!this.serverFolder.Trim().StartsWith("//"))
            //{
            //    this.ServerFolder = "//" + this.ServerFolder;
            //}

            //if (this.serverFolder.Trim().StartsWith("////")) {
            //    this.ServerFolder = "//" + this.ServerFolder.Substring(1, this.ServerFolder.Length - 1);
            //}

            if (!this.ServerFolder.Trim().EndsWith(@"/"))
            {
                return this.ServerFolder + @"/";
            }
            else
            {
                return this.ServerFolder;
            }
        }

        public string GetFTPFolder(string folder) { 
            if(!folder.StartsWith(@"/")){
                folder = @"/" + folder;
            }
            return folder;
        }

        #region GetSFTPSourceFile
        public string GetSFTPSourceFile(string fileName)
        {
            if (!this.serverFolder.Trim().StartsWith(@"/"))
            {
                this.ServerFolder = @"/" + this.ServerFolder;

            }
            //else if (!this.serverFolder.Trim().StartsWith(@"//"))
            //{
            //    this.ServerFolder = @"//" + this.ServerFolder;
            //}

            if (this.serverFolder.Trim().StartsWith("////"))
            {
                this.ServerFolder = @"/" + this.ServerFolder.Substring(1, this.ServerFolder.Length - 1);
            }

            if (!this.ServerFolder.Trim().EndsWith(@"/"))
            {
                fileName = this.ServerFolder + @"/" + fileName;
            }
            else
            {
                fileName = this.ServerFolder + fileName;
            }
            return fileName;
        }
        #endregion

        #region GetNetworkSourceFile
        public string GetNetworkSourceFile(string desFilePath, string fileName)
        {
            // *****************************************************
            // Developer: Alrazen Estrella
            // Project: ISG12152
            // Date: July 17, 2008

            // Old Code
            //if (fileName.Trim().Length < 1)
            //    fileName = this.GenFileName();


            // New Code
            if (fileName.Trim().Length < 1)
                fileName = this.GenFileName();
            else
                fileName = getFileNameOnly(fileName);

            // *****************************************************


            if (desFilePath.Trim().Length > 0)
            {
                if (desFilePath.Trim().EndsWith("\\"))
                {
                    desFilePath = string.Concat(desFilePath, fileName);
                }
                else
                {
                    desFilePath = string.Concat(desFilePath, "\\", fileName);
                }
            }
            else
            {
                desFilePath = fileName;
            }
            return desFilePath;
        }
        #endregion


        #region SetBackupFolderPathWihFileName
        public string GetBackupFolderPathWihFileName(string desFilePath)
        {
            //if (fileName.Trim().Length < 1)
            //    fileName = this.GenFileName();

            if (desFilePath.Trim().Length > 0)
            {
                if (desFilePath.Trim().EndsWith("\\"))
                {
                    desFilePath = string.Concat(desFilePath, this.OrigSourceFileName);
                }
                else
                {
                    desFilePath = string.Concat(desFilePath, "\\", this.OrigSourceFileName);
                }
            }
            else
            {
                desFilePath = this.OrigSourceFileName;
            }
            return desFilePath;
        }
        #endregion

        #region getFileNameOnly
        public string getFileNameOnly(string origFileName)
        {
            string fName = "";

            int i = origFileName.LastIndexOf("\\");

            if (i >= 0)
            {
                fName = origFileName.Substring(i + 1);
            }
            else
                fName = origFileName;


            return fName;
        }
        #endregion

        #endregion

        #region GetSourceFile
        public string GetSourceFile() {
            string srcFile = "";

            switch (this.TransmissionType) { 
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                    srcFile = this.OrigSourceFileName;
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    srcFile = this.GetFTPSourceFile(this.OrigSourceFileName);
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                    srcFile = this.GetNetworkSourceFile(this.ServerAddress, this.getFileNameOnly(this.OrigSourceFileName));
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    srcFile = this.GetFTPSourceFile(this.OrigSourceFileName);
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE:
                    srcFile = "";
                    break;
            }

            return srcFile;
        }
        #endregion

        #region GetDestinationFile
        public string GetDestinationFile(string desPath)
        {
            string srcFile = "";

            switch (this.TransmissionType)
            {
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                    srcFile = this.OrigSourceFileName;
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    srcFile = this.GetFTPSourceFile(this.OrigSourceFileName);
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                    srcFile = this.GetNetworkSourceFile(this.ServerAddress, this.GenFileName());
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    srcFile = this.GetFTPSourceFile(this.OrigSourceFileName);
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE:
                    srcFile = "";
                    break;
            }

            return srcFile;
        }

        #region GetDestinationFolder
        public string GetDestinationFolder(string desPath)
        {
            string srcFile = "";

            switch (this.TransmissionType)
            {
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                    srcFile = "";
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    //srcFile = this.ServerFolder;

                    srcFile = "ftp://" + this.ServerAddress;
                    if (srcFile.Trim().EndsWith(@"/"))
                    {
                        srcFile += this.ServerFolder;
                    }
                    else
                    {
                        srcFile += @"/" + this.ServerFolder;
                    }  
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                    srcFile = this.ServerAddress;
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    srcFile = desPath;
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE:
                    srcFile = "";
                    break;
            }

            return srcFile;
        }
        #endregion

        #region GetSourceFolder
        public string GetSourceFolder(string desPath)
        {
            string srcFile = "";

            switch (this.TransmissionType)
            {
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                    srcFile = "";
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    srcFile = "ftp://" + this.ServerAddress;
                    if (srcFile.Trim().EndsWith(@"/"))
                    {
                        srcFile += this.ServerFolder;
                    }
                    else {
                        //srcFile += @"/" + this.ServerFolder;
                        if (this.ServerFolder.Trim().StartsWith(@"/"))
                        {
                            srcFile += this.ServerFolder;
                        }
                        else
                        {
                            srcFile += @"/" + this.ServerFolder;
                        }
                    }                    
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                    srcFile = this.ServerAddress;
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    //srcFile = "sftp://" + this.ServerAddress + this.ServerFolder;
                    srcFile = "sftp://" + this.ServerAddress;
                    if (srcFile.Trim().EndsWith(@"/"))
                    {
                        srcFile += this.ServerFolder;
                    }
                    else
                    {
                        if (this.ServerFolder.Trim().StartsWith(@"/"))
                        {
                            srcFile += this.ServerFolder;
                        }
                        else
                        {
                            srcFile += @"/" + this.ServerFolder;
                        }
                    }  
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE:
                    srcFile = "";
                    break;
            }

            return srcFile;
        }
        #endregion


        #endregion

        #endregion
    }

    public class ServerSerializationDetails
    {
        public ServerDetails SourceServerDetails { get; set; }
        public ServerDetails DestinationServerDetails { get; set; }
    }
}
