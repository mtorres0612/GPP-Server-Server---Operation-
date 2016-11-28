/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Modified GetFileTransferInfo; aligned assignments
 *  MNTORRES 09192016 : Added method GetFileName
 *  
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Concurrent;
using IAPL.Transport.Data;
using IAPL.Transport.Util;
using IAPL.Transport.Operation;
using System.Data;
using IAPL.Transport.Configuration;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.IO;
//using System.Runtime.Remoting.Channels.Tcp;
//using System.Runtime.Remoting.Channels;
//using IAPLRemoteInterface;

namespace IAPL.Transport.Transactions
{
    public class MessageTransaction
    {
        #region local variables
        private bool stopApplication = false;
        private IAPL.Transport.Transactions.PingServer pingServer = null;
        //private IAPL.Transport.Transactions.MessageDetails messageDetails = null;
        private string timeFrameStart = "";
        private string timeFrameEnd = "";
        private string errorMessage = "";
        private int transferDelay = 1500;
        private int maxAttemptOnFailed = 5;
        private static object thislock = new object();

        // ***********************************************************************************
        // Project: ISG12043
        // Developer: Alrazen Estrella
        // Date: Oct. 10, 2008

        private System.Collections.Hashtable ListOfZipFiles = null;
        private System.Collections.Hashtable IMSIncompleteFiles = null;
        private System.Collections.Hashtable IMSCompleteCountriesName = null;
        private System.Collections.Hashtable IMSIncompleteCountriesName = null;

        // ***********************************************************************************

        #endregion

        #region constructors
        public MessageTransaction()
        {
            stopApplication = false;
        }
        #endregion

        #region properties

        //public IAPL.Transport.Transactions.MessageDetails getMessageDetails
        //{
        //    get
        //    {
        //        return this.messageDetails;
        //    }
        //}

        #endregion

        #region methods

        #region retrieve file list
        private System.Collections.Hashtable retrieveFileList(IAPL.Transport.Transactions.ServerDetails sDetails, IAPL.Transport.Transactions.MessageDetails mDetails, bool useFITE)     // Edited: Alrazen Estrella | ISG12152 | July 24, 2008
        {
            System.Collections.Hashtable retTable = new System.Collections.Hashtable();
            this.errorMessage = "";
            switch (sDetails.TransmissionType)
            {
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    #region FTP
                    IAPL.Transport.Operation.FTP ftp = new IAPL.Transport.Operation.FTP(sDetails.ServerAddress,
                        sDetails.ServerUserName, sDetails.ServerPassword, sDetails.ServerPort);

                    if (ftp.Connect())
                    {
                        string checkCurrentRemoteFolder = ftp.GetCurrentDirectory();
                        sDetails.ServerFolder = sDetails.GetFTPFolder(sDetails.ServerFolder);
                        ftp.ChangeDirectory(sDetails.ServerFolder);
                        checkCurrentRemoteFolder = ftp.GetCurrentDirectory();
                        if (sDetails.ServerFolder.Trim().ToLower().Replace(@"/", "").Equals(checkCurrentRemoteFolder.Trim().Replace(@"/", "").ToLower()))
                        {
                            if (useFITE)
                            {
                                // Get list of Terminators
                                retTable = ftp.GetFileList(mDetails.FITEFileMask);
                            }
                            else
                            {
                                // **********************************************************
                                // Project: ISG12043
                                // Developer: Alrazen Estrella
                                // Date: Oct. 6, 2008

                                // Check if IMS
                                if (mDetails.IndividualProcess.Equals(1))
                                {
                                    // IMS 

                                    // List all files in the FTP Server (Source)
                                    System.Collections.Hashtable FileList = new System.Collections.Hashtable();
                                    FileList = ftp.GetFileList("*.*");

                                    // List IMS files
                                    IAPL.Transport.IMS.Process IMSProcess = new IAPL.Transport.IMS.Process();
                                    ListOfZipFiles = new System.Collections.Hashtable();
                                    retTable = IMSProcess.GetIMSFiles(FileList, out ListOfZipFiles, out IMSIncompleteFiles, out IMSCompleteCountriesName, out IMSIncompleteCountriesName);
                                }
                                else
                                {
                                    // Non-IMS 

                                    // Get list of non-terminator files
                                    retTable = ftp.GetFileList(mDetails.SourceFileMask);
                                }

                                // **********************************************************
                            }
                        }
                        else
                        { // the remote folder is not existing
                            this.errorMessage = "MessageTransaction-retrieveFileList()|" +
                                "(FTP IP Address " + sDetails.ServerAddress + ") Remote folder \'" + sDetails.ServerFolder + "\' is not existing!";
                            //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-retrieveFileList()",
                            //    "(FTP IP Address " + sDetails.ServerAddress + ") Remote folder \'" + sDetails.ServerFolder + "\' is not existing!" );
                        }
                        //int i = 0;
                        ftp.Disconnect();
                    }
                    else
                    {
                        //System.Console.WriteLine("Failed to connect to FTP Server! (Hit any key to exit)");
                        //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-retrieveFileList()",
                        //    "Failed to connect to FTP server (IP Address: " + sDetails.ServerAddress + " Remote folder: " + sDetails.ServerFolder + ")!");

                        this.errorMessage = "MessageTransaction-retrieveFileList()|" + "Failed to connect to FTP server (IP Address: " + sDetails.ServerAddress + " Remote folder: " + sDetails.ServerFolder + ")!";
                    }

                    break;
                    #endregion

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    #region SFTP
                    IAPL.Transport.Operation.SFTP sftp = new IAPL.Transport.Operation.SFTP(sDetails.ServerAddress,
                        sDetails.ServerUserName, sDetails.ServerPassword, sDetails.ServerPort);

                    if (sftp.Connect())
                    {
                        string checkCurrentRemoteFolder = sftp.GetCurrentDirectory();
                        sDetails.ServerFolder = sDetails.GetFTPFolder(sDetails.ServerFolder);
                        sftp.ChangeDirectory(sDetails.ServerFolder);
                        checkCurrentRemoteFolder = sftp.GetCurrentDirectory();
                        if (sDetails.ServerFolder.Trim().ToLower().Replace(@"/", "").Equals(checkCurrentRemoteFolder.Trim().Replace(@"/", "").ToLower()))
                        {
                            if (useFITE)
                            {
                                // Get list of Terminators
                                retTable = sftp.GetFileList(mDetails.FITEFileMask);
                            }
                            else
                            {
                                // **********************************************************
                                // Project: ISG12043
                                // Developer: Alrazen Estrella
                                // Date: Oct. 6, 2008

                                // Check if IMS
                                if (mDetails.IndividualProcess.Equals(1))
                                {
                                    // IMS 
                                    System.Collections.Hashtable FileList = new System.Collections.Hashtable();
                                    FileList = sftp.GetFileList("*.*");
                                    IAPL.Transport.IMS.Process IMSProcess = new IAPL.Transport.IMS.Process();
                                    ListOfZipFiles = new System.Collections.Hashtable();
                                    retTable = IMSProcess.GetIMSFiles(FileList, out ListOfZipFiles, out IMSIncompleteFiles, out IMSCompleteCountriesName, out IMSIncompleteCountriesName);
                                }
                                else
                                {
                                    // Non-IMS 

                                    // Get list of non-terminator files
                                    retTable = sftp.GetFileList(mDetails.SourceFileMask);
                                }

                                // **********************************************************
                            }
                        }
                        else
                        {
                            //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-retrieveFileList()",
                            //    "(SFTP IP Address " + sDetails.ServerAddress + ") Remote folder \'" + sDetails.ServerFolder + "\' is not existing!");

                            this.errorMessage = "MessageTransaction-retrieveFileList()|" + "(SFTP IP Address " + sDetails.ServerAddress + ") Remote folder \'" + sDetails.ServerFolder + "\' is not existing!";
                        }
                        //int i = 0;
                        sftp.Disconnect();
                    }
                    else
                    {
                        //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-retrieveFileList()",
                        //    "Failed to connect to SFTP server (IP Address: " + sDetails.ServerAddress + " Remote folder: " + sDetails.ServerFolder + ")!");

                        //this.errorMessage = "MessageTransaction-retrieveFileList()|" + "Failed to connect to SFTP server (IP Address: " + sDetails.ServerAddress + " Remote folder: " + sDetails.ServerFolder + ")!";
                        this.errorMessage = sftp.ErrorMessage + " (SFTP: " + sDetails.ServerAddress + " Remote Folder: " + sDetails.ServerFolder + ")";
                    }

                    break;
                    #endregion

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                    break;

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                    #region Network
                    IAPL.Transport.Operation.Network networkProtocol = new IAPL.Transport.Operation.Network();

                    if (sDetails.ServerAddress.Trim().Length > 0)
                    {
                        // ****************************************************
                        // Developer: Alrazen Estrella 
                        // Project: ISG12152 //SR33621
                        // Date: July 17, 2008 

                        string strFileMask;
                        if (useFITE)
                        { strFileMask = mDetails.FITEFileMask; }
                        else
                        { strFileMask = mDetails.SourceFileMask; }

                        // Old Code
                        //retTable = networkProtocol.GetFileList(sDetails.ServerAddress, mDetails.SourceFileMask);

                        // New Code
                        retTable = networkProtocol.GetFileList(sDetails.ServerAddress, strFileMask);


                        // ****************************************************

                        if (retTable.Count < 1)
                        {
                            if (networkProtocol.ErrorMessage.Trim().Length > 0)
                            {
                                //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-retrieveFileList()",
                                //"Failed to connect to Network folder: \'" + sDetails.ServerAddress + "\'! " + networkProtocol.ErrorMessage);

                                networkProtocol.ErrorMessage = networkProtocol.ErrorMessage.Replace("\r", "");
                                networkProtocol.ErrorMessage = networkProtocol.ErrorMessage.Replace("\n", "");

                                this.errorMessage = networkProtocol.ErrorMessage + " (Network folder: \'" + sDetails.ServerAddress + "\')";
                            }
                        }

                    }
                    else
                    {
                        //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-retrieveFileList()",
                        //    "Failed to connect to Network folder " + sDetails.ServerAddress + "!");
                        this.errorMessage = "MessageTransaction-retrieveFileList()|Blank path for the network folder! Check networksetting table.";
                    }

                    break;
                    #endregion

                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NONE:
                    break;
            }

            return retTable;
        }
        #endregion

