using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using System.Configuration;

namespace ExtractPosData
{
    class clsPOMODO
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        public clsPOMODO(int storeId, decimal tax)
        {
            try
            {
                POMODOConvertRawFile(storeId, tax);
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
            return dtResult; //Returning Dattable  
        }
        public string POMODOConvertRawFile(int storeid,decimal tax)
        {
            if(Directory.Exists(BaseUrl))
            {
                if(Directory.Exists(BaseUrl + "/" + storeid + "/RAW/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + storeid + "//Raw");
                    if(directory.GetFiles().FirstOrDefault() !=null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();
                        string Url = BaseUrl + "/" + storeid + "/Raw/" + myFile;
                        if(File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = ConvertCsvToDataTable(Url);
                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                              
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                  
                                    pmsk.StoreID = storeid;

                                    string upc = Regex.Replace(dr["ItemCode"].ToString(), @"[^0-9]+", "");
                                    if (!string.IsNullOrEmpty(upc))
                                    {
                                        pmsk.upc = '#' + upc.ToString();                                  
                                        pmsk.sku = '#' + upc.ToString();                                     
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                   
                                    decimal qty = Convert.ToDecimal(dr["Qty on Hand"]);

                                    pmsk.Qty = Convert.ToInt32(qty);

                                    pmsk.StoreProductName = dr.Field<string>("Description");                               
                                    pmsk.StoreDescription = dr.Field<string>("Description");
                                    var x = dr["Price"].ToString().Replace("$", "");

                                    if(x != "")
                                    {
                                        var prc = Convert.ToDecimal(x);
                                        pmsk.Price = Convert.ToDecimal(prc);
                                    }
                                    pmsk.sprice = System.Convert.ToDecimal(null);
                                    if (pmsk.sprice > 0)
                                    {
                                        pmsk.Start = DateTime.Now.ToString("mm/dd/yyyy");
                                        pmsk.End = DateTime.Now.AddDays(1).ToString("mm/dd/yyyy");
                                    }
                                    else
                                    {
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                    }
                                    if (storeid == 11296)
                                    {
                                        var uom = dr.Field<string>("PKG Type");
                                        pmsk.uom = uom;
                                        var pack = dr["PKG Type"];
                                        if (uom.Equals("Each")|| uom.Equals("Cigar") || uom.Equals("Carton (Cigs)") || uom.Equals("Carton(Cigs)"))
                                        {
                                            pmsk.pack = 1;
                                        }
                                        else
                                        {
                                            pmsk.pack = Convert.ToInt32(uom.ToString().Split('-',' ').First());
                                        }
                                    }
                                    else
                                    {
                                        pmsk.pack = 1;
                                        pmsk.uom = dr.Field<string>("Each Container");
                                    }
                                    pmsk.Tax = tax;
                                                                     
                                    if (pmsk.Price > 0 && pmsk.Qty > 0)
                                    {
                                        prodlist.Add(pmsk);
                                    }
                                }
                                Console.WriteLine("Generating POMODOPos " + storeid + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeid, BaseUrl);
                                Console.WriteLine("Product File Generated For POMODOPos " + storeid);
                                Console.WriteLine();
                                Console.WriteLine("Generating POMODOPos " + storeid + " Fullname CSV Files.....");
                               // filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", storeid, BaseUrl);
                                Console.WriteLine("Fullname File Generated For POMODOPos " + storeid);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + storeid + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + storeid + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                                return "Not generated file for POMODO Pos " + storeid;
                            }  
                        }
                        else
                        {
                            return "Ínvalid FileName" + storeid;
                        }
                    }
                    else
                    {
                        return "Invalid Sub-Directory" + storeid;
                    }
                }
            }
            else
            {
                return "Invalid Directory" + storeid;
            }
            return "Completed generating File For POMODOPos" + storeid;
        }
    }
}
