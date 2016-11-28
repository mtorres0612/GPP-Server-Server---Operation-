using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;

namespace IAPL.Transport.Transactions
{
    class SftpTransaction
    {
        //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 19, 2009
        private static Hashtable htFiles = new Hashtable(); //this will hold generated file counter need for requeue in case failed to transmit to destination

        private IAPL.Transport.Transactions.ServerDetails serverInformation = null;
        private string errorMessage = "";

        public SftpTransaction(IAPL.Transport.Transactions.ServerDetails transferInfo) {
            this.serverInformation = transferInfo;
        }


        public SftpTransaction()
        {

        }
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


        #region Old Code - MoveFile2LocalFolder
        //private bool MoveFile2LocalFolder(object data)
        //{
        //    bool success = true;
        //    ArrayList list = (ArrayList)data;
        //    string remoteServer = (string)list[0];
        //    string remoteUser = (string)list[1];
        //    string remotePass = (string)list[2];
        //    string remoteFile = (string)list[3];
        //    string localFile = (string)list[4];
        //    string threadName = (string)list[5];

        //    //System.Console.WriteLine("Thread {0} is running ", threadName);

        //    IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(remoteServer, remoteUser, remotePass);

        //    if (sftp.Connect())
        //    {
        //        if (sftp.Download(remoteFile, localFile))
        //        {
        //            if (sftp.DeleteRemoteFile(remoteFile))
        //            {

        //            }
        //            else
        //            { // error
        //                this.ErrorMessage = sftp.ErrorMessage;
        //                success = false;
        //            }
        //        }
        //        else
        //        { // error
        //            this.ErrorMessage = sftp.ErrorMessage;
        //            success = false;
        //        }

        //        sftp.Disconnect();

        //        //System.Console.WriteLine("Thread {0} is done. ", threadName);
        //    }
        //    else
        //    {
        //        this.ErrorMessage = sftp.ErrorMessage;
        //        success = false;
        //    }

        //    return success;
        //}
        #endregion


        #region Old Code - MoveFileFromLocalFolder
        //private bool MoveFileFromLocalFolder(object data)
        //{
        //    bool success = true;
        //    ArrayList list = (ArrayList)data;
        //    string remoteServer = (string)list[0];
        //    string remoteUser = (string)list[1];
        //    string remotePass = (string)list[2];
        //    //string remotePath = (string)list[3];
        //    string remoteFile = (string)list[3];
        //    //string localPath = (string)list[5];
        //    string localFile = (string)list[4];
        //    string threadName = (string)list[5];

        //    IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
        //            this.serverInformation.ServerUserName, this.serverInformation.ServerPassword);

        //    if (sftp.Connect())
        //    {
        //        if (sftp.Upload(remoteFile, localFile))
        //        {
        //            // dont delete the backup file
        //            //IAPL.Transport.Operation.Network localFolder = new IAPL.Transport.Operation.Network();
        //            //localFolder.DeleteRemoteFile(remoteFile);

        //        }
        //        else
        //        { // error
        //            this.ErrorMessage = sftp.ErrorMessage;
        //            success = false;
        //        }

        //        sftp.Disconnect();
        //    }
        //    else
        //    {
        //        this.ErrorMessage = sftp.ErrorMessage;
        //        success = false;
        //    }

        //    return success;
        //}
        #endregion        



        // **********************************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        // Date: July 29, 2008

        private static int FileCtr = 0;

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

