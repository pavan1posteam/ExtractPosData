using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using ExtractPosData.Model;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;
using System.Configuration;
using ExtractPosData.Models;
using System.Net;

namespace ExtractPosData
{
   public class clsLightspeedRetail_Api
    {
       string AccessToken = "";
        public clsLightspeedRetail_Api(int StoreId, decimal tax, string BaseUrl,string ClientId, string ClientSecret,string RefreshToken, int AccountID,bool IsMarkUpPrice,int MarkUpValue)
        {
            try
            {
                Console.WriteLine("Generating LightspeedRetailAPI " + StoreId + " Product File....");
                Console.WriteLine("Generating LightspeedRetailAPI " + StoreId + " Fullname File....");
                LightspeedRetail_Api(StoreId, tax, BaseUrl, ClientId, ClientSecret, RefreshToken, AccountID, IsMarkUpPrice, MarkUpValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedRetailAPI " + StoreId);
            }
        }

        public void LightspeedRetail_Api(int storeid, decimal tax, string BaseUrl, string ClientId, string ClientSecret, string RefreshToken, int AccountID, bool IsMarkUpPrice, int MarkUpValue)
        {
            List<clsLightProductList.Item> Items = new List<clsLightProductList.Item>();
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
            try
            {
                var ItemResult = LightspeedSetting(BaseUrl, storeid, tax, ClientId, ClientSecret, RefreshToken, AccountID);
              //  var CategoriesList = LightspeedCategoryList(BaseUrl, storeid, tax, ClientId, ClientSecret, RefreshToken, AccountID);
                List<clsLightProductList.LightProductModel> prodList = new List<clsLightProductList.LightProductModel>();
                List<clsLightProductList.LightFullnameModel> fullNameList = new List<clsLightProductList.LightFullnameModel>();
              //  List<clsLightProductList.LightCategories> categoryList = new List<clsLightProductList.LightCategories>();
                foreach (var item in ItemResult)
                {
                    foreach (var record in item)
                    {
                        try
                        {
                            clsLightProductList.LightProductModel prod = new clsLightProductList.LightProductModel();
                            clsLightProductList.LightFullnameModel fullName = new clsLightProductList.LightFullnameModel();
                            ///  clsLightProductList.LightCategories Catlist = new clsLightProductList.LightCategories();
                            clsLightProductList.Item ItemList = new clsLightProductList.Item();
             
                            prod.StoreID = storeid;

                            var priceItem = JsonConvert.DeserializeObject<clsLightProductList.Prices>(record["Prices"].ToString());
                            var json2 = JsonConvert.SerializeObject(record["Prices"]);
                            var prc = priceItem.ItemPrice.ToString();
                            prc = prc.ToString().Split(':', ',')[1];
                            prc = prc.Replace("\"", "");

                            string pr = prc;
                            prod.Price = Convert.ToDecimal(prc);
                            fullName.Price = Convert.ToDecimal(prc);
                            if (storeid == 11312)
                            {
                                decimal price = 0;
                                if (IsMarkUpPrice)
                                {

                                    decimal markup = Convert.ToDecimal(prc)- (Convert.ToDecimal(prc)*MarkUpValue /100);
                                    prod.Price = (markup);
                                    fullName.Price = (markup);
                                }
                            }
                            var qtyItem = JsonConvert.DeserializeObject<clsLightProductList.ItemShops>(record["ItemShops"].ToString());
                            var json = JsonConvert.SerializeObject(record["ItemShops"]);
                            var qtyy = qtyItem.ItemShop.ToString();

                            qtyy = qtyy.ToString().Split(':', ',')[28];
                            qtyy = qtyy.Replace("\"", "");

                            string qt = qtyy;
                            prod.Qty = Convert.ToInt64(qt);

                            var Cat_Item = JsonConvert.DeserializeObject<clsLightProductList.Category>(record["Category"].ToString());
                            var json_cat = JsonConvert.SerializeObject(record["Category"]);
                           
                            string upc = (string)record["upc"];
                            string cat = Cat_Item.name.ToString();
                            if (storeid == 10936)                                                                                   ////31-05-2021(tckt#9597)
                            {
                                if (string.IsNullOrEmpty(upc) && (cat == "CIGARS" || cat == "CIGAR" || cat == "CIGARETTES"))
                                {
                                    prod.upc = '#' + (string)record["customSku"];
                                    fullName.upc = '#' + (string)record["customSku"];
                                }
                               
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }
                            }
                            else if (storeid == 11312)
                            {
                                if (string.IsNullOrEmpty(upc))
                                {
                                    prod.upc = '#' + (string)record["manufacturerSku"];
                                    fullName.upc = '#' + (string)record["manufacturerSku"];
                                }
                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }

                            }

                            else
                            {
                                if (string.IsNullOrEmpty(upc))
                                { continue; }

                                else
                                {
                                    prod.upc = '#' + (string)record["upc"];
                                    fullName.upc = '#' + (string)record["upc"];
                                }
                            }                                                                                                         ////31-05-2021(tckt#9597)
                            string sku = (string)record["itemID"];
                            prod.sku = '#' + sku;
                            fullName.sku = '#' + sku;
                            prod.pack = 1;
                            fullName.pack = 1;
                            prod.StoreProductName = (string)record["description"];
                            prod.StoreDescription = (string)record["description"];
                            prod.sprice = 0;
                            prod.tax = tax;
                            prod.Start = "";
                            prod.End = "";
                            prod.altupc1 = "";
                            prod.altupc2 = "";
                            prod.altupc3 = "";
                            prod.altupc4 = "";
                            prod.altupc5 = "";

                            fullName.pname = (string)record["description"];
                            fullName.pdesc = (string)record["description"];
                            fullName.pcat = Cat_Item.name.ToString();
                            fullName.pcat1 = "";
                            fullName.pcat2 = "";
                            prod.uom = "";
                            fullName.uom = "";
                            fullName.country = "";
                            fullName.region = "";
                       
                            if (storeid == 10932)
                            {
                                if (prod.Price > 0 && prod.upc.Trim() != "#" && prod.Qty > 0)
                                {
                                    prodList.Add(prod);
                                    fullNameList.Add(fullName);
                                }
                            }
                            else
                            {
                                if (prod.Price > 0 && prod.upc.Trim() != "#" && prod.StoreDescription != "SHOW CIGARILLOS TROPICAL TWISTA" && prod.StoreDescription != "SHOW CIGARILLOS SWEET" && prod.StoreDescription != "SHOW CIGARILLOS WHITE GRAPE" && prod.StoreDescription != "SHOW CIGARILLOS MANGO" && prod.StoreDescription != "KUSH BLACK 2 TIPPED CIGARS" && prod.StoreDescription != "PRIME TIME LITTLE CIGARS")
                                {
                                    prodList.Add(prod);
                                    fullNameList.Add(fullName);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    
                }
                        GenerateCSV.GenerateCSVFile(prodList, "PRODUCT", storeid, BaseDirectory);
                        GenerateCSV.GenerateCSVFile(fullNameList, "FULLNAME", storeid, BaseDirectory);
                        Console.WriteLine();
                        Console.WriteLine("Product FIle Generated For LightspeedRetailPos " + storeid);
                        Console.WriteLine("Fullname FIle Generated For LightspeedRetailPos " + storeid);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string getProduct(string BaseUrl, string clientid, string clientsecret, string RefreshToken)
        {
            string AccessToken = "";
            clsLightProductList.Root prod = new clsLightProductList.Root();
            try
            {
                var client = new RestClient(BaseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "multipart/form-data");
                request.AddHeader("cache-control", "no-cache");
                request.AddParameter("client_id", clientid);
                request.AddParameter("client_secret", clientsecret);
                request.AddParameter("refresh_token", RefreshToken);
                request.AddParameter("grant_type", "refresh_token");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var result = JsonConvert.DeserializeObject<clsLightProductList.Root>(content);
                    AccessToken = result.access_token; 
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedRetailAPI ");
            }
            return AccessToken;
        }

        public List<JArray> LightspeedSetting(string BaseUrl, int StoreId, decimal tax,string ClientId, string ClientSecret, string RefreshToken, int AccountID )
        {
            string Url = "";
            clsLightProductList.Result obj = new clsLightProductList.Result();
            int recordsTotal = 100;
            List<JArray> productList = new List<JArray>(); ;
            var accesstoken = getProduct(BaseUrl, ClientId, ClientSecret, RefreshToken);
            BaseUrl = "https://api.lightspeedapp.com/API/Account/"+AccountID+"/";
            clsLightProductList.Root prod = new clsLightProductList.Root();
          
            try
            {
                for (int pageNo = 0; pageNo <= recordsTotal-100; pageNo++)
                {
                    //string ApiUrl = BaseUrl + "Item.json"+"?load_relations=%5B%22ItemShops%22%5D";                     
                    string shops =   "load_relations=[\"ItemShops\",\"Category\"]";
                 
                    string ApiUrl = BaseUrl + "Item.json" + "?"+shops+"";                             ///31-05-2021 (Categories added in url)
                    ApiUrl = string.IsNullOrEmpty(Url) ? ApiUrl : Url;
                    var client = new RestClient(ApiUrl);
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", "Bearer " + accesstoken);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var content = response.Content;
                        string count = content.ToString();
                         count= count.Split(':',',')[2];
                         count = count.Replace("\"", ""); 
                        var pJson = (dynamic)JObject.Parse(content);
                        var jArray = (JArray)pJson["Item"];
                        productList.Add(jArray);
                        string LastItem = jArray.LastOrDefault().FirstOrDefault().ToString().Split(':')[1];
                        LastItem = LastItem.Replace("\"", ""); 
                        decimal LastItemID = Convert.ToDecimal(LastItem);
 
                        if (LastItemID.ToString() != null)
                        {
                           // Url = BaseUrl +"Item.json"+"?orderby=itemID&itemID=%3E%2C" + LastItemID +"&load_relations=%5B%22ItemShops%22%5D";
                            Url = BaseUrl + "Item.json" + "?orderby=itemID&itemID=%3E%2C" + LastItemID + "&" + shops + "";
                        }
                        recordsTotal = Convert.ToInt32(count);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedRetailAPI ");
            }
            return productList;
        }
       
        public class clsLightProductList
        {
            public class Root
            {
                public object @attributes { get; set; }
                public List<Item> Items { get; set; }
                public string access_token { get; set; }
            }
            public class Item
            {
                public object @attributes { get; set; }
                public int itemID { get; set; }
                public int systemSku { get; set; }
                public float defaultCost { get; set; }
                public float avgCost { get; set; }
                public Boolean discountable { get; set; }
                public Boolean archived { get; set; }
                public int itemType { get; set; }
                public Boolean serialized { get; set; }
                public string description { get; set; }
                public int modelYear { get; set; }
                public string upc { get; set; }
                public string ean { get; set; }
                public string customSku { get; set; }
                public string manufacturerSku { get; set; }
                public DateTime createTime { get; set; }
                public DateTime timeStamp { get; set; }
                public Boolean publishToEcom { get; set; }
                public int categoryID { get; set; }
                public int taxClassID { get; set; }
                public int departmentID { get; set; }
                public int itemMatrixID { get; set; }
                public int manufacturerID { get; set; }
                public int seasonID { get; set; }
                public int defaultVendorID { get; set; }
                public object ItemShops { get; set; }
                public object Prices { get; set; }
                public object category { get; set; }
                public int catvID { get; set; }
                public string catname { get; set; }
            }
            public class ItemShops
            {
                public object ItemShop { get; set; }
                public int itemShopID { get; set; }
                public int qoh { get; set; }
                public int sellable { get; set; }
                public int backorder { get; set; }
                public int componentQoh { get; set; }
                public int componentBackorder { get; set; }
                public int reorderPoint { get; set; }
                public int reorderLevel { get; set; }
                public DateTime timeStamp { get; set; }
                public int itemID { get; set; }
                public int shopID { get; set; }
            }
            public class ItemShop
            {
               // public object ItemShop { get; set; }
                public int itemShopID { get; set; }
                public int qoh { get; set; }
                public int sellable { get; set; }
                public int backorder { get; set; }
                public int componentQoh { get; set; }
                public int componentBackorder { get; set; }
                public int reorderPoint { get; set; }
                public int reorderLevel { get; set; }
                public DateTime timeStamp { get; set; }
                public int itemID { get; set; }
                public int shopID { get; set; }
            }
            public class Prices
            {
                public object ItemPrice { get; set; }
               // public decimal amount { get; set; }
                public string useTypeID { get; set; }
                public string useType { get; set; }
            }
            public class ItemPrice
            {
               // public object ItemPrice { get; set; }
                public decimal amount { get; set; }
                public string useTypeID { get; set; }
                public string useType { get; set; }
            }
            public class Result
            {
                public string Response { get; set; }
                public string Url { get; set; }
            }    
            public class @attributes
            {
                //public string @attributes { get; set; }
                public int count { get; set; }
                public List<Item> Items { get; set; }
               
                public string offset { get; set; }
                public string limit { get; set; }
            }
            public class Category
            {
                public int categoryID { get; set; }
                 public string name { get; set; }
                public int nodeDepth { get; set; }
                public string fullPathName { get; set; }
                public int leftNode { get; set; }
                public int rightNode { get; set; }
                public int parentID { get; set; }
                public DateTime createTime { get; set; }
                public DateTime timeStamp { get; set; }
            }

            public class LightFullnameModel
            {
                public string pname { get; set; }
                public string pdesc { get; set; }
                public string upc { get; set; }
                public string sku { get; set; }
                public decimal Price { get; set; }
                public string uom { get; set; }
                public int pack { get; set; }
                public string pcat { get; set; }
                public string pcat1 { get; set; }
                public string pcat2 { get; set; }
                public string country { get; set; }
                public string region { get; set; }
            }
            public class LightProductModel
            {
                public int StoreID { get; set; }
                public string upc { get; set; }
                public Int64 Qty { get; set; }
                public string sku { get; set; }
                public int pack { get; set; }
                public string uom { get; set; }
                public string StoreProductName { get; set; }
                public string StoreDescription { get; set; }
                public decimal Price { get; set; }
                public decimal sprice { get; set; }
                public string Start { get; set; }
                public string End { get; set; }
                public decimal tax { get; set; }
                public string altupc1 { get; set; }
                public string altupc2 { get; set; }
                public string altupc3 { get; set; }
                public string altupc4 { get; set; }
                public string altupc5 { get; set; }
               // public int catID { get; set; }
            }
        }
    }
}