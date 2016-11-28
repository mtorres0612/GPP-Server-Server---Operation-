using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace IAPL.Transport.Util
{
    public class TextLogger
    {
        private static bool isDebug = true;
        private static bool isSettingDisplay = false;
        private static bool isErrorShowOnConsole = false;
        public enum messageType { 
            Normal = 0,
            Tab,
            Bulleted
        }

        #region properties

        public static bool IsSettingDisplay
        {
            set
            {
                isSettingDisplay = value;
            }
            get
            {
                return isSettingDisplay;
            }
        }

        public static bool IsDebug {
            set {
                isDebug = value;
            }
            get {
                return isDebug;
            }
        }
        public static bool IsErrorShowOnConsole {
            set
            {
                isErrorShowOnConsole = value;
            }
            get
            {
                return isErrorShowOnConsole;
            }
        }

        #endregion

        #region logsretention
        public static void logRetention() {
            reviewPath(getFilePath(1));
            reviewPath(getFilePath(2));
            reviewPath(getFilePath(3));
        }

        private static void reviewPath(string path) {
            string durationType = IAPL.Transport.Configuration.Config.GetAppSettingsValue("logdurationtype", "day").ToLower().Trim();
            string duration = IAPL.Transport.Configuration.Config.GetAppSettingsValue("logduration", "7").ToLower().Trim();

            //check files to delete
            string[] fileList = getFileList(path, "*.txt");

            foreach (string fileName in fileList) {
                if (oldFile(fileName, durationType, duration)) {
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch { }
                }
            }
        }

        private static DateTime getNumberOfDays(string durationType, string duration)
        {
            DateTime dateCutOff = DateTime.Now;

            try
            {
                int num = Convert.ToInt16(duration);

                switch (durationType.ToLower().Trim())
                {
                    case "month":
                        dateCutOff = dateCutOff.AddMonths(-num);
                        break;
                    case "week":
                        num = 7 * num;
                        dateCutOff = dateCutOff.AddDays(-num);
                        break;
                    case "day":
                        dateCutOff = dateCutOff.AddDays(-num);
                        break;
                }
            }
            catch { }

            return dateCutOff;
        }

        private static bool oldFile(string fileName, string durationType, string duration) {
            bool deleteFile = false;
            DateTime cutOffDate = getNumberOfDays(durationType, duration);

            DateTime fileModifiedDate = File.GetLastWriteTime(fileName);

            int numberOfDays = cutOffDate.CompareTo(fileModifiedDate);

            if (numberOfDays > 0)
                deleteFile = true;

            return deleteFile;
        }

        private static string[] getFileList(string filePath, string fileMask) {
            string[] fileList = new string[1];

            try
            {
                fileList = Directory.GetFiles(filePath, fileMask);
            }
            catch { 
            
            }


            return fileList;
        }
        #endregion

        private static string getFilePath(int optn) {
            //string keyName = "";
            //string defValue = "";
            string fDir = IAPL.Transport.Configuration.Config.GetAppSettingsValue("LogPath", @"c:\logs");

            if (!fDir.EndsWith(@"\"))
            {
                fDir += @"\";
            }

            switch (optn)
            {
                case 1:
                    //keyName = "log";
                    //defValue = "log";
                    fDir += @"log\";
                    break;
                case 2:
                    //keyName = "error";
                    //defValue = @"\error";
                    fDir += @"error\";
                    break;
                case 3:
                    //keyName = "error";
                    //defValue = @"\error";
                    fDir += @"setting\";
                    break;
                default:
                    //keyName = "info";
                    //defValue = "info";
                    fDir += @"info\";
                    break;
            }            

            if (!Directory.Exists(fDir))
            {
                Directory.CreateDirectory(fDir);
            }            

            return fDir;
        }

        private static string getFileName(int optn){
            string keyName = "";
            string defValue = "";            
            string fSuffix = DateTime.Now.ToString("MMddyy");

            switch (optn)
            {
                case 1:
                    keyName = "logprefix";
                    defValue = "log";
                    break;
                case 2:
                    keyName = "errorprefix";
                    defValue = @"\error";
                    break;
                case 3:
                    keyName = "settingprefix";
                    defValue = @"\setting";
                    break;
                default:
                    keyName = "infoprefix";
                    defValue = "info";
                    break;
            }

            defValue = IAPL.Transport.Configuration.Config.GetAppSettingsValue(keyName, defValue);
            if (defValue.Substring(0, 1).Equals(@"\"))
            {
                defValue = defValue.Substring(1, defValue.Length - 1);
            }

            string logPath = getFilePath(optn).Trim();
            if(logPath.EndsWith(@"\"))
                defValue = logPath + defValue + fSuffix + ".txt";
            else
                defValue = logPath + @"\" + defValue + fSuffix + ".txt";

            return defValue;
        }

        private static void write2File(string msgTitle, string msg, int optn) {
            try
            {
                using (StreamWriter sw = File.AppendText(getFileName(optn)))
                {                    
                    if (msgTitle.Trim().ToLower().Equals("tab"))
                    {
                        sw.WriteLine("                  " + msg);
                    }
                    else if (msgTitle.Trim().ToLower().Equals("bulleted"))
                    {
                        sw.Write(DateTime.Now.ToString("MM/dd/yy hh:mm:ss"));
                        sw.WriteLine(" " + msg);
                    }
                    else
                    {
                        sw.Write(DateTime.Now.ToString("MM/dd/yy hh:mm:ss"));
                        sw.WriteLine(" " + msgTitle + ": " + msg);
                    }
                }	
            }
            catch { 
            
            }
        }

        private static void showDebug(string msgTitle, string mesg)
        {
            if (IsDebug)
            {
                if (msgTitle.Trim().ToLower().Equals("tab"))
                {
                    System.Console.WriteLine("\t: {0}", mesg);
                }
                else if (msgTitle.Trim().ToLower().Equals("bulleted"))
                {
                    System.Console.WriteLine("\t-> {0}", mesg);
                }
                else
                {
                    System.Console.WriteLine(" {0}: {1}", msgTitle, mesg);
                }
            }
        }


        public static void Log(string msgTitle, string mesg) {
            write2File(msgTitle, mesg, 1);
            showDebug(msgTitle, mesg);
        }

        public static void Log(messageType mesgType, string msgTitle, string mesg)
        {
            switch (mesgType) { 
                case messageType.Normal:
                    write2File(msgTitle, mesg, 1);
                    showDebug(msgTitle, mesg);
                    break;
                case messageType.Tab:
                    write2File(messageType.Tab.ToString(), mesg, 1);
                    showDebug(messageType.Tab.ToString(), mesg);
                    break;
                case messageType.Bulleted:
                    write2File(messageType.Bulleted.ToString(), mesg, 1);
                    showDebug(messageType.Bulleted.ToString(), mesg);
                    break;
            }                        
        }

        public static string[] ParseError(string errorMesg) {
            string[] result = null;
            try
            {
                result = errorMesg.Split(new char[] { '|' });
            }
            catch {
                result = new string[1];
                result[0] = "";
            }
            return result;
        }

        public static void LogError(string msgTitle, string mesg) {
            write2File(msgTitle, mesg, 2);
            if(IsErrorShowOnConsole)
                showDebug(msgTitle, mesg);
        }

        public static void LogSetting(string msgTitle, string mesg) {
            if(IsSettingDisplay)
                write2File(msgTitle, mesg, 3);
        }
    }
}
