using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
//using System.Web.Mvc;
//using System.Reflection;
using RestSharp;
using Newtonsoft.Json;
//using WhiskeyStore.Models;
using System.Reflection;
using System.IO;
using System.Configuration;
using System.Net;
using ExtractPosData.Models;
namespace ExtractPosData
{
   public class clsBindo
    {

        public int curentpage = 1;
        public int totalpages = 1;
        //
        // GET: /BINDOPOS/
        string baseUrl = ConfigurationManager.AppSettings["BindoApiUrl"];

        public clsBindo(int StoreId,string ApiKey)
        {
          string val=  BindoSettings(StoreId, ApiKey);
        }

        public string BindoSettings(int StoreId,string ApiKey)
        {
            string accessToken = ApiKey; 
            string slug = getStoreSlug(accessToken,StoreId);
            List<prdlistings> prd = new List<prdlistings>();
            Console.WriteLine("Generating Bindo " + StoreId + " Product file....");
            try
            {
                for (int i = 1; i <= totalpages; i++)
                {
                    prd.AddRange(getProducts(slug, accessToken, i));
                }
                string storeid = StoreId.ToString();
                
                GenerateCSVFiles(storeid, prd);
                Console.WriteLine("Product File Generated For Bindo "+storeid+"");
                Console.WriteLine("Fullname File Generated For Bindo "+storeid+"");
                return "success";
            }
            catch (Exception e)
            { 
            return "failed"+e.Message;
            }
            //return "success";
        }
        public void GenerateCSVFiles(string storeid,List<prdlistings> products)
        {
            List<ExportProducts> expprd = new List<ExportProducts>();
            ExportProducts prd = new ExportProducts();

            List<Bindofullname> fullprd = new List<Bindofullname>();
            Bindofullname fullprod = new Bindofullname();

            foreach (prdlistings item in products)
            {
                try
                {
                    prd = new ExportProducts();
                    fullprod = new Bindofullname();
                    if (item.deleted == false && item.discontinued == false && item.in_store_only == false)
                    {
                        prd.storeid = storeid;
                        prd.StoreProductName = item.name;
                        prd.Storedescription = item.name;
                        fullprod.pname = item.name;
                        fullprod.pdesc = item.name;
                        fullprod.pcat = item.category_name;
                        // prd.uom = item.
                        //if (item.description != null)
                        //{
                        //    prd.Storedescription = item.description;
                        //}
                        //else
                        //{
                        //    prd.Storedescription = item.name;
                        //}
                        prd.pack = 1;
                        fullprod.pack = 1;
                        prd.sku = "#" + Convert.ToString(item.product_id).Trim();
                        fullprod.sku = "#" + Convert.ToString(item.product_id).Trim();
                        prd.price = item.price;
                        fullprod.Price = item.price;
                        prd.tax = item.tax_rate;
                        if (item.quantity != null)
                        {
                            prd.qty = Convert.ToInt32(item.quantity);
                        }
                        else
                        {
                            prd.qty = 0;
                        }
                        prd.upc = "";
                        prd.altupc1 = "";

                        if (storeid == "10117")
                        {

                            if (item.upc != null)
                            {
                                if (item.upc.Trim() != "")
                                {
                                    prd.upc = "#" + item.upc;
                                    fullprod.upc = "#" + item.upc;
                                }
                                else
                                {
                                    if (item.ean13 != null)
                                    {
                                        if (item.ean13.Trim() != "")
                                        {
                                            prd.upc = "#" + item.ean13;
                                            fullprod.upc = "#" + item.ean13;
                                        }
                                    }
                                }
                            }
                            if (item.ean13 != null)
                            {
                                if (item.ean13.Trim() != "")
                                {
                                    prd.altupc1 = "#" + item.ean13;
                                }
                            }
                        }

                        else
                        {
                            if (item.upc != null)
                            {
                                if (item.upc.Trim() != "")
                                {
                                    prd.upc = "#" + item.upc;
                                    fullprod.upc = "#" + item.upc;
                                }
                            }
                            else if (item.ean13 != null)
                            {
                                if (item.ean13.Trim() != "")
                                {
                                    prd.upc = "#" + item.ean13;
                                    fullprod.upc = "#" + item.ean13;
                                }
                            }
                            else
                            {
                                prd.upc = "#" + item.secondary_barcode;
                                fullprod.upc = "#" + item.secondary_barcode;
                            }
                            //}

                            if (item.ean13 != null)
                            {
                                if (item.ean13.Trim() != "")
                                {
                                    prd.altupc1 = "#" + item.ean13;
                                }
                            }
                        }
                        if (item.web_price != null)
                        {
                            if (item.web_price != 0)
                            {
                                prd.sprice = Convert.ToDecimal(item.web_price);
                                prd.start = DateTime.Now.Date.ToShortDateString();
                                prd.end = DateTime.Now.Date.ToShortDateString();
                            }
                        }
                        if (prd.upc.Trim() != "" || prd.upc.Trim() != "#")
                        {
                            expprd.Add(prd);
                            fullprd.Add(fullprod);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
          

            string UploadPath = ConfigurationManager.AppSettings["BaseDirectory"] +"\\"+ storeid + "\\Upload\\" + "product" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
            string UploadPath2 = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\" + "FULLNAME" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";

            CreateCSVFromGenericList<ExportProducts>(expprd, UploadPath);
            CreateCSVFromGenericList2<Bindofullname>(fullprd, UploadPath2);

        }

        public string getStoreSlug(string accessToken, int storeid)
        {
            var client = new RestClient(baseUrl + "/me/stores");
            var request = new RestRequest(Method.GET);

            request.AddHeader("Authorization", "OAuth " + accessToken);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            string slug = "";
            ClsBindo store = (ClsBindo)JsonConvert.DeserializeObject(response.Content, typeof(ClsBindo));
            if (storeid == 11121)
            {
                 slug= store.data.stores[1].slug;
            }
            else {
                slug = store.data.stores[0].slug;
            }
            return slug;
        }

        public List<prdlistings> getProducts(string slug, string accessToken, int pageno)
        {
            var client = new RestClient(baseUrl + "/stores/"+slug+"/listings");
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "OAuth " + accessToken);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("Accept", "application/vnd.bindo-v201501+json");
            request.AddParameter("page", pageno);
            request.AddParameter("per_page", 1000);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            clsProducts store = (clsProducts)JsonConvert.DeserializeObject(response.Content, typeof(clsProducts));

            totalpages = store.paging.total_pages;

            return store.data.listings;

            //string result = response.Content;
            //result = result.Substring(result.IndexOf('['));
            //result = result.Substring(0, result.IndexOf(']') + 1);
            //return result;
        }


        public string getCategories( string accessToken)
        {
            /// for store details
//          var client = new RestClient(baseUrl + "/me/stores");

            // for departments- categories
          //  var client = new RestClient(baseUrl + "/stores/sow/departments");

            // for products list
            var client = new RestClient(baseUrl + "/stores/sow/listings");
            var request = new RestRequest(Method.GET);

            request.AddHeader("Authorization", "OAuth " + accessToken ); 
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
//            request.AddHeader("per_page", "1000");
  //          request.AddHeader("", "1");

            IRestResponse response = client.Execute(request);

            string result = response.Content;
            result = result.Substring(result.IndexOf('['));
            result = result.Substring(0, result.IndexOf(']') + 1);
            return result;
        }


        public string gettocken()
        {
            string appsecret = "thu1ajduaz86a04w1an722916qgbbc2";
            string clientid = "hy4mr3e9l80181ywdeqm7h170zl4uuw";
            string uid = "bottlecapps";
            string pwd = "M269EqaPeTYY6q";
            string baseUrl1 = baseUrl + "/oauth/authorize";
            var client = new RestClient(baseUrl1);
            var request = new RestRequest(Method.POST);


            string str = "username=" + uid + "&password=" + pwd + "&grant_type=password&client_id=" + clientid + "&client_secret=" + appsecret;

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", str, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            dynamic responseData = JsonConvert.DeserializeObject(response.Content);
            string tokenvalue = responseData["access_token"].Value;
            return tokenvalue;
        }

        public static void CreateCSVFromGenericList<T>(List<T> list, string csvNameWithExt)
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
                    sw.Write(pi.Name.ToUpper() + ",");
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
            
        public static void CreateCSVFromGenericList2<T>(List<T> list, string csvNameWithExt)
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
                    sw.Write(pi.Name.ToUpper() + ",");
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
                                .Replace("\n",string.Empty)
                                .Replace("\r\n", string.Empty)
                                .Replace("\r",string.Empty)
                                .Replace(',', ' ') + ',';

                        sw.Write(whatToWrite);

                    }
                    sw.Write(newLine);
                }
            }
        }
    }


   public class ClsBindo
   {
       public metastore meta { get; set; }
       public storedata data { get; set; }


   }

   public class metastore
   {
       public string code { get; set; }
   }

   public class storedata
   {
       public List<clsStore> stores { get; set; }
   }

   public class clsStore
   {
       public Int32 id { get; set; }
       public string slug { get; set; }
       public string title { get; set; }
       public string category { get; set; }
       public string image_url { get; set; }
       public string logo_url { get; set; }
       public Boolean transactionless_enable { get; set; }
   }

   public class clsProducts
   {
       public metastore meta { get; set; }
       public prdpaging paging { get; set; }
       public prddata data { get; set; }
   }

   public class prddata
   {
       public List<prdlistings> listings { get; set; }
   }


   public class prdpaging
   {
       public Int32 per_page { set; get; }
       public Int32 current_page { set; get; }
       public Int32 total_pages { set; get; }
       public Int32 total_entries { set; get; }
   }

   public class prdlistings
   {
       public Int32 product_id { set; get; }
       public string blid { set; get; }
       public Decimal price { set; get; }
       public Decimal? quantity { set; get; }
       public Boolean? track_quantity { set; get; }
       public Decimal tax_rate { set; get; }
       public string name { set; get; }
       public string barcode { set; get; }
       public string secondary_barcode { set; get; } 
       public string description { set; get; }
       public Boolean? in_store_only { set; get; }
       public string upc { set; get; }
       public string ean13 { set; get; }
       public string listing_barcode { set; get; }
       public Boolean? deleted { set; get; }
       public Boolean? discontinued { set; get; }
       public string category_id { set; get; }
       public string category_name { set; get; }
       public Decimal? web_price { get; set; }
   }

   public class ExportProducts
   {
       public string storeid { set; get; }
       public string upc { set; get; }
       public long qty { set; get; }
       public string sku { set; get; }
       public Int32 pack { set; get; }
       public string uom { set; get; }
       public string StoreProductName { set; get; }
       public string Storedescription { set; get; }
       public decimal price { set; get; }
       public decimal sprice { set; get; }
       public string start { set; get; }
       public string end { set; get; }
       public decimal tax { set; get; }
       public string altupc1 { set; get; }
       public string altupc2 { set; get; }
       public string altupc3 { set; get; }
       public string altupc4 { set; get; }
       public string altupc5 { set; get; }
       public string CategoryId { get; set; }
   }

   public class Bindofullname
   {
       public string pname { get; set; }
       public string pdesc { get; set; }
       public string upc { get; set; }
       public string sku { get; set; }
       public decimal Price { get; set; }
       public string uom { get; set; }
       public Int32 pack { get; set; }
       public string pcat { get; set; }
       public string pcat1 { get; set; }
       public string pcat2 { get; set; }
       public string country { get; set; }
       public string region { get; set; }
   }
}

   

