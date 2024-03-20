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

namespace ExtractPosData
{
    public class clsLightspeedAPI
    {
        int totalRecord = 1;

        public clsLightspeedAPI(int StoreId, decimal tax, string BaseUrl, string accessToken)
        {
            try
            {
                Console.WriteLine("Generating Lightspeed " + StoreId + " Product File....");
                Console.WriteLine("Generating Lightspeed " + StoreId + " Fullname File....");
                LightspeedSetting(BaseUrl, StoreId, tax, accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedAPI " + StoreId);
            }
        }
        private string LightspeedSetting(string BaseUrl, int StoreId, decimal tax, string accessToken)
        {
            totalRecord = getProductCount(BaseUrl, accessToken);
            List<Product> prod = new List<Product>();
            for (int i = 1; i <= totalRecord; i++)
            {
                var prodList = getProduct(i, BaseUrl, accessToken);
                if (prodList.Count != 0)
                {
                    prod.AddRange(prodList);
                }
                else
                {
                    break;
                }
            }
            GenerateCsvFile(StoreId, prod, tax);
            Console.WriteLine("Product File Generated For Lightspeed " + StoreId);
            Console.WriteLine("Fullname File Generated For Lightspeed " + StoreId);
            return "SUCCESS";
        }
        private int getProductCount(string BaseUrl, string accessToken)
        {
            int count = 0;
            try
            {
                var client = new RestClient(BaseUrl + "catalog/count.json");
                var request = new RestRequest(Method.GET);

                request.AddHeader("Authorization", accessToken);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                var result = JsonConvert.DeserializeObject<Count>(content);
                count = result.count;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedAPI ");
            }
            return count;
        }
        private List<Product> getProduct(int pageNo, string BaseUrl, string accessToken)
        {
            Example prod = new Example();
            try
            {
                var client = new RestClient(BaseUrl + "catalog.json?page=" + pageNo + "&Limit=250");
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", accessToken);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    prod = (Example)JsonConvert.DeserializeObject(response.Content, typeof(Example));
                }
                //totalRecord = totalRecord - 250;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " LightspeedAPI ");
            }
            return prod.products.ToList();
        }
        private void CreateCSVFromGenericList<T>(List<T> list, string csvNameWithExt)
        {
            if (list == null || list.Count == 0) return;

            //get type from 0th member
            Type t = list[0].GetType();
            string newLine = Environment.NewLine;

            using (var sw = new StreamWriter(csvNameWithExt))
            {
                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                foreach (PropertyInfo pi in props)
                {
                    sw.Write(pi.Name + ",");
                }
                sw.Write(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    //this acts as datacolumn
                    foreach (PropertyInfo pi in props)
                    {
                        //this is the row+col intersection (the value)
                        string whatToWrite =
                            Convert.ToString(item.GetType()
                                                 .GetProperty(pi.Name)
                                                 .GetValue(item, null))
                                .Replace("\n", string.Empty)
                                .Replace("\r\n", string.Empty)
                                .Replace("\r", string.Empty)
                                .Replace(',', ' ') + ',';

                        sw.Write(whatToWrite);

                    }
                    sw.Write(newLine);
                }
            }
        }
        private void GenerateCsvFile(int StoreId, List<Product> results, decimal tax)
        {
            try
            {
                var list = results.Distinct().ToList();
                List<ProductsModel> exProd = new List<ProductsModel>();
                List<clsFullnameModel> fullname = new List<clsFullnameModel>();

                List<ProductsModel> finalProd = new List<ProductsModel>();
                List<clsFullnameModel> finalFullname = new List<clsFullnameModel>();

                ProductsModel prod = new ProductsModel();
                clsFullnameModel fname = new clsFullnameModel();

                foreach (var item in results)
                {
                    prod = new ProductsModel();
                    fname = new clsFullnameModel();

                    prod.StoreID = StoreId;
                    prod.StoreDescription = item.title;
                    prod.StoreProductName = item.title;
                    fname.pname = item.title;
                    fname.pdesc = item.title;
                    prod.pack = 1;
                    if (item.categories.ToString().Length > 10)
                    {
                        foreach (var obj in JToken.Parse(item.categories.ToString()).ToList())
                        {
                            var CatItem = JsonConvert.DeserializeObject<CategoriesList>(obj.First.ToString());
                            if (CatItem.depth == 1)
                            {
                                fname.pcat = CatItem.title;
                            }
                            else if (CatItem.depth == 2)
                            {
                                fname.pcat1 = CatItem.title;
                            }
                            else
                            {
                                fname.pcat2 = CatItem.title;
                            }
                        }
                    }
                    else
                    {
                        fname.pcat = "";
                        fname.pcat1 = "";
                        fname.pcat2 = "";
                    }
                    if (item.variants.ToString().Length > 10)
                    {
                        foreach (var obj in JToken.Parse(item.variants.ToString()).ToList())
                        {
                            var variantsItem = JsonConvert.DeserializeObject<variants>(obj.First.ToString());
                            var upcsku = variantsItem.ean;
                            var sku = variantsItem.sku;
                            var upcPId = item.id;
                            if (!string.IsNullOrEmpty(upcsku))
                            {
                                prod.sku = '#' + upcsku;
                                prod.upc = '#' + upcsku;
                                fname.upc = '#' + upcsku;
                                fname.sku = '#' + upcsku;
                            }
                            else if (!string.IsNullOrEmpty(sku))
                            {
                                prod.sku = '#' + sku;
                                prod.upc = '#' + sku;
                                fname.upc = '#' + sku;
                                fname.sku = '#' + sku;
                            }
                            else if (upcPId > 0)
                            {
                                prod.sku = '#' + upcPId.ToString();
                                prod.upc = '#' + upcPId.ToString();
                                fname.upc = '#' + upcPId.ToString();
                                fname.sku = '#' + upcPId.ToString();
                            }
                            //else { continue; 
                            //}
                            var qty = variantsItem.stockLevel;
                            if (qty > 0)
                            {
                                prod.Qty = variantsItem.stockLevel;
                            }
                            else
                            {
                                //continue;
                            }
                            var prc = variantsItem.priceIncl;
                            if (prc > 0)
                            {
                                prod.Price = Convert.ToDecimal(prc);
                                fname.Price = Convert.ToDecimal(prc);
                            }
                            else
                            {
                                continue;
                            }
                            fname.uom = "";
                            fname.country = "";
                            fname.region = "";
                            prod.sprice = 0;
                            prod.Start = "";
                            prod.End = "";
                            prod.Tax = tax;
                            prod.altupc1 = "";
                            prod.altupc2 = "";
                            prod.altupc3 = "";
                            prod.altupc4 = "";
                            prod.altupc5 = "";
                            exProd.Add(prod);
                            fullname.Add(fname);
                        }
                    }
                }

                var prodList = (from a in exProd
                                select new
                                {
                                    storeid = a.StoreID,
                                    upc = a.upc,
                                    qty = a.Qty,
                                    sku = a.sku,
                                    pack = a.pack,
                                    StoreProductName = a.StoreProductName,
                                    StoreDescription = a.StoreDescription,
                                    price = a.Price,
                                    sprice = a.sprice,
                                    start = a.Start,
                                    end = a.End,
                                    tax = a.Tax,
                                    altupc1 = a.altupc1,
                                    altupc2 = a.altupc2,
                                    altupc3 = a.altupc3,
                                    altupc4 = a.altupc4,
                                    altupc5 = a.altupc5
                                }).Distinct().Select(x => new ProductsModel()
                                 {
                                     StoreID = x.storeid,
                                     upc = x.upc,
                                     Qty = x.qty,
                                     sku = x.sku,
                                     pack = x.pack,
                                     StoreProductName = x.StoreProductName,
                                     StoreDescription = x.StoreDescription,
                                     Price = x.price,
                                     sprice = x.sprice,
                                     Start = x.start,
                                     End = x.end,
                                     Tax=x.tax,
                                     altupc1 = x.altupc1,
                                     altupc2 = x.altupc2,
                                     altupc3 = x.altupc3,
                                     altupc4 = x.altupc4,
                                     altupc5 = x.altupc5
                                 }).ToList();

                var fullNameList = (from a in fullname
                                    select new
                                    {
                                        pname = a.pname,
                                        pdesc = a.pdesc,
                                        upc = a.upc,
                                        sku = a.sku,
                                        price = a.Price,
                                        uom = a.uom,
                                        pcat = a.pcat,
                                        pcat1 = a.pcat1,
                                        pcat2 = a.pcat2,
                                        country = a.country,
                                        region = a.region
                                    }).Distinct().Select(x => new clsFullnameModel()
                                {
                                    pname = x.pname,
                                    pdesc = x.pdesc,
                                    upc = x.upc,
                                    sku = x.sku,
                                    Price = x.price,
                                    uom = x.uom,
                                    pcat = x.pcat,
                                    pcat1 = x.pcat1,
                                    pcat2 = x.pcat2,
                                    country = x.country,
                                    region = x.region
                                }).ToList();

                string UploadPath = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + StoreId + "\\Upload\\" + "PRODUCT" + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                string UploadPathfull = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + StoreId + "\\Upload\\" + "FULLNAME" + StoreId + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv";
                if (StoreId == 10715 || StoreId == 10716 || StoreId == 10717)
                {
                    CreateCSVFromGenericList<ProductsModel>(prodList, UploadPath);
                    CreateCSVFromGenericList<clsFullnameModel>(fullNameList, UploadPathfull);
                }
                else
                {
                    CreateCSVFromGenericList<ProductsModel>(exProd, UploadPath);
                    CreateCSVFromGenericList<clsFullnameModel>(fullname, UploadPathfull);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public class variants
        {
            public int id { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public bool isDefault { get; set; }
            public int sortOrder { get; set; }
            public string articleCode { get; set; }
            public string ean { get; set; }
            public string sku { get; set; }
            public object hs { get; set; }
            public int tax { get; set; }
            public double priceExcl { get; set; }
            public double priceIncl { get; set; }
            public double priceCost { get; set; }
            public int oldPriceExcl { get; set; }
            public int oldPriceIncl { get; set; }
            public string stockTracking { get; set; }
            public int stockLevel { get; set; }
            public int stockAlert { get; set; }
            public int stockMinimum { get; set; }
            public int stockSold { get; set; }
            public int stockBuyMininum { get; set; }
            public int stockBuyMinimum { get; set; }
            public int stockBuyMaximum { get; set; }
            public int weight { get; set; }
            public int volume { get; set; }
            public int colli { get; set; }
            public int sizeX { get; set; }
            public int sizeY { get; set; }
            public int sizeZ { get; set; }
            public bool matrix { get; set; }
            public string title { get; set; }
        }
        public class Product
        {
            public int id { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public string visibility { get; set; }
            public int hits { get; set; }
            public string data01 { get; set; }
            public string data02 { get; set; }
            public string data03 { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public string fulltitle { get; set; }
            public string description { get; set; }
            public string content { get; set; }
            public object categories { get; set; }
            public object variants { get; set; }
        }
        public class Example
        {
            public List<Product> products { get; set; }
        }
        public class CategoriesList
        {
            public int id { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public bool isVisible { get; set; }
            public int depth { get; set; }
            public int sortOrder { get; set; }
            public string url { get; set; }
            public string title { get; set; }
        }
        public class Count
        {
            public int count { get; set; }
        }
    }
}
