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
    class clsCetech
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsCetech(int StoreId, decimal Tax)
        {
            try
            {
                clsCetechConvertRawFile(StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertCsvToDataTable(string FileName , int StoreId)
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
                            if(StoreId == 11376 && c > 14)
                            { break; }
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
        public string clsCetechConvertRawFile(int StoreId, decimal Tax)
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
                                DataTable dt = ConvertCsvToDataTable(Url, StoreId);
                                List<CetechModel> prodlist = new List<CetechModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    CetechModel pmsk = new CetechModel();
                                    pmsk.StoreID = StoreId;
                                    pmsk.upc = "#" + dr["UPC"].ToString();
                                    pmsk.Qty = (dr["Qty"]== DBNull.Value ? 0 : dr["Qty"]).ToString();
                                    pmsk.sku = dr.Field<string>("Sku");                                    
                                    pmsk.StoreProductName = dr.Field<string>("storeproductname").Trim();
                                    pmsk.StoreDescription = dr.Field<string>("storedescription").Trim();
                                    pmsk.Price = (dr["Price"].ToString().StartsWith(".") ? "0" + dr["Price"].ToString() : dr["Price"].ToString());

                                    pmsk.sprice = dr["SPrice"].ToString();
                                    pmsk.pack = 1;
                                    pmsk.Tax = Tax;
                                    if (pmsk.sprice == "0")
                                    {
                                        pmsk.Start = "";
                                        pmsk.End = "";                                       
                                    }
                                    else
                                    {
                                        pmsk.Start = dr["Start"].ToString();
                                        pmsk.End = dr["End"].ToString();
                                    }
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    pmsk.Discountable = dr["Discountable"].ToString().StartsWith("Y") ? "1" : "0".Replace('#', '0');
                                    if (pmsk.sku != "#004920" && pmsk.sku != "#004562")
                                    {
                                        prodlist.Add(pmsk);
                                    }
                                }
                                Console.WriteLine("Generating Cetech " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Cetech" + StoreId);
                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");
                                foreach (string filePath in filePaths)
                                {
                                    GC.Collect();
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for Cetech " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName" + StoreId;
                        }
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
            return "Completed generating File For Cetech" + StoreId;
        }

        public class CetechModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public string Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string uom { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public string Price { get; set; }
            public string sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal Tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
            public decimal Deposit { get; set; }
            public string Discountable { get; set; }
        }
    }
}
