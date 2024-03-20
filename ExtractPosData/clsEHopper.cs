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
    public class clsEHopper
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsEHopper(int StoreId, decimal Tax)
        {
            try
            {
                EHopperConvertRawFile(StoreId, Tax);
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
                            ////if (rows.Count() > 26)
                            ////{ break; }
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
            }
            catch (Exception)
            {
            }

            return dtResult; //Returning Dattable  
        }
        public string EHopperConvertRawFile(int StoreId, decimal Tax)
        {
            DataTable dt = new DataTable();
            DataTable finaldt = new DataTable();
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    string[] filePathss = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");
                    if (filePathss != null)
                    {
                        foreach (var itm in filePathss)
                        {
                            string Url = itm;
                            dt = ConvertCsvToDataTable(Url);
                            finaldt.Merge(dt);
                        }
                        try
                        {
                            List<ProductsModel> prodlist = new List<ProductsModel>();
                            List<FullNameProductModel> full = new List<FullNameProductModel>();

                            foreach (DataRow dr in finaldt.Rows)
                            {
                                ProductsModel pmsk = new ProductsModel();
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
                                decimal qty = Convert.ToDecimal(dr["Quantity on Hand"]);
                                pmsk.Qty = System.Convert.ToInt32(qty);
                                pmsk.StoreProductName = dr.Field<string>("Product Name (EN)");
                                fname.pname = dr.Field<string>("Product Name (EN)");
                                pmsk.StoreDescription = dr.Field<string>("Product Name (EN)").Trim();
                                fname.pdesc = dr.Field<string>("Product Name (EN)");
                                pmsk.Price = System.Convert.ToDecimal(dr["Sales Price"]);
                                fname.Price = System.Convert.ToDecimal(dr["Sales Price"]);
                                if (pmsk.Price <= 0 || fname.Price <= 0)
                                {
                                    continue;
                                }

                                pmsk.sprice = 0;
                                pmsk.pack = 1;
                                fname.pack = 1;
                                pmsk.Tax = Tax;
                                pmsk.Start = "";
                                pmsk.End = "";
                                pmsk.altupc1 = "";
                                pmsk.altupc2 = "";
                                pmsk.altupc3 = "";
                                pmsk.altupc4 = "";
                                pmsk.altupc5 = "";
                                //fname.pcat = dr.Field<string>("Category");
                                fname.pcat = dr.Field<string>("Department");   // tckt8592
                                fname.pcat1 = "";
                                fname.pcat2 = "";
                                fname.uom = "";
                                fname.region = "";
                                fname.country = "";
                                if (pmsk.Qty > 0 && (fname.pcat=="WINE" || fname.pcat=="BEER" || fname.pcat=="LIQUOR" || fname.pcat=="MIXERS")) //tckt8592
                                {
                                    prodlist.Add(pmsk);
                                    full.Add(fname);
                                }

                            }
                            Console.WriteLine("Generating EHopper " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For EHopper " + StoreId);
                            Console.WriteLine();
                            Console.WriteLine("Generating EHopper " + StoreId + " Fullname CSV Files.....");
                            filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                            Console.WriteLine("Fullname File Generated For EHopper " + StoreId);

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
                            return "Not generated file for EHopper " + StoreId;
                        }
                        //}
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
    }
}
