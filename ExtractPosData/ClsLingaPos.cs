using ExtractPosData.Model;
using ExtractPosData.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;

namespace ExtractPosData
{
    class ClsLingaPos
    {

        string folderPath = ConfigurationManager.AppSettings["BaseDirectory"];
        public ClsLingaPos(int StoreId, decimal tax, string BaseUrl, string ApiKey, string Store_id,string Token,string I_Storeid)
        {
            try
            {
                Console.WriteLine("Generating ClsLingaPos " + StoreId + " Product File....");
                LingaSetting(StoreId, tax, BaseUrl, ApiKey, Store_id, Token,I_Storeid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LINGA " + StoreId);
            }
        }
        private List<Product> getProduct(int StoreId, decimal tax, string BaseUrl, string ApiKey, string Store_id, string Token)
        {

            List<Product> productList = new List<Product>();

            try
            {
                string ApiUrl = BaseUrl + Store_id + "/menuItems";
                var client = new RestClient(ApiUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("apikey", ApiKey);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var prodResult = JsonConvert.DeserializeObject<List<Product>>(content);
                    productList.AddRange(prodResult);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "LINGAPOS");
            }
            return productList;
        }
        private List<InventoryItem> getStock(int StoreId, decimal tax, string BaseUrl, string ApiKey, string Store_id, string Token, string I_Storeid)
        {
            List<InventoryItem> Stocklist = new List<InventoryItem>();

            try
            {
                string ApiUrl = "https://inventory.lingapos.com/store/"+ I_Storeid + "/inventoryItems?inventoryLoginToken="+ Token + "&limit=10000&orderBy=ATOZ_NAME&page=1&search=&status=true";
                var client = new RestClient(ApiUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("apikey", ApiKey);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var stockResult = JsonConvert.DeserializeObject<Stock>(content);
                    Stocklist.AddRange(stockResult.inventoryItems);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "LingaAPI ");
            }
            return Stocklist;

        }
        private void LingaSetting(int StoreId, decimal tax, string BaseUrl, string ApiKey, string Store_id, string Token, string I_Storeid)
        {
            var ProductList = getProduct(StoreId, tax, BaseUrl, ApiKey, Store_id, Token);
            var pList = ProductList;
            var StockList = getStock(StoreId, tax, BaseUrl, ApiKey, Store_id, Token,I_Storeid);
            var sList = StockList;

            var queryList = (from a in pList
                             join b in sList on a.externalRetailID equals b.externalRetailID
                             where a.servingSizePrices.FirstOrDefault().price > 0
                             select new clsProductModel
                             {
                                 StoreID = StoreId,
                                 upc = "#" + a.skuCodes[0],
                                 Qty = Convert.ToInt32(b.inventoryCount),
                                 sku = "#" + a.skuCodes[0],
                                 pack = 1,
                                 StoreProductName = a.name,
                                 StoreDescription = a.name,
                                 Price = Convert.ToDecimal(a.servingSizePrices.FirstOrDefault().price) / 100,

                                 sprice = 0,
                                 Start = "",
                                 End = "",
                                 tax = tax,
                                 altupc1 = "",
                                 altupc2 = "",
                                 altupc3 = "",
                                 altupc4 = "",
                                 altupc5 = ""
                             }).ToList();

            GenerateCSV.GenerateCSVFile(queryList, "PRODUCT", StoreId, folderPath);
        }
        public class Category
        {
            public string name { get; set; }
            public int priority { get; set; }
            public List<object> servingSizePrices { get; set; }
            public string id { get; set; }
        }

        public class ServingSizePrice
        {
            public string id { get; set; }
            public string name { get; set; }
            public int price { get; set; }
            public string priceStr { get; set; }
            public List<object> priceLevels { get; set; }
        }

        public class Product
        {
            public string id { get; set; }
            public string originalID { get; set; }
            public string name { get; set; }
            public string localName { get; set; }
            public Category category { get; set; }
            public List<ServingSizePrice> servingSizePrices { get; set; }
            public int sequenceNumber { get; set; }
            public bool imageAvailable { get; set; }
            public int imageVersion { get; set; }
            public bool activeStatus { get; set; }
            public string measureType { get; set; }
            public int level { get; set; }
            public object printers { get; set; }
            public bool ebtMenuItem { get; set; }
            public string pluCode { get; set; }
            public List<string> skuCodes { get; set; }
            public string imageURL { get; set; }
            public string externalRetailID { get; set; }
        }
        public class InventoryItem
        {
            public string id { get; set; }
            public string name { get; set; }
            public string category { get; set; }
            public int minInventoryCount { get; set; }
            public int inventoryCount { get; set; }
            public int expiredCount { get; set; }
            public string inventoryUnit { get; set; }
            public string costPerUnitStr { get; set; }
            public int costPerUnit { get; set; }
            public string totalPriceStr { get; set; }
            public int totalPrice { get; set; }
            public string sellingPriceStr { get; set; }
            public int sellingPrice { get; set; }
            public string latestPurchasePrice { get; set; }
            public string externalRetailID { get; set; }
            public bool activeStatus { get; set; }
            public object invChildCount { get; set; }
            public string level { get; set; }
        }

        public class Stock
        {
            public List<InventoryItem> inventoryItems { get; set; }
            public int totalPage { get; set; }
            public int currentPage { get; set; }
            public int totalInvItems { get; set; }
        }


    }
}
