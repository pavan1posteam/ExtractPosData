using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Data;
using ExtractPosData.Model;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using ExtractPosData.Models;
namespace ExtractPosData
{
    class clsCREPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
       //private string StoreId;

        public clsCREPos(string Filename, int StoreId, decimal tax)
        {
            try
            {
                VisionRawFile(Filename, StoreId, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
            }
        }
        public DataTable ConvertTextToDataTable(string filename)
        {

            DataTable dt = new DataTable();

            var parser = new TextFieldParser(filename);
            bool headerRow = true;
            parser.SetDelimiters(",");
            while (!parser.EndOfData)
            {
                var currentRow = parser.ReadFields();
                if (headerRow)
                {
                    foreach (var field in currentRow)
                    {
                        dt.Columns.Add(field, typeof(object));
                    }
                    headerRow = false;
                }
                else
                {
                    dt.Rows.Add(currentRow);
                }
            }
            return dt;
        }
        public string VisionRawFile(string PosFileName, int StoreId, decimal tax)
        {
            baseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(baseUrl))
            {
                if (Directory.Exists(baseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(baseUrl + "/" + StoreId + "/Raw/");
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();

                        string Url = baseUrl + "/" + StoreId + "/Raw/" + myFile;
                        if (File.Exists(Url))
                        {
                            try
                            {
                                ProductsModel pdt = new ProductsModel();
                                List<ProductsModel> listProduct = new List<ProductsModel>();
                                FullnameModel full = new FullnameModel();
                                List<FullnameModel> listfull = new List<FullnameModel>();

                                DataTable dt = ConvertTextToDataTable(Url);
                                foreach (DataRow dr in dt.Rows)
                                {
                                    pdt = new ProductsModel();
                                    full = new FullnameModel();

                                    pdt.StoreID = StoreId;

                                    string upc = (dr["Upc"].ToString());
                                    if (upc == "")
                                    {
                                        pdt.upc = "";
                                        full.upc = "";

                                    }
                                    else
                                    {
                                        pdt.upc = "#" + (dr["Upc"].ToString());

                                        full.upc = "#" + (dr["Upc"].ToString());

                                    }

                                    string SKU = (dr["Sku"].ToString());

                                    if (SKU == "")
                                    {
                                        pdt.sku = "";

                                        full.sku = "";
                                    }
                                    else
                                    {
                                        pdt.sku = "#" + (dr["Sku"].ToString());

                                        full.sku = "#" + (dr["Sku"].ToString());
                                    }

                                    var x = dr["Sprice"].ToString();

                                    var m = x;
                                    if (m != "")
                                    {
                                        pdt.Price = Convert.ToDecimal(m);

                                        full.Price = Convert.ToDecimal(m);
                                    }
                                    else
                                    {
                                        pdt.Price = Convert.ToDecimal(null);
                                        full.Price = Convert.ToDecimal(null);
                                    }

                                    pdt.pack = 1;
                                    pdt.StoreProductName = dr["storeprodutname"].ToString();
                                    pdt.StoreDescription = dr["storedescription"].ToString();

                                    var y = dr["Qty"].ToString();


                                    if (y != "")
                                    {
                                        var Qtyy = Convert.ToDecimal(y);

                                        pdt.Qty = Convert.ToInt32(Qtyy);
                                    }
                                    else
                                    {
                                        pdt.Qty = Convert.ToInt32(null);
                                    }
                                    pdt.sprice = Convert.ToDecimal(null);
                                    pdt.Start = "";
                                    pdt.End = "";
                                    pdt.Tax = tax;

                                    pdt.altupc1 = "";
                                    pdt.altupc2 = "";
                                    pdt.altupc3 = "";
                                    pdt.altupc4 = "";
                                    pdt.altupc5 = "";

                                    full.pname = dr["storeprodutname"].ToString();
                                    full.pdesc = dr["storedescription"].ToString();

                                    full.uom = dr["size"].ToString();
                                    full.pcat = dr["dept"].ToString();
                                    full.pcat1 = dr["Subcatecory"].ToString();
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";

                                    if (pdt.upc != "" && pdt.Price >0)
                                    {
                                        listProduct.Add(pdt);
                                        listfull.Add(full);
                                    }
                                }
                                Console.WriteLine("Generating Product File For Store " + StoreId);
                                Console.WriteLine("Generating Fullname File For Store " + StoreId);
                                GenerateCSV.GenerateCSVFile(listProduct, "Product", StoreId, baseUrl);
                                GenerateCSV.GenerateCSVFile(listfull, "Fullname", StoreId, baseUrl);
                                Console.WriteLine("ProductFile is Generated Successfully For CREPOS Store " + StoreId);
                                Console.WriteLine("FullName File is Generated Successfully For CREPOS Store " + StoreId);

                                 
                                string[] filePaths = Directory.GetFiles(baseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                //(new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                //return "Not generated file for CREPOS " + StoreId;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName";
                        }
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
            return "Completed generating File For CREPOS" + StoreId;
        }
    }
}