        private bool MoveFiles2LocalFolder(object data, object files, object incompletefiles, int ProcessCode, Hashtable fitefiles, string IncludeTermInCondition5, string DumpFolder,
                                           IAPL.Transport.Transactions.MessageDetails msgDetails, 
                                           out Hashtable DumpFiles, out Hashtable TerminatorFiles)
        {
            bool success = true;
            DumpFiles = new Hashtable();
            TerminatorFiles = new Hashtable(); 

            ArrayList list = (ArrayList)data;
            string remoteServer = (string)list[0];
            string remoteUser = (string)list[1];
            string remotePass = (string)list[2];
            string remotePath = (string)list[3];
            string localPath = (string)list[4];
            string threadName = (string)list[5];
            string fileTerminator = (string)list[6];
            bool filesentsingle = (bool)list[7];        // Alrazen Estrella

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                    this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            string TerminatorFileExt = fileTerminator.Substring(fileTerminator.LastIndexOf(".") + 1);
            
            if (sftp.Connect())
            {
                #region Version 1.3
                int ctr1 = 0;
                int ctr2 = 0;
                if (ProcessCode.Equals(5))
                {
                    // Download file from Source to Dump folder
                    if (!sftp.Download(remotePath + @"/" + fileTerminator, DumpFolder + @"\" + fileTerminator, msgDetails.MsetFilePickupDelay))
                    {
                        this.ErrorMessage = sftp.ErrorMessage;
                        success = false;
                    }
                    else
                    {
                        //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        //if (TerminatorFileExt != IncludeTermInCondition5.Substring(IncludeTermInCondition5.LastIndexOf(".") + 1))

                        //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        if (TerminatorFileExt.ToLower() != IncludeTermInCondition5.Substring(IncludeTermInCondition5.LastIndexOf(".") + 1).ToLower())
                        {
                            ctr1++;
                            DumpFiles.Add("file" + ctr1, DumpFolder + @"\" + serverInformation.getFileNameOnly(fileTerminator));
                        }
                        else
                        {
                            ctr2++;
                            TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + IncludeTermInCondition5);
                        }
                    }

                    // Delete Remote file                        
                    if (!sftp.DeleteRemoteFile(remotePath + @"/" + fileTerminator))
                    { }
                }
                else
                {
                    //Get Files from Source
                    foreach (DictionaryEntry file in (Hashtable)files)
                    {
                        string FilenametoDownload = file.Value.ToString();
                        string FileExt = FilenametoDownload.Substring(FilenametoDownload.LastIndexOf(".") + 1);

                        //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        //if (FileExt == TerminatorFileExt)

                        //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        if (string.Equals(FileExt, TerminatorFileExt, StringComparison.OrdinalIgnoreCase))
                        {
                            // If IMS, Copy the Terminator to Dump folder
                            if (msgDetails.IndividualProcess.Equals(1))
                            {
                                string SourceFilePath = remotePath + @"/" + FilenametoDownload;
                                string DestinationFilePath = DumpFolder + @"\" + FilenametoDownload;
                                if (!sftp.Download(SourceFilePath, DestinationFilePath, msgDetails.MsetFilePickupDelay))
                                {
                                    this.ErrorMessage = sftp.ErrorMessage;
                                    success = false;
                                }
                                else
                                {
                                    // Delete Remote file                        
                                    if (!sftp.DeleteRemoteFile(remotePath + @"/" + FilenametoDownload))
                                    { }
                                }
                            }
                        }
                        //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        //else if (((FileExt != TerminatorFileExt) & (fitefiles.Count > 0)) |
                        //          (fitefiles.Count.Equals(0)))

                        //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        else if (((FileExt.ToLower() != TerminatorFileExt.ToLower()) & (fitefiles.Count > 0)) |
                                  (fitefiles.Count.Equals(0)))
                        {
                            // Download file from Source to Dump folder
                            if (!sftp.Download(remotePath + @"/" + FilenametoDownload, DumpFolder + @"\" + FilenametoDownload, msgDetails.MsetFilePickupDelay))
                            {
                                this.ErrorMessage = sftp.ErrorMessage;
                                success = false;
                            }
                            else
                            {
                                //OLD CODE SR#33117 Ccenriquez -- December 7, 2009
                                //if (FilenametoDownload.Substring(FilenametoDownload.LastIndexOf(".") + 1).ToLower().Equals("zip"))

                                //NEW CODE SR#33117 Ccenriquez -- December 7, 2009 - SFTP to FTP bug : single w/fite w/zip
                                if (FilenametoDownload.Substring(FilenametoDownload.LastIndexOf(".") + 1).ToLower().Equals(FileExt.ToLower()))   //"zip"
                                {
                                    ctr1++;
                                    DumpFiles.Add("file" + ctr1, DumpFolder + @"\" + serverInformation.getFileNameOnly(FilenametoDownload));
                                }
                            }

                            // ++++ Execute if Not IMS ++++
                            if (!msgDetails.IndividualProcess.Equals(1))
                            {
                                // Execute if with Terminator
                                if ((fileTerminator != string.Empty) & (fitefiles.Count > 0))
                                {
                                    string FileFileExt = FilenametoDownload.Substring(FilenametoDownload.LastIndexOf(".") + 1);

                                    // Copy File Terminator (Fite filename) to Backup folder
                                    string FileTerminator = "";
                                    if (filesentsingle)
                                    {
                                        FileTerminator = FilenametoDownload.Replace("." + FileFileExt, "." + TerminatorFileExt);                                        
                                        string SourceFilePath = remotePath + @"/" + FileTerminator;
                                        string DestinationFilePath = DumpFolder + @"\" + FileTerminator;
                                        if (!sftp.Download(SourceFilePath, DestinationFilePath, msgDetails.MsetFilePickupDelay))
                                        {
                                            this.ErrorMessage = sftp.ErrorMessage;
                                            success = false;
                                        }

                                        ctr2++;
                                        TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + FileTerminator);

                                        //Delete File Terminator from Source
                                        if (!sftp.DeleteRemoteFile(SourceFilePath))
                                        { }
                                    }
                                }
                            }

                            // Delete Remote file                        
                            if (!sftp.DeleteRemoteFile(remotePath + @"/" + FilenametoDownload))
                            { }
                        }
                    }

                    // FOR IMS: Move Incomplete files to IMS Incomplete folder
                    if (msgDetails.IndividualProcess.Equals(1))
                    {
                        // Create IMSIncomplete folder 
                        string IncFolder = msgDetails.BackupFolder.Substring(0, msgDetails.BackupFolder.LastIndexOf("Backup")) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                        if (!IAPL.Transport.Util.CommonTools.DirectoryExist(IncFolder))
                        { System.IO.Directory.CreateDirectory(IncFolder); }

                        foreach (DictionaryEntry file in (Hashtable)incompletefiles)
                        {
                            string DestinationFilePath = IncFolder + @"\" + file.Value.ToString();
                            if (!sftp.Download(remotePath + @"/" + file.Value.ToString(),
                                               DestinationFilePath,
                                               msgDetails.MsetFilePickupDelay))
                            {
                                this.ErrorMessage = sftp.ErrorMessage;
                                success = false;
                            }
                        }
                    }

                    if (!filesentsingle)
                    {
                        foreach (DictionaryEntry file in (Hashtable)fitefiles)
                        {
                            string SourceFilePath = remotePath + @"/" + file.Value.ToString();
                            string DestinationFilePath = DumpFolder + @"\" + file.Value.ToString();
                            if (!sftp.Download(SourceFilePath, DestinationFilePath, msgDetails.MsetFilePickupDelay))
                            {
                                this.ErrorMessage = sftp.ErrorMessage;
                                success = false;
                            }

                            ctr2++;
                            TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + file.Value.ToString());

                            //Delete File Terminator from Source
                            if (!sftp.DeleteRemoteFile(SourceFilePath))
                            { }
                        }
                    }
                }
                #endregion

                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
        //private bool MoveFileFromLocalFolder(object data, object files, int ProcessCode)      // Edited by Alrazen Estrella, ISG12152, July 24, 2008
        
        //NEW CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
        private bool MoveFileFromLocalFolder(MessageDetails msgDetails, object data, object files, int ProcessCode)    
        {
            bool success = true;
            ArrayList list = (ArrayList)data;
            string remoteServer = (string)list[0];
            string remoteUser = (string)list[1];
            string remotePass = (string)list[2];
            string remoteFile = (string)list[3];
            string localFile = (string)list[4];
            string threadName = (string)list[5];
            string fileTerminator = (string)list[6];

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                    this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            // Establish connection to SFTP server
            if (sftp.Connect())
            {
                // Upload files to SFTP Server
                if (ProcessCode.Equals(1) ||
                    ProcessCode.Equals(2) ||
                    ProcessCode.Equals(3))
                {
                    //BEGIN SR#33117 Ccenriquez / Capad -- November 19, 2009
                    if (htFiles.Count > 0 && this.serverInformation.CountSendAttempt > 0)
                    {
                        if (htFiles.ContainsKey(this.serverInformation.MessageCode))
                            files = (object)htFiles[this.serverInformation.MessageCode];
                    }

                    Hashtable filesClone = (Hashtable)((Hashtable)files).Clone();

                    foreach (DictionaryEntry file in (Hashtable)files)
                    {
                        string FileToUpload = file.Value.ToString();
                        string tempOrigSourceFileName = serverInformation.OrigSourceFileName;
                        serverInformation.OrigSourceFileName = FileToUpload;

                        string desFileName = (this.serverInformation.CountSendAttempt == 0) ? this.serverInformation.GenFileName() : file.Key.ToString();

                        if (FileToUpload != string.Empty && FileToUpload.IndexOf(@"\") > -1)
                        {
                            int lastIndex = FileToUpload.LastIndexOf(@"\");
                            FileToUpload = FileToUpload.Substring(lastIndex + 1, FileToUpload.Length - lastIndex - 1);
                        }

                        if (this.serverInformation.CountSendAttempt == 0 && serverInformation.FileNamingConvention != string.Empty && this.serverInformation.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                            this.serverInformation.IncrementCounter();

                        success = sftp.Upload(remoteFile + @"\" + FileToUpload, localFile + @"/" + desFileName);

                        if (success)
                        {
                            if (filesClone.ContainsValue(file.Value.ToString()))
                                filesClone.Remove(file.Key.ToString());
                        }
                        else
                        {
                            //This will be used when trying to send the file again
                            filesClone.Remove(file.Key);
                            filesClone.Add(desFileName, file.Value);

                            this.ErrorMessage = sftp.ErrorMessage;
                        }

                        serverInformation.OrigSourceFileName = tempOrigSourceFileName;
                    }

                    files = filesClone;

                    if (this.serverInformation.CountSendAttempt == 0 &&
                    this.serverInformation.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1 &&
                    this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND &&
                    !this.serverInformation.FileNamingConvention.Equals(string.Empty))
                    {
                        IAPL.Transport.Data.DbTransaction DBTrans = new IAPL.Transport.Data.DbTransaction();
                        DBTrans.UpdateMsgCounter(this.serverInformation.MessageCode, this.serverInformation.FileCounter);
                    }

                    if (((Hashtable)files).Count > 0)
                    {
                        if (this.serverInformation.CountSendAttempt == 0 &&
                            !htFiles.ContainsKey(this.serverInformation.MessageCode))
                            htFiles.Add(this.serverInformation.MessageCode, files);

                        this.ErrorMessage = "SFTP-MoveFileFromLocalFolder()|one or more files have not been transmitted successfully to destination";

                        if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND &&
                            this.serverInformation.CountSendAttempt >= 4)
                        {
                            RequeueFile theRequeueFile = new RequeueFile();

                            foreach (System.Collections.DictionaryEntry file in (Hashtable)files)
                                theRequeueFile.SaveRequeueFile(file.Value.ToString(), file.Key.ToString(), msgDetails, this.serverInformation, this.serverInformation);

                            if (htFiles.Count > 0)
                            {
                                if (htFiles.ContainsKey(this.serverInformation.MessageCode))
                                    htFiles.Remove(this.serverInformation.MessageCode);
                            }
                        }
                    }
                    else
                    {
                        if (htFiles.Count > 0)
                        {
                            if (htFiles.ContainsKey(this.serverInformation.MessageCode))
                                htFiles.Remove(this.serverInformation.MessageCode);
                        }
                    }
                    //END SR#33117 Ccenriquez / Capad -- November 19, 2009

                    //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 19, 2009
                    //foreach (DictionaryEntry file in (Hashtable)files)
                    //{
                    //    string FileToUpload = file.Value.ToString();
                    //    string tempOrigSourceFileName = serverInformation.OrigSourceFileName;
                    //    serverInformation.OrigSourceFileName = FileToUpload;

                    //    // if (sftp.Upload(remoteFile + @"\" + FileToUpload, localFile + @"/" + FileToUpload)) 
                    //    if (sftp.Upload(remoteFile + @"\" + FileToUpload, localFile + @"/" + this.serverInformation.GenFileName()))
                    //    {
                    //        // dont delete the backup file  
                    //    }
                    //    else
                    //    {
                    //        this.ErrorMessage = sftp.ErrorMessage;
                    //        success = false;
                    //    }
                    //    serverInformation.OrigSourceFileName = tempOrigSourceFileName;
                    //    if (!serverInformation.FileNamingConvention.Equals(string.Empty))
                    //    { serverInformation.IncrementCounter(); }
                    //}
                }
                else if (ProcessCode.Equals(4) ||
                         ProcessCode.Equals(6))
                {
                    //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
                    //if (sftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + fileTerminator))

                    //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
                    if (sftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + this.serverInformation.DesFileName))
                    {
                        // dont delete the backup file
                        //IAPL.Transport.Operation.Network localFolder = new IAPL.Transport.Operation.Network();
                        //localFolder.DeleteRemoteFile(remoteFile);
                    }
                    else
                    { // error
                        this.ErrorMessage = sftp.ErrorMessage;
                        success = false;
                    }
                }
                else if (ProcessCode.Equals(5))
                {
                    if (serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
                    {
                        //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
                        // Add static value to counter
                        //if (!this.serverInformation.FileNamingConvention.Equals(string.Empty))
                        //{
                        //    if (WithTerminator)
                        //    {
                        //        serverInformation.FileCounter = serverInformation.FileCounter + FileCtr;
                        //    }
                        //}

                        // Copy File from Backup to Destination folder
//                        if (!sftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + fileTerminator))

                        //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
                        //if (!sftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + this.serverInformation.GenFileName()))

                        //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
                        if (!sftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + this.serverInformation.DesFileName))
                        {
                            // error
                            this.ErrorMessage = sftp.ErrorMessage;
                            success = false;
                        }

                        //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 17, 2009
                        //if (!this.serverInformation.FileNamingConvention.Equals(string.Empty))
                        //{
                        //    FileCtr++;
                        //}
                    }
                }
                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        public bool MoveAllFilesFromLocalFolder2(string SourceLocalFolderPath, string DestLocalFolderPath, object Sourcefiles)
        {
            bool success = true;

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            IAPL.Transport.Transactions.ServerDetails desServerDetails = new IAPL.Transport.Transactions.ServerDetails();

            if (sftp.Connect())
            {
                //Get Files from Source
                foreach (DictionaryEntry file in (Hashtable)Sourcefiles)
                {
                    if (!sftp.Upload(SourceLocalFolderPath + @"\" + file.Value.ToString(), DestLocalFolderPath + @"/" + file.Value.ToString()))
                    {
                        this.ErrorMessage = sftp.ErrorMessage;
                        success = false;
                    }

                    // Delete file from Local folder
                    System.IO.File.Delete(SourceLocalFolderPath + @"\" + file.Value.ToString());

                }
                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
        //public bool MoveAllFilesFromListToFTP(string DestLocalFolderPath, object Sourcefiles)

        //NEW CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
        public bool MoveAllFilesFromListToSFTP(string DestLocalFolderPath, object Sourcefiles, MessageDetails msgDetails)
        {
            bool success = true;

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            IAPL.Transport.Transactions.ServerDetails desServerDetails = new IAPL.Transport.Transactions.ServerDetails();

            if (sftp.Connect())
            {
                //BEGIN SR#33117 Ccenriquez / Capad -- November 19, 2009
                if (htFiles.Count > 0 && this.serverInformation.CountSendAttempt > 0)
                {
                    if (htFiles.ContainsKey(this.serverInformation.MessageCode))
                        Sourcefiles = (object)htFiles[this.serverInformation.MessageCode];
                }

                Hashtable SourcefilesClone = (Hashtable)((Hashtable)Sourcefiles).Clone();

                foreach (DictionaryEntry file in (Hashtable)Sourcefiles)
                {
                    string FileToUpload = file.Value.ToString();
                    string tempOrigSourceFileName = serverInformation.OrigSourceFileName;
                    serverInformation.OrigSourceFileName = desServerDetails.getFileNameOnly(FileToUpload);

                    string desFileName = string.Empty;

                    if (serverInformation.CountSendAttempt == 0 && serverInformation.FileNamingConvention != string.Empty)
                        desFileName = serverInformation.GenFileName();
                    else if (serverInformation.CountSendAttempt > 0 && serverInformation.FileNamingConvention != string.Empty)
                        desFileName = file.Key.ToString();
                    else
                        desFileName = file.Value.ToString();

                    if (this.serverInformation.CountSendAttempt == 0 && this.serverInformation.FileNamingConvention != string.Empty && this.serverInformation.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                        this.serverInformation.IncrementCounter();

                    success = sftp.Upload(FileToUpload, DestLocalFolderPath + @"/" + desFileName);

                    if (success)
                    {
                        if (SourcefilesClone.ContainsValue(file.Value.ToString()))
                            SourcefilesClone.Remove(file.Key.ToString());
                    }
                    else
                    {
                        //This will be used when trying to send the file again
                        SourcefilesClone.Remove(file.Key);
                        SourcefilesClone.Add(desFileName, file.Value);

                        this.ErrorMessage = sftp.ErrorMessage;
                    }

                    serverInformation.OrigSourceFileName = tempOrigSourceFileName;
                }

                Sourcefiles = SourcefilesClone;

                if (this.serverInformation.CountSendAttempt == 0 &&
                this.serverInformation.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1 &&
                this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND &&
                !this.serverInformation.FileNamingConvention.Equals(string.Empty))
                {
                    IAPL.Transport.Data.DbTransaction DBTrans = new IAPL.Transport.Data.DbTransaction();
                    DBTrans.UpdateMsgCounter(this.serverInformation.MessageCode, this.serverInformation.FileCounter);
                }

                if (((Hashtable)Sourcefiles).Count > 0)
                {
                    if (this.serverInformation.CountSendAttempt == 0 &&
                        !htFiles.ContainsKey(this.serverInformation.MessageCode))
                        htFiles.Add(this.serverInformation.MessageCode, Sourcefiles);

                    this.ErrorMessage = "SFTP-MoveFileFromLocalFolder()|one or more files have not been transmitted successfully to destination";

                    if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND &&
                        this.serverInformation.CountSendAttempt >= 4)
                    {
                        RequeueFile theRequeueFile = new RequeueFile();

                        foreach (System.Collections.DictionaryEntry file in (Hashtable)Sourcefiles)
                            theRequeueFile.SaveRequeueFile(file.Value.ToString(), file.Key.ToString(), msgDetails, this.serverInformation, this.serverInformation);

                        if (htFiles.Count > 0)
                        {
                            if (htFiles.ContainsKey(this.serverInformation.MessageCode))
                                htFiles.Remove(this.serverInformation.MessageCode);
                        }
                    }
                }
                else
                {
                    if (htFiles.Count > 0)
                    {
                        if (htFiles.ContainsKey(this.serverInformation.MessageCode))
                            htFiles.Remove(this.serverInformation.MessageCode);
                    }
                }
                //END SR#33117 Ccenriquez / Capad -- November 19, 2009

                //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009
//                //Get Files from Source
//                foreach (DictionaryEntry file in (Hashtable)Sourcefiles)
//                {
//                    string FileToUpload = file.Value.ToString();
//                    string tempOrigSourceFileName = serverInformation.OrigSourceFileName;
//                    serverInformation.OrigSourceFileName = desServerDetails.getFileNameOnly(FileToUpload);

////                    if (!sftp.Upload(file.Value.ToString(), DestLocalFolderPath + @"/" + desServerDetails.getFileNameOnly(file.Value.ToString())))
//                    if (!sftp.Upload(file.Value.ToString(), DestLocalFolderPath + @"/" + serverInformation.GenFileName()))
//                    {
//                        this.ErrorMessage = sftp.ErrorMessage;
//                        success = false;
//                    }
//                    serverInformation.OrigSourceFileName = tempOrigSourceFileName;

//                    // Delete file from Local folder
//                    System.IO.File.Delete(file.Value.ToString());

//                    if (!serverInformation.FileNamingConvention.Equals(string.Empty))
//                    { serverInformation.IncrementCounter(); }
//                }
                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        // **********************************************************************************


        // **********************************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12043
        // Date: September 22, 2008

        public bool CheckSentFilesFromFTPServer(IAPL.Transport.Transactions.MessageDetails msgDetails,
                                                string FileToCheck,
                                                int FileSizeCheckCtr,
                                                out bool FileFound,
                                                out bool FileSizeEqual)
        {
            FileSizeEqual = false;
            FileFound = false;

            string Terminator = msgDetails.FITEFileMask.Substring(msgDetails.FITEFileMask.LastIndexOf(".") + 1);
            bool success = true;

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                                                                                   this.serverInformation.ServerUserName,
                                                                                   this.serverInformation.ServerPassword,
                                                                                   this.serverInformation.ServerPort);

            // Establish connection to SFTP server
            if (sftp.Connect())
            {
                string FileExt = FileToCheck.Substring(FileToCheck.LastIndexOf(".") + 1);

                //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                //if (!FileExt.Equals(Terminator))

                //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                if (!string.Equals(FileExt, Terminator, StringComparison.OrdinalIgnoreCase))
                {
                    string Filename = serverInformation.getFileNameOnly(FileToCheck);
                    FileFound = sftp.FileExist(FileUploaded, serverInformation.ServerFolder);
                    if (FileFound)
                    {
                        // Check if file size is the same
                        long LocalFileSize = IAPL.Transport.Util.Utility.GetFileSize_Local(msgDetails.BackupFolder + @"\" + FileToCheck);
                        long FTPSize = sftp.GetFileSize(Filename, serverInformation.ServerFolder);
                        if (LocalFileSize.Equals(FTPSize))
                        {
                            FileSizeEqual = true;
                        }
                        else
                        {
                            // If 3 tries reached and still file size in local vs destination is not equal, then log this
                            if (FileSizeCheckCtr.Equals(3))
                            { IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Max tries reached in copying " + FileToCheck); }
                        }
                    }
                }

                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        public bool CheckIMSSentFilesFromFTPServer(IAPL.Transport.Transactions.MessageDetails msgDetails,
                                                   Hashtable files,
                                                   int FileSizeCheckCtr,
                                                   out Hashtable FileFoundList,
                                                   out Hashtable FileNotFoundList)
        {
            string Terminator = msgDetails.FITEFileMask.Substring(msgDetails.FITEFileMask.LastIndexOf(".") + 1);
            FileFoundList = new Hashtable();
            FileNotFoundList = new Hashtable();
            bool success = true;

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                                                                                  this.serverInformation.ServerUserName,
                                                                                  this.serverInformation.ServerPassword,
                                                                                  this.serverInformation.ServerPort);

            // Establish connection to SFTP server
            if (sftp.Connect())
            {
                bool FileFound = false;
                string CountryCode = "";
                foreach (DictionaryEntry file in (Hashtable)files)
                {
                    string FileExt = file.Value.ToString().Substring(file.Value.ToString().LastIndexOf(".") + 1);
                    if (!FileExt.Equals(Terminator))
                    {
                        string FilePath = file.Value.ToString();
                        string Filename = serverInformation.getFileNameOnly(FilePath);
                        FileFound = sftp.FileExist(Filename, serverInformation.ServerFolder);
                        CountryCode = Filename.Substring(0, 3);
                        if (FileFound)
                        {
                            // Check if file size is the same
                            string LocalIMSFilePath = FilePath;
                            long LocalFileSize = IAPL.Transport.Util.Utility.GetFileSize_Local(msgDetails.BackupFolder + @"\" + LocalIMSFilePath);
                            long FTPSize = sftp.GetFileSize(Filename, serverInformation.ServerFolder);
                            if (LocalFileSize.Equals(FTPSize))
                            { 
                                FileFoundList.Add(CountryCode, FilePath);

                                IAPL.Transport.Data.DbTransaction DBTrans = new IAPL.Transport.Data.DbTransaction();
                                DBTrans.UpdateIMSFileStatus(msgDetails.IMSProcessId, CountryCode);
                            }
                            else
                            {
                                FileNotFoundList.Add(CountryCode, FilePath);

                                // If 3 tries reached and still file size in local vs destination is not equal, then log this
                                if (FileSizeCheckCtr.Equals(3))
                                { IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Max tries reached in copying " + FilePath); }
                            }
                        }
                        else
                        { FileNotFoundList.Add(CountryCode, FilePath); }
                    }
                }
                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        private string _FileUploaded;
        public string FileUploaded
        {
            get
            {
                return this._FileUploaded;
            }
            set
            {
                this._FileUploaded = value;
            }
        }

        private Hashtable _FTPFileFoundSentList = new Hashtable();
        public Hashtable FTPFileFoundSentList
        {
            get
            { return this._FTPFileFoundSentList; }

            set
            {
                _FTPFileFoundSentList.Clear();
                foreach (DictionaryEntry file in (Hashtable)value)
                {
                    _FTPFileFoundSentList.Add("file" + (_FTPFileFoundSentList.Count + 1), file.Value);
                }
            }
        }

        // **********************************************************************************


        public bool MoveAllFilesFromLocalFolder(string SrcFTPFolderPath, string DestLocalFolderPath)
        {
            bool success = true;

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            IAPL.Transport.Transactions.ServerDetails desServerDetails = new IAPL.Transport.Transactions.ServerDetails();

            if (sftp.Connect())
            {
                //Get Files from Source
                foreach (string file in System.IO.Directory.GetFiles(SrcFTPFolderPath))
                {
                    if (!sftp.Upload(file, DestLocalFolderPath + @"/" + desServerDetails.getFileNameOnly(file)))
                    {
                        this.ErrorMessage = sftp.ErrorMessage;
                        success = false;
                    }

                    // Delete file from Local folder
                    System.IO.File.Delete(file);
                }

                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        public bool MoveAllFiles2LocalFolder(string SrcFTPFolderPath, string DestLocalFolderPath, object files,
                                             IAPL.Transport.Transactions.MessageDetails msgDetails)
        {
            bool success = true;

            IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            if (sftp.Connect())
            {
                //Get Files from Source
                foreach (DictionaryEntry file in (Hashtable)files)
                {
                    if (!sftp.Download(SrcFTPFolderPath + @"/" + file.Value.ToString(), DestLocalFolderPath + @"\" + file.Value.ToString(), msgDetails.MsetFilePickupDelay))
                    {
                        this.ErrorMessage = sftp.ErrorMessage;
                        success = false;
                    }
                }

                //Delete Files from Source                
                foreach (DictionaryEntry file in (Hashtable)files)
                {
                    if (!sftp.DeleteRemoteFile(SrcFTPFolderPath + @"/" + file.Value.ToString()))
                    {
                        //continue transaction even if there is error
                        // error
                        //this.ErrorMessage = ftp.ErrorMessage;
                        //success = false;
                    }
                }

                sftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = sftp.ErrorMessage;
                success = false;
            }
            return success;
        }


        #region Old StartProcess Code
        //public bool StartProcess(string fileName, string threadName, string desFilePath)
        //{
        //    bool success = false;
        //    ArrayList list = new ArrayList();

        //    if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
        //    {
        //        list.Add(this.serverInformation.ServerAddress);
        //        list.Add(this.serverInformation.ServerUserName);
        //        list.Add(this.serverInformation.ServerPassword);

        //        fileName = this.serverInformation.GetFTPSourceFile(fileName);

        //        list.Add(fileName);

        //        if (!IAPL.Transport.Util.CommonTools.DirectoryExist(desFilePath))
        //        {
        //            this.ErrorMessage = "SftpTransaction-StartProcess()|Backup folder does not exist. Failed to create backup folder! Failed to backup " +
        //                fileName + " to " + desFilePath;
        //            success = false;
        //        }

        //        desFilePath = this.serverInformation.GetBackupFolderPathWihFileName(desFilePath);
        //        //desFilePath = this.serverInformation.GetNetworkSourceFile(desFilePath, this.serverInformation.GenFileName());

        //        list.Add(desFilePath);
        //        list.Add(threadName);


        //        success = this.MoveFile2LocalFolder(list);

        //    }
        //    else if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
        //    {
        //        list.Add(this.serverInformation.ServerAddress);
        //        list.Add(this.serverInformation.ServerUserName);
        //        list.Add(this.serverInformation.ServerPassword);

        //        string srcFilePath = desFilePath;

        //        srcFilePath = this.serverInformation.GetNetworkSourceFile(srcFilePath, fileName);

        //        list.Add(srcFilePath);

        //        desFilePath = this.serverInformation.GetSFTPSourceFile(this.serverInformation.GenFileName());

        //        list.Add(desFilePath);
        //        //i++;
        //        list.Add(threadName);

        //        success = this.MoveFileFromLocalFolder(list);
        //    }

        //    return success;
        //}
        #endregion


        // *********************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        // Date: July 23, 2008

        #region startprocess
        public bool StartProcess(string fileName, Hashtable fileNames, object incompletefiles, string threadName, string srcPath, string desFilePath, int ProcessCode,
                                 bool FilesSentSingle, Hashtable fitefiles, string IncludeTermInCondition5, string DumpFolder,
                                 IAPL.Transport.Transactions.MessageDetails msgDetails, 
                                 out Hashtable DumpFiles, out Hashtable TerminatorFiles) // Alrazen Estrella | ISG12152 | July 28, 2008
        {
            //OLD CODE SR# 34056 Ccenriquez -- November 26, 2009
            //bool success = false;

            //NEW CODE SR# 34056 Ccenriquez -- November 26, 2009
            bool success = true;

            DumpFiles = new Hashtable();
            TerminatorFiles = new Hashtable(); 
            ArrayList list = new ArrayList();

            list.Add(this.serverInformation.ServerAddress);
            list.Add(this.serverInformation.ServerUserName);
            list.Add(this.serverInformation.ServerPassword);

            if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
            {
                list.Add(this.serverInformation.GetFTPSourcePath());
            }
            else if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
            {
                list.Add(srcPath);
            }

            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(desFilePath))
            {
                this.ErrorMessage = "SftpTransaction-StartProcess()|Backup folder does not exist. Failed to create backup folder! Failed to backup " +
                    fileName + " to " + desFilePath;
                success = false;
            }
            else
            {
                list.Add(desFilePath);
                list.Add(threadName);
                list.Add(fileName);
                list.Add(FilesSentSingle); 

                // Move file
                if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
                {
                    // Move file from Source to Backup folder
                    //success = this.MoveFile2LocalFolder(list, ProcessCode);
                    success = this.MoveFiles2LocalFolder(list, fileNames, incompletefiles, ProcessCode, fitefiles, IncludeTermInCondition5, DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);  // Alrazen Estrella | ISG12152 | July 29, 2008
                }
                else if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
                {
                    // Move file from Backup to Destination folder
                    //success = this.MoveFileFromLocalFolder(list);

                    //OLD CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
                    //success = this.MoveFileFromLocalFolder(list, fileNames, ProcessCode);   // Alrazen Estrella | ISG12152 | July 29, 2008

                    //NEW CODE SR#33117 Ccenriquez / Capad -- November 19, 2009 
                    success = this.MoveFileFromLocalFolder(msgDetails, list, fileNames, ProcessCode);   // Alrazen Estrella | ISG12152 | July 29, 2008
                }
            }
            return success;
        }
        #endregion
    }
}
