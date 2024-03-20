using ExtractPosData.Model;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ExtractPosData
{
    public class clsECRSMacadoodles
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        string packSizeMappingPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"MacdoodlesPackMapping.json");

        public clsECRSMacadoodles(int StoreId, decimal tax)
        {
            try
            {
                MacadoodlesConvertRawFile(StoreId, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #region MyRegion
        //public DataTable ConvertCsvToDataTable(string Filename)
        //{
        //    DataTable dtResult = new DataTable();

        //    dtResult.Columns.Add("Department", typeof(String));
        //    dtResult.Columns.Add("UPC", typeof(String));
        //    dtResult.Columns.Add("Column1", typeof(String));
        //    dtResult.Columns.Add("Description", typeof(String));
        //    dtResult.Columns.Add("Size", typeof(String));
        //    dtResult.Columns.Add("long description", typeof(String));
        //    dtResult.Columns.Add("Price", typeof(String));
        //    dtResult.Columns.Add("Qty", typeof(string));
        //    using (TextFieldParser parser = new TextFieldParser(Filename))
        //    {
        //        parser.TextFieldType = FieldType.Delimited;
        //        parser.SetDelimiters(",");
        //        int i = 0;
        //        int r = 0;
        //        while (!parser.EndOfData)
        //        {
        //            if (i == 0)
        //            {
        //                string[] columns = parser.ReadFields();
        //                foreach (string col in columns)
        //                {
        //                    dtResult.Columns.Add(col);
        //                }
        //            }
        //            else
        //            {
        //                string LineValue = parser.ReadLine();

        //                if (LineValue.IndexOf("'\'") > 0)
        //                {
        //                    LineValue = LineValue.Replace("'\'", "! ");

        //                }

        //                if (LineValue.IndexOf(", ") > 0)
        //                {
        //                    LineValue = LineValue.Replace(", ", "#! ");

        //                }
        //                string[] rows = Regex.Split(LineValue, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

        //                dtResult.Rows.Add();
        //                int c = 0;
        //                foreach (string row in rows)
        //                {
        //                    var roww = row.Replace("!", "'\'");


        //                    dtResult.Rows[r][c] = roww.ToString();
        //                    c++;
        //                }

        //                r++;
        //            }
        //            i++;
        //        }
        //    }
        //    return dtResult; //Returning Dattable

        //}
        #endregion

        public string MacadoodlesConvertRawFile(int StoreId, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            string RemoveCat = ConfigurationManager.AppSettings.Get("RemoveCat");
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

                            //StreamReader r = new StreamReader(packSizeMappingPath);
                            //string json = r.ReadToEnd();
                            //List<RootObject> packItems = JsonConvert.DeserializeObject<List<RootObject>>(json);

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
                                    fname.pcat = item.Department.name;
                                    if (fname.pcat.ToUpper() == "DELETE DEPT." || fname.pcat.ToUpper().Contains("ALLOCATIONS") || fname.pcat.ToUpper() == "CIGARETTES/SMOKELESS" || fname.pcat.ToUpper() == "CIGARS/COMMERCIAL" || fname.pcat.ToUpper() == "CIGARS/HUMIDOR" || fname.pcat.ToUpper() == "Allocated" || fname.pcat.ToUpper() == "CIGARETTES")
                                    {
                                        continue;
                                    }                                 
                                    if (RemoveCat.Contains(StoreId.ToString()) && fname.pcat.ToUpper() == "KEG DEPOSIT PO")
                                    {
                                        continue;
                                    }
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
                                    if (StoreId == 10535)
                                    {
                                        if (prod.sprice > 0)
                                        {
                                            prod.Start = item.Pricing.PromotionalPricing.startDate.ToString("MM-dd-yyyyTHH:mm:ss.fff");
                                            prod.End = item.Pricing.PromotionalPricing.endDate.ToString("MM-dd-yyyyTHH:mm:ss.fff");
                                        }
                                        else
                                        {
                                            prod.Start = "";
                                            prod.End = "";
                                        }
                                    }
                                    else
                                    {
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
                                    }
                                    if (StoreId == 10535)
                                    {
                                        if (item.Department.name.Contains("Beer") || item.Department.name.Contains("Liquor") || item.Department.name.Contains("Wine"))
                                        {
                                            prod.Tax = Convert.ToDecimal(Tax);
                                        }
                                        else
                                        {
                                            prod.Tax = Convert.ToDecimal(0.0635);
                                        }
                                    }
                                    else if (StoreId == 10726)
                                    {
                                        if (item.Department.name.Contains("Snacks") || item.Department.name.Contains("Grocery"))
                                        {
                                            prod.Tax = Convert.ToDecimal(0.051);
                                        }
                                        else
                                        {
                                            prod.Tax = Convert.ToDecimal(Tax);
                                        }                                                //Tckt 6796
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

                                    if (StoreId == 10535)
                                    {
                                        if (prod.Qty > 0 && prod.Price > 0 && fname.pcat != "Build Your Own")
                                        {
                                            prodlist.Add(prod);
                                            fulllist.Add(fname);
                                        }
                                    }
                                    else
                                    {
                                        if (prod.Qty > 0 && prod.Price > 0)
                                        {
                                            prodlist.Add(prod);
                                            fulllist.Add(fname);
                                        }
                                    }
                                }
                            }
                            Console.WriteLine("Generating ECRSMacadoodles " + StoreId + " Product CSV Files.....");
                            Console.WriteLine("Generating ECRSMacadoodles " + StoreId + " Full Name CSV Files.....");
                            string pfilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            string filename = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For ECRSMacadoodles " + StoreId);
                            Console.WriteLine("Full Name File Generated For ECRSMacadoodles " + StoreId);

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
                            return "Not generated file for ECRSMacadoodles " + StoreId;
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
            return "Completed generating File For ECRSMacadoodles" + StoreId;
        }
        //public class RootObject
        //{
        //    public string uom { get; set; }
        //    public string pack { get; set; }
        //}
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
}
