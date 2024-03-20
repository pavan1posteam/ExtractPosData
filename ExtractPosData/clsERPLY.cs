using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.BAL;
using ExtractPosData.Models;
using ExtractPosData.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ExtractPosData
{
    public class clsERPLY
    {
        public clsERPLY(int storeid,decimal tax,string ClientId,string Username,string Password)
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string BaseDirectory = ConfigurationManager.AppSettings["BaseDirectory"];
            try
            {
                //int storeid = pd.PosDetails.FirstOrDefault().StoreSettings.StoreId;
                //decimal tax = pd.PosDetails.FirstOrDefault().StoreSettings.POSSettings.tax;
                Console.WriteLine("Generating Product FIle For EreplyPos " + storeid);
                Console.WriteLine("Generating Fullname FIle For EreplyPos " + storeid);
                BALgetProducts products = new BALgetProducts();
                var finalresult = products.getProduct(ClientId,Username,Password).ToList();

                List<clsProductModel> prodList = new List<clsProductModel>();
                List<ProductModel10710>  prodList10710 = new List<ProductModel10710>();
                List<clsFullnameModel> fullNameList = new List<clsFullnameModel>();

                foreach (var item in finalresult)
                {
                    foreach (var record in item)
                    {
                        if (storeid == 10424)
                        {
                            clsProductModel prod = new clsProductModel();
                            clsFullnameModel fullName = new clsFullnameModel();

                            var json = JsonConvert.SerializeObject(record["warehouses"]);
                            json = json.Replace("{\"1\":", "").Replace("}}", "}");

                            var warehouse = JToken.Parse(json);

                            prod.StoreID = storeid;
                            prod.upc = '#' + (string)record["code2"];
                            if (string.IsNullOrEmpty(prod.upc)) { continue; }
                            var qty = (decimal)warehouse["totalInStock"];
                            if (qty > 0)
                            {
                                prod.Qty = Convert.ToInt64(qty);
                            }
                            else { continue; }
                            string qqty = prod.Qty.ToString();
                            int len = qqty.Length;
                            if (len > 4) 
                            { continue; }
                            prod.sku = '#' + (string)record["code2"];
                            fullName.sku = '#' + (string)record["code2"];
                            prod.pack = 1;
                            prod.StoreProductName = (string)record["name"];
                            prod.StoreDescription = (string)record["name"];
                            prod.Price = (decimal)record["price"];
                            prod.sprice = 0;
                            prod.tax = tax;
                            prod.Start = "";
                            prod.End = "";
                            prod.altupc1 = "";
                            prod.altupc2 = "";
                            prod.altupc3 = "";
                            prod.altupc4 = "";
                            prod.altupc5 = "";

                            fullName.upc = '#' + (string)record["code2"];
                            fullName.pname = (string)record["name"];
                            fullName.pdesc = (string)record["name"];
                            fullName.Price = (decimal)record["price"];
                            fullName.pcat = (string)record["groupName"];
                            fullName.pcat1 = "";
                            fullName.pcat2 = "";
                            fullName.uom = "";
                            fullName.country = "";
                            fullName.region = "";

                            if ((int)record["groupID"] != 1 && (int)record["groupID"] != 2 && (int)record["groupID"] != 9 && (int)record["groupID"] != 10 && (int)record["groupID"] != 11 &&
                                (int)record["groupID"] != 12 && (int)record["groupID"] != 13)
                            {
                                prodList.Add(prod);
                                fullNameList.Add(fullName);
                            }
                        }
                        else
                        {
                            clsFullnameModel fullName = new clsFullnameModel();
                            ProductModel10710 prod = new ProductModel10710();

                            var json = JsonConvert.SerializeObject(record["warehouses"]);
                            json = json.Replace("{\"1\":", "").Replace("}}", "}");

                            var warehouse = JToken.Parse(json);

                            prod.StoreID = storeid;
                            prod.upc = '#' + (string)record["code"];
                            if (string.IsNullOrEmpty(prod.upc)) 
                            { continue; }
                            var qty = (decimal)warehouse["totalInStock"];
                            if (qty > 0)
                            {
                                prod.Qty = Convert.ToInt64(qty);
                            }
                            else { continue; }
                            string qqty = prod.Qty.ToString();
                            int len = qqty.Length;
                            if (len > 4)
                            { continue; }
                            prod.Qty = Convert.ToInt64(qty);
                            prod.sku = '#' + (string)record["code"];
                            prod.pack = 1;
                            prod.StoreProductName = (string)record["name"];
                            prod.StoreDescription = (string)record["name"];
                            prod.Price = (decimal)record["price"];
                            prod.sprice = (decimal)record["priceListPrice"];
                            //prod.culbprice = (decimal)record["priceListPrice"];
                            prod.tax = tax;
                            if (prod.sprice > 0)
                            {
                                prod.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                prod.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                            }
                            else
                            {
                                prod.Start = "";
                                prod.End = "";
                            }
                            prod.altupc1 = "";
                            prod.altupc2 = "";
                            prod.altupc3 = "";
                            prod.altupc4 = "";
                            prod.altupc5 = "";

                            fullName.upc = '#' + (string)record["code"];
                            fullName.pname = (string)record["name"];
                            fullName.pdesc = (string)record["name"];
                            fullName.Price = (decimal)record["price"];
                            fullName.pcat = (string)record["groupName"];
                            fullName.pcat1 = "";
                            fullName.pcat2 = "";
                            fullName.uom = "";
                            fullName.country = "";
                            fullName.region = "";

                            if ((int)record["groupID"] != 27 && (int)record["groupID"] != 34 && (int)record["groupID"] != 5)
                            {
                                prodList10710.Add(prod);
                                fullNameList.Add(fullName);
                            }
                        }
                    }
                }
                if (storeid == 10710)
                {
                    GenerateCSV.GenerateCSVFile(prodList10710, "PRODUCT", storeid, BaseDirectory);
                }
                else
                {
                    GenerateCSV.GenerateCSVFile(prodList, "PRODUCT", storeid, BaseDirectory);
                }
                GenerateCSV.GenerateCSVFile(fullNameList, "FULLNAME", storeid, BaseDirectory);
                Console.WriteLine();
                Console.WriteLine("Product FIle Generated For EreplyPos " + storeid);
                Console.WriteLine("Fullname FIle Generated For EreplyPos " + storeid);
            }
            catch (Exception ex)
            {
                //(new clsEmail()).sendEmail(DeveloperId, "", "", "Error in EreplyPos@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
        }
    }
    public class ProductModel10710
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public Int64 Qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public decimal Price { get; set; }
        public decimal sprice { get; set; }
        //public decimal culbprice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
    }
}
