////////////////////////////////////////////////////////
// SR#33117 Ccenriquez / Capad -- November 5, 2009  ////
////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using IAPL.Transport.Transactions;
using System.Data;
using IAPL.Transport.Data.Sql;
using IAPL.Transport.Configuration;
using System.Data.SqlTypes;

namespace IAPL.Transport.Data
{
    public partial class RequeueFileDALC
    {
        private string _ErrorMessage = "";

        #region Properties
        public string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }

            set
            {
                _ErrorMessage = value;
            }
        }
        #endregion

        #region Constructor
        public RequeueFileDALC() { }
        #endregion

        #region Standard CRUD Methods
        public List<RequeueFile> GetEveryRequeueFile()
        {
            string tableName = "RequeueFile";

            SqlCommand cmd = BuildGetEveryRequeueFileCommand();

            DataSet ds = SqlAccessor.ExecuteDataSet(Config.GetAppSettingsValue("DBConnectionString", ""),
                    cmd, CommandType.StoredProcedure, tableName);

            List<RequeueFile> entities = new List<RequeueFile>();

            if (ds != null)
            {
                DataTable dt = ds.Tables[0];

                foreach (DataRow drow in dt.Rows)
                {
                    RequeueFile theRequeueFile = new RequeueFile();
                    theRequeueFile.RequeueFileId = Convert.ToInt32(drow["RequeueFileId"]);
                    theRequeueFile.trdpCode = Convert.ToString(drow["trdpCode"]);
                    theRequeueFile.MsgCode = Convert.ToString(drow["MsgCode"]);
                    theRequeueFile.SourceFileName = Convert.ToString(drow["SourceFileName"]);
                    theRequeueFile.OutputFileName = Convert.ToString(drow["OutputFileName"]);
                    theRequeueFile.CreateDate = Convert.ToDateTime(drow["CreateDate"]);
                    theRequeueFile.UpdateDate = (drow["UpdateDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(drow["UpdateDate"]));
                    theRequeueFile.IsActive = Convert.ToBoolean(drow["IsActive"]);
                    theRequeueFile.ERP = Convert.ToString(drow["ERP"]);
                    theRequeueFile.MsetBackUpFolder = Convert.ToString(drow["MsetBackUpFolder"]);
                    theRequeueFile.MessageFileDestinationId = Convert.ToInt32(drow["MessageFileDestinationId"]);
                    theRequeueFile.TransmissionTypeCode = Convert.ToString(drow["TransmissionTypeCode"]);
                    theRequeueFile.TempExtension = Convert.ToString(drow["TempExtension"]);
                    entities.Add(theRequeueFile);
                }
            }

            return entities;
        }

        public bool SaveRequeueFile(RequeueFile theRequeueFile)
        {
            bool success = true;

            SqlCommand cmd = BuildSaveCommand(theRequeueFile);

            try
            {
                object[] returnObject = null;

                int result = SqlAccessor.ExecuteNonQuery(Config.GetAppSettingsValue("DBConnectionString", ""),
                                    cmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                _ErrorMessage = "DbTransaction-RequeueFileDALC-SaveRequeueFile()|" + ex.Message.ToString();
            }

            return success;
        }

        public bool DeleteRequeueFileById(int RequeueFileId)
        {
            bool success = true;

            SqlCommand cmd = BuildDeleteRequeueFileByIdCommand(RequeueFileId);

            try
            {
                object[] returnObject = null;

                int result = SqlAccessor.ExecuteNonQuery(Config.GetAppSettingsValue("DBConnectionString", ""),
                                    cmd, CommandType.StoredProcedure, out returnObject);
            }
            catch (Exception ex)
            {
                _ErrorMessage = "DbTransaction-RequeueFileDALC-DeleteRequeueFileById()|" + ex.Message.ToString();
            }

            return success;
        }
        #endregion

        #region Standard CRUD Database Commands
        private SqlCommand BuildGetEveryRequeueFileCommand()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "GetEveryRequeueFile";
            cmd.CommandType = CommandType.StoredProcedure;

            return cmd;
        }

        private SqlCommand BuildSaveCommand(RequeueFile theRequeueFile)
        {
            SqlCommand cmd = new SqlCommand();

            try
            {
                cmd.CommandText = "SaveRequeueFile";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@RequeueFileId", SqlDbType.Int, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, theRequeueFile.RequeueFileId));
                cmd.Parameters.Add(new SqlParameter("@trdpCode", SqlDbType.VarChar, 20, ParameterDirection.Input, true, 0, 0, null, DataRowVersion.Default, theRequeueFile.trdpCode));
                cmd.Parameters.Add(new SqlParameter("@MsgCode", SqlDbType.VarChar, 30, ParameterDirection.Input, true, 0, 0, null, DataRowVersion.Default, theRequeueFile.MsgCode));
                cmd.Parameters.Add(new SqlParameter("@SourceFileName", SqlDbType.VarChar, 4000, ParameterDirection.Input, true, 0, 0, null, DataRowVersion.Default, theRequeueFile.SourceFileName));
                cmd.Parameters.Add(new SqlParameter("@OutputFileName", SqlDbType.VarChar, 100, ParameterDirection.Input, true, 0, 0, null, DataRowVersion.Default, theRequeueFile.OutputFileName));
                cmd.Parameters.Add(new SqlParameter("@CreateDate", SqlDbType.DateTime, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, theRequeueFile.CreateDate));
                cmd.Parameters.Add(new SqlParameter("@UpdateDate", SqlDbType.DateTime, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, (theRequeueFile.UpdateDate == DateTime.MinValue ? (object)DBNull.Value : theRequeueFile.UpdateDate)));
                cmd.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, theRequeueFile.IsActive));
                cmd.Parameters.Add(new SqlParameter("@TempExtension", SqlDbType.VarChar, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, theRequeueFile.TempExtension));
            }
            catch (Exception ex)
            {
                _ErrorMessage = "DbTransaction-RequeueFileDALC-Save()|" + ex.Message.ToString();
            }

            return cmd;
        }

        private SqlCommand BuildDeleteRequeueFileByIdCommand(int RequeueFileId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "DeleteRequeueFileById";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@RequeueFileId", SqlDbType.Int, 0, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, RequeueFileId));

            return cmd;
        }
        #endregion
    }
}