        // -------------------------------------------------------------------------------------

        #region get server info
        private IAPL.Transport.Transactions.ServerDetails getServerInfo(System.Data.DataRow row, IAPL.Transport.Util.ConstantVariables.FileDirection fileDirecton)
        {
            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();
            string msgFileID = "";

            this.errorMessage = "";

            if (fileDirecton == IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE)
            {
                msgFileID = "MessageFileSourceID";
            }
            else
            { // SEND filedirection
                msgFileID = "MessageFileDestinationID";
            }

            msgFileID = IAPL.Transport.Util.CommonTools.ValueToString((Object)row[msgFileID]);
            if (msgFileID.Trim().Length < 1)
            { msgFileID = "0"; }
            System.Collections.Hashtable desTable = new System.Collections.Hashtable();
            desTable.Add("@ftsID", msgFileID);
            db.GetServerDetails(desTable);
            desTable = db.HTableInfo;

            IAPL.Transport.Transactions.ServerDetails serverTransactionDetails = null;

            if (desTable == null)
            {

                serverTransactionDetails = new ServerDetails();

                serverTransactionDetails.ErrorMessage = "MessageTransaction-getServerInfo()|No server information";

                this.errorMessage = serverTransactionDetails.ErrorMessage;

                return serverTransactionDetails;
            }
            //GERARD 1
            try
            {
                bool thisIsUseTempExtension = false;
                if (IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["IsUseTempExtension"]).ToUpper() == "TRUE")
                    thisIsUseTempExtension = true;

                string thisTempExtension = string.Empty;

                if (thisIsUseTempExtension)
                    thisTempExtension = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["MsgTempExtension"]).TrimStart().TrimEnd().ToUpper();

