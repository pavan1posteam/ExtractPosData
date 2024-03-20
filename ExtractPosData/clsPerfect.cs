using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtractPosData
{
    class clsPerfect
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsPerfect(string FileName, int StoreId, decimal Tax)
        {
            try
            {
                PerfectConvertRawFile(FileName, StoreId, Tax);
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
                dataTable.Columns.RemoveAt(0);
                dataTable.Columns[0].ColumnName = "SKU/UPC";
                return dataTable;
            //    DataTable dtResult = new DataTable();
            //    using (TextFieldParser parser = new TextFieldParser(FileName))
            //    {
            //        parser.TextFieldType = FieldType.Delimited;
            //        parser.SetDelimiters(",");
            //        int i = 0;
            //        int r = 0;
            //        while (!parser.EndOfData)
            //        {
            //            if (i == 0)
            //            {
            //                string[] columns = parser.ReadFields();
            //                var columnsList = columns.ToList();
            //                columnsList.RemoveAt(0);

            //                foreach (string col in columnsList)
            //                {
            //                    dtResult.Columns.Add(col);
            //                }
            //            }
            //            else
            //            {
            //                string LineValue = parser.ReadLine();
                           
            //                if (LineValue.IndexOf(", ") > 0)
            //                {
            //                    LineValue = LineValue.Replace(", ", "#! ");
            //                }
            //                string[] rows = LineValue.Split(',');
            //                var rowsList = rows.ToList();
            //                rowsList.RemoveAt(0);
            //                dtResult.Rows.Add();
            //                int c = 0;
            //                foreach (string row in rowsList)
            //                {
            //                    var roww = row.Replace('"', ' ').Trim().Replace("$", string.Empty).Replace("#! ", ", ");

            //                    dtResult.Rows[r][c] = roww.ToString();
            //                    c++;
            //                }
            //                r++;
            //            }
            //            i++;
            //        }
            //    }
            //    return dtResult; //Returning Dattable  
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

        public string PerfectConvertRawFile(string PosFileName, int StoreId, decimal Tax)
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
                                    full.pname = dr.Field<string>("Description");
                                    full.pdesc = dr.Field<string>("Description");
                                    full.pcat = dr.Field<string>("Dept");
                                    full.pcat1 = "";
                                    full.pcat2 = "";
                                    full.uom = dr.Field<string>("Size");
                                    full.country = "";
                                    full.region = "";
                                    if (!string.IsNullOrEmpty(dr["SKU/UPC"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr.Field<double>("SKU/UPC").ToString();
                                        full.upc = "#" + dr.Field<double>("SKU/UPC").ToString();
                                        pmsk.sku = "#" + dr.Field<double>("SKU/UPC");
                                        full.sku = "#" + dr.Field<double>("SKU/UPC");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    var qty = dr["Onhand"].ToString();
                                    if (!string.IsNullOrEmpty(qty))
                                    {
                                        var qty1 = Convert.ToDecimal(qty);
                                        pmsk.Qty = Convert.ToInt32(qty1);
                                    }
                                    else { continue; }
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Description")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Description");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.StoreDescription = dr.Field<string>("Description");
                                    decimal price = System.Convert.ToDecimal(dr["Price_Amt1"] == DBNull.Value ? 0 : dr["Price_Amt1"]);
                                    if (price > 0)
                                    {
                                        pmsk.Price = price;
                                        full.Price = price;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.sprice = System.Convert.ToDecimal(dr["Price_Amt2"] == DBNull.Value ? 0 : dr["Price_Amt2"]);
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
                                    }
                                    fullnamelist.Add(full);
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
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@"+StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for" + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName Or Raw Folder is Empty! " + StoreId);
                        return "";
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
            return "Completed generating File";
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
            public string pcat { get; set; }
            public string pcat1 { get; set; }
            public string pcat2 { get; set; }
            public string uom { get; set; }
            public string country { get; set; }
            public string region { get; set; }
        }
    }
}
