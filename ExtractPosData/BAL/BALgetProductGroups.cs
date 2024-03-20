using ExtractPosData.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData.BAL
{
    public class BALgetProductGroups
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings["ERPLYBaseUrl"];

        public clsGetProductGroup.RootObject getProductGroups(string ClientId,string Username,string Password)
        {
            Common common = new Common();
            clsGetProductGroup.RootObject ProductGroup = new clsGetProductGroup.RootObject();
            try
            {
                string sessionKey = common.verifyUser(ClientId,Username,Password);
                var client = new RestClient(baseUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("sessionKey", sessionKey);
                request.AddParameter("posID", "1");
                request.AddParameter("partnerKey", "6utraCHaCaWeV3pHAPApUwrU2AgeC77wpuy3nUcu");
                request.AddParameter("clientCode", "468675");
                request.AddParameter("request", "getProductGroups");
                request.AddParameter("version", "3.32.3");
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                ProductGroup = JsonConvert.DeserializeObject<clsGetProductGroup.RootObject>(content);
            }
            catch (Exception ex)
            {
                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in EreplyPos@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            return ProductGroup;
        }
    }
}
