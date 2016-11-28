using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;

using System.IO;

namespace IAPL.Transport.Transactions
{
    class FtpTransaction
    {
        //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 18, 2009
        private static Hashtable htFiles = new Hashtable(); //this will hold generated file counter need for requeue in case failed to transmit to destination

        private IAPL.Transport.Transactions.ServerDetails serverInformation = null;
        private string errorMessage = "";

        //private IAPL.Transport.Util.ConstantVariables.FileDirection fileDirection = IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE;
        //private object transferInfo = null;

        //public FtpTransaction(string fileDirection, object transerInfo) {
        public FtpTransaction(IAPL.Transport.Transactions.ServerDetails transferInfo)
        {
            this.serverInformation = transferInfo;
        }
        public FtpTransaction()
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

        #region methods

        // ********************************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        // Date: July 24, 2008

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

        public bool MoveAllFilesFromLocalFolder(string SrcFTPFolderPath, string DestLocalFolderPath)
        {
            bool success = true;

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            IAPL.Transport.Transactions.ServerDetails desServerDetails = new IAPL.Transport.Transactions.ServerDetails();

            if (ftp.Connect())
            {
                //Get Files from Source
                foreach (string file in System.IO.Directory.GetFiles(SrcFTPFolderPath))
                {
                    if (!ftp.Upload(file, DestLocalFolderPath + @"/" + desServerDetails.getFileNameOnly(file)))
                    {
                        this.ErrorMessage = ftp.ErrorMessage;
                        success = false;
                    }

                    // Delete file from Local folder
                    System.IO.File.Delete(file);
                }

                ftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        public bool MoveAllFilesFromLocalFolder2(string SourceLocalFolderPath, string DestLocalFolderPath, object Sourcefiles)
        {
            bool success = true;

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            IAPL.Transport.Transactions.ServerDetails desServerDetails = new IAPL.Transport.Transactions.ServerDetails();

            if (ftp.Connect())
            {
                //Get Files from Source
                foreach (DictionaryEntry file in (Hashtable)Sourcefiles)
                {
                    if (!ftp.Upload(SourceLocalFolderPath + @"\" + file.Value.ToString(), DestLocalFolderPath + @"/" + file.Value.ToString()))
                    {
                        this.ErrorMessage = ftp.ErrorMessage;
                        success = false;
                    }

                    // Delete file from Local folder
                    System.IO.File.Delete(SourceLocalFolderPath + @"\" + file.Value.ToString());

                }
                ftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        #region MoveAllFilesFromListToFTP v1.3
        //        public bool MoveAllFilesFromListToFTP(string DestLocalFolderPath, object Sourcefiles)
        //        {
        //            bool success = true;

        //            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
        //                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword);

        //            IAPL.Transport.Transactions.ServerDetails desServerDetails = new IAPL.Transport.Transactions.ServerDetails();

        //            if (ftp.Connect())
        //            {
        //                //Get Files from Source
        //                foreach (DictionaryEntry file in (Hashtable)Sourcefiles)
        //                {
        //                    string FileToUpload = file.Value.ToString();
        //                    string tempOrigSourceFileName = serverInformation.OrigSourceFileName;
        //                    serverInformation.OrigSourceFileName = desServerDetails.getFileNameOnly(FileToUpload);

        ////                    if (!ftp.Upload(file.Value.ToString(), DestLocalFolderPath + @"/" + desServerDetails.getFileNameOnly(file.Value.ToString())))
        //                    if (!ftp.Upload(FileToUpload, DestLocalFolderPath + @"/" + serverInformation.GenFileName()))
        //                    {
        //                        this.ErrorMessage = ftp.ErrorMessage;
        //                        success = false;
        //                    }
        //                    serverInformation.OrigSourceFileName = tempOrigSourceFileName;

        //                    // Delete file from Local folder
        //                    System.IO.File.Delete(FileToUpload);

        //                    if (!serverInformation.FileNamingConvention.Equals(string.Empty))
        //                    { serverInformation.IncrementCounter(); }
        //                }
        //                ftp.Disconnect();
        //            }
        //            else
        //            {
        //                this.ErrorMessage = ftp.ErrorMessage;
        //                success = false;
        //            }
        //            return success;
        //        }
        #endregion

        //OLD CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 
        //public bool MoveAllFilesFromListToFTP(string DestLocalFolderPath, object Sourcefiles)

        //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009 
        public bool MoveAllFilesFromListToFTP(string DestLocalFolderPath, object Sourcefiles, MessageDetails msgDetails)
        {
            bool success = true;

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            IAPL.Transport.Transactions.ServerDetails desServerDetails = new IAPL.Transport.Transactions.ServerDetails();

            if (ftp.Connect())
            {
                //BEGIN SR#33117 Ccenriquez / Capad -- November 18, 2009
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

                    success = ftp.Upload(FileToUpload, DestLocalFolderPath + @"/" + desFileName);

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

                        this.ErrorMessage = ftp.ErrorMessage;
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

                    this.ErrorMessage = "FTP-MoveFileFromLocalFolder()|one or more files have not been transmitted successfully to destination";

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
                //END SR#33117 Ccenriquez / Capad -- November 18, 2009

                //OLD CODE SR#33117 Ccenriquez / Capad -- November 18, 2009
                ////Get Files from Source
                //foreach (DictionaryEntry file in (Hashtable)Sourcefiles)
                //{
                //    string FileToUpload = file.Value.ToString();
                //    string tempOrigSourceFileName = serverInformation.OrigSourceFileName;
                //    serverInformation.OrigSourceFileName = desServerDetails.getFileNameOnly(FileToUpload);

                //    //                    if (!ftp.Upload(file.Value.ToString(), DestLocalFolderPath + @"/" + desServerDetails.getFileNameOnly(file.Value.ToString())))
                //    if (!ftp.Upload(FileToUpload, DestLocalFolderPath + @"/" + serverInformation.GenFileName()))
                //    {
                //        this.ErrorMessage = ftp.ErrorMessage;
                //        success = false;
                //    }
                //    serverInformation.OrigSourceFileName = tempOrigSourceFileName;

                //    // Delete file from Local folder
                //    //System.IO.File.Delete(FileToUpload);

                //    if (!serverInformation.FileNamingConvention.Equals(string.Empty))
                //    { serverInformation.IncrementCounter(); }
                //}

                ftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }
            return success;
        }


        public bool MoveAllFiles2LocalFolder(string SrcFTPFolderPath, string DestLocalFolderPath, object files,
                                             IAPL.Transport.Transactions.MessageDetails msgDetails)
        {
            bool success = true;

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            if (ftp.Connect())
            {
                //Get Files from Source
                foreach (DictionaryEntry file in (Hashtable)files)
                {
                    if (!ftp.Download(SrcFTPFolderPath + @"/" + file.Value.ToString(), DestLocalFolderPath + @"\" + file.Value.ToString(), msgDetails.MsetFilePickupDelay))
                    {
                        this.ErrorMessage = ftp.ErrorMessage;
                        success = false;
                    }
                }

                //Delete Files from Source                
                foreach (DictionaryEntry file in (Hashtable)files)
                {
                    if (!ftp.DeleteRemoteFile(SrcFTPFolderPath + @"/" + file.Value.ToString()))
                    {
                        //continue transaction even if there is error
                        // error
                        //this.ErrorMessage = ftp.ErrorMessage;
                        //success = false;
                    }
                }

                ftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        // ********************************************************************************

        // **********************************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12043
        // Date: September 22, 2008

        //public bool CheckSentFilesFromFTPServer(IAPL.Transport.Transactions.MessageDetails msgDetails,
        //                                        string FileToCheck, 
        //                                        int FileSizeCheckCtr,
        //                                        out bool FileFound,
        //                                        out bool FileSizeEqual)
        //{
        //    FileSizeEqual = false;
        //    FileFound = false;

        //    string Terminator = msgDetails.FITEFileMask.Substring(msgDetails.FITEFileMask.LastIndexOf(".") + 1); 
        //    bool success = true;

        //    IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
        //                                                                        this.serverInformation.ServerUserName,
        //                                                                        this.serverInformation.ServerPassword);

        //    // Establish connection to FTP server
        //    if (ftp.Connect())
        //    {
        //        string FileExt = FileToCheck.Substring(FileToCheck.LastIndexOf(".") + 1);
        //        if (!FileExt.Equals(Terminator))
        //        {
        //            string Filename = serverInformation.getFileNameOnly(FileToCheck);
        //            FileFound = ftp.FileExist(FileUploaded, serverInformation.ServerFolder);
        //            if (FileFound)
        //            { 
        //                // Check if file size is the same
        //                long LocalFileSize = IAPL.Transport.Util.Utility.GetFileSize_Local(msgDetails.BackupFolder + @"\" + FileToCheck);  
        //                long FTPSize = ftp.GetFileSize(Filename, serverInformation.ServerFolder);
        //                if (LocalFileSize.Equals(FTPSize))
        //                {
        //                    FileSizeEqual = true;
        //                }
        //                else
        //                {
        //                    // If 3 tries reached and still file size in local vs destination is not equal, then log this
        //                    if (FileSizeCheckCtr.Equals(3))
        //                    { IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", "Max tries reached in copying " + FileToCheck); }
        //                }
        //            }
        //        }

        //        ftp.Disconnect();
        //    }
        //    else
        //    {
        //        this.ErrorMessage = ftp.ErrorMessage;
        //        success = false;
        //    }
        //    return success;
        //}


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

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                                                                                this.serverInformation.ServerUserName,
                                                                                this.serverInformation.ServerPassword,
                                                                                this.serverInformation.ServerPort
                                                                                );

            // Establish connection to FTP server
            if (ftp.Connect())
            {
                bool FileFound = false;
                string CountryCode = "";
                foreach (DictionaryEntry file in (Hashtable)files)
                {
                    string FileExt = file.Value.ToString().Substring(file.Value.ToString().LastIndexOf(".") + 1);

                    //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                    //if (!FileExt.Equals(Terminator))

                    //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                    if (!string.Equals(FileExt, Terminator, StringComparison.OrdinalIgnoreCase))
                    {
                        string FilePath = file.Value.ToString();
                        string Filename = NamedConventionFiles[serverInformation.getFileNameOnly(FilePath)].ToString();
                        FileFound = ftp.FileExist(Filename, serverInformation.ServerFolder);
                        CountryCode = Filename.Substring(0, 3);
                        if (FileFound)
                        {
                            // Check if file size is the same
                            string LocalIMSFilePath = FilePath;
                            long LocalFileSize = IAPL.Transport.Util.Utility.GetFileSize_Local(msgDetails.BackupFolder + @"\" + msgDetails.IMSFolder + @"\" + LocalIMSFilePath);
                            long FTPSize = ftp.GetFileSize(Filename, serverInformation.ServerFolder);
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
                ftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        private Hashtable _NamedConventionFiles = new Hashtable();
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
                    _FTPFileFoundSentList.Add(file.Key, file.Value);
                }
            }
        }

        // **********************************************************************************


        #region MoveFile2LocalFolder
        private bool MoveFile2LocalFolder(object data, IAPL.Transport.Transactions.MessageDetails msgDetails)
        {
            bool success = true;
            ArrayList list = (ArrayList)data;
            string remoteServer = (string)list[0];
            string remoteUser = (string)list[1];
            string remotePass = (string)list[2];
            //string remotePath = (string)list[3];
            string remoteFile = (string)list[3];
            //string localPath = (string)list[5];
            string localFile = (string)list[4];
            string threadName = (string)list[5];

            //System.Console.WriteLine("Thread {0} is running ", threadName);

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                    this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            if (ftp.Connect())
            {
                if (ftp.Download(remoteFile, localFile, msgDetails.MsetFilePickupDelay))
                {
                    if (ftp.DeleteRemoteFile(remoteFile))
                    {

                    }
                    else
                    { // error
                        this.ErrorMessage = ftp.ErrorMessage;
                        success = false;
                    }
                }
                else
                { // error
                    this.ErrorMessage = ftp.ErrorMessage;
                    success = false;
                }

                ftp.Disconnect();

                //System.Console.WriteLine("Thread {0} is done. ", threadName);
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }
            return success;
        }

        //LENIN - ISG11957 - ADD - 11-27-2007
        //private bool MoveFiles2LocalFolder(object data, object files, int ProcessCode)
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

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                    this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            string TerminatorFileExt = fileTerminator.Substring(fileTerminator.LastIndexOf(".") + 1);

            if (ftp.Connect())
            {
                int ctr1 = 0;
                int ctr2 = 0;
                if (ProcessCode.Equals(5))
                {
                    // Download file from Source to Dump folder
                    if (!ftp.Download(remotePath + @"/" + fileTerminator, DumpFolder + @"\" + fileTerminator, msgDetails.MsetFilePickupDelay))
                    {
                        this.ErrorMessage = ftp.ErrorMessage;
                        success = false;
                    }
                    else
                    {
                        //OLD CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        //if (TerminatorFileExt != IncludeTermInCondition5.Substring(IncludeTermInCondition5.LastIndexOf(".") + 1))

                        //NEW CODE SR#33117 Ccenriquez -- December 2, 2009 - Fix FITE send to Destination
                        if (TerminatorFileExt.ToLower() != IncludeTermInCondition5.Substring(IncludeTermInCondition5.LastIndexOf(".") + 1).ToLower())
                        {
                            // If not Terminator
                            ctr1++;
                            DumpFiles.Add("file" + ctr1, DumpFolder + @"\" + serverInformation.getFileNameOnly(fileTerminator));
                        }
                        else
                        {
                            // If Terminator
                            ctr2++;
                            TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + IncludeTermInCondition5);
                        }

                        // Delete Remote file                        
                        if (!ftp.DeleteRemoteFile(remotePath + @"/" + fileTerminator))
                        { }
                    }
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
                            if (msgDetails.IndividualProcess.Equals(1) ||
                                (fitefiles.Count.Equals(0) && (ProcessCode.Equals(4) || ProcessCode.Equals(6))))
                            {
                                string SourceFilePath = remotePath + @"/" + FilenametoDownload;
                                string DestinationFilePath = DumpFolder + @"\" + FilenametoDownload;
                                if (!ftp.Download(SourceFilePath, DestinationFilePath, msgDetails.MsetFilePickupDelay))
                                {
                                    this.ErrorMessage = ftp.ErrorMessage;
                                    success = false;
                                }
                                else
                                {
                                    // Delete Remote file                        
                                    if (!ftp.DeleteRemoteFile(remotePath + @"/" + FilenametoDownload))
                                    { }

                                    ctr1++;
                                    DumpFiles.Add("file" + ctr1, DumpFolder + @"\" + serverInformation.getFileNameOnly(FilenametoDownload));
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
                            if (!ftp.Download(remotePath + @"/" + FilenametoDownload, DumpFolder + @"\" + FilenametoDownload, msgDetails.MsetFilePickupDelay))
                            {
                                this.ErrorMessage = ftp.ErrorMessage;
                                success = false;
                            }
                            else
                            {
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
                                    string SourceFilePath = "";
                                    string DestinationFilePath = "";

                                    // Copy File Terminator (Fite filename) to Backup folder
                                    string FileTerminator = "";
                                    if (filesentsingle)
                                    {
                                        FileTerminator = FilenametoDownload.Replace("." + FileFileExt, "." + TerminatorFileExt);
                                        SourceFilePath = remotePath + @"/" + FileTerminator;
                                        DestinationFilePath = DumpFolder + @"\" + FileTerminator;
                                        if (!ftp.Download(SourceFilePath, DestinationFilePath, msgDetails.MsetFilePickupDelay))
                                        {
                                            this.ErrorMessage = ftp.ErrorMessage;
                                            success = false;
                                        }

                                        ctr2++;
                                        TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + FileTerminator);

                                        //Delete File Terminator from Source
                                        if (!ftp.DeleteRemoteFile(SourceFilePath))
                                        { }
                                    }
                                    else
                                    {
                                        if ((!fitefiles.Count.Equals(0)) && ((ProcessCode.Equals(4)) || (ProcessCode.Equals(6))))
                                        {
                                            SourceFilePath = remotePath + @"/" + FilenametoDownload;
                                            DestinationFilePath = DumpFolder + @"\" + FilenametoDownload;
                                            if (!ftp.Download(SourceFilePath, DestinationFilePath, msgDetails.MsetFilePickupDelay))
                                            {
                                                this.ErrorMessage = ftp.ErrorMessage;
                                                success = false;
                                            }

                                            //Delete File Terminator from Source
                                            if (!ftp.DeleteRemoteFile(SourceFilePath))
                                            { }
                                        }
                                    }
                                }
                            }

                            // Delete Remote file                        
                            if (!ftp.DeleteRemoteFile(remotePath + @"/" + FilenametoDownload))
                            { }
                        }
                    }

                    // FOR IMS: Move Incomplete files to IMS Incomplete folder
                    if (msgDetails.IndividualProcess.Equals(1))
                    {
                        // Create IMSIncomplete folder 
                        int _endValue = msgDetails.BackupFolder.LastIndexOf("Backup");
                        string IncFolder = msgDetails.BackupFolder.Substring(0, _endValue) + IAPL.Transport.IMS.Process.IMSIncompleteFolder;
                        if (!IAPL.Transport.Util.CommonTools.DirectoryExist(IncFolder))
                        { System.IO.Directory.CreateDirectory(IncFolder); }

                        foreach (DictionaryEntry file in (Hashtable)incompletefiles)
                        {
                            string DestinationFilePath = IncFolder + @"\" + file.Value.ToString();
                            if (!ftp.Download(remotePath + @"/" + file.Value.ToString(),
                                              DestinationFilePath,
                                              msgDetails.MsetFilePickupDelay))
                            {
                                this.ErrorMessage = ftp.ErrorMessage;
                                success = false;
                            }
                            else
                            {
                                // Delete Remote file                        
                                if (!ftp.DeleteRemoteFile(remotePath + @"/" + file.Value.ToString()))
                                { }
                            }
                        }
                    }

                    if (!filesentsingle)
                    {
                        foreach (DictionaryEntry file in (Hashtable)fitefiles)
                        {
                            string SourceFilePath = remotePath + @"/" + file.Value.ToString();
                            string DestinationFilePath = DumpFolder + @"\" + file.Value.ToString();
                            if (!ftp.Download(SourceFilePath, DestinationFilePath, msgDetails.MsetFilePickupDelay))
                            {
                                this.ErrorMessage = ftp.ErrorMessage;
                                success = false;
                            }

                            ctr2++;
                            TerminatorFiles.Add("file" + ctr2, DumpFolder + @"\" + file.Value.ToString());

                            //Delete File Terminator from Source
                            if (!ftp.DeleteRemoteFile(SourceFilePath))
                            { }
                        }
                    }
                }
                ftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }
            return success;
        }
        #endregion

        #region MoveFileFromLocalFolder
        //private bool MoveFileFromLocalFolder(object data)
        private bool MoveFileFromLocalFolder(IAPL.Transport.Transactions.MessageDetails msgDetails,
                                             object data, object files, int ProcessCode)      // Edited by Alrazen Estrella, ISG12152, July 24, 2008
        {
            bool success = true;
            ArrayList list = (ArrayList)data;
            string remoteServer = (string)list[0];
            string remoteUser = (string)list[1];
            string remotePass = (string)list[2];
            //string remotePath = (string)list[3];
            string remoteFile = (string)list[3];
            //string localPath = (string)list[5];
            string localFile = (string)list[4];
            string threadName = (string)list[5];
            string fileTerminator = (string)list[6];

            IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(this.serverInformation.ServerAddress,
                    this.serverInformation.ServerUserName, this.serverInformation.ServerPassword, this.serverInformation.ServerPort);

            // Establish connection to FTP server
            if (ftp.Connect())
            {

                // *******************************************************************
                // Developer: Alrazen Estrella
                // Project: ISG12152
                // Date: July 24, 2008

                #region OLD CODE
                // Old code
                //if (ftp.Upload(remoteFile, localFile))
                //{
                //    // dont delete the backup file
                //    //IAPL.Transport.Operation.Network localFolder = new IAPL.Transport.Operation.Network();
                //    //localFolder.DeleteRemoteFile(remoteFile);

                //}
                //else
                //{ // error
                //    this.ErrorMessage = ftp.ErrorMessage;
                //    success = false;
                //}
                # endregion

                // New Code
                // Upload files to FTP Server
                if (ProcessCode.Equals(1) ||
                    ProcessCode.Equals(2) ||
                    ProcessCode.Equals(3))
                {
                    //BEGIN SR#33117 Ccenriquez / Capad -- November 18, 2009
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

                        bool AddToConventionName = true;
                        string ConventionName = "";
                        if (NamedConventionFiles.Count.Equals(0))
                        {
                            ConventionName = (this.serverInformation.CountSendAttempt == 0) ? this.serverInformation.GenFileName() : file.Key.ToString();
                        }
                        else
                        {
                            if (NamedConventionFiles[FileToUpload] == null)
                            {
                                ConventionName = (this.serverInformation.CountSendAttempt == 0) ? this.serverInformation.GenFileName() : file.Key.ToString();
                                ConventionName = this.serverInformation.GenFileName();
                            }
                            else
                            {
                                ConventionName = NamedConventionFiles[FileToUpload].ToString();
                                AddToConventionName = false;
                            }
                        }

                        if (FileToUpload != string.Empty && FileToUpload.IndexOf(@"\") > -1)
                        {
                            int lastIndex = FileToUpload.LastIndexOf(@"\");
                            FileToUpload = FileToUpload.Substring(lastIndex + 1, FileToUpload.Length - lastIndex - 1);
                        }

                        if (this.serverInformation.CountSendAttempt == 0 && this.serverInformation.FileNamingConvention != string.Empty && this.serverInformation.FileNamingConvention.IndexOf("<CTR>", StringComparison.OrdinalIgnoreCase) > -1)
                            this.serverInformation.IncrementCounter();

                        success = ftp.Upload(remoteFile + @"\" + FileToUpload, localFile + @"/" + ConventionName);

                        if (success)
                        {
                            if (filesClone.ContainsValue(file.Value.ToString()))
                                filesClone.Remove(file.Key.ToString());
                        }
                        else
                        {
                            //This will be used when trying to send the file again
                            filesClone.Remove(file.Key);
                            filesClone.Add(ConventionName, file.Value);

                            this.ErrorMessage = ftp.ErrorMessage;
                        }

                        if (AddToConventionName)
                            NamedConventionFiles.Add(FileToUpload, ConventionName);
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

                        this.ErrorMessage = "FTP-MoveFileFromLocalFolder()|one or more files have not been transmitted successfully to destination";

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
                    //END SR#33117 Ccenriquez / Capad -- November 18, 2009

                    //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 18, 2009
                    //foreach (DictionaryEntry file in (Hashtable)files)
                    //{
                    //    string FileToUpload = file.Value.ToString();
                    //    string tempOrigSourceFileName = serverInformation.OrigSourceFileName;
                    //    serverInformation.OrigSourceFileName = FileToUpload;

                    //    bool AddToConventionName = true;
                    //    string ConventionName = "";
                    //    if (NamedConventionFiles.Count.Equals(0))
                    //    {
                    //        ConventionName = this.serverInformation.GenFileName();
                    //    }
                    //    else
                    //    {
                    //        if (NamedConventionFiles[FileToUpload] == null)
                    //        {
                    //            ConventionName = this.serverInformation.GenFileName();
                    //        }
                    //        else
                    //        {
                    //            ConventionName = NamedConventionFiles[FileToUpload].ToString();
                    //            AddToConventionName = false;
                    //        }
                    //    }

                    ////BEGIN SR#33117 Ccenriquez / Capad -- November 18, 2009 : BUG FIXED NETWORK TO FTP ZIP SRC - W/ FITE - ZIP FACILITY
                    //if (FileToUpload != string.Empty && FileToUpload.IndexOf(@"\") > -1)
                    //{
                    //    int lastIndex = FileToUpload.LastIndexOf(@"\");
                    //    FileToUpload = FileToUpload.Substring(lastIndex + 1, FileToUpload.Length - lastIndex - 1);
                    //}
                    ////END SR#33117 Ccenriquez / Capad -- November 18, 2009

                    //if (!ftp.Upload(remoteFile + @"\" + FileToUpload, localFile + @"/" + ConventionName))
                    //{
                    //    this.ErrorMessage = ftp.ErrorMessage;
                    //    success = false;
                    //}

                    //    if (AddToConventionName)
                    //        NamedConventionFiles.Add(FileToUpload, ConventionName);

                    //    //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 18, 2009
                    //    //serverInformation.OrigSourceFileName = tempOrigSourceFileName;
                    //    //if (!serverInformation.FileNamingConvention.Equals(string.Empty))
                    //    //{   serverInformation.IncrementCounter();   }

                    //    //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 18, 2009
                    //    if (!this.serverInformation.FileNamingConvention.Equals(string.Empty) && 
                    //        this.serverInformation.CountSendAttempt == 0)
                    //        this.serverInformation.IncrementCounter();
                    //}
                }
                else if (ProcessCode.Equals(4) ||
                         ProcessCode.Equals(6))
                {
                    //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
                    //string FilenameToSend = this.serverInformation.GenFileName();

                    //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
                    string FilenameToSend = this.serverInformation.DesFileName;

                    if (!ftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + FilenameToSend))
                    {
                        this.ErrorMessage = ftp.ErrorMessage;
                        success = false;
                    }

                    this.FileUploaded = FilenameToSend;

                    //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
                    //if (!serverInformation.FileNamingConvention.Equals(string.Empty))
                    //{ serverInformation.IncrementCounter(); }
                }
                else if (ProcessCode.Equals(5))
                {
                    if (serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
                    { }
                    else if (serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
                    {
                        //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
                        //// Add static value to counter
                        //if (!this.serverInformation.FileNamingConvention.Equals(string.Empty))
                        //{
                        //    if (!msgDetails.FITEFileMask.Equals(string.Empty))
                        //    {   serverInformation.FileCounter = serverInformation.FileCounter + FileCtr;    }
                        //}

                        //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
                        // Copy File from Backup to Destination folder
                        //if (!ftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + this.serverInformation.GenFileName()))

                        //NEW CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
                        if (!ftp.Upload(remoteFile + @"\" + fileTerminator, localFile + @"/" + this.serverInformation.DesFileName))
                        {
                            // error
                            this.ErrorMessage = ftp.ErrorMessage;
                            success = false;
                        }

                        //OLD CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009
                        //if (!this.serverInformation.FileNamingConvention.Equals(string.Empty))
                        //{ FileCtr++; }
                    }
                }

                // *******************************************************************

                ftp.Disconnect();
            }
            else
            {
                this.ErrorMessage = ftp.ErrorMessage;
                success = false;
            }

            return success;
        }
        #endregion

        #region startprocess

        //public bool StartProcess(string fileName, string threadName, string desFilePath)
        public bool StartProcess(string fileName, Hashtable fileNames, string threadName, string desFilePath, int ProcessCode,
                                 IAPL.Transport.Transactions.MessageDetails msgDetails)   // Changed: Alrazen Estrella | ISG12152 | July 24, 2008
        {
            //OLD CODE SR# 34056 Ccenriquez -- November 26, 2009
            //bool success = false;

            //NEW CODE SR# 34056 Ccenriquez -- November 26, 2009
            bool success = true;

            ArrayList list = new ArrayList();

            if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
            {
                list.Add(this.serverInformation.ServerAddress);
                list.Add(this.serverInformation.ServerUserName);
                list.Add(this.serverInformation.ServerPassword);

                fileName = this.serverInformation.GetFTPSourceFile(fileName);

                list.Add(fileName);

                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(desFilePath))
                {
                    this.ErrorMessage = "FtpTransaction-StartProcess()|Backup folder does not exist. Failed to create backup folder! Failed to backup " +
                        fileName + " to " + desFilePath;
                    success = false;
                }
                else
                {
                    desFilePath = this.serverInformation.GetBackupFolderPathWihFileName(desFilePath);
                    //desFilePath = this.serverInformation.GetNetworkSourceFile(desFilePath, this.serverInformation.GenFileName());

                    list.Add(desFilePath);
                    list.Add(threadName);

                    success = this.MoveFile2LocalFolder(list, msgDetails);
                }

            }
            else if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
            {
                list.Add(this.serverInformation.ServerAddress);
                list.Add(this.serverInformation.ServerUserName);
                list.Add(this.serverInformation.ServerPassword);

                string srcFilePath = desFilePath;

                srcFilePath = this.serverInformation.GetNetworkSourceFile(srcFilePath, fileName);

                list.Add(srcFilePath);
                desFilePath = this.serverInformation.GetFTPSourceFile(this.serverInformation.GenFileName());
                list.Add(desFilePath);
                list.Add(threadName);


                // ************************************************************
                // Developer: Alrazen Estrella
                // Project: ISG12152
                // Date: July 24, 2008

                // Old Code
                //success = this.MoveFileFromLocalFolder(list);

                // New Code
                success = this.MoveFileFromLocalFolder(msgDetails, list, fileNames, ProcessCode);

                // ************************************************************
            }

            return success;
        }

        /// <summary>
        /// This function is used for File Terminator capability
        /// </summary>
        /// <param name="FITEfilename">Filename of File Terminator</param>
        /// <param name="fileNames">List Files to ZIP</param>
        /// <param name="threadName">The Name of the Thread</param>
        /// <param name="desFilePath">Folder to save the compress file</param>
        /// <returns></returns>
        //public bool StartProcess(string FITEfilename, Hashtable fileNames, string threadName, string desFilePath)
        public bool StartProcess(string FITEfilename, Hashtable fileNames, object incompletefiles, string threadName, string srcPath, string desFilePath, int ProcessCode,
                                 bool FilesSentSingle, Hashtable fitefiles, string IncludeTermInCondition5, string DumpFolder,
                                 IAPL.Transport.Transactions.MessageDetails msgDetails,
                                 out Hashtable DumpFiles, out Hashtable TerminatorFiles)
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

            // *********************************************************
            // Developer: Alrazen Estrella
            // Project: ISG12152
            // Date: July 23, 2008

            if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
            { list.Add(this.serverInformation.GetFTPSourcePath()); }
            else if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
            { list.Add(srcPath); }

            // *********************************************************

            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(desFilePath))
            {
                this.ErrorMessage = "FtpTransaction-StartProcess()|Backup folder does not exist. Failed to create backup folder! Failed to backup " +
                    "files from " + list[3].ToString() + " to " + desFilePath;
                success = false;
            }
            else
            {
                list.Add(desFilePath);
                list.Add(threadName);
                list.Add(FITEfilename);
                list.Add(FilesSentSingle);

                //success = this.MoveFiles2LocalFolder(list, fileNames);

                // ************************************************************************
                // Developer: Alrazen Estrella
                // Project: ISG12152
                // Date: July 23, 2008

                // Move file
                if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
                {
                    // Move file from FTP Source to Dump folder
                    if (Convert.ToBoolean(IAPL.Transport.Configuration.Config.GetAppSettingsValue("FTPDelayPerFile", "true")).Equals(false))
                    {
                        int ToDelay = (60 * 1000) * msgDetails.MsetFilePickupDelay;
                        System.Threading.Thread.Sleep(ToDelay);
                    }
                    success = this.MoveFiles2LocalFolder(list, fileNames, incompletefiles, ProcessCode, fitefiles, IncludeTermInCondition5, DumpFolder, msgDetails, out DumpFiles, out TerminatorFiles);
                }
                else if (this.serverInformation.FileDirection == IAPL.Transport.Util.ConstantVariables.FileDirection.SEND)
                {
                    // Move file from Backup to Destination folder
                    success = this.MoveFileFromLocalFolder(msgDetails, list, fileNames, ProcessCode);
                }
            }
            // ************************************************************************                    

            return success;
        }
        #endregion

        #endregion
    }
}
