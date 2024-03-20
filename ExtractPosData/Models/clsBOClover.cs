using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.Models
{
    public class clsBOClover
    {
    }
    public class parentItemsStock
    {
        public List<Products> elements { set; get; }
    }
    public class parentItems
    {
        public List<Products> elements { set; get; }

        public parentItems()
        {
            List<Products> Products = new List<Products>();
        }
    }
    public class ItemGroup
    {
        public string id { get; set; }
    }
    public class Products
    {
        public string storeid { set; get; }
        public string id { set; get; }
        public Boolean hidden { set; get; }
        public ItemGroup itemGroup { set; get; }
        public string name { set; get; }
        public string alternateName { set; get; }
        public string code { set; get; }
        public string sku { set; get; }
        public decimal price { set; get; }
        public string priceType { set; get; }
        public string defaultTaxRates { set; get; }
        public Decimal cost { set; get; }
        public Boolean isRevenue { set; get; }
        public long stockCount { set; get; }
        public Int64 modifiedTime { set; get; }
        public itemStock itemStock { set; get; }
        public taxRates taxRates { set; get; }
        public string altupc1 { set; get; }
        public string altupc2 { set; get; }
        public string altupc3 { set; get; }
        public string altupc4 { set; get; }
        public string altupc5 { set; get; }
        public CloverCategories categories { get; set; }

        public Products()
        {
            itemStock itemStock = new itemStock();
            taxRates taxRates = new taxRates();
        }
    }
    public class CloverCategories
    {
        public List<Categoryelements> elements { set; get; }
    }
    public class Categoryelements
    {
        public string id { set; get; }
        public string name { set; get; }
        public int sortOrder { get; set; }
    }
    public class taxRates
    {
        public List<taxelements> elements { set; get; }
    }
    public class taxelements
    {
        public string id { set; get; }
        public string name { set; get; }
        public Int32 rate { set; get; }
        public Boolean isDefault { set; get; }
        public TaxItems items { set; get; }
    }
    public class TaxItems
    {
        public List<taxitemelemnt> elements { set; get; }
    }
    public class taxitemelemnt
    {
        public string id { set; get; }
    }
    public class itemStock
    { 
        public item item { set; get; }
        public long stockCount { set; get; }
        public decimal quantity { set; get; }

        public itemStock()
        {
            item item = new item();
        }
    }
    public class item
    {
        public string id { set; get; }
        public item() { }
    }
    public class ExportProductss
    {
        //public string storeid { set; get; }
        //public string upc { set; get; }
        //public long qty { set; get; }
        //public string sku { set; get; }
        //public Int32 pack { set; get; }
        //public string StoreProductName { set; get; }
        //public string Storedescription { set; get; }
        //public decimal price { set; get; }
        //public decimal sprice { set; get; }
        //public string start { set; get; }
        //public string end { set; get; }
        //public decimal tax { set; get; }
        //public string altupc1 { set; get; }
        //public string altupc2 { set; get; }
        //public string altupc3 { set; get; }
        //public string altupc4 { set; get; }
        //public string altupc5 { set; get; }
        //public string CategoryId { get; set; }
    }
    
    //public class Tax
    //{
    //    public List<TaxElements> elements { set; get; }
    //}
    //public class TaxElements
    //{
    //    public string id { set; get; }
    //    public string name { set; get; }
    //    public Int32 rate { set; get; }
    //    public Boolean isDefault { set; get; }

    //}
}
