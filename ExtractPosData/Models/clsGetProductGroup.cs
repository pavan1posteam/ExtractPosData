using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.Models
{
    public class clsGetProductGroup
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
            public string id { get; set; }
            public int productGroupID { get; set; }
            public string name { get; set; }
            public string showInWebshop { get; set; }
            public int nonDiscountable { get; set; }
            public int positionNo { get; set; }
            public int added { get; set; }
            public int lastModified { get; set; }
            public string parentGroupID { get; set; }
            public List<object> subGroups { get; set; }
        }
        public class RootObject
        {
            public Status status { get; set; }
            public List<Record> records { get; set; }
        }
    }
}
