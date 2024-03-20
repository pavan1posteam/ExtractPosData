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
    public class clsAloha
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsAloha(int storeid, decimal tax)
        {
            try
            {
                AlohaConvertRawFile(storeid, tax);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }

        public static DataTable ConvertTextToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            using (TextFieldParser parser = new TextFieldParser(FileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters("\t");
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
                        string LineValue = parser.ReadLine();
                        if (LineValue.IndexOf("'\'") > 0)
                        {
                            LineValue = LineValue.Replace("'\'", "#! ");
                        }
                        string[] rows = LineValue.Split('\t');

                        dtResult.Rows.Add();
                        int c = 0;
                        foreach (string row in rows)
                        {
                            var roww = row.Replace("#!", "'\'");

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
        public string AlohaConvertRawFile(int StoreId, decimal Tax)
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
                                DataTable dt = ConvertTextToDataTable(Url);

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<alohaproductmodel> prodlist = new List<alohaproductmodel>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    alohaproductmodel pmsk = new alohaproductmodel();
                                    FullNameProductModel fname = new FullNameProductModel();

                                    pmsk.StoreID = StoreId;
                                    var aa = dr["SKU"];
                                    string sku = Regex.Replace(dr["SKU"].ToString(), @"[^0-9]+", "");
                                    if (!string.IsNullOrEmpty(sku))
                                    {
                                        pmsk.sku = "#" + dr["SKU"].ToString();
                                        fname.sku = "#" + dr["SKU"].ToString();
                                    }
                                    string upc = Regex.Replace(dr["SKU"].ToString(), @"[^0-9]+", "");
                                    if (!string.IsNullOrEmpty(upc))
                                    {
                                        pmsk.upc = "#" + dr["SKU"].ToString();
                                        fname.upc = "#" + dr["SKU"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Qty = (long)(System.Convert.ToDouble(dr["UnitsInCase"]) * System.Convert.ToDouble(dr["CaseCount"]));

                                    pmsk.Qty = pmsk.Qty > 0 ? pmsk.Qty : 0;
                                    pmsk.StoreProductName = dr.Field<string>("PackageDescription");
                                    fname.pname = dr.Field<string>("LONGNAME");
                                    pmsk.StoreDescription = dr.Field<string>("PackageDescription");
                                    fname.pdesc = dr.Field<string>("PackageDescription");

                                    string prc = dr["PRICE"].ToString();
                                    if (!string.IsNullOrEmpty(prc))
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(dr["PRICE"]);
                                        fname.Price = System.Convert.ToDecimal(dr["PRICE"]);
                                        if (pmsk.Price <= 0 || fname.Price <= 0)
                                        {
                                            continue;
                                        }
                                    }
                                    else { continue; }
                              
                                    string pack = dr["PackagingName"].ToString();

                                    if (pack == "4/6 pk" || pack == "6/4 pk" || pack == "2/12 pk" || pack == "24 pk loose")
                                    {
                                        pmsk.pack = "24";
                                    }
                                    else if (pack == "Single (1/12)" || pack == "Single (1/15)" || pack == "Single (1/6)" || pack == "Single (1/24)" || pack == "NA" || pack == "Single (1/4)" || pack == "Keg" || pack == "Single (1/80)" || pack == "Single (1/16)" || pack == "Single (1/8)" || pack == "Slushie 20oz" || pack == "Slushie 32oz" || pack == "One (1/1)" || pack == "Single (1/72)")
                                    {
                                        pmsk.pack = "1";
                                    }
                                    else if (pack == "4/15 pk" || pack == "15 pk(1/1)" || pack == "15 pk(1/2)")
                                    {
                                        pmsk.pack = "15";
                                    }
                                    else if (pack == "2/8 pk" || pack == "16 pk")
                                    {
                                        pmsk.pack = "16";
                                    }
                                    else if (pack == "1/6 pk" || pack == "6 pk" || pack == "6 pack (1/3)")
                                    {
                                        pmsk.pack = "6";
                                    }
                                    else if (pack == "1/3 Pk")
                                    {
                                        pmsk.pack = "3";
                                    }
                                    else if (pack == "1/28 pk")
                                    {
                                        pmsk.pack = "28";
                                    }
                                    else if (pack == "1/2 pk")
                                    {
                                        pmsk.pack = "2";
                                    }
                                    else if (pack == "1/120")
                                    {
                                        pmsk.pack = "120";
                                    }
                                    else if (pack == "1/12 pk" || pack == "12 pk (1/1)" || pack == "12 pk")
                                    {
                                        pmsk.pack = "12";
                                    }
                                    else if (pack == "4 pk" || pack == "4 pk (4/16)" || pack == "4 pack (1/3)")
                                    {
                                        pmsk.pack = "4";
                                    }
                                    else if (pack == "8 pk (1/3)" || pack == "8 pk (1/2)" || pack == "8 Pack")
                                    {
                                        pmsk.pack = "8";
                                    }
                                    else if (pack == "9 pk (1/2)")
                                    {
                                        pmsk.pack = "9";
                                    }
                                    else if (pack == "18 pk" || pack == "1/18 pk")
                                    {
                                        pmsk.pack = "18";
                                    }
                                    else if (pack == "30 pk")
                                    {
                                        pmsk.pack = "30";
                                    }
                                    else if (pack == "36 pk")
                                    {
                                        pmsk.pack = "36";
                                    }
                                    else
                                    {
                                        pmsk.pack = "1";
                                    }
                                    
                                    var Taxable = dr.Field<string>("Taxable");

                                    if(StoreId == 11297 && Taxable == "Y") // If Taxable == Y the tax 0.06 as per ticket #14518
                                    {
                                        pmsk.Tax = Tax;
                                    }
                                    else
                                    {
                                        pmsk.Tax = System.Convert.ToDecimal("0.00");  // As per ticket #14518
                                    }
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
                                    var alloc = dr.Field<string>("Allocated");
                                    fname.pcat = "Beer";
                                    fname.pcat1 = dr.Field<string>("Classification");
                                    fname.pcat2 = "BeerTypeName";
                                    pmsk.uom = dr.Field<string>("SizeName");
                                    fname.uom = dr.Field<string>("SizeName");
                                    fname.region = "";
                                    fname.country = "";
                                    if ( alloc == "0")
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }
                                }
                                Console.WriteLine("Generating Aloha " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Aloha " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Aloha " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For Aloha " + StoreId);

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
                                return "Not generated file for Profiteer " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName or Raw Folder is Empty! " + StoreId);
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
    public class alohaproductmodel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
        public string sku { get; set; }
        public string pack { get; set; }
        public string uom { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public decimal Price { get; set; }
        public decimal sprice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal  Tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
        public decimal deposit { get; set; }

    }
}