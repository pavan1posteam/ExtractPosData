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
    public class clsGeneralStorePOS
    {
        public clsGeneralStorePOS(int StoreId, decimal tax)
        {
            try
            {
                GeneralStorePosConvertRawFile(StoreId, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static List<string> ConvertCsvToList(string filename)
        {
            var dataLines = File.ReadAllText(filename).Split('\n').ToList();
            dataLines.RemoveAll(a => a.Contains("\"ITEM NUMBER\",\"DESCRIPTION\",\"CONTROL FLAG\",\"TAX\",\"DEPT\",\"CAT\",\"On Hand\",\"On Order\",\"Order Point\",\"Order UpTo\",\"COST\",\"PRICE-1\",\"PRICE-2\",\"PRICE-3\",\"PRICE-4\",\"VENDOR 1\",\"ORDERNO 1\",\"COST 1\",\"VENDOR 2\",\"ORDERNO 2\",\"COST 2\",\"VENDOR 3\",\"ORDERNO 3\",\"COST-3\",\"VENDOR 4\",\"ORDERNO 4\",\"COST 4\",\"VENDOR 5\",\"ORDERNO 5\",\"COST 5\",\"UNITSINPACK\",\"BULKITEM\",\"SUBCAT\",\"ALTLOOK\",\"ALTLOOK2\",\"LOCATION\",\"FOODSTAMP\",\"AGE\",\"TAGALONG\",\"TAGQTY\",\"COMMISSION\",\"QTYPROMPT\",\"PRICE-5\",\"PRICE-6\",\"CaseQty-1\",\"CaseCost-1\",\"CaseQty-2\",\"CaseCost-2\",\"CaseQty-3\",\"CaseCost-3\",\"CaseQty-4\",\"CaseCost-4\",\"CaseQty-5\",\"CaseCost-5\""));


            for (int k = 0; k < dataLines.Count; k++)
            {
                dataLines[k] = dataLines[k].Replace("\"", "");

                ProductModel pmodel = new ProductModel();
                var xdata = dataLines[k];
                var data = dataLines[k].Split(',').ToList();
                int idx = data.FindIndex(a => Regex.IsMatch(a, @"^-\d+$"));
                if(idx > 6)
                {
                    idx = idx - 7;
                    int idx2 = idx + 1;
                    data[idx] = data[idx] + " " + data[idx2] + " " + data[idx+2];
                    data.RemoveRange(idx2, 2);

                }

                var hh = string.Join(",", data).TrimEnd('\r');
                dataLines[k] = hh;

            }
            return dataLines;

        }



        public string GeneralStorePosConvertRawFile(int StoreId, decimal tax)
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
                                List<ProductModel> prodlist1 = new List<ProductModel>();
                                List<FullnameModelFile> full = new List<FullnameModelFile>();
                                foreach (string products in dt)
                                {
                                    try
                                    {
                                        ProductsModel pmsk = new ProductsModel();
                                        FullnameModelFile fname = new FullnameModelFile(); string[] elements = products.Split(',');

                                        pmsk.StoreID = StoreId;
                                       
                                        if (!string.IsNullOrEmpty(elements[0]) && elements[0] != "")
                                        {
                                            string numericUpc = new string(elements[0].Where(char.IsDigit).ToArray());

                                            if (!string.IsNullOrEmpty(numericUpc))
                                            {

                                                pmsk.upc = "#" + elements[0].ToString().TrimStart('0');
                                                fname.upc = "#" + elements[0].ToString().TrimStart('0');
                                                pmsk.sku = "#" + elements[0].ToString().TrimStart('0');
                                                fname.sku = "#" + elements[0].ToString().TrimStart('0');
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(elements[6]))
                                        {
                                            string qtyString = elements[6].Trim(); 

                                            qtyString = qtyString.Replace("-", "");
                                            qtyString = qtyString.Replace(".", ""); 
                                            if (int.TryParse(qtyString, out int qty)) 
                                            {
                                                pmsk.Qty = qty; 
                                            }

                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.pack = 1;
                                        fname.pack = 1;
                                        pmsk.Tax = tax;
                                        if (!string.IsNullOrEmpty(elements[1]) && elements[1] != "")
                                        {
                                            pmsk.StoreProductName = elements[1];
                                            fname.pname = elements[1].Trim();
                                            pmsk.StoreDescription = elements[1];
                                            fname.pdesc = elements[1].Trim();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.uom = "";
                                        string result = Regex.Replace(elements[11], "\\$", "");
                                        pmsk.Price = Convert.ToDecimal(result);
                                        fname.Price = Convert.ToDecimal(result);
                                        pmsk.sprice = 0;
                                        fname.pcat = elements[4].ToString();
                                        fname.pcat1 = "";
                                        fname.pcat2 = "";
                                        fname.region = "";
                                        fname.country = "";

                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        if(pmsk.Price>0 && pmsk.Qty >0)
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                    
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                Console.WriteLine("Generating GeneralStorePos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For GeneralStorePos " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating GeneralStorePos " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FullName", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For GeneralStorePos " + StoreId);

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
            return "Completed generating Files For GeneralStorePOS" + StoreId;


        }
    }
}