using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.Models
{
    public class clsBOCloverStoreSettings
    {
        public string clientid { get; set; }
        public string merchantid { get; set; }
        public string code { get; set; }
        public string tokenid { get; set; }
        public string instock { get; set; }
        public List<categories> categories { set; get; }
    }
    //public class categories
    //{
    //    public string id { get; set; }
    //    public string name { get; set; }
    //    public decimal taxrate { get; set; }
    //    public Boolean selected { get; set; }
    //}
    public class merchant
    {
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public owner owner { get; set; }

        public address address { set; get; }
        public merchantPlan merchantPlan { set; get; }
        public string phoneNumber { set; get; }
        public string customerContactEmail { set; get; }

        public string createdTime { set; get; }
        public gateway gateway { set; get; }
        public tenders tenders { set; get; }
        public shifts shifts { set; get; }
        public orders orders { set; get; }
        public payments payments { set; get; }
        public taxRates taxRates { set; get; }

        public string isBillable { set; get; }

    }
    public class merchantPlan
    {
    }
    public class owner
    {
        public string name { get; set; }
        public string nickname { get; set; }
        public string email { get; set; }
    }
    public class gateway
    {}
    public class shifts
    {}
    public class tenders
    {}
    public class orders
    {}
    public class payments
    {}
    //public class address
    //{
    //    public string address1 { get; set; }
    //    public string address2 { get; set; }
    //    public string address3 { get; set; }
    //    public string country { get; set; }
    //    public string zip { get; set; }
    //    public string phoneNumber { get; set; }
    //}
}
