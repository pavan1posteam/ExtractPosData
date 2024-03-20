using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using System.Configuration;
using System.IO;
using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;

namespace ExtractPosData
{
    class clsSigmansEcrs
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsSigmansEcrs(string FileName, int StoreId, decimal Tax)
        {
            try
            {
                SigmansConvertRawFile(FileName, StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        //previous code
        #region
        //public static DataTable ConvertCsvToDataTable(string FileName)
        //{
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
        //                foreach (string col in columns)
        //                {
        //                    dtResult.Columns.Add(col);
        //                }
        //            }
        //            else
        //            {
        //                string[] rows = parser.ReadFields();
        //                dtResult.Rows.Add();
        //                int c = 0;
        //                foreach (string row in rows)
        //                {
        //                    var roww = row.Replace('"', ' ').Trim().Replace("$", string.Empty);

        //                    dtResult.Rows[r][c] = roww.ToString();
        //                    c++;
        //                }

        //                r++;
        //            }
        //            i++;
        //        }
        //    }
        //    return dtResult; //Returning Dattable  
        //}
        //public string SigmansConvertRawFile(string PosFileName, int StoreId, decimal Tax)
        //{
        //    string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        //    if (Directory.Exists(BaseUrl))
        //    {
        //        if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
        //        {
        //            string Url = BaseUrl + "/" + StoreId + "/Raw/" + PosFileName;
        //            if (File.Exists(Url))
        //            {
        //                try
        //                {
        //                    DataTable dt = ConvertCsvToDataTable(Url);


        //                    var dtr = from s in dt.AsEnumerable() select s;
        //                    List<ProdModel> prodlist = new List<ProdModel>();
        //                    foreach (DataRow dr in dt.Rows)
        //                    {
        //                        ProdModel pmsk = new ProdModel();
        //                        pmsk.StoreID = StoreId;
        //                        if (!string.IsNullOrEmpty(dr["Item ID"].ToString()))
        //                        {
        //                            if (dr.Field<string>("Item Id").Contains('A'))
        //                            {
        //                                continue;
        //                            }
        //                            else
        //                            {
        //                                //pmsk.upc = "#" + dr.Field<string>("Item ID").ToString();
        //                                pmsk.upc = dr.Field<string>("Item ID").ToString();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            pmsk.upc = dr.Field<string>("Item ID").ToString();
        //                        }
        //                        var qty = dr["On Hand"];
        //                        if (qty != "")
        //                        {
        //                            pmsk.Qty = Convert.ToInt32(qty);
        //                        }
        //                        //pmsk.sku = "#" + dr.Field<string>("Item ID");
        //                        pmsk.sku = dr.Field<string>("Item ID");
        //                        if (!string.IsNullOrEmpty(dr.Field<string>("Receipt Alias")))
        //                        {
        //                            //pmsk.StoreProductName = dr.Field<string>("Receipt Alias").Replace("750Ml", "").Replace("750M", "").Replace("1.75Lit", "").Replace("1.75 Lit", "").Replace("25Oz", "")
        //                            //                        .Replace("1Ltr", "").Replace("375 Ml", "").Replace("100Ml", "").Replace("375Ml", "").Replace("200Ml", "").Replace("50Ml", "").Replace("200M", "")
        //                            //                        .Replace("375", "").Replace("Lit", "").Replace("750", "").Replace("1.75L", "").Replace("1.75Ml", "").Replace("1.75 Ltr", "").Replace("1.75Ltr", "")
        //                            //                        .Replace("1.75", "").Replace("Ml", "").Replace("2 50Ml", "").Replace("50 Ml", "").Replace("1 Lit", "").Replace("1.75Lt", "").Replace("1 Lt", "")
        //                            //                        .Replace("1Lt", "").Replace("50 Ml", "").Replace("1.75 L", "").Replace("375M", "").Replace("22Oz", "").Replace("6Pk", "").Replace("4Pk", "")
        //                            //                        .Replace("4Ltr", "").Replace("1.5Lt", "").Replace("1.5", "").Replace("1.5L", "");
        //                        }
        //                        else
        //                        {
        //                            continue;
        //                        }
        //                        pmsk.StoreProductName = dr.Field<string>("Receipt Alias");
        //                        //tckt 6302
        //                        pmsk.StoreDescription = dr.Field<string>("Receipt Alias").Replace("750Ml", "").Replace("750M", "").Replace("1.75Lit", "").Replace("1.75 Lit", "").Replace("25Oz", "")
        //                                                    .Replace("1Ltr", "").Replace("375 Ml", "").Replace("100Ml", "").Replace("375Ml", "").Replace("200Ml", "").Replace("50Ml", "").Replace("200M", "")
        //                                                    .Replace("375", "").Replace("Lit", "").Replace("750", "").Replace("1.75L", "").Replace("1.75Ml", "").Replace("1.75 Ltr", "").Replace("1.75Ltr", "")
        //                                                    .Replace("1.75", "").Replace("Ml", "").Replace("2 50Ml", "").Replace("50 Ml", "").Replace("1 Lit", "").Replace("1.75Lt", "").Replace("1 Lt", "")
        //                                                    .Replace("1Lt", "").Replace("50 Ml", "").Replace("1.75 L", "").Replace("375M", "").Replace("22Oz", "").Replace("6Pk", "").Replace("4Pk", "")
        //                                                    .Replace("4Ltr", "").Replace("1.5Lt", "").Replace("1.5", "").Replace("1.5L", "");
        //                        var pric = dr["Base Price"];
        //                        if (pric != "")
        //                        {
        //                            pmsk.Price = System.Convert.ToDecimal(dr["Base Price"] == DBNull.Value ? 0 : dr["Base Price"]);
        //                        }
        //                        pmsk.sprice = 0;
        //                        pmsk.pack = 1;
        //                        pmsk.Tax = Tax;
        //                        pmsk.Start = "";
        //                        pmsk.End = "";
        //                        pmsk.altupc1 = "";
        //                        pmsk.altupc2 = "";
        //                        pmsk.altupc3 = "";
        //                        pmsk.altupc4 = "";
        //                        pmsk.altupc5 = "";
        //                        if (pmsk.Qty > 0 && pmsk.Price > 0)
        //                        {
        //                            prodlist.Add(pmsk);
        //                        }
        //                    }
        #endregion
        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("dpt_name");
            dtResult.Columns.Add("upc");
            dtResult.Columns.Add("desc");
            dtResult.Columns.Add("inv_name");
            dtResult.Columns.Add("inv_size");
            dtResult.Columns.Add("desc2");
            dtResult.Columns.Add("price");
            dtResult.Columns.Add("baseprice");
            dtResult.Columns.Add("lastsolddate");
            dtResult.Columns.Add("sil_lastsold");
            dtResult.Columns.Add("size2");
            dtResult.Columns.Add("store");
            dtResult.Columns.Add("remark");

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
                        string LineValue = parser.ReadLine();
                        if (LineValue.IndexOf("'\'") > 0)
                        {
                            LineValue = LineValue.Replace("'\'", "#! ");
                        }
                        string[] rows = LineValue.Split('|');

                        dtResult.Rows.Add();
                        int c = 0;
                        foreach (string row in rows)
                        {
                            var roww = row.Replace("#!", "'\'");

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

        public string SigmansConvertRawFile(string PosFileName, int StoreId, decimal Tax)
        {
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
                                            pmsk.upc = "#" + dr["upc"].ToString();
                                            full.upc = "#" + dr["upc"].ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        decimal qty = Convert.ToDecimal(dr["baseprice"]);
                                        pmsk.Qty = Convert.ToInt32(qty);

                                        pmsk.sku = "#" + dr.Field<string>("upc");
                                        full.sku = "#" + dr.Field<string>("upc");

                                        if (!string.IsNullOrEmpty(dr.Field<string>("inv_name")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("inv_name").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("inv_name").Trim();
                                            full.pname = dr.Field<string>("inv_name").Trim();
                                            full.pdesc = dr.Field<string>("inv_name").Trim();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        pmsk.Price = System.Convert.ToDecimal(dr["price"] == DBNull.Value ? 0 : dr["price"]);
                                        full.Price = System.Convert.ToDecimal(dr["price"] == DBNull.Value ? 0 : dr["price"]);
                                        full.pcat = full.pdesc = dr.Field<string>("dpt_name").Trim();
                                        full.pcat1 = "";
                                        full.pcat2 = "";
                                        full.country = "";
                                        full.region = "";
                                        pmsk.sprice = 0;
                                        pmsk.pack = 1;
                                        full.pack = 1;
                                        full.uom = dr.Field<string>("inv_size");
                                        pmsk.Tax = Tax;

                                        pmsk.Start = "";
                                        pmsk.End = "";
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                        if (pmsk.StoreID == 11382)
                                        {
                                            fulllist.Add(full);
                                            prodlist.Add(pmsk);
                                        }
                                        else if (pmsk.Qty > 0 && pmsk.Price > 0)
                                        {
                                            fulllist.Add(full);
                                            prodlist.Add(pmsk);
                                        }
                                    }
                                    Console.WriteLine("Generating sigman ecrs " + StoreId + " Product CSV Files.....");
                                    Console.WriteLine("Generating sigman ecrs " + StoreId + " Fullname CSV Files.....");
                                    string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                    string filename1 = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                                    Console.WriteLine("Product File Generated For sigman ecrs" + StoreId);
                                    Console.WriteLine("Fullname File Generated For sigman ecrs" + StoreId);

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
                                    return "Not generated file for sigman ecrs " + StoreId;
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
                return "Completed generating File For sigman ecrs" + StoreId;
            }
        }
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
}
