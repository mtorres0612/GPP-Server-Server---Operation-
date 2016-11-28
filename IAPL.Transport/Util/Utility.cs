using System;
using System.Text;          // Added by Alrazen Estrella | ISG12128 | 09/04/08    
using System.Security.Cryptography;
using System.IO;            // Added by Alrazen Estrella | ISG12152 | 07/07/08
using System.Collections;
using System.Collections.Generic;

namespace IAPL.Transport.Util
{
    /// <summary>
    /// Utility Class for global static functions like encryption/decryption, textwriters and loggers
    /// </summary>
    /// <remarks>Utility Class</remarks>
	public class Utility
	{
        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Constructor logic</remarks>
        public Utility()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static string ResourceMap
		{
			get
			{
				return "/scripts/";
			}
		}

        private static string getPrivateKey(){
            return IAPL.Transport.Configuration.Config.GetAppSettingsValue("privatekey", "ProjectTitle");
            //return "ProjectTitle";
        }

		public static string GetResourceScriptTags(string scriptFileName)
		{
			return string.Format("<script language='javascript' src='{0}{1}'></script>", ResourceMap, scriptFileName);
		}

		public static string Encrypt(string ToEncrypt)
		{
			string                         password;
			TripleDESCryptoServiceProvider des;
			MD5CryptoServiceProvider       hashmd5;
			byte[]                         pwdhash, buff;

			//"Project Title" = Private key
			//password = "ProjectTitle";
            password = getPrivateKey();
            
			hashmd5 = new MD5CryptoServiceProvider();
			pwdhash = hashmd5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(password));
			hashmd5 = null;

			
			des = new TripleDESCryptoServiceProvider();

			
			des.Key = pwdhash;
			des.Mode = CipherMode.ECB;
			buff =System.Text.ASCIIEncoding.ASCII.GetBytes(ToEncrypt);
			

