using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.Models
{
    public class BroudyProductModel
    {
        public int storeid{ get; set; }
        public int SKU { get; set; }
        public string name { get; set; }
        public string upc { get; set; }
        public string volumeValue { get; set; }
        public string volumeUnit { get; set; }
        public int packSize { get; set; }
        public string containerType { get; set; }
        public int quantityOnHand { get; set; }
        public decimal  price { get; set; }
        public string priceSale { get; set; }
        public decimal taxRate { get; set; }
        public int catCode { get; set; }
        public string catName { get; set; }
        public int typeCode { get; set; }
        public string typeName { get; set; }
        public decimal priceMember { get; set; }
    }
    public class BProductModel {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public decimal Price { get; set; }
        public decimal sprice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal Tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
        public decimal clubprice { get; set; }
    
    }
}
