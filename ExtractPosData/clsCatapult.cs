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
    class clsCatapult
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsCatapult(int StoreId, decimal liquortax, decimal beertax, decimal winetax, decimal Tax)
        {
            try
            {
                CatapultConvertRawFile(StoreId, liquortax, beertax, winetax, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public DataTable ConvertCsvToDataTable(string filename)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("cat");
            dt.Columns.Add("upc");
            dt.Columns.Add("brand");
            dt.Columns.Add("name");
            dt.Columns.Add("uom");
            dt.Columns.Add("description");
            dt.Columns.Add("price");
            dt.Columns.Add("qty");
            dt.Columns.Add("alt");
            dt.Columns.Add("alt2");
            dt.Columns.Add("alt3");
            dt.Columns.Add("alt4");
            dt.Columns.Add("alt5");
            try
            {
                var parser = new TextFieldParser(filename);
                bool headerRow = true;
                parser.SetDelimiters("|");
                while (!parser.EndOfData)
                {
                    var currentRow = parser.ReadFields();
                    if (headerRow == false)
                    {
                        foreach (var field in currentRow)
                        {

                            dt.Columns.Add(field, typeof(object));
                        }
                        headerRow = false;
                    }
                    else
                    {
                        dt.Rows.Add(currentRow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt;
        }
        public string CatapultConvertRawFile(int StoreId, decimal liquortax, decimal beertax, decimal winetax, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
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
                                DataTable dt = ConvertCsvToDataTable(Url);

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    try
                                    {
                                        ProductsModel pmsk = new ProductsModel();
                                        FullNameProductModel full = new FullNameProductModel();
                                        pmsk.StoreID = StoreId;
                                        if (!string.IsNullOrEmpty(dr["upc"].ToString()))
                                        {
                                            pmsk.upc = "#" + dr["upc"].ToString();
                                            full.upc = "#" + dr["upc"].ToString();
                                            pmsk.sku = "#" + dr.Field<string>("upc");
                                            full.sku = "#" + dr.Field<string>("upc");
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        var y = dr["qty"].ToString();

                                        if (y != "")
                                        {
                                            var Qtyy = Convert.ToDecimal(y);

                                            pmsk.Qty = Convert.ToInt32(Qtyy);
                                        }
                                        else
                                        {
                                            pmsk.Qty = Convert.ToInt32(null);
                                        }

                                        if (!string.IsNullOrEmpty(dr.Field<string>("name")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("name").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("name").Trim();
                                            full.pname = dr.Field<string>("name").Trim();
                                            full.pdesc = dr.Field<string>("name").Trim();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        var X = dr["price"].ToString();

                                        if (X != "")
                                        {
                                            var PRC = Convert.ToDecimal(X);

                                            pmsk.Price = Convert.ToDecimal(PRC);
                                            full.Price = Convert.ToDecimal(PRC);
                                        }
                                        else
                                        {
                                            pmsk.Price = Convert.ToDecimal(null);
                                            full.Price = Convert.ToDecimal(null);
                                        }
                                        full.pcat = dr["cat"].ToString().Replace("01", "").Replace("02", "").Replace("03", "").Replace("04", "").Trim();
                                        full.uom = dr["uom"].ToString();
                                        pmsk.uom = dr["uom"].ToString();
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        full.pack = 1;
                                        if (StoreId == 11275 || StoreId == 11309)
                                        {
                                            if (full.pcat.ToUpper() == "WINE")
                                            {
                                                pmsk.Tax = winetax;
                                            }
                                            else if (full.pcat.ToUpper() == "LIQUOR")
                                            {
                                                pmsk.Tax = liquortax;
                                            }
                                            else if (full.pcat.ToUpper() == "BEER")
                                            {
                                                pmsk.Tax = beertax;
                                            }

                                            else
                                            {
                                                pmsk.Tax = Tax;
                                            }
                                        }
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        if (StoreId == 11309) // Irrespective of Qty as per ticket 12194
                                        {
                                            if (pmsk.Price > 0)
                                            {
                                                fulllist.Add(full);
                                                prodlist.Add(pmsk);
                                            }
                                        }

                                        else if (pmsk.Price > 0 && pmsk.Qty > 0 && full.pcat.ToUpper() != "TOBACCO")
                                        {
                                            fulllist.Add(full);
                                            prodlist.Add(pmsk);
                                        }
                                    }
                                    catch (Exception e)
                                    { Console.WriteLine(e.Message); }
                                }
                                Console.WriteLine("Generating Catapult " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating Catapult " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Catapult" + StoreId);
                                Console.WriteLine("Fullname File Generated For Catapult" + StoreId);

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
                                //(new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                //return "Not generated file for Catapult " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName";
                        }
                    }
                }
                else
                {
                    return "Invalid Sub-Directory";
                }
            }
            else
            {
                return "Invalid Directory";
            }
            return "Completed generating File For Catapult" + StoreId;
        }
    }
}
