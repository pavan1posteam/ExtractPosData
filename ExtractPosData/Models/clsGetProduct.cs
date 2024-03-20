using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.Models
{
    public class clsGetProduct
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

        public class Warehouses
        {
            public int warehouseID { get; set; }
            public string totalInStock { get; set; }
            public int reserved { get; set; }
            public int free { get; set; }
            public int orderPending { get; set; }
            public int reorderPoint { get; set; }
            public int restockLevel { get; set; }
        }

        public class Record
        {
            public int productID { get; set; }
            public string name { get; set; }
            public string nameENG { get; set; }
            public string code { get; set; }
            public string code2 { get; set; }
            public string code3 { get; set; }
            public string supplierCode { get; set; }
            public int groupID { get; set; }
            public double price { get; set; }
            public int active { get; set; }
            public int nonDiscountable { get; set; }
            public string manufacturerName { get; set; }
            public string priorityGroupID { get; set; }
            public string countryOfOriginID { get; set; }
            public int brandID { get; set; }
            public int added { get; set; }
            public double priceWithVat { get; set; }
            public object unitName { get; set; }
            public object brandName { get; set; }
            public string groupName { get; set; }
            public int categoryID { get; set; }
            public object categoryName { get; set; }
            public string status { get; set; }
            public string priceListPrice { get; set; }
            public double priceListPriceWithVat { get; set; }
            public List<object> priceCalculationSteps { get; set; }
            public int taxFree { get; set; }
            public string type { get; set; }
            public Warehouses warehouses { get; set; }
        }
        public class RootObject
        {
            public Status status { get; set; }
            public List<Record> records { get; set; }
        }
    }
}
