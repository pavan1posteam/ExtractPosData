using System;
using System.Collections.Generic;
using System.Linq;
using ExtractPosData.Models;
using System.IO;
using System.Configuration;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace ExtractPosData
{
    class clsHonorPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsHonorPos(int storeId, decimal Tax)
        {
            try
            {
                HonorPosConvertRawFile(storeId, Tax);
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

                    for (int i = 0; i < columns.Length; i++)
                    {
                        dt.Columns.Add(columns[i], typeof(string));
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        DataRow newrow = dt.NewRow();

                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (dt.Columns.Count != fields.Length)
                            {
                                break;
                            }
                            newrow[i] = fields[i];
                        }

                        dt.Rows.Add(newrow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt;
        }
        public string HonorPosConvertRawFile(int StoreId, decimal Tax)
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
                                foreach (DataRow dr in dt.Rows)
                                {
                                    try
                                    {
                                        ProductsModel pmsk = new ProductsModel();
                                        FullNameProductModel full = new FullNameProductModel();
                                        pmsk.StoreID = StoreId;
                                        if (string.IsNullOrEmpty(dr.Field<string>("ItemNo")))
                                        {
                                            continue;
                                        }
                                        pmsk.sku = "#" + dr.Field<string>("ItemNo");
                                        pmsk.upc = "#" + dr["ItemNo"].ToString();
                                        pmsk.Qty = 9999;
                                        if (!string.IsNullOrEmpty(dr.Field<string>("L1")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("L1").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("L1").Trim();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.Price = Convert.ToDecimal(dr["Price1"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Price1"])/100);
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        pmsk.uom = "";
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
                                            prodlist.Add(pmsk);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                }
                                Console.WriteLine("Generating HonorPos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For HonorPos" + StoreId);

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
                                return "Not generated file for HonorPos " + StoreId;
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
            return "Completed generating File For HonorPos" + StoreId;
        }
    }

}
