using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using IAPLRemoteInterface;

namespace IAPL.Transport.Transactions
{
    class PingServer
    {
        #region local variables
        private IIAPLRemoteService _resService;
        //private IAPL.Transport.Transactions.MessageDetails mesgDetails = null;
        private System.Data.DataSet mainDataSet = null;
        private IChannel clientChannel = null;
        private TcpServerChannel serverChannel = null;
        #endregion

        #region constructor
        public PingServer() {

        }

        ~ PingServer()
        {
            try
            {
                if (this.clientChannel != null)
                {
                    ChannelServices.UnregisterChannel(clientChannel);
                }
                if (this.serverChannel != null)
                {
                    ChannelServices.UnregisterChannel(serverChannel);
                }
            }
            catch { }
        }
        #endregion

        #region methods

        #region initServerList
        private void initServerList() {

            mainDataSet = new System.Data.DataSet();

            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            //get the FTP servers
            if (db.GetServerList(IAPL.Transport.Util.ConstantVariables.TransmissionMode.FTP, "ftpDetails")) {
                mainDataSet.Tables.Add(db.CommonDataTable.Copy());
            }            

            //get the SFTP servers
            if (db.GetServerList(IAPL.Transport.Util.ConstantVariables.TransmissionMode.SFTP, "sftpDetails")) {
                mainDataSet.Tables.Add(db.CommonDataTable.Copy());
            }            

            //db.GetFileTransferInfo();

            //System.Data.DataTable dTable = db.CommonDataTable;

            //if (dTable.Rows.Count > 0) {
            //    System.Data.DataRow row = dTable.Rows[0];
            //    //foreach (System.Data.DataRow row in dTable.Rows) {

            //    mesgDetails = new MessageDetails(row["MsetSourceFileMask"].ToString(), row["MsetBackUpFolder"].ToString());

            //    mesgDetails.ERP = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["ERP"]);
            //    mesgDetails.Principal = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["PRNCPL"]);
            //    mesgDetails.TradingCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["trdpCode"]);
            //    mesgDetails.TradingName = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["trdpName"]);
            //    mesgDetails.MessageCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["MsgCode"]);
            //    mesgDetails.ApplicationCode = IAPL.Transport.Util.CommonTools.ValueToString((Object)row["apluCode"]);
            //    //}
            //}            
        }
        #endregion

        #region runClientNow
        private void runClientNow(string serverIP, string serverPort, string appName) {

            try
            {
                if (clientChannel != null)
                {                    
                    ChannelServices.UnregisterChannel(clientChannel);
                }
                clientChannel = new TcpClientChannel();

                string pingURL = "tcp://" + serverIP + ":" + serverPort + "/IAPLRemoteService";
                    //IAPL.Transport.Configuration.Config.GetAppSettingsValue("pingserverurl", "tcp://localhost:9988/IAPLRemoteService");

                ChannelServices.RegisterChannel(clientChannel, false);
                _resService = (IIAPLRemoteService)Activator.GetObject(typeof(IIAPLRemoteService), pingURL);

                _resService.Connect(appName);
                //_resService.Connect(IAPL.Transport.Configuration.Config.GetAppSettingsValue("pingservername", "FTP Tool"));
            }
            catch (Exception ex)
            {
                IAPL.Transport.Util.TextLogger.LogError("PingServer-initPingServer()|[IPAddress: " + serverIP + ";Port: " + serverIP + ";AppName: " + appName + "]", ex.Message.ToString());
                //logErrorToDb( "PingServer-initPingServer()|[IPAddress: " + serverIP + ";Port: " + serverIP + ";AppName: " + appName + "]" + ex.Message.ToString() );
            }
        }
        #endregion

        #region runServerNow

        private bool runServerNow() {
            int serverPort = 0;
            bool success = true;

            if (this.serverChannel == null)
            {
                try
                {
                    serverPort = Convert.ToInt16(IAPL.Transport.Configuration.Config.GetAppSettingsValue("pingserverport", "9988"));
                }
                catch
                {
                    serverPort = 9988;
                }

                try
                {
                    serverChannel = new TcpServerChannel(serverPort);
                    ChannelServices.RegisterChannel(serverChannel, false);
                    RemotingConfiguration.RegisterWellKnownServiceType(typeof(IAPL.Transport.Util.IAPLRemoteService),
                        "IAPLRemoteService", WellKnownObjectMode.SingleCall);

                    //serverChannel = new TcpServerChannel(serverPort);
                    //ChannelServices.RegisterChannel(serverChannel, false);
                    //RemotingConfiguration.RegisterWellKnownServiceType(typeof(IIAPLRemoteService),
                    //    "IAPLRemoteService", WellKnownObjectMode.SingleCall);

                    //TcpServerChannel channel = new TcpServerChannel(Configuration.RemoteServerPort);
                    //ChannelServices.RegisterChannel(channel, false);
                    //RemotingConfiguration.RegisterWellKnownServiceType(typeof(IAPLRemoteService),
                    //    "IAPLRemoteService", WellKnownObjectMode.SingleCall);

                    IAPL.Transport.Util.TextLogger.Log("Initialize Ping server", "running....");
                }
                catch (Exception ex)
                {
                    IAPL.Transport.Util.TextLogger.LogError("PingServer-runServerNow()", ex.Message.ToString());
                    success = false;
                }
            }
            else {
                //IAPL.Transport.Util.TextLogger.Log("Ping server", "running.");
            }

            return success;
        }
        #endregion

