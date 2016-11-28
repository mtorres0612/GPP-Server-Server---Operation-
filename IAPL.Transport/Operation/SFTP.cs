/******************************************************
 * 
 *  Change Log:
 * 
 *  MNTORRES 09162016 : Removed dependency to Rebex
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using Renci.SshNet;
using System.IO;
using System.Linq;
namespace IAPL.Transport.Operation
{
    class SFTP
    {
        //private Sftp client = null;
        private SftpClient client = null;
        private string userName = "";
        private string password = "";
        private string ftpServer = "";
        private string errorMessage = "";
        private int portNumber = 0;

        public SFTP(string serverName, string userName, string password, int portNumber)
        {
            this.ftpServer = serverName;
            this.UserName = userName;
            this.Password = password;
            this.PortNumber = portNumber;
        }

        #region Properties
        public int PortNumber
        {
            get { return portNumber; }
            set { portNumber = value; }
        }

        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }

        public string FtpServer
        {
            get
            {
                return ftpServer;
            }
            set
            {
                ftpServer = value;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
            }
        }

        #endregion

        #region Methods

        private bool isPassiveMode()
        {
            bool passiveType = false;
            string settingValue = IAPL.Transport.Configuration.Config.GetAppSettingsValue("passivetype", "no");

            if (settingValue.ToLower().Trim().Equals("yes"))
            {
                passiveType = true;
            }
            return passiveType;
        }

        private Int32 getConnectionTimeout()
        {
            Int32 timeOut = 30000;
            string settingValue = IAPL.Transport.Configuration.Config.GetAppSettingsValue("ftptimeout", "30000");

            try
            {
                timeOut = Convert.ToInt32(settingValue);
            }
            catch
            {
                timeOut = 30000;
            }
            return timeOut;
        }

        public bool Connect()
        {
            bool success = true;
            this.ErrorMessage = "";
            try
            {

                //this.client = new Sftp();
                //this.client.Timeout = getConnectionTimeout();

                //if (PortNumber > 0)
                //{
                //    this.client.Connect(this.FtpServer, PortNumber);
                //}
                //else
                //    this.client.Connect(this.FtpServer);

                //SshPrivateKey privateKey = new SshPrivateKey("key_rsa.pem", this.Password);
                //this.client.Login(this.UserName, privateKey);

                //this.client.Login(this.UserName, this.Password);
                if (PortNumber > 0)
                {
                    this.client = new SftpClient(this.FtpServer, PortNumber, this.UserName, this.Password);
                }
                else
                {
                    this.client = new SftpClient(this.FtpServer, this.UserName, this.Password);
                }
                    this.client.Connect();
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "SFTP-Connect()|" + ex.Message.ToString();
            }
            return success;
        }

        public bool Disconnect()
        {
            bool success = false;
            this.ErrorMessage = "";
            try
            {
                this.client.Disconnect();
                this.client.Dispose();
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "SFTP-Disconnect()|" + ex.Message.ToString();
            }
            return success;
        }

        public string GetCurrentDirectory()
        {
            return this.client.WorkingDirectory;
        }

        public bool ChangeDirectory(string newDirectory)
        {
            bool success = true;
            this.ErrorMessage = "";
            try
            {
                this.client.ChangeDirectory(newDirectory);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "SFTP-ChangeDirectory()|" + ex.Message.ToString();
            }
            return success;
        }

        public bool Download(string srcFile, string desFile, int Delay)
        {
            bool success = true;
            this.ErrorMessage = "";
            try
            {
                int ToDelay = (60 * 1000) * Delay;
                System.Threading.Thread.Sleep(ToDelay);
                //long result = this.client.GetFile(srcFile.Trim(), desFile.Trim());
                using (var file = File.OpenWrite(desFile.Trim()))
                {
                    this.client.DownloadFile(srcFile.Trim(), file);
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "SFTP-Download()|" + ex.Message.ToString();
            }
            return success;
        }

        public bool Rename(string thisFrom, string thisTo)
        {
            bool success = true;

            try
            {
                this.client.RenameFile(thisFrom, thisTo);
            }
            catch (Exception ex)
            {
                success = false;
                this.errorMessage = "SFTP-Rename()|" + ex.Message.ToString();
            }

            return success;
        }

        public bool Upload(string srcFile, string desFile)
        {
            bool success = true;
            try
            {
                desFile = desFile.Replace("//", @"/");
                //long result = this.client.PutFile(srcFile, desFile); //"/ciba_folder/sftp00ss1.txt");
                using (var fileStream = new FileStream(srcFile, FileMode.Open))
                {
                    this.client.UploadFile(fileStream, desFile);
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "SFTP-Upload()|" + ex.Message.ToString();
            }

            return success;
        }

        public bool DeleteRemoteFile(string srcFile)
        {
            bool success = true;
            try
            {
                this.client.DeleteFile(srcFile);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "SFTP-DeleteRemoteFile()|" + ex.Message.ToString();
            }

            return success;
        }

        public string[] GetFileList()
        {
            //SftpItemCollection list = client.GetList();
            var files = client.ListDirectory(this.client.WorkingDirectory);
            string[] results = new string[files.Count()];
            int i = 0;

            foreach (var item in files)
            {
                results[i++] = item.Name;
            }

            return results;
        }

        public System.Collections.Hashtable GetFileList(string fileMask)
        {
            var files = client.ListDirectory(this.client.WorkingDirectory);
            System.Collections.Hashtable fileList = new System.Collections.Hashtable();
            int i = 0;

            if (fileMask == "*.*")
            {
                fileMask = "*";
            }

            if (fileMask.Contains(".*"))
            {
                fileMask = fileMask.Replace(".*", "*");
            }

            foreach (var s in files.Where(x => x.Name.Contains("." + fileMask.Split('.')[1])).Select(x => new { x.Name }))
            {

                i++;

                fileList.Add("file" + i.ToString(), s.Name);
            }


            return fileList;
        }

        #endregion

        // *******************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12043
        // Date: September 19, 2008

        // Checks if the file in the FTP server exist
        public bool FileExist(string FilePath, string ServerFolder)
        {
            bool Found = false;
            try
            {
                //SR#34056 Ccenriquez -- November 26, 2009
                if (!ServerFolder.EndsWith(@"/"))
                    ServerFolder += @"/";

                Found = client.Exists(ServerFolder + FilePath);
            }
            catch (Exception ex)
            {
                Found = false;
                this.ErrorMessage = "FTP-FileExists()|" + ex.Message.ToString();
            }
            return Found;
        }

        public long GetFileSize(string FilePath, string ServerFolder)
        {
            FileInfo fileInfo = new FileInfo(ServerFolder + FilePath);
            long BFileSize = 0;
            try
            { BFileSize = fileInfo.Length; }
            catch (Exception ex)
            { this.ErrorMessage = "FTP-FileExists()|" + ex.Message.ToString(); }
            return BFileSize;
        }

        // *******************************************************************

    }
}
