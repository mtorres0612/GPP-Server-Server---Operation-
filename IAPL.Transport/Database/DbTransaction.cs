/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Added method GenerateFileTransferDetails
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using IAPL.Transport.Configuration;
using IAPL.Transport.Data.Sql;
using IAPL.Transport.Transactions;

namespace IAPL.Transport.Data
{
    class DbTransaction
    {
        private Hashtable returnTable = null;
        private DataSet commonDataSet = null;
        private DataTable commonDataTable = null;
        private IAPL.Transport.Transactions.MessageDetails fileTransDetails = null;
        private string errorMessage = "";

        private string processLogID = "0";
        private string sourceFileLogID = "0";

        public DbTransaction() { }

        #region properties

        public string GetProcessLogID {
            get {
                return this.processLogID;
            }
        }

        public string GetSourceFileLogID {
            get {
                return this.sourceFileLogID;
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

        public DataTable CommonDataTable {
            get {
                return this.commonDataTable;
            }

            set {
                this.commonDataTable = value;
            }
        }

        public Hashtable HTableInfo {
            set {
                this.returnTable=value;
            }
            get
            {
                return this.returnTable;
            }
        }

        public DataSet DSetInfo {
            set {
                this.commonDataSet = value;
            }
            get {
                return this.commonDataSet;
            }
        }

        public IAPL.Transport.Transactions.MessageDetails FileTranferDetails {
            set
            {
                this.fileTransDetails = value;
            }
            get
            {
                return this.fileTransDetails;
            }
        }

        #endregion

        #region Methods

        //BEGIN SR#33117 Ccenriquez / Capad -- November 16, 2009 
        #region UpdateMsgCounter
        public bool UpdateMsgCounter(string MsgCode, int MsgCounter)
        {
            bool success = true;

            SqlCommand sqlCmd = BuildUpdateMsgCounterCommand(MsgCode, MsgCounter);

            try
            {
                object[] returnObject = null;

                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-UpdateMsgCounter()|" + ex.Message.ToString();
            }

            return success;
        }

        private SqlCommand BuildUpdateMsgCounterCommand(string MsgCode, int MsgCounter)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "UpdateMsgCounter";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@MsgCode", SqlDbType.VarChar, 30, ParameterDirection.Input, true, 0, 0, null, DataRowVersion.Default, MsgCode));
            cmd.Parameters.Add(new SqlParameter("@MsgCounter", SqlDbType.Int, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, MsgCounter));

            return cmd;
        }
        #endregion UpdateMsgCounter
        //END SR#33117 Ccenriquez / Capad -- November 16, 2009 

        #region GetFileTransferInfo
        // SR30441 Oct.19,2009 -- start 
        //old
        //private SqlCommand GenerateFileTransferDetails()
        //{
        //    SqlCommand sqlCmd = new SqlCommand();

        //    try
        //    {
        //        sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_FILELIST; //"FTPToolFileListProc";
        //        sqlCmd.CommandType = CommandType.StoredProcedure;

                //return value
                //SqlParameter orderID = new SqlParameter("@returnValue", SqlDbType.Variant, 4);
                //orderID.Direction = ParameterDirection.Output;
                //sqlCmd.Parameters.Add(orderID);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.ErrorMessage = "DbTransaction-GenerateFileTransferDetails()|" + ex.Message.ToString();
        //    }

        //    return sqlCmd;
        //}

        private SqlCommand GenerateFileTransferDetails(string MsgCodeExcluded)
        {
            SqlCommand sqlCmd = new SqlCommand();

            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_FILELIST; //"FTPToolFileListProc";
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@MsgCodeExcluded", MsgCodeExcluded); //Peng SR#33041 : added SP param MsgCodeExcluded
                // MDO 20160223
                // Add new parameter "ServerID" for multiple instance capability
                sqlCmd.Parameters.AddWithValue("@ServerID", IAPL.Transport.Configuration.Config.GetAppSettingsValue("ServerID", "").Trim()); 
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GenerateFileTransferDetails()|" + ex.Message.ToString();
            }

            return sqlCmd;
        }

        private SqlCommand GenerateFileTransferDetails(string msgCode, string erp, string principal)
        {
            SqlCommand sqlCmd = new SqlCommand();

            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_FILELISTITEM;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("@MsgCode", msgCode);
                sqlCmd.Parameters.AddWithValue("@ERP", erp);
                sqlCmd.Parameters.AddWithValue("@PRNCPL", principal);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GenerateFileTransferDetails()|" + ex.Message.ToString();
            }

            return sqlCmd;
        }


        //public bool GetFileTransferInfo() -- SR30441 Added param (MsgCodeExcluded)
        public bool GetFileTransferInfo(string msgCode, string erp, string principal)
        {
            bool success = true;
            const string TABLENAME = "ftpDetails";

            this.ErrorMessage = "";

            // GET LIST TO CHECK
            //SqlCommand sqlCmd = this.GenerateFileTransferDetails();  -- SR30441 Added param (MsgCodeExcluded)
            SqlCommand sqlCmd = this.GenerateFileTransferDetails(msgCode, erp, principal);

            if (this.ErrorMessage.Trim().Length > 0) {
                return false;
            }

            try
            {
                
                DataSet dSet = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, TABLENAME);

                if (dSet != null)
                {
                    if (dSet.Tables[TABLENAME].Rows.Count > 0)
                    {
                        //this.HTableInfo = new Hashtable();
                        //this.HTableInfo.Add("MaxRows", dSet.Tables[TABLENAME].Rows.Count.ToString());
                        //int index = 0;
                        this.CommonDataTable = dSet.Tables[TABLENAME];
                    }
                }
                else {
                    this.CommonDataTable = new DataTable();
                }
                
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("Error: {0}", ex.Message.ToString());                
                //returnTable.Add("result", ex.Message.ToString());
                this.ErrorMessage = "DbTransaction-GetFileTransferInfo()|" + ex.Message.ToString();
                this.CommonDataTable = new DataTable();
                success = false;
            }

