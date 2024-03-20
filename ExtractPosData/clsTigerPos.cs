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
                                DataTable dt = ConvertCsvToDataTable(Url);

                                List<ProductModel> prodlist = new List<ProductModel>();
                                List<FullNameProductModel> fullnamelist = new List<FullNameProductModel>();

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
       
