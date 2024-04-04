using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace ExtractPosData
{
    public class clsCustomSoftwarePOS
    {
        public clsCustomSoftwarePOS(int StoreId, decimal tax)
        {
            try
            {
                CustomSoftwareConvertRawFile(StoreId, tax);
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
            return dtResult; //Returning datatable 
        }


        public string CustomSoftwareConvertRawFile(int StoreId, decimal tax)
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
                                    ProductsModel pmsk = new ProductsModel();

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        var upc = dr["UPC"].ToString().Replace(" -", "");
                                        if (upc.Length >= 10)
                                        {
                                            upc = upc.Trim('0');
                                        }
                                        pmsk.upc = "#" + upc;
                                        pmsk.sku = pmsk.upc;

                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    pmsk.StoreProductName = dr["Product Name"].ToString();
                                    pmsk.StoreDescription = dr["Product Name"].ToString();
                                    pmsk.uom = dr["Product Size"].ToString();
                                    pmsk.Price = Convert.ToDecimal(dr["Retail Price"]);
                                    pmsk.pack = 1;
                                    pmsk.Qty = 999;
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    pmsk.Tax = tax;
                                    pmsk.sprice = 0;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.Deposit = 0;

                                    if (pmsk.Price > 0 && pmsk.Qty > 0)
                                    {
                                        prodlist.Add(pmsk);
                                    }


                                }
                                Console.WriteLine("Generating CustomSoftwarePOS " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For CustomSoftwarePOS" + StoreId);
                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            return "Invalid FileName" + StoreId;
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
            }
            return "Completed generating File For CustomSoftwarePOS" + StoreId;


        }
    }
}