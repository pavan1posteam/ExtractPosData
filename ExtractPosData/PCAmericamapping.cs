using ExcelDataReader;
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
    class clsPCAmerica
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsPCAmerica(int StoreId, decimal Tax)
        {
            try
            {
                clsCetechConvertRawFile(StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertCsvToDataTable(string FileName, int StoreId)
        {
            FileStream stream = File.Open(FileName, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            DataSet result = excelReader.AsDataSet();
            DataTable dt = new DataTable();
            dt = result.Tables[0];
            List<DataRow> list = new List<DataRow>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr);
            }
            dt = dt.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["Column0"].ToString() == string.Empty || dt.Rows[i]["Column4"].ToString() == string.Empty || string.IsNullOrEmpty(dt.Rows[i]["Column5"].ToString()) || dt.Rows[i]["Column6"].ToString() == string.Empty)
                {
                    DataRow dr = dt.Rows[i];
                    dr.Delete();
                    continue;
                }
            }
            dt.AcceptChanges();
            stream.Close();
            return dt; //Returning Datatable
        }
        public string clsCetechConvertRawFile(int StoreId, decimal Tax)
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
                                DataTable dt = ConvertCsvToDataTable(Url, StoreId);
                                List<CetechModel> prodlist = new List<CetechModel>();
                                List<FullNameProductModel> fulllist = new List<FullNameProductModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    CetechModel pmsk = new CetechModel();
                                    pmsk.StoreID = StoreId;
                                    pmsk.upc = "#" + dr["Column1"].ToString();                                    
                                    decimal qty = Convert.ToDecimal(dr["Column4"]);
                                    pmsk.Qty = Convert.ToInt32(qty);
                                    pmsk.sku = "#"+ dr["Column1"].ToString();
                                    pmsk.StoreProductName = dr.Field<string>("Column0").Trim();
                                    pmsk.StoreDescription = dr.Field<string>("Column0").Trim();
                                    pmsk.Price = (dr["Column6"].ToString());
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
                                    if (pmsk.Qty < 0)
                                    {
                                        pmsk.Qty = 0;
                                    }
                                    prodlist.Add(pmsk);                                    
                                }
                                Console.WriteLine("Generating Cetech " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Cetech" + StoreId);
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
                                return "Not generated file for Cetech " + StoreId;
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
            return "Completed generating File For Cetech" + StoreId;
        }

        public class CetechModel
        {
            public int StoreID { get; set; }
            public string upc { get; set; }
            public int Qty { get; set; }
            public string sku { get; set; }
            public int pack { get; set; }
            public string uom { get; set; }
            public string StoreProductName { get; set; }
            public string StoreDescription { get; set; }
            public string Price { get; set; }
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
    }
}
