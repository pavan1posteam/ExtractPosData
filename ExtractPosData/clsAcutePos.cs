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
    public class clsAcutePos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string Quantity = ConfigurationManager.AppSettings["Quantity"];
        string StaticQty = ConfigurationManager.AppSettings["StaticQty"];

        public clsAcutePos(int storeid, decimal tax)
        {
            try
            {
                AcutePosConvertRawFile(storeid, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

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
            return dtResult; // Returning DataTable
        }

        public string AcutePosConvertRawFile(int StoreId, decimal Tax)
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

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModelAcute> prodlist = new List<ProductsModelAcute>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();
                                if(StoreId == 11882)
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        ProductsModelAcute pmsk = new ProductsModelAcute();
                                        FullNameProductModel fname = new FullNameProductModel();
                                        Verify v = new Verify(dr, StoreId);
                                        pmsk.StoreID = StoreId;
                                        if (!string.IsNullOrEmpty(v.GetStringByIndex(1)))
                                        {
                                            var upc = v.GetStringByIndex(1).ToLower();
                                            var sku = v.GetStringByIndex(0).ToLower();
                                            string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                            string numberSku = Regex.Replace(sku, "[^0-9.]", "");
                                            if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                            {
                                                if (!string.IsNullOrEmpty(numberUpc))
                                                {
                                                    pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                    fname.upc = "#" + numberUpc.Trim().ToLower();
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
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        string qty = v.GetStringByIndex(3);
                                        pmsk.Qty = Convert.ToInt32(Convert.ToDecimal(qty));

                                        if (pmsk.Qty < 0)
                                        {
                                            pmsk.Qty = 0;
                                        }
                                        if (StaticQty.Contains(StoreId.ToString()))
                                            pmsk.Qty = 999;
                                        pmsk.StoreProductName = v.GetStringByIndex(2);
                                        pmsk.StoreDescription = v.GetStringByIndex(2);
                                        fname.pdesc = v.GetStringByIndex(2);
                                        fname.pname = v.GetStringByIndex(2);

                                        pmsk.Price = v.GetDecimalByIndex(5);
                                        fname.Price = v.GetDecimalByIndex(5);

                                        pmsk.sprice = 0;
                                        string numberPart = new string(v.GetStringByIndex(4).TakeWhile(char.IsDigit).ToArray()); //#35736
                                        pmsk.pack = int.TryParse(numberPart, out int result) ? result : 1;

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

                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        pmsk.Deposit = 0;
                                        fname.pcat = "";
                                        fname.pcat1 = "";
                                        fname.pcat2 = "";
                                        fname.uom = "";
                                        fname.region = "";
                                        fname.country = "";
                                        if (pmsk.Price > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        ProductsModelAcute pmsk = new ProductsModelAcute();
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
                                        if(StoreId == 12173)
                                        {
                                            pmsk.Qty = 999;
                                        }
                                        else if (StaticQty.Contains(StoreId.ToString()))
                                            pmsk.Qty = 999;
                                        else
                                        {
                                            string qty = dr["StockInHand"].ToString();
                                            pmsk.Qty = Convert.ToInt32(Convert.ToDecimal(qty));
                                        }

                                        pmsk.StoreProductName = dr["ProductName"]?.ToString();
                                        pmsk.StoreDescription = dr["ProductName"]?.ToString();
                                        fname.pdesc = dr["ProductName"]?.ToString();
                                        fname.pname = dr["ProductName"]?.ToString();
                                        fname.pdesc = dr["ProductName"]?.ToString();

                                        pmsk.Price = Convert.ToDecimal(dr["RetailPrice"]);
                                        fname.Price = Convert.ToDecimal(dr["RetailPrice"]);

                                        pmsk.sprice = 0;
                                        string numberPart = new string(dr["PackSize"]?.ToString().TakeWhile(char.IsDigit).ToArray()); //#35736
                                        pmsk.pack = int.TryParse(numberPart, out int result) ? result : 1;

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

                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        pmsk.Deposit = 0;
                                        fname.pcat = "";
                                        fname.pcat1 = "";
                                        fname.pcat2 = "";
                                        fname.uom = dr.Field<string>("PackSize");
                                        fname.region = "";
                                        fname.country = "";
                                        if (Quantity.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                        else if (pmsk.Price > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                }

                                Console.WriteLine("Generating AcutePos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For AcutePos " + StoreId);
                                Console.WriteLine("Generating AcutePos " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For AcutePos " + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for AcutePos " + StoreId;
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
    }

    public class ProductsModelAcute
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
}
