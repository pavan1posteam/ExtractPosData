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
 
    public class clsLiquorLand
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string PfOnly = ConfigurationManager.AppSettings["ProductFileOnly"];
        string RawFiletax = ConfigurationManager.AppSettings["RawFiletax"];

        public clsLiquorLand(int storeId, decimal Tax)
         {
            try
            {
                LiquorLandConvertRawFile(storeId, Tax);
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
                        var rows = parser.ReadFields().ToList();
                        if (rows.Count() == 12) rows.RemoveAt(11);             
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
        public string LiquorLandConvertRawFile(int StoreId, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
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
                            
                            List<ProductMode> prodlist = new List<ProductMode>();
                            List<FullNameProductModel> fullnamelist = new List<FullNameProductModel>();
                            foreach (DataRow dr in dt.Rows)
                            {
                                try
                                {
                                    ProductMode pmsk = new ProductMode();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;

                                    if (!string.IsNullOrEmpty(dr["Upc"].ToString()))
                                    {
                                        pmsk.upc = dr["Upc"].ToString().Trim();
                                        full.upc = dr["Upc"].ToString().Trim();
                                        full.sku = dr["UPC"].ToString().Trim();
                                        pmsk.sku = dr["UPC"].ToString().Trim();
                                        pmsk.Qty = Convert.ToInt32(dr["Qty"]);
                                    }
                                    if (PfOnly.Contains(StoreId.ToString()))
                                    {
                                        pmsk.sku = dr["SKU"].ToString().Trim();
                                        full.sku = dr["SKU"].ToString().Trim();
                                    }
                                    pmsk.uom = dr["Size"].ToString();
                                    full.uom = dr["Size"].ToString();
                                    var size = dr["Size"].ToString();

                                    if (StoreId == 10409)
                                    {
                                        if (!string.IsNullOrEmpty(size))
                                        {
                                            if (size == "2 Pack")
                                            {
                                                pmsk.pack = 2;
                                            }
                                            else if (size == "4PK" || size == "4 PK" || size == "4 PK CAN")
                                            {
                                                pmsk.pack = 4;
                                            }
                                            else if (size == "6PK" || size == "6 PK")
                                            {
                                                pmsk.pack = 6;
                                            }
                                            else if (size.Contains("9PK"))
                                            {
                                                pmsk.pack = 9;
                                            }
                                            else if (size == "10 PACK")
                                            {
                                                pmsk.pack = 10;
                                            }
                                            else if (size.Contains("12 PK"))
                                            {
                                                pmsk.pack = 12;
                                            }
                                            else if (size == "15 PK")
                                            {
                                                pmsk.pack = 15;
                                            }
                                            else if (size == "18 PK")
                                            {
                                                pmsk.pack = 18;
                                            }
                                            else if (size == "24 PK")
                                            {
                                                pmsk.pack = 24;
                                            }
                                            else
                                            {
                                                pmsk.pack = 1;
                                            }
                                        }
                                        else
                                        {
                                            pmsk.pack = 1;
                                        }
                                    }

                                    else if (PfOnly.Contains(StoreId.ToString()))
                                    {
                                        pmsk.pack = Convert.ToInt32(dr["Pack Size"]);
                                        full.pack = Convert.ToInt32(dr["Pack Size"]);
                                    }

                                    else
                                    {
                                        pmsk.pack = Convert.ToInt32(dr["Pack"]);
                                        full.pack = Convert.ToInt32(dr["Pack"]);
                                    }
                                    pmsk.StoreProductName = dr["StoreProductName"].ToString().Trim();
                                    pmsk.StoreDescription = dr["StoreProductName"].ToString().Trim();
                                    full.pname = dr["StoreProductName"].ToString().Trim();
                                    full.pdesc = dr["StoreProductName"].ToString().Trim();

                                    if (StoreId == 11951)
                                    {
                                        pmsk.StoreProductName += " | " + pmsk.uom;
                                        full.pname = pmsk.StoreProductName;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Price".ToLower()] == DBNull.Value ? 0 : dr["Price".ToLower()]);
                                    full.Price = System.Convert.ToDecimal(dr["Price".ToLower()] == DBNull.Value ? 0 : dr["Price".ToLower()]);

                                    if (PfOnly.Contains(StoreId.ToString()))
                                    {
                                        pmsk.sprice = 0;
                                    }
                                    else
                                    {
                                        if (!String.IsNullOrEmpty(dr.Field<string>("sprice")))
                                        {
                                            pmsk.sprice = System.Convert.ToDecimal(dr["sprice"] == DBNull.Value ? 0 : dr["sprice"]);
                                        }

                                    }
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    if (RawFiletax.Contains(StoreId.ToString()))
                                    {
                                        decimal ta = Convert.ToDecimal(dr["Tax"]);
                                        if (ta > 0)
                                        {
                                            pmsk.tax = ta / 100;
                                        }

                                    }
                                    else
                                    {
                                        pmsk.tax = Tax;
                                    }
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";

                                    if (PfOnly.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0) // 11338 
                                    {
                                        prodlist.Add(pmsk);
                                    }
                                    else
                                    {
                                        if (pmsk.Qty > 0 && pmsk.Price > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            fullnamelist.Add(full);
                                        }
                                    }
                                }
                                catch (Exception E)
                                {
                                    Console.WriteLine(E.Message);
                                }
                            }
                            Console.WriteLine("Generating JMSC " + StoreId + " Product CSV Files.....");
                            Console.WriteLine("Generating JMSC " + StoreId + " Fullname CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            filename = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                            Console.WriteLine("Fullname File Generated For JMSC " + StoreId);
                            Console.WriteLine("Product File Generated For JMSC " + StoreId);

                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                            foreach (string filepath in filePaths)
                            {
                                string destpath = filepath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyymmddhhmmss"));
                                File.Move(filepath, destpath);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("" + e.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                            return "Not generated file for JMSC " + StoreId;
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
            else
            {
                return "Invalid Directory" + StoreId;
            }
            return "Completed generating File For JMSC" + StoreId;
        }
    }

    public class ProductMode
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string uom { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public decimal Price { get; set; }
        public decimal sprice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
    }
    public class FullnameMode
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
