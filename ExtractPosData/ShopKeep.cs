using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Microsoft.VisualBasic.FileIO;

namespace ExtractPosData
{
    class ShopKeep
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public ShopKeep(string FileName, int storeId, decimal Tax)
        {
            try
            {
                ShopKeepConvertRawFile(FileName, storeId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
               ///// Console.Read();
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
        public string ShopKeepConvertRawFile(string PosFileName, int StoreId, decimal Tax)
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
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["UPC"].ToString();
                                        full.upc = "#" + dr["UPC"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    decimal qty = Convert.ToDecimal(dr["Quantity"]);
                                    pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                    if (StoreId == 10815)
                                    {
                                        pmsk.sku = "#" + dr.Field<string>("SKU (Do Not Edit)");
                                        full.sku = "#" + dr.Field<string>("SKU (Do Not Edit)");
                                    }
                                    else
                                    {
                                        pmsk.sku = "#" + dr.Field<string>("Store Code (SKU)");
                                        full.sku = "#" + dr.Field<string>("Store Code (SKU)");
                                    }
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Name")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Name").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("Name").Trim();
                                        full.pname = dr.Field<string>("Name").Trim();
                                        full.pdesc = dr.Field<string>("Name").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    full.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    if (pmsk.Price <= 0 || full.Price <= 0)
                                    {
                                        continue;
                                    }
                                    full.pcat = dr["Department"].ToString();
                                    full.pcat1 = dr["Category"].ToString();
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";
                                    pmsk.sprice = 0;
                                    pmsk.pack = 1;
                                    full.pack = 1;
                                    if (StoreId == 10869)
                                    {
                                        if (full.pcat == "BEER" || full.pcat == "LIQUOR" || full.pcat == "WINE")
                                        {
                                            pmsk.Tax = Tax;

                                        }
                                        else
                                        {
                                            pmsk.Tax = Convert.ToDecimal(0.04);
                                        }
                                    }
                                    else
                                    {
                                        pmsk.Tax = Tax;
                                    }
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if ( full.pcat != "Tobacco")
                                    {
                                        fulllist.Add(full);
                                        prodlist.Add(pmsk);
                                    }
                                }
                                Console.WriteLine("Generating ShopKeep " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating ShopKeep " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For ShopKeep" + StoreId);
                                Console.WriteLine("Fullname File Generated For ShopKeep" + StoreId);

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
                                return "Not generated file for ShopKeep " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName";
                        }
                    }
                }
                else
                {
                    return "Invalid Sub-Directory";
                }
            }
            else
            {
                return "Invalid Directory";
            }
            return "Completed generating File For ShopKeep" + StoreId;
        }
    }
    public class ProductsModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal  Qty { get; set; }
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
        public decimal Deposit { get; set; }
        public decimal Cost { get; set; }
    }
}
