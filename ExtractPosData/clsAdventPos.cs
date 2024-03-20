using ExtractPosData.Models;
using Newtonsoft.Json;
//using System.Xaml;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ExtractPosData
{
    public class clsAdventPos
    {
        public List<JArray> products(int StoreId, decimal tax, string BaseUrl, string Username, string Password, string Pin)
        {
            List<JArray> productList = new List<JArray>();

            string authInfo = Username + ":" + Password + ":" + Pin;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            string content = null;
            clsProductList obj = new clsProductList();

            BaseUrl = string.IsNullOrEmpty(obj.Url) ? BaseUrl : obj.Url;
            var client = new RestClient(BaseUrl);
            var request = new RestRequest(Method.GET);

            request.AddHeader("Authorization", "Basic " + authInfo);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Accept", "application/json");

            IRestResponse response = client.Execute(request);
            
            
            content = response.Content;
            var result = JsonConvert.DeserializeObject<clsProductList.items>(content);

            var pJson = (dynamic)JObject.Parse(content);
            var jArray = (JArray)pJson["Data"];
            productList.Add(jArray);

            return productList;
        }
    }

    public class CsvProducts
    {
        // string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        public CsvProducts(int storeid, decimal tax, string BaseUrl, string Username, string Password, string Pin)
        {
            productForCSV(storeid, tax, BaseUrl, Username, Password, Pin);
        }
        public void productForCSV(int storeid, decimal tax, string BaseUrl, string Username, string Password, string Pin)
        {
            try
            {
                clsAdventPos products = new clsAdventPos();
                var productList = products.products(storeid, tax, BaseUrl, Username, Password, Pin);

                BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
                List<datatableModel> pf = new List<datatableModel>();

                foreach (var item in productList)
                {
                    foreach (var itm in item)
                    {

                        datatableModel pdf = new datatableModel();
                        pdf.StoreID = storeid;
                        decimal result;
                        string upc = itm["UPC"].ToString();
                        Decimal.TryParse(upc, System.Globalization.NumberStyles.Float, null, out result);
                        upc = result.ToString();

                        if (upc == "" || upc == "0")
                        {
                            pdf.upc = "";
                        }
                        else
                        {
                            pdf.upc = upc;
                        }
                        string sku = itm["SKU"].ToString();

                        pdf.sku = sku;
                        pdf.Qty = Convert.ToInt32(itm["TotalQty"]);
                        pdf.pack = 1;
                        pdf.StoreProductName = itm["ItemName"].ToString();
                        pdf.StoreDescription = itm["ItemName"].ToString();
                        pdf.Price = Convert.ToDecimal(itm["Price"]);
                        pdf.sprice = Convert.ToDecimal(itm["SALEPRICE"]);
                        pdf.Start = "";
                        pdf.End = "";
                        pdf.Tax = tax;
                        pdf.altupc1 = itm["ALTUPC2"].ToString();
                        pdf.altupc2 = itm["ALTUPC1"].ToString(); 
                        pdf.altupc3 = "";
                        pdf.altupc4 = "";
                        pdf.altupc5 = "";
                        pdf.uom = itm["SizeName"].ToString();
                        pdf.pcat = itm["Department"].ToString();

                        pf.Add(pdf);
                    }
                    Datatabletocsv csv = new Datatabletocsv();
                    csv.Datatablecsv(storeid, tax, pf);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + storeid);
            }
        }
    }
    public class clsProductList
    {
        public bool StatusVal { get; set; }
        public int StatusCode { get; set; }
        public string StatusMsg { get; set; }
        public string Price { get; set; }
        public string SessionID { get; set; }

        public string Url { get; set; }
        public class Data
        {
            public string UPC { get; set; }
            public int SKU { get; set; }
            public string ItemName { get; set; }
            public double Price { get; set; }
            public double Cost { get; set; }
            public double SALEPRICE { get; set; }
            public string SizeName { get; set; }
            public object PackName { get; set; }
            public string Vintage { get; set; }
            public string Department { get; set; }
            public double PriceA { get; set; }
            public double PriceB { get; set; }
            public double PriceC { get; set; }
            public double TotalQty { get; set; }
            public string ALTUPC1 { get; set; }
            public string ALTUPC2 { get; set; }
            public int STORECODE { get; set; }
        }

        public class items
        {
            public List<Data> item { get; set; }
        }
    }
    public class datatableModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string uom { get; set; }
        public string pcat { get; set; }
        public string pcat1 { get; set; }
        public string pcat2 { get; set; }
        public string country { get; set; }
        public string region { get; set; }
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
    public class ListtoDataTableConverter
    {
        public DataTable ToDataTable<T>(List<T> items, int StoreId)
        {
            DataTable dt = new DataTable(typeof(T).Name);

            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {
                dt.Columns.Add(prop.Name);
            }

            foreach (T item in items)
            {
                var values = new object[Props.Length];

                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dt.Rows.Add(values);
            }
            return dt;
        }
    }
    public class Datatabletocsv
    {
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public void Datatablecsv(int storeid, decimal tax, List<datatableModel> dtlist)
        {
            try
            {
                ListtoDataTableConverter cvr = new ListtoDataTableConverter();

                DataTable dt = cvr.ToDataTable(dtlist, storeid);
                var dtr = from s in dt.AsEnumerable() select s;
                List<ProductsModel> prodlist = new List<ProductsModel>();
                List<FullNameProductModel> full = new List<FullNameProductModel>();

                dynamic upcs;
                dynamic taxs;
                int barlenth = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    ProductsModel pmsk = new ProductsModel();
                    FullNameProductModel fname = new FullNameProductModel();
                    dt.DefaultView.Sort = "sku";
                    upcs = dt.DefaultView.FindRows(dr["sku"]).ToArray();
                    barlenth = ((Array)upcs).Length;
                    pmsk.StoreID = storeid;

                    if (barlenth > 0)
                    {
                        for (int i = 0; i <= barlenth - 1; i++)
                        {
                            if (i == 0)
                            {
                                if (!string.IsNullOrEmpty(dr["upc"].ToString()))
                                {
                                    var upc = "#" + upcs[i]["upc"].ToString().ToLower();
                                    string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                    if(storeid == 10708)
                                    {
                                        if (!string.IsNullOrEmpty(numberUpc))
                                        {
                                            pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                            fname.upc = "#" + numberUpc.Trim().ToLower();
                                        }
                                    }

                                     else if (numberUpc.Count() >= 7 && storeid !=10708)
                                        {
                                            if (!string.IsNullOrEmpty(numberUpc))
                                            {
                                                pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                fname.upc = "#" + numberUpc.Trim().ToLower();
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            if (i == 1)
                            {
                                pmsk.altupc1 = "#" + upcs[i]["upc"];
                            }
                            if (i == 2)
                            {
                                pmsk.altupc2 = "#" + upcs[i]["upc"];
                            }
                            if (i == 3)
                            {
                                pmsk.altupc3 = "#" + upcs[i]["upc"];
                            }
                            if (i == 4)
                            {
                                pmsk.altupc4 = "#" + upcs[i]["upc"];
                            }
                            if (i == 5)
                            {
                                pmsk.altupc5 = "#" + upcs[i]["upc"];
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(dr["sku"].ToString()))
                    {
                        pmsk.sku = "#" + dr["sku"].ToString();
                        fname.sku = "#" + dr["sku"].ToString();
                    }
                    else
                    { continue; }


                    pmsk.Qty = Convert.ToInt32(dr["Qty"]);

                    pmsk.StoreProductName = dr.Field<string>("StoreProductName").Trim();
                    pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
                    fname.pdesc = dr.Field<string>("StoreProductName").Trim();
                    fname.pname = dr.Field<string>("StoreProductName").Trim();

                    pmsk.Price = System.Convert.ToDecimal(dr["Price"].ToString());
                    fname.Price = System.Convert.ToDecimal(dr["Price"].ToString());

                    if (storeid == 10708)
                    {
                        pmsk.altupc1 = "#" + dr.Field<string>("altupc1").Trim();
                        pmsk.altupc2 = "#" + dr.Field<string>("altupc2").Trim();
                    }

                    pmsk.sprice = System.Convert.ToDecimal(dr["sprice"].ToString());
                    pmsk.pack = 1;
                    pmsk.Tax = Convert.ToDecimal(dr["Tax"]);
                    if (pmsk.sprice > 0)
                    {
                        pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                        pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                    }
                    else
                    {
                        pmsk.Start = "";
                        pmsk.End = "";
                    }
                    fname.pcat = dr.Field<string>("pcat");
                    fname.pcat1 = dr.Field<string>("pcat1");
                    fname.pcat2 = "";
                    fname.pack = 1;
                    pmsk.uom = dr.Field<string>("uom");
                    fname.uom = dr.Field<string>("uom");
                    fname.region = "";
                    fname.country = "";

                    if (storeid == 10826)
                    {
                        if (pmsk.Price > 0 && pmsk.Qty > 0 && !string.IsNullOrEmpty(pmsk.upc) && fname.pcat != "Cigarette" && fname.pcat != "Lotto" && fname.pcat != "Cigarettes" && fname.pcat != "Cigars" && fname.pcat != "TOBACCO" && fname.pcat != "CIG" && fname.pcat != "CIGARILLOS" && fname.pcat != "CIGAR" && fname.pcat != "CIGERATT" && fname.pcat != "CIGERATTE" && fname.pcat != "Tobacco")
                        {
                            prodlist.Add(pmsk);
                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                            prodlist = prodlist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                            full.Add(fname);
                            full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                            full = full.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                        }
                    }
                    else
                    {
                        if (pmsk.Price > 0 && !string.IsNullOrEmpty(pmsk.upc) && fname.pcat != "Cigarette" && fname.pcat != "Lotto" && fname.pcat != "Cigarettes" && fname.pcat != "Cigars" && fname.pcat != "TOBACCO" && fname.pcat != "CIG" && fname.pcat != "CIGARILLOS" && fname.pcat != "CIGAR" && fname.pcat != "CIGERATT" && fname.pcat != "CIGERATTE" && fname.pcat != "Tobacco")
                        {
                            prodlist.Add(pmsk);
                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                            prodlist = prodlist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                            full.Add(fname);
                            full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                            full = full.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                        }
                    }
                }
                Console.WriteLine("Generating ADVENTPOS " + storeid + " Product CSV Files.....");
                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeid, BaseUrl);
                Console.WriteLine("Product File Generated For ADVENTPOS " + storeid);
                Console.WriteLine();
                Console.WriteLine("Generating ADVENTPOS " + storeid + " Fullname CSV Files.....");
                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", storeid, BaseUrl);
                Console.WriteLine("Fullname File Generated For ADVENTPOS " + storeid);
            }
            catch (Exception e)
            {
                Console.WriteLine("" + e.Message + storeid);
            }
        }
    }
}
