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
    class clsPetrasoftSmart
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsPetrasoftSmart(int StoreId, decimal Tax)
        {
            try
            {
                DiscountConvertRawFile(StoreId, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                        if (columns.Contains("Station"))
                        {
                            foreach (string col in columns)
                            {
                                dtResult.Columns.Add(col);
                            }
                        }
                        else { continue; }
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
        public string DiscountConvertRawFile(int StoreId, decimal Tax)
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

                                List<ProductMod> prodlist = new List<ProductMod>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductMod pmsk = new ProductMod();
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
                                    var qty = dr.Field<string>("On Hand").Replace("(", "").Replace(")", "");
                                    pmsk.Qty = Convert.ToDecimal(qty) > 0 ? Convert.ToDecimal(qty) : 0;
                                    pmsk.StoreProductName = dr.Field<string>("Item Name");
                                    fname.pname = dr.Field<string>("Item Name");
                                    pmsk.StoreDescription = dr.Field<string>("Item Name").Trim();
                                    fname.pdesc = dr.Field<string>("Item Name");
                                    pmsk.Price = System.Convert.ToDecimal(dr["Retail,$"].ToString().Replace("$", string.Empty));
                                    fname.Price = System.Convert.ToDecimal(dr["Retail,$"].ToString().Replace("$", string.Empty));
                                    if (pmsk.Price <= 0 || fname.Price <= 0)
                                    {
                                        continue;
                                    }
                                    pmsk.sprice = 0;
                                    pmsk.pack = 1;
                                    pmsk.tax = Tax;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    fname.pcat = dr.Field<string>("Dep");
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.uom = dr.Field<string>("Size");
                                    fname.region = "";
                                    fname.country = "";
                                    if ( fname.pcat != "Other Tobacco Produc" && fname.pcat != "E-Cigs" && fname.pcat != "Cigarettes")
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }
                                }
                                Console.WriteLine("Generating Petrasoft Smart " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Petrasoft Smart " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Petrasoft Smart " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For Petrasoft Smart " + StoreId);

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
                                return "Not generated file for Petrasoft Smart " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName or RAW folder is empty!" + StoreId);
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
        public class ProductMod
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public decimal Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
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
    }
}
