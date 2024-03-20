using ExtractPosData.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.BAL
{
    public class BALgetProducts
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings["ERPLYBaseUrl"];

        public clsGetProduct.RootObject getStatus(string ClientId,string Username,string Password)
        {
            clsGetProduct.RootObject pStatus = new clsGetProduct.RootObject();
            BALgetProductGroups pGroups = new BALgetProductGroups();
            Common common = new Common();
            try
            {
                string sessionKey = common.verifyUser(ClientId,Username,Password);
                //var PGId = pGroups.getProductGroups().records;
                //int count = PGId.Count();
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                //request.AddParameter("groupID", item.productGroupID);
                request.AddParameter("recordsOnPage", "1000");
                request.AddParameter("active", "1");
                request.AddParameter("sessionKey", sessionKey);
                request.AddParameter("request", "getProducts");
                request.AddParameter("clientCode", ClientId);
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                pStatus = JsonConvert.DeserializeObject<clsGetProduct.RootObject>(content);
            }
            catch (Exception)
            {

                throw;
            }
            return pStatus;
        }

        public List<JArray> getProduct(string ClientId,string Username,string Password)
        {
            Common common = new Common();
            BALgetProductGroups pGroups = new BALgetProductGroups();
            List<JArray> productList = new List<JArray>();
            clsGetProduct.RootObject getProducts = new clsGetProduct.RootObject();
            //clsGetProduct.Status pStatus = new clsGetProduct.Status();
            try
            {
                var totalRecord = getStatus(ClientId,Username,Password).status.recordsTotal;
                //var PGId = pGroups.getProductGroups().records;
                int pageNo;
                string sessionKey = common.verifyUser(ClientId,Username,Password);
                //var recordInResponse = 100;
                //foreach (var item in PGId)
                //{
                for (pageNo = 1; pageNo < totalRecord + 100; pageNo++)
                {
                    var client = new RestClient(baseUrl);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    //request.AddParameter("groupID", 3);
                    request.AddParameter("recordsOnPage", "100");
                    request.AddParameter("active", "1");
                    request.AddParameter("pageNo", pageNo);
                    request.AddParameter("includeMatrixVariations", "0");
                    request.AddParameter("getStockInfo", "1");
                    request.AddParameter("warehouseID", "1");
                    request.AddParameter("getReplacementProducts", "1");
                    request.AddParameter("getRelatedProducts", "1");
                    request.AddParameter("orderBy", "name");
                    request.AddParameter("orderByDir", "asc");
                    request.AddParameter("getPriceListPrices", "1");
                    request.AddParameter("getContainerInfo", "1");
                    request.AddParameter("getAllLanguages", "1");
                    request.AddParameter("getRecipes", "1");
                    request.AddParameter("type", "PRODUCT,MATRIX,BUNDLE,ASSEMBLY");
                    request.AddParameter("getPriceCalculationSteps", "1");
                    request.AddParameter("clientID", "3");
                    request.AddParameter("sessionKey", sessionKey);
                    //request.AddParameter("partnerKey", "6utraCHaCaWeV3pHAPApUwrU2AgeC77wpuy3nUcu");
                    request.AddParameter("clientCode", ClientId);
                    request.AddParameter("request", "getProducts");
                    request.AddParameter("version", "3.32.3");
                    IRestResponse response = client.Execute(request);
                    var content = response.Content;
                    var pJson = (dynamic)JObject.Parse(content);
                    var jArray = (JArray)pJson["records"];

                    productList.Add(jArray);
                    totalRecord = totalRecord - 100;

                }

            }
            catch (Exception)
            {

                throw;
            }
            return productList;
        }
    }
}
