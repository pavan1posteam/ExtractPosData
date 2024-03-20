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
    class clsRetailPOSS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsRetailPOSS(int StoreId, decimal Tax)
        {
            try
            {
                RetailPOSSConvertRawFile(StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public static DataTable ConvertCsvToDataTable(string FileName)
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
            return dtResult; //Returning datatable 
        }
        public string RetailPOSSConvertRawFile(int StoreId, decimal Tax)
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

                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["UPC"].ToString().Replace("-", "");
                                        full.upc = "#" + dr["UPC"].ToString().Replace("-", "");

                                    }
                                    else
                                    {
                                        pmsk.upc = '#' + Convert.ToDouble(dr["Store Code (SKU)"]).ToString().Replace(" -", "");

                                    }
                                    decimal qty = Convert.ToDecimal(dr["Quantity"]);
                                    pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                    pmsk.sku = "#" + dr.Field<string>("Store Code (SKU)");
                                    full.sku = "#" + dr.Field<string>("Store Code (SKU)");
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Name")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Name").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("Name").Trim();
                                        full.pname = dr.Field<string>("Name").Trim();
                                        full.pdesc = dr.Field<string>("Name").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    full.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    if (pmsk.Price <= 0 || full.Price <= 0)
                                    {
                                        continue;
                                    }
                                    full.pack = 1;
                                    full.pcat = dr["Department"].ToString();
                                    full.pcat1 = "";
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.uom = dr["Category"].ToString();
                                    pmsk.uom = dr["Category"].ToString();
                                    full.region = "";
                                    pmsk.sprice = 0;
                                    pmsk.pack = 1;
                                    pmsk.Tax = Tax;
                                    if (pmsk.sprice > 0)
                                    {
                                        pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                        pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
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
                                    if (full.pcat.ToUpper() != "TOBACCO" && full.pcat.ToUpper() != "CIGAR" && full.pcat.ToUpper() != "CIGARETTE" && full.pcat.ToUpper() != "MIX" && full.pcat.ToUpper() != "NON-ALCH DRINKS" && full.pcat.ToUpper() != "READY TO DRINK" && full.pcat.ToUpper() != "GIFT ACC." && full.pcat.ToUpper() != "88" && pmsk.Price > 0 && pmsk.Qty > 0)
                                    {
                                        fulllist.Add(full);
                                        prodlist.Add(pmsk);
                                    }
                                }
                                Console.WriteLine("Generating RetailPOSS " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating RetailPOSS " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For RetailPOSS" + StoreId);
                                Console.WriteLine("Fullname File Generated For RetailPOSS" + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    GC.Collect();
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for RetailPOSS " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName" + StoreId;
                        }
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
            return "Completed generating File For RetailPOSS" + StoreId;
        }
    }
}
