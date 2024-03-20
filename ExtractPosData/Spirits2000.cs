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
    public class clsSpirits2000
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsSpirits2000(int StoreId, decimal Tax)
        {
            try
            {
                clsSpirits2000ConvertRawFile(StoreId, Tax);
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
                            ////if (rows.Count() > 26)
                            ////{ break; }
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
            }
            catch (Exception)
            {
            }

            return dtResult; //Returning Dattable  
        }
        public string clsSpirits2000ConvertRawFile(int StoreId, decimal Tax)
        {
            //DataTable dt = new DataTable();
            DataTable invDt = new DataTable();
            DataTable prcDt = new DataTable();
            DataTable stkDt = new DataTable();
            DataTable upcDt = new DataTable();
            //DataTable finaldt = new DataTable();

            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    string[] filePathss = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");
                    
                    if (filePathss.Length >= 4)
                    {
                        foreach (var itm in filePathss)
                        {
                                string Url = itm;
                                //dt= ConvertCsvToDataTable(Url);
                                if (Url.Contains("inv"))
                                    invDt = ConvertCsvToDataTable(Url);
                                else if (Url.Contains("prc"))
                                    prcDt = ConvertCsvToDataTable(Url);
                                else if (Url.Contains("stk"))
                                    stkDt = ConvertCsvToDataTable(Url);
                                else if (Url.Contains("upc"))
                                    upcDt = ConvertCsvToDataTable(Url);
                         }
                        try
                        {
                            
                            var productquery = (from DataRow inv in invDt.Rows
                                                join DataRow prc in prcDt.Rows on inv.Field<string>("sku") equals prc.Field<string>("sku")
                                                join DataRow stk in stkDt.Rows on inv.Field<string>("sku") equals stk.Field<string>("sku")
                                                join DataRow upc in upcDt.Rows on inv.Field<string>("sku") equals upc.Field<string>("sku")
                                                select new ProductModel
                                                {
                                                    StoreID = StoreId,
                                                    upc = '#' + upc.Field<string>("UPC"),
                                                    Qty = Convert.ToDecimal(stk.Field<string>("BACK")) + Convert.ToDecimal(stk.Field<string>("FLOOR")),
                                                    sku = '#' + inv.Field<string>("SKU"),
                                                    pack = 1,
                                                    //pack = Convert.ToInt32(inv.Field<string>("PACK")),
                                                    StoreProductName = inv.Field<string>("NAME"),
                                                    StoreDescription = inv.Field<string>("NAME"),
                                                    Price = Convert.ToDecimal(prc.Field<string>("PRICE")),
                                                    sprice = 0,
                                                    Tax = Tax,
                                                    uom = inv.Field<string>("SNAME").ToString().Trim() != "N/A" ? inv.Field<string>("SNAME").ToString() : "",
                                                    Start = "",
                                                    End = "",
                                                    altupc1 = "",
                                                    altupc2 = "",
                                                    altupc3 = "",
                                                    altupc4 = "",
                                                    altupc5 = ""
                                                }).ToList();
                            
                            productquery.RemoveAll(x => Regex.Match(x.upc, @"[A-Z]").Success);
                           
                            //fullquery.RemoveAll(x => Regex.Match(x.upc, @"[A-Z]").Success);
                            Console.WriteLine("Generating Spirits2000 " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(productquery, "PRODUCT", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For Spirits2000 " + StoreId);


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
                            return "Not generated file for Spirits2000 " + StoreId;
                        }
                        //}
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

        private void ToList()
        {
            throw new NotImplementedException();
        }
    }
}
