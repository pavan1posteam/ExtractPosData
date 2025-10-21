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
    public class clsSUPERSONICPOS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        public clsSUPERSONICPOS(int storeId, decimal tax)
        {
            try
            {
                SUPERSONICPOSConvertRawFile(storeId, tax);
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

        public string SUPERSONICPOSConvertRawFile(int StoreId, decimal Tax)
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
                            List<SUPERSONICPOSModels.ProductModel> prodlist = new List<SUPERSONICPOSModels.ProductModel>();
                            List<SUPERSONICPOSModels.FullnameModel> fulllist = new List<SUPERSONICPOSModels.FullnameModel>();
                            try
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    SUPERSONICPOSModels.ProductModel prod = new SUPERSONICPOSModels.ProductModel();
                                    SUPERSONICPOSModels.FullnameModel fname = new SUPERSONICPOSModels.FullnameModel();

                                    prod.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["UPC/PLU"].ToString()))
                                    {
                                        prod.upc = "#" + dr["UPC/PLU"].ToString().Trim();
                                        fname.upc = "#" + dr["UPC/PLU"].ToString().Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    prod.sku = "#" + dr["Product ID"].ToString().Trim();
                                    fname.sku = "#" + dr["Product ID"].ToString().Trim();
                                    if (!string.IsNullOrEmpty(dr["In Stock"].ToString()))
                                    {
                                        prod.Qty = Convert.ToInt32(dr["In Stock"]);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (StoreId == 12137)
                                    {
                                        prod.Qty = 999;
                                    }
                                    prod.StoreProductName = dr["Name"].ToString();
                                    fname.pname = dr["Name"].ToString();
                                    prod.StoreDescription = dr["Name"].ToString();
                                    fname.pdesc = dr["Name"].ToString();

                                    string prc = dr["Retail Price"].ToString().Replace("$","");
                                    prod.Price = Convert.ToDecimal(prc);
                                    fname.Price = Convert.ToDecimal(prc);
                                    prod.sprice = 0;
                                    prod.uom = "";
                                    fname.uom = "";
                                    prod.pack = 1;
                                    fname.pack = 1;
                                    prod.Start = "";
                                    prod.End = "";
                                    prod.Tax = Tax;
                                    prod.altupc1 = "";
                                    prod.altupc2 = "";
                                    prod.altupc3 = "";
                                    prod.altupc4 = "";
                                    prod.altupc5 = "";
                                    fname.pcat = dr["Department"].ToString();
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.region = "";
                                    fname.country = "";


                                    if (prod.Price > 0)
                                    {
                                        prodlist.Add(prod);
                                        fulllist.Add(fname);

                                    }
                                }

                                if (prodlist.Count > 1)
                                {
                                    Console.WriteLine("Generating SUPERSONICPOS " + StoreId + " Product CSV Files.....");
                                    string pfilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    Console.WriteLine("Generating SUPERSONICPOS " + StoreId + " Full Name CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                }
                                Console.WriteLine("Product File Generated For SUPERSONICPOS " + StoreId);
                                Console.WriteLine("Full Name File Generated For SUPERSONICPOS " + StoreId);
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
                            return "Not Generated File for SUPERSONICPOS " + StoreId;
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

            return "Completed generating File For SUPERSONICPOS" + StoreId;
        }
    }


    public class SUPERSONICPOSModels
    {
        public class ProductModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public int Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
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
            public int pack { get; set; }
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string country { get; set; }
            public string region { get; set; }
        }
    }
}

