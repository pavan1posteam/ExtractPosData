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
    public class clsCStorePro
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsCStorePro(int storeid, decimal tax)
        {
            try
            {
                JensenConvertRawFile(storeid, tax);
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
                parser.SetDelimiters("|");
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
        public string JensenConvertRawFile(int StoreId, decimal Tax)
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
                                dt = dt.AsEnumerable().Skip(1).CopyToDataTable();
                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();
                                dynamic upcs;
                                dynamic taxs;
                                int barlenth = 0;
                                dynamic upcs2;
                                dt.DefaultView.Sort = "ITEM_NO";
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel fname = new FullNameProductModel();
                                    if (dr[0].ToString().Contains("rows affected"))
                                    { continue; }
                                    dt.DefaultView.Sort = "ITEM_NO";
                                    upcs = dt.DefaultView.FindRows(dr["ITEM_NO"]).ToArray();
                                    upcs2 = dt.DefaultView.FindRows(dr["BARCODE2"]).ToArray();
                                    barlenth = ((Array)upcs).Length;
                                    pmsk.StoreID = StoreId;

                                    if (barlenth > 0)
                                    {
                                        for (int i = 0; i <= barlenth - 1; i++)
                                        {
                                            if (i == 0)
                                            {
                                                if (!string.IsNullOrEmpty(dr["BARCODE2"].ToString()))
                                                {
                                                    var upc = "#" + upcs[i]["BARCODE2"].ToString().ToLower();
                                                    string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                                    if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                                    {
                                                        if (!string.IsNullOrEmpty(numberUpc))
                                                        {
                                                            pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                            fname.upc = "#" + numberUpc.Trim().ToLower();
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
                                            }
                                            if (i == 1)
                                            {
                                                pmsk.altupc1 = "#" + upcs[i]["BARCODE2"];
                                            }
                                            if (i == 2)
                                            {
                                                pmsk.altupc2 = "#" + upcs[i]["BARCODE2"];
                                            }
                                            if (i == 3)
                                            {
                                                pmsk.altupc3 = "#" + upcs[i]["BARCODE2"];
                                            }
                                            if (i == 4)
                                            {
                                                pmsk.altupc4 = "#" + upcs[i]["BARCODE2"];
                                            }
                                            if (i == 5)
                                            {
                                                pmsk.altupc5 = "#" + upcs[i]["BARCODE2"];
                                            }
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(dr["ITEM_NO"].ToString()))
                                    {
                                        pmsk.sku = "#" + dr["ITEM_NO"].ToString();
                                        fname.sku = "#" + dr["ITEM_NO"].ToString();
                                    }
                                    else
                                    { continue; }
                                    double qty = System.Convert.ToDouble(dr["QTY_AVAIL"]);
                                    if (qty > 0)
                                    {
                                        pmsk.Qty = (int)qty;
                                    }
                                    else { continue; }
                                    if (!string.IsNullOrEmpty(dr.Field<string>("DESCR").Trim()))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("DESCR").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("DESCR").Trim();
                                        fname.pdesc = dr.Field<string>("DESCR").Trim();
                                        fname.pname = dr.Field<string>("DESCR").Trim();
                                        pmsk.StoreDescription = Regex.Replace(pmsk.StoreDescription, @"'[^']+'(?=!\w+)", "");
                                        fname.pdesc = Regex.Replace(fname.pdesc, @"'[^']+'(?=!\w+)", "");
                                    }
                                    else
                                    { continue; }
                                    var p = dr["PRC_1"].ToString();

                                    if (p != "NULL")
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(dr["PRC_1"].ToString());
                                        fname.Price = System.Convert.ToDecimal(dr["PRC_1"].ToString());
                                    }
                                    else { continue; }
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
                                    fname.pcat = dr.Field<string>("CATEG_SUBCAT");
                                    fname.pcat1 = dr.Field<string>("SUBCAT_COD");
                                    fname.pcat2 = "";
                                    fname.uom = "";
                                    fname.region = "";
                                    fname.country = "";
                                    if (pmsk.Price > 0 && !string.IsNullOrEmpty(pmsk.upc))
                                    {
                                        prodlist.Add(pmsk);
                                        prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        full.Add(fname);
                                        full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                    }
                                }
                                List<ProductsModel> prodlist2 = new List<ProductsModel>();
                                List<FullNameProductModel> full2 = new List<FullNameProductModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk2 = new ProductsModel();
                                    FullNameProductModel fname2 = new FullNameProductModel();
                                    if (dr[0].ToString().Contains("rows affected"))
                                    { continue; }
                                    dt.DefaultView.Sort = "ITEM_NO";
                                    upcs = dt.DefaultView.FindRows(dr["ITEM_NO"]).ToArray();
                                    barlenth = ((Array)upcs).Length;
                                    if (barlenth > 0)
                                    {
                                        for (int i = 0; i <= barlenth - 1; i++)
                                        {
                                            if (i == 0)
                                            {
                                                if (!string.IsNullOrEmpty(dr["BARCODE2"].ToString()))
                                                {
                                                    var upc = "#" + dr["BARCODE2"].ToString().ToLower();
                                                    string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                                    if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                                    {
                                                        if (!string.IsNullOrEmpty(numberUpc))
                                                        {
                                                            pmsk2.upc = "#" + numberUpc.Trim().ToLower();
                                                            fname2.upc = "#" + numberUpc.Trim().ToLower();
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
                                                if (i == 1)
                                                {
                                                    pmsk2.altupc1 = "#" + upcs[i]["BARCODE2"];
                                                }
                                                if (i == 2)
                                                {
                                                    pmsk2.altupc2 = "#" + upcs[i]["BARCODE2"];
                                                }
                                                if (i == 3)
                                                {
                                                    pmsk2.altupc3 = "#" + upcs[i]["BARCODE2"];
                                                }
                                                if (i == 4)
                                                {
                                                    pmsk2.altupc4 = "#" + upcs[i]["BARCODE2"];
                                                }
                                                if (i == 5)
                                                {
                                                    pmsk2.altupc5 = "#" + upcs[i]["BARCODE2"];
                                                }
                                            }
                                        }
                                        string Altnumber1 = (dr["ALT_1_NUMER"]).ToString();
                                        string Altnumber2 = (dr["ALT_2_NUMER"]).ToString();
                                        string Altnumber3 = (dr["ALT_3_NUMER"]).ToString();
                                        string Altnumber4 = (dr["ALT_4_UNIT"]).ToString();
                                        string Altnumber5 = (dr["ALT_5_NUMER"]).ToString();
                                        pmsk2.StoreID = StoreId;
                                        if (Altnumber1 != "NULL" || Altnumber2 != "NULL" || Altnumber3 != "NULL" && Altnumber4 != "NULL" && Altnumber5 != "NULL")
                                        {                       
                                            string Altprice1 = (dr["ALT_1_PRC_1"]).ToString();
                                            string Altprice2 = dr["ALT_2_PRC_1"].ToString();
                                            string Altprice3 = dr["ALT_3_PRC_1"].ToString();
                                            string Altprice4 = dr["ALT_4_PRC_1"].ToString();
                                            string Altprice5 = dr["ALT_5_PRC_1"].ToString();

                                            if (Altnumber1 != "NULL" && Altprice1 != "NULL")
                                            {
                                                pmsk2.pack = Convert.ToInt32(Altnumber1);
                                                fname2.pack = Convert.ToInt32(Altnumber1);
                                                pmsk2.Price = Convert.ToDecimal(Altprice1);
                                            }
                                            else if (Altnumber2 != "NULL" && Altprice2 != "NULL")
                                            {
                                                pmsk2.pack = Convert.ToInt32(Altnumber2);
                                                fname2.pack = Convert.ToInt32(Altnumber2);
                                                pmsk2.Price = Convert.ToDecimal(Altprice2);
                                            }
                                           
                                            else if (Altnumber5 != "NULL" && Altprice5 != "NULL")
                                            {
                                                pmsk2.pack = Convert.ToInt32(Altnumber5);
                                                fname2.pack = Convert.ToInt32(Altnumber5);
                                                pmsk2.Price = Convert.ToDecimal(Altprice5);
                                            }
                                            double qty = System.Convert.ToDouble(dr["QTY_AVAIL"]);
                                            if (qty > 0 && pmsk2.pack > 0)
                                            {
                                                pmsk2.Qty = (int)qty / pmsk2.pack;
                                            }

                                            if (!string.IsNullOrEmpty(dr["ITEM_NO"].ToString()))
                                            {
                                                pmsk2.sku = "#" + dr["ITEM_NO"].ToString();
                                                fname2.sku = "#" + dr["ITEM_NO"].ToString();
                                            }
                                            else

                                            { continue; }

                                            if (!string.IsNullOrEmpty(dr.Field<string>("DESCR").Trim()))
                                            {
                                                pmsk2.StoreProductName = dr.Field<string>("DESCR").Trim();
                                                pmsk2.StoreDescription = dr.Field<string>("DESCR").Trim();
                                                fname2.pdesc = dr.Field<string>("DESCR").Trim();
                                                fname2.pname = dr.Field<string>("DESCR").Trim();
                                                pmsk2.StoreDescription = Regex.Replace(pmsk2.StoreDescription, @"'[^']+'(?=!\w+)", "");
                                                fname2.pdesc = Regex.Replace(fname2.pdesc, @"'[^']+'(?=!\w+)", "");
                                            }
                                            else
                                            { continue; }
                                          
                                            pmsk2.sprice = 0;
                                            pmsk2.Tax = Tax;
                                            if (pmsk2.sprice > 0)
                                            {
                                                pmsk2.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                                pmsk2.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                            }
                                            else
                                            {
                                                pmsk2.Start = "";
                                                pmsk2.End = "";
                                            }
                                            fname2.pcat = dr.Field<string>("CATEG_SUBCAT");
                                            fname2.pcat1 = dr.Field<string>("SUBCAT_COD");
                                            fname2.pcat2 = "";
                                            fname2.uom = "";
                                            fname2.region = "";
                                            fname2.country = "";

                                            if (pmsk2.Price > 0 && !string.IsNullOrEmpty(pmsk2.upc) && pmsk2.Qty>0)
                                            {
                                                prodlist2.Add(pmsk2);
                                                prodlist2 = prodlist2.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                                full2.Add(fname2);
                                                full2 = full2.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            }
                                         
                                        }
                                    }
                                }
                                prodlist = prodlist
                             .Concat(prodlist2)
                             .ToList();
                                full = full.Concat(full2).ToList();
                                 Console.WriteLine("Generating Jensen " + StoreId + " Product CSV Files.....");
                                 string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                 Console.WriteLine("Product File Generated For Jensen " + StoreId);
                                 Console.WriteLine();
                                 Console.WriteLine("Generating Jensen " + StoreId + " Fullname CSV Files.....");
                                 filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                 Console.WriteLine("Fullname File Generated For Jensen " + StoreId);
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
                                return "Not generated file for Jensen " + StoreId;
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
