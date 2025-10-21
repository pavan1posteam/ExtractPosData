using ExcelDataReader;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtractPosData
{
    class clsLightningPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsLightningPos(int StoreId, decimal Tax)
        {
            try
            {
                clsLightningPosConvertRawFile(StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertCsvToDataTable(string FileName, int StoreId)
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
                            if (StoreId == 11399 && c > 5)
                            {
                                break;
                            }
                            var roww = row.Replace('"', ' ').Trim();
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

        //public static DataTable ConvertExcelToDataTable(string FileName)
        //{
        //    try
        //    {
        //        DataTable dtResult = new DataTable();
        //        FileStream stream = File.Open(FileName, FileMode.Open, FileAccess.Read);
        //        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
        //        DataSet result = excelReader.AsDataSet();
        //        excelReader.Close();
        //        dtResult = result.Tables[0];

        //        int count = dtResult.Columns.Count;
        //        var cName = dtResult.Rows[0];

        //        for (int i = 0; i < count; i++)
        //        {
        //            string columnName = cName[i]?.ToString();
        //            if (!string.IsNullOrWhiteSpace(columnName))
        //            {
        //                dtResult.Columns[i].ColumnName = columnName;
        //            }
        //            else
        //            {
        //                // Provide a default name for empty column names.
        //                dtResult.Columns[i].ColumnName = "Column" + i;
        //            }
        //        }
        //        for (int i = dtResult.Columns.Count - 1; i >= 0; i--)
        //        {
        //            string columnName = dtResult.Columns[i].ColumnName;
        //            bool isEmptyColumn = dtResult.AsEnumerable().All(row => string.IsNullOrWhiteSpace(row.Field<string>(columnName)));
        //            if (isEmptyColumn)
        //            {
        //                dtResult.Columns.RemoveAt(i);
        //            }
        //        }

        //        // Remove the first row (column headers) from the DataTable.
        //        dtResult.Rows.RemoveAt(0);

        //        return dtResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        // You can log the exception or return null, an empty DataTable, or some other error indication.
        //        Console.WriteLine(ex.Message);
        //        return null;
        //    }
        //}
        public static DataTable ConvertExcelToDataTable(string fileName)
        {
            try
            {
                FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
                IExcelDataReader excelReader;

                if (Path.GetExtension(fileName).Equals(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    excelReader = ExcelReaderFactory.CreateBinaryReader(stream); // For .xls
                }
                else if (Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream); // For .xlsx
                }
                else
                {
                    throw new NotSupportedException("Unsupported file extension.");
                }

                using (excelReader)
                {
                    var result = excelReader.AsDataSet();
                    if (result.Tables.Count == 0)
                        return null;

                    DataTable dtResult = result.Tables[0];

                    if (dtResult.Rows.Count == 0)
                        return null;

                    // Set column names from first row
                    var headerRow = dtResult.Rows[0];
                    for (int i = 0; i < dtResult.Columns.Count; i++)
                    {
                        string columnName = headerRow[i]?.ToString()?.Trim();
                        dtResult.Columns[i].ColumnName = string.IsNullOrWhiteSpace(columnName) ? $"Column{i}" : columnName;
                    }

                    // Remove header row
                    dtResult.Rows.RemoveAt(0);

                    // Remove completely empty columns
                    for (int i = dtResult.Columns.Count - 1; i >= 0; i--)
                    {
                        bool isEmptyColumn = dtResult.AsEnumerable().All(row =>
                        {
                            var value = row[i];
                            return value == null || string.IsNullOrWhiteSpace(value.ToString());
                        });

                        if (isEmptyColumn)
                        {
                            dtResult.Columns.RemoveAt(i);
                        }
                    }

                    return dtResult;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading Excel file: " + ex.Message);
                return null;
            }
        }
        public string clsLightningPosConvertRawFile(int StoreId, decimal Tax)
        {
            DataTable dt = new DataTable();
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            string xlmsFormate = ConfigurationManager.AppSettings.Get("xlmsFormate");
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
                                if (Url.Contains(".xlsx") || Url.Contains(".xls"))
                                {
                                    dt = ConvertExcelToDataTable(Url);
                                }
                                else if (Url.Contains(".csv"))
                                {
                                    dt = ConvertCsvToDataTable(Url, StoreId);
                                }
                                List<LightningModel> prodlist = new List<LightningModel>();
                                List<FullNameProductModels> fulllist = new List<FullNameProductModels>();
                                if (xlmsFormate.Contains(StoreId.ToString()))
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        LightningModel pmsk = new LightningModel();
                                        FullNameProductModels fmsk = new FullNameProductModels();
                                        pmsk.StoreID = StoreId;

                                        if (decimal.TryParse(dr["QtyonHand"]?.ToString(), out decimal qty) && dr["SellonInternet"].ToString() == "Y")
                                        {
                                            string SKU = dr["SKU"]?.ToString();
                                            string UPC = dr["SKU 4th"]?.ToString();
                                            UPC = Regex.Replace(UPC, @"\s+", "");
                                            SKU = Regex.Replace(SKU, @"\s+", "");
                                            UPC = Regex.Replace(UPC, @"[^0-9]+", "");
                                            SKU = Regex.Replace(SKU, @"[^0-9]+", "");
                                           
                                            if (!string.IsNullOrEmpty(UPC))
                                            {
                                                pmsk.upc = "#" + UPC;
                                                fmsk.upc = "#" + UPC;
                                            }
                                            else if (!string.IsNullOrEmpty(SKU))
                                            {
                                                pmsk.upc = "#" + SKU;
                                                fmsk.upc = "#" + SKU;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            if (StoreId == 11897 && !string.IsNullOrEmpty(SKU))
                                            {
                                                pmsk.upc = "#" + SKU;
                                                fmsk.upc = "#" + SKU;
                                            }
                                            if (!string.IsNullOrEmpty(SKU))
                                            {
                                                pmsk.sku = "#" + SKU;
                                                fmsk.sku = "#" + SKU;
                                            }
                                            else if (!string.IsNullOrEmpty(UPC))
                                            {
                                                pmsk.sku = pmsk.upc;
                                                fmsk.sku = fmsk.upc;
                                            }


                                            fmsk.pname = dr.Field<string>("Description").Trim();
                                            fmsk.pdesc = dr.Field<string>("Description").Trim();

                                            pmsk.Qty = dr["QtyonHand"]?.ToString();
                                            var uom = dr["Size"]?.ToString();
                                            pmsk.uom =Regex.Replace(uom, @"\s+", "");
                                            fmsk.uom = Regex.Replace(uom, @"\s+", "");
                                            pmsk.StoreProductName = dr.Field<string>("Description")?.Trim();
                                            pmsk.StoreDescription = dr.Field<string>("Description")?.Trim();
                                            if (decimal.TryParse(dr["PromoPrice"]?.ToString(), out decimal sprice))
                                            {
                                                pmsk.sprice = sprice;                                               
                                            }
                                            pmsk.Start = dr["PromoStartDate"].ToString();
                                            pmsk.End = dr["PromoEndDate"].ToString();
                                            pmsk.pack = 1;
                                            fmsk.pack = 1;
                                            fmsk.pcat = dr.Field<string>("Department")?.Trim();
                                            fmsk.pcat1 = dr.Field<string>("Style")?.Trim();
                                            fmsk.pcat2 = "";

                                            pmsk.Tax = Tax;
                                            pmsk.altupc1 = "";
                                            pmsk.altupc2 = "";
                                            pmsk.altupc3 = "";
                                            pmsk.altupc4 = "";
                                            pmsk.altupc5 = "";
                                            fmsk.region = "";
                                            fmsk.country = "";

                                            if (decimal.TryParse(dr["Price"]?.ToString(), out decimal price) && price > 0)
                                            {
                                                pmsk.Price = price;
                                                fmsk.Price = price;
                                               
                                            }
                                            var lastsold = dr["LastSold"].ToString();
                                            if (DateTime.TryParse(lastsold, out DateTime date))
                                            {
                                                if (date.Year >= 2022)
                                                {
                                                    prodlist.Add(pmsk);
                                                    fulllist.Add(fmsk);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        LightningModel pmsk = new LightningModel();
                                        FullNameProductModels fmsk = new FullNameProductModels();
                                        Verify v = new Verify(StoreId, dr);
                                        pmsk.StoreID = StoreId;
                                        
                                        if(StoreId == 12259)
                                        {
                                            pmsk.upc = dr["upc"].ToString();
                                            pmsk.Qty = dr["qty"].ToString();
                                            pmsk.sku = dr.Field<string>("SKU");
                                            pmsk.uom = dr["uom"].ToString();
                                            pmsk.StoreProductName = dr.Field<string>("StoreProductName").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("Storedescription").Trim();
                                            pmsk.Price = Convert.ToDecimal(dr["price"]);  
                                            if(!string.IsNullOrEmpty(dr["sprice"].ToString()))
                                            pmsk.sprice = Convert.ToDecimal(dr["sprice"]);
                                            pmsk.pack = Convert.ToInt32(dr["pack"]);
                                            pmsk.Tax = Tax;
                                            pmsk.altupc1 = "";
                                            pmsk.altupc2 = "";
                                            pmsk.altupc3 = "";
                                            pmsk.altupc4 = "";
                                            pmsk.altupc5 = "";
                                        }
                                        else if (StoreId == 12261)
                                        {
                                            pmsk.upc = v.GetString("ucode1");
                                            pmsk.Qty = v.GetString("stock");
                                            pmsk.sku = v.GetString("ucode1");
                                            pmsk.uom = v.GetString("sname");
                                            pmsk.StoreProductName = v.GetString("name");
                                            pmsk.StoreDescription = v.GetString("name");
                                            pmsk.Price = v.GetDecimal("price1");
                                            pmsk.pack = 1;
                                            pmsk.Tax = Tax;
                                            pmsk.altupc1 = v.GetString("ucode2");
                                            pmsk.altupc2 = v.GetString("ucode3");
                                            pmsk.altupc3 = v.GetString("ucode4");
                                            pmsk.altupc4 = "";
                                            pmsk.altupc5 = "";
                                        }
                                        else if(StoreId == 12289)
                                        {
                                            Verify v1 = new Verify(dr, StoreId);
                                            if (!string.IsNullOrEmpty(v1.GetStringByIndex(0)))
                                            {
                                                var upc = v1.GetStringByIndex(0).ToLower();
                                                string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                                pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                pmsk.sku = "#" + v1.GetStringByIndex(0);
                                                fmsk.upc = pmsk.upc;
                                                fmsk.sku = pmsk.sku;
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                            pmsk.StoreProductName = v1.GetStringByIndex(1);
                                            pmsk.StoreDescription = v1.GetStringByIndex(1);
                                            fmsk.pname = pmsk.StoreProductName;
                                            fmsk.pdesc = pmsk.StoreDescription;
                                            var uomsize = v1.GetStringByIndex(2);
                                            pmsk.uom = Regex.Replace(uomsize, @"(?<=\s|^)\.(\d+L)\b", "$1");
                                            fmsk.uom = pmsk.uom;
                                            pmsk.Price = v1.GetDecimalByIndex(4);
                                            fmsk.Price = pmsk.Price;
                                            pmsk.Qty = v1.GetStringByIndex(5);
                                            fmsk.pack = 1;
                                            pmsk.sprice = 0;
                                            pmsk.pack = 1;
                                            pmsk.Tax = Tax;
                                            pmsk.altupc1 = "";
                                            pmsk.altupc2 = "";
                                            pmsk.altupc3 = "";
                                            pmsk.altupc4 = "";
                                            pmsk.altupc5 = "";
                                            fmsk.pcat = v1.GetStringByIndex(3);
                                            fmsk.pcat1 = "";
                                            fmsk.pcat2 = "";
                                            fmsk.region = "";
                                            fmsk.country = "";
                                        }
                                        else
                                        {
                                            pmsk.upc = "#" + dr["SKU"].ToString();
                                            pmsk.Qty = dr["QtyonHand"].ToString();
                                            pmsk.sku = dr.Field<string>("SKU");
                                            pmsk.uom = "";
                                            pmsk.StoreProductName = dr.Field<string>("Description").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("Description").Trim();
                                            pmsk.Price = Convert.ToDecimal(dr["Price"]);  
                                            pmsk.sprice = 0;
                                            pmsk.pack = 1;
                                            pmsk.Tax = Tax;
                                            pmsk.altupc1 = "";
                                            pmsk.altupc2 = "";
                                            pmsk.altupc3 = "";
                                            pmsk.altupc4 = "";
                                            pmsk.altupc5 = "";
                                        }
                                        #region FullName File
                                        //fmsk.pname = dr.Field<string>("Description").Trim();
                                        //fmsk.pdesc = dr.Field<string>("Description").Trim();
                                        //fmsk.sku = "#" + dr["SKU"].ToString();
                                        //fmsk.upc = "#" + dr["SKU"].ToString();
                                        //fmsk.uom = dr.Field<string>("Size");
                                        //fmsk.Price = (dr["Price"]).ToString();  ////Price_Drizly
                                        //fmsk.pack = 1;
                                        //fmsk.pcat = dr.Field<string>("Department").Trim(); ;
                                        //fmsk.pcat1 = "";
                                        //fmsk.pcat2 = "";
                                        #endregion
                                        if (StoreId == 12289 && Convert.ToDecimal(pmsk.Price) > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(fmsk);
                                        }
                                        else if (Convert.ToDecimal(pmsk.Price) > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            //fulllist.Add(fmsk);
                                        }
                                    }
                                }
                                Console.WriteLine("Generating Lightning " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Lightning" + StoreId);
                                Console.WriteLine("Generating Lightning " + StoreId + " fullname CSV Files.....");
                                string filenames = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("fullname File Generated For Lightning" + StoreId);
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
                                return "Not generated file for Lightning " + StoreId;
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
            return "Completed generating File For Lightning" + StoreId;
        }

        public class LightningModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public string Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string uom { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public decimal sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal Tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }
            public decimal Deposit { get; set; }
        }
        class FullNameProductModels
        {
            public string pname { get; set; }
            public string pdesc { get; set; }
            public string upc { get; set; }
            public string sku { get; set; }
            public decimal Price { get; set; }
            public string uom { get; set; }
            public int pack { get; set; }
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string country { get; set; }
            public string region { get; set; }
        }
    }
}
