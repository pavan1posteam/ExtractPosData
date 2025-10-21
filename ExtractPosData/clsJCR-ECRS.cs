using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ExtractPosData
{
    class clsJCR_ECRS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsJCR_ECRS(int storeId, decimal Tax, int StoreMapId)
        {
            try
            {
                JCRECRSConvertRawFile(storeId, Tax, StoreMapId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        //older code
        #region
        //public static DataTable ConvertTXTToDataTable(string FileName)
        //{
        //    DataTable dtResult = new DataTable();

        //    //using (TextFieldParser parser = new TextFieldParser(FileName))
        //    //{
        //    //    try
        //    //    {
        //    //        parser.TextFieldType = FieldType.Delimited;
        //    //        parser.SetDelimiters(",");
        //    //        int i = 0;
        //    //        int r = 0;
        //    //        while (!parser.EndOfData)
        //    //        {
        //    //            if (i == 0)
        //    //            {
        //    //                string[] columns = parser.ReadFields();
        //    //                foreach (string col in columns)
        //    //                {
        //    //                    dtResult.Columns.Add(col);
        //    //                }
        //    //            }
        //    //            else
        //    //            {
        //    //                string[] rows = parser.ReadFields();
        //    //                dtResult.Rows.Add();
        //    //                int c = 0;
        //    //                foreach (string row in rows)
        //    //                {
        //    //                    var roww = row.Replace('"', ' ').Trim();

        //    //                    dtResult.Rows[r][c] = roww.ToString();
        //    //                    c++;
        //    //                }

        //    //                r++;
        //    //            }
        //    //            i++;
        //    //        }
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        Console.WriteLine(ex.Message);
        //    //    }
        //    }

        //    return dtResult; //Returning Dattable    
        //}
        #endregion
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
        public string JCRECRSConvertRawFile(int StoreId, decimal Tax, int StoreMapId)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreMapId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreMapId + "/Raw/");
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

                                    //if (!String.IsNullOrEmpty(dr.Field<string>("'Sale Price'")))
                                    //{
                                    //    pmsk.sprice = System.Convert.ToDecimal(dr["'Sale Price'"] == DBNull.Value ? 0 : dr["'Sale Price'"]);
                                    //}
                                    //else
                                    //{
                                    pmsk.sprice = 0;
                                    //}
                                    //if (pmsk.sprice > 0)
                                    //{
                                    //    pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                    //    pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                    //}
                                    //else
                                    //{
                                    //    pmsk.Start = "";
                                    //    pmsk.End = "";
                                    //}
                                    pmsk.Tax = Tax;
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    fname.pack = 1;
                                    fname.Price = System.Convert.ToDecimal(dr["'Retail Price'".ToLower()] == DBNull.Value ? 0 : dr["'Retail Price'".ToLower()]);
                                    if (pmsk.Price <= 0 || fname.Price <= 0)
                                    {
                                        continue;
                                    }
                                    fname.region = "";
                                    fname.country = "";
                                    string cat = dr["'Category'"].ToString();
                                    cat = Regex.Replace(cat, @"(')", "");
                                    fname.pcat = cat.ToString();
                                    string subcat = dr["'Subcategory'"].ToString();
                                    subcat = Regex.Replace(subcat, @"(')", "");
                                    fname.pcat1 = subcat.ToString();
                                    fname.pcat2 = "";
                                    string size = dr["'Unit of measure'"].ToString();
                                    size = Regex.Replace(size, @"(')", "");
                                    fname.uom = size.ToString();
                                    string store = dr["'Store'"].ToString();
                                    store = Regex.Replace(store, @"(')", "");
                                    store = store.ToString();
                                    if (StoreId == 11006)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES SR-16" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11007)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES DISCOUNT US-1 S" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11008)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES MANDARIN" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11009)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES AVONDALE" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11010)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES LANE AVE" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11011)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES ROADHOUSE" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11012)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES ARLINGTON" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11013)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES  CASSAT" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11014)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES  LEMTURNER" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11015)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES  EDGEWOOD" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11016)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES 103RD" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11017)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES BLANDING" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11021)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES  ARGYLE" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11340)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES CR-210 W" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11774)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES LIQUORS WELLS ROAD" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11778)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES LIQUORS MCDUFF" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 11953)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES SAN PABLO" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if(StoreId == 12177)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES LIQUORS RIVERTOWN" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    else if (StoreId == 12384)
                                    {
                                        if (pmsk.Qty > 0 && pmsk.upc.Trim() != "#" && pmsk.Price > 0 && store.ToUpper() == "SHORES  PALM VALLEY" && fname.pcat.ToUpper() != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fname);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            fulllist = fulllist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
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

                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreMapId + "/Raw/");

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
}