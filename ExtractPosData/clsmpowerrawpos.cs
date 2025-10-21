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
    class clsmpowerrawpos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string StaticQty = ConfigurationManager.AppSettings["StaticQty"];
        string Quantity = ConfigurationManager.AppSettings["Quantity"];
        public clsmpowerrawpos(int storeid,decimal tax, string path, decimal Deposit)
        {
            try
            {
                MpowerRawPosConvertRawFile(storeid, tax, path, Deposit);
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
        public string MpowerRawPosConvertRawFile(int StoreId, decimal Tax, string path, decimal Deposit)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                path = string.IsNullOrEmpty(path) ? "Raw" : path;
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/"+path+"/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/" + path + "/");
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();

                        string Url = BaseUrl + "/" + StoreId + "/" + path + "/" + myFile;
                        if (File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = ConvertCsvToDataTable(Url);

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel1> prodlist = new List<ProductsModel1>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel1 pmsk = new ProductsModel1();
                                    Verify v = new Verify(dr, StoreId);

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(v.GetStringByIndex(1)))
                                    {
                                        var upc = v.GetStringByIndex(1);
                                        string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                        if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                        {
                                            if (!string.IsNullOrEmpty(numberUpc))
                                            {
                                                pmsk.upc = "#" + numberUpc;
                                                pmsk.sku = "#" + v.GetStringByIndex(3);
                                            }
                                            else
                                                continue;
                                        }
                                        else
                                            continue;
                                    }
                                    else
                                        continue;
                                    if (StaticQty.Contains(StoreId.ToString()))
                                        pmsk.Qty = 999;
                                    else
                                        pmsk.Qty = v.GetIntByIndex(2);
                                    pmsk.StoreProductName = v.GetStringByIndex(4);
                                    pmsk.StoreDescription = v.GetStringByIndex(5);
                                    pmsk.Price = Convert.ToDecimal(v.GetDecimalByIndex(7));
                                    string size = v.GetStringByIndex(6);
                                    pmsk.pack = v.getpack(size).ToString();
                                    pmsk.uom = v.getVolume(size);
                                    pmsk.sprice = Convert.ToDecimal(v.GetDecimalByIndex(8));
                                    pmsk.Tax = Tax;
                                    pmsk.Start = v.GetStringByIndex(9);
                                    pmsk.End = v.GetStringByIndex(10);
                                    pmsk.altupc1 =  v.GetStringByIndex(12);
                                    pmsk.altupc2 =  v.GetStringByIndex(13);
                                    pmsk.altupc3 =  v.GetStringByIndex(14);
                                    pmsk.altupc4 =  v.GetStringByIndex(15);
                                    pmsk.altupc5 =  v.GetStringByIndex(16);
                                    if (string.IsNullOrEmpty(Deposit.ToString()))
                                        pmsk.Deposit = 0;
                                    else
                                        pmsk.Deposit = Convert.ToDecimal(Deposit);

                                    if (Quantity.Contains(StoreId.ToString()) && pmsk.Qty > 0 && pmsk.Price > 0)
                                        prodlist.Add(pmsk);
                                    else if (pmsk.Price > 0)
                                        prodlist.Add(pmsk);
                                }
                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/" + path + "/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/" + path + "/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }

                                Console.WriteLine("Generating MPOWER " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For MPOWER " + StoreId);
                               
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for MPOWER " + StoreId;
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
    public class ProductsModel1
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
        public string sku { get; set; }
        public string pack { get; set; }
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
    class FullNameProductModel1
    {
        public string pname { get; set; }
        public string pdesc { get; set; }
        public string upc { get; set; }
        public string sku { get; set; }
        public decimal Price { get; set; }
        public string uom { get; set; }
        public string pack { get; set; }
        public string pcat { get; set; }
        public string pcat1 { get; set; }
        public string pcat2 { get; set; }
        public string country { get; set; }
        public string region { get; set; }
    }
}
