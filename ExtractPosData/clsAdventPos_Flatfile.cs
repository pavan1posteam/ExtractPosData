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
    class clsAdventPOS_flatfile
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsAdventPOS_flatfile(int StoreId, decimal Tax, int MarkUpValue)
        {
            try
            {
                AdventConvertRawFile(StoreId, Tax, MarkUpValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                    try
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    finally
                    { }
                }
            }
            return dtResult; //Returning Dattable  
        }
        public string AdventConvertRawFile(int StoreId, decimal Tax, int MarkUpValue)
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

                                List<ADVENTProductsModels> prodlist = new List<ADVENTProductsModels>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    try
                                    {
                                        ADVENTProductsModels pmsk = new ADVENTProductsModels();
                                        pmsk.StoreID = StoreId;
                                        if (!string.IsNullOrEmpty(dr["MainUPC"].ToString()))
                                        {
                                            pmsk.upc = "#" + dr["MainUPC"].ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        if (!string.IsNullOrEmpty(dr["MainUPC"].ToString()))
                                        {
                                            pmsk.sku = "#" + dr["MainUPC"].ToString();
                                        }
                                        pmsk.Qty = Convert.ToInt32(dr["TotalQty_Multi"]) > 0 ? Convert.ToInt32(dr["TotalQty_Multi"]) : 0;
                                        pmsk.StoreProductName = dr.Field<string>("ItemName");
                                        pmsk.StoreDescription = dr.Field<string>("ItemName").Trim();
                                        string price = dr.Field<string>("ItemPrice");
                                        if (!string.IsNullOrEmpty(price))
                                        {
                                            var Price = dr["ItemPrice"].ToString();
                                            decimal pric = Convert.ToDecimal(price);
                                            decimal P = (pric+pric/100*MarkUpValue);
                                            pmsk.Price = Math.Round(P, 2); 
                                        }
                                        else
                                        {
                                            pmsk.Price = Convert.ToDecimal(0);
                                        }
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        pmsk.uom = " ";
                                        pmsk.Tax = Tax;
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        if (pmsk.Price > 0)
                                        {
                                            prodlist.Add(pmsk);
                                            prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    catch (Exception ex)
                                    { Console.WriteLine(ex.Message); }
                                }
                                Console.WriteLine("Generating AdventPOSFlat " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For AdventPOSFlat  " + StoreId);
                                Console.WriteLine();

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
    public class ADVENTProductsModels
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
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
       
