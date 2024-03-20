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
    class clsPossystemsinc
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsPossystemsinc(int StoreId, decimal Tax)
        {
            try
            {
                INCConvertRawFile(StoreId, Tax);
            }
             catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public DataTable ConvertCsvToDataTable(string filename)
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("STORE ID");
            dt.Columns.Add("UPC");
            dt.Columns.Add("ONHAND QTY");
            dt.Columns.Add("SKU");
            dt.Columns.Add("SIZE");
            dt.Columns.Add("BRAND");
            dt.Columns.Add("DESCRIPTION");
            dt.Columns.Add("PRICE");
            dt.Columns.Add("SalePrice");
            dt.Columns.Add("SALES TAX");

            try
            {

                var parser = new TextFieldParser(filename);
                bool headerRow = true;
                parser.SetDelimiters(",");
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
        public string INCConvertRawFile(int StoreId,decimal Tax)
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
                                        if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                        {
                                            pmsk.upc =  dr["UPC"].ToString().Replace('"', ' ').Trim();
                                            full.upc =  dr["UPC"].ToString().Replace('"', ' ').Trim();
                                            pmsk.sku =  dr.Field<string>("SKU").Replace('"', ' ').Trim();
                                            full.sku =  dr.Field<string>("SKU").Replace('"', ' ').Trim();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        var y = dr["ONHAND QTY"].ToString();

                                        if (y != "")
                                        {
                                            var Qtyy = Convert.ToDecimal(y);

                                            pmsk.Qty = Convert.ToInt32(Qtyy);
                                        }
                                        else
                                        {
                                            pmsk.Qty = Convert.ToInt32(null);
                                        }

                                        if (!string.IsNullOrEmpty(dr.Field<string>("DESCRIPTION")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("DESCRIPTION").Replace('"', ' ').Trim();
                                            pmsk.StoreDescription = dr.Field<string>("DESCRIPTION").Replace('"', ' ').Trim();
                                            full.pname = dr.Field<string>("DESCRIPTION").Replace('"', ' ').Trim();
                                            full.pdesc = dr.Field<string>("DESCRIPTION").Replace('"', ' ').Trim();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        var X = dr["PRICE"].ToString();

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
                                      
                                        full.pcat = dr["BRAND"].ToString().Replace('"', ' ').Trim();
                                        pmsk.uom = dr["SIZE"].ToString().Replace('"', ' ').Trim();
                                        full.uom = dr["SIZE"].ToString().Replace('"', ' ').Trim();
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";

                                        var S = dr["SalePrice"].ToString();

                                        if(S != "")
                                        {
                                            var SPR = Convert.ToDecimal(S);

                                            pmsk.sprice = Convert.ToDecimal(SPR);
                                        }
                                        if(pmsk.sprice > 0)
                                        {
                                            pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                            //pmsk.End = "12/31/22";  // As per  ticket 13096
                                            pmsk.End = "12/31/2999";  // As per  ticket #13096
                                        }
                                        
                                        pmsk.pack = 1;
                                        full.pack = 1;
                                        var T = dr["SALES TAX"].ToString();
                                        if(T != "")
                                        {
                                            var tax = Convert.ToDecimal(T);
                                            pmsk.Tax = Convert.ToDecimal(tax);
                                        }                                      
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        if (pmsk.Price > 0 && pmsk.Qty > 0 && full.pcat != "Tobacco" && full.pcat != "CIGAR BOX" && full.pcat != "CIGAR CITY BREWING")
                                        {
                                            fulllist.Add(full);
                                            prodlist.Add(pmsk);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message); 
                                    }
                                }
                                Console.WriteLine("Generating INC " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating INC " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For INC" + StoreId);
                                Console.WriteLine("Fullname File Generated For INC" + StoreId);

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
                                //return "Not generated file for INC " + StoreId;
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
            return "Completed generating File For INC" + StoreId;
        }

    }
}
