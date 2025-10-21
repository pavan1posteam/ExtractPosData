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
    class clsncrcounterpoint
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsncrcounterpoint(int storeid, decimal tax)
        {
            try
            {
                clsncrcounterpointConvertRawFile(storeid, tax);
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
            return dtResult;
        }

        public string clsncrcounterpointConvertRawFile(int StoreId, decimal Tax)
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
                                List<ProductModelncr> prodlist = new List<ProductModelncr>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();
                                if(StoreId == 12353)
                                {
                                    prodlist = new List<ProductModelncr>();
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        ProductModelncr pmsk = new ProductModelncr();
                                        Verify v = new Verify(dr, StoreId);
                                        pmsk.StoreID = StoreId;
                                        if (!string.IsNullOrEmpty(v.GetStringByIndex(1)))
                                        {
                                            pmsk.upc = "#" + v.GetStringByIndex(1);
                                            pmsk.sku = "#" + v.GetStringByIndex(0);
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.StoreProductName = v.GetStringByIndex(2);
                                        pmsk.StoreDescription = pmsk.StoreProductName;
                                        pmsk.Price = v.GetDecimalByIndex(4);
                                        pmsk.Qty = v.GetIntByIndex(6);
                                        pmsk.pack = "1";
                                        if (pmsk.Price > 0)
                                        {
                                            prodlist.Add(pmsk);
                                        }
                                    }
                                }
                                else
                                {
                                    prodlist = new List<ProductModelncr>();
                                    foreach (DataRow dr in dt.Rows)
                                    {
                                        if (dr["BARCOD"].ToString().Contains("---"))
                                        {
                                            continue;
                                        }
                                        ProductModelncr pmsk = new ProductModelncr();
                                        FullNameProductModel fname = new FullNameProductModel();
                                        Verify v = new Verify(StoreId, dr);

                                        pmsk.StoreID = StoreId;
                                        if (!string.IsNullOrEmpty(v.GetString("BARCOD")))
                                        {
                                            pmsk.upc = "#" + v.GetString("BARCOD");
                                            fname.upc = "#" + v.GetString("BARCOD");
                                            pmsk.sku = "#" + v.GetString("ITEM_NO");
                                            fname.sku = "#" + v.GetString("ITEM_NO");
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                        if (prodlist.Any(x => x.sku == pmsk.sku))
                                            continue;
                                        pmsk.StoreProductName = v.GetString("DESCR");
                                        pmsk.StoreDescription = v.GetString("DESCR");
                                        fname.pdesc = v.GetString("DESCR");
                                        fname.pname = v.GetString("DESCR");

                                        pmsk.Price = Convert.ToDecimal(v.GetString("PRC_1").Replace("$", ""));
                                        fname.Price = Convert.ToDecimal(v.GetString("PRC_1").Replace("$", ""));

                                        pmsk.pack = v.GetString("STK_UNIT");
                                        pmsk.Tax = Tax;
                                        string qq = dr["QTY_AVAIL"].ToString();
                                        pmsk.Qty = Convert.ToInt32(Convert.ToDecimal(qq));

                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        fname.pcat = v.GetString("CATEG_SUBCAT");
                                        fname.pcat1 = "";
                                        fname.pcat2 = "";
                                        fname.region = "";
                                        fname.country = "";
                                        if (pmsk.Price > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                    Console.WriteLine("Generating NCRCounterpoint " + StoreId + " Fullname CSV Files.....");
                                    string filename1 = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                    Console.WriteLine("Fullname File Generated For NCRCounterpoint " + StoreId);
                                }
                                
                                Console.WriteLine("Generating NCRCounterpoint " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For NCRCounterpoint " + StoreId);
                                
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
                                return "Not generated file for NCRCounterpoint " + StoreId;
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
    public class ProductModelncr
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public int Qty { get; set; }
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

    }
}
