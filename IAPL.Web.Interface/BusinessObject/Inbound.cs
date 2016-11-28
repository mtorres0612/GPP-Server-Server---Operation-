using System;
using System.Data;
using System.Configuration;
using System.Collections.Generic;

namespace IAPL.Web.Interface.BusinessObject
{
    public class Inbound
    {
        private int _seconds;
        private string _name;

        public Inbound(int seconds, string name)
        {
            _seconds = seconds;
            _name = name;
        }

        public int Seconds
        {
            get { return _seconds; }
            set { _seconds = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }
}
