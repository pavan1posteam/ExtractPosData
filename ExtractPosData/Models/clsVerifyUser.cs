using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.Models
{
    public class clsVerifyUser
    {
        public class Status
        {
            public string request { get; set; }
            public int requestUnixTime { get; set; }
            public string responseStatus { get; set; }
            public int errorCode { get; set; }
            public double generationTime { get; set; }
            public int recordsTotal { get; set; }
            public int recordsInResponse { get; set; }
        }
        public class Record
        {
            public string userID { get; set; }
            public string userName { get; set; }
            public string employeeName { get; set; }
            public string groupID { get; set; }
            public string groupName { get; set; }
            public string sessionKey { get; set; }
        }

        public class RootObject
        {
            public Status status { get; set; }
            public List<Record> records { get; set; }
        }
    }
}