                serverTransactionDetails = new ServerDetails(fileDirecton,
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetBackUpFolder"]),
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetSourceFileMask"]),
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["MsgFileNameConvention"]),
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["MsgCounter"]),
                    IAPL.Transport.Util.ConstantVariables.FileAction.NONE,
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["TransmissionTypeCode"]),
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["MessageCode"]),
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["PortNumber"]),
                    thisTempExtension
                    );

                serverTransactionDetails.FileNamingExtension = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["MsgFileNameExtension"]);
                serverTransactionDetails.FileNameDateFormat = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["MsgFileNameDateFormat"]);
                serverTransactionDetails.ErrorMessage = "";
                serverTransactionDetails.FileTempExtension = thisTempExtension;
                serverTransactionDetails.IsUseFileTempExtension = thisIsUseTempExtension;
            }
            catch (Exception ex)
            {
                serverTransactionDetails = new ServerDetails();
                serverTransactionDetails.ErrorMessage = "MessageTransaction-getServerInfo()|" + ex.Message.ToString();
                //System.Console.WriteLine(serverTransactionDetails.ErrorMessage);
                //continue;
                IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getServerInfo()", serverTransactionDetails.ErrorMessage);
                return serverTransactionDetails;
            }

            #region OLD CODE SR#33117 Ccenriquez / Capad -- November 6, 2009
            //System.Collections.Hashtable srcTable = new System.Collections.Hashtable();
            //srcTable.Add("@ftsID", fileTransSettingsID);

            //// with complete server information
            //if (db.GetFTPDetails(srcTable, serverTransactionDetails.TransmissionType))
            //{
            //    desTable = db.HTableInfo;

            //    serverTransactionDetails.ErrorMessage = "";

            //    serverTransactionDetails.ServerUserName = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["UserName"]);

            //    serverTransactionDetails.ServerPassword = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["Password"]);
            //    //dencrypt the password
            //    if (serverTransactionDetails.ServerPassword.Trim().Length > 0)
            //    {
            //        serverTransactionDetails.ServerPassword = IAPL.Transport.Util.Utility.Decrypt(serverTransactionDetails.ServerPassword);
            //    }

            //    if (serverTransactionDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL)
            //    {
            //        serverTransactionDetails.EmailAddressFrom = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailAddressFrom"]);
            //        serverTransactionDetails.EmailAddressTo = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailAddressTo"]);
            //        serverTransactionDetails.EmailAddressCC = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailAddressCC"]);
            //        serverTransactionDetails.EmailSubject = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailSubject"]);
            //        serverTransactionDetails.EmailBody = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailBody"]);
            //    }
            //    else if (serverTransactionDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK)
            //    {
            //        serverTransactionDetails.ServerAddress = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["Path"]);
            //        if (serverTransactionDetails.ServerAddress.Trim().Length < 1)
            //        {
            //            serverTransactionDetails.ErrorMessage = "Destination Server's path is blank! Review the NetworkSetting table. ";
            //            //System.Console.WriteLine(serverTransactionDetails.ErrorMessage);
            //            //continue;
            //            IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getServerInfo()", serverTransactionDetails.ErrorMessage);
            //            return serverTransactionDetails;
            //        }
            //    }
            //    else if (serverTransactionDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP)
            //    {
            //        serverTransactionDetails.ServerAddress = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["URL"]);
            //        if (serverTransactionDetails.ServerAddress.Trim().Length < 1)
            //        {
            //            serverTransactionDetails.ErrorMessage = "Destination Server URL is blank! Review the database settings. ";
            //            //System.Console.WriteLine(serverTransactionDetails.ErrorMessage);
            //            //continue;
            //            IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getServerInfo()", serverTransactionDetails.ErrorMessage);
            //            return serverTransactionDetails;
            //        }
            //    }
            //    else
            //    {
            //        serverTransactionDetails.ServerAddress = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["IPAddress"]);
            //        if (serverTransactionDetails.ServerAddress.Trim().Length < 1)
            //        {
            //            serverTransactionDetails.ErrorMessage = "Destination Server IP Address is blank! Review the database settings. ";
            //            //System.Console.WriteLine(serverTransactionDetails.ErrorMessage);
            //            //continue;
            //            IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getServerInfo()", serverTransactionDetails.ErrorMessage);
            //            return serverTransactionDetails;
            //        }

            //        serverTransactionDetails.ServerFolder = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["Folder"]);
            //    }

            //    if (db != null)
            //        db.Dispose();
            //    if (desTable != null)
            //    {
            //        desTable.Clear();
            //        desTable = null;
            //    }
            //    if (srcTable != null)
            //    {
            //        srcTable.Clear();
            //        srcTable = null;
            //    }

            //}
            //else
            //{
            //    serverTransactionDetails.ErrorMessage = db.ErrorMessage;
            //}        
            #endregion

            //NEW CODE SR#33117 Ccenriquez / Capad -- November 6, 2009
            GetTransmissionDetails(desTable["FileTransferSettingID"].ToString(), ref serverTransactionDetails, ref desTable, ref db);

            return serverTransactionDetails;
        }
        #endregion

        //NEW CODE SR#33117 Ccenriquez / Capad -- November 6, 2009
        private void GetTransmissionDetails(string fileTransSettingsID, ref ServerDetails serverTransactionDetails, ref Hashtable desTable, ref DbTransaction db)
        {
            System.Collections.Hashtable srcTable = new System.Collections.Hashtable();
            srcTable.Add("@ftsID", fileTransSettingsID);

            // with complete server information
            if (db.GetFTPDetails(srcTable, serverTransactionDetails.TransmissionType))
            {
                desTable = db.HTableInfo;

                serverTransactionDetails.ErrorMessage = "";

                serverTransactionDetails.ServerUserName = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["UserName"]);

                serverTransactionDetails.ServerPassword = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["Password"]);
                //dencrypt the password
                if (serverTransactionDetails.ServerPassword.Trim().Length > 0)
                {
                    serverTransactionDetails.ServerPassword = IAPL.Transport.Util.Utility.Decrypt(serverTransactionDetails.ServerPassword);
                }

                if (serverTransactionDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL)
                {
                    serverTransactionDetails.EmailAddressFrom = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailAddressFrom"]);
                    serverTransactionDetails.EmailAddressTo = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailAddressTo"]);
                    serverTransactionDetails.EmailAddressCC = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailAddressCC"]);
                    serverTransactionDetails.EmailSubject = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailSubject"]);
                    serverTransactionDetails.EmailBody = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["EmailBody"]);
                }
                else if (serverTransactionDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK)
                {
                    serverTransactionDetails.ServerAddress = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["Path"]);
                    if (serverTransactionDetails.ServerAddress.Trim().Length < 1)
                    {
                        serverTransactionDetails.ErrorMessage = "Destination Server's path is blank! Review the NetworkSetting table. ";
                        //System.Console.WriteLine(serverTransactionDetails.ErrorMessage);
                        //continue;
                        IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getServerInfo()", serverTransactionDetails.ErrorMessage);
                        return;
                    }
                }
                else if (serverTransactionDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP)
                {
                    serverTransactionDetails.ServerAddress = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["URL"]);
                    if (serverTransactionDetails.ServerAddress.Trim().Length < 1)
                    {
                        serverTransactionDetails.ErrorMessage = "Destination Server URL is blank! Review the database settings. ";
                        //System.Console.WriteLine(serverTransactionDetails.ErrorMessage);
                        //continue;
                        IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getServerInfo()", serverTransactionDetails.ErrorMessage);
                        return;
                    }
                }
                else
                {
                    serverTransactionDetails.ServerAddress = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["IPAddress"]);
                    if (serverTransactionDetails.ServerAddress.Trim().Length < 1)
                    {
                        serverTransactionDetails.ErrorMessage = "Destination Server IP Address is blank! Review the database settings. ";
                        //System.Console.WriteLine(serverTransactionDetails.ErrorMessage);
                        //continue;
                        IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getServerInfo()", serverTransactionDetails.ErrorMessage);
                        return;
                    }

                    serverTransactionDetails.ServerFolder = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["Folder"]);
                    string thisStrPortNum = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["PortNumber"]);
                    int thisIntPortNum = 0;
                    int.TryParse(thisStrPortNum, out thisIntPortNum);
                    serverTransactionDetails.ServerPort = thisIntPortNum;

                    //bool thisIsUseTempExtension = false;
                    //bool.TryParse(IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["IsUseTempExtension"]).ToString(),
                    //    out thisIsUseTempExtension);

                    //string thisTempExtension = string.Empty;
                    //if (thisIsUseTempExtension)
                    //    thisTempExtension = IAPL.Transport.Util.CommonTools.ValueToString((Object)desTable["MsgTempExtension"]);
                    //serverTransactionDetails.FileTempExtension = thisTempExtension;
                }

                if (db != null)
                    db.Dispose();
                if (desTable != null)
                {
                    desTable.Clear();
                    desTable = null;
                }
                if (srcTable != null)
                {
                    srcTable.Clear();
                    srcTable = null;
                }

            }
            else
            {
                serverTransactionDetails.ErrorMessage = db.ErrorMessage;

            }
        }
        // -------------------------------------------------------------------------------------

        #region GetFileListItem
        public void GetFileListItem(string messageCode, string erp, string principal)
        {
            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            db.GetFileTransferInfo(messageCode, erp, principal);

            System.Data.DataTable dTable = db.CommonDataTable;

            if (dTable == null) return;

            foreach (DataRow row in dTable.Rows)
            {
                Process(row);
            }

            if (dTable != null)
                dTable.Dispose();
            if (db != null)
                db.Dispose();
        }
        #endregion
        #region getFileList
        private void getFileList()
        {
            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            #region // SR#33041 Ccenriquez / Capad -- October 19,2009 Start
            string MsgCodeExcluded = string.Empty;

            //lock (IAPL.Transport.Util.GlobalVariables.htMessageCodes)
            lock (thislock)
            {
                foreach (string msgCode in IAPL.Transport.Util.GlobalVariables.htMessageCodes.Keys)
                    MsgCodeExcluded += (MsgCodeExcluded == string.Empty) ? msgCode : "," + msgCode;
            }

            db.GetFileTransferInfo("GSK001", "MKT", "16");
            #endregion // SR#33041 Ccenriquez / Capad -- October 19,2009 End

            //db.GetFileTransferInfo(); -- removed

            if (db.ErrorMessage.Trim().Length > 0)
            {
                IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getFileList()", "Database error: " + db.ErrorMessage);
                return;
            }

            #region retrieve info from messagesettings

            System.Data.DataTable dTable = db.CommonDataTable;

            if (dTable == null) return;

            foreach (DataRow row in dTable.Rows)
            {
                Process(row);
            }


            //LENIN - ISG11597 - ADD - 11292007
            //Purging portion
            foreach (System.Data.DataRow row in dTable.Rows)
            {
                IAPL.Transport.Util.Utility.PurgeFiles(row["MsetBackUpFolder"].ToString(), row["MsetRetention"].ToString());
            }

            if (dTable != null)
                dTable.Dispose();
            if (db != null)
                db.Dispose();

            #endregion

        }

        private void Process(DataRow row)
        {
            #region Process
            #region retrieve info from source server
            this.errorMessage = "";

            //source server
            IAPL.Transport.Transactions.ServerDetails sourceServerDetails = this.getServerInfo(row, IAPL.Transport.Util.ConstantVariables.FileDirection.RECEIVE);
            List<Task> fileTasks = new List<Task>();
            //limit dev testing to internal servers only.
            //check 'environment' app config key value.
            //if value is DEV, check if source server is not internal.
            //assume internal = server starts with PHISG.
            //if not DEV and internal, skip processing
            if ((IAPL.Transport.Configuration.Config.GetAppSettingsValue("environment", "").Trim()) == "DEV")
            {
                string thisSourceServerAddress = sourceServerDetails.ServerAddress.Trim().ToUpper().ToString() + "     ";
                if (thisSourceServerAddress.Substring(0, 5) != "PHISG")
                {
                    //IAPL.Transport.Util.TextLogger.Log("DEV mode -- **SKIPPING** ", "MsgCode " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]));
                    //continue;
                    return;
                }
            }
            else
            {
                // MDO 20160222
                //IAPL.Transport.Util.TextLogger.Log("Processing", "MsgCode " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]));
                IAPL.Transport.Util.TextLogger.Log("Processing", string.Format("Server: {0} -> MsgCode: {1}", IAPL.Transport.Configuration.Config.GetAppSettingsValue("ServerID", ""), IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"])));

            }

            IAPL.Transport.Transactions.MessageDetails messageDetails = new MessageDetails();

            if (sourceServerDetails.ErrorMessage.Trim().Length > 0)
            {
                IAPL.Transport.Util.TextLogger.Log("tab", sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.LogError("MsgCode: " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]), sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.Log("tab", "Sending error email message...");
                this.sendEmailNotification(messageDetails, sourceServerDetails, false);
                //continue;
                return;
            }

            try
            {
                messageDetails = new MessageDetails(
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetSourceFileMask"]),
                    IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetBackUpFolder"]));

                messageDetails.ERP                        = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["ERP"]);
                messageDetails.Principal                  = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["PRNCPL"]);
                messageDetails.TradingCode                = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["trdpCode"]);
                messageDetails.TradingName                = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["trdpName"]);
                messageDetails.MessageCode                = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]);
                messageDetails.ApplicationCode            = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["apluCode"]);
                messageDetails.SupplierID                 = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["SUPPID"]);
                messageDetails.SupplierName               = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["SUPPNAME"]);
                messageDetails.SetSendSuccessNotification = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetSendSuccessNotification"]);
                messageDetails.StartDate                  = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
                #region < Set messageDetails property for File Terminator and Zipping Functionality >
                //LENIN
                messageDetails.FITEFileMask            = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetFITEMask"]);
                messageDetails.SetZippingFunctionality = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetIsZip"]);
                messageDetails.ZIPPassword             = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetZipPassword"]);
                messageDetails.Retention               = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetRetention"]);
                #endregion

                // **********************************************************
                // Developer: Alrazen Estrella
                // Date: July 17, 2008
                // Project: ISG12152

                messageDetails.SetZipSource       = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetIsZipSource"]);
                messageDetails.SetFilesSentSingle = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetFilesSentSingle"]);
                messageDetails.SetFilesSentBatch  = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetFilesSentBatch"]);

                // **********************************************************

                // **********************************************************
                // Developer: Alrazen Estrella
                // Date: September 4, 2008
                // Project: ISG12128

                messageDetails.SetFileConvertionFlag = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetFileConvertionFlag"]);
                messageDetails.SourceCodePage        = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetSourceCodePage"]));
                messageDetails.DestinationCodePage   = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetDestinationCodePage"]));

                // **********************************************************

                // **********************************************************
                // Developer: Alrazen Estrella
                // Date: September 25, 2008
                // Project: ISG12043

                messageDetails.MsetFilePickupDelay = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetFilePickupDelay"]));
                messageDetails.IndividualProcess   = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["IndividualProcess"]));
                messageDetails.MsgManualRunFlag    = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgManualRunFlag"]));
                messageDetails.MsgStartTime        = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgStartTime"]));
                messageDetails.MsgEndTime          = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgEndTime"]));
                messageDetails.MsetBatchRun        = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetBatchRun"]));
                messageDetails.MsetBatchTime       = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetBatchTime"]));
                messageDetails.MsetRuntime         = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetRuntime"]));
                messageDetails.MsetStartTime       = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetStartTime"]));
                messageDetails.MsetEndTime         = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetEndTime"]));
                messageDetails.MsetInterval        = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetInterval"]));
                messageDetails.IMSBatchRun         = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetIMSBatchRun"]));
                messageDetails.IMSFolder           = "";
                messageDetails.CrashStatus         = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["CrashStatus"]));
                messageDetails.MsgMonday           = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgMonday"]));
                messageDetails.MsgTuesday          = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgTuesday"]));
                messageDetails.MsgWednesday        = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgWednesday"]));
                messageDetails.MsgThursday         = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgThursday"]));
                messageDetails.MsgFriday           = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgFriday"]));
                messageDetails.MsgSaturday         = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgSaturday"]));
                messageDetails.MsgSunday           = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgSunday"]));

                try
                {
                    messageDetails.IMSProcessId = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["ProcessId"]);
                }
                catch
                { }

                //SR#34273 Ccenriquez -- December 4, 2009
                messageDetails.MsetMaxThreadCount = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetMaxThreadCount"]));

                // **********************************************************
            }
            catch (Exception ex)
            {
                sourceServerDetails.ErrorMessage = "MessageTransaction-getFileList()|" + ex.Message.ToString();

                IAPL.Transport.Util.TextLogger.Log("tab", sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.LogError("MsgCode: " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]), sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.Log("tab", "Sending error email message...");
                this.sendEmailNotification(messageDetails, sourceServerDetails, false);
                //continue;
                return;
            }

            if (messageDetails.SourceFileMask.Trim().Length < 1)
            {
                //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getFileList()", messageDetails.MessageCode + ": MsetSourceFileMask is blank! Please check your database settings. ");
                sourceServerDetails.ErrorMessage = "MessageTransaction-getFileList()|MsetSourceFileMask is blank! Please check your database settings.";

                IAPL.Transport.Util.TextLogger.Log("tab", sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.LogError("MsgCode: " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]), sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.Log("tab", "Sending error email message...");
                this.sendEmailNotification(messageDetails, sourceServerDetails, false);
                //continue;
                return;
            }

            if (messageDetails.BackupFolder.Trim().Length < 1)
            {
                //IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-getFileList()", messageDetails.MessageCode + ": Backupfolder is blank! Please check your database settings. ");
                sourceServerDetails.ErrorMessage = "MessageTransaction-getFileList()|MsetSourceFileMask is blank! Backupfolder is blank! Please check your database settings.";

                IAPL.Transport.Util.TextLogger.Log("tab", sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.LogError("MsgCode: " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]), sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.Log("tab", "Sending error email message...");
                this.sendEmailNotification(messageDetails, sourceServerDetails, false);
                //continue;
                return;
            }

            #endregion


            #region < Temporary >
            //TODO: temporary
            System.Console.WriteLine("MsetFITEMask - " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetFITEMask"]));
            System.Console.WriteLine("MsetIsZip - " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetIsZip"]));
            System.Console.WriteLine("MsetZipPassword - " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetZipPassword"]));
            System.Console.WriteLine("MsetRetention - " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsetRetention"]));
            #endregion


            #region retrieve the files from the source server

            IAPL.Transport.Util.TextLogger.Log("tab", " Connecting to source server (" + sourceServerDetails.GetSourceFolder("") + ") ...");

            int countAttempt = 0;
            errorMessage = "first try";

            System.Collections.Hashtable messageTable = new System.Collections.Hashtable();

            // ****************************************************
            // Developer: Alrazen Estrella
            // Project: ISG12152
            // Date: July 17, 2008

            System.Collections.Hashtable messageTableFite = new System.Collections.Hashtable();
            System.Collections.Hashtable messageTableFiles = new System.Collections.Hashtable();

            // ****************************************************

            //LENIN - ISG11597 - ADD - 11-27-2007
            string FITEFileName = "";

            while (errorMessage.Trim().Length > 0 && countAttempt < this.maxAttemptOnFailed)
            {

                // NEW CODE
                // ****************************************************
                // Developer: Alrazen Estrella
                // Project: ISG12152
                // Date: July 17, 2008

                // Set the Process code
                if (messageDetails.IsZIP == true)
                {
                    // No Password
                    if (messageDetails.ZIPPassword.Equals(""))
                    {
                        if (messageDetails.IsZIPSource.Equals(true))
                        { messageDetails.ProcessCode = 1; }
                        else
                        { messageDetails.ProcessCode = 4; }
                    }
                    else  // With Password
                    {
                        if (messageDetails.IsZIPSource.Equals(true))
                        { messageDetails.ProcessCode = 2; }
                        else
                        { messageDetails.ProcessCode = 6; }
                    }
                }

                else
                {
                    if (messageDetails.IsZIPSource.Equals(true))
                    { messageDetails.ProcessCode = 3; }
                    else
                    { messageDetails.ProcessCode = 5; }
                }

                //Gerard


                // ****************************************************

                #region old
                ////LENIN - ISG11597 - EDIT - 11-25-2007
                //if (messageDetails.FITEFileMask.ToString().Trim() != "")
                //{

                //    // ****************************************************
                //    // Developer: Alrazen Estrella
                //    // Project: ISG12152
                //    // Date: July 17, 2008

                //    #region OLD CODE
                //    //messageTable = this.retrieveFileList(sourceServerDetails, messageDetails, true);
                //    //if (messageTable.Count > 0)
                //    //{
                //    //    FITEFileName = messageTable["file1"].ToString();
                //    //    messageTable = this.retrieveFileList(sourceServerDetails, messageDetails, false);

                //    //    foreach (DictionaryEntry file in messageTable)
                //    //    {
                //    //        if (string.Compare(file.Value.ToString(), FITEFileName) == 0)
                //    //        {
                //    //            messageTable.Remove(file.Key);
                //    //            break;
                //    //        }
                //    //    }
                //    //}
                //    #endregion


                //    // NEW CODE
                //    // Check if there are existing FileMask
                //    System.Collections.Hashtable messageTableFite = new System.Collections.Hashtable();
                //    System.Collections.Hashtable messageTableFiles = new System.Collections.Hashtable();

                //    messageTableFite = this.retrieveFileList(sourceServerDetails, messageDetails, true);
                //    if (messageTableFite.Count > 0)
                //    {
                //        FITEFileName = messageTableFite["file1"].ToString();

                //        // List all Files without Filemask
                //        messageTableFiles = this.retrieveFileList(sourceServerDetails, messageDetails, false);

                //        // Get Filemask Extension
                //        string FiteMaskExt = messageDetails.FITEFileMask.Substring(messageDetails.FITEFileMask.LastIndexOf(".") + 1);

                //        string FilesToCheck = "";
                //        string FiteFileToVerify = "";
                //        int FileCtr = 0;
                //        foreach (DictionaryEntry file in messageTableFiles)
                //        {
                //            FileCtr++;
                //            FilesToCheck = file.Value.ToString();
                //            foreach (DictionaryEntry fileFite in messageTableFite)
                //            {
                //                FiteFileToVerify = fileFite.Value.ToString();
                //                if (string.Compare(FilesToCheck.Substring(0, FilesToCheck.LastIndexOf(".")),
                //                                   FiteFileToVerify.Substring(0, FiteFileToVerify.LastIndexOf(".")), true).Equals(0))
                //                {
                //                    messageTable.Add("@File" + FileCtr, FilesToCheck);
                //                }
                //            }
                //        }
                //    }

                //    // ****************************************************

                //}
                //else
                //{
                //    messageTable = this.retrieveFileList(sourceServerDetails, messageDetails, false);
                //}
                #endregion

                //GEEK STEP 1
                if (messageDetails.FITEFileMask.ToString().Trim() != "")
                {
                    // Check if there are existing FileMask                        
                    messageTableFite = this.retrieveFileList(sourceServerDetails, messageDetails, true);
                    if (messageTableFite.Count > 0)
                    {
                        FITEFileName = messageTableFite["file1"].ToString();

                        // List all Files without Filemask
                        messageTableFiles = this.retrieveFileList(sourceServerDetails, messageDetails, false);

                        //NEW CODE SR#33117 Ccenriquez / Capad -- November 13, 2009
                        sourceServerDetails.TotalFiles = (messageDetails.FilesSentBatch && messageDetails.IsZIP) ? 1 : messageTableFiles.Count;

                        if (messageDetails.FilesSentSingle)
                        {
                            // Get files having the same terminator

                            // Get Filemask Extension
                            string FiteMaskExt = messageDetails.FITEFileMask.Substring(messageDetails.FITEFileMask.LastIndexOf(".") + 1);

                            string FilesToCheck = "";
                            string FiteFileToVerify = "";
                            int FileCtr = 0;

                            //OLD CODE SR#34273 Ccenriquez -- December 4, 2009
                            //foreach (DictionaryEntry file in messageTableFiles)
                            //{
                            //    FilesToCheck = file.Value.ToString();
                            //    foreach (DictionaryEntry fileFite in messageTableFite)
                            //    {
                            //        FiteFileToVerify = fileFite.Value.ToString();
                            //        if (string.Compare(FilesToCheck.Substring(0, FilesToCheck.LastIndexOf(".")),
                            //                           FiteFileToVerify.Substring(0, FiteFileToVerify.LastIndexOf(".")), true).Equals(0))
                            //        {
                            //            FileCtr++;
                            //            messageTable.Add("@File" + FileCtr, FilesToCheck);
                            //        }
                            //    }
                            //}

                            //NEW CODE SR#34273 Ccenriquez -- December 4, 2009
                            Hashtable messageTableFiteFinal = new Hashtable();

                            foreach (DictionaryEntry file in messageTableFiles)
                            {
                                FilesToCheck = file.Value.ToString();
                                foreach (DictionaryEntry fileFite in messageTableFite)
                                {
                                    FiteFileToVerify = fileFite.Value.ToString();
                                    if (string.Compare(FilesToCheck.Substring(0, FilesToCheck.LastIndexOf(".")),
                                                       FiteFileToVerify.Substring(0, FiteFileToVerify.LastIndexOf(".")), true).Equals(0))
                                    {
                                        if (messageDetails.MsetMaxThreadCount > 0 && messageTable.Count >= messageDetails.MsetMaxThreadCount)
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            FileCtr++;

                                            messageTable.Add("@File" + FileCtr, FilesToCheck);

                                            messageTableFiteFinal.Add(fileFite.Key, fileFite.Value);
                                        }
                                    }
                                }

                                if (messageDetails.MsetMaxThreadCount > 0 && messageTable.Count >= messageDetails.MsetMaxThreadCount)
                                    break;
                            }

                            messageTableFite = messageTableFiteFinal;
                        }
                        else
                        {
                            //NEW CODE SR#34273 Ccenriquez -- December 4, 2009
                            int FileCtr = 0;

                            if (messageDetails.MsetMaxThreadCount > 0 && messageTableFiles.Count >= messageDetails.MsetMaxThreadCount)
                            {
                                foreach (DictionaryEntry file in messageTableFiles)
                                {
                                    if (messageDetails.MsetMaxThreadCount > 0 && messageTable.Count >= messageDetails.MsetMaxThreadCount)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        FileCtr++;
                                        messageTable.Add("@File" + FileCtr, file.Value);
                                    }
                                }

                                messageTableFite.Clear(); //clear fite; for next batch will need the fite.
                            }
                            else
                            {
                                messageTable = messageTableFiles;
                            }

                            //OLD CODE SR#34273 Ccenriquez -- December 4, 2009
                            //messageTable = messageTableFiles;
                        }

                        if (messageDetails.ProcessCode.Equals(5) && !messageDetails.SourceFileMask.Equals("*.*"))
                            messageTable = IAPL.Transport.Util.Utility.CombineHashtables(messageTable, messageTableFite);
                    }

                    // ****************************************************

                }
                else
                {
                    //NEW CODE SR#34273 Ccenriquez -- December 4, 2009
                    int FileCtr = 0;


                    if (messageDetails.MsetMaxThreadCount > 0 && messageTableFiles.Count >= messageDetails.MsetMaxThreadCount)
                    {
                        foreach (DictionaryEntry file in messageTableFiles)
                        {
                            if (messageTable.Count >= messageDetails.MsetMaxThreadCount)
                            {
                                break;
                            }
                            else
                            {
                                FileCtr++;
                                messageTable.Add("@File" + FileCtr, file.Value);
                            }
                        }
                    }
                    else
                    {
                        messageTable = messageTableFiles;
                    }

                    //OLD CODE SR#34273 Ccenriquez -- December 4, 2009
                    //messageTable = this.retrieveFileList(sourceServerDetails, messageDetails, false);

                    //NEW CODE SR#33117 Ccenriquez / Capad -- November 13, 2009
                    sourceServerDetails.TotalFiles = (messageDetails.FilesSentBatch && messageDetails.IsZIP) ? 1 : messageTable.Count;
                }

                // If IMS and values read Crashed
                if (messageDetails.CrashStatus.Equals(true) &&
                    messageDetails.IndividualProcess.Equals(1))
                {
                    IAPL.Transport.Data.DbTransaction DBTrans = new IAPL.Transport.Data.DbTransaction();
                    ListOfZipFiles = new System.Collections.Hashtable();
                    IMSCompleteCountriesName = new System.Collections.Hashtable();

                    // MDO - Disable for dev debug
                    // messageTable = DBTrans.GetIMSFilesLeftProcessedBeforeCrashed(out ListOfZipFiles, out IMSCompleteCountriesName);
                }

                // If error encountered
                if (errorMessage.Trim().Length > 0)
                {
                    IAPL.Transport.Util.TextLogger.LogError("tab", "(MsgCode " + messageDetails.MessageCode +
                        " " + sourceServerDetails.GetSourceFolder("") + ") Error: " + errorMessage);

                    IAPL.Transport.Util.TextLogger.Log("tab", " Get file list from source server - failed! Attempt number " +
                        Convert.ToString(countAttempt + 1) + "...");

                    // cause to delay the transfer of file to destination server to avoid file locking
                    System.Threading.Thread.Sleep(this.transferDelay);
                }

                // Add attempts
                countAttempt++;
            }

            #endregion

            // If error encountered
            if (this.errorMessage.Trim().Length > 0)
            {
                sourceServerDetails.ErrorMessage = this.errorMessage;
                IAPL.Transport.Util.TextLogger.Log("tab", sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.LogError("MsgCode: " + IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]), sourceServerDetails.ErrorMessage);
                IAPL.Transport.Util.TextLogger.Log("tab", "Sending error email message...");

                this.sendEmailNotification(messageDetails, sourceServerDetails, false);
                //continue;
                return;
            }

            if (messageTable.Count > 0)
            {
                #region retrieve info from destination server

                //target server

                IAPL.Transport.Transactions.ServerDetails destinationServerDetails = getServerInfo(row, IAPL.Transport.Util.ConstantVariables.FileDirection.SEND);

                #endregion

                #region // SR#33041  oct19,2009 start
                //lock (IAPL.Transport.Util.GlobalVariables.htMessageCodes)
                lock (thislock)
                {
                    //Add messageCode to the htMessageCodes if not exists. htMessageCodes is the list of pending MessageCodes to be processed
                    if (!IAPL.Transport.Util.GlobalVariables.htMessageCodes.ContainsKey(messageDetails.MessageCode))
                        IAPL.Transport.Util.GlobalVariables.htMessageCodes.Add(messageDetails.MessageCode, (messageDetails.IsZIP || destinationServerDetails.TransmissionType == ConstantVariables.TransmissionMode.HTTP) ? 1 : messageTable.Count);
                    //else
                    //    continue;  //goto next messageCode if already exists in htMessageCodes
                }
                #endregion // SR#33041  oct19,2009 end

                // check backup folder
                if (messageDetails.BackupFolder.Trim().Length < 1)
                {
                    messageDetails.BackupFolder = IAPL.Transport.Util.CommonTools.GetCurrentDirectory();
                }

                // Log to file
                IAPL.Transport.Util.TextLogger.Log("tab", "From " +
                    sourceServerDetails.TransmissionType.ToString() + " to " +
                        destinationServerDetails.TransmissionType.ToString() +
                    " (" + messageTable.Count.ToString() + " file/s)");

                int i = 1;
                bool isMsgCounterIncremented = false;

                // **************************************************************3
                // Developer: Alrazen Estrella
                // Project: ISG12152
                // Date: July 22, 2008

                bool ExecuteOnce = false;

                // **************************************************************3


                //LENIN - ISG11597 - EDIT - 11-25-2007
                //                    if (FITEFileName == "")

                if (destinationServerDetails.TransmissionType == IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP)
                {
                    messageDetails.ProcessCode = 7;
                }

                if (messageDetails.ProcessCode.Equals(4) ||
                    messageDetails.ProcessCode.Equals(5) ||
                    messageDetails.ProcessCode.Equals(6))
                {
                    string threadName = messageDetails.MessageCode + "-" + Guid.NewGuid();

                    //GERARD DO SORT HERE
                    messageTable = (Hashtable)IAPL.Transport.Util.Utility.SortedHashTable(messageTable, "file");
                    
                    //actual
                    IAPL.Transport.Transactions.Worker workerThread = new Worker(threadName,
                                                                                 GetFileName(messageTable.Values.Cast<string>().ToList()),
                                                                                 messageDetails,
                                                                                 sourceServerDetails,
                                                                                 destinationServerDetails,
                                                                                 this.transferDelay,
                                                                                 this.maxAttemptOnFailed);
                    
                    workerThread.ProcessFiles(ExecuteOnce, this.transferDelay);    

                }
                else if (messageDetails.ProcessCode.Equals(1) ||
                         messageDetails.ProcessCode.Equals(2) ||
                         messageDetails.ProcessCode.Equals(3))
                {
                    // With File Masking

                    //LENIN - ISG11597 - ADD - 11-26-2007
                    string threadName = messageDetails.MessageCode + ": FITE (" + FITEFileName + ")";
                    //string threadName = messageDetails.MessageCode + "-" + Guid.NewGuid();

                    IAPL.Transport.Transactions.Worker workerThread = new Worker(threadName,
                                                                                 FITEFileName,
                                                                                 messageDetails,
                                                                                 sourceServerDetails,
                                                                                 destinationServerDetails,
                                                                                 this.transferDelay,
                                                                                 this.maxAttemptOnFailed);

                    workerThread.FileNames = messageTable;
                    workerThread.FITEFileName = FITEFileName;
                    workerThread.FiteFiles = messageTableFite;
                    workerThread.TotalFiles = messageTable.Count;

                    if (messageDetails.IndividualProcess.Equals(1))         // ISG12043 | Alrazen Estrella | Oct. 7, 2008
                    {
                        if (ListOfZipFiles != null)
                            workerThread.ListOfZipFiles = ListOfZipFiles;

                        if (IMSIncompleteFiles != null)
                            workerThread.ListOfIncompleteFiles = IMSIncompleteFiles;

                        if (IMSCompleteCountriesName != null)
                            workerThread.ListOfCompleteCountriesName = IMSCompleteCountriesName;

                        if (IMSIncompleteCountriesName != null)
                            workerThread.ListOfIncompleteCountriesName = IMSIncompleteCountriesName;
                    }

                    //new Thread(new ThreadStart(workerThread.ProcessFile)).Start();
                    fileTasks.Add(Task.Factory.StartNew(() => workerThread.ProcessFile()));

                    isMsgCounterIncremented = true;
                }

                //gerard
                else if (messageDetails.ProcessCode == 7)
                {

                    Data.DbTransaction _dbTransact = new IAPL.Transport.Data.DbTransaction();
                    int _Flag = _dbTransact.HTTPProcessCheckForEntries(messageDetails.MessageCode);

                    //Do not continue if there are REQUEUE entries.                       

                    if (_Flag == 0)
                    {
                        string threadName = messageDetails.MessageCode + ": FITE (" + FITEFileName + ")";
                        //string threadName = messageDetails.MessageCode + "-" + Guid.NewGuid();

                        IAPL.Transport.Transactions.Worker workerThread = new Worker(threadName, FITEFileName,
                                                                                     messageDetails,
                                                                                     sourceServerDetails,
                                                                                     destinationServerDetails,
                                                                                     this.transferDelay,
                                                                                     this.maxAttemptOnFailed);

                        workerThread.FileNames = messageTable;
                        workerThread.FITEFileName = FITEFileName;
                        workerThread.FiteFiles = messageTableFite;
                        workerThread.TotalFiles = messageTable.Count;

                        //new Thread(new ThreadStart(workerThread.ProcessFile)).Start();
                        fileTasks.Add(Task.Factory.StartNew(() => workerThread.ProcessFile()));

                    }
                    else
                    {
                        Console.WriteLine("REQUEUE FOUND - DOING NOTHING ON MAIN THREAD FOR MESSAGE PROCESS 7");
                    }
                    isMsgCounterIncremented = true;
                }


                #region save message counter
                //COMMENT CODE BELOW SR#33117 Ccenriquez / Capad -- November 16, 2009 
                //if (isMsgCounterIncremented) 
                //{
                //    if (!destinationServerDetails.FileNamingConvention.Equals(string.Empty))
                //    {
                //        IAPL.Transport.Util.Utility.UpdateFilenameConventionCounter(destinationServerDetails.MessageCode,
                //                                                                    destinationServerDetails.FileCounter.ToString());
                //    }

                //    //System.Collections.Hashtable fldList = new System.Collections.Hashtable();

                //    ////log to db the mesgCounter of source server
                //    ////fldList.Add("@MsgCode", sourceServerDetails.MessageCode);
                //    ////fldList.Add("@MsgCounter", sourceServerDetails.FileCounter.ToString());
                //    ////db.UpdateMessageCounter(fldList);

                //    ////log to db the mesgCounter of destination server
                //    //fldList = new System.Collections.Hashtable();
                //    //fldList.Add("@MsgCode", destinationServerDetails.MessageCode);
                //    //fldList.Add("@MsgCounter", destinationServerDetails.FileCounter.ToString());
                //    //db.UpdateMessageCounter(fldList);
                //}
                #endregion
            }

            else
            {
                // If IMS, Send report 1
                if (messageDetails.IndividualProcess.Equals(1))
                {
                    if (messageDetails.IMSBatchRun.Equals(true))
                    {

                        IAPL.Transport.IMS.Process.IMSSendEmailWhenNoFilesFound("IMS", messageDetails, true);
                    }
                }
                else
                {
                    //System.Console.WriteLine(messageDetails.MessageCode + ": No files to backup.");
                    IAPL.Transport.Util.TextLogger.Log("tab", messageDetails.MessageCode + " - No files to backup.");
                }
            }
            #endregion
        }

        private List<string> GetFileName(List<string> list)
        {
            List<string> lstFileName = new List<string>();

            foreach (var item in list)
            {
                lstFileName.Add(Path.GetFileName(item));
            }

            return lstFileName;
        }
        #endregion

        // -------------------------------------------------------------------------------------

        #region sendEmailNotification
        private bool sendEmailNotification(IAPL.Transport.Transactions.MessageDetails msgDetails, IAPL.Transport.Transactions.ServerDetails serverDetails, bool isSuccess)
        {
            bool success = true;
            string prlgIsSuccess = "0";

            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();
            System.Collections.Hashtable hTable2 = new System.Collections.Hashtable();

            if (!isSuccess)
            {
                msgDetails.TransDescription = "FILETRANSFER - failed.";
                prlgIsSuccess = "0";
            }
            else
            {
                msgDetails.TransDescription = "FILETRANSFER - successful.";
                prlgIsSuccess = "1";
            }

            hTable2.Add("@apluCode", msgDetails.ApplicationCode);
            hTable2.Add("@PRNCPL", msgDetails.Principal);
            hTable2.Add("@msgCode", msgDetails.MessageCode);
            hTable2.Add("@ERPID", msgDetails.ERP);
            hTable2.Add("@prlgCustID", "");
            hTable2.Add("@prlgProcessSource", "");

            msgDetails.EndDate = DateTime.Now.ToString();
            hTable2.Add("@prlgStartDate", msgDetails.StartDate);
            hTable2.Add("@prlgEndDate", msgDetails.EndDate); // this should appear on the update method
            hTable2.Add("@prlgIsSuccess", prlgIsSuccess); // this should appear on the update method
            hTable2.Add("@prlgDescription", msgDetails.TransDescription);
            hTable2.Add("@prlgTechnicalErrDesc", serverDetails.ErrorMessage);
            hTable2.Add("@prlgSourceParent", "");
            hTable2.Add("@prlgSourceParentCount", "0");
            hTable2.Add("@prlgSourceChild", "0");
            hTable2.Add("@prlgSourceChildCount", "0");
            db.LogProcessInfo(hTable2);

            msgDetails.ProcessLogID = db.GetProcessLogID;
            msgDetails.TechnicalErrorDescription = serverDetails.ErrorMessage;


            IAPL.Transport.Transactions.EmailTransaction emailTrans = new EmailTransaction(msgDetails);
            emailTrans.DestinationFolder = "";
            emailTrans.SourceFile = "";
            emailTrans.OutputFile = "";
            //IAPL.Transport.Transactions.EmailTransaction emailTrans = new EmailTransaction(msgDetails);
            emailTrans.SendEmailNotification("", "", false);

            return success;
        }
        #endregion

        // -------------------------------------------------------------------------------------

        #region checkworkschedule
        private bool workNow()
        {
            bool timeToWork = false;
            DateTime currentTime = DateTime.Now;
            DateTime startTimeFrame;
            DateTime endTimeFrame;

            try
            {
                startTimeFrame = Convert.ToDateTime(currentTime.ToString("MM/dd/yyyy") + " " + this.timeFrameStart);
                endTimeFrame = Convert.ToDateTime(currentTime.ToString("MM/dd/yyyy") + " " + this.timeFrameEnd);
                if ((currentTime >= startTimeFrame) && (currentTime <= endTimeFrame))
                {
                    timeToWork = true;
                }
            }
            catch (Exception ex)
            {
                IAPL.Transport.Util.TextLogger.LogError("MessageTransaction-workNow()", ex.Message.ToString());
            }

            return timeToWork;
        }
        #endregion

        // -------------------------------------------------------------------------------------

        #region get config settings
        private void checkApplicationSettings()
        {
            string appMode = IAPL.Transport.Configuration.Config.GetAppSettingsValue("showsettings", "no");

            IAPL.Transport.Util.TextLogger.IsSettingDisplay = false;
            if (appMode.ToLower().Trim().Equals("yes"))
            {
                IAPL.Transport.Util.TextLogger.IsSettingDisplay = true;
            }

            appMode = IAPL.Transport.Configuration.Config.GetAppSettingsValue("errorshowonconsole", "no");

            IAPL.Transport.Util.TextLogger.IsErrorShowOnConsole = false;
            if (appMode.ToLower().Trim().Equals("yes"))
            {
                IAPL.Transport.Util.TextLogger.IsErrorShowOnConsole = true;
            }

            // get transfer delay transferdelay
            appMode = IAPL.Transport.Configuration.Config.GetAppSettingsValue("transferdelay", "1500");

            try
            {
                this.transferDelay = Convert.ToInt16(appMode);
            }
            catch
            {
                this.transferDelay = 1500;
            }

            // get transfer delay transferdelay
            appMode = IAPL.Transport.Configuration.Config.GetAppSettingsValue("maxattemptonfailed", "5");

            try
            {
                this.maxAttemptOnFailed = Convert.ToInt16(appMode);
            }
            catch
            {
                this.maxAttemptOnFailed = 5;
            }

            appMode = IAPL.Transport.Configuration.Config.GetAppSettingsValue("appmode", "release");

            switch (appMode.Trim().ToLower())
            {
                case "debug":
                    IAPL.Transport.Util.TextLogger.IsDebug = true;
                    break;
                default:
                    IAPL.Transport.Util.TextLogger.IsDebug = false;
                    break;
            }

            this.timeFrameStart = IAPL.Transport.Configuration.Config.GetAppSettingsValue("timeframestart", "07:00 AM");
            this.timeFrameEnd = IAPL.Transport.Configuration.Config.GetAppSettingsValue("timeframeend", "07:00 PM");

        }

        public string TestAppSettings()
        {
            return IAPL.Transport.Configuration.Config.GetAppSettingsValue("processinterval", "None");
        }
        #endregion

        // -------------------------------------------------------------------------------------

        // APPLICATION START RUN
        #region start application
        public void StartApplication()
        {
            Int32 processInterval = 30000; //300000;
            try
            {
                //processInterval = Convert.ToInt32(IAPL.Transport.Configuration.Config.GetAppSettingsValue("processinterval", "300000"));
                processInterval = Convert.ToInt32(IAPL.Transport.Configuration.Config.GetAppSettingsValue("processinterval", "30000"));
            }
            catch
            //catch (Exception ex)
            //{
            //    IAPL.Transport.Util.TextLogger.Log("Application Error", ex.Message + " -2- " + ex.StackTrace);
            {
                processInterval = 300000;
            }
            //}

            pingServer = new PingServer();

            //try
            //{
            Console.WriteLine(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""));
            // MDO 20160222
            Console.WriteLine(string.Format("Server ID: {0}", IAPL.Transport.Configuration.Config.GetAppSettingsValue("ServerID", "")));
            while (!this.stopApplication)
            {
                var stopWatch = Stopwatch.StartNew();

                checkApplicationSettings();

                IAPL.Transport.Util.TextLogger.logRetention();
                pingServer.StartProcess();

                if (workNow()) // check if current time is w/in time frame
                {
                    //Gerard
                    //REQUEUE HERE
                    Console.WriteLine("Requeue: START");

                    // SR#33117 Ccenriquez / Capad -- November 5, 2009 -- BEGIN
                    #region Requeue Logic
                    NetTransaction theNetTransaction = new NetTransaction();

                    string destinationPath = string.Empty, errorMessage = string.Empty;
                    int countAttempt = 0;

                    RequeueFileDALC theRequeueFileDALC = new RequeueFileDALC();

                    //Get every IsActive RequeueFile
                    List<RequeueFile> lstRequeueFile = theRequeueFileDALC.GetEveryRequeueFile();

                    //loop each RequeueFile; 
                    foreach (RequeueFile requeueFile in lstRequeueFile)
                    {
                        ServerDetails theServerDetails = new ServerDetails(ConstantVariables.FileDirection.SEND,
                                                                requeueFile.MsetBackUpFolder,
                                                                string.Empty,
                                                                string.Empty,
                                                                string.Empty,
                                                                ConstantVariables.FileAction.NONE,
                                                                requeueFile.TransmissionTypeCode,
                                                                requeueFile.MsgCode,
                                                                string.Empty,
                                                                requeueFile.TempExtension);

                        string requeueFolder = requeueFile.MsetBackUpFolder.Substring(0, requeueFile.MsetBackUpFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + IAPL.Transport.Configuration.Config.GetAppSettingsValue("RequeueFolder", "requeue");
                        string requeuePath = theServerDetails.GetNetworkSourceFile(requeueFolder, requeueFile.OutputFileName);

                        DbTransaction db = new DbTransaction();
                        Hashtable desTable = new Hashtable();
                        desTable.Add("@ftsID", requeueFile.MessageFileDestinationId);
                        db.GetServerDetails(desTable);
                        desTable = db.HTableInfo;

                        GetTransmissionDetails(desTable["FileTransferSettingID"].ToString(), ref theServerDetails, ref desTable, ref db);

                        //Initialize all variables need for sending to destination
                        FTP ftp = null;
                        SFTP sftp = null;

                        //Email
                        EmailTransaction emailTrans = new EmailTransaction();
                        DataTable dTable = null;
                        string emailSMTP = string.Empty;

                        switch (theServerDetails.TransmissionType)
                        {
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                #region FTP
                                ftp = new FTP(theServerDetails.ServerAddress, theServerDetails.ServerUserName, theServerDetails.ServerPassword, theServerDetails.ServerPort);
                                break;
                                #endregion
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                #region SFTP
                                sftp = new SFTP(theServerDetails.ServerAddress, theServerDetails.ServerUserName, theServerDetails.ServerPassword, theServerDetails.ServerPort);
                                destinationPath = theServerDetails.ServerFolder + @"/" + requeueFile.OutputFileName;
                                break;
                                #endregion
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                #region NETWORK
                                destinationPath = theServerDetails.GetNetworkSourceFile(theServerDetails.ServerAddress, requeueFile.OutputFileName);
                                break;
                                #endregion
                            case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                                #region Email
                                db.emailDistributionList(false, requeueFile.MsgCode, requeueFile.ERP, 0);

                                dTable = db.CommonDataTable;
                                emailSMTP = Config.GetAppSettingsValue("emailnotificationsmtp", "");
                                break;
                                #endregion
                        }

                        bool isSuccess = false;
                        string emailTo = string.Empty;

                        //begin sending until maxFailedAttempt; 
                        int RequeueMaxResendAttempt = Convert.ToInt32(Config.GetAppSettingsValue("RequeueMaxResendAttempt", "5"));

                        while (!isSuccess && countAttempt < RequeueMaxResendAttempt)
                        {
                            switch (theServerDetails.TransmissionType)
                            {
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                                    #region FTP
                                    if (ftp != null)
                                    {
                                        if (ftp.Connect())
                                        {
                                            destinationPath = theServerDetails.ServerFolder + @"/" + requeueFile.OutputFileName;

                                            isSuccess = ftp.Upload(requeuePath, destinationPath);

                                            if (isSuccess)
                                                if (requeueFile.TempExtension.Length > 0)
                                                {
                                                    string oldName = destinationPath;
                                                    string newName;
                                                    if (oldName.Substring(oldName.Length -
                                                        requeueFile.TempExtension.Length, requeueFile.TempExtension.Length)
                                                        == requeueFile.TempExtension)
                                                    {
                                                        newName = oldName.Substring(0,
                                                            oldName.Length - requeueFile.TempExtension.Length);
                                                        isSuccess = ftp.Rename(oldName, newName);
                                                    }
                                                }

                                            ftp.Disconnect();
                                        }
                                    }
                                    break;
                                    #endregion
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                                    #region SFTP
                                    if (sftp != null)
                                    {
                                        if (sftp.Connect())
                                        {
                                            isSuccess = sftp.Upload(requeuePath, destinationPath);
                                            if (isSuccess)
                                                if (requeueFile.TempExtension.Length > 0)
                                                {
                                                    string oldName = destinationPath;
                                                    string newName;
                                                    if (oldName.Substring(oldName.Length -
                                                        requeueFile.TempExtension.Length, requeueFile.TempExtension.Length)
                                                        == requeueFile.TempExtension)
                                                    {
                                                        newName = oldName.Substring(0,
                                                            oldName.Length - requeueFile.TempExtension.Length);
                                                        isSuccess = sftp.Rename(oldName, newName);
                                                    }
                                                }
                                            sftp.Disconnect();
                                        }
                                    }
                                    break;
                                    #endregion
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                                    #region Network
                                    isSuccess = theNetTransaction.CopyFileFrom(requeuePath, destinationPath);
                                    break;
                                    #endregion
                                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                                    #region Email
                                    if (dTable != null)
                                    {
                                        foreach (System.Data.DataRow row in dTable.Rows)
                                        {
                                            //IAPL.Transport.Configuration.Config.GetAppSettingsValue("emailnotificationsender", "")

                                            emailTo = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrTO"]);

                                            IAPL.Transport.Operation.Email email = new IAPL.Transport.Operation.Email(
                                                IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrFROM"]),
                                                emailTo,
                                                IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrCC"]),
                                                IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldIntEmailAddrBCC"]),
                                                IAPL.Transport.Util.CommonTools.ValueToString((Object)row["emldEmailSubject"]),
                                                2,
                                                emailSMTP);

                                            string mesgBody = emailTrans.readNoticationMessageBody(requeuePath, true, CommonTools.ValueToString((Object)row["emldXSLTPath"]));

                                            ArrayList list = new ArrayList();
                                            list.Add(requeuePath);

                                            isSuccess = email.Send(false, mesgBody, list);
                                        }
                                    }

                                    break;
                                    #endregion
                            }

                            if (!isSuccess)
                            {
                                TextLogger.Log(TextLogger.messageType.Bulleted, "",
                                    "The requeue file ['" + requeueFile.OutputFileName + "'] file failed on transmisson from " + requeueFolder + " to " + theServerDetails.ServerAddress + " server failed! Attempt number " + Convert.ToString(countAttempt + 1) + "...");

                                int RequeueResendDelay = Convert.ToInt32(Config.GetAppSettingsValue("RequeueResendDelay", "1500"));

                                System.Threading.Thread.Sleep(RequeueResendDelay); //delay sending file to destination
                            }

                            ++countAttempt;
                        }

                        MessageDetails msgDetails = db.GetMessageSettingsBytrdpCodeAndMsgCode(requeueFile.trdpCode, requeueFile.MsgCode);
                        theServerDetails.OrigSourceFileName = requeueFile.SourceFileName;
                        theServerDetails.DesFileName = requeueFile.OutputFileName;

                        if (isSuccess)
                        {
                            Network netOperation = new Network();
                            isSuccess = netOperation.DeleteRemoteFile(requeuePath);
                            errorMessage = netOperation.ErrorMessage;

                            //if success; delete from requeue table; log status;
                            theRequeueFileDALC.DeleteRequeueFileById(requeueFile.RequeueFileId);

                            if (theServerDetails.TransmissionType == ConstantVariables.TransmissionMode.EMAIL)
                                errorMessage = "The requeue file ['" + requeueFile.OutputFileName + "'] has been moved from " + requeueFolder + " to " + emailTo;
                            else
                                errorMessage = "The requeue file ['" + requeueFile.OutputFileName + "'] has been moved from " + requeueFolder + " to " + theServerDetails.ServerAddress;

                            SendEmailNotification(db, theServerDetails, theServerDetails, msgDetails, requeueFile.SourceFileName, string.Empty, requeueFolder);
                        }
                        else
                        {
                            //if not sucess; movefile from requeue to suspended folder; update requeue table IsActive = 0; log status;
                            string suspendedFolder = requeueFile.MsetBackUpFolder.Substring(0, requeueFile.MsetBackUpFolder.LastIndexOfAny(@"\".ToCharArray())) + @"\" + Config.GetAppSettingsValue("SuspendedFolder", "suspended");

                            try
                            {
                                string suspendedPath = theServerDetails.GetNetworkSourceFile(suspendedFolder, requeueFile.OutputFileName);

                                if (!IAPL.Transport.Util.CommonTools.DirectoryExist(suspendedFolder))
                                    System.IO.Directory.CreateDirectory(suspendedFolder);

                                if (requeueFile.TempExtension.Length > 0)
                                    if (suspendedPath.Substring(suspendedPath.Length - requeueFile.TempExtension.Length, requeueFile.TempExtension.Length)
                                        == requeueFile.TempExtension)
                                        suspendedPath = suspendedPath.Substring(0, suspendedPath.Length - requeueFile.TempExtension.Length);

                                if (theNetTransaction.CopyFileFrom(requeuePath, suspendedPath))
                                {
                                    Network netOperation = new Network();
                                    isSuccess = netOperation.DeleteRemoteFile(requeuePath);
                                    //errorMessage = netOperation.ErrorMessage;

                                    errorMessage = "The requeue file ['" + requeueFile.OutputFileName + "'] has been moved from " + requeuePath + " to " + suspendedFolder;
                                }
                                else
                                    errorMessage = "The requeue file ['" + requeueFile.OutputFileName + "'] file failed on transmisson from " + requeuePath + " to " + suspendedFolder + " server!] " + errorMessage;
                            }
                            catch
                            {
                                errorMessage = "The requeue file ['" + requeueFile.OutputFileName + "'] file failed on transmisson from " + requeuePath + " to " + suspendedFolder + " server!] " + errorMessage;
                            }
                            finally
                            {
                                RequeueFile theRequeueFile = new RequeueFile();
                                theRequeueFile.RequeueFileId = requeueFile.RequeueFileId;
                                theRequeueFile.trdpCode = requeueFile.trdpCode;
                                theRequeueFile.MsgCode = requeueFile.MsgCode;
                                theRequeueFile.SourceFileName = requeueFile.SourceFileName;
                                theRequeueFile.OutputFileName = requeueFile.OutputFileName;
                                theRequeueFile.CreateDate = requeueFile.CreateDate;
                                theRequeueFile.UpdateDate = DateTime.Now;
                                theRequeueFile.IsActive = false;
                                theRequeueFileDALC.SaveRequeueFile(theRequeueFile);
                            }

                            SendEmailNotification(db, theServerDetails, theServerDetails, msgDetails, requeueFile.SourceFileName, errorMessage, requeueFolder);
                        }

                        IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", errorMessage);
                    }
                    #endregion
                    // SR#33117 Ccenriquez / Capad -- November 5, 2009 -- END

                    IAPL.Transport.Transactions.Requeue _requeue = new Requeue();
                    System.Windows.Forms.MethodInvoker simpleDelegate = new System.Windows.Forms.MethodInvoker(_requeue.ProcessQueue);

                    // start FooOneSecond, but pass it some data this time!
                    // look at the second parameter
                    IAsyncResult tag = simpleDelegate.BeginInvoke(null, "no parameter");//simpleDelegate.BeginInvoke(null, "passing some state");

                    simpleDelegate.EndInvoke(tag);

                    // write the state object
                    Console.WriteLine("Requeue State: " + tag.AsyncState.ToString());
                    Console.WriteLine("Requeue:END");


                    //new Thread(new ThreadStart(_requeue.ProcessQueue)).Start();
                    //End Requeue
                    IAPL.Transport.Util.TextLogger.Log("StartApplication", string.Format("{0} - {1}", "Running again...", DateTime.Now.ToString()));
                    getFileList();
                    IAPL.Transport.Util.TextLogger.Log("StartApplication", string.Format("{0} - {1}", "Application is done.", DateTime.Now.ToString()));
                }
                else
                {
                    IAPL.Transport.Util.TextLogger.Log("StartApplication", "Not in timeframe. Rest for now.");
                }
                Thread.Sleep(processInterval);
            };
            pingServer.StopProcess();
            IAPL.Transport.Util.TextLogger.Log("FileTransfer Service", "Application has been stopped.");
            //}
            //catch (Exception ex)
            //{
            //    IAPL.Transport.Util.TextLogger.Log("Application Error", ex.Message + " -3- " + ex.StackTrace);
            //}

        }
        #endregion

        #region Send Email Notification
        private void SendEmailNotification(DbTransaction db, ServerDetails srcServerDetails, ServerDetails desServerDetails, MessageDetails msgDetails, string fileName, string errorMessage, string requeueFolder)
        {
            IAPL.Transport.Transactions.EmailTransaction emailTrans = null;

            bool _mailSent = false;

            // Perform Email notification
            if (errorMessage.Trim().Length < 1)
            {
                #region Success
                IAPL.Transport.Util.TextLogger.Log(IAPL.Transport.Util.TextLogger.messageType.Bulleted, "", fileName + " has been moved from requeue folder to destination server.");

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
                        else
                        {
                            emailTrans.SourceFile = fileName;

                            //OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                            //emailTrans.OutputFile = desServerDetails.GenFileName();

                            //NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                            emailTrans.OutputFile = desServerDetails.DesFileName;
                        }

                        emailTrans.SourceFolder = requeueFolder;


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
                                errorMessage = fileName + " [\'" + fileName + "\' file has been backup on " + msgDetails.BackupFolder + " folder but failed on transmisson to " +
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

                                if (!emailTrans.SendEmailNotification(fileName, fileName, false))
                                {
                                    string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailTrans.ErrorMessage);
                                    if (errorList.Length >= 2)
                                    {
                                        IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[" + fileName + "] " + errorList[1]);
                                    }
                                    else
                                    {
                                        IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                                            "[" + fileName + "] " + emailTrans.ErrorMessage);
                                    }
                                }
                                _mailSent = true;
                            }
                        }
                        else
                        {
                            if (PerformEmail && (!emailTrans.SendEmailNotification(fileName, fileName, true)))
                            {
                                string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailTrans.ErrorMessage);
                                if (errorList.Length >= 2)
                                {
                                    IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[" + fileName + "] " + errorList[1]);
                                }
                                else
                                {
                                    IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                                        "" + fileName + "] " + emailTrans.ErrorMessage);
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
                    errorMessage = fileName + " [\'" + fileName + "\' file has been backup on " + msgDetails.BackupFolder + " folder but failed on transmisson to " +
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
                    emailTrans.SourceFile = fileName; //srcServerDetails.GetSourceFile();

                    //OLD CODE SR#33117 Ccenriquez -- Dec 8, 2009
                    //emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);

                    //NEW CODE SR#33117 Ccenriquez -- Dec 8, 2009
                    if (desServerDetails.TransmissionType == ConstantVariables.TransmissionMode.HTTP)
                        emailTrans.OutputFile = desServerDetails.GetDestinationFile(desServerDetails.ServerFolder);
                    else
                        emailTrans.OutputFile = desServerDetails.DesFileName;

                    if (!emailTrans.SendEmailNotification(fileName, fileName, false))
                    {
                        string[] errorList = IAPL.Transport.Util.TextLogger.ParseError(emailTrans.ErrorMessage);
                        if (errorList.Length >= 2)
                        {
                            IAPL.Transport.Util.TextLogger.LogError(errorList[0], "[" + fileName + "] " + errorList[1]);
                        }
                        else
                        {
                            IAPL.Transport.Util.TextLogger.LogError("Worker-ProcessFile()",
                                "[" + fileName + "] " + emailTrans.ErrorMessage);
                        }
                    }
                }

            }
        }
        #endregion

        // -------------------------------------------------------------------------------------

        #region stop application
        public void StopApplication()
        {
            stopApplication = true;
            pingServer.StopProcess();
        }
        #endregion

        #endregion
    }
}
