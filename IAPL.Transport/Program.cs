using System;
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport
{
    class Program
    {
        public Program() 
        { 
        
        }
        

        static void Main(string[] args)
        {
           // try
            //{
                IAPL.Transport.Transactions.MessageTransaction mainProcess = new IAPL.Transport.Transactions.MessageTransaction();

                mainProcess.StartApplication();

            //}
            //catch (Exception ex)
           // {
            //    IAPL.Transport.Util.TextLogger.Log("Application Error", ex.Message + " -1- " + ex.StackTrace);
            //}
            ////sourcefilelog table
            //System.Collections.Hashtable hTable = new System.Collections.Hashtable();            
            //hTable.Add("@trdpCode", "TRDP-TEST");
            //hTable.Add("@msgCode", "MSG-TEST");
            //hTable.Add("@erpID", "TXT");
            //hTable.Add("@sflgIsCountrySetup", "1");
            //hTable.Add("@sflgFileType", "1");
            //hTable.Add("@sflgSourceFilename", "2");

            ////processlog table
            //System.Collections.Hashtable hTable2 = new System.Collections.Hashtable();
            //hTable2.Add("@apluCode","MesgCode01");
            //hTable2.Add("@ERPID","");
            //hTable2.Add("@prlgCustID","");
            //hTable2.Add("@prlgProcessSource","ProcessSource");
            //hTable2.Add("@prlgStartDate", DateTime.Now.ToString());
            //hTable2.Add("@prlgEndDate",DateTime.Now.ToString());
            //hTable2.Add("@prlgIsSuccess","1");
            //hTable2.Add("@prlgDescription","Testing....");
            //hTable2.Add("@prlgTechnicalErrDesc","no error");
            //hTable2.Add("@prlgSourceParent","source parent here");
            //hTable2.Add("@prlgSourceParentCount","1");
            //hTable2.Add("@prlgSourceChild","1");
            //hTable2.Add("@prlgSourceChildCount", "1");

            //db.InsertTransactionLog(hTable, hTable2);

            //db.UpdateTransactionLog();            

            //IAPL.Transport.Transaction.FtpTransaction ftpTrans = new IAPL.Transport.Transaction.FtpTransaction();
            //ftpTrans.StartApplication();

            //IAPL.Transport.Transaction.SftpTransaction sftpTrans = new IAPL.Transport.Transaction.SftpTransaction();
            //sftpTrans.DoFtpTransfer();            
        }
    }
}
