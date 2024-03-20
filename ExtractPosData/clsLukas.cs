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
using System.Threading.Tasks;

namespace ExtractPosData
{
    public class clsLukas
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsLukas(int StoreId, decimal Tax)
        {
            try
            {
                LukasConvertRawFile(StoreId, Tax);
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
        public string LukasConvertRawFile(int StoreId, decimal Tax)
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

                            List<LukasProductsModel> prodlist = new List<LukasProductsModel>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                LukasProductsModel pmsk = new LukasProductsModel();
                                pmsk.StoreID = StoreId;
                                pmsk.upc = dr["UPC"].ToString().Trim();
                                var aaa = dr["Qty"];
                                var bb = aaa;
                                pmsk.Qty = Convert.ToInt32(dr["Qty"]) > 0 ? Convert.ToInt32(dr["Qty"]) : 0;
                                pmsk.sku = dr["UPC"].ToString().Trim();
                                pmsk.pack = 1;
                                pmsk.StoreProductName = dr["StoreProductName"].ToString().Trim();
                                pmsk.StoreDescription = dr["StoreProductName"].ToString().Trim();
                                pmsk.Price = System.Convert.ToDecimal(dr["Price".ToLower()] == DBNull.Value ? 0 : dr["Price".ToLower()]);
                                if (!String.IsNullOrEmpty(dr.Field<string>("sprice")))
                                {
                                    pmsk.sprice = System.Convert.ToDecimal(dr["sprice"] == DBNull.Value ? 0 : dr["sprice"]);
                                }
                                else
                                {
                                    pmsk.sprice = 0;
                                }
                                if (pmsk.sprice > 0)
                                {
                                    pmsk.Start = dr["Start"].ToString().Trim();
                                    pmsk.End = dr["End"].ToString().Trim();
                                }
                                else
                                {
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                }                                
                                if (Tax == 0)
                                {
                                    pmsk.Tax = System.Convert.ToDecimal(dr["tax".ToLower()] == DBNull.Value ? 0 : dr["tax".ToLower()]);
                                }
                                else
                                {
                                    pmsk.Tax = Tax;
                                }
                                pmsk.altupc1 = dr["altupc1"].ToString().Trim();
                                pmsk.altupc2 = dr["altupc2"].ToString().Trim();
                                pmsk.altupc3 = dr["altupc3"].ToString().Trim();
                                pmsk.altupc4 = dr["altupc4"].ToString().Trim();
                                pmsk.altupc5 = dr["altupc5"].ToString().Trim();

                                pmsk.Discountable = dr.Field<string>("Discountable");

                                if ( pmsk.Price > 0)
                                {
                                    prodlist.Add(pmsk);
                                }
                            }
                            Console.WriteLine("Generating Lukas " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For Lukas " + StoreId);

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
                            return "Not generated file for Lukas " + StoreId;
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
            return "Completed generating File For Lukas" + StoreId;
        }
    }
    public class LukasProductsModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public Int64 Qty { get; set; }
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
        public decimal Deposit { get; set; }
        public string Discountable { get; set; }
    }
}
