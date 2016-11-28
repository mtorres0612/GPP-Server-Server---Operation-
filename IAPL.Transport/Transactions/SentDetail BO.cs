using System;
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport.Transactions
{
    class SentDetail_BO
    {
        public SentDetail_BO()
        {}
            string _fileName;
            string _dateSent;

        public string FileName
        {
            get
            {
                return this._fileName;
            }
            set
            {
                this._fileName = value;
            }
        }

        public string DateSent
        {
            get
            {
                return this._dateSent;
            }
            set
            {
                this._dateSent = value;
            }
        }


        
    }
}
