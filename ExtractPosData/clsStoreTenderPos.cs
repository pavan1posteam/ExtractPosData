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
    public class clsStoreTenderPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsStoreTenderPos(int storeid, decimal tax)
        {
            try
            {
                StoreTenderPosConvertRawFile(storeid, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static DataTable ConvertCsvToDataTable(string filePath)
        {
            DataTable dt = new DataTable();
            var lines = File.ReadAllLines(filePath);

            if (lines.Length > 6)
            {
                string[] headers = lines[6].Split(',');
                foreach (var header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }
                for (int i = 7; i < lines.Length; i++)
                {
                    string[] rowValues = lines[i].Split(',');
                    DataRow row = dt.NewRow();
                    for (int j = 0; j < rowValues.Length; j++)
                    {
                        if (IsRequiredColumn(headers[j]))
                        {
                            row[j] = rowValues[j].Trim();
                        }
                    }
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

        public static bool IsRequiredColumn(string columnName)
        {
            List<string> requiredColumns = new List<string>
            {
                "Plu Number", "On Hand", "Description", "Price", "Pk", "Size" 
            };
            return requiredColumns.Contains(columnName.Trim());
        }

        public string StoreTenderPosConvertRawFile(int StoreId, decimal Tax)
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
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel fname = new FullNameProductModel();

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["Plu Number"].ToString()))
                                    {
                                        var upc = "#" + dr["Plu Number"].ToString().ToLower();
                                        string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                        if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                        {
                                            if (!string.IsNullOrEmpty(numberUpc))
                                            {
                                                pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                fname.upc = "#" + dr["Plu Number"].ToString();
                                                pmsk.sku = "#" + dr["Plu Number"].ToString();
                                                fname.sku = "#" + dr["Plu Number"].ToString();
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    pmsk.Qty = Convert.ToInt32(dr["On Hand"]);

                                    pmsk.StoreProductName = dr["Description"]?.ToString();
                                    pmsk.StoreDescription = dr["Description"]?.ToString();
                                    fname.pdesc = dr["Description"]?.ToString();
                                    fname.pname = dr["Description"]?.ToString();


                                    pmsk.Price = Convert.ToDecimal(dr["Price"].ToString().Replace("$", ""));
                                    fname.Price = Convert.ToDecimal(dr["Price"].ToString().Replace("$", ""));


                                    pmsk.sprice = 0;
                                    pmsk.pack = Convert.ToInt32(dr["Pk"]);
                                    pmsk.Tax = Tax;

                                    pmsk.Start = "";
                                    pmsk.End = "";


                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    pmsk.Deposit = 0;
                                    fname.pcat = "";
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.uom = dr.Field<string>("Size");
                                    fname.region = "";
                                    fname.country = "";
                                    if (pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                        
                                    }
                                }

                                Console.WriteLine("Generating clsStoreTenderPos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For clsStoreTenderPos " + StoreId);
                                Console.WriteLine("Generating clsStoreTenderPos " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For clsStoreTenderPos " + StoreId);
                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for StoreTenderPos " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid FileName or Raw Folder is Empty! " + StoreId);
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
