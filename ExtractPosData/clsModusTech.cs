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
    public class clsModusTech
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsModusTech(int storeid, decimal tax)
        {
            try
            {
                clsModusTechConvertRawFile(storeid, tax);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
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
        #region
        //public static DataTable ConvertTextToDataTable(string FileName)
        //{
        //        DataTable dtResult = new DataTable();
        //        using (TextFieldParser parser = new TextFieldParser(FileName))
        //        {
        //            try
        //           {

        //            parser.TextFieldType = FieldType.Delimited;
        //            parser.SetDelimiters(",");
        //            int i = 0;
        //            int r = 0;
        //            while (!parser.EndOfData)
        //            {
        //                if (i == 0)
        //                {
        //                    string[] columns = parser.ReadFields();
        //                    foreach (string col in columns)
        //                    {
        //                        dtResult.Columns.Add(col);
        //                    }
        //                }
        //                else
        //                {
        //                    string[] rows = parser.ReadFields();
        //                    dtResult.Rows.Add();
        //                    int c = 0;
        //                    foreach (string row in rows)
        //                    {
        //                        if (r >= 5973)
        //                        {
        //                           // string[] cells = row.Replace("\"", "").Split(new string[] { " " }, StringSplitOptions.None);
        //                            goto d;
        //                        }
        //                   //  var    line = RemoveWhiteSpace(row[i]).Trim(); // here i am getting error in RemoveWhiteSpace, that it is not in the context

        //                        var roww = row.Replace('"', ' ').Trim().Replace(" ", string.Empty).Replace("#! ", ", ");
        //                       // var roww = row.Replace('"', ' ').Trim();
        //                      // roww =  row.Replace('""" """', ' ').Trim();
        //                        dtResult.Rows[r][c] = roww.ToString();
        //                        c++;
        //                    }
        //                d:
        //                    r++;
        //                }
        //                i++;
        //            }
        //                }
        //         catch (Exception ex)
        //      {
        //        //  
        //        Console.WriteLine(ex.Message);

        //       // return dtResult; //Returning Dattable  
        //    }
        //        return dtResult; //Returning Dattable  
        //    }

        //}
        #endregion
        public string clsModusTechConvertRawFile(int StoreId, decimal tax)
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

                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel fname = new FullNameProductModel();
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(dr["ItemLookupCode"].ToString()))
                                        {
                                            var upc = dr["ItemLookupCode"].ToString().ToLower();
                                            // string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                            //if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                            //{
                                            if (!string.IsNullOrEmpty(upc))
                                            {
                                                pmsk.upc = "#" + upc.Trim().ToLower();
                                                fname.upc = "#" + upc.Trim().ToLower();
                                                pmsk.sku = "#" + upc.Trim().ToLower();
                                                fname.sku = "#" + upc.Trim().ToLower();
                                            }
                                            //else
                                            //{
                                            //    continue;
                                            //}
                                            // }
                                            else
                                            {
                                                continue;
                                            }
                                            pmsk.StoreID = StoreId;
                                            double qty = System.Convert.ToDouble(dr["Quantity"]);
                                            
                                            pmsk.Qty = (int)qty > 0 ? (int)(qty) : 0;
                                            
                                            if (!string.IsNullOrEmpty(dr.Field<string>("StoreProductName").Trim()))
                                            {
                                                pmsk.StoreProductName = dr.Field<string>("StoreProductName").Trim();
                                                pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
                                                fname.pdesc = dr.Field<string>("StoreProductName").Trim();
                                                fname.pname = dr.Field<string>("StoreProductName").Trim();
                                                pmsk.StoreDescription = Regex.Replace(pmsk.StoreDescription, @"'[^']+'(?=!\w+)", "");
                                                fname.pdesc = Regex.Replace(fname.pdesc, @"'[^']+'(?=!\w+)", "");
                                            }
                                            else
                                            { continue; }

                                            string Price = System.Convert.ToString(dr["price"]);
                                            if (Price != "")
                                            {
                                                pmsk.Price = System.Convert.ToDecimal(dr["price"].ToString());
                                                fname.Price = System.Convert.ToDecimal(dr["price"].ToString());
                                            }
                                            string sprice = System.Convert.ToString(dr["SalePrice"]);
                                            if (sprice != "")
                                            {
                                                pmsk.sprice = System.Convert.ToDecimal(dr["SalePrice"].ToString()); ;
                                            }
                                            // pmsk.sprice = System.Convert.ToDecimal(dr["SalePrice"].ToString()); ;
                                            pmsk.pack = 1;

                                            if (StoreId != 10248)
                                            {
                                                pmsk.Tax = System.Convert.ToDecimal(dr["Tax"].ToString()); ;
                                            }
                                            else
                                            {
                                                pmsk.Tax = tax;
                                            }
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
                                            if (!string.IsNullOrEmpty(dr["AltUpc1"].ToString()))
                                            {
                                                var altupc1 = "#" + dr["AltUpc1"].ToString().ToLower();
                                                string altnumberUpc1 = altupc1.ToString();
                                                pmsk.altupc1 = altnumberUpc1;
                                            }
                                            else { pmsk.altupc1 = ""; }
                                            if (!string.IsNullOrEmpty(dr["AltUpc2"].ToString()))
                                            {
                                                var altupc2 = "#" + dr["AltUpc2"].ToString().ToLower();
                                                string altnumberUpc2 = altupc2.ToString();
                                                pmsk.altupc2 = altnumberUpc2;
                                            }
                                            else { pmsk.altupc2 = ""; }
                                            if (!string.IsNullOrEmpty(dr["AltUpc3"].ToString()))
                                            {
                                                var altupc3 = "#" + dr["AltUpc3"].ToString().ToLower();
                                                string altnumberUpc3 = altupc3.ToString();
                                                pmsk.altupc3 = altnumberUpc3;
                                            }
                                            else { pmsk.altupc3 = ""; }
                                            if (!string.IsNullOrEmpty(dr["AltUpc4"].ToString()))
                                            {
                                                var altupc4 = "#" + dr["AltUpc4"].ToString().ToLower();
                                                string altnumberUpc4 = altupc4.ToString();
                                                pmsk.altupc4 = altnumberUpc4;
                                            }
                                            else { pmsk.altupc4 = ""; }
                                            if (!string.IsNullOrEmpty(dr["AltUpc5"].ToString()))
                                            {
                                                var altupc5 = "#" + dr["AltUpc5"].ToString().ToLower();
                                                string altnumberUpc5 = altupc5.ToString();
                                                pmsk.altupc5 = altnumberUpc5;
                                            }
                                            else { pmsk.altupc5 = ""; }
                                            string Deposit = System.Convert.ToString(dr["Deposit"]);
                                            if (Deposit != "")
                                            {
                                                pmsk.Deposit = System.Convert.ToDecimal(Deposit.ToString());
                                            }
                                            //pmsk.Deposit = dr["Deposit"].ToString();
                                            fname.pcat = "";
                                            fname.pcat1 = "";
                                            fname.pcat2 = "";
                                            fname.uom = "";
                                            fname.pack = 1;
                                            fname.region = "";
                                            fname.country = "";

                                            if (StoreId == 10248)
                                            {
                                                if (pmsk.Price > 0 && pmsk.Qty > 0)
                                                {
                                                    prodlist = prodlist.Distinct().ToList();
                                                    prodlist.Add(pmsk);

                                                    full = full.Distinct().ToList();
                                                    full.Add(fname);
                                                }
                                            }
                                            else
                                            {
                                                if (pmsk.Price > 0)
                                                {
                                                    prodlist = prodlist.Distinct().ToList();
                                                    prodlist.Add(pmsk);

                                                    full = full.Distinct().ToList();
                                                    full.Add(fname);
                                                }
                                            }
                                        }
                                        //PREVIOUS CODE FOR 10248
                                        #region
                                        //DataTable dt = ConvertTextToDataTable(Url);


                                        //var dtr = from s in dt.AsEnumerable() select s;


                                        //List<ProductsModel> prodlist = new List<ProductsModel>();
                                        //List<FullNameProductModel> full = new List<FullNameProductModel>();

                                        //dynamic upcs;
                                        //dynamic taxs;
                                        //int barlenth = 0;

                                        //dt.DefaultView.Sort = "ID";

                                        //foreach (DataRow dr in dt.Rows)
                                        //{
                                        //    ProductsModel pmsk = new ProductsModel();
                                        //    FullNameProductModel fname = new FullNameProductModel();

                                        //    dt.DefaultView.Sort = "ID";
                                        //    try
                                        //    {

                                        //        upcs = dt.DefaultView.FindRows(dr["ID"]).ToArray();

                                        //        barlenth = ((Array)upcs).Length;      

                                        //        if (barlenth > 0)
                                        //        {
                                        //            for (int i = 0; i <= barlenth - 1; i++)
                                        //            {
                                        //                if (i == 0)
                                        //                {
                                        //                    if (!string.IsNullOrEmpty(dr["Alias"].ToString()))
                                        //                    {
                                        //                        var upc = "#" + upcs[i]["Alias"].ToString().ToLower();
                                        //                        string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                        //                        if (numberUpc.Count() >= 7 && numberUpc.Count() <= 15)
                                        //                        {
                                        //                            if (!string.IsNullOrEmpty(numberUpc))
                                        //                            {
                                        //                                pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                        //                                fname.upc = "#" + numberUpc.Trim().ToLower();
                                        //                            }
                                        //                            else
                                        //                            {
                                        //                                continue;
                                        //                            }
                                        //                        }
                                        //                        else
                                        //                        {
                                        //                            continue;
                                        //                        }
                                        //                    }
                                        //                    else
                                        //                    {
                                        //                        continue;
                                        //                    }

                                        //                }
                                        //                if (i == 1)
                                        //                {
                                        //                    pmsk.altupc1 = "#" + upcs[i]["Alias"];
                                        //                }
                                        //                if (i == 2)
                                        //                {
                                        //                    pmsk.altupc2 = "#" + upcs[i]["Alias"];
                                        //                }
                                        //                if (i == 3)
                                        //                {
                                        //                    pmsk.altupc3 = "#" + upcs[i]["Alias"];
                                        //                }
                                        //                if (i == 4)
                                        //                {
                                        //                    pmsk.altupc4 = "#" + upcs[i]["Alias"];
                                        //                }
                                        //                if (i == 5)
                                        //                {
                                        //                    pmsk.altupc5 = "#" + upcs[i]["Alias"];
                                        //                }
                                        //                pmsk.StoreID = StoreId;

                                        //                if (!string.IsNullOrEmpty(dr["ID"].ToString()))
                                        //                {

                                        //                    pmsk.sku = "#" + dr["ID"].ToString();
                                        //                    fname.sku = "#" + dr["ID"].ToString();
                                        //                }
                                        //                else
                                        //                { continue; }
                                        //                double qty = System.Convert.ToDouble(dr["Quantity"]);
                                        //                if (qty > 0)
                                        //                {
                                        //                    pmsk.Qty = (int)qty;
                                        //                }
                                        //                else { continue; }
                                        //                if (!string.IsNullOrEmpty(dr.Field<string>("Description").Trim()))
                                        //                {
                                        //                    pmsk.StoreProductName = dr.Field<string>("Description").Trim();
                                        //                    pmsk.StoreDescription = dr.Field<string>("Description").Trim();
                                        //                    fname.pdesc = dr.Field<string>("Description").Trim();
                                        //                    fname.pname = dr.Field<string>("Description").Trim();
                                        //                    pmsk.StoreDescription = Regex.Replace(pmsk.StoreDescription, @"'[^']+'(?=!\w+)", "");
                                        //                    fname.pdesc = Regex.Replace(fname.pdesc, @"'[^']+'(?=!\w+)", "");
                                        //                }
                                        //                else
                                        //                { continue; }
                                        //                pmsk.Price = System.Convert.ToDecimal(dr["Price"].ToString());
                                        //                fname.Price = System.Convert.ToDecimal(dr["Price"].ToString());
                                        //                pmsk.sprice = System.Convert.ToDecimal(dr["SalePrice"].ToString()); ;
                                        //                pmsk.pack = 1;
                                        //                pmsk.Tax = tax;
                                        //                if (pmsk.sprice > 0)
                                        //                {
                                        //                    pmsk.Start = DateTime.Now.ToString("MM/dd/yyyy");
                                        //                    pmsk.End = DateTime.Now.AddDays(1).ToString("MM/dd/yyyy");
                                        //                }
                                        //                else
                                        //                {
                                        //                    pmsk.Start = "";
                                        //                    pmsk.End = "";
                                        //                }
                                        //                fname.pcat = "";
                                        //                fname.pcat1 = "";
                                        //                fname.pcat2 = "";
                                        //                fname.uom = "";
                                        //                fname.region = "";
                                        //                fname.country = "";


                                        //                string sku = dr["ID"].ToString();
                                        //                if (pmsk.Price > 0 && !string.IsNullOrEmpty(pmsk.upc))
                                        //                {

                                        //                    prodlist = prodlist.Distinct().ToList();
                                        //                    prodlist.Add(pmsk);

                                        //                    full = full.Distinct().ToList();
                                        //                    full.Add(fname);
                                        //                }

                                        //            }
                                        #endregion
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("" + ex.Message);
                                    }
                                }
                                prodlist = prodlist.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();
                                full = full.GroupBy(x => x.sku).Select(y => y.FirstOrDefault()).ToList();

                                Console.WriteLine("Generating ModusTech " + StoreId + " Product CSV File.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For ModusTech " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating ModusTech " + StoreId + " Fullname CSV File.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For ModusTech " + StoreId);

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
                                return "Not generated file for ModusTech " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName or Raw Folder is Empty! " + StoreId);
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
