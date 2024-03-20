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
    class clsRetaillEdge
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsRetaillEdge(int StoreId, decimal Tax)
        {
            try
            {
                RetailEdgeConvertRawFile(StoreId, Tax);
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
            return dtResult; //Returning datatable 
        }
        public string RetailEdgeConvertRawFile(int StoreId, decimal Tax)
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

                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["Inv_Sku"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["Inv_Sku"].ToString().Replace("-", "");
                                        full.upc = "#" + dr["Inv_Sku"].ToString().Replace("-", "");

                                    }
                                    else
                                    {
                                        pmsk.upc = '#' + Convert.ToDouble(dr["UPC"]).ToString().Replace("-", "");

                                    }
                                    decimal qty = Convert.ToDecimal(dr["Inv_Quan"]);
                                    pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                    pmsk.sku = "#" + dr.Field<string>("Inv_ItemID");
                                    full.sku = "#" + dr.Field<string>("Inv_ItemID");
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Inv_Desc")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Inv_Desc").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("Inv_Desc").Trim();
                                        full.pname = dr.Field<string>("Inv_Desc").Trim();
                                        full.pdesc = dr.Field<string>("Inv_Desc").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Inv_Loc_Price_Was"] == DBNull.Value ? 0 : dr["Inv_Loc_Price_Was"]);
                                    full.Price = System.Convert.ToDecimal(dr["Inv_Loc_Price_Was"] == DBNull.Value ? 0 : dr["Inv_Loc_Price_Was"]);
                                    if (pmsk.Price <= 0 || full.Price <= 0)
                                    {
                                        continue;
                                    }
                                    full.pack = 1;
                                    full.pcat = dr["Inv_DepartID"].ToString();
                                    full.pcat1 = "";
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";
                                    pmsk.sprice = 0;
                                    pmsk.pack = 1;
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
                                    if (full.pcat.ToUpper() != "TOBACCO" && full.pcat.ToUpper() != "CIGAR" && full.pcat.ToUpper() != "CIGARETTE" && full.pcat.ToUpper() != "MIX" && full.pcat.ToUpper() != "NON-ALCH DRINKS" && full.pcat.ToUpper() != "READY TO DRINK" && full.pcat.ToUpper() != "GIFT ACC." && full.pcat.ToUpper() != "88" && pmsk.Price > 0 && pmsk.Qty > 0)
                                    {
                                        fulllist.Add(full);
                                        prodlist.Add(pmsk);
                                    }
                                }
                                Console.WriteLine("Generating REATILEDGE " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating REATILEDGE " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For REATILEDGE" + StoreId);
                                Console.WriteLine("Fullname File Generated For REATILEDGE" + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    GC.Collect();
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for RETAILEDGE " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName" + StoreId;
                        }
                    }
                }
                else
                {
                    return "Invalid Sub-Directory" + StoreId;
                }
            }
            else
            {
                return "Invalid Directory" + StoreId;
            }
            return "Completed generating File For RETAILEDGE" + StoreId;
        }
    }
}
