using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ExtractPosData.Models;
using System.IO;
using System.Data.OleDb;
using Microsoft.VisualBasic.FileIO;
using System.Configuration;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExtractPosData
{
    public class clsMicrobiz
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsMicrobiz(string FileName, string FileName2, int StoreId, decimal Tax)
        {
            try
            {
                MicrobizConvertRawFile(FileName, FileName2, StoreId, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            return dtResult; //Returning Dattable  
        }

        public static DataTable ConvertCsvToDataTable2(string FileName2)
        {
            DataTable dtResult = new DataTable();
            using (TextFieldParser parser = new TextFieldParser(FileName2))
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
        #region
        //public string MicrobizConvertRawFile(string PosFileName,string PosFileName2, int StoreId, decimal Tax)
        //{
        //    if (Directory.Exists(BaseUrl))
        //    {
        //        if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
        //        {
        //            var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
        //            if (directory.GetFiles().FirstOrDefault() != null)
        //            {
        //                var myFile = (from f in directory.GetFiles()
        //                              orderby f.LastWriteTime descending
        //                              select f).First();

        //                string Url = BaseUrl + "/" + StoreId + "/Raw/" + PosFileName;
        //                string Url2 = BaseUrl + "/" + StoreId + "/Raw/" + PosFileName2;

        //                if (File.Exists(Url))
        //                {
        //                    try
        //                    {
        //                        DataTable dt = ConvertCsvToDataTable(Url);
        //                        DataTable dt2 = ConvertCsvToDataTable2(Url2);

        //                        DataTable data = new DataTable();

        //                        //dt.Merge(dt2);

        //                       // var dtr = from s in dt.AsEnumerable() select s;
        //                        List<ProductsModel> prodlist = new List<ProductsModel>();
        //                        List<FullnameModel> full = new List<FullnameModel>();

        //                        foreach (DataRow dr in dt.Rows)
        //                        {
        //                            try
        //                            {
        //                                ProductsModel pmsk = new ProductsModel();
        //                                FullnameModel fname = new FullnameModel();

        //                                pmsk.StoreID = StoreId;
        //                                if (!string.IsNullOrEmpty(dr["upc"].ToString()))
        //                                {
        //                                    pmsk.upc = "#"+dr["upc"].ToString();
        //                                    fname.upc = "#" + dr["upc"].ToString();
        //                                    pmsk.sku = "#" + dr["upc"].ToString();
        //                                    fname.sku = "#" + dr["upc"].ToString();
        //                                }
        //                                else
        //                                {
        //                                    continue;
        //                                }
        //                                String qty = Regex.Replace(dr["qty"].ToString(), @"[^0-9]+", "");

        //                                if (!string.IsNullOrEmpty(qty))
        //                                {
        //                                    var qtyy = Convert.ToInt32(qty);
        //                                    pmsk.Qty = System.Convert.ToInt32(qtyy);
        //                                }
        //                               // pmsk.Qty = System.Convert.ToInt32(dr["qty"] == DBNull.Value ? 0 : dr["qty"]);
        //                                pmsk.StoreProductName = dr.Field<string>("StoreProductName");
        //                                fname.pname = dr.Field<string>("StoreProductName");
        //                                pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
        //                                fname.pdesc = dr.Field<string>("StoreProductName");
        //                                pmsk.Price = System.Convert.ToDecimal(dr["price"].ToString().Replace("$", string.Empty));
        //                                fname.Price = System.Convert.ToDecimal(dr["price"].ToString().Replace("$", string.Empty));

        //                                //else
        //                                //{
        //                                pmsk.sprice = 0;
        //                                //}
        //                                //pmsk.sprice = 0;
        //                                pmsk.pack = Convert.ToInt32(dr["pack"]);
        //                                pmsk.Tax = Tax;
        //                                //if (pmsk.sprice > 0)
        //                                //{
        //                                //    pmsk.Start = dr["Sale Start Date"].ToString();
        //                                //    pmsk.End = dr["Sale End Date"].ToString();
        //                                //}
        //                                //else
        //                                //{
        //                                pmsk.Start = "";
        //                                pmsk.End = "";
        //                                // }
        //                                fname.pack = Convert.ToInt32(dr["pack"]);
        //                                fname.pcat = dr["cat"].ToString();
        //                                fname.pcat1 = "";
        //                                fname.pcat2 = "";
        //                                fname.uom = dr.Field<string>("size");
        //                                fname.region = "";
        //                                fname.country = "";

        #endregion
        public string MicrobizConvertRawFile(string PosFileName, string PosFileName2, int StoreId, decimal Tax)
        {
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

                        string Url = BaseUrl + "/" + StoreId + "/Raw/" + PosFileName;
                        string Url2 = BaseUrl + "/" + StoreId + "/Raw/" + PosFileName2;

                        if (File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = ConvertCsvToDataTable(Url);
                                DataTable dt2 = ConvertCsvToDataTable2(Url2);

                                List<MicProductsModel> prodlist1 = new List<MicProductsModel>();
                                List<MicFullnameModel> full = new List<MicFullnameModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    MicProductsModel pmsk = new MicProductsModel();
                                    MicFullnameModel fname = new MicFullnameModel();

                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["upc"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["upc"].ToString();
                                        fname.upc = "#" + dr["upc"].ToString();
                                        pmsk.sku = "#" + dr["upc"].ToString();
                                        fname.sku = "#" + dr["upc"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    String qty = (dr["qty"].ToString());

                                    if (!string.IsNullOrEmpty(qty))
                                    {
                                        var qtyy = Convert.ToDecimal(qty);
                                        pmsk.Qty = System.Convert.ToInt32(qtyy);  // > 0 ? Convert.ToInt32(qtyy) : 0;
                                    }
                                    pmsk.StoreProductName = dr.Field<string>("StoreProductName");
                                    fname.pname = dr.Field<string>("StoreProductName");
                                    pmsk.StoreDescription = dr.Field<string>("StoreProductName").Trim();
                                    fname.pdesc = dr.Field<string>("StoreProductName");
                                    if (!string.IsNullOrEmpty(dr["price"].ToString()))
                                    {
                                        pmsk.Price = System.Convert.ToDecimal(dr["price"].ToString().Replace("$", string.Empty));
                                        fname.Price = System.Convert.ToDecimal(dr["price"].ToString().Replace("$", string.Empty));
                                    }
                                    if (pmsk.Price <= 0 || fname.Price <= 0)
                                    {
                                        continue;
                                    }
                                    pmsk.sprice = 0;
                                    pmsk.pack = Convert.ToInt32(dr["pack"]);
                                    pmsk.Tax = Tax;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.altupc1 = dr["cat"].ToString();
                                    pmsk.altupc2 = dr.Field<string>("size");
                                    fname.pack = Convert.ToInt32(dr["pack"]);
                                    fname.pcat = dr["cat"].ToString();
                                    fname.pcat1 = "";
                                    fname.pcat2 = "";
                                    fname.uom = dr.Field<string>("size");
                                    fname.region = "";
                                    fname.country = "";
                                    prodlist1.Add(pmsk);
                                    full.Add(fname);
                                }

                                List<MicProductsModel> prodlist2 = new List<MicProductsModel>();
                                List<MicFullnameModel> full2 = new List<MicFullnameModel>();

                                foreach (DataRow dr2 in dt2.Rows)
                                {
                                    MicProductsModel pmsk2 = new MicProductsModel();
                                    MicFullnameModel fname2 = new MicFullnameModel();

                                    pmsk2.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr2["upc"].ToString()))
                                    {
                                        pmsk2.upc = "#" + dr2["upc"].ToString();
                                        fname2.upc = "#" + dr2["upc"].ToString();
                                        pmsk2.sku = "#" + dr2["upc"].ToString();
                                        fname2.sku = "#" + dr2["upc"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    String qty2 = dr2["qty"].ToString();

                                    if (!string.IsNullOrEmpty(qty2))
                                    {
                                        var qtyy = Convert.ToDecimal(qty2);
                                        pmsk2.Qty = System.Convert.ToInt32(qtyy);  //> 0 ? Convert.ToInt32(qtyy) : 0;
                                    }
                                    pmsk2.StoreProductName = dr2.Field<string>("StoreProductName");
                                    fname2.pname = dr2.Field<string>("StoreProductName");
                                    pmsk2.StoreDescription = dr2.Field<string>("StoreProductName").Trim();
                                    fname2.pdesc = dr2.Field<string>("StoreProductName");
                                    if (!string.IsNullOrEmpty(dr2["price"].ToString()))
                                    {
                                        pmsk2.Price = System.Convert.ToDecimal(dr2["price"].ToString().Replace("$", string.Empty));
                                        fname2.Price = System.Convert.ToDecimal(dr2["price"].ToString().Replace("$", string.Empty));
                                    }

                                    pmsk2.sprice = 0;

                                    pmsk2.pack = Convert.ToInt32(dr2["pack"]);
                                    pmsk2.Tax = Tax;

                                    pmsk2.Start = "";
                                    pmsk2.End = "";
                                    pmsk2.altupc1 = dr2["cat"].ToString();
                                    pmsk2.altupc2 = dr2.Field<string>("size");
                                    fname2.pack = Convert.ToInt32(dr2["pack"]);
                                    fname2.pcat = dr2["cat"].ToString();
                                    fname2.pcat1 = "";
                                    fname2.pcat2 = "";
                                    fname2.uom = dr2.Field<string>("size");
                                    fname2.region = "";
                                    fname2.country = "";
                                    prodlist2.Add(pmsk2);
                                    full2.Add(fname2);
                                }

                                var prodList = (from a in prodlist1
                                                join b in prodlist2 on a.upc equals b.upc

                                                select new
                                                {
                                                    storeid = StoreId,
                                                    upc = b.upc == "" || b.upc == null || b.upc == "null" ? "" : b.upc.ToString(),
                                                    qty =Convert.ToInt32(a.Qty + b.Qty) > 0 ? Convert.ToInt32(a.Qty + b.Qty):0,                                                   
                                                    sku = b.sku == null ? "" : b.sku,
                                                    pack = 1,
                                                    StoreProductName = b.StoreProductName,
                                                    StoreDescription = b.StoreDescription,
                                                    price = a.Price,
                                                    sprice = 0,
                                                    start = "",
                                                    end = "",
                                                    tax = b.Tax,
                                                    altupc1 = a.altupc1,
                                                    altupc2 = a.altupc2,
                                                    altupc3 = "",
                                                    altupc4 = "",
                                                    altupc5 = "",

                                                }).Distinct().Select(x => new MicProductsModel()
                                                {
                                                    StoreID = x.storeid,
                                                    upc = x.upc,
                                                    Qty = Convert.ToInt64(x.qty),
                                                    sku = x.sku,
                                                    pack = x.pack,
                                                    StoreProductName = x.StoreProductName,
                                                    StoreDescription = x.StoreDescription,
                                                    Price = Convert.ToDecimal(x.price),
                                                    sprice = x.sprice,
                                                    Start = x.start,
                                                    End = x.end,
                                                    Tax = x.tax,
                                                    altupc1 = x.altupc1,
                                                    altupc2 = x.altupc2,
                                                    altupc3 = x.altupc3,
                                                    altupc4 = x.altupc4,
                                                    altupc5 = x.altupc5
                                                }).ToList();

                                var fullNameList = (from a in prodList
                                                    select new
                                                    {
                                                        pname = a.StoreProductName,
                                                        pdesc = a.StoreDescription,
                                                        upc = a.upc == "" || a.upc == null || a.upc == "null" ? "" : a.upc.ToString(),
                                                        sku = a.sku == null ? "" : a.sku,
                                                        pack = 1,
                                                        price = a.Price,
                                                        uom = a.altupc2,
                                                        pcat = a.altupc1,
                                                        pcat1 = "",
                                                        pcat2 = "",
                                                        country = "",
                                                        region = ""
                                                    }).Distinct().Select(x => new MicFullnameModel()
                                                    {
                                                        pname = x.pname,
                                                        pdesc = x.pdesc,
                                                        upc = x.upc,
                                                        sku = x.sku,
                                                        pack = x.pack,
                                                        Price = Convert.ToDecimal(x.price),
                                                        uom = x.uom,
                                                        pcat = x.pcat,
                                                        pcat1 = x.pcat1,
                                                        pcat2 = x.pcat2,
                                                        country = x.country,
                                                        region = x.region
                                                    }).ToList();
                           

                                Console.WriteLine("Generating Microbiz " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodList, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For Microbiz " + StoreId);
                                Console.WriteLine();
                                Console.WriteLine("Generating Microbiz " + StoreId + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(fullNameList, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For Microbiz " + StoreId);

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
                                return "Not generated file for Microbiz " + StoreId;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Ínvalid FileName or RAW folder is empty!" + StoreId);
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
    public class MicProductsModel
    {
        public int StoreID { get; set; }
        public string upc { get; set; }
        public Int64 Qty { get; set; }
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
        public decimal Deposit { get; set; }
    }
    public class MicFullnameModel
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

