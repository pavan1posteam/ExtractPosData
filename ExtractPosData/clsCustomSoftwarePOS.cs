using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
        public static List<string> ConvertCsvToList(string filename)
        {
            var dataLines = File.ReadAllText(filename).Split('\n').ToList();
            dataLines.RemoveAll(a => a.Contains("Product Name,Item Number,Product Size,Retail Price,Tax Rate,Sale Price,Sale Date Range,Quanty On Hand,UPC 1,UPC 2,UPC 3"));


            for (int k = 0; k < dataLines.Count; k++)
            {
                dataLines[k] = dataLines[k].Replace("\"", "");

                ProductModel pmodel = new ProductModel();
                var xdata = dataLines[k];
                var data = dataLines[k].Split(',').ToList();
                int idx = data.FindIndex(a => Regex.IsMatch(a, @"^-\d+$"));
                

                var hh = string.Join(",", data).TrimEnd('\r');
                dataLines[k] = hh;

            }
            return dataLines;

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
                                List<string> dt = ConvertCsvToList(Url);
                                dt.RemoveAll(a => a.Length <= 0);

                                List<ProductsModel> prodlist = new List<ProductsModel>();

                                foreach (string products in dt)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    string[] elements = products.Split(',');

                                    pmsk.StoreID = StoreId;
                                    string numericUpc = new string(elements[9].Where(char.IsDigit).ToArray());

                                    string otherUPC = new string(elements[8].Where(char.IsDigit).ToArray());

                                    if (!string.IsNullOrEmpty(elements[9]) && elements[9] != "")
                                    {
                                        if (!string.IsNullOrEmpty(numericUpc))
                                        {

                                            pmsk.upc = "#" + elements[9].ToString();
                                           
                                        }

                                    }
                                    else if (!string.IsNullOrEmpty(otherUPC))
                                    {
                                        pmsk.upc = "#" + elements[8].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    if (!string.IsNullOrEmpty(elements[1]))
                                    {
                                        pmsk.sku = "#" + elements[1].ToString();
                                    }

                                    pmsk.StoreProductName = elements[0].ToString();
                                    pmsk.StoreDescription = elements[0].ToString();
                                    pmsk.uom = elements[2].ToString();
                                    pmsk.Price = Convert.ToDecimal(elements[3]);
                                    pmsk.pack = 1;
                                    pmsk.Qty = Convert.ToInt32(elements[7]);
                                    if (!string.IsNullOrEmpty(elements[10]))
                                    {
                                        pmsk.altupc1 = "#" + elements[10].ToString();
                                    }
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