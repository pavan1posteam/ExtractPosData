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
    class clsbevmaxspirits
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string StaticQty = ConfigurationManager.AppSettings["StaticQty"];
        string Quantity = ConfigurationManager.AppSettings["Quantity"];
        public readonly int discount_flag;
        public clsbevmaxspirits(int storeid, decimal tax, int discountable)
        {
            try
            {
                discount_flag = discountable;
                BevMaxSpiritsConvertRawFile(storeid, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            try
            {
                using (TextFieldParser parser = new TextFieldParser(FileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.Delimiters = new string[] { "," };
                    parser.HasFieldsEnclosedInQuotes = true;

                    string[] columns = parser.ReadFields();

                    for (int i = 0; i < columns.Length; i++)
                    {
                        dtResult.Columns.Add(columns[i], typeof(string));
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        DataRow newrow = dtResult.NewRow();
                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (dtResult.Columns.Count != fields.Length)
                            {
                                break;
                            }
                            newrow[i] = fields[i];
                        }
                        dtResult.Rows.Add(newrow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dtResult; 
        }

        public string BevMaxSpiritsConvertRawFile(int StoreId, decimal Tax)
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
                                List<ProductsModelBevMax> prodlist = new List<ProductsModelBevMax>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();
                                Console.WriteLine("StoreID: " + StoreId);
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModelBevMax pmsk = new ProductsModelBevMax();
                                    FullNameProductModel fname = new FullNameProductModel();
                                    Verify v = new Verify(StoreId, dr);

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(v.GetString("UPC")))
                                    {
                                        pmsk.upc = v.GetString("UPC");
                                        fname.upc = v.GetString("UPC");
                                        pmsk.sku = v.GetString("SKU");
                                        fname.sku = v.GetString("SKU");
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    pmsk.StoreProductName = v.GetString("STOREPRODUCTNAME");
                                    pmsk.StoreDescription = v.GetString("WEB_REVIEW");
                                    fname.pdesc = v.GetString("WEB_REVIEW");
                                    fname.pname = v.GetString("STOREPRODUCTNAME");

                                    pmsk.Price = Convert.ToDecimal(v.GetString("PRICE").Replace("$", ""));
                                    fname.Price = Convert.ToDecimal(v.GetString("PRICE").Replace("$", ""));

                                    pmsk.sprice = v.GetDecimal("SPRICE");
                                    pmsk.pack = v.GetInt("PACK");
                                    pmsk.Tax = Tax;
                                    if (StaticQty.Contains(StoreId.ToString()))
                                        pmsk.Qty = 999;
                                    else
                                        pmsk.Qty = Convert.ToInt32(Math.Floor(v.GetDecimal("QTY") / pmsk.pack));
                                    
                                    pmsk.Start = v.GetString("START");
                                    pmsk.End = v.GetString("END");
                                    pmsk.altupc1 = v.GetString("ALTUPC1");
                                    pmsk.altupc2 = v.GetString("ALTUPC2");
                                    pmsk.altupc3 = v.GetString("ALTUPC3");
                                    pmsk.altupc4 = v.GetString("ALTUPC4");
                                    pmsk.altupc5 = v.GetString("ALTUPC5");
                                    pmsk.Cost = v.GetDecimal("COST");
                                    pmsk.Deposit = Convert.ToDecimal(v.GetString("DEPOSIT").Replace("$", ""));
                                    fname.pcat = v.GetString("CATEGORY");
                                    fname.pcat1 = v.GetString("SUBCATEGORY");
                                    fname.pcat2 = "";
                                    fname.uom = v.GetString("UOM");
                                    pmsk.uom = v.GetString("UOM");
                                    fname.region = "";
                                    fname.country = "";
                                    pmsk.ClubPrice = v.GetDecimal("CLUBPRICE");
                                    pmsk.Vintage = v.GetString("VINTAGE");
                                    string discount = v.GetString("DISCOUNT");
                                    //if(discount == "A02") // Added on 04/07/2025 tkt: #42120
                                    //{
                                    //    pmsk.sprice = Math.Round(pmsk.Price - (pmsk.Price*10/100),2);
                                    //}
                                    if (discount_flag == 1)// Added on 05/08/2025
                                    {
                                        if (discount == "A05" || discount == "A02")
                                            pmsk.Discountable = 1;
                                        else
                                            pmsk.Discountable = 0;
                                    }
                                    else if(discount_flag == 2) 
                                    {
                                        if (discount == "A05" || discount == "A02")
                                            pmsk.Discountable = 1;
                                        else
                                            pmsk.Discountable = 0;
                                        if (pmsk.sprice == 0)  // Added on 16/07/2025 tkt: #42120
                                            pmsk.Discountable = 0;
                                    }
                                    else if (discount_flag == 3)// Case Discount Condition Added on 04/08/2025
                                    {
                                        string discount2 = v.GetString("ML");
                                        if (fname.pcat.ToUpper().Contains("LIQUOR") || fname.pcat.ToUpper().Contains("WINE"))
                                        {
                                            if (discount2.Contains("750") || discount2.Contains("1000") || pmsk.uom.Contains("1 L") || pmsk.uom.Contains("750ml"))
                                                pmsk.Discountable = 1;
                                        }
                                        else
                                            pmsk.Discountable = 0;
                                    }
                                    else if(discount_flag == 4)//OLD Condition before 04/08/2025
                                    {
                                        if (discount == "A05")
                                            pmsk.Discountable = 0;
                                        else
                                            pmsk.Discountable = 1;
                                        if (pmsk.sprice == 0)  // Added on 16/07/2025 tkt: #42120
                                            pmsk.Discountable = 0;
                                    }
                                    if (Quantity.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }
                                    else if (pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }
                                }
                                Console.WriteLine("Generating clsBevMaxSpirits " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For clsBevMaxSpirits " + StoreId);
                                Console.WriteLine("Generating clsBevMaxSpirits " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For clsBevMaxSpirits " + StoreId);
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
                                return "Not generated file for clsBevMaxSpirits " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid FileName or Raw Folder is Empty! " + StoreId);
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
    public class ProductsModelBevMax
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
        public decimal ClubPrice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal Tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
        public decimal Deposit { get; set; }
        public int Discountable { get; set; }
        public string Vintage { get; set; }
        public decimal Cost { get; set; }
    }
}
