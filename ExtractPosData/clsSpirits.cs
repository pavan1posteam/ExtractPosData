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
    public class clsSpirits
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsSpirits(int StoreId, decimal Tax)
        {
            try
            {
                SpiritsConvertRawFile(StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public DataTable ConvertCsvToDataTable(string FileName)
        {
            DataTable dt = new DataTable();

            try
            {
                bool firstFile = true;
                using (TextFieldParser parser = new TextFieldParser(FileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.Delimiters = new string[] { "," };
                    parser.HasFieldsEnclosedInQuotes = true;

                    string[] columns = parser.ReadFields();

                    if (firstFile == true)
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            dt.Columns.Add(columns[i], typeof(string));
                        }
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        DataRow newrow = dt.NewRow();

                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (dt.Columns.Count != fields.Length)
                            {
                                break;
                            }
                            newrow[i] = fields[i];
                        }

                        dt.Rows.Add(newrow);
                    }
                }
                firstFile = false;
            }
            catch (Exception)
            {
            }
            return dt;
        }
        public string SpiritsConvertRawFile(int StoreId, decimal Tax)
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
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["UPC"].ToString();
                                        full.upc = "#" + dr["UPC"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    var q = dr["Qty"].ToString();
                                    if (!string.IsNullOrEmpty(q))
                                    {
                                        decimal qty = Convert.ToDecimal(dr["Qty"]);
                                        pmsk.Qty = Convert.ToInt32(qty);
                                    }
                                    else { 
                                        continue;
                                    }
                                    if (!string.IsNullOrEmpty(dr["Sku"].ToString()))
                                    {
                                        if (dr["Sku"].ToString().Contains("#"))
                                        {
                                            pmsk.sku = dr["Sku"].ToString();
                                            full.sku = dr["Sku"].ToString();
                                        }
                                        else
                                        {
                                            pmsk.sku = "#" + dr["Sku"].ToString();
                                            full.sku = "#" + dr["Sku"].ToString();
                                        }
                                    }
                                    else { 

                                        continue;
                                    }
                                    if (!string.IsNullOrEmpty(dr.Field<string>("storeproductname")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("storeproductname");
                                        pmsk.StoreDescription = dr.Field<string>("storedescription").Trim();
                                        full.pname = dr.Field<string>("storeproductname");
                                        full.pdesc = dr.Field<string>("storedescription");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    var p = dr["Price"].ToString();
                                    if (!string.IsNullOrEmpty(p) && !Char.IsLetter(p[0]))
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                        full.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    }
                                    else { continue; }
                                    pmsk.sprice = System.Convert.ToDecimal(dr["SPrice"] == DBNull.Value ? 0 : dr["SPrice"]);
                                    pmsk.pack = 1;
                                    pmsk.Tax = Tax;

                                    if (pmsk.sprice > 0)
                                    {
                                        pmsk.Start = dr["Start"].ToString();
                                        pmsk.End = dr["End"].ToString();
                                    }
                                    else
                                    {
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                    }
                                    full.uom = ""; 
                                    full.pcat = "";
                                    full.pcat1 = ""; 
                                    full.pcat2 = ""; 
                                    full.country = ""; 
                                    full.region = "";
                                    pmsk.altupc1 = ""; 
                                    pmsk.altupc2 = ""; 
                                    pmsk.altupc3 = ""; 
                                    pmsk.altupc4 = ""; 
                                    pmsk.altupc5 = "";
                                    if (pmsk.Qty > 0 && pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        fulllist.Add(full);
                                    }
                                }
                                Console.WriteLine("Generating Spirits " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating Spirits " + StoreId + " FullName CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Spirits");
                                Console.WriteLine("Full Name File Generated For Spirits");

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
                                return "Not generated file for Spirits " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName";
                        }
                    }
                    else
                    {
                        Console.WriteLine("There is no file in the Raw Folder of " + StoreId);
                        return "";
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
            return "Completed generating File";
        }
    }
}