        #region logErrorToDb
        private void logErrorToDb(string errorMesg){
            IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

            System.Collections.Hashtable hTable2 = new System.Collections.Hashtable();
            //hTable2.Add("@apluCode", mesgDetails.ApplicationCode);
            //hTable2.Add("@PRNCPL", mesgDetails.Principal);
            //hTable2.Add("@msgCode", mesgDetails.MessageCode);
            //hTable2.Add("@ERPID", mesgDetails.ERP);
            hTable2.Add("@apluCode", "FILETRANSFER");
            hTable2.Add("@PRNCPL", "");
            hTable2.Add("@msgCode", "");
            hTable2.Add("@ERPID", "");
            hTable2.Add("@prlgCustID", "");
            hTable2.Add("@prlgProcessSource", "");
            hTable2.Add("@prlgStartDate", DateTime.Now.ToString());
            hTable2.Add("@prlgEndDate", DateTime.Now.ToString()); // this should appear on the update method
            hTable2.Add("@prlgIsSuccess", "0"); // this should appear on the update method
            hTable2.Add("@prlgDescription", "FILETRANSFER: Ping server - failed.");
            hTable2.Add("@prlgTechnicalErrDesc", errorMesg);
            hTable2.Add("@prlgSourceParent", "");
            hTable2.Add("@prlgSourceParentCount", "0");
            hTable2.Add("@prlgSourceChild", "0");
            hTable2.Add("@prlgSourceChildCount", "0");

            //log to text file if with error
            if (!db.LogProcessInfo(hTable2)) {
                string[] aStr = db.ErrorMessage.Split(new char[] { '|' });
                IAPL.Transport.Util.TextLogger.LogError(aStr[0], aStr[1]);
                IAPL.Transport.Util.TextLogger.LogError("Below are the stored procedure's parameter:", "");
                foreach (DictionaryEntry dEntry in hTable2)
                {
                    IAPL.Transport.Util.TextLogger.LogError("tab", dEntry.Key.ToString() + "-" + dEntry.Value);
                }
                IAPL.Transport.Util.TextLogger.LogError("---------------------------------------", "-----------------------");
                //success = false;
            }
        }
        #endregion

        #region startClientProcess
        public void StartClientProcess() {
            initServerList();
            string appName = IAPL.Transport.Configuration.Config.GetAppSettingsValue("pingservername", "FTP Tool");
            string serverPort = IAPL.Transport.Configuration.Config.GetAppSettingsValue("pingserverport", "9988");
            string serverIP = "";

            foreach (System.Data.DataTable dt in this.mainDataSet.Tables)
            {
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    serverIP = IAPL.Transport.Util.CommonTools.ValueToString(row["ipaddress"]);
                    if (serverIP.Trim().Length > 0 && serverPort.Trim().Length > 0)
                    {
                        this.runClientNow(serverIP, serverPort, appName);
                    }
                }
            }

            for (int i = 0; i < mainDataSet.Tables.Count; i++)
            {
                mainDataSet.Tables[i].Dispose();
            }
            mainDataSet.Dispose();
        }
        #endregion

        #region stopServer

        private void stopAllPingConnections()
        {
            try
            {
                if (this.serverChannel == null)
                {
                    ChannelServices.UnregisterChannel(serverChannel);
                }

                if (clientChannel != null)
                {
                    ChannelServices.UnregisterChannel(clientChannel);
                }
                IAPL.Transport.Util.TextLogger.Log("PingServer-stopAllPingConnections()", "The channel has been unregistered successfully.");
            }
            catch (Exception ex)
            {
                IAPL.Transport.Util.TextLogger.LogError("PingServer-stopAllPingConnections()|", ex.Message.ToString());
                //logErrorToDb( "PingServer-initPingServer()|[IPAddress: " + serverIP + ";Port: " + serverIP + ";AppName: " + appName + "]" + ex.Message.ToString() );
            }
        }
        #endregion

        #region startprocess
        public void StartProcess()
        {
            this.runServerNow();
        }
        #endregion

        #region stopprocess
        public void StopProcess()
        {
            this.stopAllPingConnections();
        }
        #endregion

        #endregion
    }
}
