using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;

namespace ExtractPosData
{
    class clsWorldBank
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string Irrespective_of_qty = ConfigurationManager.AppSettings.Get("Irrespective_of_qty");

        public clsWorldBank(int StoreId, decimal Tax)
        {
            try
            {
                clsWorldBankConvertRawFile(StoreId, Tax);
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
                            var roww = row.Replace(',', ' ').Trim();

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
        public string clsWorldBankConvertRawFile(int StoreId, decimal Tax)
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

                                    if (string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        continue;
                                    }
                                    pmsk.upc = "#" + dr["UPC"].ToString();
                                    full.upc = "#" + dr["UPC"].ToString();
                                    var qty = Convert.ToString(dr["On Hand"]);
                                    if (qty.Trim().ToUpper() == "NULL")
                                    {
                                        qty = "0";
                                    }
                                    if (string.IsNullOrEmpty(qty))
                                    {
                                        continue;
                                    }
                                    var qty1 = Convert.ToDecimal(qty);
                                    pmsk.Qty = Convert.ToInt64(qty1) > 0 ? Convert.ToInt64(qty1) : 0;
                                    pmsk.sku = pmsk.upc;
                                    full.sku = pmsk.upc;
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Item Name")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Item Name").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("Item Name").Trim();
                                        full.pname = dr.Field<string>("Item Name").Trim();
                                        full.pdesc = dr.Field<string>("Item Name").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    try
                                    {
                                        pmsk.Price = Convert.ToDecimal(dr["Retail Price"]);
                                        full.Price = Convert.ToDecimal(dr["Retail Price"]);
                                    }
                                    catch (Exception)
                                    {

                                        continue;
                                    }

                                    if (pmsk.Price <= 0 || full.Price <= 0)
                                    {
                                        continue;
                                    }
                                    full.pack = 1;
                                    full.pcat = dr["Category"].ToString();
                                    full.pcat1 = "";
                                    pmsk.uom = "";
                                    full.uom = "";
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";
                                    pmsk.sprice = 0;
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
                                    if (Irrespective_of_qty.Contains(StoreId.ToString()) && pmsk.Qty > 0)
                                    {
                                        fulllist.Add(full);
                                        prodlist.Add(pmsk);
                                    }
                                    else if(!Irrespective_of_qty.Contains(StoreId.ToString()))
                                    {
                                        fulllist.Add(full);
                                        prodlist.Add(pmsk);
                                    }
                                }
                                Console.WriteLine("Generating WorldBank " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating WorldBank " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For WorldBank" + StoreId);
                                Console.WriteLine("Fullname File Generated For WorldBank" + StoreId);

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
                                return "Not generated file for WorldBank " + StoreId;
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
            return "Completed generating File For PTECHMAPPING" + StoreId;
        }
    }
}
