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


namespace ExtractPosData
{
   public class clsVendHQ
    {
       string AccessToken = "";
       public clsVendHQ(int StoreId, decimal tax, string BaseUrl, string ClientId, string ClientSecret, string RefreshToken)
        {
            try
            {
                Console.WriteLine("Generating VendHQ " + StoreId + " Product File....");
                Console.WriteLine("Generating VendHQ " + StoreId + " Fullname File....");
                clsVendHQ_Products(StoreId, tax, BaseUrl, ClientId, ClientSecret, RefreshToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " VendHQ " + StoreId);
            }
        }

       public void clsVendHQ_Products(int storeid, decimal tax, string BaseUrl, string ClientId, string ClientSecret, string RefreshToken)
        {
            List<clsVendHQProductList.Root> Items = new List<clsVendHQProductList.Root>();
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
            try
            {
                var ItemResult = VendHqSetting(BaseUrl, storeid, tax, ClientId, ClientSecret, RefreshToken);
                List<clsVendHQProductList.VendHQProductModel> prodList = new List<clsVendHQProductList.VendHQProductModel>();
                List<clsVendHQProductList.VendHQFullNameProductModel> fullNameList = new List<clsVendHQProductList.VendHQFullNameProductModel>();
             
                foreach (var item in ItemResult)
                {
                    try
                    {
                        clsVendHQProductList.VendHQProductModel prod = new clsVendHQProductList.VendHQProductModel();
                        clsVendHQProductList.VendHQFullNameProductModel fullName = new clsVendHQProductList.VendHQFullNameProductModel();

                        clsVendHQProductList.Root ItemList = new clsVendHQProductList.Root();
                        var prc = "";
                        if (item.price==null || item.price<=0)
                        {

                            continue;
                        }
                        else
                        {
                            prc = item.price.ToString();
                            prod.Price = Convert.ToDecimal(prc);
                            fullName.Price = Convert.ToDecimal(prc);
                           
                        }
                     
                    
                        var qtyy = "";
                        if (item.inventory==null)
                        {
                            continue;
                            
                        }
                        else
                        {
                            qtyy = item.inventory.FirstOrDefault().count.ToString();
                            prod.Qty = Convert.ToInt64(qtyy);
                        }
                         

                       

                        #region

                        //foreach (var c in CategoriesList)
                        //{
                        //    foreach (var obj in JToken.Parse(c.ToString()).ToList())
                        //    {
                        //        var CatItem = JsonConvert.DeserializeObject<clsLightProductList.LightCategories>(obj.First.ToString());

                        //        ItemList.categoryID = CatItem.categoryID;
                        //        Catlist.categoryID = CatItem.categoryID;
                        //        Catlist.name = CatItem.name;
                        //    }

                        //}

                        //var query = from a in i 
                        //           join o in Catlist on a.catID 
                        //            select new { o.Name, o };

                        //if (record.ToString().Length > 10)
                        //{
                        //    foreach (var obj in JToken.Parse(record.ToString()))
                        //    {

                        //json2 = json2.Replace("{\"ItemPrice\":", "").Replace("}", "");
                        //json2 = json2.Replace("[{\"", "{").Replace("}}", "}");



                        // prod.Price = prc;
                        //var json = JsonConvert.SerializeObject(record["ItemShops"]);


                        //json = json.Replace("{\"ItemShop\":", "").Replace("}}", "}");
                        //json = json.Replace("[{\"", "{").Replace("]}", "");
                        //var warehouse = JToken.Parse(json);


                        //var a = JsonConvert.DeserializeObject<clsLightProductList.ItemShops>(objj.First().ToString());
                        /// var c = JsonConvert.SerializeObject(warehouse["ItemShops"]);
                        // prod.Qty = Convert.ToInt32(c.ToString());
                        //  prod.Price = Convert.ToDecimal(Prices["amount"]);      

                        // var prci = JToken.Parse(json2);

                        //     var qtyItem = JsonConvert.DeserializeObject<clsLightProductList.ItemShop>(c["Itemshop"].ToString());
                        //string q  = qtyItem.ItemShop.ToString();
                        //if (qtyItem.qoh)
                        //{
                        //    prod.Qty = qtyItem.ItemShop.qoh;
                        // }

                        //}
                        //  }
                        //var json = JsonConvert.SerializeObject(record["ItemShops"]);
                        ////foreach (var i in json)
                        ////{
                        ////    prod.Qty = i.
                        ////}
                        //json = json.Replace("{\"ItemShop\":", "").Replace("}", "");
                        //var json1 = JsonConvert.SerializeObject("ItemShop");

                        // var ItemShop = JToken.Parse(json);


                        //ItemShop = JToken.Parse(json1);
                        //var json2 = JsonConvert.SerializeObject(record["Prices"]);
                        ////json2 = json2.Replace("{\"ItemPrice\":", "").Replace("}", "");
                        //var war = json;
                        //var pr = json2;
                        //foreach (var i in prci )
                        //{
                        //    //var a = JsonConvert.DeserializeObject<clsLightProductList.ItemPrice>(i["Prices"].ToString());
                        //   // var c = JsonConvert.SerializeObject();
                        //   // prod = Convert.ToInt32(i["qoh"]);

                        //     // prod.Price = Convert.ToDecimal(a.amount);
                        //}

                        //var qty = (Int64)ItemShop["qoh"];
                        //if (qty > 0)
                        //{
                        //    prod.Qty = Convert.ToInt64(qty);
                        //}
                        //else { continue; }
                        //string qqty = prod.Qty.ToString();
                        //int len = qqty.Length;
                        //if (len > 4)
                        //{ continue; }
                        // string sku = (string)record["systemSku"];

                        //var price = (decimal)Prices["amount"];
                        //if (price > 0)
                        //{
                        //    prod.Price = Convert.ToDecimal(price);
                        //    fullName.Price = Convert.ToDecimal(price);
                        //}
                        #endregion

                        prod.StoreID = storeid;
                        string UPC = item.sku.ToString();

                        if (string.IsNullOrEmpty(UPC)) { continue; }
                        if (prod.upc != "")
                        {
                            prod.upc = '#' + UPC;
                            prod.sku = '#' + UPC;
                            fullName.sku = '#' + UPC;
                            fullName.upc = '#' + UPC;
                        }
                       
                        prod.pack = 1;
                        prod.uom = "";
                        fullName.pack = 1;
                        prod.StoreProductName = item.name.ToString();
                        prod.StoreDescription = item.name.ToString();
                        prod.sprice = 0;
                        prod.Tax = tax;
                        prod.Start = "";
                        prod.End = "";
                        prod.altupc1 = "";
                        prod.altupc2 = "";
                        prod.altupc3 = "";
                        prod.altupc4 = "";
                        prod.altupc5 = "";

                       
                        fullName.pname = item.name.ToString();
                        fullName.pdesc = item.name.ToString();

                        if (fullName.pcat != "")
                        {
                            fullName.pcat = item.type.ToString();
                        }
                        else { fullName.pcat = ""; }

                      
                        if (fullName.pcat1 != "")
                        {
                            fullName.pcat1 = item.tags.ToString();
                        }
                        else { fullName.pcat1 = ""; }
                        fullName.pcat2 = "";
                        fullName.uom = "";
                        fullName.country = "";
                        fullName.region = "";

                        if ( prod.upc.Trim() != "#" && prod.Price>0 && prod.Qty>0 && fullName.pcat!="Cigarettes")
                        {
                            prodList.Add(prod);
                            prodList = prodList.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                            fullNameList = fullNameList.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                            fullNameList.Add(fullName);
                        }
                    }
                    catch (Exception ex)
                    {
                     Console.WriteLine(ex.Message);
                    }
                    }
                    GenerateCSV.GenerateCSVFile(prodList, "PRODUCT", storeid, BaseDirectory);
                    GenerateCSV.GenerateCSVFile(fullNameList, "FULLNAME", storeid, BaseDirectory);
                    Console.WriteLine();
                    Console.WriteLine("Product FIle Generated For VendHQPos " + storeid);
                    Console.WriteLine("Fullname FIle Generated For VendHQPos " + storeid);
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string getaccesstoken(string BaseUrl, string clientid, string clientsecret, string RefreshToken)
        {
            string AccessToken = "";
            clsVendHQProductList.Root prod = new clsVendHQProductList.Root();
            try
            {
                var client = new RestClient(BaseUrl + "1.0/token" + "");
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Cookie", "rguserid=e938db22-bb4f-48a7-8df6-a2f515edbceb; rguuid=true; rgisanonymous=true; vend_retailer=1TCx6za3eEJpuAs8dMJvSRmarDW:T2OOT31NLOIicLonQR6dUgtNqzB");
                request.AddParameter("refresh_token", RefreshToken);
                request.AddParameter("client_id", clientid);
                request.AddParameter("client_secret", clientsecret);
                request.AddParameter("grant_type", "refresh_token");
                request.AddParameter("redirect_uri", "https://localhost/");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var result = JsonConvert.DeserializeObject<clsVendHQProductList.Root>(content);
                    AccessToken = result.access_token.ToString();
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " VendHQ ");
            }
            return AccessToken;
        }

        public List<clsVendHQProductList.Product> VendHqSetting(string BaseUrl, int StoreId, decimal tax,string ClientId, string ClientSecret, string RefreshToken)
        {
            string Url = "";
            clsVendHQProductList obj = new clsVendHQProductList();
            int recordsTotal = 200;
            List<clsVendHQProductList.Product> productList = new List<clsVendHQProductList.Product>(); ;
            var accesstoken = getaccesstoken(BaseUrl, ClientId, ClientSecret, RefreshToken);
            //BaseUrl ";
            clsVendHQProductList.Product ItemList= new clsVendHQProductList.Product();
            int page_size = 200;
            try
            {
                for (int pageNo = 0; pageNo <= recordsTotal-page_size; pageNo++)
                {
                    string ApiUrl = BaseUrl + "products"+"?page="+pageNo+"&page_size="+page_size+"";
                    ApiUrl = string.IsNullOrEmpty(Url) ? ApiUrl : Url;
                    var client = new RestClient(ApiUrl);
                    var request = new RestRequest(Method.GET);
                    request.AddHeader("Authorization", "Bearer " + accesstoken);
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/json");
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                       var content = response.Content;
                       var result = JsonConvert.DeserializeObject<clsVendHQProductList.Root>(content);
                       var  count = result.pagination.results.ToString();
                       var pagesize = result.pagination.page_size.ToString();
                        recordsTotal = Convert.ToInt32(count);
                        page_size = Convert.ToInt32(pagesize);
                        productList.AddRange(result.products.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " VENDHQ ");
            }
            return productList;
        }
   
        public class clsVendHQProductList
        {
            public class Pagination
            {
                public int results { get; set; }
                public int page { get; set; }
                public int page_size { get; set; }
                public int pages { get; set; }
            }

            public class PriceBookEntry
            {
                public string price_book_name { get; set; }
                public string id { get; set; }
                public string product_id { get; set; }
                public string price_book_id { get; set; }
                public string type { get; set; }
                public string outlet_name { get; set; }
                public string outlet_id { get; set; }
                public string customer_group_name { get; set; }
                public string customer_group_id { get; set; }
                public double price { get; set; }
                public object loyalty_value { get; set; }
                public string tax_id { get; set; }
                public double tax_rate { get; set; }
                public string tax_name { get; set; }
                public int display_retail_price_tax_inclusive { get; set; }
                public string min_units { get; set; }
                public string max_units { get; set; }
                public string valid_from { get; set; }
                public string valid_to { get; set; }
                public double tax { get; set; }
            }

            public class Tax
            {
                public string outlet_id { get; set; }
                public string tax_id { get; set; }
            }

            public class Inventory
            {
                public string outlet_id { get; set; }
                public string outlet_name { get; set; }
                public string count { get; set; }
                public string reorder_point { get; set; }
                public string restock_level { get; set; }
            }

            public class Product
            {
                public string id { get; set; }
                public string source_id { get; set; }
                public string handle { get; set; }
                public bool has_variants { get; set; }
                public string variant_parent_id { get; set; }
                public string variant_option_one_name { get; set; }
                public string variant_option_one_value { get; set; }
                public string variant_option_two_name { get; set; }
                public string variant_option_two_value { get; set; }
                public string variant_option_three_name { get; set; }
                public string variant_option_three_value { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public string description { get; set; }
                public string image { get; set; }
                public string image_large { get; set; }
                public List<object> images { get; set; }
                public string sku { get; set; }
                public string tags { get; set; }
                public string supplier_code { get; set; }
                public string supply_price { get; set; }
                public string account_code_purchase { get; set; }
                public string account_code_sales { get; set; }
                public string button_order { get; set; }
                public List<PriceBookEntry> price_book_entries { get; set; }
                public double price { get; set; }
                public double tax { get; set; }
                public string tax_id { get; set; }
                public double tax_rate { get; set; }
                public string tax_name { get; set; }
                public int display_retail_price_tax_inclusive { get; set; }
                public string updated_at { get; set; }
                public string deleted_at { get; set; }
                public string base_name { get; set; }
                public string brand_id { get; set; }
                public string variant_source_id { get; set; }
                public string brand_name { get; set; }
                public string supplier_name { get; set; }
                public bool track_inventory { get; set; }
                public List<Tax> taxes { get; set; }
                public string type { get; set; }
                public List<Inventory> inventory { get; set; }
            }

            public class Root
            {
                public Pagination pagination { get; set; }
                public string access_token { get; set; }
                public List<Product> products { get; set; }
            }
            public class VendHQProductModel
            {
                public int StoreID { get; set; }
                public string upc { get; set; }
                public decimal Qty { get; set; }
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
           public  class VendHQFullNameProductModel
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
        }
    }
}
