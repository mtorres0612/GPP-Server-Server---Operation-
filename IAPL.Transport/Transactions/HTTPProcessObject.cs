using System;
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport.Transactions
{
    class HTTPProcessObject
    {
        public HTTPProcessObject()
        {
           
        }

        string _errorMessage;
        string _msgCode = string.Empty;
        string _principal = string.Empty;
        string _sourceFile = string.Empty;
        string _supplierID = string.Empty;
        string _path = string.Empty;
        string _tradingCode = string.Empty;
        string _guid = string.Empty;
        string _fileName = string.Empty;
        string _fileTransferSettingID = string.Empty;
        string _transactionDate = string.Empty;
        string _url = string.Empty;
        string _userName = string.Empty;
        string _password = string.Empty;
        string _supplierName = string.Empty;
        string _erp = string.Empty;
        string _emldEmailSubject = string.Empty;
        string _emldIntEmailAddrFROM = string.Empty;
        string _emldIntEmailAddrTO = string.Empty;
        string _emldIntEmailAddrCC  = string.Empty;
        string _emldIntEmailAddrBCC = string.Empty;
        string _emldExtEmailAddrTO = string.Empty;
        string _emldExtEmailAddrCC  = string.Empty;
        string _emldXSLTPath = string.Empty;

        public string ErrorMessage
        {
            get
            {
                return this._errorMessage;
            }
            set
            {
                this._errorMessage = value;
            }
        }


        public string MessageCode
        {
            get
            {
                return this._msgCode;
            }
            set
            {
                this._msgCode = value;
            }
        }
        public string Principal
        {
            get
            {
                return this._principal;
            }
            set
            {
                this._principal = value;
            }
        }
        public string SourceFile
        {
            get
            {
                return this._sourceFile;
            }
            set
            {
                this._sourceFile = value;
            }
        }
        public string SupplierID
        {
            get
            {
                return this._supplierID;
            }
            set
            {
                this._supplierID = value;
            }
        }
        public string TradingCode
        {
            get
            {
                return this._tradingCode;
            }
            set
            {
                this._tradingCode = value;
            }
        }
        public string path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }
        public string Guid
        {
            get
            {
                return this._guid;
            }
            set
            {
                this._guid = value;
            }
        }
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
        public string URL
        {
            get
            {
                return this._url;
            }
            set
            {
                this._url = value;
            }
        }
        public string TransactionDate
        {
            get
            {
                return this._transactionDate;
            }
            set
            {
                this._transactionDate = value;
            }
        }
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
            }
        }
        public string UserName
        {
            get
            {
                return this._userName;
            }
            set
            {
                this._userName = value;
            }
        }
        public string FileTransferSettingID
        {
            get
            {
                return this._fileTransferSettingID;
            }
            set
            {
                this._fileTransferSettingID = value;
            }
        }

        public string SupplierName
        {
            get
            {
                return this._supplierName;
            }
            set
            {
                this._supplierName = value;
            }
        }

        public string ERP
        {
            get
            {
                return this._erp;
            }
            set
            {
                this._erp = value;
            }
        }

        public string EmailSubject
        {
            get
            {
                return this._emldEmailSubject;
            }
            set
            {
                this._emldEmailSubject = value;
            }
        }

        public string EmailFrom
        {
            get
            {
                return this._emldIntEmailAddrFROM;
            }
            set
            {
                this._emldIntEmailAddrFROM = value;
            }
        }
        
        //
        public string EmailTo
        {
            get
            {
                return this._emldIntEmailAddrTO;
            }
            set
            {
                this._emldIntEmailAddrTO = value;
            }
        }
        public string EmailCC
        {
            get
            {
                return this._emldIntEmailAddrCC;
            }
            set
            {
                this._emldIntEmailAddrCC = value;
            }
        }
        public string EmailBCC
        {
            get
            {
                return this._emldIntEmailAddrBCC;
            }
            set
            {
                this._emldIntEmailAddrBCC = value;
            }
        }
        public string EmailEXTTo
        {
            get
            {
                return this._emldExtEmailAddrTO;
            }
            set
            {
                this._emldExtEmailAddrTO = value;
            }
        }
        public string EmailEXTCC
        {
            get
            {
                return this._emldExtEmailAddrCC;
            }
            set
            {
                this._emldExtEmailAddrCC = value;
            }
        }
        public string EmailXSLTPath
        {
            get
            {
                return this._emldXSLTPath;
            }
            set
            {
                this._emldXSLTPath = value;
            }
        }
       
        
    }
}
