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
    public class SSCS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public SSCS(int StoreId, decimal Tax)
        {
            try
            {
                DiscountConvertRawFile(StoreId, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public static DataTable ConvertCsvToDataTable(string FileName,int StoreId)
        {
                      
                DataTable dtResult = new DataTable();
            dtResult.Columns.Add("PartId", typeof(string));
            dtResult.Columns.Add("Pack", typeof(string));
            dtResult.Columns.Add("Upc", typeof(string));
            dtResult.Columns.Add("SaleDate", typeof(string));
            dtResult.Columns.Add("Amount", typeof(string));
            dtResult.Columns.Add("ListPrice", typeof(string));
            dtResult.Columns.Add("MinqQty", typeof(string));
            dtResult.Columns.Add("Quantity", typeof(string));
            dtResult.Columns.Add("Description", typeof(string));

            var dataLines = File.ReadAllLines(FileName).ToList();
                dataLines.RemoveAll(r => r.Trim() == "");
                dataLines.RemoveAll(r => r.Substring(0, 8).All(a => !Char.IsDigit(a)));
                for (int i = 0; i < dataLines.Count; i++)
                {
                    dataLines[i] = Regex.Replace(dataLines[i], @"\s+", " ");
                }

                for (int i = 0; i < dataLines.Count; i++)
                {
                    var row = dtResult.NewRow();

                    var data = dataLines[i].Split(' ').ToList();
                    row[0] = data[0];
                    row[1] = data[1];
                    row[2] = data[2];
                    row[3] = data[3];
                    row[4] = data[4];

                    data.RemoveRange(0, 5);

                    row[5] = data[data.Count() - 3];
                    data.RemoveAt(data.Count() - 3);

                    row[6] = data[data.Count() - 2];
                    data.RemoveAt(data.Count() - 2);

                    row[7] = data[data.Count() - 1];
                    data.RemoveAt(data.Count() - 1);

                    row[8] = string.Join(" ", data);

                    dtResult.Rows.Add(row);
                }
            
            return dtResult; //Returning Dattable  
        }

        public string DiscountConvertRawFile(int StoreId, decimal Tax)
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
                               
                                DataTable dt = ConvertCsvToDataTable(Url,StoreId);
                                //dt.Columns.Add("Item number"); dt.Columns.Add("Normal description"); dt.Columns.Add("Vintage"); dt.Columns.Add("Item notes"); dt.Columns.Add("Size description");
                                //dt.Columns.Add("Long description"); dt.Columns.Add("Units per case"); dt.Columns.Add("Unit retail"); dt.Columns.Add("Pack retail"); dt.Columns.Add("Units per pack");

                                List<ProductMod> prodlist = new List<ProductMod>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();


                                foreach (DataRow dr in dt.Rows)
                                {

                                    try
                                    {
                                        ProductMod pmsk = new ProductMod();
                                        FullNameProductModel fname = new FullNameProductModel();

                                        pmsk.StoreID = StoreId;



                                      
                                            if (!string.IsNullOrEmpty(dr["Upc"].ToString()))
                                            {
                                                pmsk.upc = "#" + dr["Upc"].ToString();
                                                fname.upc = "#" + dr["Upc"].ToString();
                                                pmsk.sku = "#" + dr["Upc"].ToString();
                                                fname.sku = "#" + dr["Upc"].ToString();
                                            }
                                            else
                                            {
                                                continue;
                                            }


                                            var qty = "999";
                                            pmsk.Qty = Convert.ToInt32(qty);
                                            pmsk.StoreProductName = dr.Field<string>("Description");
                                            fname.pname = dr.Field<string>("Description");
                                            pmsk.StoreDescription = dr.Field<string>("Description").Trim();
                                            fname.pdesc = dr.Field<string>("Description");
                                            string price = dr.Field<string>("ListPrice");
                                            if (price != "" && price != "Text90")
                                            {
                                                pmsk.Price = System.Convert.ToDecimal(dr["ListPrice"].ToString());
                                                fname.Price = System.Convert.ToDecimal(dr["ListPrice"].ToString());
                                            }
                                            else
                                            {
                                                pmsk.Price = Convert.ToDecimal(null);
                                                fname.Price = Convert.ToDecimal(null);
                                            }
                                            pmsk.sprice = "";
                                            pmsk.pack = 1;
                                            fname.pack = 1;
                                            pmsk.tax = Tax;
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                            fname.pcat = "";
                                            fname.pcat1 = "";
                                            fname.pcat2 = "";
                                            fname.uom = "";
                                            fname.region = "";
                                            fname.country = "";
                                            if (pmsk.Qty > 0 && pmsk.Price > 0 && fname.pcat != "Other Tobacco Produc" && fname.pcat != "E-Cigs" && fname.pcat != "Cigarettes")
                                            {
                                                prodlist.Add(pmsk);
                                                prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                                full.Add(fname);
                                                full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                            }
                                        }

                                    
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }


                                Console.WriteLine("Generating Petrasoft Smart " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Petrasoft Smart " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Petrasoft Smart " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For Petrasoft Smart " + StoreId);

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
                                return "Not generated file for Petrasoft Smart " + StoreId;
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
