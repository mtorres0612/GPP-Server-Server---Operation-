using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace IAPL.Transport.Operation
{
    class Network
    {
        private string userName = "";
        private string password = "";
        private string ftpServer = "";
        private string errorMessage = "";

        public Network() { 
        
        }

        #region Properties
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

        public bool Connect()
        {
            //this.client = new Ftp(); //new FTP(this.FtpServer, this.UserName, this.Password);            
            bool success = true;
            //this.ErrorMessage = "";
            try
            {
                
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = ex.Message.ToString();
            }
            return success;
        }

        public bool Disconnect()
        {
            bool success = false;
         
            return success;
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }

        public bool ChangeDirectory(string newDirectory)
        {
            bool success = true;
            try
            {
                Directory.SetCurrentDirectory(newDirectory);
            }
            catch {
                success = false;
            }

            return success;
        }

        public bool Download(string srcFile, string desFile)
        {
            bool success = true;
         
            return success;
        }

        public bool Upload(string srcFile, string desFile)
        {
            bool success = true;
           
            return success;
        }

        public bool DeleteRemoteFile(string srcFile)
        {
            bool success = true;
            this.ErrorMessage = "";
            try
            {
                using (StreamWriter sw = File.CreateText(srcFile)) { }

                File.Delete(srcFile);
            }
            catch (Exception ex)
            {
                success = false;
                this.ErrorMessage = ex.Message.ToString();
            }

            return success;
        }

        public System.Collections.Hashtable GetFileList(string fPath, string fileMask)
        {
            System.Collections.Hashtable fileList = new System.Collections.Hashtable();

            int i = 0;
            this.ErrorMessage = "";
            try
            {
                
                foreach (string s in System.IO.Directory.GetFiles(fPath, fileMask))
                {
                    i++;

                    fileList.Add("file" + i.ToString(), s);
                }
            }
            catch (Exception ex) {
                this.ErrorMessage = "Network-GetFileList()|" + ex.Message.ToString();
            }

            return fileList;
        }


        #endregion

    }
}
