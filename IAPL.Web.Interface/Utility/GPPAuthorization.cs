using System;
using System.Configuration;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Xml;
using System.Data;

namespace IAPL.Web.Interface.Utility
{
    
	public class GPPAuthorization : IHttpModule
    {
        #region Default
        public GPPAuthorization()
		{
		}
		public void Dispose()
		{
        }
        #endregion

        public void Init(HttpApplication application)
		{
			application.AuthenticateRequest += new EventHandler(this.OnAuthenticateRequest);
			application.EndRequest += new EventHandler(this.OnEndRequest);
		}

		public void OnAuthenticateRequest(object source, EventArgs eventArgs)
		{
			HttpApplication app = (HttpApplication) source;

			string authStr = app.Request.Headers["Authorization"];

			if (authStr == null || authStr.Length == 0)
			{
				// No credentials; anonymous request
				return;
			}

			authStr = authStr.Trim();
			if (authStr.IndexOf("Basic",0) != 0)
			{
				// Don't understand this header...we'll pass it along and 
				// assume someone else will handle it
				return;
			}

			string encodedCredentials = authStr.Substring(6);

			byte[] decodedBytes = Convert.FromBase64String(encodedCredentials);
			string s = new ASCIIEncoding().GetString(decodedBytes);

			string[] userPass = s.Split(new char[] {':'});
			string username = userPass[0];
			string password = userPass[1];

			string[] roles;
			if (AuthenticateUser(app,username,password,out roles))
			{
				app.Context.User = new GenericPrincipal(new GenericIdentity(username, "Basic"),roles);
			}
			else
			{
				// Invalid credentials; deny access
				DenyAccess(app);
				return;
			}
		}

		public void OnEndRequest(object source, EventArgs eventArgs)
		{
			// We add the WWW-Authenticate header here, so if an authorization 
			// fails elsewhere than in this module, we can still request authentication 
			// from the client.

			HttpApplication app = (HttpApplication) source;
			if (app.Response.StatusCode == 401)
			{
                string realm = ConfigurationManager.AppSettings["BasicAuthenticationModule_Realm"];
				string val = String.Format("Basic Realm=\"{0}\"",realm);
				app.Response.AppendHeader("WWW-Authenticate",val);
			}
		}

		private void DenyAccess(HttpApplication app)
		{
			app.Response.StatusCode = 401;
			app.Response.StatusDescription = "Access Denied";

			// Write to response stream as well, to give user visual 
			// indication of error during development
			app.Response.Write("401 Access Denied");

			app.CompleteRequest();
		}

		protected virtual bool AuthenticateUser(HttpApplication app, string username, string password, out string[] roles)
        {
           
            //Utility.Tools.BasicLogs("Authentication Started for IP Address: " + _sb.ToString());
            //Utility.Tools.Log("Authentication Started for IP Address: " + _sb.ToString());//+ app.Request.UserHostAddress);
            
            roles = null;
            bool _ret = false;
            _ret = DataAccess.AuthorizationDB.GetInstance().ValidateUser(ConfigurationManager.AppSettings["ConnectionString"], username, password);

            if (_ret == false)
            {
                Utility.Tools.ProcessLogs("AuthenticateUser",false, "Login Failed for user:" + username + " With IP Address: " + app.Request.UserHostAddress, "Header:  " + app.Request.Headers);
                //Utility.Tools.BasicLogs("Authentication successful for user: " + username);
            }
            
            return _ret;

        }

        
	}
}
