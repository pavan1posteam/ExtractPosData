using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.Models
{
    // Getting Location

    public class locations
    {
        public string id { get; set; }
        public string name { get; set; }
        public string timezone { get; set; }
        public string status { get; set; }
        public string merchant_id { get; set; }
        public string currency { get; set; }
        public string business_name { get; set; }
        public string[] capabilities { get; set; }
        public string type { get; set; }
        public string country { get; set; }
        public address address { get; set; }
    }
    public class address
    {
        public string address_line_1 { get; set; }
        public string city { get; set; }
        public string locality { get; set; }
        public string administrative_district_level_1 { get; set; }
        public int postal_code { get; set; }
        public string country { get; set; }
    }
    public class SquareLocations
    {
        public List<locations> locations { get; set; }
    }
    // For Product Category

    public class ProductCategory
    {
        public string type { get; set; }
        public string id { get; set; }
        public string updated_at { get; set; }
        public Int64 version { get; set; }
        public bool is_deleted { get; set; }
        public bool present_at_all_locations { get; set; }
        public List<attributes> custom_attributes { get; set; }
        public CategoryData category_data { get; set; }
    }
    public class attributes
    {
        public string name { get; set; }
        public int int_value { get; set; }
    }
    public class CategoryData
    {
        public string name { get; set; }
    }
    public class SquareCategory
    {
        public List<ProductCategory> objects { get; set; }
    }
    // For Tax
    public class Tax
    {
        public string type { get; set; }
        public string id { get; set; }
        public string updated_at { get; set; }
        public Int64 version { get; set; }
        public bool is_deleted { get; set; }
        public TaxData tax_data { get; set; }
    }
    public class TaxData
    {
        public string name { get; set; }
        public string calculation_phase { get; set; }
        public string inclusion_type { get; set; }
        public decimal percentage { get; set; }
        public bool applies_to_custom_amounts { get; set; }
        public bool enabled { get; set; }
        public string tax_type_id { get; set; }
        public string tax_type_name { get; set; }
    }
    public class SquareTax
    {
        public List<Tax> objects { get; set; }
    }
    // For Item
    public class Product
    {
        public string type { get; set; }
        public string id { get; set; }
        public string updated_at { get; set; }
        public Int64 version { get; set; }
        public bool is_deleted { get; set; }
        public bool present_at_all_locations { get; set; }
        public ItemData item_data { get; set; }
        public List<CatalogV1Id> catalog_v1_ids { get; set; }
    }
    public class CatalogV1Id
    {
        public string catalog_v1_id { get; set; }
        public string location_id { get; set; }
    }
    public class ItemData
    {
        public string name { get; set; }
        public string description { get; set; }
        public string visibility { get; set; }
        public string category_id { get; set; }
        public List<string> tax_ids { get; set; }
        public List<Variation> variations { get; set; }
        public string product_type { get; set; }
        public bool skip_modifier_screen { get; set; }
    }
    public class Variation
    {
        public string type { get; set; }
        public string id { get; set; }
        public string updated_at { get; set; }
        public Int64 version { get; set; }
        public bool is_deleted { get; set; }
        public bool present_at_all_locations { get; set; }
        public List<string> present_at_location_ids { get; set; }
        public ItemVariation item_variation_data { get; set; }
    }
    public class ItemVariation
    {
        public string item_id { get; set; }
        public string name { get; set; }
        public string sku { get; set; }
        public string upc { get; set; }
        public int ordinal { get; set; }
        public string pricing_type { get; set; }
        public PriceMoney price_money { get; set; }
    }
    public class PriceMoney
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }
    public class SquareProductItem
    {
        public string Cursor { get; set; }
        public List<Product> objects { get; set; }
    }

    // Inventory Model for getting Quantity

    public class Count
    {
        public string catalog_object_id { get; set; }
        public string catalog_object_type { get; set; }
        public string state { get; set; }
        public string location_id { get; set; }
        public string quantity { get; set; }
        public DateTime calculated_at { get; set; }
    }

    public class Inventory
    {
        public List<Count> counts { get; set; }
        public string cursor { get; set; }
    }
    //public class Inventory
    //{
    //    //public string variation_id { get; set; }      // Used in Older Version V1
    //    //public int quantity_on_hand { get; set; }     // Used in Older Version V1
    //    // Used in New Version V2
    //    public string catalog_object_id { get; set; }
    //    public string catalog_object_type { get; set; }
    //    public string state { get; set; }
    //    public string location_id { get; set; }
    //    public string quantity { get; set; }
    //    public string calculated_at { get; set; }
    //}
    public class v1Items
    {
        public bool available_for_pickup { get; set; }
        public bool available_online { get; set; }
        public string visibility { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string category_id { get; set; }
        public string type { get; set; }
        public List<v1Fees> fees { get; set; }
        public List<v1Variations> variations { get; set; }
        public v1Category category { get; set; }
    }
    public class v1Category
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class QuantityList
    {
        public string ItemId { get; set; }
        public string Id { get; set; }
        public int QuantityOnHand { get; set; }
        public string category_id { get; set; }

    }
    public class v1Fees
    {
        public bool enabled { get; set; }
        public bool applies_to_custom_amounts { get; set; }
        public string inclusion_type { get; set; }
        public string adjustment_type { get; set; }
        public string calculation_phase { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string rate { get; set; }
        public string type { get; set; }
    }
    public class v1Variations
    {
        public string inventory_alert_type { get; set; }
        public bool track_inventory { get; set; }
        public string pricing_type { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string sku { get; set; }
        public int ordinal { get; set; }
        public string item_id { get; set; }
        public string user_data { get; set; }
        public v1PriceMoney price_money { get; set; }
    }
    public class v1PriceMoney
    {
        public string currency_code { get; set; }
        public decimal amount { get; set; }
    }

    public class SquareProductModel
    {
        public string v1Itemid { get; set; }
        public int StoreID { get; set; }
        public string Upc { get; set; }
        public int Qty { get; set; }
        public string Sku { get; set; }
        public int Pack { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public string CategoryName { get; set; }
        public string category_id { get; set; }
        public decimal? Price { get; set; }
        public decimal? Sprice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }

    }
    public class SquareCSvProductModel
    {
        public int storeid { get; set; }
        public string upc { get; set; }
        public int qty { get; set; }
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

    }
    public class QueryResult
    {
        public string Response { get; set; }
        public string Url { get; set; }

    }
    public class SquareCSvProductModelDemo
    {
        public int storeid { get; set; }
        public string upc { get; set; }
        public int qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public string uom { get; set; }
  
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
        public string categoryid { get; set; }
        public string ItemVariationID { get; set; }
        public string InvVariationId { get;set; }
    }
    public class SquareCSvProductModels
    {
        public int storeid { get; set; }
        public string upc { get; set; }
        public int qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string uom { get; set; }
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

    }
    public class ProductList
    {
        public string varitionId { get; set; }
        public string prodname { get; set; }
        public string sku { get; set; }
        public string upc { get; set; }
        public string category_id { get; set; }
        public string size { get; set; }
        public decimal amount { get; set; }
        public string description { get; set; }
    }

}