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
    class clsNeimannFoodsPOS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        public clsNeimannFoodsPOS(int storeid, decimal tax)
        {
            try
            {
                NRSConvertRawFile(storeid, tax);
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
        public string NRSConvertRawFile(int StoreId, decimal tax)
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
                                dt.Rows.RemoveAt(0);
                                List<Productfile> prodlist = new List<Productfile>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    Productfile prod = new Productfile();

                                    prod.StoreID = StoreId;
                                    if (dr["UPC"] != DBNull.Value)
                                    {
                                        string upc = Regex.Replace(dr["UPC"].ToString(), @"[^0-9]+", "");
                                        if (!string.IsNullOrEmpty(upc))
                                        {
                                            prod.upc = "#" + upc;
                                            prod.sku = "#" + upc;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    prod.uom = dr["Size"].ToString();

                                    if (dr["TaxRate"] != DBNull.Value && decimal.TryParse(dr["TaxRate"].ToString(), out decimal taxRate))
                                    {
                                        prod.Tax = taxRate/100;
                                    }
                                    prod.StoreProductName = dr["Description"].ToString();
                                    prod.StoreDescription = dr["Description"].ToString();

                                    if (dr["DealRetail"] != DBNull.Value && decimal.TryParse(dr["DealRetail"].ToString(), out decimal dealRetail))
                                    {
                                        prod.sprice = dealRetail;

                                        if (prod.sprice > 0)
                                        {
                                            prod.Start = dr["SaleStart"].ToString();
                                            prod.End = dr["SaleEnd"].ToString();
                                        }
                                        else
                                        {
                                            prod.Start = "";
                                            prod.End = "";
                                        
                                    }
                                }

                                    prod.Qty = 999;

                                    if (dr["RegularRetail"] != DBNull.Value && decimal.TryParse(dr["RegularRetail"].ToString(), out decimal priceValue))
                                    {
                                        prod.Price = priceValue;

                                    }
                                    else
                                    {

                                    }
                                    prod.pack = 1;
                                    prod.Deposit = 0;
                                    prod.altupc5 = "";
                                    prod.altupc4 = "";
                                    prod.altupc3 = "";
                                    prod.altupc2 = "";
                                    prod.altupc1 = "";

                                    if (prod.Price > 0)
                                    {
                                        prodlist.Add(prod);
                                    }
                                }
                                Console.WriteLine("Generating Neimann Foods POS " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Neimann Foods POS " + StoreId);
                                Console.WriteLine();

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine("" + ex.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                                return "Not generated file for Neimann Foods POS " + StoreId;
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
            return "Completed generating File For Neimann Foods POS" + StoreId;
        }
    }
    public class Productfile
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
