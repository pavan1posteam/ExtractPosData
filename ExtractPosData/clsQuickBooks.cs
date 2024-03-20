using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
namespace ExtractPosData
{
    public class clsQuickBooks
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsQuickBooks(int StoreId, decimal tax)
        {
            try
            {
                QuickbooksConvertRawFile(StoreId, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public static DataTable ConvertExcelToDataTable(string FileName)
        {
            DataTable dtResult = null;
            int totalSheet = 0; //No of sheets on excel file  
            using (OleDbConnection objConn = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + FileName + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1;';"))
            {
                objConn.Open();
                OleDbCommand cmd = new OleDbCommand();
                OleDbDataAdapter oleda = new OleDbDataAdapter();
                DataSet ds = new DataSet();
                DataTable dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string sheetName = string.Empty;
                if (dt != null)
                {
                    var tempDataTable = (from dataRow in dt.AsEnumerable()
                                         where !dataRow["TABLE_NAME"].ToString().Contains("FilterDatabase")
                                         select dataRow).CopyToDataTable();
                    dt = tempDataTable;
                    totalSheet = dt.Rows.Count;
                    sheetName = dt.Rows[0]["TABLE_NAME"].ToString();
                }
                cmd.Connection = objConn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM [" + sheetName + "]";
                oleda = new OleDbDataAdapter(cmd);
                oleda.Fill(ds, "excelData");
                dtResult = ds.Tables["excelData"];
                objConn.Close();
                return dtResult; //Returning Dattable  
            }
        }
        public static DataTable ConvertCsvToDataTablee(string FileName)
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
                        if (columns.Contains("Department"))
                        {
                            foreach (string col in columns)
                            {
                                dtResult.Columns.Add(col);
                            }
                        }
                        else { continue; }
                    }
                    else
                    {
                        string[] rows = parser.ReadFields();
                        dtResult.Rows.Add();
                        int c = 0;
                        foreach (string row in rows)
                        {
                            var roww = row.Replace('"', ' ').Trim();

                            dtResult.Rows[r][c] = roww.ToString();
                            c++;
                        }

                        r++;
                    }
                    i++;
                }
            }
            return dtResult; //Returning Dattable  
        }
        public static DataTable ConvertCsvToDataTables(string FileName)
        {
            DataTable dt = new DataTable();

            try
            {
                bool firstFile = true;
                using (TextFieldParser parser = new TextFieldParser(FileName))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.Delimiters = new string[] { "," };
                    parser.HasFieldsEnclosedInQuotes = true;

                    string[] columns = parser.ReadFields();

                    if (firstFile == true)
                    {
                        int i = 0;
                        for (i = 0; i < columns.Length; i++)
                        {
                            dt.Columns.Add(columns[i], typeof(string));
                        }
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
                    firstFile = false;
                }
            }
            catch (Exception e)
            {
            }
            dt = dt.AsEnumerable().Skip(5).CopyToDataTable();
            return dt;
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
                            dtResult.Columns.Add(col);
                        }
                    }
                    else
                    {
                        string[] rows = parser.ReadFields();
                        dtResult.Rows.Add();
                        int c = 0;
                        foreach (string row in rows)
                        {
                            var roww = row.Replace('"', ' ').Trim();

                            dtResult.Rows[r][c] = roww.ToString();
                            c++;
                        }

                        r++;
                    }
                    i++;
                }
            }
            return dtResult; //Returning Dattable  
        }
        public string QuickbooksConvertRawFile(int StoreId, decimal Tax)
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
                                DataTable dt;
                                if (StoreId == 10816)
                                {
                                    dt = ConvertCsvToDataTablee(Url);
                                }
                                else if (StoreId == 10853)
                                {
                                    dt = ConvertExcelToDataTable(Url);
                                }
                                else
                                {
                                    dt = ConvertCsvToDataTable(Url);
                                }

                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (StoreId == 10902 || StoreId == 10785 || StoreId == 11292)
                                    {
                                        if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                        {
                                            pmsk.upc = "#" + dr["UPC"].ToString();
                                            full.upc = "#" + dr["UPC"].ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        decimal qty = Convert.ToDecimal(dr["Qty 1"]);
                                        if (qty > 0)
                                        {
                                            pmsk.Qty = Convert.ToInt32(qty);
                                        }
                                        else { continue; }

                                        //SKU
                                        pmsk.sku = "#" + dr.Field<string>("Item Number");
                                        full.sku = "#" + dr.Field<string>("Item Number");



                                        if (!string.IsNullOrEmpty(dr.Field<string>("Item Name")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("Item Name");
                                            pmsk.StoreDescription = dr.Field<string>("Item Name").Trim();
                                            full.pname = dr.Field<string>("Item Name");
                                            full.pdesc = dr.Field<string>("Item Name");
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.Price = System.Convert.ToDecimal(dr["Regular Price"] == DBNull.Value ? 0 : dr["Regular Price"]);
                                        full.Price = System.Convert.ToDecimal(dr["Regular Price"] == DBNull.Value ? 0 : dr["Regular Price"]);
                                        if (pmsk.Price <= 0 || full.Price <= 0)
                                        {
                                            continue;
                                        }
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        pmsk.Tax = Tax;

                                        if (pmsk.sprice > 0)
                                        {
                                            pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                            pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                        }
                                        else
                                        {
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                        }
                                        full.pack = 1;
                                        pmsk.uom = dr["Size"].ToString();
                                        full.uom = dr["Size"].ToString();
                                        full.pcat = dr["Department Name"].ToString();
                                        full.pcat1 = "";
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";

                                        prodlist.Add(pmsk);
                                        fulllist.Add(full);

                                    }
                                    else if (StoreId == 10853)
                                    {
                                        if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                        {
                                            pmsk.upc = "#" + dr["UPC"].ToString();
                                            full.upc = "#" + dr["UPC"].ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        decimal qty = Convert.ToDecimal(dr["Qty 1"]);

                                        pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;

                                        pmsk.sku = "#" + dr.Field<double>("Item Number");
                                        if (!string.IsNullOrEmpty(dr.Field<string>("Item Name")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("Item Name");
                                            pmsk.StoreDescription = dr.Field<string>("Item Name").Trim();
                                            full.pname = dr.Field<string>("Item Name");
                                            full.pdesc = dr.Field<string>("Item Name");
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.Price = System.Convert.ToDecimal(dr["Regular Price"] == DBNull.Value ? 0 : dr["Regular Price"]);
                                        full.Price = System.Convert.ToDecimal(dr["Regular Price"] == DBNull.Value ? 0 : dr["Regular Price"]);
                                        if (pmsk.Price <= 0 || full.Price <= 0)
                                        {
                                            continue;
                                        }
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        pmsk.Tax = Tax;

                                        if (pmsk.sprice > 0)
                                        {
                                            pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                            pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                        }
                                        else
                                        {
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                        }
                                        full.pack = 1;
                                        full.uom = dr["Size"].ToString();
                                        string cat = dr["Department Name"].ToString();
                                        cat = string.Concat(cat.TakeWhile((c) => c != ':'));
                                        full.pcat = cat.ToString();
                                        full.pcat1 = "";
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        if (full.pcat != "TOBACCO")
                                        {
                                            prodlist.Add(pmsk);
                                            fulllist.Add(full);
                                        }
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(dr["UPC"].ToString()))
                                        {
                                            pmsk.upc = "#" + dr["UPC"].ToString();
                                            full.upc = "#" + dr["UPC"].ToString();
                                            pmsk.sku = "#" + dr.Field<string>("UPC");
                                            full.sku = "#" + dr.Field<string>("UPC");
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        decimal qty = Convert.ToDecimal(dr["On-hand Qty"]);
                                        pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                        if (!string.IsNullOrEmpty(dr.Field<string>("Item Name")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("Item Name");
                                            pmsk.StoreDescription = dr.Field<string>("Item Name").Trim();
                                            full.pname = dr.Field<string>("Item Name");
                                            full.pdesc = dr.Field<string>("Item Name");
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.Price = System.Convert.ToDecimal(dr["Active Price"] == DBNull.Value ? 0 : dr["Active Price"]);
                                        full.Price = System.Convert.ToDecimal(dr["Active Price"] == DBNull.Value ? 0 : dr["Active Price"]);
                                        if (pmsk.Price <= 0 || full.Price <= 0)
                                        {
                                            continue;
                                        }
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        pmsk.Tax = Tax;

                                        if (pmsk.sprice > 0)
                                        {
                                            pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                            pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                        }
                                        else
                                        {
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                        }
                                        full.pack = 1;
                                        full.uom = dr["Size"].ToString();
                                        full.pcat = dr["Department"].ToString();
                                        full.pcat1 = "";
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";

                                        prodlist.Add(pmsk);
                                        fulllist.Add(full);
                                    }
                                }
                                Console.WriteLine("Generating Quickbooks " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating Quickbooks " + StoreId + " FullName CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Quickbooks");
                                Console.WriteLine("Full Name File Generated For Quickbooks");

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
                                return "Not generated file for Quickbooks " + StoreId;
                            }
                        }
                        else
                        {
                            return "Invalid FileName";
                        }
                    }
                    else
                    {
                        Console.WriteLine("There is no file in the Raw Folder of " + StoreId);
                        return "";
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
            return "Completed generating File";
        }
    }
}
