using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace IAPL.Transport.Transactions
{
    class NetTransaction
    {
        private IAPL.Transport.Transactions.ServerDetails serverInformation = null;
        private string errorMessage = "";
        private string threadName = "";


        //****************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        private int processcode = 0;
        public int ProcessCode
        {
            get
            {
                return this.processcode;
            }
            set
            {
                this.processcode = value;
            }
        }

        private bool filesentsingle;
        public bool FileSentSingle
        {
            get
            {
                return this.filesentsingle;
            }
            set
            {
                this.filesentsingle = value;
            }
        }

        private bool withterminator;
        public bool WithTerminator
        {
            get
            {
                return this.withterminator;
            }
            set
            {
                this.withterminator = value;
            }
        }

        private static int FileCtr = 0;
        //****************************************************************
        #region constructors
        public NetTransaction() 
        {}

        public NetTransaction(IAPL.Transport.Transactions.ServerDetails sDetails) 
        {
            this.serverInformation = sDetails;
        }

        #endregion

        #region properties

        public string ErrorMessage
        {
            get
            {
                return this.errorMessage;
            }
            set
            {
                this.errorMessage = value;
            }
        }

        #endregion

        #region methods

        #region movefileto (backupfolder to destination folder)
        public bool moveFileTo(string srcFile, string desFile)
        {
            bool success = true;

            success = this.copyFile(srcFile, desFile);            

            return success;
        }
        #endregion

        #region backupfilefrom (src to backupfolder)
        public bool backupFileFrom(string srcFile, string desFile) {
            bool success = true;

            success = this.copyFile(srcFile, desFile);

            if (success)
            {
                IAPL.Transport.Operation.Network netOperation = new IAPL.Transport.Operation.Network();

                if (!netOperation.DeleteRemoteFile(srcFile))
                {
                    success = false;
                    this.ErrorMessage = netOperation.ErrorMessage;
                }
            }
            else {
                success = false;
            }
            return success;
        }
        public bool CopyFileFrom(string srcFile, string desFile)
        {
            bool success = true;

            success = this.copyFile(srcFile, desFile);

            //if (success)
            //{
            //    IAPL.Transport.Operation.Network netOperation = new IAPL.Transport.Operation.Network();

            //    if (!netOperation.DeleteRemoteFile(srcFile))
            //    {
            //        success = false;
            //        this.ErrorMessage = netOperation.ErrorMessage;
            //    }
            //}
            //else
            //{
            //    success = false;
            //}
            return success;
        }
        #endregion

        #region CreateTempFileNewFileName 
        public bool CreateTempFileNewFileName(string srcFile, string desFile)
        {
            bool success = true;

            success = this.copyFile(srcFile, desFile);

            if (success)
            {
                //IAPL.Transport.Operation.Network netOperation = new IAPL.Transport.Operation.Network();

                //if (!netOperation.DeleteRemoteFile(srcFile))
                //{
                //    success = false;
                //    this.ErrorMessage = netOperation.ErrorMessage;
                //}
            }
            else
            {
                success = false;
            }

            return success;
        }
        #endregion        

        #region RemoveTempFileName
        public bool RemoveTempFileName(string srcFile)
        {
            bool success = true;


            IAPL.Transport.Operation.Network netOperation = new IAPL.Transport.Operation.Network();

            if (!netOperation.DeleteRemoteFile(srcFile))
            {
                success = false;
                this.ErrorMessage = netOperation.ErrorMessage;
            }            

            return success;
        }
        #endregion        

        #region copyfile
        private bool copyFile(string srcFile, string desFile) {
            bool success = true;

            try{
                IAPL.Transport.Util.TextLogger.Log("CopyFile", this.threadName + " -> CopyFiles from " + srcFile + " to " + desFile);

                //delete existing file
                //using (FileStream fs = File.Create(path)) { }
                // Ensure that the target does not exist.
                File.Delete(desFile);

                File.Copy(srcFile, desFile);
            }
            catch(Exception ex)
            {
                this.ErrorMessage = "NetTransaction-copyFile()|" + ex.Message.ToString();
                success = false;
            }

            return success;
        }
        #endregion

        //private string getFileNameOnly(string origFileName) {
        //    string fName = "";

        //    int i = origFileName.LastIndexOf("\\");

        //    if (i >= 0) {

        //        fName = origFileName.Substring(i+1);
        //    }


        //    return fName;
        //}

        #region StartProcess
        public bool StartProcess(string fileName, string threadName, string desFilePath,
                                 IAPL.Transport.Transactions.ServerDetails desServerDetails)
        {
            bool success = false;
            //ArrayList list = new ArrayList();
            string srcFileName;
            //string fName = this.serverInformation.getFileNameOnly(fileName); //getFileNameOnly(fileName);
            this.threadName = threadName;

            if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
            {
                #region RECEIVE

                srcFileName = this.serverInformation.GetNetworkSourceFile(this.serverInformation.ServerAddress, fileName);                

                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(desFilePath))
                {
                    this.ErrorMessage = "NetTransaction-StartProcess()|Backup folder does not exist. Failed to create backup folder! Failed to backup " + 
                        fileName + " to " + desFilePath;
                    success = false;
                }
                else {

                    desFilePath = this.serverInformation.GetBackupFolderPathWihFileName(desFilePath);
                    success = this.backupFileFrom(srcFileName, desFilePath); //this.copyFile(srcFileName, desFilePath);
                }

                #endregion

            }
            else if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
            {
                #region SEND

                string srcFilePath = desFilePath;

                srcFileName = this.serverInformation.GetNetworkSourceFile(srcFilePath, fileName);

                //COMMENT CODE BELOW SR#33117 Ccenriquez / Capad -- November 13, 2009
                // Add static value to counter
                //if (!this.serverInformation.FileNamingConvention.Equals(string.Empty))
                //{
                //    if (WithTerminator)
                //    {
                //        desServerDetails.FileCounter = desServerDetails.FileCounter + FileCtr;
                //    }
                //}

                //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 13, 2009
                //desFilePath = this.serverInformation.GetNetworkSourceFile(this.serverInformation.ServerAddress, this.serverInformation.GenFileName());

                //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 13, 2009
                desFilePath = this.serverInformation.GetNetworkSourceFile(this.serverInformation.ServerAddress, this.serverInformation.DesFileName);


//                desFilePath = this.serverInformation.GetNetworkSourceFile(this.serverInformation.ServerAddress, desServerDetails.GenFileName());

                // *****************************************************************
                // Developer: Alrazen Estrella
                // Project: ISG12152
                // Date: July 29, 2008

                // Send files to destination
                //if (desFilePath.LastIndexOf(".") < 0)
                //{
                //    desFilePath = desFilePath.Substring(0, desFilePath.LastIndexOf("\\")) + @"\" + fileName;
                //}
                // *****************************************************************

                success = this.moveFileTo(srcFileName, desFilePath); //success = this.MoveFileFromLocalFolder(list);

                //COMMENT CODE BELOW SR#33117 Ccenriquez / Capad -- November 13, 2009
                //if (!this.serverInformation.FileNamingConvention.Equals(string.Empty))
                //{
                //    FileCtr++;
                //    CounterIncrement(desServerDetails);
                //}

                #endregion
            }            
            
            return success;
        }
        #endregion

        // ****************************************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        private void CounterIncrement(IAPL.Transport.Transactions.ServerDetails ServerDetails)
        {
            if (!ServerDetails.FileNamingConvention.Equals(string.Empty))
            { ServerDetails.IncrementCounter(); }
        }
        // ****************************************************************************************



        #endregion
    }
}
