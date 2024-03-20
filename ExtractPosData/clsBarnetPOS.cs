using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
//using System.Xaml;
using System.Web.Script.Serialization;
using System.IO;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using RestSharp.Authenticators;
using System.Net;
using System.Runtime.Serialization;
using System.Configuration;
using ExtractPosData.Model;
using ExtractPosData.Models;

namespace ExtractPosData
{
    class clsBarnetPOS
    {
        public List<JArray> products(int StoreId, decimal tax, string BaseUrl, string Username, string Password,  int ACCOUNTID, int SHOPID)
        {
            List<JArray> productList = new List<JArray>(); ;

            string authInfo = Username + ":" + Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            string content = null;
            Paginator obj = new Paginator();
            int PageNo;
            int Count = 1;
            for (PageNo = 1; PageNo <= Count; PageNo++)
            {
                BaseUrl = string.IsNullOrEmpty(obj.Url) ? BaseUrl : obj.Url;
                var client = new RestClient(BaseUrl + "store/products?p=" + PageNo + "");
                var request = new RestRequest(Method.GET);

                request.AddHeader("Authorization", "Basic " + authInfo);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("Accept", "application/json");

                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    content = response.Content;
                    var result = JsonConvert.DeserializeObject<clsProductList.items>(content);

                    var pJson = (dynamic)JObject.Parse(content);
                    var jArray = (JArray)pJson["items"];
                    var jArray2 = (JArray)pJson["Paginator"];
                    string Pagecount = content.ToString();
                    Pagecount = Pagecount.Split(':', ',')[6];
                    Pagecount = Pagecount.Replace("/", "");
                    Pagecount = Pagecount.Replace("}", "");
                    Count = Convert.ToInt32(Pagecount);
                    productList.Add(jArray);
                }
            }
            return productList;
        }
    }

    public class Barnet_Products
    {
        public Barnet_Products(int storeid, decimal tax, string BaseUrl, string Username, string Password, int ACCOUNTID, int SHOPID)
        {
            BarnetproductForCSV(storeid, tax, BaseUrl, Username, Password, ACCOUNTID, SHOPID);
        }
        public void BarnetproductForCSV(int storeid, decimal tax, string BaseUrl, string Username, string Password, int ACCOUNTID, int SHOPID)
        {
            try
            {
                clsBarnetPOS products = new clsBarnetPOS();
                var productList = products.products(storeid, tax, BaseUrl, Username, Password, ACCOUNTID, SHOPID);

                BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
                List<ProductsModel> pf = new List<ProductsModel>();
                List<FullnameModel> fn = new List<FullnameModel>();

                foreach (var item in productList)
                {
                    foreach (var itm in item)
                    {
                        ProductsModel pdf = new ProductsModel();
                        FullnameModel fnf = new FullnameModel();

                        pdf.StoreID = storeid;

                        decimal result;
                        string upc = itm["pid"].ToString();
                        Decimal.TryParse(upc, System.Globalization.NumberStyles.Float, null, out result);
                        upc = result.ToString();

                        if (upc == "" || upc == "0")
                        {
                            pdf.upc = "";

                            fnf.upc = "";
                        }
                        else
                        {
                            pdf.upc = "#" + upc;

                            fnf.upc = "#" + upc;
                        }
                        string sku = itm["cspcid"].ToString();
                        if (sku == "")
                        {
                            fnf.sku = "";
                            fnf.sku = "";
                        }
                        else
                        {
                            pdf.sku = "#" + sku;
                            fnf.sku = "#" + sku;
                        }
                        var qty = Convert.ToString(itm["on_hand"]);
                        if (!string.IsNullOrEmpty(qty))
                        {
                            var qty1 = Convert.ToDecimal(qty);
                            pdf.Qty = Convert.ToInt64(qty1);
                        }
                        pdf.pack = 1;
                        pdf.StoreProductName = itm["description"].ToString();
                        pdf.StoreDescription = itm["description"].ToString();
                        pdf.Price = Convert.ToDecimal(itm["net_price"]);
                        pdf.sprice = Convert.ToDecimal(itm["sale_price"]);
                        if (pdf.sprice > 0)
                        {
                            pdf.Start = DateTime.Now.ToString("MM/dd/yyyy");
                            pdf.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                        }
                        else
                        {
                            pdf.Start = "";
                            pdf.End = "";
                        }
                        pdf.Tax = tax;
                        pdf.altupc1 = "";
                        pdf.altupc2 = "";
                        pdf.altupc3 = "";
                        pdf.altupc4 = "";
                        pdf.altupc4 = "";
                        pdf.altupc5 = "";

                        fnf.pname = itm["description"].ToString();
                        fnf.pdesc = itm["description"].ToString();
                        fnf.Price = Convert.ToDecimal(itm["net_price"]);
                        fnf.uom = itm["unit_name"].ToString();
                        fnf.pack = 1;
                        fnf.pcat = itm["group_name"].ToString();
                        fnf.pcat1 = itm["category_name"].ToString();
                        fnf.pcat2 = "";
                        fnf.country = "";
                        fnf.region = "";

                            if (pdf.Price > 0 && pdf.upc != "" && fnf.pcat != "CIGARS" && fnf.pcat != "CIGARETTES" && fnf.pcat != "Cigarette")
                            {
                                pf.Add(pdf);
                                fn.Add(fnf);
                            }
                            pf = pf.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                            fn = fn.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                        
                    }
                }
                GenerateCSV.GenerateCSVFile(pf, "PRODUCT", storeid, BaseUrl);
                GenerateCSV.GenerateCSVFile(fn, "FULLNAME", storeid, BaseUrl);
                Console.WriteLine("Product file is Generated for BarnetPos" + " " + storeid);
                Console.WriteLine("FullName File is Generated for BarnetPos" + " " + storeid);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class Paginator
    {
        public int items_count { get; set; }
        public int page { get; set; }
        public int pages { get; set; }
        public string Url { get; set; }
    }

    public class Item
    {
        public string unit_name { get; set; }
        public bool is_featured { get; set; }
        public string cspcid { get; set; }
        public string item_province { get; set; }
        public bool is_staff_picks { get; set; }
        public string image { get; set; }
        public string pid { get; set; }
        public string group_name { get; set; }
        public object accolades_info { get; set; }
        public int accolades { get; set; }
        public object producer_name { get; set; }
        public string reserved { get; set; }
        public string on_hand { get; set; }
        public object sweetness_name { get; set; }
        public string base_cost { get; set; }
        public int id { get; set; }
        public bool is_new_item { get; set; }
        public object discount_date_finish { get; set; }
        public object alcohol { get; set; }
        public object discount_date_start { get; set; }
        public object import_ind { get; set; }
        public string net_price { get; set; }
        public object type_name { get; set; }
        public bool is_best_seller { get; set; }
        public string country_name { get; set; }
        public string sale_price { get; set; }
        public object discount_amount { get; set; }
        public string varital_name { get; set; }
        public object status { get; set; }
        public object multiple { get; set; }
        public string description { get; set; }
        public string price_before_discount { get; set; }
        public string specialty { get; set; }
        public object cont_type { get; set; }
        public string more_info { get; set; }
        public object litre_per_unit { get; set; }
        public string unit_group { get; set; }
        public bool is_special_item { get; set; }
        public object size_rate_for_singles { get; set; }
        public bool tax1 { get; set; }
        public bool tax3 { get; set; }
        public bool tax2 { get; set; }
        public object effective_date { get; set; }
        public bool is_sale { get; set; }
        public bool tax4 { get; set; }
        public string category_name { get; set; }
        public string more_info_html { get; set; }
        public object distributor_name { get; set; }
        public object status1 { get; set; }
        public object status2 { get; set; }
        public string wine_series { get; set; }
        public object vendor_name { get; set; }
        public string deposit_price { get; set; }
        public object wine_name { get; set; }
        public bool show_on_web { get; set; }
        public bool is_my { get; set; }
        public object distributor_id { get; set; }
        public object sweetness { get; set; }
        public bool tax_included { get; set; }
    }

    public class Root
    {
        public Paginator paginator { get; set; }
        public List<Item> items { get; set; }
    }
}
