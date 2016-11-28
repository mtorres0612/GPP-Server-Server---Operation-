using System;
using System.Data;
using System.Configuration;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.IO;
namespace IAPL.Web.Interface.Utility
{
    public class Tools
    {
        public Tools()
        {}
        
        public static bool DirExists(string sDirName)
        {
            try
            {
                string _baseDirectory = System.Configuration.ConfigurationManager.AppSettings["Inbound_Folder"].ToString();
                return (System.IO.Directory.Exists(_baseDirectory + sDirName));    
            }
            catch (Exception)
            {
                return (false);                                 //Exception occured, return False
            }
        }

        public static bool FileExists(string sPathName)
        {
            try
            {
               string _baseDirectory = System.Configuration.ConfigurationManager.AppSettings["Inbound_Folder"].ToString();
                return (System.IO.Directory.Exists(_baseDirectory+sPathName));  //Exception for folder
            }
            catch (Exception)
            {
                return (false);                                   //Error occured, return False
            }
        }

        public static void CreateDirectory(string sDirectory)
        {
            try
            {
                string _baseDirectory = System.Configuration.ConfigurationManager.AppSettings["Inbound_Folder"].ToString();
                System.IO.Directory.CreateDirectory(_baseDirectory + sDirectory);
               
            }
            catch (Exception ex)
            {
                ProcessLogs("CreateDirectory", false, "Error Createing Directory", ex.Message);
            }
        }

        public static string Encrypt(string ToEncrypt)
        {
            string password;
            TripleDESCryptoServiceProvider des;
            MD5CryptoServiceProvider hashmd5;
            byte[] pwdhash, buff;

            password = ConfigurationManager.AppSettings["PrivateKey"];

            hashmd5 = new MD5CryptoServiceProvider();
            pwdhash = hashmd5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(password));
            hashmd5 = null;


            des = new TripleDESCryptoServiceProvider();


            des.Key = pwdhash;
            des.Mode = CipherMode.ECB;
            buff = System.Text.ASCIIEncoding.ASCII.GetBytes(ToEncrypt);


            return Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(buff, 0, buff.Length));
        }
        #region Old
        //public static void BasicLogs(String logMessage)
        //{
        //    if (ConfigurationManager.AppSettings["TurnOffLogs"].ToLower() == "true")
        //    {
        //        return;
        //    }


        //    if (!Tools.DirExists(ConfigurationManager.AppSettings["BasicLogs"]))
        //    {
        //        Tools.CreateDirectory(ConfigurationManager.AppSettings["BasicLogs"]);
        //    }
        //    string currentDate = DateTime.Now.ToString("MM-dd-yyyy"); 
        //    string _fileName = "GeneralLogs-"+currentDate+".logs";
        //    //FileStream fs = new FileStream("c:\\" + currentDate + ".txt", System.IO.FileMode.OpenOrCreate);
        //       string _path = ConfigurationManager.AppSettings["BasicLogs"] + _fileName;
        //    if (!File.Exists(_path))
        //    {
        //        File.Create(_path);
        //    }

        //    int count = 0;
        //    bool success = false;
        //    while (count < 5 && !success)
        //    {
              
        //        try
        //        {
        //            // Open with the least amount of locking possible
        //           StreamWriter w = File.AppendText(_path);

        //            w.Write("\r\nLog Entry : ");
        //            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
        //            w.WriteLine("{0}", logMessage);
        //            w.WriteLine("-------------------------------");
        //            // Update the underlying file.
        //            w.Flush();
        //            w.Close();
        //            success = true;
        //        }
        //        catch (IOException)
        //        {
        //            System.Threading.Thread.Sleep(50); // Give a little while to free up file
        //        }
               
        //        ++count;

        //    }
           

        //}

        //public static void PrincipalLogs(string principal, string logMessage)
        //{
        //    if (ConfigurationManager.AppSettings["TurnOffLogs"].ToLower() == "true")
        //    {
        //        return;
        //    }

        //    if (!Tools.DirExists(principal + "\\Logs"))
        //    {
        //        Tools.CreateDirectory(principal + "\\Logs");
        //    }
        //    string currentDate = DateTime.Now.ToString("MM-dd-yyyy");
        //    string _fileName = principal+ "-" + currentDate + ".logs";
        //    //FileStream fs = new FileStream("c:\\" + currentDate + ".txt", System.IO.FileMode.OpenOrCreate);
        //    string _path = ConfigurationManager.AppSettings["Inbound_Folder"] + principal + "\\logs\\" + _fileName;
        //    if (!File.Exists(_path))
        //    {
        //        File.Create(_path);
        //    }

        //    int count = 0;
        //    bool success = false;
          
        //    while (count < 5 && !success)
        //    {
            
        //        try
        //        {
        //            // Open with the least amount of locking possible
        //           StreamWriter w = File.AppendText(_path);

        //            w.Write("\r\nLog Entry : ");
        //            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
        //            w.WriteLine("{0}", logMessage);
        //            w.WriteLine("-------------------------------");
        //            // Update the underlying file.
        //            w.Flush();
        //            w.Close();
        //            success = true;
        //        }
        //        catch (IOException)
        //        {
        //            System.Threading.Thread.Sleep(50); // Give a little while to free up file
        //        }
        //        ++count;
        //    }
           
        //}
#endregion
        public static void ProcessLogs(string Method, bool isSuccess, string description, string technicalErr)
        {
            if (ConfigurationManager.AppSettings["TurnOffLogs"].ToLower() == "true")
            {
                return;
            }
            DataAccess.AuthorizationDB.GetInstance().ProcessLog(ConfigurationManager.AppSettings["ConnectionString"], Method, isSuccess, description, technicalErr);
        }

    }

}
