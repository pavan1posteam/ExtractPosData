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
    class clsEposNow_FlatFile
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        public clsEposNow_FlatFile(int StoreId, decimal tax, bool IsMarkUpPrice, int MarkUpValue)
        {
            try
            {
                Console.WriteLine("Generating EPOSNOW " + StoreId + " Product CSV Files.....");
                Console.WriteLine("Generating EPOSNOW " + StoreId + " Fullname CSV Files.....");
                EposConvertRawFile(StoreId, tax, IsMarkUpPrice, MarkUpValue);
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
        public string EposConvertRawFile(int StoreId, decimal Tax, bool IsMarkUpPrice, int MarkUpValue)
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

                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["Barcode"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["Barcode"].ToString();
                                        full.upc = "#" + dr["Barcode"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    decimal qty = Convert.ToDecimal(dr["TotalStock"]);
                                    //  decimal.qty = Convert.ToDecimal(dr["CurrentStock"]);
                                    pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                    pmsk.sku = "#" + dr.Field<string>("Barcode");
                                    full.sku = "#" + dr.Field<string>("Barcode");
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
                                    decimal price;

                                    if (IsMarkUpPrice)
                                    {
                                        price = Convert.ToDecimal(dr["SalePrice"]);
                                        decimal markup = price * MarkUpValue / 100 + price;
                                        pmsk.Price = (markup);
                                        full.Price = (markup);

                                        pmsk.Price = Decimal.Round(pmsk.Price, 2);
                                        full.Price = Decimal.Round(full.Price, 2);
                                    }
                                    else
                                    {
                                        pmsk.Price = Convert.ToDecimal(dr["SalePrice"]);
                                        full.Price = Convert.ToDecimal(dr["SalePrice"]);
                                    }
                                    full.pcat = dr["CategoryName"].ToString();
                                    full.pcat1 = "";
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";
                                    pmsk.sprice = 0;
                                    full.pack = 1;
                                    pmsk.pack = 1;
                                    pmsk.Tax = Tax;
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
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if (full.pcat.ToUpper() != "TOBACCO" && full.pcat.ToUpper() != "CIGAR" && full.pcat.ToUpper() != "CIGARETTE" && pmsk.Price > 0)
                                    {
                                        fulllist.Add(full);
                                        prodlist.Add(pmsk);
                                    }
                                }
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For EPOSNOW" + StoreId);
                                Console.WriteLine("Fullname File Generated For EPOSNOW" + StoreId);

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
                                return "Not generated file for EPOSNOW " + StoreId;
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
            return "Completed generating File For EPOSNOW" + StoreId;
        }
    }
}
