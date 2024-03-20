using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractPosData
{
   public class clsMaxtrix_POS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsMaxtrix_POS(int StoreId, decimal Tax)
        {
            try
            {
                MaxtrixConvertRawFile(StoreId, Tax);
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
                        }
                        finally 
                        { }
                    }
              
            }
           
            return dtResult; //Returning Dattable  
        }

        public string MaxtrixConvertRawFile(int StoreId, decimal Tax)
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
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    try
                                    {
                                        ProductModel pmsk = new ProductModel();
                                        FullNameProductModel fname = new FullNameProductModel();

                                        pmsk.StoreID = StoreId;
                                        if (!string.IsNullOrEmpty(dr["BAR_CODE"].ToString()))
                                        {
                                            pmsk.upc = "#" + dr["BAR_CODE"].ToString();
                                            fname.upc = "#" + dr["BAR_CODE"].ToString();
                                           
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        if (!string.IsNullOrEmpty(dr["ITEM"].ToString()))
                                        {
                                            pmsk.sku = "#" + dr["ITEM"].ToString();
                                            fname.sku = "#" + dr["ITEM"].ToString();
                                        }
                                     ///   var qty = "999";
                                        pmsk.Qty = Convert.ToInt32(dr["ON_HAND"]) > 0 ? Convert.ToInt32(dr["ON_HAND"]) : 0;
                                        pmsk.StoreProductName = dr.Field<string>("DESC");
                                        fname.pname = dr.Field<string>("DESC");
                                        pmsk.StoreDescription = dr.Field<string>("DESC").Trim();
                                        fname.pdesc = dr.Field<string>("DESC");
                                        string price = dr.Field<string>("RETAIL_PRICE");
                                        if (!string.IsNullOrEmpty(price))
                                        {
                                            pmsk.Price = System.Convert.ToDecimal(dr["RETAIL_PRICE"].ToString());
                                            fname.Price = System.Convert.ToDecimal(dr["RETAIL_PRICE"].ToString());
                                        }
                                        else {
                                            pmsk.Price = Convert.ToDecimal(0);
                                            fname.Price = Convert.ToDecimal(0);
                                        }
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        fname.pack = 1;
                                        pmsk.uom = dr.Field<string>("SIZE");
                                        fname.uom = dr.Field<string>("SIZE");
                                        fname.pack = 1;
                                        pmsk.Tax = Tax;
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        string cat = dr.Field<string>("DEPT");
                                        fname.pcat = "";
                                        fname.pcat1 = "";
                                        fname.pcat2 = "";
                                        
                                        fname.region = "";
                                        fname.country = "";
                                        if (pmsk.Price > 0 && cat != "CIG" && cat != "TOB")
                                        {
                                            prodlist.Add(pmsk);
                                          ///  prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            full.Add(fname);
                                            ///full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                        }
                                    }
                                    catch (Exception ex)
                                    { Console.WriteLine(ex.Message); }
                                }
                                Console.WriteLine("Generating MaxtrixPOS " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For MaxtrixPOS " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating MaxtrixPOS " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For MaxtrixPOS " + StoreId);

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
                                return "Not generated file for MaxtrixPOS " + StoreId;
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
