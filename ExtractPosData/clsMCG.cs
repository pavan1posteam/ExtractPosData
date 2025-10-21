using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;

namespace ExtractPosData
{
    class clsMCG
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsMCG(int StoreId, decimal Tax)
        {
            try
            {
                clsMCGConvertRawFile(StoreId, Tax);
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
                DataTable dtResult = new DataTable();
                FileStream stream = File.Open(FileName, FileMode.Open, FileAccess.Read);
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                DataSet result = excelReader.AsDataSet();
                excelReader.Close();
                dtResult = result.Tables[0];

                int count = dtResult.Columns.Count;
                var cName = dtResult.Rows[0];

                for (int i = 0; i < count; i++)
                {
                    string columnName = cName[i]?.ToString();
                    if (!string.IsNullOrWhiteSpace(columnName))
                    {
                        dtResult.Columns[i].ColumnName = columnName;
                    }
                    else
                    {
                        // Provide a default name for empty column names.
                        dtResult.Columns[i].ColumnName = "Column" + i;
                    }
                }
                for (int i = dtResult.Columns.Count - 1; i >= 0; i--)
                {
                    string columnName = dtResult.Columns[i].ColumnName;
                    var a = dtResult.Columns[i].ToString();
                    bool isEmptyColumn = dtResult.AsEnumerable().All(row => string.IsNullOrWhiteSpace(row.Field<string>(columnName)));
                    if (isEmptyColumn)
                    {
                        dtResult.Columns.RemoveAt(i);
                    }
                }

                // Remove the first row (column headers) from the DataTable.
                dtResult.Rows.RemoveAt(0);

                return dtResult;
            }
            catch (Exception ex)
            {
                // You can log the exception or return null, an empty DataTable, or some other error indication.
                Console.WriteLine(ex.Message);
                return null;
            }

        }
        public string clsMCGConvertRawFile(int StoreId, decimal Tax)
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
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel full = new FullNameProductModel();
                                    pmsk.StoreID = StoreId;

                                    if (string.IsNullOrEmpty(dr["UPC"].ToString()))
                                    {
                                        continue;
                                    }
                                    pmsk.upc = "#" + dr["UPC"].ToString();
                                    full.upc = "#" + dr["UPC"].ToString();
                                    //var qty = Convert.ToString(dr["On Hand"]);
                                    //if (qty.Trim().ToUpper() == "NULL")
                                    //{
                                    //    qty = "0";
                                    //}
                                    //if (string.IsNullOrEmpty(qty))
                                    //{
                                    //    continue;
                                    //}
                                    //var qty1 = Convert.ToDecimal(qty);
                                  //  pmsk.Qty = Convert.ToInt64(qty1) > 0 ? Convert.ToInt64(qty1) : 0;
                                    pmsk.Qty = 999;
                                    pmsk.sku = pmsk.upc;
                                    full.sku = pmsk.upc;
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Product Name")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Product Name").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("Product Name").Trim();
                                        full.pname = dr.Field<string>("Product Name").Trim();
                                        full.pdesc = dr.Field<string>("Product Name").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    try
                                    {
                                        pmsk.Price = Convert.ToDecimal(dr["Price"]);
                                        full.Price = Convert.ToDecimal(dr["Price"]);
                                    }
                                    catch (Exception)
                                    {

                                        continue;
                                    }

                                    if (pmsk.Price <= 0 || full.Price <= 0)
                                    {
                                        continue;
                                    }
                                    full.pack = 1;
                                    full.pcat = dr["Category"].ToString();
                                    full.pcat1 = "";
                                    pmsk.uom = "";
                                    full.uom = "";
                                    full.pcat2 = "";
                                    full.country = "";
                                    full.region = "";
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
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";

                                    fulllist.Add(full);
                                    prodlist.Add(pmsk);
                                }
                                Console.WriteLine("Generating MCG " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating MCG " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For MCG" + StoreId);
                                Console.WriteLine("Fullname File Generated For MCG" + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    GC.Collect();
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for MCG " + StoreId;
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
            return "Completed generating File For MCG" + StoreId;
        }
    }
}
