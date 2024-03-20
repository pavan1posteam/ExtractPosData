using ExtractPosData.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace ExtractPosData
{
    public class clsCloverPos : clsBOClover
    {

        string constr = ConfigurationManager.AppSettings.Get("LiquorAppsConnectionString");
        string baseUrl = ConfigurationManager.AppSettings["CloverBaseURL"];
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        StoreSetting ss = new StoreSetting();
        int storeId;

        public clsCloverPos(int StoreId, string MerchantId, string TokenId, string ClientId, string Code, string InStock, List<categories> Category)
        {
            Console.WriteLine("Generating Product File of Clover " + StoreId);
            string val = CloverSettings(StoreId, MerchantId, ClientId, TokenId, Code, InStock, Category);
            if (!string.IsNullOrEmpty(val))
            {
                Console.WriteLine("Product File Generated for Clover " + StoreId);
            }
            else
            {
                Console.WriteLine("Product File Not Generated For Clover " + StoreId);
            }
            storeId = StoreId;
        }
        public string CloverSettings(int StoreId, string MerchantId, string ClientId, string TokenId, string Code, string InStock, List<categories> Category)
        {
            clsBOCloverStoreSettings cloverposserttings = new clsBOCloverStoreSettings();
            categories cats = new categories();
            //cloverposserttings = JsonConvert.DeserializeObject<clsBOCloverStoreSettings>();
            List<categories> ExistCategories = cloverposserttings.categories;
            cloverposserttings.categories = new List<categories>();

            string catjson = getCategories(MerchantId, TokenId, StoreId);
            if (!string.IsNullOrEmpty(catjson))
            {
                List<categories> cat = (List<categories>)JsonConvert.DeserializeObject(catjson, typeof(List<categories>));
                cat.Add(new categories { id = "Other", name = "Other", selected = false, taxrate = 0 });
                foreach (var item in cat)
                {
                    //categories cats = new categories();
                    if (ExistCategories != null)
                    {
                        var findcat = ExistCategories.Where(m => m.id == item.id).FirstOrDefault();
                        if (findcat != null)
                        {
                            cats.taxrate = findcat.taxrate;
                            cats.selected = true;
                        }
                    }
                    cats.id = item.id;
                    cats.name = item.name;
                    if (item.id != "AKGXX4R4H9YP2")
                    {
                        cloverposserttings.categories.Add(cats);
                    }
                }


                //clsBOCloverStoreSettings cloverposserttings = new clsBOCloverStoreSettings();
                cloverposserttings.merchantid = MerchantId;
                cloverposserttings.clientid = ClientId;
                cloverposserttings.code = Code;
                cloverposserttings.tokenid = TokenId;
                cloverposserttings.instock = InStock;

                //cloverposserttings.categories = new List<categories>();

                foreach (var item in cloverposserttings.categories)
                {
                    if (item.selected)
                    {
                        //categories cat = new categories();
                        cats.id = item.id;
                        //cat.name = item.name;
                        //cat.selected = item.selected;
                        cats.taxrate = item.taxrate;
                        cloverposserttings.categories.Add(cats);
                    }
                }
                JsonSerializer serializer = new JsonSerializer();
                string cloversettings = JsonConvert.SerializeObject(cloverposserttings);
                //StoreAddress st = new StoreAddress();
                //st.UpdatePosSettings(StoreId, "CLOVER", cloversettings);
                string filename = GenerateCSVFiles(StoreId.ToString(), cloverposserttings);


                return filename;
            }
            else
            { return ""; }
        }

        public string getCategories(string merchant_id, string accessToken, int StoreId)
        {
            Thread.Sleep(1000);
            var client = new RestClient(baseUrl + "/v3/merchants/" + merchant_id + "/categories?limit=100&access_token=" + accessToken);
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            try
            {
                if (response.StatusCode.ToString().ToUpper() == "UNAUTHORIZED")
                {
                    Exception e = new Exception();
                    (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in " + StoreId + " ExtractPOS@" + DateTime.UtcNow + " GMT", "StatusCode:Unauthorized Response" + "<br/>" + e.StackTrace);
                }
                else
                {
                    string result = response.Content;
                    result = result.Substring(result.IndexOf('['));
                    result = result.Substring(0, result.IndexOf(']') + 1);
                    //var path = @"C:\Workspace\result_cat.txt";
                    //File.WriteAllText(path, result.ToString());
                    return result;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
            return "";
        }
        public string gettocken(string clientid, string code)
        {
            string appsecret = ConfigurationManager.AppSettings["CloverAPPSecrete"];
            string baseUrl1 = baseUrl + "/oauth/token";
            var client = new RestClient(baseUrl1);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("authorization", "Bearer <access_token>");
            request.AddHeader("TokenExpiry", "100");

            string str = "client_id=" + clientid + "&client_secret=" + appsecret + "&code=" + code;


            request.AddParameter("application/x-www-form-urlencoded", str, ParameterType.RequestBody);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);
            dynamic responseData = JsonConvert.DeserializeObject(response.Content);
            string tokenvalue = responseData["access_token"].Value;
            return tokenvalue;

        }
        public T Load<T>(string jsonstr)
        {
            return JsonConvert.DeserializeObject<T>(jsonstr);
        }

        public void SaveRequestResponse(string jsonStringReq, string CLOVERJsonResp, string ErrorMessage, string storeid)
        {
            DataSet dsResult = new DataSet();

            List<SqlParameter> sItemParams = new List<SqlParameter>();
            try
            {

                sItemParams.Add(new SqlParameter("@PosName", "CLOVER"));
                sItemParams.Add(new SqlParameter("@PosRequest", jsonStringReq));
                sItemParams.Add(new SqlParameter("@PosResponse", CLOVERJsonResp));
                sItemParams.Add(new SqlParameter("@ErrorMessage", ErrorMessage));
                sItemParams.Add(new SqlParameter("@StoreID", storeid));

                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.CommandText = "usp_bc_Clover_PosReqResInsert";
                        cmd.CommandTimeout = 3600;
                        foreach (SqlParameter par in sItemParams)
                        {
                            cmd.Parameters.Add(par);
                        }
                        using (SqlDataAdapter da = new SqlDataAdapter())
                        {
                            da.SelectCommand = cmd;
                            da.Fill(dsResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            { }
        }
        public string GenerateCSVFiles(string storeid, clsBOCloverStoreSettings settings)
        {
            try
            {
                string merchant_id = settings.merchantid;
                string accessToken = settings.tokenid;

                decimal deftax = 0;
                var client1 = new RestClient(baseUrl + "/v3/merchants/" + merchant_id + "/tax_rates?limit=100&access_token=" + accessToken);
                var request1 = new RestRequest(Method.GET);
                request1.AddHeader("cache-control", "no-cache");
                request1.AddHeader("content-type", "application/x-www-form-urlencoded");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response1 = client1.Execute(request1);
                string result1 = response1.Content;

                Tax ptax = Load<Tax>(result1);

                var vtaxrate = ptax.elements.Where(t => t.isDefault = true).Select(t => t.rate).FirstOrDefault();
                if (vtaxrate != 0)
                {
                    deftax = vtaxrate / Convert.ToDecimal(100000);
                    deftax /= 100;
                }

                List<ExportProducts> expprd = new List<ExportProducts>();
                parentItems items = new parentItems();


                int offset = 0;
                //string cat = "";
                Decimal taxrate = 0;
                string strSelectedCat = string.Join(",", settings.categories.Select(x => x.id));
                offset = 0;
                for (int i = 0; i <= 100000; i++)
                {
                    try
                    {
                        if (i != 0)
                        {
                            offset = 1000;
                            offset = (offset * i) + 1;
                        }
                        else
                        {
                            offset = 0;
                        }
                        string StoreID = storeid.ToString();
                        //retrieving sku,upc
                        var client = new RestClient(baseUrl + "/v3/merchants/" + merchant_id + "/items?expand=itemStock,taxRates,categories&offset=" + offset.ToString() + "&limit=1000&access_token=" + accessToken);
                        var request = new RestRequest(Method.GET);
                        request.AddHeader("cache-control", "no-cache");
                        request.AddHeader("content-type", "application/x-www-form-urlencoded");
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        IRestResponse response = client.Execute(request);
                        string result = response.Content;
                        if (response.StatusCode.ToString().ToUpper() != "OK")
                        {
                            if (StoreID != "10002")
                            {
                                Exception e = new Exception();
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in " + storeid + " ExtractPOS@" + DateTime.UtcNow + " GMT", "StatusCode:ERROR Response1" + "<br/>" + e.Message + "<br/>" + e.StackTrace);
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            ////////////For temporary purpose- To save Request & Response
                            if (response.ErrorMessage == null)
                            {
                                string msg = "No Errror";
                                SaveRequestResponse(client.BaseUrl.AbsoluteUri, result, msg, StoreID.ToString());
                            }
                            else
                            {
                                string msg = response.ErrorMessage.ToString();
                                SaveRequestResponse(client.BaseUrl.AbsoluteUri, result, msg, StoreID.ToString());
                            }
                            ////////////For temporary purpose- To save Request & Response
                            parentItems p = Load<parentItems>(result);
                            if (p.elements == null)
                            {
                                i = 1000;
                                break;
                            }
                            if (p.elements.Count == 0)
                            {
                                i = 1000;
                                break;
                            }
                            else
                            {

                                foreach (Products item in p.elements)
                                {
                                    taxrate = 0;
                                    bool nodefatax = true;
                                    if (item.defaultTaxRates != null)
                                    {
                                        if (item.defaultTaxRates.ToUpper() == "TRUE")
                                        {
                                            nodefatax = false;
                                            taxrate = deftax;
                                        }

                                    }

                                    if (nodefatax)
                                    {
                                        if (item.taxRates != null)
                                        {
                                            if (item.taxRates.elements.Count > 0)
                                            {
                                                if (item.taxRates.elements[0].rate != 0)
                                                {
                                                    taxrate = item.taxRates.elements[0].rate / Convert.ToDecimal(100000);
                                                    taxrate /= 100;
                                                }
                                            }
                                        }
                                    }
                                    if (settings.instock.ToUpper() == "TRUE")
                                    {
                                        if (item.itemStock != null)
                                        {
                                            if (item.itemStock.stockCount != 0)
                                            {
                                                if (item.itemStock.stockCount <= 0)
                                                {
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    ExportProducts exp = new ExportProducts();

                                    exp.storeid = storeid;
                                    exp.StoreProductName = "";
                                    if (item.name != null)
                                    {
                                        exp.StoreProductName = item.name;
                                    }
                                    exp.Storedescription = "";
                                    if (item.alternateName != null)
                                    {
                                        exp.Storedescription = item.alternateName;
                                    }
                                    exp.sku = "";
                                    if (item.id != null)
                                    {
                                        exp.sku = item.id;
                                    }
                                    exp.pack = 1;
                                    exp.qty = 0;
                                    if (item.itemStock != null)
                                    {
                                        if (item.itemStock.stockCount != 0)
                                        {
                                            if (item.itemStock.stockCount > 9999)
                                            {
                                                exp.qty = 9999;
                                            }
                                            else
                                            {
                                                exp.qty = item.itemStock.stockCount;
                                            }
                                        }
                                    }

                                    exp.price = 0;
                                    if (item.price != 0)
                                    {
                                        if (item.price != 0)
                                        {
                                            exp.price = item.price / 100;
                                        }
                                    }
                                    exp.tax = taxrate;
                                    exp.upc = "";
                                    if (item.code != null)
                                    {
                                        exp.upc = "#" + item.code.ToString();
                                    }
                                    if (storeid == "11387")               //#13773  5/04/2022
                                    {
                                        item.code = item.code == null ? "" : item.code;
                                        item.sku = item.sku == null ? "" : item.sku;
                                        exp.upc = "";
                                        if (item.sku != "")
                                        {
                                            exp.upc = "#" + item.sku.ToString();
                                        }
                                        else if (item.code != "")
                                        {
                                            exp.upc = "#" + item.code.ToString();
                                        }
                                        else
                                        {
                                            exp.upc = "";
                                        }
                                    }

                                    if (storeid == "11370")            //12903 # 14/02/2022
                                    {
                                        item.sku = item.sku == null ? "" : item.sku;
                                        exp.upc = "";
                                        if (item.sku != "")
                                        {
                                            exp.upc = "#" + item.sku.ToString();
                                        }
                                        else if (item.sku == "")
                                        {
                                            exp.upc = "";
                                        }
                                    }
                                    if (storeid == "11258")  //29/03/2022    #11057
                                    {
                                        string str = item.id;
                                        str = Regex.Replace(str, "[^0-9]", String.Empty);
                                        item.sku = item.sku == null ? "" : item.sku;
                                        exp.upc = "";
                                        if (item.sku != "")
                                        {
                                            exp.upc = "#" + item.sku.ToString();
                                        }
                                        else
                                        {
                                            exp.upc = "#9911258" + str;
                                        }

                                    }
                                    exp.altupc1 = item.altupc1;
                                    exp.altupc2 = item.altupc2;
                                    exp.altupc3 = item.altupc3;
                                    exp.altupc4 = item.altupc4;
                                    exp.altupc5 = item.altupc5;
                                    exp.CategoryId = item.categories.elements.Count > 0 ? string.Join(",", item.categories.elements.Select(x => x.id)) : "Other";

                                    if (storeid == "11258" && exp.upc != "" || storeid == "11370" && exp.upc != "" || storeid == "11387" && exp.upc != "")
                                    {
                                        expprd.Add(exp);
                                    }
                                    else
                                    {
                                        if (exp.CategoryId != "AKGXX4R4H9YP2" && item.code != null && item.code != "" && exp.sku != "YWBMNBHY8J63E" && exp.sku != "BSX0WDE4S26GR")
                                        {
                                            expprd.Add(exp);
                                        }
                                    }

                                }

                            }  
                        }
                    }

                    catch (Exception ex)
                    {
                        if (storeid != "10002")
                        {
                            Console.WriteLine(ex.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in " + storeid + " ExtractPOS@" + DateTime.UtcNow + " GMT", "StatusCode:ERROR Response2" + "<br/>" + ex.Message + "<br/>" + ex.StackTrace);
                        }
                        else
                        {
                        }
                    }
                }
                if (storeid == "11400")  // For Ticket 14100 Merging Qty for Duplicates
                {
                    var duplicates =
                                           from a in expprd
                                           group a by a.upc into values
                                           select new
                                           {
                                               upc = values.Key,
                                               qty = values.Sum(x => x.qty),
                                           };
                    var listss = duplicates.ToList();
                    List<Duplicateslist> dl = new List<Duplicateslist>();
                    Duplicateslist dli = new Duplicateslist();
                    var dlist = from d in listss
                                select new Duplicateslist
                                {
                                    upc = d.upc,
                                    qty = d.qty
                                };
                    var list = dlist.ToList();
                    var finallist = (from a in expprd
                                     join b in list
                    on a.upc equals b.upc
                                     select new ExportProductss
                                     {
                                         storeid = a.storeid,
                                         upc = a.upc,
                                         sku = a.sku,
                                         uom = a.uom,
                                         qty = b.qty,
                                         pack = a.pack,
                                         StoreProductName = a.StoreProductName,
                                         Storedescription = a.Storedescription,
                                         price = a.price,
                                         sprice = a.sprice,
                                         start = a.start,
                                         end = a.end,
                                         tax = a.tax,
                                         altupc1 = a.altupc1,
                                         altupc2 = a.altupc2,
                                         altupc3 = a.altupc3,
                                         altupc4 = a.altupc4,
                                         altupc5 = a.altupc5,
                                         CategoryId = a.CategoryId
                                     }
                                     ).ToList();
                    finallist = finallist.AsEnumerable()
                                       .GroupBy(x => x.upc)
                                       .Select(y => y.First())
                                       .ToList();
                    string UploadPaths = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\" + "product" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
                    CreateCSVFromGenericList<ExportProductss>(finallist, UploadPaths);
                    return UploadPaths;
                }
                else
                {
                    List<ExportProducts> exptData = new List<ExportProducts>();

                    foreach (var categoryItemid in settings.categories)
                    {
                        var listProductdata = expprd.Where(x => x.CategoryId.Contains(categoryItemid.id)).ToList();
                        exptData.AddRange(listProductdata);

                    }
                    string UploadPath = ConfigurationManager.AppSettings["BaseDirectory"] + "\\" + storeid + "\\Upload\\" + "product" + storeid + DateTime.UtcNow.ToString("yyyymmddHHmmss") + ".csv";
                    CreateCSVFromGenericList<ExportProducts>(expprd, UploadPath);

                    return UploadPath;
                }               
            }
            catch (Exception ex)
            {

                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in " + storeid + " ExtractPOS@" + DateTime.UtcNow + " GMT", "StatusCode:ERROR Response3" + "<br/>" + ex.Message + "<br/>" + ex.StackTrace);

                return ex.Message.ToString();

            }
            //return "";

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
                    if (pi.Name.ToUpper() == "CATEGORYID")
                    {
                        continue;
                    }
                    else
                    {
                        sw.Write(pi.Name.ToUpper() + ",");
                    }
                }
                sw.Write(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    //this acts as datacolumn
                    foreach (PropertyInfo pi in props)
                    {
                        //this is the row+col intersection (the value)
                        if (pi.Name.ToUpper() == "CATEGORYID")
                        {
                            continue;
                        }
                        else
                        {
                            string whatToWrite =
                                Convert.ToString(item.GetType()
                                                     .GetProperty(pi.Name)
                                                     .GetValue(item, null))
                                    .Replace(',', ' ') + ',';

                            sw.Write(whatToWrite);
                        }
                    }
                    sw.Write(newLine);
                }
            }
        }
        public class Tax
        {
            public List<TaxElements> elements { set; get; }
        }
        public class TaxElements
        {
            public string id { set; get; }
            public string name { set; get; }
            public Int32 rate { set; get; }
            public Boolean isDefault { set; get; }

        }
        public class FullName
        {
            public string storeid { set; get; }
            public string upc { set; get; }
            public Int32 qty { set; get; }
            public string sku { set; get; }
        }
        public class ExportProductss
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
        public class Duplicateslist
        {
            public string upc { set; get; }
            public long qty { set; get; }
        }
    }
}
