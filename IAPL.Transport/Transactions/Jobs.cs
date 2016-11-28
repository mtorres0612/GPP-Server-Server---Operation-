using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAPL.Transport.Transactions
{
    public class Jobs
    {
        public JobGroup JobGroups { get; set; }
    }

    public class JobGroup
    {
        public ABCD ABCD { get; set; }
        public EFGH EFGH { get; set; }
    }

    public class MainAlpha
    {
        public List<Message> Messages { get; set; }
    }

    public class ABCD : MainAlpha
    {
    }

    public class EFGH : MainAlpha
    {

    }

    public class Message
    {
        public string ThreadName { get; set; }
        public string MessageCode { get; set; }
        public string ERP { get; set; }
        public string PrincipalCode { get; set; }
        public bool MsgMonday { get; set; }
        public bool MsgTuesday { get; set; }
        public bool MsgWednesday { get; set; }
        public bool MsgThursday { get; set; }
        public bool MsgFriday { get; set; }
        public bool MsgSaturday { get; set; }
        public bool MsgSunday { get; set; }
        public bool MsetRunTime { get; set; }
        public DateTime MsetStartTime { get; set; }
        public DateTime MsetEndTime { get; set; }
        public string JobCronSchedule { get; set; }
        public bool RepeatForever { get; set; }
        public bool StartNow { get; set; }

    }
}
