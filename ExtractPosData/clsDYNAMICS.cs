using ExcelDataReader;
using ExtractPosData.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace ExtractPosData
{
    public class clsDYNAMICS
    {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsDYNAMICS(int storeId, decimal tax)
        {

            try
            {
                dynamicsConvertRawFile(storeId, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        public static DataTable ConvertExceltoDatatable(string filename)
        {
            DataTable dt = new DataTable();
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    });
                    dt = dataset.Tables[0];
                }
            }
            return dt;
        }

        public string dynamicsConvertRawFile(int storeid, decimal tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + storeid + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + storeid + "/Raw/");
                    var myFile = (from f in directory.GetFiles()
                                  orderby f.LastWriteTime descending
                                  select f).First();

                    string Url = BaseUrl + "/" + storeid + "/Raw/" + myFile;
                    if (File.Exists(Url))
                    {
                        try
                        {
                            DataTable dt = ConvertExceltoDatatable(Url);
                            List<ProductsModel> prodlist = new List<ProductsModel>();
                            List<FullNameProductModel> fulllist = new List<FullNameProductModel>();
                            string currentCategory = null;

                            foreach (DataRow dr in dt.Rows)
                            {

                                if (!string.IsNullOrEmpty(dr["Category"].ToString()))
                                {
                                    currentCategory = dr["Category"].ToString();
                                }
                                else if (currentCategory != null)
                                {
                                    dr["Category"] = currentCategory;
                                }

                                ProductsModel prod = new ProductsModel();
                                FullNameProductModel fdf = new FullNameProductModel();

                                prod.StoreID = storeid; ;
                                if (!string.IsNullOrEmpty(dr["Column1"].ToString()))
                                {
                                    prod.upc = "#" + dr["Column1"].ToString();
                                }
                                else
                                {
                                    continue;
                                }
                                prod.Qty = Convert.ToInt32(999);
                                prod.sku = "#" + dr["Column1"].ToString();
                                fdf.upc = "#" + dr["Column1"].ToString();
                                fdf.sku = "#" + dr["Column1"].ToString();
                                prod.pack = 1;
                                fdf.pack = 1;
                                prod.uom = "";
                                fdf.uom = "";
                                prod.StoreProductName = dr["Item"].ToString();
                                fdf.pname = dr["Item"].ToString();
                                prod.StoreDescription = dr["Item"].ToString();
                                fdf.pdesc = dr["Item"].ToString();
                                prod.Price = Convert.ToDecimal(dr["Price"]);
                                fdf.Price = prod.Price;
                                prod.sprice = 0;
                                prod.Start = "";
                                prod.End = "";
                                prod.altupc1 = "";
                                prod.altupc2 = "";
                                prod.altupc3 = "";
                                prod.altupc4 = "";
                                prod.altupc5 = "";
                                fdf.pcat = currentCategory.ToString();
                                fdf.region = "";
                                fdf.pcat1 = "";
                                fdf.pcat2 = "";
                                fdf.country = "";

                                if (prod.Price > 0 && prod.Qty > 0)
                                {
                                    prodlist.Add(prod);
                                    fulllist.Add(fdf);
                                }


                            }
                            Console.WriteLine("Generating DYNAMICS " + storeid + " Product CSV Files.....");
                            Console.WriteLine("Generating DYNAMICS " + storeid + " Fullname CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeid, BaseUrl);
                            string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", storeid, BaseUrl);
                            Console.WriteLine("Product File Generated For DYNAMICS" + storeid);
                            Console.WriteLine("Fullname File Generated For DYNAMICS" + storeid);
                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + storeid + "/Raw/");

                            foreach (string filePath in filePaths)
                            {
                                GC.Collect();
                                string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                File.Move(filePath, destpath);
                            }

                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + storeid + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                            return "Not generated file for DYNAMICS " + storeid;
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

            else
            {
                return "Invalid Directory" + storeid;
            }
            return "Completed generating File For DYNAMICS" + storeid;
        }
    }
}