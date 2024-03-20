using ExcelDataReader;
using ExtractPosData.Models;
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
    class clsBeverages2u
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsBeverages2u(int storeid, decimal tax)
        {
            try
            {
                beverages2uConvertRawFile(storeid, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public string beverages2uConvertRawFile(int storeid, decimal tax)
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
                            DataTable dt = ConvertRawFileToDataTable(Url);

                            List<ProductModel> prodlist = new List<ProductModel>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                ProductModel product = new ProductModel();
                                product.StoreID = storeid;
                                var up = dr["upc"].ToString();
                                if (!string.IsNullOrEmpty(up))
                                {
                                    product.upc = "#" + up;
                                    product.sku = "#" + up;
                                }
                                else
                                {
                                    continue;
                                }
                                var name = dr["storeproductname"].ToString();
                                if (!string.IsNullOrEmpty(name))
                                {
                                    product.StoreProductName = name;
                                    product.StoreDescription = name;
                                }
                                else
                                {
                                    continue;
                                }
                                product.Qty = Convert.ToInt32(dr["qty"]) > 0 ? Convert.ToInt32(dr["qty"]) : 0;
                                product.pack = 1;
                                product.uom = "";
                                product.Tax = tax;
                                product.sprice = 0;
                                if (product.sprice > 0)
                                {
                                    product.Start = "";
                                    product.End = "";
                                }
                                string pr = dr["price"].ToString();
                                if (pr != "")
                                {
                                    product.Price = Convert.ToDecimal(pr);
                                }
                                else
                                {
                                    continue;
                                }
                                product.altupc5 = "";
                                product.altupc4 = "";
                                product.altupc3 = "";
                                product.altupc2 = "";
                                product.altupc1 = "";

                                if (product.Qty > 0 && product.Price > 0)
                                {
                                    prodlist.Add(product);
                                }
                            }

                            Console.WriteLine("Generating Beverages2u " + storeid + " Product CSV Files.....");

                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeid, BaseUrl);

                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + storeid + "/Raw/");

                            foreach (string filepath in filePaths)
                            {
                                string destpath = filepath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyymmddhhmmss"));
                                File.Move(filepath, destpath);
                            }
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine("" + ex.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + storeid + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                            return "Not generated file for POSnation " + storeid;
                        }
                    }
                }
            }
            return "Product File Generated For " + storeid;
        }
        public DataTable ConvertRawFileToDataTable(string filename)
        {
            DataTable dataTable = new DataTable();
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                    {
                        ConfigureDataTable = _ => new ExcelDataTableConfiguration
                        {
                            UseHeaderRow = true
                        }
                    });
                    dataTable = dataSet.Tables[0];
                }
            }
            return dataTable;
        }
    }
}