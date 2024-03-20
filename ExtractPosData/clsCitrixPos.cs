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
    class clsCitrixPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsCitrixPos(int StoreId, decimal tax)
        {
            try
            {
                CitrixConvertRawFile(StoreId, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertCsvToDataTable(string FileName)
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
                            var roww = row.Replace('"', ' ').Trim();

                            dtResult.Rows[r][c] = roww.ToString();
                            c++;
                        }

                        r++;
                    }
                    i++;
                }
            }
            dtResult = dtResult.AsEnumerable().Skip(6).CopyToDataTable();
            return dtResult; //Returning Dattable  
        }

        public string CitrixConvertRawFile(int StoreId, decimal tax)
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
                                DataTable dt = ConvertCsvToDataTable(Url);

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<CitrixProductModel> prodlist = new List<CitrixProductModel>();
                                List<CitrixFullnameModel> full = new List<CitrixFullnameModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    try
                                    {
                                        CitrixProductModel pmsk = new CitrixProductModel();
                                        CitrixFullnameModel fname = new CitrixFullnameModel();

                                        pmsk.StoreID = StoreId;

                                        string upc = Regex.Replace(dr["Column12"].ToString(), @"[^0-9]+", "");
                                        if (!string.IsNullOrEmpty(upc))
                                        {
                                            pmsk.upc = "#" + upc.ToString();
                                            fname.upc = "#" + upc.ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                        string SKU = Regex.Replace(dr["Drs Orders"].ToString(), @"[^0-9]+", "");
                                        if (!string.IsNullOrEmpty(SKU))
                                        {
                                            pmsk.sku = "#" + SKU.ToString();
                                            fname.sku = "#" + SKU.ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        String qty = Regex.Replace(dr["Column8"].ToString(), @"[^0-9]+", "");
                                        if (StoreId == 10873)
                                        {
                                            if (!string.IsNullOrEmpty(qty))
                                            {
                                                var qtyy = Convert.ToInt32(qty);
                                                pmsk.Qty = System.Convert.ToInt32(qtyy / 1000);
                                            }
                                        }
                                        else if (!string.IsNullOrEmpty(qty))
                                        {
                                            var qtyy = Convert.ToInt32(qty);
                                            pmsk.Qty = System.Convert.ToInt32(qtyy/1000);
                                        }
                                        // pmsk.Qty = System.Convert.ToInt32(dr["Column8"]);

                                        pmsk.StoreProductName = dr.Field<string>("Column1").Replace("=", "");
                                        fname.pname = dr.Field<string>("Column1").Replace("=", "");
                                        pmsk.StoreDescription = dr.Field<string>("Column1").Trim().Replace("=", "");
                                        fname.pdesc = dr.Field<string>("Column1").Replace("=", "");
                                        pmsk.Price = System.Convert.ToDecimal(dr["Column9"]);
                                        fname.Price = System.Convert.ToDecimal(dr["Column9"]);
                                        pmsk.sprice = System.Convert.ToDecimal(null);

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

                                        pmsk.Tax = tax;

                                        fname.pcat1 = "";
                                        fname.pcat2 = "";
                                        fname.region = "";
                                        fname.country = "";
                                        String pck = Regex.Replace(dr["Column7"].ToString(), @"[^0-9]+", "");

                                        if (StoreId == 10872)
                                        {
                                            pmsk.pack = System.Convert.ToInt32(pck);
                                            pmsk.pack = (pmsk.pack / 1000);
                                            fname.pack = System.Convert.ToInt32(pck);
                                            fname.pack = (fname.pack / 1000);
                                        }
                                        else
                                        {
                                            pmsk.pack = System.Convert.ToInt32(pck);
                                            pmsk.pack = (pmsk.pack / 1000);
                                            fname.pack = System.Convert.ToInt32(pck);
                                            fname.pack = (fname.pack / 1000);
                                        }
                                        if (pmsk.Price > 0 && (pmsk.Qty > 0 && pmsk.Qty <= 999))
                                        {
                                            prodlist.Add(pmsk);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            full.Add(fname);
                                            full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }
                                Console.WriteLine("Generating CitrixPos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For CitrixPos " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating CitrixPos " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For CitrixPos " + StoreId);

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
                                return "Not generated file for CitrixPos " + StoreId;
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
            }
            else
            {
                return "Invalid Directory" + StoreId;
            }
                return "Completed generating File For CitrixPos" + StoreId;
            }
        }

    public class CitrixProductModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public Int32 Qty { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
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
    public class CitrixFullnameModel
    {
        public string pname { get; set; }
        public string pdesc { get; set; }
        public string upc { get; set; }
        public string sku { get; set; }
        public decimal Price { get; set; }
        public string uom { get; set; }
        public Int32 pack { get; set; }
        public string pcat { get; set; }
        public string pcat1 { get; set; }
        public string pcat2 { get; set; }
        public string country { get; set; }
        public string region { get; set; }
    }
 }
