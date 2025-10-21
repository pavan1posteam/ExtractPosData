using ExcelDataReader;
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
    class clsEbsPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsEbsPos(int storeid, decimal tax)
        {
            try
            {
                EBSConvertRawFile(storeid, tax);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertExcelToDataTable(string fileName)
        {
            try
            {
                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    IExcelDataReader reader = null;
                    var ext = Path.GetExtension(fileName).ToLower();

                    if (ext == ".xls")
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (ext == ".xlsx")
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported file extension: " + ext);
                    }

                    using (reader)
                    {
                        var result = reader.AsDataSet();
                        if (result.Tables.Count == 0)
                            return null;

                        DataTable raw = result.Tables[0];

                        // Manually detect first data row
                        int startRowIndex = -1;
                        for (int i = 0; i < raw.Rows.Count; i++)
                        {
                            var cell1 = raw.Rows[i][0]?.ToString();
                            var cell2 = raw.Rows[i][1]?.ToString();

                            int temp;
                            if (int.TryParse(cell1, out temp) || int.TryParse(cell2, out temp))
                            {
                                startRowIndex = i;
                                break;
                            }
                        }

                        if (startRowIndex == -1)
                            throw new Exception("No valid data row found.");

                        // Clone structure and copy from detected row
                        DataTable clean = raw.Clone();
                        for (int i = startRowIndex; i < raw.Rows.Count; i++)
                        {
                            clean.ImportRow(raw.Rows[i]);
                        }

                        return clean;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading Excel: " + ex.Message);
                return null;
            }
        }

        //public static DataTable ConvertExcelToDataTable(string fileName)
        //{
        //    try
        //    {
        //        FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read);
        //        IExcelDataReader excelReader;

        //        if (Path.GetExtension(fileName).Equals(".xls", StringComparison.OrdinalIgnoreCase))
        //        {
        //            excelReader = ExcelReaderFactory.CreateBinaryReader(stream); // For .xls
        //        }
        //        else if (Path.GetExtension(fileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        //        {
        //            excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream); // For .xlsx
        //        }
        //        else
        //        {
        //            throw new NotSupportedException("Unsupported file extension.");
        //        }

        //        using (excelReader)
        //        {
        //            var result = excelReader.AsDataSet();
        //            if (result.Tables.Count == 0)
        //                return null;

        //            DataTable dtResult = result.Tables[0];

        //            if (dtResult.Rows.Count == 0)
        //                return null;

        //            // Set column names from first row
        //            var headerRow = dtResult.Rows[0];
        //            for (int i = 0; i < dtResult.Columns.Count; i++)
        //            {
        //                string columnName = headerRow[i]?.ToString()?.Trim();
        //                dtResult.Columns[i].ColumnName = string.IsNullOrWhiteSpace(columnName) ? $"Column{i}" : columnName;
        //            }

        //            // Remove header row
        //            dtResult.Rows.RemoveAt(0);

        //            // Remove completely empty columns
        //            for (int i = dtResult.Columns.Count - 1; i >= 0; i--)
        //            {
        //                bool isEmptyColumn = dtResult.AsEnumerable().All(row =>
        //                {
        //                    var value = row[i];
        //                    return value == null || string.IsNullOrWhiteSpace(value.ToString());
        //                });

        //                if (isEmptyColumn)
        //                {
        //                    dtResult.Columns.RemoveAt(i);
        //                }
        //            }
        //            return dtResult;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error reading Excel file: " + ex.Message);
        //        return null;
        //    }
        //}
        public static DataTable ConvertCsvToDataTable(string FileName)
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

        public string EBSConvertRawFile(int StoreId, decimal Tax)
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
                                DataTable dt = new DataTable();
                                if (Url.Contains(".xlsx") || Url.Contains(".xls"))
                                {
                                    dt = ConvertExcelToDataTable(Url);
                                }
                                else if (Url.Contains(".csv"))
                                {
                                    dt = ConvertCsvToDataTable(Url);
                                }
                                List<ProductsModelEBS> prodlist = new List<ProductsModelEBS>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModelEBS pmsk = new ProductsModelEBS();
                                    FullNameProductModel fname = new FullNameProductModel();
                                    Verify v = new Verify(dr, StoreId);
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(v.GetStringByIndex(1)) || !string.IsNullOrEmpty(v.GetStringByIndex(0)))
                                    {
                                        var upc = v.GetStringByIndex(0).ToLower();
                                        var sku = v.GetStringByIndex(1).ToLower();
                                        string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                        string numberSku = Regex.Replace(sku, "[^0-9.]", "");
                                        if (!string.IsNullOrEmpty(numberUpc) && !string.IsNullOrEmpty(numberSku))
                                        {
                                            pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                            fname.upc = "#" + numberUpc.Trim().ToLower();
                                            pmsk.sku = "#" + numberSku.Trim().ToLower();
                                            fname.sku = "#" + numberSku.Trim().ToLower();
                                        }
                                        else if (!string.IsNullOrEmpty(numberUpc) && string.IsNullOrEmpty(numberSku))
                                        {
                                            pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                            fname.upc = "#" + numberUpc.Trim().ToLower();
                                            pmsk.sku = "#" + numberUpc.Trim().ToLower();
                                            fname.sku = "#" + numberUpc.Trim().ToLower();
                                        }
                                        else if(string.IsNullOrEmpty(numberUpc) && !string.IsNullOrEmpty(numberSku))
                                        {
                                            pmsk.upc = "#" + numberSku.Trim().ToLower();
                                            fname.upc = "#" + numberSku.Trim().ToLower();
                                            pmsk.sku = "#" + numberSku.Trim().ToLower();
                                            fname.sku = "#" + numberSku.Trim().ToLower();
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
                                    pmsk.Tax = Tax;
                                    pmsk.StoreProductName = v.GetStringByIndex(2);
                                    pmsk.StoreDescription = pmsk.StoreProductName;
                                    fname.pname = pmsk.StoreProductName;
                                    fname.pdesc = pmsk.StoreProductName;
                                    pmsk.Price = v.GetDecimalByIndex(3);
                                    fname.Price = pmsk.Price;
                                    pmsk.Qty = v.GetIntByIndex(7);
                                    if(pmsk.Qty < 0)
                                    {
                                        pmsk.Qty = 0;
                                    }
                                    pmsk.pack = getpack(pmsk.StoreProductName);
                                    pmsk.uom = getVolume(pmsk.StoreProductName);
                                    fname.uom = pmsk.uom;
                                    if (pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        full.Add(fname);
                                    }
                                }
                                Console.WriteLine("Generating clsEbsPos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For clsEbsPos " + StoreId);
                                Console.WriteLine("Generating clsEbsPos " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For clsEbsPos " + StoreId);
                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.Message + StoreId);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid FileName or Raw Folder is Empty! " + StoreId);
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
        public int getpack(string prodName)
        {
            //prodName = prodName.ToUpper();
            var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)(pk|can)", RegexOptions.IgnoreCase);
            var prodPack = regexMatch.Groups["Result"].Value;
            if (prodPack.Length > 0)
            {
                return ParseIntValue(prodPack);
            }
            return 1;
        }
        public string getVolume(string prodName)
        {
            //prodName = prodName.ToUpper();
            var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)(ML|LTR|OZ|L)", RegexOptions.IgnoreCase);
            //var regexMatch = Regex.Match(prodName, @"(?<Result>\d+)ML| (?<Result>\d+)\sML| (?<Result>\d+)LTR|(?<Result>\d+)\sLTR| (?<Result>\d+)OZ | (?<Result>\d+)L | (?<Result>\d+)\sOZ");
            var prodPack = regexMatch.Groups["Result"].Value;
            if (prodPack.Length > 0)
            {
                return regexMatch.ToString();
            }
            if (regexMatch.Length > 6)
            {
                return "";
            }
            return "";
        }
        public int ParseIntValue(string val)
        {
            int outVal = 0;
            int.TryParse(val.Replace("$", ""), out outVal);
            return outVal;
        }
    }

    public class ProductsModelEBS
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public int Qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string uom { get; set; }
        public string StoreProductName { get; set; }
        public string StoreDescription { get; set; }
        public decimal Price { get; set; }
        public decimal sprice { get; set; }
        public decimal ClubPrice { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public decimal Tax { get; set; }
        public string altupc1 { get; set; }
        public string altupc2 { get; set; }
        public string altupc3 { get; set; }
        public string altupc4 { get; set; }
        public string altupc5 { get; set; }
        public decimal Deposit { get; set; }
        public int Discountable { get; set; }
        public string Vintage { get; set; }
    }
}
