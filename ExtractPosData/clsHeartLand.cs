using ExtractPosData.Model;
using ExtractPosData.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData
{
    class clsHeartLand
    {
        string folderPath = ConfigurationManager.AppSettings["BaseDirectory"];
        public clsHeartLand(int StoreId, decimal tax, string BaseUrl, string ApiKey)
        {
            try
            {
                Console.WriteLine("Generating Heaartland " + StoreId + " Product File....");
                Console.WriteLine("Generating Heaartland " + StoreId + " Fullname File....");
                HeartlandSetting(StoreId, tax, BaseUrl, ApiKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " Heaartland " + StoreId);
            }
        }
        private List<Product> getProduct(int StoreId, decimal tax, string BaseUrl, string ApiKey)
        {
            List<Product> productList = new List<Product>();

            try
            {
                string ApiUrl = BaseUrl + "/items?per_page=100000";
                var client = new RestClient(ApiUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + ApiKey);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    Product prodResult = (Product)JsonConvert.DeserializeObject(content, typeof(Product));
                    productList.Add(prodResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "HeartlandAPI ");
            }
            return productList;
        }
        private List<Stock> getStock(int StoreId, decimal tax, string BaseUrl, string ApiKey)
        {
            List<Stock> Stocklist = new List<Stock>();

            try
            {
                string ApiUrl = BaseUrl + "/inventory/values?group[]=item_id&per_page=100000";
                var client = new RestClient(ApiUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + ApiKey);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    Stock stockResult = (Stock)JsonConvert.DeserializeObject(content, typeof(Stock));
                    Stocklist.Add(stockResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "HeartlandAPI ");
            }
            return Stocklist;
        }
        private void HeartlandSetting(int StoreId, decimal tax, string BaseUrl, string ApiKey)
        {
            var ProductList = getProduct(StoreId, tax, BaseUrl, ApiKey);
            var pList = ProductList.FirstOrDefault().results;
            var StockList = getStock(StoreId, tax, BaseUrl, ApiKey);
            var sList = StockList.FirstOrDefault().results;          

            var queryList = (from a in pList
                             join b in sList on a.id equals b.item_id
                             where a.price > 0
                             select new clsProductModel
                             {
                                 StoreID = StoreId,                                 
                                 upc = "#" + (a.custom.ean == "" ? a.custom.plu_sku == "" ? a.id.ToString() : a.custom.plu_sku : a.custom.ean),
                                 Qty = Convert.ToInt32(b.qty_on_hand),
                                 sku = "#" + (a.custom.ean == "" ?a.custom.plu_sku==""?a.id.ToString():a.custom.plu_sku:a.custom.ean),
                                 pack = 1,
                                 StoreProductName = a.description,
                                 StoreDescription = a.description,
                                 Price = Convert.ToDecimal(a.price),
                                 sprice = 0,
                                 Start = "",
                                 End = "",
                                 tax = tax,
                                 altupc1 ="",
                                 altupc2 = "",
                                 altupc3 = "",
                                 altupc4 = "",
                                 altupc5 = ""
                             }).ToList();         
            GenerateCSV.GenerateCSVFile(queryList, "PRODUCT", StoreId, folderPath);
        }
        private class Custom
        {
            public string ean { get; set; }
            public string brand { get; set; }
            public string plu_sku { get; set; }
            public string tax_category { get; set; }
            public string _1st_department { get; set; }
            public string _2nd_department { get; set; }
            public string _3rd_department { get; set; }
            public string _4th_department { get; set; }
            public string department_name { get; set; }
            public string kitchen_department { get; set; }
        }
        private class ProductResult
        {
            public int id { get; set; }
            public double? cost { get; set; }
            public double? price { get; set; }
            public string description { get; set; }
            public string long_description { get; set; }
            public string public_id { get; set; }
            public Custom custom { get; set; }
            public bool Active { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public string primary_barcode { get; set; }
            public double? original_price { get; set; }
        }
        private class Product
        {
            public List<ProductResult> results { get; set; }
            public int total { get; set; }
            public int pages { get; set; }
        }
        private class StockResult
        {
            public double qty { get; set; }
            public double qty_on_hand { get; set; }
            public double qty_committed { get; set; }
            public double qty_on_po { get; set; }
            public double qty_in_transit { get; set; }
            public double qty_available { get; set; }
            //public double unit_cost { get; set; }
            public double qty_contributed_to_unit_cost { get; set; }
            public int item_id { get; set; }
        }
        private class Stock
        {
            public List<StockResult> results { get; set; }
            public int total { get; set; }
            public int pages { get; set; }
        }
    }
}
