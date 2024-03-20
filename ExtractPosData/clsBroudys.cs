using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using RestSharp;
using System.Net.Http;
namespace ExtractPosData
{
    public class clsBroudys
    {
        public static string ApiBaseAddress { get; set; }
        
        public clsBroudys(int StoreId,int StoreMapId,string APIKey)
        {
            try
            {
                ApiBaseAddress = ConfigurationManager.AppSettings.Get("BroudyAPIUrl").ToString();
                string val = Broudy_ConvertToFile(StoreId, StoreMapId, APIKey);
                Console.WriteLine(val);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private string Broudy_GetProductListJson(int store,string APIKey)
        {
            POSSettings pobj = new POSSettings();
            pobj.IntializeStoreSettings();
            string content = "";
            try
            {
                string Url = ApiBaseAddress +"ExportProd?store=" + store + "&key=" + APIKey;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                HttpWebResponse responsestr = (HttpWebResponse)request.GetResponse();
                content = new StreamReader(responsestr.GetResponseStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return content;
        }
    
        private string Broudy_ConvertToFile(int StoreId,int StoreMapId,string APIKey)
        {
            try
            {
                var Json = Broudy_GetProductListJson(StoreMapId, APIKey);
                List<BroudyProductModel> list = JsonConvert.DeserializeObject<List<BroudyProductModel>>(Json.ToString());
                List<BProductModel> prodlist = list.Select(s => new BProductModel
                {
                    upc = "#" + s.upc,
                    sku = "#" + s.SKU.ToString(),
                    StoreProductName = s.name,
                    StoreDescription = "",
                    StoreID = s.storeid,
                    Qty = s.quantityOnHand,
                    Price = s.price,
                    sprice = System.Convert.ToDecimal(s.priceSale == "" || s.priceSale == null ? "0" : s.priceSale),
                    Tax = s.taxRate,
                    pack = s.packSize,
                    End = "",
                    Start = "",
                    altupc1 = "",
                    altupc2 = "",
                    altupc3 = "",
                    altupc4 = "",
                    altupc5 = "",
                    clubprice=s.priceMember
                }).ToList();

                string BaseURL = ConfigurationManager.AppSettings.Get("BaseDirectory");
                if (Directory.Exists(BaseURL))
                {
                    Console.WriteLine("Generating Broudys " + StoreId + " Product CSV File......");
                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseURL);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("" + e.Message);
                //Console.Read();
                return "Not Generated file for Broudy's " + StoreId;
                
            }
                    return "Completed Generating Broudys " + StoreId + " Files...";
        }
        
    }
}
