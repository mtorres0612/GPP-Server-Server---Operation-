using System;
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport.Configuration
{
    public class Config
    {

        public static string GetAppSettingsValue(string key, string defValue)
        {
            string strRes = "";
            try
            {
                strRes = System.Configuration.ConfigurationSettings.AppSettings.Get(key);
                //.ConfigurationSettings.AppSettings.Get(key);
                if (strRes == null)
                {
                    return defValue;
                }
            }
            catch
            {
                return defValue;
            }

            //if (strRes.Length < 1)
            //{
            //    return defValue;
            //}
            return strRes;
        }

        public static string GetConnectionSettings(string key, string defValue)
        {
            string strRes = "";
            try
            {
                //ConnectionStringSettingsCollection connectionStrings = ConfigurationManager.ConnectionStrings;
                //strRes = System.Configuration.ConfigurationManager.AppSetting[key];
                strRes = System.Configuration.ConfigurationSettings.AppSettings.Get(key);
                if (strRes == null)
                {
                    return defValue;
                }
            }
            catch
            {
                return defValue;
            }

            //if (strRes.Length < 1)
            //{
            //    return defValue;
            //}
            return strRes;
        }
    }
}
