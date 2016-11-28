using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.IO;
using System.Text;
using IAPL.Web.Interface.Utility;
namespace IAPL.Web.Interface
{
    public partial class _Default : System.Web.UI.Page
    {
        string trdp_Code = string.Empty;
            
        protected void Page_Load(object sender, EventArgs e)
        {

            Receive();
            //CreateDirectory(DataAccess.AuthorizationDB.GetInstance().GetTrdpCode(ConfigurationManager.AppSettings["ConnectionString"], Context.User.Identity.Name));
        }

        private string CreateDirectory()//string trdpCode)
        {
            try
            {
                string _ret = string.Empty;
                if (!Tools.DirExists(TrdpCode))
                {
                    Tools.CreateDirectory(TrdpCode);
                }
               
            }
            catch (Exception ex)
            {
                Utility.Tools.ProcessLogs("CreateDirectory", false, "Error Createing Directory", ex.Message);
                //Utility.Tools.PrincipalLogs(TrdpCode, "Error on CreateDirectory(): " + ex.Message + "-" + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress);
            }
            return ConfigurationManager.AppSettings["Inbound_Folder"] + TrdpCode;
            
        }
        private void Receive()
        {
            XmlTextWriter _default = null;
            try
            {
                DataTable dt = DataAccess.AuthorizationDB.GetInstance().GetTrdpCode(ConfigurationManager.AppSettings["ConnectionString"], Context.User.Identity.Name);
                //Utility.Tools.Log("Managing Directories for user: " + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress );
                this.TrdpCode = dt.Rows[0][0].ToString();
                //string _trdpCode = 
                int _XmlNodeIdentity = 0;

                if (DBNull.Value != dt.Rows[0][1])
                {
                    _XmlNodeIdentity = Convert.ToInt32(dt.Rows[0][1]);
                }

                string _baseDirectory = CreateDirectory();
                Utility.Tools.ProcessLogs("Receive", true, "managing directories", "None");
                //Utility.Tools.PrincipalLogs(TrdpCode, "Managing Directories for user: " + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress);

                Page.Response.ContentType = "text/xml";
                // Read XML posted via HTTP
                //StreamReader reader = new StreamReader(Page.Request.InputStream);
                //String xmlData = reader.ReadToEnd();

                string _identifier = Guid.NewGuid().ToString();

                string _path = _baseDirectory + "\\" + _identifier + ".tmp";

                XmlDocument _xmlText = new XmlDocument();
                _xmlText.Load(Page.Request.InputStream);

                _default = new XmlTextWriter(_path, Encoding.ASCII);//DesFile + "Default.xml", Encoding.Default);//_request.GetRequestStream(), Encoding.UTF8);
                _default.Formatting = Formatting.Indented;
                _xmlText.WriteTo(_default);
                _default.Close();

                //tw = new StreamWriter(_path);
                
                //Write the XML
                //tw.WriteLine(xmlData);

                //RENAME THE FILE
                //close the stream
                //tw.Close();


                Utility.Tools.ProcessLogs("Receive", true, "Writing File", "None");
                //Utility.Tools.PrincipalLogs(TrdpCode, "Writing File for user: " + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress);

                string _newFileName = _baseDirectory + "\\" + HeaderName(_path, _XmlNodeIdentity) + "-" + _identifier + ".xml";

                //Rename it
                RenameFile(_path, _newFileName);

                Utility.Tools.ProcessLogs("Receive", true, "transaction Complete", "None");
                //Utility.Tools.PrincipalLogs(TrdpCode, "Transaction complete for user: " + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress);

            }
            catch (Exception ex)
            {
                Utility.Tools.ProcessLogs("Receive", false, "Error on Receive", ex.Message);
                //Utility.Tools.PrincipalLogs(TrdpCode, "Error on Receive(): " + ex.Message + "-" + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress);
            }
            finally 
            {
                if (_default != null)
                {
                    _default.Close();
                }
               // tw.Close();
                //_default.Close();
                //tw.Dispose();
        
            }
        }

        private void RenameFile(string oldName, string newName)
        {
            FileInfo fi = new FileInfo(oldName);
            try
            {
                

                if (fi.Exists)
                {
                    fi.MoveTo(newName);

                }
            }
            catch (Exception ex)
            {
                Utility.Tools.ProcessLogs("RenameFile", false, "Error on Renaming", ex.Message);
                //Utility.Tools.PrincipalLogs(TrdpCode, "Error on RenameFile(): " + ex.Message + "-" + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress);
            }
            

        }
        private string HeaderName(string fileName, int xmlIdentityNode)
        {

            XmlTextReader textReader = new XmlTextReader(fileName);

            textReader.Read();
                      
            string _err = "";
            int count = 0;
            while (textReader.Read())
            {
                textReader.MoveToElement();
                if (count == xmlIdentityNode)
                {
                    _err = textReader.Name;
                }
                count++;
            }
            textReader.Close();

            if (_err == "")
            {
                _err = "NoHeader";
                Utility.Tools.ProcessLogs("XML Node", false, "No title on the given Node", "None");
                //Utility.Tools.PrincipalLogs(TrdpCode, "No Header found on the given XML Node for this transaction: " + Context.User.Identity.Name + " with IP address: " + Context.Request.UserHostAddress);
            }
            return _err;
           
 
        }

        private string TrdpCode
        {
            get {return trdp_Code;}
            set{trdp_Code = value;}

        }
    }
}
