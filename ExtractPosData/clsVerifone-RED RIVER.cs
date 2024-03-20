using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;

namespace ExtractPosData
{
    class clsVerifone_RED_RIVER
    {
        public clsVerifone_RED_RIVER(int storeId, decimal Tax)
        {
            try
            {
                RedRiverConvertRawFile(storeId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            try
            {
                DataTable dataTable = GetDataTableFromCsv(FileName, true);
                return dataTable;
            }
            catch (Exception)
            {
                throw;
            }
        }

        static DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {
            string header = isFirstRowHeader ? "Yes" : "No";

            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties=\"Text;HDR=" + header + "\""))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
        }


        //public static DataTable ConvertCsvToDataTable(string FileName)
        //{
        //    DataTable dtResult = new DataTable();
        //    using (TextFieldParser parser = new TextFieldParser(FileName))
        //    {
        //        try
        //        {
        //            parser.TextFieldType = FieldType.Delimited;
        //            parser.SetDelimiters(",");
        //            int i = 0;
        //            int r = 0;
        //            while (!parser.EndOfData)
        //            {
        //                if (i == 0)
        //                {
        //                    string[] columns = parser.ReadFields();
        //                    if (columns.Contains("Loc"))
        //                    {
        //                        foreach (string col in columns)
        //                        {
        //                            if (col == "Package") { break; }
        //                            else
        //                            {
        //                                dtResult.Columns.Add(col);
        //                            }
        //                        }
        //                    }
        //                    else { continue; }
        //                }
        //                else
        //                {
        //                    string[] rows = parser.ReadFields();
        //                    dtResult.Rows.Add();
        //                    int c = 0;
        //                    foreach (string row in rows)
        //                    {
        //                        var roww = row.Replace('"', ' ').Trim();

        //                        dtResult.Rows[r][c] = roww.ToString();
        //                        c++;
        //                    }

        //                    r++;
        //                }
        //                i++;
        //            }
        //        }
        //        catch (Exception ex)
        //        { Console.WriteLine(ex.Message); }
        //    }
        //    return dtResult; //Returning Dattable  
        //}
        public string RedRiverConvertRawFile(int StoreId, decimal Tax)
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

                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["upc"].ToString()))
                                    {
                                        pmsk.upc =  dr["upc"].ToString();
                                        full.upc =  dr["upc"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                  //  decimal qty = Convert.ToDecimal(dr["QTY"]);
                                    pmsk.Qty = Convert.ToInt32(999);
                                   
                                        pmsk.sku =  dr.Field<string>("sku");
                                        full.sku =  dr.Field<string>("sku");
                                   
                                    if (!string.IsNullOrEmpty(dr.Field<string>("StoreProductName")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("StoreProductName").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
                                        full.pname = dr.Field<string>("StoreProductName").Trim();
                                        full.pdesc = dr.Field<string>("StoreProductName").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    full.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    full.pcat = "";
                                    full.pcat1 = "";
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";
                                    pmsk.sprice = 0;
                                    pmsk.pack = 1;
                                    full.pack = 1;
                                    pmsk.uom = "";
                                    full.uom = "";
                                    pmsk.Tax = Tax;
                                    
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    if (pmsk.Qty > 0 && pmsk.Price>0)
                                    {
                                        fulllist.Add(full);
                                        prodlist.Add(pmsk);
                                    }
                                }
                                Console.WriteLine("Generating REDRIVERPOS " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating REDRIVERPOS " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For REDRIVERPOS" + StoreId);
                                Console.WriteLine("Fullname File Generated For REDRIVERPOS" + StoreId);

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
                                return "Not generated file for REDRIVERPOS " + StoreId;
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
            return "Completed generating File For REDRIVERPOS" + StoreId;
        }
    }
}
