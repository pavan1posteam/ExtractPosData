using ExtractPosData.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace ExtractPosData
{
    class clsRAPIDRMSMappingTool
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        string SQty = ConfigurationManager.AppSettings["StaticQty"];
        string DiffQty = ConfigurationManager.AppSettings["GreaterThenZeroQtyRapidRms"];


        public clsRAPIDRMSMappingTool(int StoreID, decimal tax, int StoreMapId)
        {

            try
            {
                RAPIDRMSConvertRawFile(StoreID, tax, StoreMapId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static List<string> ConvertCsvToList(string filename)
        {
            var dataLines = File.ReadAllText(filename).Split('\n').ToList();
            dataLines.RemoveAll(a => a.Contains("UPC,ItemName,Price,QOH,Department"));


            for (int k = 0; k < dataLines.Count; k++)
            {
                ProductModel pmodel = new ProductModel();
                var xdata = dataLines[k];
                var data = dataLines[k].Split(',').ToList();
                int idx = data.FindIndex(a => Regex.IsMatch(a, @"^\d+\.\d+$"));

                if (idx > 2)
                {
                    idx = idx - 2;
                    int idx2 = idx + 1;
                    data[idx] = data[idx] + " " + data[idx2];
                    data.RemoveAt(idx2);

                }
                var hh = string.Join(",", data).TrimEnd('\r');
                dataLines[k] = hh;

            }
            return dataLines;

        }

        public string RAPIDRMSConvertRawFile(int StoreId, decimal tax, int StoreMapId)
        {
            try
            {
                if (Directory.Exists(BaseUrl))
                {
                    if (Directory.Exists(BaseUrl + "/" + StoreMapId + "/Raw/"))
                    {
                        var directory = new DirectoryInfo(BaseUrl + "/" + StoreMapId + "/Raw/");
                        if (directory.GetFiles().FirstOrDefault() != null)
                        {
                            var myFile = (from f in directory.GetFiles()
                                          orderby f.LastWriteTime descending
                                          select f).First();

                            string Url = BaseUrl + "/" + StoreMapId + "/Raw/" + myFile;
                            if (File.Exists(Url))
                            {
                                try
                                {

                                    List<string> dt = ConvertCsvToList(Url);
                                    dt.RemoveAll(a => a.Length <= 0);
                                    List<ProductsModelSRAPIDRMS> prodlist = new List<ProductsModelSRAPIDRMS>();
                                    List<FullnameModelFile> full = new List<FullnameModelFile>();
                                    foreach (string products in dt)
                                    {
                                        try
                                        {
                                            ProductsModelSRAPIDRMS pmsk = new ProductsModelSRAPIDRMS();
                                            FullnameModelFile fname = new FullnameModelFile();
                                            string[] elements = products.Split(',');

                                            pmsk.StoreID = StoreId;

                                            if (!string.IsNullOrEmpty(elements[0]) && elements[0] != "")
                                            {
                                                string numericUpc = new string(elements[0].Where(char.IsDigit).ToArray());

                                                // string upc = Regex.Replace(elements[0].ToString(), @"[^0-9]+", "");
                                                if (!string.IsNullOrEmpty(numericUpc))
                                                {
                                                    pmsk.upc = "#" + elements[0].ToString().Replace("#", "");
                                                    fname.upc = "#" + elements[0].ToString().Replace("#", "");
                                                    pmsk.sku = "#" + elements[0].ToString().Replace("#", "");
                                                    fname.sku = "#" + elements[0].ToString().Replace("#", "");
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            //decimal qty = Convert.ToDecimal(elements[3]);
                                            //pmsk.Qty = Convert.ToInt32(qty);

                                            if (SQty.Contains(StoreId.ToString()))
                                            {
                                                pmsk.Qty = 999;
                                            }
                                            else
                                            {
                                                pmsk.Qty = Convert.ToInt32(elements[3]);
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

                                            Regex uomRegex = new Regex(@"\b(\d+(\.\d+)?)\s*(ml|ML|oz|OZ|L|l|M|m|PK|0Z)\b", RegexOptions.IgnoreCase);
                                            Match match = uomRegex.Match(elements[1]);

                                            if (match.Success)
                                            {
                                                pmsk.uom = match.Value.ToUpper();
                                                fname.uom = match.Value.ToUpper();
                                            }
                                            else
                                            {
                                                pmsk.uom = "";
                                                fname.uom = "";
                                            }




                                            string result = Regex.Replace(elements[2], "\\$", "");
                                            pmsk.Price = Convert.ToDecimal(result);
                                            fname.Price = Convert.ToDecimal(result);
                                            pmsk.sprice = System.Convert.ToDecimal(null);

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


                                            if (DiffQty.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0)
                                            {
                                                prodlist.Add(pmsk);
                                                full.Add(fname);
                                            }
                                            else if (!DiffQty.Contains(StoreId.ToString()) && pmsk.Price > 0)
                                            {
                                                prodlist.Add(pmsk);
                                                full.Add(fname);
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);

                                        }
                                    }

                                    Console.WriteLine("Generating RAPIDRMSMAPPINGTOOL " + StoreId + " Product CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    Console.WriteLine("Product File Generated For RAPIDRMS " + StoreId);
                                    Console.WriteLine();
                                    Console.WriteLine("Generating RAPIDRMSMAPPINGTOOL " + StoreId + " Fullname CSV Files.....");
                                    filename = GenerateCSV.GenerateCSVFile(full, "FullName", StoreId, BaseUrl);
                                    Console.WriteLine("Fullname File Generated For RAPIDRMSMAPPINGTOOL " + StoreId);

                                    string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreMapId + "/Raw/");

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
                                    return "Not generated file for RAPIDRMSMAPPINGTOOL " + StoreId;
                                }
                            }
                            else
                            {
                                return "Ínvalid FileName" + StoreId;

                            }
                        }
                        else
                        {
                            return "Invalid Directory" + StoreId;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);


            }
            return "Completed generating File For RAPIDRMSMAPPINGTOOL" + StoreId;
        }

        public class ProductsModelSRAPIDRMS
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public int Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string uom { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public decimal sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal Tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
            public double Deposit { get; set; }
        }

        public class FullnameModelFile
        {
            public string pname { get; set; }
            public string pdesc { get; set; }
            public string upc { get; set; }
            public string sku { get; set; }
            public decimal Price { get; set; }
            public string uom { get; set; }
            public int pack { get; set; }
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string country { get; set; }
            public string region { get; set; }
        }
    }
}
