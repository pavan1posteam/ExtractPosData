using ExtractPosData.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData
{
    public class Common
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings["ERPLYBaseUrl"];

        public string verifyUser(string ClientId,string Username,string Password)
        {
            string sessionKey = "";
            try
            {
                var client = new RestClient("https://"+ ClientId +".erply.com/api/");
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("clientCode", ClientId);
                request.AddParameter("username", Username);
                request.AddParameter("password", Password);
                request.AddParameter("request", "verifyUser");
                IRestResponse response = client.Execute(request);
                var content = response.Content;
                var result = JsonConvert.DeserializeObject<clsVerifyUser.RootObject>(content);
                sessionKey = result.records[0].sessionKey.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return sessionKey;
        }
    }
}
