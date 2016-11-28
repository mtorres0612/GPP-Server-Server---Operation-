using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using IAPL.ApplicationBlocks.Data.Sql;

namespace IAPL.Web.Interface.DataAccess
{
    public class AuthorizationDB
    {
        private const string APLU_NAME = "IAPL.Web.Interface.DataAccess.Authorization";
        private const string APLI_CODE = "GPPAuthorization";
        private const string PL_VALIDATEUSER_SP = "FT_ValidateUser";
        private const string PL_GETTRDPCODE_SP = "FT_GetTRDPCode";
        private const string PL_PROCESSLOG_SP = "FT_webProcessLogInsertProc";

        
        static AuthorizationDB _instance;


        #region Singelton Access
        public static AuthorizationDB GetInstance()
        {
            if (null == _instance)
            {
                _instance = new AuthorizationDB();
            }

            return _instance;
        }
        #endregion

        public bool ValidateUser(string connectionString, string username, string password)
        {
            DataTable dt = null;
            bool _ret = false;
            try
            {
                SqlParameter[] parameters =	
								{
                                    new SqlParameter("@username", username),									
                                    new SqlParameter("@password",  Utility.Tools.Encrypt(password)),									
									SqlAccessor.SqlParameterBuilder("@returnValue", SqlDbType.Int, ParameterDirection.Output)
								};

                dt = SqlAccessor.ExecuteDataSet(connectionString, SqlAccessor.SqlCommandBuilder(new SqlCommand(PL_VALIDATEUSER_SP), parameters), CommandType.StoredProcedure, "").Tables[0];
                if (Convert.ToInt32(dt.Rows[0][0]) !=0)
                {
                    _ret = true;
                }
            }
            catch (Exception exc)
            {
                //ErrorLog.Log(APLU_NAME, "GetAll():" + exc.Message);
            }
            return _ret;
        }

        public DataTable GetTrdpCode(string connectionString, string username)
        {
            DataTable dt = null;
           
            try
            {
                SqlParameter[] parameters =	
								{
                                    new SqlParameter("@username", username),									
                                    SqlAccessor.SqlParameterBuilder("@returnValue", SqlDbType.Int, ParameterDirection.Output)
								};

                dt = SqlAccessor.ExecuteDataSet(connectionString, SqlAccessor.SqlCommandBuilder(new SqlCommand(PL_GETTRDPCODE_SP), parameters), CommandType.StoredProcedure, "").Tables[0];
              
            }
            catch (Exception exc)
            {
                //ErrorLog.Log(APLU_NAME, "GetAll():" + exc.Message);
            }
            return dt;
        }

        public void ProcessLog(string connectionString, string processSource, bool isSuccess, string description, string technicalError)
        {
             DataTable dt = null;
           
            try
            {
                 SqlParameter[] parameters =	
								{
                                    new SqlParameter("@prlgProcessSource", processSource),									
                                    new SqlParameter("@prlgIsSuccess", isSuccess),									
                                    new SqlParameter("@prlgDescription", description),									
                                    new SqlParameter("@prlgTechnicalErrDesc",technicalError ),									
									SqlAccessor.SqlParameterBuilder("@returnValue", SqlDbType.Int, ParameterDirection.Output)
								};

                 dt = SqlAccessor.ExecuteDataSet(connectionString, SqlAccessor.SqlCommandBuilder(new SqlCommand(PL_PROCESSLOG_SP), parameters), CommandType.StoredProcedure, "").Tables[0];
                
            }
            catch (Exception exc)
            {
                //ErrorLog.Log(APLU_NAME, "GetAll():" + exc.Message);
            }
            


        }
    }
}
