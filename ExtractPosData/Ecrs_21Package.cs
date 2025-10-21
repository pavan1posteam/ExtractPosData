using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Microsoft.VisualBasic.FileIO;

namespace ExtractPosData
{
    class Ecrs_21Package
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string StaticQty = ConfigurationManager.AppSettings["StaticQty"];
        string Quantity = ConfigurationManager.AppSettings["Quantity"];
        public Ecrs_21Package(int storeId, decimal Tax)
        {
            try
            {
                Ecrs_21PackageConvertRawFile(storeId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               ///// Console.Read();
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
            dt.Columns.Add("alt6");
            dt.Columns.Add("alt7");
           
            try
            {
                var parser = new TextFieldParser(filename);
                bool headerRow = true;
                parser.SetDelimiters("|");
                
                while (!parser.EndOfData)
                {
                    var currentRow = parser.ReadFields();
                    currentRow = currentRow.Select(s => s.Replace("'", "")).ToArray();
                    currentRow = currentRow.Select(s => s.Trim()).ToArray();
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
        public string Ecrs_21PackageConvertRawFile(int StoreId, decimal Tax)
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
                                        if (StaticQty.Contains(StoreId.ToString()))
                                            pmsk.Qty = 999;
                                        else
                                        {
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
                                        full.pcat = dr["cat"].ToString();
                                        if (StoreId==10535)
                                        {
                                            if (full.pcat.ToUpper() == "DELETE DEPT." || full.pcat.ToUpper().Contains("ALLOCATIONS") || full.pcat.ToUpper() == "CIGARETTES/SMOKELESS" || full.pcat.ToUpper() == "CIGARS/COMMERCIAL" || full.pcat.ToUpper() == "CIGARS/HUMIDOR" || full.pcat.ToUpper() == "Allocated" || full.pcat.ToUpper() == "CIGARETTES")
                                            {
                                                continue;
                                            }
                                        }
                                        
                                        full.uom = dr["uom"].ToString();
                                        pmsk.uom = dr["uom"].ToString();
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        full.pack = 1;
                                        pmsk.Tax = Tax;
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        if (Quantity.Contains(StoreId.ToString()) && pmsk.Price > 0 && pmsk.Qty>0 && full.pcat != "Tobacco")
                                        {
                                            fulllist.Add(full);
                                            prodlist.Add(pmsk);
                                        }
                                        else if (pmsk.Price > 0 && full.pcat != "Tobacco")
                                        {
                                            fulllist.Add(full);
                                            prodlist.Add(pmsk);
                                        }
                                    }
                                    catch (Exception e)
                                    { Console.WriteLine(e.Message); }
                                }
                                Console.WriteLine("Generating Ecrs_21Package " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating Ecrs_21Package " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Ecrs_21Package" + StoreId);
                                Console.WriteLine("Fullname File Generated For Ecrs_21Package" + StoreId);

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
                                //return "Not generated file for Ecrs_21Package " + StoreId;
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
            return "Completed generating File For Ecrs_21Package" + StoreId;
        }
    }
}
