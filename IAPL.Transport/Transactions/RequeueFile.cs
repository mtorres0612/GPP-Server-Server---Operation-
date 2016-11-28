////////////////////////////////////////////////////////
// SR#33117 Ccenriquez / Capad -- November 5, 2009  ////
////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using IAPL.Transport.Configuration;
using System.IO;
using IAPL.Transport.Data;

namespace IAPL.Transport.Transactions
{
    /// <summary>
    /// Entity class for the RequeueFile database table.
    /// </summary>
    public partial class RequeueFile 
    {
        private System.Int32 _RequeueFileId;
        private System.String _trdpCode = string.Empty;
        private System.String _MsgCode = string.Empty;
        private System.String _SourceFileName = string.Empty;
        private System.String _OutputFileName = string.Empty;
        private System.DateTime _CreateDate;
        private System.DateTime _UpdateDate;
        private System.Boolean _IsActive;
        private System.String _MsetBackUpFolder = string.Empty;
        private System.Int32 _MessageFileDestinationId;
        private System.String _TransmissionTypeCode = string.Empty;
        private System.String _ERP = string.Empty;
        private string _TempExtension = string.Empty;

        public RequeueFile() { }

        #region Properties
        public System.Int32 RequeueFileId
        {
            get
            {
                return _RequeueFileId;
            }
            set
            {
                _RequeueFileId = value;
            }
        }

        public System.String trdpCode
        {
            get
            {
                return _trdpCode;
            }
            set
            {
                _trdpCode = value;
            }
        }

        public System.String MsgCode
        {
            get
            {
                return _MsgCode;
            }
            set
            {
                _MsgCode = value;
            }
        }

        public System.String SourceFileName
        {
            get
            {
                return _SourceFileName;
            }
            set
            {
                _SourceFileName = value;
            }
        }

        public System.String OutputFileName
        {
            get
            {
                return _OutputFileName;
            }
            set
            {
                _OutputFileName = value;
            }
        }

        public System.DateTime CreateDate
        {
            get
            {
                return _CreateDate;
            }
            set
            {
                _CreateDate = value;
            }
        }

        public System.DateTime UpdateDate
        {
            get
            {
                return _UpdateDate;
            }
            set
            {
                _UpdateDate = value;
            }
        }

        public System.Boolean IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                _IsActive = value;
            }
        }

        public System.String MsetBackUpFolder
        {
            get
            {
                return _MsetBackUpFolder;
            }
            set
            {
                _MsetBackUpFolder = value;
            }
        }

        public System.Int32 MessageFileDestinationId
        {
            get
            {
                return _MessageFileDestinationId;
            }
            set
            {
                _MessageFileDestinationId = value;
            }
        }

        public System.String TransmissionTypeCode
        {
            get
            {
                return _TransmissionTypeCode;
            }
            set
            {
                _TransmissionTypeCode = value;
            }
        }

        public System.String ERP
        {
            get
            {
                return _ERP;
            }
            set
            {
                _ERP = value;
            }
        }

        public string TempExtension
        {
            get { return _TempExtension; }
            set { _TempExtension = value; }
        }

        #endregion

        #region Methods
        public void SaveRequeueFile(string srcFileName, string desFileName, MessageDetails msgDetails, ServerDetails srcServerDetails, ServerDetails desServerDetails)
        {
            //create requeue folder if not exist
            string requeueFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("RequeueFolder", "requeue");
            string requeuePath = string.Empty;
            string dumpPath = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + Config.GetAppSettingsValue("tempfolderforzip", "temp") + @"\Dump";
            string sourcePath = string.Empty;

            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(requeueFolder))
                System.IO.Directory.CreateDirectory(requeueFolder);

            requeuePath = desServerDetails.GetNetworkSourceFile(requeueFolder, desFileName);

            if (File.Exists(srcFileName))
            {
                sourcePath = srcFileName;

                if (File.Exists(requeuePath))
                    File.Delete(requeuePath);

                File.Copy(sourcePath, requeuePath);
            }
            else
            {
                sourcePath = desServerDetails.GetNetworkSourceFile(dumpPath, srcFileName);

                if (File.Exists(sourcePath))
                {
                    if (File.Exists(requeuePath))
                        File.Delete(requeuePath);

                    File.Copy(sourcePath, requeuePath);

                    string origSourcePath = srcServerDetails.GetNetworkSourceFile(srcServerDetails.ServerAddress, srcFileName);

                    if (File.Exists(origSourcePath))
                        File.Delete(origSourcePath);
                }
                else
                {
                    sourcePath = desServerDetails.GetNetworkSourceFile(msgDetails.BackupFolder, srcFileName);

                    if (File.Exists(requeuePath))
                        File.Delete(requeuePath);

                    File.Copy(sourcePath, requeuePath);
                }
            }
            //save to requeue table
            RequeueFile theRequeueFile = new RequeueFile();
            theRequeueFile.RequeueFileId = 0;
            theRequeueFile.trdpCode = msgDetails.TradingCode;
            theRequeueFile.MsgCode = msgDetails.MessageCode;
            theRequeueFile.SourceFileName = (msgDetails.FilesSentBatch && msgDetails.IsZIP && msgDetails.SourceFiles != string.Empty) ? msgDetails.SourceFiles : srcFileName;            
            theRequeueFile.OutputFileName = desFileName;
            theRequeueFile.CreateDate = DateTime.Now;
            theRequeueFile.UpdateDate = DateTime.MinValue;
            theRequeueFile.IsActive = true;
            theRequeueFile.TempExtension = (desServerDetails.IsUseFileTempExtension ? desServerDetails.FileTempExtension : string.Empty);

            RequeueFileDALC theRequeueFileDALC = new RequeueFileDALC();
            theRequeueFileDALC.SaveRequeueFile(theRequeueFile);
        }
        #endregion
    }
}
