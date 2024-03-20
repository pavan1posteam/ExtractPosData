using ExtractPosData.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtractPosData
{
    class clsSTCRPOS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsSTCRPOS(int StoreID, decimal tax)
        {

            try
            {
                STCRConvertRawFile(StoreID, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static List<string> ConvertCsvToList(string filename)
        {
            var dataLines = File.ReadAllText(filename).Split('\n').ToList();
            dataLines.RemoveAll(a => a.Contains("UPC,ALT,QTY,UOM,POS_DESCRIPTION"));


            for (int k = 0; k < dataLines.Count; k++)
            {
                ProductModel pmodel = new ProductModel();
                var xdata = dataLines[k];
                var data = dataLines[k].Split(',').ToList();
                int idx = data.FindIndex(a => Regex.IsMatch(a, @"^\d+$"));
                if (idx > 6)
                {
                    idx = idx - 2;
                    int idx2 = data.FindIndex(a => Regex.IsMatch(a.Replace("-", ""), @"^\d+[.]\d+$")) + 1;
                    for (int i = idx2; i < idx; i++)
                    {
                        data[idx2] = data[idx2] + data[idx2 + 1];
                        data.RemoveAt(idx2 + 1);
                    }
                }
                var hh = string.Join(",", data);
                dataLines[k] = hh;

            }
            return dataLines;
        }
        public string STCRConvertRawFile(int StoreId, decimal tax)
        {
            try
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

                                    List<string> dt = ConvertCsvToList(Url);
                                    dt.RemoveAll(a => a.Length <= 0);
                                    List<ProductsModelSTCR> prodlist = new List<ProductsModelSTCR>();
                                    List<FullnameModelFile> full = new List<FullnameModelFile>();

                                    foreach (string products in dt)
                                    {
                                        try
                                        {
                                            ProductsModelSTCR pmsk = new ProductsModelSTCR();
                                            FullnameModelFile fname = new FullnameModelFile();
                                            string[] elements = products.Split(',');

                                            pmsk.StoreID = StoreId;

                                            

                                            if (!string.IsNullOrEmpty(elements[1]) && elements[1] != "")
                                            {
                                                string upc = Regex.Replace(elements[1].ToString(), @"[^0-9]+", "");
                                                pmsk.upc = "#" + upc.ToString().Replace("#", "");
                                                fname.upc = "#" + upc.ToString().Replace("#", "");
                                                pmsk.sku = "#" + upc.ToString().Replace("#", "");
                                                fname.sku = "#" + upc.ToString().Replace("#", "");
                                            }
                                            else
                                            {
                                                continue;
                                            }

                                            decimal qty = Convert.ToDecimal(elements[3]);
                                            pmsk.Qty = Convert.ToInt32(qty);

                                            pmsk.pack = 1;
                                            fname.pack = 1;
                                            pmsk.uom = "";

                                            if (!string.IsNullOrEmpty(elements[4]) && elements[4] != "")
                                            {
                                                pmsk.StoreProductName = elements[4];
                                                fname.pname = elements[4].Trim();
                                                pmsk.StoreDescription = elements[4];
                                                fname.pdesc = elements[4].Trim();
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            string result = Regex.Replace(elements[12], "\\$", "");
                                            pmsk.Price = Convert.ToDecimal(result);
                                            fname.Price = Convert.ToDecimal(result);
                                            pmsk.sprice = System.Convert.ToDecimal(null);

                                            fname.pcat = elements[5].ToString();
                                            fname.pcat1 = "";
                                            fname.pcat2 = "";
                                            fname.region = "";
                                            fname.country = "";

                                            pmsk.Start = "";
                                            pmsk.End = "";
                                            pmsk.altupc1 = elements[2].Replace("#", "") == "" ? "" : elements[2];
                                            pmsk.altupc2 = "";
                                            pmsk.altupc3 = "";
                                            pmsk.altupc4 = "";
                                            pmsk.altupc5 = "";

                                            if (pmsk.Qty > 0 && pmsk.Price > 0)
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

                                    Console.WriteLine("Generating STCRPOS " + StoreId + " Product CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    Console.WriteLine("Product File Generated For STCRPOS " + StoreId);
                                    Console.WriteLine();
                                    Console.WriteLine("Generating STCRPOS " + StoreId + " Fullname CSV Files.....");
                                    filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                    Console.WriteLine("Fullname File Generated For STCRPOS " + StoreId);

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
                                    return "Not generated file for STCRPOS " + StoreId;
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
            return "Completed generating File For STCRPOS" + StoreId;
        }

    }
    public class ProductsModelSTCR
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
