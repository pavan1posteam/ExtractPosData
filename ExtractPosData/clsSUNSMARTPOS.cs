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
    public class clsSUNSMARTPOS
    {

        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        public clsSUNSMARTPOS(int storeid, decimal tax)
        {
            try
            {
                SunsmartPOSConvertRawFile(storeid, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }


        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            DataTable dt = new DataTable();
            try
            {
                using (TextFieldParser parser = new TextFieldParser(FileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.Delimiters = new string[] { "," };
                    parser.HasFieldsEnclosedInQuotes = true;

                    string[] columns = parser.ReadFields();

                    for (int i = 0; i < columns.Length; i++)
                    {
                        dt.Columns.Add(columns[i], typeof(string));
                    }
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        DataRow newrow = dt.NewRow();

                        for (int i = 0; i < fields.Length; i++)
                        {
                            if (dt.Columns.Count != fields.Length)
                            {
                                break;
                            }
                            newrow[i] = fields[i];
                        }
                        dt.Rows.Add(newrow);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt; //Returning Dattable  
        }


        public string SunsmartPOSConvertRawFile(int StoreId, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    var myFile = (from f in directory.GetFiles()
                                  orderby f.LastWriteTime descending
                                  select f).First();

                    string Url = BaseUrl + "/" + StoreId + "/Raw/" + myFile;
                    if (File.Exists(Url))
                    {
                        try
                        {
                            DataTable dt = ConvertCsvToDataTable(Url);

                            var dtr = from s in dt.AsEnumerable() select s;
                            List<SunsmartModels.ProductModel> prodlist = new List<SunsmartModels.ProductModel>();
                            List<SunsmartModels.FullnameModel> fulllist = new List<SunsmartModels.FullnameModel>();
                            try
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    SunsmartModels.ProductModel prod = new SunsmartModels.ProductModel();
                                    SunsmartModels.FullnameModel fname = new SunsmartModels.FullnameModel();

                                    prod.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        prod.upc = "#" + dr["UPC"].ToString().Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    prod.sku = "#" + dr["UPC"].ToString().Trim();
                                    prod.Qty = Convert.ToInt32(dr["QTY"]);
                                    prod.StoreProductName = dr["DESCRIPTION"].ToString();
                                    prod.StoreDescription = dr["DESCRIPTION"].ToString();
                                    prod.Price = Convert.ToDecimal(dr["price"]);
                                    prod.sprice = 0;
                                    prod.uom = dr["SIZE"].ToString();
                                    prod.pack = dr["PACK"].ToString();
                                    prod.Start = "";
                                    prod.End = "";
                                    prod.Tax = Tax;
                                    prod.altupc1 = "";
                                    prod.altupc2 = "";
                                    prod.altupc3 = "";
                                    prod.altupc4 = "";
                                    prod.altupc5 = "";


                                    if (StoreId != 12093)
                                    {
                                        fname.upc = "#" + dr["UPC"].ToString().Trim();
                                        fname.sku = "#" + dr["UPC"].ToString().Trim();
                                        fname.pname = dr["DESCRIPTION"].ToString();
                                        fname.pdesc = dr["DESCRIPTION"].ToString();
                                        fname.uom = dr["SIZE"].ToString();
                                        fname.Price = Convert.ToDecimal(dr["price"]);
                                        fname.pack = dr["PACK"].ToString();
                                        fname.upc = "#" + dr["UPC"].ToString().Trim();
                                        fname.pcat = dr["DEPT"].ToString();
                                        fname.pcat1 = "";
                                        fname.pcat2 = "";
                                        fname.region = "";
                                        fname.country = "";
                                    }


                                    if (prod.Price > 0)
                                    {
                                        prodlist.Add(prod);

                                        if (StoreId != 12093)
                                        {
                                            fulllist.Add(fname);
                                        }

                                    }
                                }


                                if (prodlist.Count > 1 )
                                {
                                    Console.WriteLine("Generating SunSmart " + StoreId + " Product CSV Files.....");
                                    string pfilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                }
                                if (fulllist.Count > 1)
                                {

                                    Console.WriteLine("Generating SunSmart " + StoreId + " Full Name CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                }
                                Console.WriteLine("Product File Generated For SunSmartPOS " + StoreId);
                                Console.WriteLine("Full Name File Generated For SunSmartPOS " + StoreId);
                            }



                            catch (Exception ex)
                            {
                                Console.WriteLine("" + ex.Message);


                            }

                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");
                            foreach (string filePath in filePaths)
                            {
                                string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                File.Move(filePath, destpath);
                            }


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                            return "Not Generated File for Vision " + StoreId;
                        }
                    }
                    else
                    {
                        return "Ínvalid FileName" + StoreId;
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

            return "Completed generating File For SunSmartPOS" + StoreId;
        }
    }


    public class SunsmartModels
    {
        public class ProductModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public int Qty { get; set; }
            public string sku { get; set; }
            public string pack { get; set; }
            public string uom { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public decimal sprice { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public decimal Tax { get; set; }
            public string altupc1 { get; set; }
            public string altupc2 { get; set; }
            public string altupc3 { get; set; }
            public string altupc4 { get; set; }
            public string altupc5 { get; set; }

        }

        public class FullnameModel
        {
            public string pname { get; set; }
            public string pdesc { get; set; }
            public string upc { get; set; }
            public string sku { get; set; }
            public decimal Price { get; set; }
            public string uom { get; set; }
            public string pack { get; set; }
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string country { get; set; }
            public string region { get; set; }
        }
    }
}