			return Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(buff, 0, buff.Length));
		}

		public static string Decrypt(string ToDecrypt)
		{
			byte[] pwdhash, buff;
			MD5CryptoServiceProvider hashmd5= new MD5CryptoServiceProvider();
			//"Project Title" = Private key
            string password = getPrivateKey();//"ProjectTitle";

			buff = Convert.FromBase64String(ToDecrypt);
			pwdhash = hashmd5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(password));

			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
			des.Key = pwdhash;
			des.Mode = CipherMode.ECB;
		
            return System.Text.ASCIIEncoding.ASCII.GetString(des.CreateDecryptor().TransformFinalBlock(buff, 0, buff.Length));		
		}

		public static string ConvertStringToQuery(string Sentence)
		{
			return Sentence.Replace("'", "''");
		}

        public static bool PurgeFiles(string path, string retention)
        {
            DateTime date = DateTime.Now;

            try
            {
                if ((IAPL.Transport.Util.CommonTools.ValueToString((Object)path) == string.Empty) ||
                    (Convert.ToInt32(IAPL.Transport.Util.CommonTools.ValueToString((Object)retention)) == 0))
                    return false;

                foreach (string file in System.IO.Directory.GetFiles(path))
                {
                    DateTime createtime = System.IO.File.GetLastWriteTime(file);
                    if (createtime.AddMonths(Convert.ToInt32(retention)) < date)
                    {
                        System.IO.File.Delete(file);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string CreateZIP(string zipPathFileName, string sourcePath, System.Collections.Hashtable fileList, string password)
        {
            Resco.IO.Zip.ZipArchive archive = null;

            try
            {
                archive = new Resco.IO.Zip.ZipArchive(zipPathFileName, Resco.IO.Zip.ZipArchiveMode.Create, System.IO.FileShare.None);
                archive.AutoUpdate = true;

                foreach (System.Collections.DictionaryEntry file in fileList)
                {
                    // **********************************************
                    // Developer: Alrazen Estrella
                    // Project: ISG12152
                    // Date: July 21, 2008

                    // Old Code
                    //archive.Add(sourcePath + @"\" + file.Value.ToString(), @"\", password, true, null);


                    // New Code
                    archive.Add(file.Value.ToString(), zipPathFileName, password, true, null);

                    // **********************************************                
                }
                return zipPathFileName;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                archive.Close();
            }
        }

        // ***********************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12152
        // Date: July 21, 2008

        // Create Zip file
        public static string ZipCreate(string zipSourcePath, string zipDestination, string password)
        {
            Resco.IO.Zip.ZipArchive archive = null;

            try
            {
                archive = new Resco.IO.Zip.ZipArchive(zipDestination, Resco.IO.Zip.ZipArchiveMode.Create, System.IO.FileShare.None);
                archive.AutoUpdate = true;
                archive.Add(zipSourcePath, @"\", password, true, null);
                return zipDestination;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                archive.Close();
            }
        }

        // Zip all the files in a folder
        public static string ZIPAll(string zipFileName, string sourcePath, string destPath, string password, out string ZipName)
        {
            Resco.IO.Zip.ZipArchive archive = null;
            ZipName = "";
            try
            {
                string destFilePath = "";
                string fileMask = "*.*";

                // If no given Zip filename, then get the name of the first file
                IAPL.Transport.Transactions.ServerDetails SrvrDetails = new IAPL.Transport.Transactions.ServerDetails();
                if (zipFileName.Equals(""))
                {
                    ZipName = SrvrDetails.getFileNameOnly(System.IO.Directory.GetFiles(sourcePath, fileMask).GetValue(0).ToString());
                    ZipName = ZipName.Substring(0, ZipName.Length - 3) + "zip";
                    destFilePath = destPath + @"\" + ZipName;
                }
                else
                {
                    destFilePath = destPath + @"\" + zipFileName;
                    ZipName = destFilePath;
                }

                archive = new Resco.IO.Zip.ZipArchive(destFilePath, Resco.IO.Zip.ZipArchiveMode.Create, System.IO.FileShare.None);
                archive.AutoUpdate = true;
                archive.Add(sourcePath, @"\", password, true, null);
                return destFilePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                archive.Close();
            }
        }

        // Extracts all the files in the zip
        public static string ExtractAllFilesInZip(string SourceZipFilePath, string DestinationFolderPath)
        {
            try
            {
                Resco.IO.Zip.ZipArchive archive = Resco.IO.Zip.ZipArchive.Open(SourceZipFilePath);
                archive.Extract(@"\", DestinationFolderPath, null);
                archive.Close();
                GetFilesOnlyInExtractedZip(DestinationFolderPath);
                return DestinationFolderPath;
            }
            catch (Exception ex)
            {
                throw ex; 
            }
        }

        // Picks up files extracted from zip and move it to root folder
        public static bool GetFilesOnlyInExtractedZip(string SourcePath)
        {
            bool success = false;
            try
            {
                IAPL.Transport.Transactions.ServerDetails SrvrDetails = new IAPL.Transport.Transactions.ServerDetails();

                int FileExist = System.IO.Directory.GetFiles(SourcePath).Length;
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(SourcePath);
                if (FileExist.Equals(0))
                {
                    foreach (System.IO.DirectoryInfo g in dir.GetDirectories())
                    {
                        string FolderInZip = SourcePath + @"\" + g.ToString();
                        foreach (string file in System.IO.Directory.GetFiles(FolderInZip))
                        {
                            string FileInsideFolderInZip = file.ToString();
                            string DestinationFilePath = SourcePath + @"\" + SrvrDetails.getFileNameOnly(file.ToString());
                            File.Copy(FileInsideFolderInZip, DestinationFilePath);
                            File.Delete(FileInsideFolderInZip);
                        }
                        if (!FolderInZip.Equals(""))
                        { System.IO.Directory.Delete(FolderInZip, true); }
                    }
                }
                success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return success;
        }

        // Update the Convention Counter in the database
        public static bool UpdateFilenameConventionCounter(string MsgCode, string MsgCounter)
        {
            bool success = true;
            try
            {
                IAPL.Transport.Data.DbTransaction db = new IAPL.Transport.Data.DbTransaction();

                System.Collections.Hashtable fldList = new System.Collections.Hashtable();
                fldList = new System.Collections.Hashtable();
                fldList.Add("@MsgCode", MsgCode);
                fldList.Add("@MsgCounter", MsgCounter);
                db.UpdateMessageCounter(fldList);
            }
            catch
            {
                success = false;
            }
            return success; 
        }

        // ***********************************************************************

        // ***********************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12128
        // Date: September 4, 2008

        // Convert Files according to File Format (CodePage)
        public static bool FileConvert(string SourceFilePath, string DestinationFilePath, int SourceCodePage, int DestinationCodePage)
        {
            bool success = true;
            try
            {
                StreamReader INStream = new StreamReader(SourceFilePath, Encoding.GetEncoding(SourceCodePage));
                StreamWriter OUTStream = new StreamWriter(DestinationFilePath, false, Encoding.GetEncoding(DestinationCodePage));

                char[] chBuffer = new char[4096];
                int intCount;

                while ((intCount = INStream.Read(chBuffer, 0, 4096)) > 0)
                {   OUTStream.Write(chBuffer, 0, intCount);     }

                INStream.Close();
                OUTStream.Close();
            }
            catch
            {   
                success = false;
                
            }
            return success;
        }

        // ***********************************************************************


        // ***********************************************************************
        // Developer: Alrazen Estrella
        // Project: ISG12043
        // Date: September 24, 2008

        // Check Zip file if valid
        public static bool ValidateZipFile(string SourceZipFilePath)
        {
            bool success = false;
            try
            {
                Resco.IO.Zip.ZipArchive archive = Resco.IO.Zip.ZipArchive.Open(SourceZipFilePath);

                // Get all files in the zip
                foreach (string g in archive.GetAllEntries())
                {
                    success = true;
                    break;
                }
                archive.Close();
            }
            catch (Resco.IO.Zip.ArchiveCorruptedException)
            { success = false; }
            catch
            { 
                success = false;
                //this.ErrorMessage = "FileConvert()|" + ex.Message.ToString();
            }
            return success;
        }

        // Get File size
        public static long GetFileSize_Local(string SourceFilePath)
        {
            FileInfo FI = new FileInfo(SourceFilePath);
            return FI.Length;
        }

        // Convert Byte size to Kilobyte
        public static double ConvertByteToKB(long ByteValue)
        {
            double ConvertionValue = 1;
            if (ByteValue > 1024)
                ConvertionValue = ByteValue / 1024;
            return ConvertionValue; 
        }

        public static string[] ParseString(string Phrase)
        {
            string[] stringArray = Phrase.Split(new char[] { '|' }, StringSplitOptions.None);
            //int length = stringArray.Length;
            return stringArray;
        }

        // Combine 2 Hashtables for output
        /// <summary>
        /// Combines Hastables
        /// </summary>
        public static System.Collections.Hashtable CombineHashtables(System.Collections.Hashtable HashTable1,
                                                                     System.Collections.Hashtable HashTable2)
        {
            System.Collections.Hashtable HashTablesContent = new System.Collections.Hashtable();
            int Ctr = 0;
            foreach (System.Collections.DictionaryEntry file1 in (System.Collections.Hashtable)HashTable1)
            {
                Ctr++;
                HashTablesContent.Add("file" + Ctr, file1.Value);
            }

            foreach (System.Collections.DictionaryEntry file2 in (System.Collections.Hashtable)HashTable2)
            {
                Ctr++;
                HashTablesContent.Add("file" + Ctr, file2.Value);
            }

            return HashTablesContent;
        }

        // ***********************************************************************



        public static Hashtable SortedHashTable(Hashtable ht, string identifier)
        {
            Hashtable _ret = new Hashtable();
            try
            {
                List<string> _list = new List<string>();
                for (int i = 1; i < ht.Count + 1; i++)
                {
                    _list.Add(ht[identifier + i].ToString());
                }

                _list.Sort();
             

                for (int i = 0; i < _list.Count; i++)
                {
                    _ret.Add(identifier + i, _list[i].ToString());
                }
                return _ret;
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                return ht;
            }
            
        }

        public static Hashtable SortedHashTable(Hashtable ht)
        {  
            Hashtable _ret = new Hashtable();
            Dictionary<string, object> _out = new Dictionary<string, object>();
            foreach (DictionaryEntry child in ht)
            {
                _out.Add((string)child.Key, child.Value);
            }

            List<string> _sorter = new List<string>(_out.Keys);
            _sorter.Sort();

            _sorter.ForEach(delegate(string _key)
            {
                _ret.Add(_sorter[0].ToString(), _out.Values.ToString());
            });
            

                
            
//------//

            

                return _ret;
       
            


          
        }

        public static void StringToFile(String stringToWrite, string Path)
        {

           // string _path = @"C:\temp\geek\output.html";//ConfigurationManager.AppSettings["BasicLogs"] + _fileName;
           try
            {
                // Open with the least amount of locking possible
                StreamWriter w = File.AppendText(Path);

                w.Write(stringToWrite);
                w.Flush();
                w.Close();

            }
            catch (Exception ex)
            {
                string a = ex.Message;
                //MessageBox.Show(ex.Message);//System.Threading.Thread.Sleep(50); // Give a little while to free up file
            }




        }

        //Method to parse Text into HTML
        public static string parseTextToHTML(string html)
        {



            //StringBuilder sc = new StringBuilder();
            System.Collections.Specialized.StringCollection sc = new System.Collections.Specialized.StringCollection();
            // get rid of unnecessary tag spans (comments and title)
            sc.Add(@"<!--(\w|\W)+?-->");
            sc.Add(@"<title>(\w|\W)+?</title>");
            // Get rid of classes and styles
            sc.Add(@"\s?class=\w+");
            sc.Add(@"\s+style='[^']+'");
            // Get rid of unnecessary tags
            sc.Add(
            @"<(meta|link|/?o:|/?style|/?div|/?st\d|/?head|/?html|body|/?body|/?span|!\[)[^>]*?>");
            // Get rid of empty paragraph tags
            sc.Add(@"(<[^>]+>)+&nbsp;(</\w+>)+");
            // remove bizarre v: element attached to <img> tag
            sc.Add(@"\s+v:\w+=""[^""]+""");
            // remove extra lines
            sc.Add(@"(\r\n){2,}");
            foreach (string s in sc)
            {
                html = System.Text.RegularExpressions.Regex.Replace(html, s, "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            return html;

        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);

                //   System.IO.File.Delete(DumpFolder + @"\" + FilenameToCopy);
            }

        }

        public static void DeleteGUIDDirectory(string path)
        {
            if (IAPL.Transport.Util.CommonTools.DirectoryExist(path))
            { System.IO.Directory.Delete(path, true); }
        }
	}
}
