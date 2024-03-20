using ExtractPosData.Model;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData
{
    class clsRandalls
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsRandalls(int StoreId, decimal Tax)
        {
            try
            {
                RandallsConvertRawFile(StoreId, Tax);
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
        public string RandallsConvertRawFile(int StoreId, decimal Tax)
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


                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductFile> prodlist = new List<ProductFile>();
                                List<clsFullnameModel> fullnamelist = new List<clsFullnameModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductFile pmsk = new ProductFile();
                                    clsFullnameModel full = new clsFullnameModel();
                                    pmsk.StoreID = StoreId;
                                    full.pname = dr.Field<string>("STOREPRODUCTNAME");
                                    full.pdesc = dr.Field<string>("STOREPRODUCTNAME");
                                    full.pcat = dr.Field<string>("DEPT");
                                    full.pcat1 = "";
                                    full.pcat2 = "";
                                    full.uom = dr.Field<string>("SIZE");
                                    full.country = "";
                                    full.region = "";
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr.Field<string>("UPC").ToString();
                                        full.upc = "#" + dr.Field<string>("UPC").ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Qty = System.Convert.ToDecimal(dr["QTY"] == DBNull.Value ? 0 : dr["QTY"]);
                                    pmsk.sku = "#" + dr.Field<string>("SKU");
                                    full.sku = "#" + dr.Field<string>("SKU");
                                    if (!string.IsNullOrEmpty(dr.Field<string>("STOREPRODUCTNAME")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("STOREPRODUCTNAME");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.StoreDescription = dr.Field<string>("STOREPRODUCTNAME");
                                    decimal price = System.Convert.ToDecimal(dr["PRICE"] == DBNull.Value ? 0 : dr["PRICE"]);
                                    if (price > 0)
                                    {
                                        pmsk.Price = price;
                                        full.Price = price;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.sprice = dr.Field<string>("SPRICE");
                                    pmsk.pack = dr.Field<string>("PACK");
                                    pmsk.tax = Tax;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if (pmsk.Qty > 0)
                                    {
                                        prodlist.Add(pmsk);
                                    }
                                    fullnamelist.Add(full);
                                }
                                Console.WriteLine("Generating Randalls Pos " + StoreId + " Product CSV Files.....");
                                string ProductFilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Randalls Pos " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Randalls Pos " + StoreId + " Full Name CSV Files.....");
                                string FullFileName = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Full Name File Generated For Randalls Pos " + StoreId);

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
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for" + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName Or Raw Folder is Empty! " + StoreId);
                        return "";
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
            return "Completed generating File";
        }
        public class ProductFile
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public decimal Qty { get; set; }
            public string sku { get; set; }
            public string pack { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public string sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
        }
    }

}
