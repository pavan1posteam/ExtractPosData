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


namespace ExtractPosData
{
    class clsBottlePos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public static int column_count;
        public clsBottlePos(int storeId, decimal Tax, decimal beertax, decimal winetax)
        {
            try
            {
                BottlePOsConvertRawFile(storeId, Tax, beertax, winetax);
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
                    column_count = dt.Columns.Count;

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
        public string BottlePOsConvertRawFile(int StoreId, decimal Tax, decimal beertax, decimal winetax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            string StaticQty = ConfigurationManager.AppSettings.Get("StaticQty");
            string Quantity = ConfigurationManager.AppSettings.Get("Quantity");
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

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();
                                if (clsBottlePos.column_count == 6)
                                {
                                    dynamic upcs;
                                    //dynamic taxs;
                                    int barlenth = 0;
                                    //dynamic upcs2;
                                    dt.DefaultView.Sort = "SKU";
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        try
                                        {
                                            ProductsModel pmsk = new ProductsModel();
                                            FullNameProductModel full = new FullNameProductModel();
                                            pmsk.StoreID = StoreId;

                                            if (dr[0].ToString().Contains("rows affected"))
                                            { continue; }
                                            dt.DefaultView.Sort = "SKU";
                                            upcs = dt.DefaultView.FindRows(dr["SKU"]).ToArray();
                                            /// upcs2 = dt.DefaultView.FindRows(dr["BARCODE2"]).ToArray();
                                            barlenth = ((Array)upcs).Length;
                                            pmsk.StoreID = StoreId;

                                            if (StoreId == 11113)
                                            {

                                                if (barlenth > 0)
                                                {
                                                    for (int i = 0; i <= barlenth - 1; i++)
                                                    {
                                                        if (i == 0)
                                                        {
                                                            if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                                            {
                                                                var upc = "#" + upcs[i]["UPC"].ToString().ToLower();
                                                                string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                                                if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                                                {
                                                                    if (!string.IsNullOrEmpty(numberUpc))
                                                                    {
                                                                        pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                                        full.upc = "#" + numberUpc.Trim().ToLower();
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
                                                            pmsk.altupc1 = "#" + upcs[i]["UPC"];
                                                        }
                                                        if (i == 2)
                                                        {
                                                            pmsk.altupc2 = "#" + upcs[i]["UPC"];
                                                        }
                                                        if (i == 3)
                                                        {
                                                            pmsk.altupc3 = "#" + upcs[i]["UPC"];
                                                        }
                                                        if (i == 4)
                                                        {
                                                            pmsk.altupc4 = "#" + upcs[i]["UPC"];
                                                        }
                                                        if (i == 5)
                                                        {
                                                            pmsk.altupc5 = "#" + upcs[i]["UPC"];
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                                {
                                                    pmsk.upc = "#" + dr["UPC"].ToString();
                                                    full.upc = "#" + dr["UPC"].ToString();
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                            if (StaticQty.Contains(StoreId.ToString()))
                                                pmsk.Qty = 999;
                                            else
                                            {
                                                decimal qty = Convert.ToDecimal(dr["QTY"]);
                                                pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                            }

                                            pmsk.sku = "#" + dr.Field<string>("SKU");
                                            full.sku = "#" + dr.Field<string>("SKU");

                                            if (!string.IsNullOrEmpty(dr.Field<string>("NAME")))
                                            {
                                                pmsk.StoreProductName = dr.Field<string>("NAME").Trim();
                                                pmsk.StoreDescription = dr.Field<string>("NAME").Trim();
                                                full.pname = dr.Field<string>("NAME").Trim();
                                                full.pdesc = dr.Field<string>("NAME").Trim();
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            pmsk.Price = System.Convert.ToDecimal(dr["PRICE"] == DBNull.Value ? 0 : dr["PRICE"]);
                                            full.Price = System.Convert.ToDecimal(dr["PRICE"] == DBNull.Value ? 0 : dr["PRICE"]);
                                            if (pmsk.Price <= 0 || full.Price <= 0)
                                            {
                                                continue;
                                            }
                                            full.pcat = "";
                                            full.pcat1 = "";
                                            full.pcat2 = "";
                                            full.country = "";
                                            full.region = "";
                                            pmsk.sprice = 0;
                                            pmsk.pack = 1;
                                            full.pack = 1;
                                            pmsk.uom = dr.Field<string>("SIZE");
                                            full.uom = dr.Field<string>("SIZE");
                                            pmsk.Tax = Tax;

                                            pmsk.Start = "";
                                            pmsk.End = "";
                                            pmsk.altupc1 = "";
                                            pmsk.altupc2 = "";
                                            pmsk.altupc3 = "";
                                            pmsk.altupc4 = "";
                                            pmsk.altupc5 = "";

                                            if (StoreId == 11113)
                                            {
                                                if (pmsk.Price > 0)
                                                {
                                                    fulllist.Add(full);
                                                    prodlist.Add(pmsk);
                                                }
                                            }
                                            else
                                            {
                                                if (Quantity.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0)
                                                {
                                                    fulllist.Add(full);
                                                    prodlist.Add(pmsk);
                                                    prodlist = prodlist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                                    fulllist = fulllist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                                }
                                                else if (pmsk.Qty > 0 && pmsk.Price > 0)
                                                {
                                                    fulllist.Add(full);
                                                    prodlist.Add(pmsk);
                                                    prodlist = prodlist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                                    fulllist = fulllist.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e.Message);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach(DataRow dr in dt.Rows)
                                    {
                                        try
                                        {
                                            ProductsModel pmsk = new ProductsModel();
                                            FullNameProductModel full = new FullNameProductModel();
                                            pmsk.StoreID = StoreId;
                                            Verify v = new Verify(dr, StoreId);
                                            if (!string.IsNullOrEmpty(v.GetStringByIndex(1)))
                                            {
                                                pmsk.upc = "#" + v.GetStringByIndex(1);
                                                pmsk.sku = "#" + v.GetStringByIndex(26);
                                                full.upc = pmsk.upc;
                                                full.sku = pmsk.sku;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            pmsk.StoreProductName = v.GetStringByIndex(3);
                                            pmsk.StoreDescription = pmsk.StoreProductName;
                                            pmsk.uom = v.GetStringByIndex(4);
                                            pmsk.Cost = v.GetDecimalByIndex(6);
                                            pmsk.Price = v.GetDecimalByIndex(8);
                                            pmsk.pack = v.getpack(pmsk.uom);
                                            if (pmsk.pack == 1)
                                                pmsk.pack = v.getpack(pmsk.StoreProductName);
                                            if (StaticQty.Contains(StoreId.ToString()))
                                                pmsk.Qty = 999;
                                            else
                                                pmsk.Qty = v.GetIntByIndex(18);
                                            


                                            full.pname = pmsk.StoreProductName;
                                            full.pdesc = pmsk.StoreProductName;
                                            full.uom = pmsk.uom;
                                            full.Price = pmsk.Price;
                                            full.pack = pmsk.pack;
                                            full.pcat = v.GetStringByIndex(15);
                                            full.pcat1 = v.GetStringByIndex(14);
                                            if (full.pcat.ToUpper().Contains("BEER"))
                                                pmsk.Tax = beertax;
                                            else if (full.pcat.ToUpper().Contains("WINE") || full.pcat.ToUpper().Contains("LIQUOR"))
                                                pmsk.Tax = winetax;
                                            else
                                                pmsk.Tax = Tax;
                                            if (Quantity.Contains(StoreId.ToString()))
                                            {
                                                if (pmsk.Price > 0 && pmsk.Qty > 0)
                                                {
                                                    fulllist.Add(full);
                                                    prodlist.Add(pmsk);
                                                }
                                            }
                                            else if (pmsk.Price > 0)
                                            {
                                                fulllist.Add(full);
                                                prodlist.Add(pmsk);
                                            }
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    }
                                }
                                Console.WriteLine("Generating BOTTLEPOS " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating BOTTLEPOS " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For BOTTLEPOS" + StoreId);
                                Console.WriteLine("Fullname File Generated For BOTTLEPOS" + StoreId);

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
                                return "Not generated file for BOTTLEPOS " + StoreId;
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
            return "Completed generating File For BOTTLEPOS" + StoreId;
        }
    }
   
}
