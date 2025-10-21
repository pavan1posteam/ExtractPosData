using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtractPosData
{
    class RevelMapping
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        public RevelMapping(int StoreId, decimal tax)
        {
            try
            {
                NRSConvertRawFile(StoreId, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private DataTable ConvertCsvToDataTable(string FileName)
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
        private string NRSConvertRawFile(int StoreId, decimal tax)
        {
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
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    pmsk.StoreID = StoreId;
                                    string upc = Regex.Replace(dr["Barcode"].ToString(), @"[^0-9]+", "");
                                    string sku = Regex.Replace(dr["SKU"].ToString(), @"[^0-9]+", "");
                                    if (!string.IsNullOrEmpty(upc) && upc != "")
                                    {
                                        pmsk.upc = "#" + upc.ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.sku = !string.IsNullOrEmpty(sku) && sku != "" ? "#" + sku.ToString() : pmsk.upc;
                                    Decimal qty = Convert.ToDecimal(dr["Quantity in Hand"]);
                                    pmsk.Qty = Convert.ToInt32(qty);
                                    pmsk.StoreProductName = dr.Field<string>("Name");
                                    pmsk.StoreDescription = dr.Field<string>("Name");
                                    string ccProdname = pmsk.StoreProductName.ToUpper();
                                    Regex filter = new Regex(@"(\d+(?:\.\d+)?\s*(?:LB|ML|L|OZ|PK|sOZ|oz+))");
                                    var match = filter.Match(ccProdname);
                                    if (match.Success)
                                    {
                                        pmsk.uom = match.Value;
                                    }
                                    else
                                    {
                                        pmsk.uom = "";
                                    }
                                    string prc = dr["Price"].ToString().Replace("$", "");
                                    pmsk.Price = Convert.ToDecimal(prc);
                                    pmsk.sprice = 0;
                                    pmsk.pack = 1;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.Tax = tax;
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if (pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                    }

                                }
                                Console.WriteLine("Generating RevelMapping " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For RevelMapping " + StoreId);
                                Console.WriteLine();
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
                                return "Not generated file for NRSPos " + StoreId;
                            }

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
            return "Completed generating File For RevelMapping" + StoreId;
        }


    }
}
