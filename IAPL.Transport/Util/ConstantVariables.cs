/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Added constant 'SP_FILELISTITEM'
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport.Util
{
    //OLD CODE SR#33117 Ccenriquez / Capad -- November 18, 2009
    //class ConstantVariables

    //NEW CODE SR#33117 Ccenriquez / Capad -- November 18, 2009
    public class ConstantVariables
    {

        public enum FileDirection  {
            NONE =0,
            RECEIVE,
            SEND
        };

        public enum FileAction
        {
            NONE = 0,
            GETFILELIST,
            DOWNLOAD,
            UPLOAD
        };

        public enum TransmissionMode
        {
            NONE = 0,
            FTP,
            SFTP,
            NETWORK,
            EMAIL,
            HTTP
        };

        public enum TransStatus {
            NOERROR = 0,
            ERROR,
            ONGOING
        };

        public const string ERRORRESULT = "###error###";

        public const string SP_MESSAGECOUNTERUPDATE = "FT_MessagesUpdateProc";
        public const string SP_PROCESSLOGUPDATE = "FT_ProcessLogUpdateProc";
        public const string SP_PROCESSLOGINSERT = "FT_ProcessLogInsertProc";

        public const string SP_SOURCEFILELOGINSERT = "FT_SourceFileLogInsertProc";
        public const string SP_SOURCEFILELOGUPDATE = "FT_SourceFileLogUpdateProc";

        public const string SP_FILELIST = "FT_FileListProc";
        public const string SP_FILELISTITEM = "FT_FileListItemProc";
        //public const string SP_FILELIST = "FT_FileListProc_v1_4";

        public const string SP_EMAILCONNECTIONDETAILS = "FT_GetEmailConnDetailsProc";
        public const string SP_FTPCONNECTIONDETAILS = "FT_GetFTPConnDetailsProc";
        public const string SP_HTTPCONNECTIONDETAILS = "FT_GetHTTPConnDetailsProc";
        
        public const string SP_SERVERDETAILS = "FT_GetServerDetailsProc";
        public const string SP_SFTPCONNECTIONDETAILS = "FT_GetSFTPConnDetailsProc";

        public const string SP_FTPSERVERLIST = "FT_FTPServerListProc";
        public const string SP_SFTPSERVERLIST = "FT_SFTPServerListProc";

        public const string SP_EMAILDISTRIBUTIONLIST = "FT_GetEmailDistributionListProc";

        public const string SP_NETWORKCONNECTIONDETAILS = "FT_GetNetworkConnDetailsProc";
        
        // *******************************************************************
        // Project: ISG12043
        // Developer: Alrazen Estrella
        // Date: September 23, 2008

        public const string SP_DELETEIMSFILEPROCESSED = "FT_IMSFileProcessedDel";
        public const string SP_INSERTIMSFILEPROCESSED = "FT_IMSFileProcessedIns";
        public const string SP_UPDATEIMSFILEPROCESSED = "FT_IMSFileProcessedUpd";
        public const string SP_IMSGETFILEPROCESSLIST = "FT_IMSFILEPROCESSSEL";
        public const string SP_DELETEIMSPROCESS = "FT_IMSProcessedDel";
        public const string SP_SetIMSProcessID = "FT_IMSSetProcessId";
        public const string SP_GetIMSDetails = "FT_IMSGetCountries";
        public const string SP_IMSEMAILDISTRIBUTIONLIST = "FT_IMSGetEmailDistributionListProc";
                      
        public const string IMSCodeName = "IMS";
        public const string SP_INSERTHTTPPROCESS = "FT_HTTPInsProcessProc";
        public const string SP_GETHttpProcess = "FT_HTTPGetProcessProc";
        public const string SP_DELHttpProcess = "FT_HTTPDelProcessProc";
        public const string SP_UPDHttpProcess = "FT_HTTPUpdProcessProc";
        public const string SP_GETHTTPFAILED = "FT_GetHTTPFailedProc";
        public const string SP_GETXSLTPATH = "FT_EmailDistributionListSelProc";
        public const string SP_UPDAllHttpProcess = "FT_HTTPUpdAllProcessProc";
        public const string SP_GETHTTPPROCESSCHECK = "FT_HTTPProcessCheckForEntries";
       
        public const string SP_GETDISTINCTHTTPPROCESS = "FT_HTTPProcessSelDistinctProc";
        public const string SP_GETHTTPPROCESSWITHORDER = "FT_HTTPGetALLProcessProcWithOrder";

        public const string SP_GETDISTINCTIMSLastSendDate = "FT_IMSLastSendDateGET";
        public const string SP_INSIMSMailStatus = "FT_IMSMailStatusIns";

        // *******************************************************************
    }
}
