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
using System.Text.RegularExpressions;


namespace ExtractPosData
{
    class clsPosX
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsPosX(int storeId, decimal Tax)
        {
            try
            {
                PosXConvertRawFile(storeId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            DataTable dt = new DataTable();
            try
            {
                using (TextFieldParser parser = new TextFieldParser(FileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.Delimiters = new string[] { "," };
                    parser.HasFieldsEnclosedInQuotes = true;
                    string[] columns = parser.ReadFields();
                    foreach (string columnName in columns)
                    {
                        dt.Columns.Add(columnName, typeof(string));
                    }

                    // Read remaining lines
                    while (!parser.EndOfData)
                    {
                        var fields = parser.ReadFields().ToList();
                        if (fields.Contains("1752201671"))
                        {

                        }
                        DataRow newRow = dt.NewRow();
                        if (fields[0] == "" && Regex.IsMatch(fields[1], @"\d{5,}"))
                        {
                            fields.RemoveAt(0);
                        }

                        if (!Regex.IsMatch(fields[2], @"\d+[.]\d+"))
                        {
                            var idx = fields.FindIndex(a => Regex.IsMatch(a, @"\d+\.\d+"));
                            var cc = string.Join(",", fields.GetRange(1, idx - 1)).Replace(",", "");
                            fields.RemoveRange(1, idx - 1);
                            fields.Insert(1, cc);
                        }
                        if (fields.Count > 32)
                        {
                            fields.RemoveRange(32, fields.Count - 32);
                        }
                        for (int i = 0; i < fields.Count; i++)
                        {
                            newRow[i] = fields[i];
                        }
                        dt.Rows.Add(newRow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt;
        }
        public string PosXConvertRawFile(int StoreId, decimal Tax)
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
                                string[] lines = File.ReadAllLines(Url);
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    if (lines[i].Contains("\""))
                                    {
                                        lines[i] = lines[i].Replace("\"", "");
                                    }
                                }
                                File.WriteAllLines(Url, lines);

                                DataTable dt = ConvertCsvToDataTable(Url);
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    try
                                    {
                                        ProductsModel pmsk = new ProductsModel();
                                        FullNameProductModel full = new FullNameProductModel();
                                        pmsk.StoreID = StoreId;
                                        if (dr.Field<string>("Code") == "0")
                                        {

                                        }
                                        if (string.IsNullOrEmpty(dr.Field<string>("Code")))
                                        {
                                            continue;
                                        }
                                        pmsk.sku = "#" + dr.Field<string>("Code");
                                        full.sku = "#" + dr.Field<string>("Code");
                                        pmsk.upc = "#" + dr["Code"].ToString();
                                        full.upc = "#" + dr["Code"].ToString();


                                        decimal qty = Convert.ToDecimal(dr["Onhand"]);
                                        pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;



                                        if (!string.IsNullOrEmpty(dr.Field<string>("Description")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("Description").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("Description").Trim();
                                            full.pname = dr.Field<string>("Description").Trim();
                                            full.pdesc = dr.Field<string>("Description").Trim();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.Price = System.Convert.ToDecimal(dr["PRICE"] == DBNull.Value ? 0 : dr["PRICE"]);
                                        full.Price = System.Convert.ToDecimal(dr["PRICE"] == DBNull.Value ? 0 : dr["PRICE"]);
                                        full.pcat = dr.Field<string>("Dept").Trim();
                                        full.pcat1 = "";
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";
                                        pmsk.sprice = 0;
                                        pmsk.pack = Convert.ToInt32(dr.Field<string>("Pack"));
                                        full.pack = Convert.ToInt32(dr.Field<string>("Pack"));
                                        pmsk.uom = dr.Field<string>("SIZE");
                                        full.uom = dr.Field<string>("SIZE");
                                        pmsk.Tax = Tax;

                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";

                                        if (pmsk.Price > 0)
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
                                Console.WriteLine("Generating PosX " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating PosX " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For PosX" + StoreId);
                                Console.WriteLine("Fullname File Generated For PosX" + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for PosX " + StoreId;
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
            return "Completed generating File For PosX" + StoreId;
        }
    }

}
