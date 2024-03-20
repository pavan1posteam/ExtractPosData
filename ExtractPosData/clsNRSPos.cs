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
    class clsNRSPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        string SQty = ConfigurationManager.AppSettings["StaticQty"];
        string Qty = ConfigurationManager.AppSettings["Quantity"];
        string stocks = ConfigurationManager.AppSettings["stocks"];
        string Depst = ConfigurationManager.AppSettings["Deposit"];
        string differentQty = ConfigurationManager.AppSettings["differentQty"];
        public clsNRSPos(int StoreId, decimal tax, List<categories> cat, bool IsMarkUpPrice, int MarkUpValue)
        {
            try
            {
                NRSConvertRawFile(StoreId, tax, cat, IsMarkUpPrice, MarkUpValue);
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
        public string NRSConvertRawFile(int StoreId, decimal tax, List<categories> cat, bool IsMarkUpPrice, int MarkUpValue)
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
                                List<ProductsModelNRS> prodlist = new List<ProductsModelNRS>();
                                List<FullnameModel> full = new List<FullnameModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModelNRS pmsk = new ProductsModelNRS();
                                    FullnameModel fname = new FullnameModel();

                                    pmsk.StoreID = StoreId;

                                    string upc = Regex.Replace(dr["upc"].ToString(), @"[^0-9]+", "");
                                    if (!string.IsNullOrEmpty(upc) && upc != "")
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
                                    if (Qty.Contains(StoreId.ToString()))   //  Qunatity  column as qty 
                                    {
                                        string qty = Regex.Replace(dr["qty"].ToString(), "=", "");

                                        if (!string.IsNullOrEmpty(qty))
                                        {
                                            var qtyy = Convert.ToInt32(int.Parse(qty));
                                            pmsk.Qty = System.Convert.ToInt32(qtyy);
                                        }
                                    }

                                    if (SQty.Contains(StoreId.ToString()))  // StoreID:10947;11466;11473
                                    {
                                        pmsk.Qty = 999;  // As per ticket 13931 
                                    }
                                    else if (!stocks.Contains(StoreId.ToString()))
                                    {
                                        string qty = Regex.Replace(dr["Stock"].ToString(), "=", "");
                                        if (!string.IsNullOrEmpty(qty))
                                        {
                                            var qtyy = Convert.ToInt32(int.Parse(qty));
                                            pmsk.Qty = System.Convert.ToInt32(qtyy);
                                        }
                                    }
                                    pmsk.StoreProductName = dr.Field<string>("Name").Trim().Replace("=", "").Replace("DiseÃ±o", "").Replace("'", "");
                                    fname.pname = dr.Field<string>("Name").Trim().Replace("=", "").Replace("DiseÃ±o", "").Replace("'", "");
                                    pmsk.StoreDescription = dr.Field<string>("Name").Trim().Replace("=", "").Replace("DiseÃ±o", "").Replace("'", "");
                                    fname.pdesc = dr.Field<string>("Name").Trim().Replace("=", "").Replace("DiseÃ±o", "").Replace("'", "");
                                    var result = Regex.Split(dr.Field<string>("Name"), "\r\n");
                                    var result1 = "";
                                    for (int i = 0; i < result.Count(); i++)
                                    {
                                        result1 = result1 + result[i];
                                    }
                                    pmsk.StoreProductName = result1;
                                    fname.pname = result1;
                                    pmsk.StoreDescription = result1;
                                    fname.pdesc = result1;
                                    Decimal Price = System.Convert.ToDecimal(dr["cents"]);
                                    if (IsMarkUpPrice)
                                    {
                                        pmsk.Price = Price / 100;
                                        fname.Price = Price / 100;
                                        pmsk.Price = Math.Round(pmsk.Price * MarkUpValue / 100 + pmsk.Price, 2);
                                        fname.Price = Math.Round(fname.Price * MarkUpValue / 100 + fname.Price, 2);
                                    }
                                    else
                                    {
                                        pmsk.Price = Price / 100;
                                        fname.Price = Price / 100;
                                    }
                                    pmsk.sprice = System.Convert.ToDecimal(null);

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

                                    pmsk.pack = 1;
                                    fname.pack = 1;
                                    pmsk.Tax = tax;

                                    pmsk.uom = dr.Field<string>("size").Replace("=", "");
                                    fname.uom = dr.Field<string>("size").Replace("=", "");
                                    String Dept = dr.Field<string>("Department");
                                    if (StoreId == 10986)
                                    {
                                        Dept = Regex.Replace(Dept.ToString(), @"[^A-Z]+", "");
                                    }
                                    fname.pcat = Dept.ToString();
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.region = "";
                                    fname.country = "";
                                    if (differentQty.Contains(StoreId.ToString()))
                                    {
                                        string qty = Regex.Replace(dr["setstock"].ToString(), "=", "");
                                        if (fname.pcat.Contains("Drinks") || fname.pcat.Contains("JUICE") || fname.pcat.Contains("Mixes"))
                                        {
                                            pmsk.Qty = 999;
                                        }
                                        else if (!string.IsNullOrEmpty(qty))
                                        {
                                            var qtyy = Convert.ToInt32(int.Parse(qty));
                                            pmsk.Qty = System.Convert.ToInt32(qtyy);
                                        }
                                        else { continue; }

                                    }
                                    if (pmsk.Qty < 0)  //// If Any Qty < 0 we make those products as 0 
                                    {
                                        pmsk.Qty = 0;
                                    }
                                    if (Depst.Contains(StoreId.ToString()))
                                    {
                                        if (fname.pcat == "Soft Drinks - $0.05" || fname.pcat == "Beer $0.05 CRV")
                                        {
                                            pmsk.Deposit = 0.05;
                                        }
                                        else if (fname.pcat == "Soft Drinks - $0.10" || fname.pcat == "Beer $0.10 CRV")
                                        {
                                            pmsk.Deposit = 0.10;
                                        }
                                    }
                                    if (StoreId == 11263)  //|| StoreId==11291 older one Raw file 
                                    {
                                        if (pmsk.Price > 0 && pmsk.Qty > 0 && (fname.pcat.ToUpper() == "ALCOHOL" || fname.pcat.ToUpper() == "DRINKS"))
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                    else if (StoreId == 10947)
                                    {
                                        if (pmsk.Price > 0 && (fname.pcat.ToUpper() != "GENERIC CIGARETTES" && fname.pcat.ToUpper() != "HOUSEHOLD"
                                            && fname.pcat.ToUpper() != "PREMIUM CARTONS" && fname.pcat.ToUpper() != "PREMIUM CIGARETTES"
                                            && fname.pcat.ToUpper() != "SCRATCH OFFS" && fname.pcat.ToUpper() != "SMOKE"
                                            && fname.pcat.ToUpper() != "SUB-GENERIC CARTON" && fname.pcat.ToUpper() != "SUB-GENERIC CIGARETTES"
                                            && fname.pcat.ToUpper() != "TOBACCO" && fname.pcat.ToUpper() != "KRATOM"
                                            && fname.pcat.ToUpper() != "GENERIC CARTONS"))
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                    else
                                    {
                                        if (pmsk.Price > 0 && pmsk.Qty > 0 && (fname.pcat != "Cigarettes" && fname.pcat != "Cigars" && fname.pcat != "Vapes"))
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                }
                                Console.WriteLine("Generating NRSPos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For NRSPos " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating NRSPos " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For NRSPos " + StoreId);

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
                                return "Not generated file for NRSPos " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName" + StoreId;
                        }
                    }
                    else
                    {
                        return "Invalid Sub-Directory" + StoreId;
                    }
                }
            }
            else
            {
                return "Invalid Directory" + StoreId;
            }
            return "Completed generating File For NRSPos" + StoreId;
        }
    }
    public class ProductsModelNRS
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
}
