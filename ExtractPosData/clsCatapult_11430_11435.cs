using ExtractPosData;
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
class clsCatapult_11430_11435
{
    string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
    public clsCatapult_11430_11435(int storeId, decimal Tax, int StoreMapId)
    {
        try
        {
            CatapultConvertRawFile(storeId, Tax, StoreMapId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    public static DataTable ConvertCsvToDataTable(string FileName)
    {
        DataTable dt = new DataTable();
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
                    dt.Columns.Add(columns[i], typeof(string));
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return dt; //Returning Dattable  
    }
    public string CatapultConvertRawFile(int StoreId, decimal Tax, int StoreMapId)
    {
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        if (Directory.Exists(BaseUrl))
        {
            if (Directory.Exists(BaseUrl + "/" +StoreId+  "/Raw/"))
            {
                var directory = new DirectoryInfo(BaseUrl + "/" + StoreMapId +  "/Raw/");
                var myFile = (from f in directory.GetFiles()
                              orderby f.LastWriteTime descending
                              select f).First();

                string Url = BaseUrl + "/" + StoreMapId + "/Raw/" + myFile;
                if (File.Exists(Url))
                {
                    try
                    {
                        DataTable dt = ConvertCsvToDataTable(Url);

                        var dtr = from s in dt.AsEnumerable() select s;
                        List<ProductModel> prodlist = new List<ProductModel>();
                        List<FullnameModel> fulllist = new List<FullnameModel>();
                        try
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                ProductModel pmsk = new ProductModel();
                                FullnameModel fname = new FullnameModel();
                                pmsk.StoreID = StoreId;

                                string upc = dr["'UPC'"].ToString().Trim();
                                string numberUpc = Regex.Replace(upc, "[^0-9.]", "");

                                pmsk.upc = '#' + numberUpc;
                                pmsk.sku = '#' + numberUpc;
                                fname.upc = '#' + numberUpc;
                                fname.sku = '#' + numberUpc;
                                // }
                                var qty = Convert.ToString(dr["'Quantity on hand'"]);
                                string numberqty = Regex.Replace(qty, "[^0-9.]", "");
                                if (!string.IsNullOrEmpty(numberqty))
                                {
                                    var qty1 = Convert.ToDecimal(numberqty);
                                    pmsk.Qty = Convert.ToInt64(qty1);
                                }
                                pmsk.pack = 1;
                                string prodname = dr["'Product Name'"].ToString().Trim();
                                prodname = Regex.Replace(prodname, @"(')", "");
                                pmsk.StoreProductName = prodname.ToString().Trim();
                                pmsk.StoreDescription = prodname.ToString().Trim();
                                fname.pname = prodname.ToString().Trim();
                                fname.pdesc = prodname.ToString().Trim();
                                pmsk.Price = System.Convert.ToDecimal(dr["'Retail Price'".ToLower()] == DBNull.Value ? 0 : dr["'Retail Price'".ToLower()]);
                                pmsk.sprice = 0;
                                pmsk.Tax = Tax;
                                pmsk.altupc1 = "";
                                pmsk.altupc2 = "";
                                pmsk.altupc3 = "";
                                pmsk.altupc4 = "";
                                pmsk.altupc5 = "";
                                fname.pack = 1;
                                fname.Price = System.Convert.ToDecimal(dr["'Retail Price'".ToLower()] == DBNull.Value ? 0 : dr["'Retail Price'".ToLower()]);
                                fname.region = "";
                                fname.country = "";
                                string cat = dr["'Subcategory'"].ToString(); 
                                cat = Regex.Replace(cat, @"(')", "");
                                fname.pcat = cat.ToString();

                                string subcat = dr["'Category'"].ToString();
                                subcat = Regex.Replace(subcat, @"(')", "");
                                fname.pcat1 = subcat.ToString();
                                fname.pcat2 = "";
                                string size = dr["'Unit of measure'"].ToString();
                                size = Regex.Replace(size, @"(')", "");
                                fname.uom = size.ToString();
                                pmsk.uom = size.ToString();
                                string store = dr["'Store'"].ToString();
                                store = Regex.Replace(store, @"(')", "");
                                store = store.ToString();

                                if (StoreId == 11430)
                                {
                                    if (pmsk.Qty > 0 && pmsk.Price > 0 && store.ToUpper() == "PJ LIQUOR" && fname.pcat.ToUpper() != "TOBACCO")
                                    {
                                        prodlist.Add(pmsk);
                                        fulllist.Add(fname);
                                    }
                                }
                                else if (StoreId == 11435)
                                {
                                    if (pmsk.Qty > 0 && pmsk.Price > 0 && store.ToUpper() == "PJ LIQUOR BEACH" && fname.pcat.ToUpper() != "TOBACCO")
                                    {
                                        prodlist.Add(pmsk);
                                        fulllist.Add(fname);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("" + e.Message);
                        }
                        Console.WriteLine("Generating JCRECRS " + StoreId + " Product CSV Files.....");
                        Console.WriteLine("Generating JCRECRS " + StoreId + " Full Name CSV Files.....");
                        string pfilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                        string filename = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                        Console.WriteLine("Product File Generated For JCRECRS " + StoreId);
                        Console.WriteLine("Full Name File Generated For JCRECRS " + StoreId);

                        string[] filePaths = Directory.GetFiles(BaseUrl + "/"+ StoreMapId + "/Raw/");

                        //foreach (string filePath in filePaths)
                        //{
                        //    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                        //    File.Move(filePath, destpath);
                        //}
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("" + e.Message);
                        (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                        return "Not generated file for JCRECRS " + StoreId;
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
        return "Completed generating File For JCRECRS" + StoreId;
    }
}
