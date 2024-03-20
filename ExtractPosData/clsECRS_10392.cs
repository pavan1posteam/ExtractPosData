using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
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
    class clsECRS_10392
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsECRS_10392(int StoreId, decimal Tax)
        {
                try
                {
                    ECRSConvertRawFile(StoreId, Tax);
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
                            var coll = col.Replace("'", "");
                                  dtResult.Columns.Add(coll);
                            }
                        }
                        else
                        {
                            string[] rows = parser.ReadFields();
                            dtResult.Rows.Add();
                            int c = 0;
                            foreach (string row in rows)
                            {
                            var col1 = row.Replace("'", "");
                            var roww = col1.Replace('"', ' ').Trim();

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
            public string ECRSConvertRawFile(int StoreId, decimal Tax)
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

                                var col = dt.Columns[0].DataType;
                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProdModel> prodlist = new List<ProdModel>();
                                List<FullnameModel> fullnamelist = new List<FullnameModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProdModel pmsk = new ProdModel();
                                    FullnameModel full = new FullnameModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["ItemiD"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["ItemiD"];
                                        full.upc = "#" + dr["ItemiD"];
                                        pmsk.sku = "#" + dr["ItemiD"];
                                        full.sku = "#" + dr["ItemiD"];
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    full.pcat = dr["department"].ToString();
                                    full.pcat1 = dr["subDepartment"].ToString();
                                    full.pcat2 = "";
                                    full.uom = dr["size"].ToString();
                                    full.country = "";
                                    full.region = "";
                                    var qty = dr["onHand"].ToString();
                                    if (!string.IsNullOrEmpty(qty))
                                    {
                                        var qty1 = Convert.ToDecimal(qty);
                                        pmsk.Qty = Convert.ToInt32(qty1);
                                    }
                                    else { continue; }
                                    if (!string.IsNullOrEmpty(dr["itemName"].ToString()))
                                    {
                                        pmsk.StoreProductName = dr["itemName"].ToString();
                                        pmsk.StoreDescription = dr["itemName"].ToString();
                                        full.pname = dr["itemName"].ToString();
                                        full.pdesc = dr["itemName"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                        decimal price = Convert.ToDecimal(dr["pricePL1"] == "" ? 0 : dr["pricePL1"]);

                                        if (price > 0)
                                        {
                                            pmsk.Price = price;
                                            full.Price = price;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    pmsk.sprice = "";
                                    pmsk.pack = 1;
                                    pmsk.Tax = Tax;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if (pmsk.Qty > 0)
                                    {
                                        prodlist.Add(pmsk);
                                        fullnamelist.Add(full);
                                    }
                                }
                                Console.WriteLine("Generating Perfect Pos " + StoreId + " Product CSV Files.....");
                                string ProductFilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Perfect Pos " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Perfect Pos " + StoreId + " Full Name CSV Files.....");
                                string FullFileName = GenerateCSV.GenerateCSVFile(fullnamelist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Full Name File Generated For Perfect Pos " + StoreId);

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
                                return "Not generated file for" + StoreId;
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
        public class ProdModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public int Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public decimal Price { get; set; }
            public string sprice { get; set; }
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
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string uom { get; set; }
            public string country { get; set; }
            public string region { get; set; }
        }
    }
}
