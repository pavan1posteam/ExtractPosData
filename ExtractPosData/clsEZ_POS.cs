
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
    class clsEZ_POS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsEZ_POS(int StoreId, decimal Tax)
        {
            try
            {
                EZPOSConvertRawFile(StoreId, Tax);
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
            return dtResult; //Returning datatable 
        }
        public string EZPOSConvertRawFile(int StoreId, decimal Tax)
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

                                List<EZroductsModels> prodlist = new List<EZroductsModels>();
                                List<EZFullNameProductModel> fulllist = new List<EZFullNameProductModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    EZroductsModels pmsk = new EZroductsModels();
                                    EZFullNameProductModel full = new EZFullNameProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["Code"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["Code"].ToString().Replace(" -", "");
                                        full.upc = "#" + dr["Code"].ToString().Replace(" -", "");
                                    }
                                    else
                                    {
                                        pmsk.upc = '#' + Convert.ToDouble(dr["UPC"]).ToString().Replace("-", "");
                                    }
                                    decimal qty = Convert.ToDecimal(dr["Onhand"]);
                                    pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                    pmsk.sku = "#" + dr.Field<string>("Code");
                                    full.sku = "#" + dr.Field<string>("Code");
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Description")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Description").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("Description").Trim();
                                        full.pname = dr.Field<string>("Description").Trim();
                                        full.pdesc = dr.Field<string>("Description").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    full.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    if (pmsk.Price <= 0 || full.Price <= 0)
                                    {
                                        continue;
                                    }
                                    full.pack = 1;
                                    full.pcat = dr["Dept"].ToString();
                                    full.pcat1 = "";
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
                                Console.WriteLine("Generating REATILEDGE " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating REATILEDGE " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For REATILEDGE" + StoreId);
                                Console.WriteLine("Fullname File Generated For REATILEDGE" + StoreId);

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
                                return "Not generated file for RETAILEDGE " + StoreId;
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
    }
    public class EZroductsModels
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public decimal Qty { get; set; }
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
    }
    class EZFullNameProductModel
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
