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
    public class clsGoldenKey
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsGoldenKey(int StoreId, decimal tax)
        {
            try
            {
                GoldenKeyConvertRawFile(StoreId, tax);
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
            return dtResult; //Returning Dattable  
        }
        public string GoldenKeyConvertRawFile(int StoreId, decimal Tax)
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
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel fname = new FullNameProductModel();

                                    pmsk.StoreID = StoreId;
                                    string upc = Regex.Replace(dr["CODE"].ToString(), @"[^0-9]+", "");
                                    if (!string.IsNullOrEmpty(upc))
                                    {
                                        pmsk.upc = "#" + upc.ToString();
                                        fname.upc = "#" + upc.ToString();
                                        pmsk.sku = "#" + upc.ToString();
                                        fname.sku = "#" + upc.ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Qty = 999;
                                    pmsk.StoreProductName = dr.Field<string>("NAME").Replace("=","");
                                    fname.pname = dr.Field<string>("NAME").Replace("=", "");
                                    pmsk.StoreDescription = dr.Field<string>("NAME").Trim().Replace("=", "");
                                    fname.pdesc = dr.Field<string>("NAME").Replace("=", "");
                                    pmsk.Price = System.Convert.ToDecimal(dr["PRICE"]);
                                    fname.Price = System.Convert.ToDecimal(dr["PRICE"]);
                                    if (pmsk.Price <= 0 || fname.Price <= 0)
                                    {
                                        continue;
                                    }
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
                                    fname.pcat = dr.Field<string>("CATGRY");
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.uom = "";
                                    fname.region = "";
                                    fname.country = "";
                                    if (pmsk.Qty > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }

                                }
                                Console.WriteLine("Generating GoldenKey " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For GoldenKey " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating GoldenKey " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For GoldenKey " + StoreId);

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
                                return "Not generated file for GoldenKey " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName or RAW folder is empty!" + StoreId);
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
