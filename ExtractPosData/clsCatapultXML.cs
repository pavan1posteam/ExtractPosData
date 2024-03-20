using ExtractPosData.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ExtractPosData
{
    class clsCatapultXML
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsCatapultXML(int StoreId, decimal tax, decimal winetax, decimal liquortax, decimal beertax)
        {
            try
            {
                ConvertRawFile(StoreId, tax, winetax, liquortax, beertax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
        public string ConvertRawFile(int StoreId, decimal Tax, decimal winetax, decimal liquortax, decimal beertax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    var myFile = (from f in directory.GetFiles("*.xml")
                                  orderby f.LastWriteTime descending
                                  select f).First();

                    string Url = BaseUrl + "/" + StoreId + "/Raw/" + myFile;
                    if (File.Exists(Url))
                    {
                        try
                        {
                            WebClient web = new WebClient();
                            string fPath = string.Format(Url);
                            string response = web.DownloadString(fPath);

                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(response);

                            var json = JsonConvert.SerializeXmlNode(xmlDoc).Replace("@", "");
                            json = "{" + json.Substring(26, json.Length - 26);
                            var itms = JsonConvert.DeserializeObject<Root>(json);

                            List<ProductsModel> prodlist = new List<ProductsModel>();
                            List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                            ProductsModel prod = new ProductsModel();
                            FullNameProductModel fname = new FullNameProductModel();
                            foreach (var item in itms.Items.Item)
                            {
                                foreach (var prcitem in item.Pricing.Price)
                                {
                                    prod = new ProductsModel();
                                    fname = new FullNameProductModel();

                                    prod.StoreID = StoreId;
                                    prod.upc = '#' + item.scancode;
                                    fname.upc = '#' + item.scancode;
                                    decimal qty = Convert.ToDecimal(item.OnHand);
                                    prod.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                    prod.sku = '#' + item.scancode;
                                    fname.sku = '#' + item.scancode;
                                    prod.pack = 1;
                                    fname.pack = 1;
                                    prod.StoreProductName = item.ReceiptAlias;
                                    prod.StoreDescription = item.ReceiptAlias;
                                    fname.pname = item.ReceiptAlias;
                                    fname.pdesc = item.ReceiptAlias;
                                    fname.uom = item.Size;
                                    fname.pcat = item.Department.name.Replace("01", "").Replace("02", "").Replace("03", "").Replace("04", "").Replace("05", "").Replace("06", "").Replace("07", "").Trim();
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.country = "";
                                    fname.region = "";
                                    var priceLevel = Convert.ToInt32(prcitem.priceLevel);
                                    if (priceLevel == 1)
                                    {
                                        prod.Price = Convert.ToDecimal(prcitem.price);
                                        fname.Price = Convert.ToDecimal(prcitem.price);
                                        if (item.Pricing.PromotionalPricing != null)
                                        {
                                            prod.sprice = Convert.ToDecimal(item.Pricing.PromotionalPricing.price.FirstOrDefault().price);
                                        }
                                        else
                                        {
                                            prod.sprice = 0;
                                        }
                                    }
                                    else
                                    { continue; }

                                    if (prod.sprice > 0)
                                    {
                                        prod.Start = item.Pricing.PromotionalPricing.startDate.ToString("MM/dd/yyyy");
                                        prod.End = item.Pricing.PromotionalPricing.endDate.ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        prod.Start = "";
                                        prod.End = "";
                                    }
                                    if (StoreId == 11309)
                                    {
                                        if (item.Department.name.Contains("BEER"))
                                        {
                                            prod.Tax = Convert.ToDecimal(beertax);
                                        }
                                        else if (item.Department.name.Contains("LIQUOR"))
                                        {
                                            prod.Tax = Convert.ToDecimal(liquortax);
                                        }
                                        else if (item.Department.name.Contains("WINE"))
                                        {
                                            prod.Tax = Convert.ToDecimal(winetax);
                                        }
                                        else
                                        {
                                            prod.Tax = Convert.ToDecimal(Tax);
                                        }
                                    }
                                    else
                                    {
                                        prod.Tax = Convert.ToDecimal(Tax);
                                    }
                                    prod.altupc1 = "";
                                    prod.altupc2 = "";
                                    prod.altupc3 = "";
                                    prod.altupc4 = "";
                                    prod.altupc5 = "";

                                    if (prod.Qty > 0 && prod.Price > 0)
                                    {
                                        prodlist.Add(prod);
                                        fulllist.Add(fname);
                                    }
                                }
                            }
                            Console.WriteLine("Generating ECRSCatapult " + StoreId + " Product CSV Files.....");
                            Console.WriteLine("Generating ECRSCatapult " + StoreId + " Full Name CSV Files.....");
                            string pfilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            string filename = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For ECRSCatapult " + StoreId);
                            Console.WriteLine("Full Name File Generated For ECRSCatapult " + StoreId);

                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                            foreach (string filePath in filePaths)
                            {
                                string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                File.Move(filePath, destpath);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("" + e.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                            return "Not generated file for ECRSCatapult " + StoreId;
                        }
                    }
                    else
                    {
                        return "Ínvalid FileName" + StoreId;
                    }
                }
                else
                {
                    return "Invalid Sub-Directory" + StoreId;
                }
            }
            else
            {
                return "Invalid Directory" + StoreId;
            }
            return "Completed generating File For ECRSCatapult" + StoreId;
        }
    }
    public class Department
    {
        public string name { get; set; }
        public string number { get; set; }
    }
    public class ItemGroup
    {
        public string name { get; set; }
    }
    public class Price
    {
        public string price { get; set; }
        public string priceLevel { get; set; }
    }
    public class PromotionalPricing
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public List<Price> price { get; set; }

    }
    public class Pricing
    {
        public List<Price> Price { get; set; }
        public PromotionalPricing PromotionalPricing { get; set; }
    }
    public class Item
    {
        public string scancode { get; set; }
        public string discontinued { get; set; }
        public string taxExempt { get; set; }
        public string ReceiptAlias { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public Department Department { get; set; }
        public string OnHand { get; set; }
        public string SafetyStock { get; set; }
        public string PowerField2 { get; set; }
        public object ItemGroup { get; set; }
        public Pricing Pricing { get; set; }
    }
    public class Items
    {
        public DateTime dateGenerated { get; set; }
        public List<Item> Item { get; set; }
    }
    public class Root
    {
        public Items Items { get; set; }
    }
}
