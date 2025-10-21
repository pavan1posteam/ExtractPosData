using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtractPosData
{
    class clsVision
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string specificUOM11989 = ConfigurationManager.AppSettings["specificUOM11989"];
        string specificSprice11989 = ConfigurationManager.AppSettings["specificSprice11989"];
        string visionOutofstock = ConfigurationManager.AppSettings["visionOutofstock"];
        string QtyPerPack = ConfigurationManager.AppSettings["QtyPerPack"];
        


        public clsVision(string FileName, int StoreId, decimal Tax, bool IsMarkUpPrice, int MarkUpValue, int LiquorDiscount, int WineDiscount, decimal LiquorMarkup)
        {
            try
            {
                VisionConvertRawFile(FileName, StoreId, Tax, IsMarkUpPrice, MarkUpValue, LiquorDiscount, WineDiscount, LiquorMarkup);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static DataTable ConvertTextToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            using (TextFieldParser parser = new TextFieldParser(FileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                int i = 0;
                int r = 0;
                while (!parser.EndOfData)
                {
                    if (i == 0)
                    {
                        string[] columns = parser.ReadFields();
                        foreach (string col in columns)
                        {
                            dtResult.Columns.Add(col);
                        }
                    }
                    else
                    {
                        string[] rows = parser.ReadFields();
                        dtResult.Rows.Add();
                        int c = 0;
                        foreach (string row in rows)
                        {
                            var roww = row.Replace('"', ' ').Trim();

                            dtResult.Rows[r][c] = roww.ToString();
                            c++;
                        }

                        r++;
                    }
                    i++;
                }
            }
            return dtResult; //Returning Dattable  
        }
        DataTable dte = new DataTable();
        public string VisionConvertRawFile(string PosFileName, int StoreId, decimal Tax, bool IsMarkUpPrice, int MarkUpValue, int LiquorDiscount, int WineDiscount, decimal LiquorMarkup)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            string LimitedProducts = ConfigurationManager.AppSettings.Get("LimitedProducts");
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
                                    if (!string.IsNullOrEmpty(dr["Most recent upc code"].ToString()))
                                    {
                                        full.upc = "#" + dr.Field<string>("Most recent upc code").ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    full.uom = dr.Field<string>("Size description");
                                    pmsk.uom = dr.Field<string>("Size description");
                                    full.pcat = dr.Field<string>("Group name");
                                    full.pcat1 = dr.Field<string>("Sub-departmentname");
                                    full.pcat2 = "";

                                    if (StoreId == 10922)
                                    {
                                        if (full.uom == "50ML" || full.uom == "100ML" || full.uom == "200ML" && full.pcat.ToUpper() == "LIQUOR")
                                        {
                                            continue;
                                        }
                                    }
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
                                    //pmsk.pack = System.Convert.ToInt32(dr["Units per pack"] == "" ? 1 : dr["Units per pack"]);
                                    //full.pack = System.Convert.ToInt32(dr["Units per pack"] == "" ? 1 : dr["Units per pack"]);
                                    string pak = Regex.Replace(dr["Units per pack"].ToString(), @"[^0-9]", "1");
                                    if (!string.IsNullOrEmpty(pak))
                                    {
                                        pmsk.pack = System.Convert.ToInt32(pak);
                                        full.pack = System.Convert.ToInt32(pak);
                                    }
                                    // pmsk.Qty = System.Convert.ToDecimal(dr["Quantity on hand"] == "" ? 0 : dr["Quantity on hand"]);
                                    //string qty = Regex.Replace(dr["Quantity on hand"].ToString(), @"[^0-9]+", "0");
                                    string qty = dr["Quantity on hand"].ToString(); 
                                    if (!string.IsNullOrEmpty(qty))
                                    {
                                        pmsk.Qty = System.Convert.ToDecimal(qty);
                                    }

                                    //  tckt 6506
                                    pmsk.sku = "#" + dr.Field<string>("Item number").ToString();
                                    full.sku = "#" + dr.Field<string>("Item number").ToString();

                                    // tckt 6506, tckt #33393
                                    if (QtyPerPack.Contains(StoreId.ToString()))
                                    {
                                        if (full.pcat.ToUpper().Contains("BEER"))
                                        {
                                            pmsk.Qty = (pmsk.Qty / pmsk.pack);
                                            pmsk.Qty = Math.Floor(pmsk.Qty);
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(dr.Field<string>("Normal description")) && !dr.Field<string>("Normal description").Contains("(DQ"))
                                    {

                                        pmsk.StoreProductName = dr.Field<string>("Normal description");
                                        pmsk.StoreDescription = dr.Field<string>("Normal description");
                                        full.pname = dr.Field<string>("Normal description");
                                        full.pdesc = dr.Field<string>("Normal description");
                                    }
                                    if (pmsk.StoreID == 11401)
                                    {
                                        pmsk.uom = dr.Field<string>("Size description");
                                        if (!string.IsNullOrEmpty(dr.Field<string>("Normal description")) && !dr.Field<string>("Normal description").Contains("(DQ"))
                                        {
                                            pmsk.StoreProductName = (dr.Field<string>("Normal description") + " " + dr.Field<string>("Vintage")).Substring(pmsk.StoreProductName.IndexOf('.') + 1);
                                            pmsk.StoreDescription = (dr.Field<string>("Normal description") + " " + dr.Field<string>("Vintage")).Substring(pmsk.StoreDescription.IndexOf('.') + 1);
                                            full.pname = (dr.Field<string>("Normal description") + " " + dr.Field<string>("Vintage")).Substring(full.pname.IndexOf('.') + 1);
                                            full.pdesc = (dr.Field<string>("Normal description") + " " + dr.Field<string>("Vintage")).Substring(full.pdesc.IndexOf('.') + 1);
                                        }
                                        
                                    }
                                    string prc = Regex.Replace(dr["Unit retail"].ToString(), @"[^0-9.]+", "0");
                                    if (!string.IsNullOrEmpty(prc))
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(prc);
                                        full.Price = System.Convert.ToDecimal(prc);
                                    }
                                    string sprice;

                                    if (specificSprice11989.Contains(StoreId.ToString()))
                                    {
                                        sprice = dr.Field<string>("Web Unit Sale");
                                    }
                                    else
                                    {
                                        sprice = dr.Field<string>("Unit sale retail");
                                    }

                                    if (specificUOM11989.Contains(StoreId.ToString()))
                                    {
                                        string uoms = "4L,3L,1.75L,1.5L,500ML,200ML,100ML,50ML";
                                        string sizeDescription = dr.Field<string>("Size description"); 

                                        if (uoms.Contains(sizeDescription))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            // No match found, assign value of "Size description" to pmsk.uom and full.uom
                                            pmsk.uom = sizeDescription;
                                            full.uom = sizeDescription;
                                        }
                                    }
                                    string packRetail = dr["Pack retail"].ToString();
                                    decimal BeerPrice = System.Convert.ToDecimal(packRetail == "" ? 0 : dr["Pack retail"]);
                                    string packsaleprice = dr.Field<string>("Pack Sale Price");

                                    if (pmsk.pack > 1 && BeerPrice > 0)
                                    {
                                        pmsk.Price = BeerPrice;
                                        full.Price = BeerPrice;
                                        pmsk.sprice = packsaleprice;
                                    }
                                    else if (pmsk.pack > 0)
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(prc);
                                        full.Price = System.Convert.ToDecimal(prc);
                                        pmsk.sprice = sprice;
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    if (StoreId == 11401 && full.pcat == "LIQUOR") // As per ticket 14418
                                    {
                                        decimal var = pmsk.Price - ((pmsk.Price) * LiquorDiscount / 100);
                                        pmsk.sprice = var.ToString();
                                    }
                                    if (StoreId == 11401 && full.pcat == "WINE" && !full.pcat1.Contains("CHAMPAGNE")) // As per ticket 14418
                                    {
                                        decimal var = pmsk.Price - ((pmsk.Price) * WineDiscount / 100);
                                        pmsk.sprice = var.ToString();
                                    }
                                    if (StoreId == 10703 && full.pcat == "BEER")
                                    {
                                        decimal markup = pmsk.Price + ((pmsk.Price) * MarkUpValue / 100);
                                        pmsk.Price = markup;
                                        full.Price = markup;
                                        pmsk.Price = Decimal.Round(pmsk.Price, 2);
                                        full.Price = Decimal.Round(full.Price, 2);
                                    }
                                    if (Tax == 0 && StoreId != 10703)
                                    {
                                        pmsk.tax = System.Convert.ToDecimal(dr["Tax Rate"] == DBNull.Value ? 0 : dr["Tax Rate"]);
                                        pmsk.tax = pmsk.tax / 100;

                                    }
                                    else
                                    {
                                        pmsk.tax = Tax;
                                    }
                                    if (pmsk.StoreID == 11401 && pmsk.sprice != "")
                                    {
                                        pmsk.Start = DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy");
                                        pmsk.End = "12/30/9999";
                                    }
                                    else
                                    {
                                        pmsk.Start = DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy");
                                        pmsk.End = dr.Field<string>("Date sale ends");
                                    }
                                    if (pmsk.StoreID == 11401 && !dr["Region"].ToString().Contains("CHAMPAGNE")) // As per ticket #39397
                                    {
                                        if (dr["Department name"].ToString().Contains("SPARKLING WINES"))
                                        {
                                            string p = dr["Invoice Cost per Bottle"].ToString();
                                            pmsk.sprice = Math.Round((Convert.ToDecimal(p) * Convert.ToDecimal(1.25)),2).ToString();
                                            pmsk.Price = Math.Round(Convert.ToDecimal(pmsk.sprice) * Convert.ToDecimal(1.30),2);
                                            full.Price = pmsk.Price;

                                            pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                            pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                        }
                                    }
                                    if (pmsk.StoreID == 11401 && dr["Group name"].ToString().ToUpper().Contains("LIQUOR")) // As per ticket #40844
                                    {
                                        if(Regex.IsMatch(pmsk.uom,@"^(700ML|750ML|1L|1.5L|1.75L)$"))
                                        {
                                            decimal price = Convert.ToDecimal(dr["Invoice Cost per Bottle"].ToString());
                                            price = price * Convert.ToDecimal(LiquorMarkup);
                                            pmsk.Price = Math.Round(price, 2);
                                            full.Price = pmsk.Price;
                                            pmsk.sprice = "";
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                        }
                                    }
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    pmsk.Vintage = dr.Field<string>("Vintage");

                                    if (pmsk.Qty > 0 && pmsk.Price > 0 && !visionOutofstock.Contains(StoreId.ToString()))
                                    {
                                        prodlist.Add(pmsk);
                                        fullnamelist.Add(full);
                                    }
                                    else if (visionOutofstock.Contains(StoreId.ToString()))
                                    {
                                        if (full.pcat1 != "KEG BEER")
                                        {
                                            prodlist.Add(pmsk);
                                            fullnamelist.Add(full);
                                        }
                                    }


                                    if (StoreId == 12108)
                                    {
                                        if (dr.Field<string>("Group name").Contains("Beer"))
                                        {
                                            ProductMod pmsk1 = new ProductMod();
                                            FullnameModel full1 = new FullnameModel();

                                            pmsk1.StoreID = StoreId;
                                            if (!string.IsNullOrEmpty(dr["Most recent upc code"].ToString()))
                                            {
                                                pmsk1.upc = "#" + dr.Field<string>("Most recent upc code").ToString();

                                                full1.upc = "#" + dr.Field<string>("Most recent upc code").ToString();
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            pmsk1.sku = "#" + dr.Field<string>("Item number").ToString();
                                            full1.sku = "#" + dr.Field<string>("Item number").ToString();

                                            full1.pcat = dr.Field<string>("Group name");
                                            full1.pcat1 = dr.Field<string>("Sub-departmentname");
                                            full1.pcat2 = "";

                                            string dqty = dr["Quantity on hand"].ToString();
                                            if (!string.IsNullOrEmpty(dqty))
                                            {
                                                pmsk1.Qty = System.Convert.ToDecimal(dqty);
                                            }

                                            string packRetail1 = dr["Pack retail"].ToString();
                                            BeerPrice = System.Convert.ToDecimal(packRetail1 == "" ? 0 : dr["Pack retail"]);
                                            packsaleprice = dr.Field<string>("Pack Sale Price");
                                            string packs = Regex.Replace(dr["Units per case"].ToString(), @"[^0-9]", "1");
                                            pmsk1.pack = System.Convert.ToInt32(packs);
                                            full1.pack = System.Convert.ToInt32(packs);

                                            if (full1.pcat.ToUpper().Contains("BEER"))
                                            {
                                                pmsk1.Qty = (pmsk1.Qty / pmsk1.pack);
                                                pmsk1.Qty = Math.Floor(pmsk1.Qty);
                                            }

                                            pmsk1.uom = "Case";
                                            full1.uom = "Case";

                                            if (!string.IsNullOrEmpty(dr.Field<string>("Normal description")) && !dr.Field<string>("Normal description").Contains("(DQ"))
                                            {

                                                pmsk1.StoreProductName = dr.Field<string>("Normal description");
                                                pmsk1.StoreDescription = dr.Field<string>("Normal description");
                                                full1.pname = dr.Field<string>("Normal description");
                                                full1.pdesc = dr.Field<string>("Normal description");
                                            }

                                            string price = Regex.Replace(dr["(Warm) Case retail"].ToString(), @"[^0-9.]+", "0");
                                            if (!string.IsNullOrEmpty(price))
                                            {
                                                pmsk1.Price = System.Convert.ToDecimal(price);
                                                full1.Price = System.Convert.ToDecimal(price);
                                            }

                                            pmsk1.sprice = "0";
                                            pmsk1.Start = "";
                                            pmsk1.End = "";



                                            pmsk1.tax = Tax;

                                            full1.country = dr.Field<string>("Country of origin");
                                            full1.region = dr.Field<string>("Region");

                                            pmsk1.Vintage = dr.Field<string>("Vintage");



                                            if (pmsk1.Qty > 0 && pmsk1.Price > 0 && !visionOutofstock.Contains(StoreId.ToString()))
                                            {
                                                prodlist.Add(pmsk1);
                                                fullnamelist.Add(full1);
                                            }
                                        }

                                    }
                                }




                                var filess = directory.GetFiles().ToList();
                                //var aaa = filess.Select(x => x.Name.Contains("Garnet_Wine_and_Liquor.csv"));
                                var G_filepath = directory + "Garnet_Wine_and_Liquor.csv";
                                if (StoreId == 11401 && filess.Count > 1 && File.Exists(G_filepath))
                                {
                                    //var myFile1 = (from f in directory.GetFiles()
                                    //               orderby f.LastWriteTime descending
                                    //               select f).Last();
                                    var myFile1 = "Garnet_Wine_and_Liquor.csv";
                                    string Url1 = BaseUrl + "/" + StoreId + "/Raw/" + myFile1;

                                    dte = ConvertTextToDataTable(Url1);

                                    foreach (DataRow dr in dte.Rows)
                                    {
                                        ProductMod pmsk = new ProductMod();
                                        pmsk.StoreID = StoreId;
                                        pmsk.upc = "#" + dr.Field<string>("UPC").ToString();
                                        pmsk.pack = 1;
                                        pmsk.Qty = 99;
                                        var Case = dr.Field<string>("Case").ToString();
                                        pmsk.uom = "CASEx" + Case + "-" + dr.Field<string>("Size").ToString().Trim();
                                        pmsk.Price = Convert.ToDecimal(dr.Field<string>("Price"));
                                        decimal markup = pmsk.Price + ((pmsk.Price) * 30 / 100);
                                        pmsk.Price = Math.Round(markup, 2);
                                        pmsk.sku = "#" + dr.Field<string>("Item Num").ToString();
                                        pmsk.StoreProductName = dr.Field<string>("Description");
                                        pmsk.StoreDescription = dr.Field<string>("Description");
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        pmsk.Vintage = dr.Field<string>("Vintage");

                                        prodlist.Add(pmsk);
                                    }
                                }
                                if (LimitedProducts.Contains(StoreId.ToString()) && prodlist.Count > 10000)
                                {
                                    Console.WriteLine("Generating Vision " + StoreId + " Product CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    Console.WriteLine("Product File Generated For Vision " + StoreId);
                                    Console.WriteLine();
                                    Console.WriteLine("Generating Vision " + StoreId + " FullNameFile CSV Files.....");
                                    string fullfilename = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                                    Console.WriteLine("Fullname File Generated For Vision " + StoreId);
                                }
                                else if (!LimitedProducts.Contains(StoreId.ToString()))
                                {
                                    Console.WriteLine("Generating Vision " + StoreId + " Product CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    Console.WriteLine("Product File Generated For Vision " + StoreId);
                                    Console.WriteLine();
                                    Console.WriteLine("Generating Vision " + StoreId + " FullNameFile CSV Files.....");
                                    string fullfilename = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                                    Console.WriteLine("Fullname File Generated For Vision " + StoreId);
                                }


                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                if (StoreId == 11401)
                                {

                                    foreach (string filePath in filePaths)
                                    {
                                        GC.Collect();
                                        string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                        if (!filePath.Contains("Garnet_Wine_and_Liquor"))
                                        {
                                            File.Move(filePath, destpath);
                                        }
                                    }
                                    var xfilePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/").ToList();
                                    if (xfilePaths.Count > 1)
                                    {
                                        for (int i = 0; i < xfilePaths.Count - 1; i++)
                                        {
                                            if (xfilePaths.Count > 1)
                                            {
                                                string filePath = BaseUrl + "/" + StoreId + "/Raw/";
                                                var garnetFile = (from f in directory.GetFiles()
                                                                  orderby f.LastWriteTime ascending
                                                                  select f).Last();
                                                string garnetUrl = filePath + garnetFile;
                                                string destpath = BaseUrl + "/" + StoreId + "/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss") + garnetFile;
                                                File.Move(garnetUrl, destpath);
                                                xfilePaths.RemoveAt(i);
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    foreach (string filePath in filePaths)
                                    {
                                        GC.Collect();
                                        string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                        File.Move(filePath, destpath);
                                    }
                                }


                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not Generated File for Vision " + StoreId;
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
            //}
        }

    }
    public class ProductMod
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
        public string sku { get; set; }
        public string uom { get; set; }
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

        public string Vintage { get; set; }
    }
    public class FullnameModel
    {
        public string pname { get; set; }
        public string pdesc { get; set; }
        public string upc { get; set; }
        public string sku { get; set; }
        public decimal Price { get; set; }
        public string uom { get; set; }
        public int pack { get; set; }
        public string pcat { get; set; }
        public string pcat1 { get; set; }
        public string pcat2 { get; set; }
        public string country { get; set; }
        public string region { get; set; }
    }
}


