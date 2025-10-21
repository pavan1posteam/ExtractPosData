using ExtractPosData.Model;
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
    class clsAsiFisher
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsAsiFisher(int StoreId,int StoreMapId)
        {
            try
            {
                AsiFisher(StoreId, StoreMapId);
            }catch(Exception e)
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
        public string AsiFisher(int StoreId, int StoreMapId)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreMapId + "/Upload/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreMapId + "/Upload/");
                    var path = BaseUrl + "/" + StoreMapId + "/Upload/";
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        try
                        {
                            string productFile = null;
                            string fullNameFile = null;

                            string[] files = Directory.GetFiles(path);

                            foreach (string file in files)
                            {
                                string fileName = Path.GetFileName(file);

                                if (fileName.Contains("PRODUCT"))
                                {
                                    productFile = fileName;
                                }
                                else if (fileName.Contains("FULLNAME"))
                                {
                                    fullNameFile = fileName;
                                }

                                if (productFile != null && fullNameFile != null)
                                    break;
                            }
                            if (productFile != null)
                            {
                                string Url = BaseUrl + "/" + StoreMapId + "/Upload/" + productFile;
                                DataTable dt = ConvertCsvToDataTable(Url);
                                List<AsiProductModels> prodlist = new List<AsiProductModels>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    AsiProductModels pmsk = new AsiProductModels();
                                    pmsk.StoreID = StoreId;
                                    pmsk.upc = dr["upc"].ToString();
                                    pmsk.Qty = Convert.ToDecimal(dr["qty"]);
                                    pmsk.sku = dr["sku"].ToString();
                                    pmsk.StoreProductName = dr["StoreProductName"].ToString();
                                    pmsk.StoreDescription = dr["Storedescription"].ToString();
                                    pmsk.Price = Convert.ToDecimal(dr["price"]);
                                    pmsk.sprice = Convert.ToDecimal(dr["sprice"]);
                                    pmsk.pack = Convert.ToInt32(dr["pack"]);
                                    pmsk.sprice = Convert.ToDecimal(dr["sprice"]);
                                    pmsk.Start = dr["start"].ToString();
                                    pmsk.End = dr["end"].ToString();
                                    pmsk.Tax = Convert.ToDecimal(dr["Tax"]);
                                    pmsk.altupc1 = dr["altupc1"].ToString();
                                    pmsk.altupc2 = dr["altupc2"].ToString();
                                    pmsk.altupc3 = dr["altupc3"].ToString();
                                    pmsk.altupc4 = dr["altupc4"].ToString();
                                    pmsk.altupc5 = dr["altupc5"].ToString();
                                    pmsk.Clubprice = Convert.ToDecimal(dr["Clubprice"]);
                                    pmsk.Deposit = Convert.ToDecimal(dr["Deposit"]);
                                    pmsk.Vintage = dr["Vintage"].ToString();
                                    pmsk.Cost = Convert.ToDecimal(dr["Cost"]);

                                    prodlist.Add(pmsk);                                    
                                }
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For ASIFISHER: " + StoreId);
                            }
                            else
                            {
                                Console.WriteLine("PRODUCT CSV file not found.");
                            }

                            if (fullNameFile != null)
                            {
                                string Url = BaseUrl + "/" + StoreMapId + "/Upload/" + fullNameFile;
                                DataTable dt = ConvertCsvToDataTable(Url);
                                List<clsASIFullName> prodlist = new List<clsASIFullName>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    clsASIFullName fname = new clsASIFullName();
                                    fname.upc = dr["upc"].ToString();
                                    fname.sku = dr["sku"].ToString();
                                    fname.pack = Convert.ToInt32(dr["pack"]);
                                    fname.pname = dr["prodname"].ToString();
                                    fname.pdesc = dr["descript"].ToString();
                                    fname.uom = dr["uom"].ToString();
                                    fname.pcat = dr["pcat"].ToString();
                                    fname.pcat1 = dr["pcat1"].ToString();
                                    fname.pcat2 = dr["pcat2"].ToString();
                                    fname.Price = Convert.ToDecimal(dr["price"]);
                                    fname.country = dr["country"].ToString();
                                    fname.region = dr["region"].ToString();

                                    prodlist.Add(fname);
                                }
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("FullName File Generated For ASIFISHER: " + StoreId);
                            }
                            else
                            {
                                Console.WriteLine("FULLNAME CSV file not found.");
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }                  
                }
                else
                {
                    return "Invalid Sub-Directory" + StoreMapId;
                }
            }
            else
            {
                return "Invalid Directory" + StoreMapId;
            }
            return "";
        }
    }
    public class  AsiProductModels
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
        public decimal Clubprice { get; set; }
        public string Vintage { get; set; }
        public decimal Cost { get; set; }
    }
    public class clsASIFullName
    {
        public string sku { get; set; }
        public int pack { get; set; }
        public string pname { get; set; }
        public string pdesc { get; set; }
        public string uom { get; set; }
        public string pcat { get; set; }
        public string pcat1 { get; set; }
        public string pcat2 { get; set; }
        public string upc { get; set; }       
        public decimal Price { get; set; }
        public string country { get; set; }
        public string region { get; set; }
    }
}
