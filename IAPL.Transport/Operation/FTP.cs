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
using System.Collections;
//using Rebex.Net;
using System.Net.FtpClient;
using System.Net;
using System.IO;
using System.Linq;

namespace IAPL.Transport.Operation
{
    class FTP
    {
        private FtpClient client = null;
        private string userName = "";
        private string password = "";
        private string ftpServer = "";
        private string errorMessage = "";
        private int portNumber = 0;

        public FTP(string serverName, string userName, string password, int portNumber)
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
                this.client = new FtpClient();
                this.client.ConnectTimeout = getConnectionTimeout();
                this.client.Host = this.FtpServer;
                this.client.Credentials = new NetworkCredential(this.UserName, this.Password);

                this.client.Connect();
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "FTP-Connect()|" + ex.Message.ToString();
            }
            return success;
        }

        public bool Rename(string thisFrom, string thisTo)
        {
            bool success = true;

            try
            {
                this.client.Rename(thisFrom, thisTo);
            }
            catch (Exception ex)
            {
                success = false;
                this.errorMessage = "FTP-Rename()|" + ex.Message.ToString();
            }

            return success;
        }

        public bool Disconnect()
        {
            bool success = false;
            try
            {
                this.client.Disconnect();
                success = true;
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "FTP-Disconnect()|" + ex.Message.ToString();
            }
            return success;
        }

        public string GetCurrentDirectory()
        {
            return this.client.GetWorkingDirectory();
        }

        public void ChangeDirectory(string newDirectory)
        {
            try
            {
                this.client.SetWorkingDirectory(newDirectory);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "FTP-ChangeDirectory()|" + ex.Message.ToString();
            }
        }

        public bool Download(string srcFile, string desFile, int Delay)
        {
            bool success = true;
            this.ErrorMessage = "";
            try
            {
                using (var ftpStream = this.client.OpenRead(srcFile))
                using (var fileStream = File.Create(desFile, (int)ftpStream.Length))
                {
                    var buffer = new byte[8 * 1024];
                    int count;
                    while ((count = ftpStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, count);
                    }

                }
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "FTP-Download()|" + ex.Message.ToString();
            }
            return success;
        }

        public bool Upload(string srcFile, string desFile)
        {
            bool success = true;
            try
            {
                //using (var fileStream = File.OpenRead(srcFile))
                //{
                //    using (var ftpStream = this.client.OpenWrite(string.Format("{0}/{1}", Path.GetDirectoryName(desFile).Replace("\\", "/"), Path.GetFileName(srcFile))))
                //    {
                //        fileStream.CopyTo(ftpStream);
                //    }
                //}
                using (var fileStream = File.OpenRead(srcFile))
                {
                    using (var ftpStream = this.client.OpenWrite(string.Format("{0}/{1}", Path.GetDirectoryName(desFile).Replace("\\", "/"), Path.GetFileName(srcFile))))
                    {
                        var buffer = new byte[1024 * 1024];
                        int count;
                        while ((count = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ftpStream.Write(buffer, 0, count);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "FTP-Upload()|" + ex.Message.ToString();
            }
            return success;
        }

        public bool DeleteRemoteFile(string srcFile)
        {
            bool success = true;
            this.ErrorMessage = "";
            try
            {
                this.client.DeleteFile(srcFile);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = "FTP-DeleteRemoteFile()|" + ex.Message.ToString();
            }

            return success;
        }

        public System.Collections.Hashtable GetFileList(string fileMask)
        {
            //ArrayList aList = new ArrayList();
            //FtpList list = client.GetList();
            System.Collections.Hashtable fileList = new Hashtable();

            //string[] results = new string[list.Count];
            int i = 0;
            //foreach (FtpItem item in list)
            if (fileMask == "*.*")
            {
                fileMask = "*";
            }

            if (fileMask.Contains(".*"))
            {
                fileMask = fileMask.Replace(".*", "*");
            }

            if (fileMask.Contains("*."))
            {
                fileMask = fileMask.Replace("*.", "*");
            }

            foreach (var ftpListItem in this.client.GetListing().Where(ftpListItem => string.Equals(Path.GetExtension(ftpListItem.Name), "." + fileMask.Split('*')[1], StringComparison.CurrentCultureIgnoreCase)))
            {
                i++;

                fileList.Add("file" + i.ToString(), ftpListItem.Name);
            }

            return fileList;
        }

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

                Found = this.client.FileExists(ServerFolder + FilePath);
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
            long BFileSize = 0;
            try
            { BFileSize = this.client.GetFileSize(ServerFolder + FilePath); }
            catch (Exception ex)
            { this.ErrorMessage = "FTP-FileExists()|" + ex.Message.ToString(); }
            return BFileSize;
        }






        // *******************************************************************

        #endregion
    }
}
