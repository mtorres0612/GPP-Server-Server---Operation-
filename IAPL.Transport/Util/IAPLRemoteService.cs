using System;
using System.Collections.Generic;
using System.Text;

using IAPLRemoteInterface;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace IAPL.Transport.Util
{
    public class IAPLRemoteService : MarshalByRefObject, IIAPLRemoteService
    {
        public IAPLRemoteService() { 
        
        }

        private delegate int addDelegate(object o);

        public bool Connect(string applicationName)
        {
            //ApplicationMonitor o = new ApplicationMonitor();
            //o.ApplicationName = applicationName;
            //o.LastRunDate = DateTime.Now;
            //o.IsConnected = true;

            //System.Console.WriteLine("Message from Client: {0}", applicationName);
            IAPL.Transport.Util.TextLogger.Log("Message from Client", applicationName);

            return true;
        }
    }
}
