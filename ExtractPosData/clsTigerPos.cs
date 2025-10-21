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
    class clsTigerPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        string DifferentFormateTigerPOS = ConfigurationManager.AppSettings.Get("DifferentFormateTigerPOS");
        string StaticQty = ConfigurationManager.AppSettings["StaticQty"];
        string Quantity = ConfigurationManager.AppSettings["Quantity"];

        public clsTigerPos(int StoreId, decimal Tax)
        {
            try
            {
                TigerConvertRawFile(StoreId, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static DataTable ConvertCsvToDataTable(string FileName, int StoreId)
        {
            if(StoreId == 10464 || StoreId == 12019)
            {
                DataTable dtResult = new DataTable();
                using (TextFieldParser parser = new TextFieldParser(FileName))
                {

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    int i = 0;
                    //int r = 0;
                    while (!parser.EndOfData)
                    {
                        try
                        {
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

                                    if (rows.Length > 6)
                                    {
                                        List<string> modifiedRows = new List<string>(rows);

                                        string thirdField = modifiedRows[2];
                                        modifiedRows.RemoveAt(3);
                                        modifiedRows[2] = thirdField;
                                        rows = modifiedRows.ToArray();
                                    }
                                    DataRow dataRow = dtResult.NewRow();
                                    for (int c = 0; c < rows.Length; c++)
                                    {
                                        var field = rows[c].Replace('"', ' ').Trim();
                                        dataRow[c] = field;
                                    }
                                    dtResult.Rows.Add(dataRow);
                                }

                                i++;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }

                    }
                }
                return dtResult; //Returning Datatable 
            }
            else
            {
                DataTable dtResult = new DataTable();
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
                            dtResult.Columns.Add(columns[i], typeof(string));
                        }

                        while (!parser.EndOfData)
                        {
                            string[] fields = parser.ReadFields();
                            DataRow newrow = dtResult.NewRow();
                            for (int i = 0; i < fields.Length; i++)
                            {
                                if (dtResult.Columns.Count != fields.Length)
                                {
                                    break;
                                }
                                newrow[i] = fields[i];
                            }
                            dtResult.Rows.Add(newrow);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                return dtResult;
            }
        }
        public string TigerConvertRawFile(int StoreId, decimal Tax)
        {
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
                                DataTable dt = ConvertCsvToDataTable(Url, StoreId);

                                List<ProductModel> prodlist = new List<ProductModel>();
                                List<FullNameProductModel> fullnamelist = new List<FullNameProductModel>();
                                if (DifferentFormateTigerPOS.Contains(StoreId.ToString()))
                                {

                                    if (StoreId == 12019)
                                    {
                                        foreach (DataRow dr in dt.Rows)
                                        {
                                            try
                                            {
                                                ProductModel pmsk = new ProductModel();
                                                pmsk.StoreID = StoreId;
                                                if (!string.IsNullOrEmpty(dr["UPC"].ToString()) && !dr["UPC"].ToString().Contains("---"))
                                                {
                                                    pmsk.upc = "#" + dr["UPC"].ToString();
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                if (!string.IsNullOrEmpty(dr["SKU"].ToString()) && !dr["SKU"].ToString().Contains("---"))
                                                {
                                                    pmsk.sku = "#" + dr["SKU"].ToString();
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                pmsk.Qty = Convert.ToInt32(Convert.ToDecimal(dr["QtyOnHand"]));

                                                pmsk.StoreProductName = dr.Field<string>("ItemName").Trim();
                                                pmsk.StoreDescription = dr.Field<string>("ItemName").Trim();
                                                pmsk.Price = Convert.ToDecimal(dr.Field<string>("Price"));
                                                pmsk.sprice = Convert.ToDecimal(dr.Field<string>("SalePrice"));
                                                if (pmsk.sprice > 0)
                                                {
                                                    pmsk.Start = DateTime.Today.ToString("MM/dd/yyyy"); ;
                                                    pmsk.End = "12/31/2999";
                                                }
                                                pmsk.uom = "";
                                                pmsk.pack = 1;
                                                pmsk.Tax = Tax;

                                                if (pmsk.Price > 0)
                                                {
                                                    prodlist.Add(pmsk);
                                                }
                                            }
                                            catch (Exception ex)
                                            { Console.WriteLine(ex.Message); }

                                        }


                                        Console.WriteLine("Generating TigerPos " + StoreId + " Product CSV Files.....");
                                        string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                        Console.WriteLine("Product File Generated For TigerPos  " + StoreId);
                                        Console.WriteLine();

                                        string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                        foreach (string filePath in filePaths)
                                        {
                                            string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                            File.Move(filePath, destpath);
                                        }
                                    }
                                    else
                                    {
                                        foreach (DataRow dr in dt.Rows)
                                        {
                                            try
                                            {
                                                ProductModel pmsk = new ProductModel();
                                                FullNameProductModel full = new FullNameProductModel();
                                                Verify v = new Verify(dr, StoreId);
                                                pmsk.StoreID = StoreId;
                                                if (!string.IsNullOrEmpty(v.GetStringByIndex(2)) && !v.GetStringByIndex(2).Contains("---"))
                                                {
                                                    pmsk.upc = "#" + v.GetStringByIndex(2);
                                                    full.upc = "#" + v.GetStringByIndex(2);
                                                    pmsk.sku = "#" + v.GetStringByIndex(2);
                                                    full.sku = "#" + v.GetStringByIndex(2);
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                                if (StaticQty.Contains(StoreId.ToString()))
                                                    pmsk.Qty = 999;
                                                else
                                                    pmsk.Qty = Convert.ToInt32(Convert.ToDecimal(v.GetDecimalByIndex(3)));
                                                pmsk.StoreProductName = v.GetStringByIndex(1).Trim();
                                                pmsk.StoreDescription = v.GetStringByIndex(1).Trim();
                                                full.pname = v.GetStringByIndex(1).Trim();
                                                full.pdesc = v.GetStringByIndex(1).Trim();
                                                pmsk.Price = Convert.ToDecimal(v.GetDecimalByIndex(7));
                                                full.Price = Convert.ToDecimal(v.GetDecimalByIndex(7));
                                                pmsk.uom = v.GetStringByIndex(4).Trim();
                                                full.uom = v.GetStringByIndex(4).Trim();
                                                full.pcat = v.GetStringByIndex(5).Trim();
                                                full.pcat1 = v.GetStringByIndex(6).Trim();
                                                pmsk.sprice = 0;
                                                pmsk.pack = v.getpack(pmsk.StoreProductName);
                                                full.pack = pmsk.pack;
                                                pmsk.Tax = Tax;
                                                pmsk.Start = "";
                                                pmsk.End = "";
                                                full.pcat2 = "";
                                                full.country = "";
                                                full.region = "";
                                                if (Quantity.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0)
                                                {
                                                    prodlist.Add(pmsk);
                                                    fullnamelist.Add(full);
                                                }
                                                else if (pmsk.Price > 0)
                                                {
                                                    prodlist.Add(pmsk);
                                                    fullnamelist.Add(full);
                                                }
                                            }
                                            catch (Exception ex)
                                            { Console.WriteLine(ex.Message); }
                                        }
                                        Console.WriteLine("Generating TigerPos " + StoreId + " Product CSV Files.....");
                                        string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                        Console.WriteLine("Product File Generated For TigerPos  " + StoreId);
                                        Console.WriteLine("Generating TigerPos " + StoreId + " Fullname CSV Files.....");
                                        filename = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                                        Console.WriteLine("Fullname File Generated For TigerPos  " + StoreId);
                                        Console.WriteLine();

                                        string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                        foreach (string filePath in filePaths)
                                        {
                                            string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                            File.Move(filePath, destpath);
                                        }
                                    }
                                }
                                else if (StoreId == 10464)
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        try
                                        {
                                            ProductModel pmsk = new ProductModel();
                                            FullNameProductModel full = new FullNameProductModel();
                                            pmsk.StoreID = StoreId;
                                            if (!string.IsNullOrEmpty(dr["ItemScanId"].ToString()) && !dr["ItemScanId"].ToString().Contains("---"))
                                            {
                                                pmsk.upc = "#" + dr["ItemScanId"].ToString();
                                                full.upc = "#" + dr["ItemScanId"].ToString();
                                                pmsk.sku = "#" + dr["ItemScanId"].ToString();
                                                full.sku = "#" + dr["ItemScanId"].ToString();
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            pmsk.Qty = Convert.ToInt32(Convert.ToDecimal(dr["QtyOnHand"]));
                                            pmsk.StoreProductName = dr.Field<string>("ItemName").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("ItemName").Trim();
                                            full.pname = dr.Field<string>("ItemName").Trim();
                                            full.pdesc = dr.Field<string>("ItemName").Trim();
                                            pmsk.Price = Convert.ToDecimal(dr.Field<string>("Price"));
                                            full.Price = Convert.ToDecimal(dr.Field<string>("Price"));
                                            pmsk.uom = dr.Field<string>("ItemSize").Trim();
                                            full.uom = dr.Field<string>("ItemSize").Trim();
                                            full.pcat = dr.Field<string>("Department").Trim();
                                            full.pcat1 = dr.Field<string>("Category").Trim();


                                            pmsk.sprice = 0;
                                            pmsk.pack = 1;
                                            full.pack = 1;
                                            pmsk.Tax = Tax;
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                            full.pcat2 = "";
                                            full.country = "";
                                            full.region = "";
                                            if (pmsk.Price > 0)
                                            {
                                                prodlist.Add(pmsk);
                                                fullnamelist.Add(full);
                                                //prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            }
                                        }
                                        catch (Exception ex)
                                        { Console.WriteLine(ex.Message); }
                                    }

                                    Console.WriteLine("Generating TigerPos " + StoreId + " Product CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    Console.WriteLine("Product File Generated For TigerPos  " + StoreId);
                                    Console.WriteLine();

                                    string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                    foreach (string filePath in filePaths)
                                    {
                                        string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                        File.Move(filePath, destpath);
                                    }
                                }


                                //for  new stores 
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        try
                                        {
                                            ProductModel pmsk = new ProductModel();
                                            FullNameProductModel full = new FullNameProductModel();
                                            Verify v = new Verify(dr, StoreId);
                                            pmsk.StoreID = StoreId;
                                            if (!string.IsNullOrEmpty(v.GetStringByIndex(2)) && !v.GetStringByIndex(2).Contains("---"))
                                            {
                                                pmsk.upc = "#" + v.GetStringByIndex(2);
                                                full.upc = pmsk.upc;
                                                pmsk.sku = "#" + v.GetStringByIndex(0);
                                                full.sku = pmsk.sku;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            if (StaticQty.Contains(StoreId.ToString()))
                                                pmsk.Qty = 999;
                                            else
                                                pmsk.Qty = v.GetDecimalByIndex(4);
                                            pmsk.StoreProductName = v.GetStringByIndex(1).Trim();
                                            pmsk.StoreDescription = v.GetStringByIndex(1).Trim();
                                            full.pname = pmsk.StoreProductName;
                                            full.pdesc = pmsk.StoreDescription;
                                            pmsk.Price = v.GetDecimalByIndex(7);
                                            full.Price = pmsk.Price;
                                            pmsk.uom = v.getVolume(v.GetStringByIndex(3)).Trim();
                                            full.uom = pmsk.uom;
                                            full.pcat = v.GetStringByIndex(5).Trim();
                                            full.pcat1 = v.GetStringByIndex(6).Trim();
                                            pmsk.sprice = 0;
                                            pmsk.pack = v.getpack(v.GetStringByIndex(3));
                                            full.pack = pmsk.pack;
                                            pmsk.Tax = Tax;
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                            full.pcat2 = "";
                                            full.country = "";
                                            full.region = "";
                                            if (Quantity.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0)
                                            {
                                                prodlist.Add(pmsk);
                                                fullnamelist.Add(full);
                                            }
                                            else if (pmsk.Price > 0)
                                            {
                                                prodlist.Add(pmsk);
                                                fullnamelist.Add(full);
                                            }
                                        }
                                        catch (Exception ex)
                                        { Console.WriteLine(ex.Message); }
                                    }

                                    Console.WriteLine("Generating TigerPos " + StoreId + " Product CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    Console.WriteLine("Product File Generated For TigerPos  " + StoreId);
                                    Console.WriteLine("Generating TigerPos " + StoreId + " Fullname CSV Files.....");
                                    filename = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                                    Console.WriteLine("Fullname File Generated For TigerPos  " + StoreId);
                                    Console.WriteLine();

                                    string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                    foreach (string filePath in filePaths)
                                    {
                                        string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                        File.Move(filePath, destpath);
                                    }
                                }

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for AdventPOSFlat " + StoreId;
                            }
                        }
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
       
