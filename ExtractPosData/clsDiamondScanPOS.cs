using ExtractPosData.Model;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;

namespace ExtractPosData
{
    public class clsDiamondScanPOS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsDiamondScanPOS(int StoreID, decimal Tax)
        {

            try
            {
                DiamondScanConvertRawFile(StoreID, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }

        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            try
            {
                using (StreamReader sr = new StreamReader(FileName))
                {
                    string line;
                    bool headersSet = false;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] fields = line.Split('|');
                        if (!headersSet)
                        {
                            foreach (string field in fields)
                            {
                                dtResult.Columns.Add(field.Trim());
                            }
                            headersSet = true;
                        }
                        else
                        {
                            DataRow row = dtResult.Rows.Add();
                            for (int i = 0; i < fields.Length; i++)
                            {
                                row[i] = fields[i].Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dtResult;
        }
        public string DiamondScanConvertRawFile(int storeID, decimal tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            List<clsProductModel> prodlist = new List<clsProductModel>();
            List<FullNameProductModel> fullnamelist = new List<FullNameProductModel>();
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + storeID + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + storeID + "/Raw/");
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();
                        string Url = BaseUrl + "/" + storeID + "/Raw/" + myFile;



                        if (File.Exists(Url))
                        {
                            try
                            {

                                DataTable dt = ConvertCsvToDataTable(Url);

                                foreach (DataRow dr in dt.Rows)
                                {
                                    clsProductModel pmsk = new clsProductModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = storeID;

                                    if (dr["upcCode"].ToString().Contains("--------"))
                                    {
                                        continue;
                                    }
                                    if (!string.IsNullOrEmpty(dr["primaryUpc"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr.Field<string>("primaryUpc").ToString();
                                        full.upc = "#" + dr.Field<string>("primaryUpc").ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    //string qtyOnHand = dr.Field<string>("qtyOnHand");
                                    //if (!string.IsNullOrEmpty(qtyOnHand) && qtyOnHand != "Null")
                                    //{
                                    //    if (int.TryParse(qtyOnHand, out int qty))
                                    //    {
                                    //        pmsk.Qty = qty;
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    continue;
                                    //}

                                    string qtyOnHand = dr.Field<string>("qtyOnHand");
                                    if (!string.IsNullOrEmpty(qtyOnHand) && qtyOnHand.ToLower() != "null")
                                    {
                                        pmsk.Qty = Convert.ToInt32(Convert.ToDecimal(dr.Field<string>("qtyOnHand")));
                                    }
                                    if (!string.IsNullOrEmpty(dr["upcCode"].ToString()))
                                    {
                                        pmsk.sku = "#" + dr.Field<string>("upcCode").ToString();
                                        full.sku = "#" + dr.Field<string>("upcCode").ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    pmsk.pack = 1;
                                    full.pack = 1;
                                    pmsk.StoreProductName = dr.Field<string>("description").ToString();
                                    pmsk.StoreDescription = dr.Field<string>("description").ToString();
                                    full.pname = dr.Field<string>("description").ToString();
                                    full.pdesc = dr.Field<string>("description").ToString();
                                    full.pcat = dr.Field<string>("departmentName").ToString();
                                    full.pcat1 = "";
                                    full.pcat2 = "";
                                    full.region = "";
                                    full.country = "";

                                    pmsk.Price = Convert.ToDecimal(dr.Field<string>("retail1"));
                                    full.Price = Convert.ToDecimal(dr.Field<string>("retail1"));
                                    pmsk.sprice = 0;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    pmsk.tax = tax;


                                    if (pmsk.Qty > 0 && pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        fullnamelist.Add(full);

                                    }

                                }

                                Console.WriteLine("Generating DiamondScanPOS " + storeID + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeID, BaseUrl);
                                Console.WriteLine("Product File Generated For DiamondScanPOS " + storeID);
                                Console.WriteLine();
                                Console.WriteLine("Generating DiamondScanPOS " + storeID + " FullNameFile CSV Files.....");
                                string fullfilename = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", storeID, BaseUrl);
                                Console.WriteLine("Fullname File Generated For DiamondScanPOS " + storeID);


                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + storeID + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        else
                        {
                            return "Invalid FileName" + storeID;
                        }
                    }
                    else
                    {
                        return "Invalid Sub-Directory" + storeID;
                    }
                }
                else
                {
                    return "Invalid Directory" + storeID;
                }
            }
            return "Completed generating File For DiamondScanPOS" + storeID;

        }
    }
}