            return success;
        }
   
        #endregion
        // SR30441 Oct.19,2009 -- end


        #region GetServerInfo
        private SqlCommand GenerateServerDetails(Hashtable inputData, string storedProcName)
        {
            SqlCommand sqlCmd = new SqlCommand();
            this.ErrorMessage = "";

            try
            {
                sqlCmd.CommandText = storedProcName;// "FTPToolGetServerDetailsProc";
                sqlCmd.CommandType = CommandType.StoredProcedure;

                foreach (DictionaryEntry dEntry in inputData)
                {
                    sqlCmd.Parameters.Add(new SqlParameter(dEntry.Key.ToString(), dEntry.Value));
                }

                //return value
                //SqlParameter orderID = new SqlParameter("@returnValue", SqlDbType.Variant, 4);
                //orderID.Direction = ParameterDirection.Output;
                //sqlCmd.Parameters.Add(orderID);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GetServerInfo()|" + ex.Message.ToString();
            }

            return sqlCmd;
        }

        public bool GetServerDetails(Hashtable inputData)
        {
            const string TABLENAME = "ServerDetails";
            bool success = true;

            SqlCommand sqlCmd = this.GenerateServerDetails(inputData, IAPL.Transport.Util.ConstantVariables.SP_SERVERDETAILS);// "FTPToolGetServerDetailsProc");
            this.ErrorMessage = "";

            try
            {
                DataSet dSet = new DataSet();
                dSet = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, TABLENAME);

                if (dSet != null)
                {
                    if (dSet.Tables[TABLENAME].Rows.Count > 0)
                    { // this is for the resultset with 1 record only
                        this.HTableInfo = new Hashtable();
                        this.HTableInfo.Add("MaxRows", dSet.Tables[TABLENAME].Rows.Count.ToString());
                        //int index = 0;

                        foreach(DataRow row in dSet.Tables[TABLENAME].Rows)
                        {
                            //index++;
                            foreach(DataColumn column in dSet.Tables[TABLENAME].Columns)
                            {
                                this.HTableInfo.Add(column.ColumnName, row[column].ToString());
                                //Console.WriteLine(row[column]);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("Error: {0}", ex.Message.ToString());
                this.ErrorMessage = "DbTransaction-GetServerDetails()|" + ex.Message.ToString();
                returnTable.Add("result", ex.Message.ToString());
                success = false;
            }

            return success;
        }

        #endregion

        #region GetFTPInfo        
        public bool GetFTPDetails(Hashtable inputData, IAPL.Transport.Util.ConstantVariables.TransmissionMode transmissionType)
        {
            const string TABLENAME = "FTPDetails";
            bool success = true;
            string spName = "";

            //Gerard
            switch (transmissionType) {
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    spName = IAPL.Transport.Util.ConstantVariables.SP_FTPCONNECTIONDETAILS; //"FTPToolGetFTPConnDetailsProc";
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    spName = IAPL.Transport.Util.ConstantVariables.SP_SFTPCONNECTIONDETAILS; // "FTPToolGetSFTPConnDetailsProc";
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.EMAIL:
                    spName = IAPL.Transport.Util.ConstantVariables.SP_EMAILCONNECTIONDETAILS; //"FTPToolGetEmailConnDetailsProc";
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.NETWORK:
                    //this.ErrorMessage = "Network transfer is not yet available!";
                    //success = false;
                    spName = IAPL.Transport.Util.ConstantVariables.SP_NETWORKCONNECTIONDETAILS;
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.HTTP:
                    spName = IAPL.Transport.Util.ConstantVariables.SP_HTTPCONNECTIONDETAILS; 
                    break;
            }

            if (!success) {
                this.HTableInfo = new Hashtable();
                this.HTableInfo.Add("MaxRows", "0");
                return success;
            }

            SqlCommand sqlCmd = this.GenerateServerDetails(inputData, spName);

            try
            {

                DataSet dSet = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, TABLENAME);

                if (dSet != null)
                {
                    if (dSet.Tables[TABLENAME].Rows.Count > 0)
                    { // this is for the resultset with 1 record only
                        this.HTableInfo = new Hashtable();
                        this.HTableInfo.Add("MaxRows", dSet.Tables[TABLENAME].Rows.Count.ToString());
                        //int index = 0;

                        foreach (DataRow row in dSet.Tables[TABLENAME].Rows)
                        {
                            //index++;
                            foreach (DataColumn column in dSet.Tables[TABLENAME].Columns)
                            {
                                this.HTableInfo.Add(column.ColumnName, row[column].ToString());                                
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("Error: {0}", ex.Message.ToString());
                this.ErrorMessage = "DbTransaction-GetFTPDetails()|" + ex.Message.ToString();
                returnTable.Add("result", ex.Message.ToString());
                success = false;
            }

            return success;
        }

        #endregion

        #region InsertTransactionLog
        public bool InsertTransactionLog(Hashtable transInfo, Hashtable transInfo2)
        {
            this.HTableInfo = new Hashtable();
            bool success = true;

            if (!LogSourceFile(transInfo)) {
                string[] aStr = this.ErrorMessage.Split(new char[] { '|' });
                IAPL.Transport.Util.TextLogger.LogError(aStr[0], aStr[1]);
                IAPL.Transport.Util.TextLogger.LogError("Below are the stored procedure's parameter:", "");
                foreach (DictionaryEntry dEntry in transInfo)
                {
                    IAPL.Transport.Util.TextLogger.LogError("tab", dEntry.Key.ToString() + "-" + dEntry.Value);
                }
                IAPL.Transport.Util.TextLogger.LogError("---------------------------------------", "-----------------------");
                success = false;
            }

            if (!this.LogProcessInfo(transInfo2)) {
                string[] aStr = this.ErrorMessage.Split(new char[] { '|' });
                IAPL.Transport.Util.TextLogger.LogError(aStr[0], aStr[1]);
                IAPL.Transport.Util.TextLogger.LogError("Below are the stored procedure's parameter:", "");
                foreach (DictionaryEntry dEntry in transInfo2)
                {
                    IAPL.Transport.Util.TextLogger.LogError("tab", dEntry.Key.ToString() + "-" + dEntry.Value);
                }
                IAPL.Transport.Util.TextLogger.LogError("---------------------------------------", "-----------------------");
                success = false;
            }

            return success;
        }
        #endregion

        #region AddSourceFileTransaction-GenerateSourceFileLog_Insert
        private SqlCommand GenerateSourceFileLog_Insert(Hashtable inputData)
        {
            SqlCommand sqlCmd = new SqlCommand();

            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_SOURCEFILELOGINSERT;  //"FTPToolSourceFileLogInsProc"; 
                sqlCmd.CommandType = CommandType.StoredProcedure;

                foreach (DictionaryEntry dEntry in inputData) { 
                    sqlCmd.Parameters.Add(new SqlParameter(dEntry.Key.ToString(), dEntry.Value));
                }

                //return value
                SqlParameter orderID = new SqlParameter("@SFLGID", SqlDbType.Int, 4);
                orderID.Direction = ParameterDirection.Output;
                sqlCmd.Parameters.Add(orderID);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GenerateSourceFileLog_Insert()|" + ex.Message.ToString();
            }

            return sqlCmd;
        }
        #endregion

        #region logsourcefile
        public bool LogSourceFile(Hashtable fieldList)
        {
            //Hashtable returnTable = new Hashtable();
            bool success = true;

            SqlCommand sqlCmd = GenerateSourceFileLog_Insert(fieldList);

            try
            {
                object[] returnObject = null;

                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, out returnObject);

                string retValue = "0";
                if (returnObject.Length > 0)
                {
                    retValue = returnObject.GetValue(0).ToString();
                }
                //get the sourcefilelog id
                this.sourceFileLogID = retValue;

                //returnObject = null;

                ////Processed
                //returnTable.Add("result", "");
                //returnTable.Add("id", retValue);
                //System.Console.WriteLine("Operation Result: {0} Return Object : {1}", result, retValue);
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("Error: {0}", ex.Message.ToString());
                //returnTable.Add("result", ex.Message.ToString());
                success = false;
                this.ErrorMessage = "DbTransaction-LogSourceFile()|" + ex.Message.ToString();
            }

            return success;
        }

        #endregion

        #region UpdateTransactionLog
        public bool UpdateTransactionLog(bool isSuccess, string transDescription, string errorDescription)
        {
            string tmpStr = "";
            bool success = true;
            System.Collections.Hashtable srcFileTable = new Hashtable();
            srcFileTable.Add("@sflgID", this.sourceFileLogID);
            if (isSuccess)
                srcFileTable.Add("@status", "Processed");
            else
                srcFileTable.Add("@status", "Error");
            srcFileTable.Add("@sflgInterchangeNo", "1");
            srcFileTable.Add("@sflgDocumentNo", "1");

            if (!UpdateSourceFile(srcFileTable)) {
                string[] aStr = this.ErrorMessage.Split(new char[] {'|'});
                IAPL.Transport.Util.TextLogger.LogError(aStr[0], aStr[1]);
                IAPL.Transport.Util.TextLogger.LogError("Below are the stored procedure's parameter:", "");    
                foreach (DictionaryEntry dEntry in srcFileTable)
                {
                    IAPL.Transport.Util.TextLogger.LogError("tab", dEntry.Key.ToString() + "-" + dEntry.Value);
                }
                IAPL.Transport.Util.TextLogger.LogError("---------------------------------------", "-----------------------");
                success = false;
            }

            srcFileTable = new Hashtable();
            srcFileTable.Add("@prlgID", this.processLogID);
            if (isSuccess)
                tmpStr = "1";
            else
                tmpStr = "0";
            srcFileTable.Add("@prlgIsSuccess", tmpStr);
            srcFileTable.Add("@prlgDescription", transDescription);
            srcFileTable.Add("@prlgTechnicalErrDesc", errorDescription);

            if (!updateProcessLog(srcFileTable)) {
                string[] aStr = this.ErrorMessage.Split(new char[] { '|' });
                IAPL.Transport.Util.TextLogger.LogError(aStr[0], aStr[1]);
                IAPL.Transport.Util.TextLogger.LogError("Below are the stored procedure's parameter:", "");
                foreach (DictionaryEntry dEntry in srcFileTable)
                {
                    IAPL.Transport.Util.TextLogger.LogError("tab", dEntry.Key.ToString() + "-" + dEntry.Value);
                }
                IAPL.Transport.Util.TextLogger.LogError("---------------------------------------", "-----------------------");
                success = false;
            }
            return success;
        }       
        #endregion        

        #region UpdateMessageCounter
        public bool UpdateMessageCounter(Hashtable fieldList)
        {
            bool success = true;

            SqlCommand sqlCmd = GenerateServerDetails(fieldList, IAPL.Transport.Util.ConstantVariables.SP_MESSAGECOUNTERUPDATE);//  "FTMessagesUpdateProc"); 

            try
            {

                object[] returnObject = null;

                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, out returnObject);

                //System.Console.WriteLine("Operation Result: {0} Return Object : {1}", result, retValue);
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("Error: {0}", ex.Message.ToString());
                this.ErrorMessage = "DbTransaction-UpdateMessageCounter()|" + ex.Message.ToString();
            }

            return success;
        }
        #endregion

        #region UpdateSourceFile
        private bool UpdateSourceFile(Hashtable fieldList)
        {
            bool success = true;

            SqlCommand sqlCmd = GenerateServerDetails(fieldList, IAPL.Transport.Util.ConstantVariables.SP_SOURCEFILELOGUPDATE);  //"FTSourceFileLogUpdateProc");  

            try
            {

                object[] returnObject = null;

                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, out returnObject);                

                //System.Console.WriteLine("Operation Result: {0} Return Object : {1}", result, retValue);
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("DbTransaction-UpdateSourceFile()|{0}", ex.Message.ToString());
                this.ErrorMessage = "DbTransaction-UpdateSourceFile()|" + ex.Message.ToString();
                success = false;
            }

            return success;
        }
        #endregion

        #region UpdateProcessLog
        private bool updateProcessLog(Hashtable fieldList)
        {
            bool success = true;

            SqlCommand sqlCmd = GenerateServerDetails(fieldList, IAPL.Transport.Util.ConstantVariables.SP_PROCESSLOGUPDATE);// "FTProcessLogUpdateProc");

            try
            {
                SqlParameter retValue = new SqlParameter("@returnValue", SqlDbType.Int, 4);
                retValue.Direction = ParameterDirection.Output;
                sqlCmd.Parameters.Add(retValue);

                object[] returnObject = null;

                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, out returnObject);

                //System.Console.WriteLine("Operation Result: {0} Return Object : {1}", result, retValue);
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("DbTransaction-UpdateSourceFile()|{0}", ex.Message.ToString());
                this.ErrorMessage = "DbTransaction-UpdateSourceFile()|" + ex.Message.ToString();
                success = false;
            }

            return success;
        }

        #endregion

        #region AddProcessLog-GenerateProcessLog_Insert
        private SqlCommand GenerateProcessLog_Insert(Hashtable inputData)
        {
            SqlCommand sqlCmd = new SqlCommand();

            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_PROCESSLOGINSERT; //"FTPToolProcessLogInsProc"; 
                sqlCmd.CommandType = CommandType.StoredProcedure;

                foreach (DictionaryEntry dEntry in inputData)
                {
                    sqlCmd.Parameters.Add(new SqlParameter(dEntry.Key.ToString(), dEntry.Value));
                }                

                //return value
                SqlParameter orderID = new SqlParameter("@returnValue", SqlDbType.Int, 4);
                orderID.Direction = ParameterDirection.Output;
                sqlCmd.Parameters.Add(orderID);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GenerateProcessLog_Insert()|" + ex.Message.ToString();
            }

            return sqlCmd;
        }
        #endregion

        #region logprocessinfo
        public bool LogProcessInfo(Hashtable fieldList)
        {
            //Hashtable returnTable = new Hashtable();
            bool success = true;

            SqlCommand sqlCmd = this.GenerateProcessLog_Insert (fieldList);

            try
            {

                object[] returnObject = null;

                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, out returnObject);

                string retValue = "0";
                if (returnObject.Length > 0)
                {
                    retValue = returnObject.GetValue(0).ToString();
                }

                // get processlog id
                this.processLogID = retValue;

                //Processed
                //returnTable.Add("result", "");
                //returnTable.Add("id", retValue);
                //System.Console.WriteLine("Operation Result: {0} Return Object : {1}", result, retValue);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-LogProcessInfo()|" + ex.Message.ToString();
                success = false;
            }

            return success;//returnTable;
        }
        #endregion

        #region FTP server list
        public bool GetServerList(IAPL.Transport.Util.ConstantVariables.TransmissionMode transMode, string tableName) {
            bool success = true;
            string spName = "";
            System.Collections.Hashtable fieldList = new Hashtable();

            switch (transMode) { 
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP:
                    spName = IAPL.Transport.Util.ConstantVariables.SP_FTPSERVERLIST;
                    break;
                case IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP:
                    spName = IAPL.Transport.Util.ConstantVariables.SP_SFTPSERVERLIST;
                    break;
            }

            if (spName.Trim().Length < 1) {
                this.ErrorMessage = "DbTransaction-GetServerList()|Failed to identify the transmissiontype! " ;
                return false;
            }

            SqlCommand sqlCmd = GenerateServerDetails(fieldList, spName );
                //this.GenerateFileTransferDetails();

            try
            {

                DataSet dSet = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, tableName);

                if (dSet != null)
                {
                    this.CommonDataTable = dSet.Tables[tableName].Copy();
                    //if (dSet.Tables[tableName].Rows.Count > 0)
                    //{
                    //    //this.HTableInfo = new Hashtable();
                    //    //this.HTableInfo.Add("MaxRows", dSet.Tables[TABLENAME].Rows.Count.ToString());
                    //    //int index = 0;
                    //    this.CommonDataTable = dSet.Tables[tableName].Copy();
                    //}
                }
                else {
                    this.CommonDataTable = new DataTable();
                }
                
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("Error: {0}", ex.Message.ToString());                
                //returnTable.Add("result", ex.Message.ToString());
                this.ErrorMessage = "DbTransaction-GetServerList()" + ex.Message.ToString();
                this.CommonDataTable = new DataTable();
                success = false;
            }

            return success;
        }

        #endregion

        #region emaildistributionlist
        public bool emailDistributionList(bool IMS, string mesgCode, string ERP, int repCode)
        {
            bool success = true;
            const string tableName = "emailDetails";
            System.Collections.Hashtable fieldList = new Hashtable();

            SqlCommand sqlCmd = new SqlCommand();

            fieldList.Add("@MsgCode", mesgCode);
            if (IMS)
            {
                fieldList.Add("@RepCode", repCode);
                sqlCmd = GenerateServerDetails(fieldList, IAPL.Transport.Util.ConstantVariables.SP_IMSEMAILDISTRIBUTIONLIST);
            }
            else
            {
                fieldList.Add("@ERP", ERP);
                sqlCmd = GenerateServerDetails(fieldList, IAPL.Transport.Util.ConstantVariables.SP_EMAILDISTRIBUTIONLIST);
            }
            
            try
            {
                DataSet dSet = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                    sqlCmd, CommandType.StoredProcedure, tableName);

                if (dSet != null)
                {
                    if (dSet.Tables[tableName].Rows.Count > 0)
                    {
                        this.CommonDataTable = dSet.Tables[tableName].Copy();
                    }
                }

            }
            catch (Exception ex)
            {
                //System.Console.WriteLine("Error: {0}", ex.Message.ToString());
                IAPL.Transport.Util.TextLogger.LogError("DbTransaction-emailDistributionList()",
                    ex.Message.ToString());
                returnTable.Add("result", ex.Message.ToString());
            }

            return success;
        }
        #endregion

        #region Dispose resources
        public void Dispose() {
            if (this.CommonDataTable != null)
                this.CommonDataTable.Dispose();
            if (this.HTableInfo != null)
                this.HTableInfo.Clear();
            if (this.returnTable != null)
                this.returnTable.Clear();

        }
        #endregion

        #endregion


        // ****************************************************************************
        // Project: ISG12043
        // Developer: Alrazen Estrella
        // Date: Oct. 9, 2008

        #region IMS
        public bool GetIMSCountries(string storedProcName, string DBTable)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            this.ErrorMessage = "";
            try
            {
                sqlCmd.CommandText = storedProcName;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                DataSet dSet = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                  sqlCmd, CommandType.StoredProcedure, DBTable);
                if (dSet != null)
                    this.CommonDataTable = dSet.Tables[DBTable].Copy();
                else
                    this.CommonDataTable = new DataTable();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GetIMSCountries()|" + ex.Message.ToString();
                this.CommonDataTable = new DataTable();
                success = false;
            }
            return success;
        }

        public bool SaveIMSFileProcessed(string IMSProcessId, 
                                         string MsgCode,
                                         string ERP,
                                         string Principal,
                                         string IMSCountryCode, 
                                         string IMSVersionNo,
                                         string IMSFolder, 
                                         string IMSRunType, 
                                         DateTime IMSDateAndTimeExec)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_INSERTIMSFILEPROCESSED;
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@IMSProcessId", IMSProcessId));
                sqlCmd.Parameters.Add(new SqlParameter("@MsgCode", MsgCode));
                sqlCmd.Parameters.Add(new SqlParameter("@ERP", ERP));
                sqlCmd.Parameters.Add(new SqlParameter("@PRNCPL", Principal));
                sqlCmd.Parameters.Add(new SqlParameter("@IMSCountryCode", IMSCountryCode));
                sqlCmd.Parameters.Add(new SqlParameter("@IMSVersionNo", IMSVersionNo));
                sqlCmd.Parameters.Add(new SqlParameter("@IMSFolder", IMSFolder));
                sqlCmd.Parameters.Add(new SqlParameter("@IMSRunType", IMSRunType));
                sqlCmd.Parameters.Add(new SqlParameter("@IMSDateAndTimeExec", IMSDateAndTimeExec));

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-SaveIMSFileProcessed()|" + ex.Message.ToString();
            }
            return success;
        }

        // Delete the IMS Process from the database
        public bool DeleteIMSFileProcessed(string ProcessId)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_DELETEIMSFILEPROCESSED;
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@ProcessId", ProcessId));

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-DeleteIMSFileProcessed()|" + ex.Message.ToString();
            }
            return success;
        }

        // Delete the process in the Temp table
        public bool DeleteIMSProcess(string ProcessId)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_DELETEIMSPROCESS;
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@ProcessId", ProcessId));

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-DeleteIMSProcess()|" + ex.Message.ToString();
            }
            return success;
        }


        public Hashtable GetIMSFilesLeftProcessedBeforeCrashed(out Hashtable ListOfZipFiles, 
                                                               out Hashtable IMSCompleteCountriesName)
        {
            const string TABLENAME = "ftpDetails";

            Hashtable Details = new Hashtable();
            ListOfZipFiles = new Hashtable();
            IMSCompleteCountriesName = new Hashtable(); 

            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_IMSGETFILEPROCESSLIST; 
            sqlCmd.CommandType = CommandType.StoredProcedure;

            try
            {
                DataSet dSet = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                  sqlCmd, CommandType.StoredProcedure, TABLENAME);

                if (dSet != null)
                {
                    foreach (DataTable table in dSet.Tables)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            foreach (DataColumn column in table.Columns)
                            {
                                string filename = row[column].ToString() + "." ;
                                IMSCompleteCountriesName.Add("File" + (Details.Count + 1), row[column].ToString());
                                ListOfZipFiles.Add("File" + (Details.Count + 1), filename + IAPL.Transport.IMS.Process.IMSFileExt);
                                Details.Add("File" + (Details.Count + 1), filename + IAPL.Transport.IMS.Process.IMSFileExt);
                                Details.Add("File" + (Details.Count + 1), filename + IAPL.Transport.IMS.Process.IMSLogExt);
                                Details.Add("File" + (Details.Count + 1), filename + IAPL.Transport.IMS.Process.IMSTerminatorExt);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GetIMSFilesLeftProcessedBeforeCrashed()|" + ex.Message.ToString();
            }
            return Details;
        }



        
        // Set the IMS Process ID from the Temp table
        public bool SetIMSProcessId(string MsgCode, string ERP, string Principal, string ProcessId)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_SetIMSProcessID;
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@MsgCode", MsgCode));
                sqlCmd.Parameters.Add(new SqlParameter("@ERP", ERP));
                sqlCmd.Parameters.Add(new SqlParameter("@Principal", Principal));
                sqlCmd.Parameters.Add(new SqlParameter("@ProcessId", ProcessId));

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-SetIMSProcessId()|" + ex.Message.ToString();
            }
            return success;
        }

        
        public bool UpdateIMSFileStatus(string IMSProcessId, string IMSCountryCode)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_UPDATEIMSFILEPROCESSED;
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@IMSProcessId", IMSProcessId));
                sqlCmd.Parameters.Add(new SqlParameter("@IMSCountryCode", IMSCountryCode));

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);

            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-UpdateIMSFileStatus()|" + ex.Message.ToString();
            }
            return success;
        }

        //Gerard

        //public const string SP_GETDISTINCTIMSLastSendDate = "FT_IMSMailStatusIns";
        //public const string SP_INSIMSMailStatus = "FT_IMSLastSendDateGET";
        public DataTable GetLastSendDate(string msgCode, string ERP, string Prncpal)
        {
            DataTable dt = null;

            try
            {


                SqlParameter[] parameters = { 
                    new SqlParameter("@MsgCode", msgCode),
                    new SqlParameter("@Prncpl", Prncpal),
                    new SqlParameter("@ERP", ERP),
                };
                dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), IAPL.Transport.Data.Sql.SqlAccessor.SqlCommandBuilder(new SqlCommand(IAPL.Transport.Util.ConstantVariables.SP_GETDISTINCTIMSLastSendDate), parameters), CommandType.StoredProcedure, "").Tables[0];
            }
            catch (Exception exc)
            {
                this.ErrorMessage = "DbTransaction-GetLastSendDate()|" + exc.Message.ToString();
            }
            return dt;
        }

        public bool SaveIMSMailStatus(string MsgCode, string Prncpal, string ERP)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_INSIMSMailStatus;
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.Add(new SqlParameter("@MsgCode", MsgCode));
                sqlCmd.Parameters.Add(new SqlParameter("@Prncpl", Prncpal));
                sqlCmd.Parameters.Add(new SqlParameter("@ERP", ERP));
                
                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-SaveIMSMailStatus()|" + ex.Message.ToString();
            }
            return success;
        }




        #endregion

        // ****************************************************************************
        #region HTTP Process
        public DataTable GetHTTPProcess(string tradingCode, string msgCode, string Prncpal, string SupplierID, string FileName)
        {
    /*
    @TradingCode VARCHAR(50),
	@MsgCode VARCHAR(200),
	@Prncpal VARCHAR(50),
	@SourceFile VARCHAR(200),
	@SupplierID VARCHAR(50),
	@FileName VARCHAR(250)
            */
            DataTable dt = null;
      
            try
            {

               
                SqlParameter[] parameters = { 
                    new SqlParameter("@TradingCode", tradingCode), 
                    new SqlParameter("@MsgCode", msgCode),
                    new SqlParameter("@Prncpal", Prncpal),
                    new SqlParameter("@SupplierID", SupplierID),
                    new SqlParameter("@FileName", FileName),
                };
                dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), IAPL.Transport.Data.Sql.SqlAccessor.SqlCommandBuilder(new SqlCommand(IAPL.Transport.Util.ConstantVariables.SP_GETHttpProcess), parameters), CommandType.StoredProcedure, "").Tables[0];
            }
            catch (Exception exc)
            {
                this.ErrorMessage = "DbTransaction-GetHTTPProcess()|" + exc.Message.ToString();
            }
            return dt;
        }
        public bool SaveHTTPProcess(string TradingCode, string MsgCode,
                                     string Prncpal,
                                        string SourceFile,
                                        string SupplierID,
                                        string FileName,
                                        string Guid,
                                        string Path)
            
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_INSERTHTTPPROCESS  ;
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@TradingCode", TradingCode));
                sqlCmd.Parameters.Add(new SqlParameter("@MsgCode", MsgCode));
                sqlCmd.Parameters.Add(new SqlParameter("@Prncpal", Prncpal));
                sqlCmd.Parameters.Add(new SqlParameter("@SourceFile", SourceFile));
                sqlCmd.Parameters.Add(new SqlParameter("@SupplierID", SupplierID));
                sqlCmd.Parameters.Add(new SqlParameter("@FileName", FileName));
                sqlCmd.Parameters.Add(new SqlParameter("@Path", Path));
                sqlCmd.Parameters.Add(new SqlParameter("@Guid", Guid));
               
                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-SaveHTTPProcess()|" + ex.Message.ToString();
            }
            return success;
        }


        public bool DeleteHTTPProcess(string TradingCode, string MsgCode,
                                     string Prncpal, string SupplierID, string FileName)
        {
            
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_DELHttpProcess;
                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add(new SqlParameter("@TradingCode", TradingCode));
                sqlCmd.Parameters.Add(new SqlParameter("@MsgCode", MsgCode));
                sqlCmd.Parameters.Add(new SqlParameter("@Prncpal", Prncpal));
                sqlCmd.Parameters.Add(new SqlParameter("@SupplierID", SupplierID));
                sqlCmd.Parameters.Add(new SqlParameter("@FileName", FileName));
                
               object[] returnObject = null;
               int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), sqlCmd, CommandType.StoredProcedure, out returnObject);
               
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-SaveHTTPProcess()|" + ex.Message.ToString();
            }
            return success;
        }


        public bool UpdateHTTPProcess(string URL, string UserName, string Password, string Path)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_UPDHttpProcess;
                sqlCmd.CommandType = CommandType.StoredProcedure;


                Password = IAPL.Transport.Util.Utility.Encrypt(Password);
                
                Uri _uri = new Uri(Path);
                Path = _uri.LocalPath;
                
                sqlCmd.Parameters.Add(new SqlParameter("@Url", URL));
                sqlCmd.Parameters.Add(new SqlParameter("@UserName", UserName));
                sqlCmd.Parameters.Add(new SqlParameter("@password", Password));
                sqlCmd.Parameters.Add(new SqlParameter("@Path", Path));

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);

            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-UpdateHTTPProcess|" + ex.Message.ToString();
            }
            return success;
        }
 

        //HTTP EMAILS
         public bool CheckIFHTTPSendingFailed(string MsgCode, string Prncpal, string FileName)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_GETHTTPFAILED;
                sqlCmd.CommandType = CommandType.StoredProcedure;


                //Password = IAPL.Transport.Util.Utility.Encrypt(Password);
                
                //Uri _uri = new Uri(Path);
                //Path = _uri.LocalPath;
                
                sqlCmd.Parameters.Add(new SqlParameter("@msgCode", MsgCode));
                sqlCmd.Parameters.Add(new SqlParameter("@Prncpal", Prncpal));
                sqlCmd.Parameters.Add(new SqlParameter("@FileName", FileName));
                

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);

                if (result != 0)
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-CheckIFHTTPSendingFailed|" + ex.Message.ToString();
            }
            return success;
        }
        public string GetXSLTPath(string MsgCode, string Prncpal)
        {


            
            string _return = string.Empty;
            DataTable _dt = null;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                SqlParameter[] parameters = { 
                    new SqlParameter("@MsgCode", MsgCode),
                    new SqlParameter("@ERP", Prncpal)};

                _dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), IAPL.Transport.Data.Sql.SqlAccessor.SqlCommandBuilder(new SqlCommand(IAPL.Transport.Util.ConstantVariables.SP_GETXSLTPATH), parameters), CommandType.StoredProcedure, "").Tables[0];

                //object[] returnObject = null;
                //_dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                //sqlCmd, CommandType.StoredProcedure, out returnObject);

                if(_dt.Rows.Count != 0)
                {
                    _return = _dt.Rows[0]["emldXSLTPath"].ToString();
                }
                return _return;
                
            }
            catch (Exception ex)
            {
                _return = string.Empty;
                this.ErrorMessage = "DbTransaction-GetXSLTPath|" + ex.Message.ToString();
            }
            return _return;
        }

        public DataTable GetEmailInfo(string MsgCode, string Prncpal)
        {



            
            DataTable _dt = null;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                SqlParameter[] parameters = { 
                    new SqlParameter("@MsgCode", MsgCode),
                    new SqlParameter("@ERP", Prncpal)};

                _dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), IAPL.Transport.Data.Sql.SqlAccessor.SqlCommandBuilder(new SqlCommand(IAPL.Transport.Util.ConstantVariables.SP_GETXSLTPATH), parameters), CommandType.StoredProcedure, "").Tables[0];

                //object[] returnObject = null;
                //_dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                //sqlCmd, CommandType.StoredProcedure, out returnObject);

                
               

            }
            catch (Exception ex)
            {
                
                this.ErrorMessage = "DbTransaction-GetXSLTPath|" + ex.Message.ToString();
            }
            return _dt;
        }


        public bool UpdateAllHTTPProcess(string URL, string UserName, string Password, string Guid)
        {
            bool success = true;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                sqlCmd.CommandText = IAPL.Transport.Util.ConstantVariables.SP_UPDAllHttpProcess;
                sqlCmd.CommandType = CommandType.StoredProcedure;


                Password = IAPL.Transport.Util.Utility.Encrypt(Password);

                Uri _uri = new Uri(Guid);
                Guid = _uri.LocalPath;

                sqlCmd.Parameters.Add(new SqlParameter("@Url", URL));
                sqlCmd.Parameters.Add(new SqlParameter("@UserName", UserName));
                sqlCmd.Parameters.Add(new SqlParameter("@password", Password));
                sqlCmd.Parameters.Add(new SqlParameter("@Guid", Guid));

                object[] returnObject = null;
                int result = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteNonQuery(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""),
                                                                                 sqlCmd, CommandType.StoredProcedure, out returnObject);

            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "DbTransaction-UpdateAllHTTPProcess|" + ex.Message.ToString();
            }
            return success;
        }

        //FT_HTTPProcessCheckForEntries
        public int HTTPProcessCheckForEntries(string MsgCode)
        {
            int _return = 0;
            DataTable _dt = null;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                SqlParameter[] parameters = { 
                    new SqlParameter("@MsgCode", MsgCode)};

                _dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), IAPL.Transport.Data.Sql.SqlAccessor.SqlCommandBuilder(new SqlCommand(IAPL.Transport.Util.ConstantVariables.SP_GETHTTPPROCESSCHECK), parameters), CommandType.StoredProcedure, "").Tables[0];

        
                _return = (int)_dt.Rows[0][0];

            }
            catch (Exception ex)
            {

                this.ErrorMessage = "DbTransaction-HTTPProcessCheckForEntries|" + ex.Message.ToString();
            }
            return _return;
        }
        //START THE REQUEUE HERE
        //
        public List<string> GetDistinctHTTPProcess()
        {
            List<string> _return = new List<string>();
            DataTable _dt = null;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                SqlParameter[] parameters = {};

                _dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), IAPL.Transport.Data.Sql.SqlAccessor.SqlCommandBuilder(new SqlCommand(IAPL.Transport.Util.ConstantVariables.SP_GETDISTINCTHTTPPROCESS), parameters), CommandType.StoredProcedure, "").Tables[0];

                for (int i = 0; i < _dt.Rows.Count; i++)
                {
                    _return.Add(_dt.Rows[i][0].ToString());
                }
                
            }
            catch (Exception ex)
            {

                this.ErrorMessage = "DbTransaction-GetDistinctHTTPProcess|" + ex.Message.ToString();
            }
            return _return;
        }

        public  DataTable GetHTTPProcessOrderByFileNameAndDate()
        {
            
            DataTable _dt = null;
            SqlCommand sqlCmd = new SqlCommand();
            try
            {
                SqlParameter[] parameters = {};

                _dt = IAPL.Transport.Data.Sql.SqlAccessor.ExecuteDataSet(IAPL.Transport.Configuration.Config.GetAppSettingsValue("DBConnectionString", ""), IAPL.Transport.Data.Sql.SqlAccessor.SqlCommandBuilder(new SqlCommand(IAPL.Transport.Util.ConstantVariables.SP_GETHTTPPROCESSWITHORDER), parameters), CommandType.StoredProcedure, "").Tables[0];

               
            }
            catch (Exception ex)
            {

                this.ErrorMessage = "DbTransaction-FT_HTTPGetALLProcessProcWithOrder|" + ex.Message.ToString();
            }
            return _dt;
        }

        //SR#33117 Ccenriquez -- Dec 10, 2009
        public IAPL.Transport.Transactions.MessageDetails GetMessageSettingsBytrdpCodeAndMsgCode(string trdpCode, string MsgCode)
        {
            string tableName = "GetMessageSettingsBytrdpCodeAndMsgCode";

            SqlCommand cmd = BuildGetMessageSettingsBytrdpCodeAndMsgCodeCommand(trdpCode, MsgCode);

            DataSet ds = SqlAccessor.ExecuteDataSet(Config.GetAppSettingsValue("DBConnectionString", ""),
                    cmd, CommandType.StoredProcedure, tableName);
            
            MessageDetails messageDetails = new MessageDetails(
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetSourceFileMask"]),
                        IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetBackUpFolder"]));

            messageDetails.ERP = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["ERP"]);
            messageDetails.Principal = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["PRNCPL"]);
            messageDetails.TradingCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["trdpCode"]);
            messageDetails.TradingName = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["trdpName"]);
            messageDetails.MessageCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsgCode"]);
            messageDetails.ApplicationCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["apluCode"]);

            messageDetails.SupplierID = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["SUPPID"]);
            messageDetails.SupplierName = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["SUPPNAME"]);
            messageDetails.SetSendSuccessNotification = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetSendSuccessNotification"]);

            messageDetails.StartDate = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");

            #region < Set messageDetails property for File Terminator and Zipping Functionality >
            //LENIN
            messageDetails.FITEFileMask = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetFITEMask"]);
            messageDetails.SetZippingFunctionality = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetIsZip"]);
            messageDetails.ZIPPassword = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetZipPassword"]);
            messageDetails.Retention = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetRetention"]);
            #endregion

            // **********************************************************
            // Developer: Alrazen Estrella
            // Date: July 17, 2008
            // Project: ISG12152

            messageDetails.SetZipSource = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetIsZipSource"]);
            messageDetails.SetFilesSentSingle = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetFilesSentSingle"]);
            messageDetails.SetFilesSentBatch = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetFilesSentBatch"]);

            // **********************************************************

            // **********************************************************
            // Developer: Alrazen Estrella
            // Date: September 4, 2008
            // Project: ISG12128

            messageDetails.SetFileConvertionFlag = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetFileConvertionFlag"]);
            messageDetails.SourceCodePage = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetSourceCodePage"]));
            messageDetails.DestinationCodePage = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetDestinationCodePage"]));

            // **********************************************************

            // **********************************************************
            // Developer: Alrazen Estrella
            // Date: September 25, 2008
            // Project: ISG12043

            messageDetails.MsetFilePickupDelay = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetFilePickupDelay"]));
            //messageDetails.IndividualProcess = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["IndividualProcess"]));
            messageDetails.MsgManualRunFlag = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsgManualRunFlag"]));
            messageDetails.MsgStartTime = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsgStartTime"]));
            messageDetails.MsgEndTime = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsgEndTime"]));
            messageDetails.MsetBatchRun = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetBatchRun"]));
            messageDetails.MsetBatchTime = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetBatchTime"]));
            messageDetails.MsetRuntime = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetRuntime"]));
            messageDetails.MsetStartTime = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetStartTime"]));
            messageDetails.MsetEndTime = (DateTime)DateTime.Parse(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetEndTime"]));
            messageDetails.MsetInterval = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetInterval"]));
            //messageDetails.IMSBatchRun = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetIMSBatchRun"]));
            //messageDetails.IMSFolder = "";
            //messageDetails.CrashStatus = Convert.ToBoolean(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["CrashStatus"]));

            try
            {
                messageDetails.IMSProcessId = IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["ProcessId"]);
            }
            catch
            { }

            //SR#34273 Ccenriquez -- December 4, 2009
            messageDetails.MsetMaxThreadCount = Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)ds.Tables[0].Rows[0]["MsetMaxThreadCount"]));

            return messageDetails;
        }

        private SqlCommand BuildGetMessageSettingsBytrdpCodeAndMsgCodeCommand(string trdpCode, string MsgCode)
        {
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd.CommandText = "GetMessageSettingsBytrdpCodeAndMsgCode";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@trdpCode", SqlDbType.VarChar, 20, ParameterDirection.Input, true, 0, 0, null, DataRowVersion.Default, trdpCode));
                cmd.Parameters.Add(new SqlParameter("@MsgCode", SqlDbType.VarChar, 30, ParameterDirection.Input, true, 0, 0, null, DataRowVersion.Default, MsgCode));
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "DbTransaction-GetMessageSettingsBytrdpCodeAndMsgCode()|" + ex.Message.ToString();
            }

            return cmd;
        }
        #endregion
    }
}
