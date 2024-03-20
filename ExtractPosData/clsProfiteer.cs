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
    public class clsProfiteer
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string Wholesale = ConfigurationManager.AppSettings["WholesalePrice"];
        string stocks = ConfigurationManager.AppSettings["profiteerstocks"];
        string Sprice = ConfigurationManager.AppSettings["NotSprice"];
        public clsProfiteer(int storeid, decimal tax, int StoreMapId)
        {
            try
            {
                ProfiteerConvertRawFile(storeid, tax, StoreMapId);
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
        public string ProfiteerConvertRawFile(int StoreId, decimal Tax, int StoreMapId)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
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
                                DataTable dt = ConvertTextToDataTable(Url);

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModelpft> prodlist = new List<ProductsModelpft>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModelpft pmsk = new ProductsModelpft();
                                    FullNameProductModel fname = new FullNameProductModel();

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["UPC"].ToString();
                                        fname.upc = "#" + dr["UPC"].ToString();
                                        pmsk.sku = "#" + dr["UPC"].ToString();
                                        fname.sku = "#" + dr["UPC"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    string qty = Regex.Replace(dr["QOH"].ToString(), @"[^0-9]+", "0");
                                    if (!string.IsNullOrEmpty(qty))
                                    {
                                        pmsk.Qty = System.Convert.ToDecimal(qty);
                                    }
                                    pmsk.StoreProductName = dr.Field<string>("Description");
                                    fname.pname = dr.Field<string>("Description");
                                    pmsk.StoreDescription = dr.Field<string>("Description").Trim(); 
                                    fname.pdesc = dr.Field<string>("Description");
                                    
                                    string prc = Regex.Replace(dr["CurrentPrice"].ToString(), @"[^0-9.]+", "0");
                                    if (!string.IsNullOrEmpty(prc))
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(prc);
                                        fname.Price = System.Convert.ToDecimal(prc);
                                        if (pmsk.Price <= 0 || fname.Price <= 0)
                                        {
                                            continue;
                                        }
                                    }
                                    if (Wholesale.Contains(StoreId.ToString()))
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(dr["WholesalePrice"].ToString());  // WholesalePrice as per ticket #13932
                                        fname.Price = System.Convert.ToDecimal(dr["WholesalePrice"].ToString());
                                        if (pmsk.Price <= 0 || fname.Price <= 0)
                                        {
                                            continue;
                                        }
                                    }
                                    if (!Sprice.Contains(StoreId.ToString()))
                                    {
                                        pmsk.sprice = System.Convert.ToDecimal(dr["Promo1Price"].ToString());
                                    }
                                    
                                    pmsk.pack = 1;
                                    fname.pack = 1;
                                    pmsk.Tax = Tax;
                                    
                                    if (pmsk.sprice > 0 && pmsk.StoreID != 11384 && pmsk.StoreID != 11633)
                                    {
                                        pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                        pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                    }
                                    else
                                    {
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                    }
                                    fname.pcat = dr.Field<string>("Dept");
                                    fname.pcat1 = dr.Field<string>("SubDept");
                                    fname.pcat2 = "";
                                    fname.uom = dr.Field<string>("SubDescription");
                                    fname.region = "";
                                    fname.country = "";
                                    if(stocks.Contains(StoreId.ToString()) && pmsk.Qty >= 1)
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }
                                    else if(!stocks.Contains(StoreId.ToString()))
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }
                                }
                                Console.WriteLine("Generating Profiteer " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Profiteer " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Profiteer " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For Profiteer " + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreMapId + "/Raw/");

                                if (StoreId !=10395 && StoreId != 11384 && StoreId != 11633)
                                {
                                    foreach (string filePath in filePaths)
                                    {
                                        string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                        File.Move(filePath, destpath);
                                    }
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
    public class ProductsModelpft
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
        public decimal Tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
        public decimal Deposit { get; set; }
    }
}
