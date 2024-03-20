using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ExtractPosData.Models;
using System.IO;
using System.Data.OleDb;
using Microsoft.VisualBasic.FileIO;
using System.Configuration;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExtractPosData
{
    class clsLiqReg_Market_11181
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsLiqReg_Market_11181(string FileName, string FileName2, int StoreId, decimal Tax)
        {
            try
            {
                LiqReg_Market_ConvertRawFile(FileName, FileName2, StoreId, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            return dtResult; //Returning Dattable  
        }

        public static DataTable ConvertCsvToDataTable2(string FileName2)
        {
            DataTable dtResult = new DataTable();
            using (TextFieldParser parser = new TextFieldParser(FileName2))
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

        public string LiqReg_Market_ConvertRawFile(string PosFileName, string PosFileName2, int StoreId, decimal Tax)
        {
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    var a = directory.GetFiles().FirstOrDefault();
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();
                        var myFile2 = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).Last();

                        string Url = BaseUrl + "/" + StoreId + "/Raw/" + myFile;
                        string Url2 = BaseUrl + "/" + StoreId + "/Raw/" + myFile2;

                        if (File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = ConvertCsvToDataTable(Url);
                                DataTable dt2 = ConvertCsvToDataTable2(Url2);

                                dt.Merge(dt2);
                                var cnt = dt.Rows.Count;

                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullnameModel> full = new List<FullnameModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullnameModel fname = new FullnameModel();

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["upc"].ToString()))
                                    {
                                        pmsk.upc = dr["upc"].ToString();
                                        fname.upc = dr["upc"].ToString();
                                        pmsk.sku = dr["upc"].ToString();
                                        fname.sku = dr["upc"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    String qty = (dr["qty"].ToString());

                                    if (!string.IsNullOrEmpty(qty))
                                    {
                                        var qtyy = Convert.ToInt32(qty);
                                        pmsk.Qty = System.Convert.ToInt32(qtyy) > 0 ? Convert.ToInt32(qtyy) : 0;
                                    }
                                    // pmsk.Qty = System.Convert.ToInt32(dr["qty"] == DBNull.Value ? 0 : dr["qty"]);
                                    pmsk.StoreProductName = dr.Field<string>("StoreProductName");
                                    fname.pname = dr.Field<string>("StoreProductName");
                                    pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
                                    fname.pdesc = dr.Field<string>("StoreProductName");
                                    pmsk.Price = System.Convert.ToDecimal(dr["price"].ToString().Replace("$", string.Empty));
                                    fname.Price = System.Convert.ToDecimal(dr["price"].ToString().Replace("$", string.Empty));

                                    pmsk.sprice = 0;

                                    pmsk.pack = Convert.ToInt32(dr["pack"]);
                                    pmsk.Tax = Tax;

                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    fname.pack = Convert.ToInt32(dr["pack"]);
                                    fname.pcat = dr["cat"].ToString();
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.uom = dr.Field<string>("size");
                                    pmsk.uom = dr.Field<string>("size");
                                    fname.region = "";
                                    fname.country = "";
                                    if (pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        full.Add(fname);
                                        full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                    }
                                }
                                Console.WriteLine("Generating LiqReg_MarketPOS " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For LiqReg_MarketPOS " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating LiqReg_MarketPOS " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For LiqReg_MarketPOS " + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                //foreach (string filePath in filePaths)
                                //{
                                //    GC.Collect();
                                //    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                //    File.Move(filePath, destpath);
                                //}
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for LiqReg_MarketPOS " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid FileName or RAW folder is empty!" + StoreId);
                        return "";
                    }
                }
                else
                {
                    return "Invalid Sub-Directory " + StoreId;
                }
            }
            else
            {
                return "Invalid Directory " + StoreId;
            }
            return "Completed generating File";

        }
    }
}
