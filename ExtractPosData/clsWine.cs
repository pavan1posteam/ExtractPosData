using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData
{
    public class clsWine
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        public clsWine(int StoreId, decimal Tax)
        {
            try
            {
                WineConvertRawFile(StoreId, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string WineConvertRawFile(int StoreId, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            List<ProductMod> prodlist = new List<ProductMod>();
            List<FullnameModel> fullnamelist = new List<FullnameModel>();
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();

                        string Url = BaseUrl + "/" + StoreId + "/Raw/" + myFile;
                        if (File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = new DataTable();
                                dt.Columns.Add("Item number"); dt.Columns.Add("Normal description"); dt.Columns.Add("Vintage"); dt.Columns.Add("Item notes"); dt.Columns.Add("Size description");
                                dt.Columns.Add("Long description"); dt.Columns.Add("Units per case"); dt.Columns.Add("Unit retail"); dt.Columns.Add("Pack retail"); dt.Columns.Add("Units per pack");
                                dt.Columns.Add("(Warm) Case retail"); dt.Columns.Add("Quantity on hand"); dt.Columns.Add("Most recent upc code"); dt.Columns.Add("Department name"); dt.Columns.Add("Group name");
                                dt.Columns.Add("Sub-departmentname"); dt.Columns.Add("Primary vendor"); dt.Columns.Add("Proof"); dt.Columns.Add("Tax flag"); dt.Columns.Add("Unit sale retail");
                                dt.Columns.Add("Case sale retail"); dt.Columns.Add("Date sale starts"); dt.Columns.Add("Date sale ends"); dt.Columns.Add("Tasting/other notes"); dt.Columns.Add("Country of origin");
                                dt.Columns.Add("Region"); dt.Columns.Add("Sub region #1"); dt.Columns.Add("Sub region #2"); dt.Columns.Add("Grape"); dt.Columns.Add("Color");
                                dt.Columns.Add("Classification"); dt.Columns.Add("Wine Maker"); dt.Columns.Add("Winery"); dt.Columns.Add("Special rating"); dt.Columns.Add("Parker rating"); dt.Columns.Add("Other Rating");
                                dt.Columns.Add("Featured Item? Y/N "); dt.Columns.Add("Web Address of product"); dt.Columns.Add("Invoice Cost per Bottle"); dt.Columns.Add("Sale Type Code (standard) ");
                                dt.Columns.Add("Pack Sale Price"); dt.Columns.Add("Full Case Discount "); dt.Columns.Add("Mixed Case Discount"); dt.Columns.Add("Web selling Threshold(Max)"); dt.Columns.Add("Web Groupings");
                                dt.Columns.Add("Web unit Price"); dt.Columns.Add("Web Pack price"); dt.Columns.Add("Web Case Price"); dt.Columns.Add("Web Unit Sale"); dt.Columns.Add("Web Pack Sale");
                                dt.Columns.Add("Web Case Sale"); dt.Columns.Add("Web Sale Type"); dt.Columns.Add("Web Sale Starting Date"); dt.Columns.Add("Web Sale Ending Date");
                                dt.Columns.Add("Number units on openPO’s"); dt.Columns.Add("Points"); dt.Columns.Add("Web Code"); dt.Columns.Add("Available to Sell"); dt.Columns.Add("Continent"); dt.Columns.Add("Vineyard");
                                dt.Columns.Add("Item Flags"); dt.Columns.Add("On C/O"); dt.Columns.Add("Full Bar Code (Prefix,Data,Suffix)"); dt.Columns.Add("Date Item Inserted"); dt.Columns.Add("URL of Item Image");
                                dt.Columns.Add("Location"); dt.Columns.Add("Wine Condition"); dt.Columns.Add("Gross Weight of product (forshipping)"); dt.Columns.Add("Tax Rate");
                                dt.Columns.Add("Imported Item Number(conversions)"); dt.Columns.Add("Reserved 71"); dt.Columns.Add("Reserved 72"); dt.Columns.Add("Reserved 73"); dt.Columns.Add("Reserved 74");
                                dt.Columns.Add("Reserved 75"); dt.Columns.Add("Reserved 76"); dt.Columns.Add("Reserved 77"); dt.Columns.Add("Reserved 78"); dt.Columns.Add("Reserved 79"); dt.Columns.Add("Reserved 80");
                                dt.Columns.Add("Reserved 81");


                                string Fulltext;
                                using (StreamReader reader = new StreamReader(Url))
                                {

                                    while (!reader.EndOfStream)
                                    {
                                        Fulltext = reader.ReadToEnd().ToString(); //read full file text  
                                        string[] rows = Fulltext.Split('\n'); //split full file text into rows  

                                        for (int i = 0; i < rows.Count() - 1; i++)
                                        {
                                            string[] rowValues = rows[i].Split('\t'); //split each row with tab to get individual values  
                                            {
                                                DataRow dr = dt.NewRow();
                                                for (int k = 0; k < rowValues.Count(); k++)
                                                {

                                                    dr[k] = rowValues[k].ToString();

                                                }
                                                dt.Rows.Add(dr); //add other rows  
                                            }
                                        }
                                    }
                                }
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductMod pmsk = new ProductMod();
                                    FullnameModel full = new FullnameModel();
                                    pmsk.StoreID = StoreId;
                                    full.upc = "#" + dr.Field<string>("Most recent upc code").ToString();
                                    //full.Price = System.Convert.ToDecimal(dr["Unit retail"] == DBNull.Value ? 0 : dr["Unit retail"]);
                                    full.uom = dr.Field<string>("Size description");
                                    full.pcat = dr.Field<string>("Group name");
                                    full.pcat1 = dr.Field<string>("Sub-departmentname");
                                    full.pcat2 = "";
                                    full.country = dr.Field<string>("Country of origin");
                                    full.region = dr.Field<string>("Region");
                                    if (!string.IsNullOrEmpty(dr["Most recent upc code"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr.Field<string>("Most recent upc code").ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Qty = System.Convert.ToDecimal(dr["Quantity on hand"] == DBNull.Value ? 0 : dr["Quantity on hand"]);
                                    pmsk.sku = "#" + dr.Field<string>("Item number").ToString();
                                    full.sku = "#" + dr.Field<string>("Item number").ToString();
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Normal description")) && !dr.Field<string>("Normal description").Contains("(DQ"))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Normal description");
                                        pmsk.StoreDescription = dr.Field<string>("Normal description");
                                        full.pname = dr.Field<string>("Normal description");
                                        full.pdesc = dr.Field<string>("Normal description");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.pack = System.Convert.ToInt32(dr["Units per pack"] == DBNull.Value ? 1 : dr["Units per pack"]);
                                    //pmsk.Price  = System.Convert.ToDecimal(dr["Unit retail"] == DBNull.Value ? 0 : dr["Unit retail"]);
                                    //full.Price = System.Convert.ToDecimal(dr["Unit retail"] == DBNull.Value ? 0 : dr["Unit retail"]);
                                    pmsk.sprice = dr.Field<string>("Unit sale retail").ToString();
                                    string group = dr["Group name"].ToString();
                                    string PackRetail = dr["Pack retail"].ToString();
                                    string UnitsperPack = dr["Units per pack"].ToString();
                                    string CaseRetail = dr["(Warm) Case retail"].ToString();
                                    string size = dr["size description"].ToString();
                                    decimal Unitsperpack = Convert.ToDecimal(UnitsperPack);
                                    decimal Packretail = Convert.ToDecimal(PackRetail);
                                    decimal Caseretail = Convert.ToDecimal(PackRetail);

                                    if (Unitsperpack > 1 && Packretail != 0)
                                    {
                                        pmsk.Price = Convert.ToDecimal(dr["Pack retail"]);
                                        full.Price = Convert.ToDecimal(dr["Pack retail"]);
                                    }
                                    else if (group == "WINE" && size == "187ML" && Unitsperpack == 4 || group == "LIQUOR" && size == "187ML" && Unitsperpack == 4)
                                    {
                                        pmsk.Price = Convert.ToDecimal(dr["Unit retail"]);
                                        full.Price = Convert.ToDecimal(dr["Unit retail"]);
                                    }
                                    else
                                    {
                                        pmsk.Price = Convert.ToDecimal(dr["Unit retail"]);
                                        full.Price = Convert.ToDecimal(dr["Unit retail"]);
                                    }
                                    if (Tax == 0)
                                    {
                                        pmsk.tax = System.Convert.ToDecimal(dr["Tax Rate"] == DBNull.Value ? 0 : dr["Tax Rate"]);
                                        pmsk.tax = pmsk.tax / 100;
                                    }
                                    else
                                    {
                                        pmsk.tax = Tax;
                                    }
                                    if (!string.IsNullOrEmpty(pmsk.sprice))
                                    {
                                        pmsk.Start = dr.Field<string>("Date sale starts");
                                        pmsk.End = dr.Field<string>("Date sale ends");
                                    }
                                    else
                                    {
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                    }
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if (pmsk.Qty > 0 && pmsk.Price > 0 && full.pcat != "TOBACCO" && full.pcat != "CIGARETTES" && full.pcat != "E CIGARETTE")
                                    {
                                        prodlist.Add(pmsk);
                                        fullnamelist.Add(full);
                                    }
                                }

                                Console.WriteLine("Generating WinePos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For WinePos " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating WinePos " + StoreId + " FullNameFile CSV Files.....");
                                string fullfilename = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For WinePos " + StoreId);


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
                                clsEmail email = new clsEmail();
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not Generated File for WinePos " + StoreId;
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName Or Raw Folder is Empty! " + StoreId);
                    }
                }
            }
            return "";
        }
        public class ProductMod
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public decimal Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public string sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
        }
        public class FullnameModel
        {
            public string pname { get; set; }
            public string pdesc { get; set; }
            public string upc { get; set; }
            public string sku { get; set; }
            public decimal Price { get; set; }
            public string uom { get; set; }
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string country { get; set; }
            public string region { get; set; }
        }
    }
}
