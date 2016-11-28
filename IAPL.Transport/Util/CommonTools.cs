using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace IAPL.Transport.Util
{
    class CommonTools
    {

        public static string GetCurrentDirectory() {
            return Directory.GetCurrentDirectory();
        }

        public static bool DirectoryExist(string dName)
        {
            try
            {
                if (!Directory.Exists(dName))
                {
                    Directory.CreateDirectory(dName);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ValueToString(Object obj) {
            if (obj == null) {
                return "";
            }
            
            return obj.ToString();
        }

        public static string CreateGuidFolder(string DefaultFolder)
        {
            string GuidFolder = DefaultFolder + @"\" + Guid.NewGuid().ToString();
            if (!IAPL.Transport.Util.CommonTools.DirectoryExist(GuidFolder))
            { System.IO.Directory.CreateDirectory(GuidFolder); }
            return GuidFolder;
        }
    }
}
