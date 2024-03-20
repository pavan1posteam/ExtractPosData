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
    class clsFrontierPOS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsFrontierPOS(int StoreId, decimal Tax, int StoreMapId)
        {
            try
            {
                FrontierConvertRawFile(StoreId, Tax, StoreMapId);
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
        public string FrontierConvertRawFile(int StoreId, decimal Tax, int StoreMapId)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreMapId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreMapId + "/Raw/");
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();

                        string Url = BaseUrl + "/" + StoreMapId + "/Raw/" + myFile;
                        if (File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = ConvertCsvToDataTable(Url);

                                List<FrontierProductModels> prodlist = new List<FrontierProductModels>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    FrontierProductModels pmsk = new FrontierProductModels();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["upc"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["upc"].ToString().Replace(" -", "");
                                    }
                                    else
                                    {
                                        pmsk.upc = '#' + Convert.ToDouble(dr["UPC"]).ToString().Replace("-", "");
                                    }
                                    decimal qty = Convert.ToDecimal(dr["qty"]);
                                    pmsk.Qty = Convert.ToInt32(qty) > 0 ? Convert.ToInt32(qty) : 0;
                                    pmsk.sku = "#" + dr.Field<string>("upc");
                                    if (!string.IsNullOrEmpty(dr.Field<string>("StoreProductName")))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("StoreProductName").Trim();
                                        pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["price"] == DBNull.Value ? 0 : dr["price"]);
                                    if (pmsk.Price <= 0)
                                    {
                                        continue;
                                    }
                                    pmsk.uom = dr["size"].ToString();
                                    pmsk.sprice = System.Convert.ToDecimal(dr["sprice"]);
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

                                    prodlist.Add(pmsk);
                                }
                                Console.WriteLine("Generating REATILEDGE " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For FRONTIER" + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreMapId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    //GC.Collect();
                                    //string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    //File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                return "Not generated file for FRONTIER " + StoreId;
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
    public class FrontierProductModels
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
}

