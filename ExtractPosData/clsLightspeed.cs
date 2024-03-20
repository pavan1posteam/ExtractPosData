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
    public class clsLightspeed
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsLightspeed(int storeid,decimal tax)
        {
            try
            {
                LightspeedConvertRawFile(storeid, tax);
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
        public string LightspeedConvertRawFile(int StoreId, decimal Tax)
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
                                DataTable dt = ConvertTextToDataTable(Url);

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel fname = new FullNameProductModel();

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        var upc = "#" + dr["UPC"].ToString().ToLower();
                                        string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                        if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                        {
                                            if (!string.IsNullOrEmpty(numberUpc))
                                            {
                                                pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                fname.upc = "#" + dr["UPC"].ToString();
                                                pmsk.sku = "#" + dr["UPC"].ToString();
                                                fname.sku = "#" + dr["UPC"].ToString();
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
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    double qty = System.Convert.ToDouble(dr["Inventory"]);
                                    if (qty > 0)
                                    {
                                        pmsk.Qty = System.Convert.ToInt32(dr["Inventory"] == DBNull.Value ? 0 : System.Convert.ToDecimal(dr["Inventory"]));
                                    }
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Code").Trim()) && !dr.Field<string>("Code").Contains("DQ") && !dr.Field<string>("Code").Contains("(DQ)"))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Code").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("Code").Trim();
                                        fname.pdesc = dr.Field<string>("Code").Trim();
                                        fname.pname = dr.Field<string>("Code").Trim();
                                        pmsk.StoreDescription = Regex.Replace(pmsk.StoreDescription, @"'[^']+'(?=!\w+)", "");
                                        fname.pdesc = Regex.Replace(fname.pdesc, @"'[^']+'(?=!\w+)", "");
                                    }
                                    else
                                    { continue; }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Sell"].ToString());
                                    fname.Price = System.Convert.ToDecimal(dr["Sell"].ToString());
                                    if (pmsk.Price <= 0 || fname.Price <= 0)
                                    {
                                        continue;
                                    }
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
                                    fname.pcat = dr.Field<string>("Class");
                                    fname.pcat1 = dr.Field<string>("Family");
                                    fname.pcat2 = "";
                                    fname.uom = dr.Field<string>("Size");
                                    fname.region = "";
                                    fname.country = "";
                                    if (pmsk.Qty >= 1 && pmsk.Qty <= 999)
                                    {
                                        prodlist.Add(pmsk);
                                    }
                                    full.Add(fname);
                                }
                                Console.WriteLine("Generating Lightspeed " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Lightspeed " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Lightspeed " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For Lightspeed " + StoreId);

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
                                return "Not generated file for Lightspeed " + StoreId;
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
}
