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
    class clsPOSNation
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsPOSNation(int storeId, decimal Tax)
        {
            try
            {
                POSnationConvertRawFile(storeId, Tax);
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
                        try
                        {
                            var rows = parser.ReadFields().ToList();
                            if (rows.Count == 7)
                            {
                                string tempPrice = rows[3];
                                rows[3] = rows[4];
                                rows[4] = rows[5];
                                rows[5] = rows[6];
                                //rows[2] = tempPrice;
                                //rows.Insert(3, tempPrice);
                                rows.RemoveRange(6, rows.Count - 6);
                            }
                            if (rows.Count > 7)
                            {
                                string tempPrice = rows[3];
                                rows[3] = rows[5];
                                rows[4] = rows[6];
                                rows[5] = rows[7];
                                //  rows[2] = tempPrice;
                                //rows.Insert(3, tempPrice);
                                rows.RemoveRange(6, rows.Count - 6);
                            }
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
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    i++;
                }
            }
            return dtResult; //Returning Dattable  
        }
        public string POSnationConvertRawFile(int StoreId, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
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
                            List<ProductMode> prodlist = new List<ProductMode>();
                            foreach (DataRow dr in dt.Rows)
                            {
                                ProductMode pdf = new ProductMode();
                                try
                                {
                                    pdf.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        if (Regex.IsMatch(dr["UPC"].ToString(),@"\d[0-9]+"))
                                        {
                                            pdf.upc = "#" + dr["UPC"].ToString().Trim();
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
                                    if (!string.IsNullOrEmpty(dr["SKU"].ToString()))
                                    {
                                        pdf.sku = "#" + dr["SKU"].ToString().Trim();
                                    }
                                    decimal qty = Convert.ToDecimal(dr["QTY"]);
                                    int qtyInt = (int)qty;
                                    pdf.Qty = qtyInt > 0 ? qtyInt : 0;
                                    pdf.StoreProductName = dr["NAME"].ToString().Trim();
                                    pdf.StoreDescription = dr["NAME"].ToString().Trim();
                                    pdf.pack = 1;
                                    pdf.tax = Tax;
                                    var p = (dr["PRICE"]).ToString();
                                    pdf.Price = Convert.ToDecimal(dr["PRICE"]);
                                    pdf.sprice = 0;
                                    pdf.Start = "";
                                    pdf.End = "";
                                    pdf.altupc1 = "";
                                    pdf.altupc2 = "";
                                    pdf.altupc3 = "";
                                    pdf.altupc4 = "";
                                    pdf.altupc5 = "";
                                    pdf.uom = dr["SIZE"].ToString();
                                    if (pdf.uom == "")
                                    {
                                        string ccProdname = dr["NAME"].ToString().ToUpper();
                                        Regex filter = new Regex(@"(\d+(?:\.\d+)?\s*(?:LB|ML|L|OZ|sOZ|oz+))");
                                        var match = filter.Match(ccProdname);
                                        if (match.Success)
                                        {
                                            pdf.uom = match.Value;
                                        }
                                        else
                                        {
                                            pdf.uom = pdf.uom;
                                        }
                                    }
                                    if (pdf.Price > 0)
                                    {
                                        prodlist.Add(pdf);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            Console.WriteLine("Generating POSnation " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For POSnation " + StoreId);

                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                            foreach (string filepath in filePaths)
                            {
                                string destpath = filepath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyymmddhhmmss"));
                                File.Move(filepath, destpath);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("" + e.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                            return "Not generated file for POSnation " + StoreId;
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
            else
            {
                return "Invalid Directory" + StoreId;
            }
            return "Completed generating File For POSnation" + StoreId;
        }
    }
}
