using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using System.IO;
using System.Configuration;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace ExtractPosData
{
    class MagStar
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public MagStar(int StoreId, decimal Tax)
        {
            try
            {
                MagStarConvertRawFile(StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertTextToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
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
        public string MagStarConvertRawFile(int StoreId, decimal Tax)
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
                                DataTable dt = ConvertTextToDataTable(Url);

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["Primary Upc"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["Primary Upc"].ToString();
                                        full.upc = "#" + dr["Primary Upc"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    decimal qty = Convert.ToDecimal(dr["Qty Avail"]);
                                    pmsk.Qty = Convert.ToInt32(qty);
                                    pmsk.sku = "#" + dr.Field<string>("Primary Upc");
                                    full.sku = "#" + dr.Field<string>("Primary Upc");
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Desc")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Desc");
                                        pmsk.StoreDescription = dr.Field<string>("Desc").Trim();
                                        full.pname = dr.Field<string>("Desc");
                                        full.pdesc = dr.Field<string>("Desc");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Retail Price"] == DBNull.Value ? 0 : dr["Retail Price"]);
                                    full.Price = System.Convert.ToDecimal(dr["Retail Price"] == DBNull.Value ? 0 : dr["Retail Price"]);
                                    //var PromoPrice = System.Convert.ToDecimal(dr["Promo Price"] == DBNull.Value ? 0 : dr["Promo Price"]);
                                    var ClubPrice = System.Convert.ToDecimal(dr["Club Prc"] == DBNull.Value ? 0 : dr["Club Prc"]);
                                    //if (PromoPrice != 0 && ClubPrice != 0)
                                    //{
                                    //    if (PromoPrice > 0)
                                    //    {
                                    //        pmsk.sprice = PromoPrice;
                                    //    }
                                    //    else if (ClubPrice > 0)
                                    //    {
                                    //        pmsk.sprice = ClubPrice;
                                    //    }
                                    //    else if (PromoPrice > 0 && ClubPrice > 0 && PromoPrice < ClubPrice)
                                    //    {
                                    //        pmsk.sprice = PromoPrice;
                                    //    }
                                    //    else if (PromoPrice > 0 && ClubPrice > 0 && ClubPrice < PromoPrice)
                                    //    {
                                    //        pmsk.sprice = ClubPrice;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    pmsk.sprice = 0;
                                    //}

                                    if (ClubPrice > 0)
                                    {
                                        pmsk.sprice = ClubPrice;
                                    }
                                    else
                                    {
                                        pmsk.sprice = 0;
                                    }
                                    pmsk.pack = 1;
                                    full.pack = 1;
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
                                    full.uom = "";
                                    full.pcat = dr["Dept Desc"].ToString();
                                    full.pcat1 = dr["SubDep Desc"].ToString();
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";
                                    if (!string.IsNullOrEmpty(dr["UPC2"].ToString()))
                                    {
                                        pmsk.altupc1 = "#" + dr["UPC2"].ToString();
                                    }
                                    else { pmsk.altupc1 = ""; }
                                    if (!string.IsNullOrEmpty(dr["UPC3"].ToString()))
                                    {
                                        pmsk.altupc2 = "#" + dr["UPC3"].ToString();
                                    }
                                    else { pmsk.altupc2 = ""; }
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if (pmsk.Qty > 0 && pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        fulllist.Add(full);
                                    }
                                }
                                Console.WriteLine("Generating MagStar " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating MagStar " + StoreId + " FullName CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For MagStar");
                                Console.WriteLine("Full Name File Generated For MagStar");

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
                                return "Not generated file for MagStar " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName";
                        }
                    }
                    else
                    {
                        Console.WriteLine("There is no file in the Raw Folder of " + StoreId);
                        return "";
                    }
                }
                else
                {
                    return "Invalid Sub-Directory";
                }
                }
                else
                {
                    return "Invalid Directory";
                }
                return "Completed generating File";
            }

        }
    }

