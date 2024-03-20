using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtractPosData.Models;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ExtractPosData
{
    class clsRite_11396
    {
        public clsRite_11396(int storeId, decimal Tax)
        {
            try
            {
                RiteConvertRawFile(storeId, Tax);
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
            return dtResult; //Returning Dattable  
        }
        public string RiteConvertRawFile(int StoreId, decimal Tax)
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

                            var dtr = from s in dt.AsEnumerable() select s;
                            List<ProductModel> prodlist = new List<ProductModel>();

                            try
                            {

                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductModel pmsk = new ProductModel();
                                    pmsk.StoreID = StoreId;
                                    if (StoreId == 10355 || StoreId == 10356 || StoreId == 10545 || StoreId == 10584 || StoreId == 10687 || StoreId == 10688 || StoreId == 10690 || StoreId == 10691 || StoreId == 10841 || StoreId == 10352 || StoreId == 10232 || StoreId == 10477 || StoreId == 11203 || StoreId == 10850 || StoreId == 11396)
                                    {
                                        if (dr["UPC"].ToString().Contains("#"))
                                        {
                                            pmsk.upc = dr["UPC"].ToString().Trim().Replace("-", "");
                                            pmsk.sku = dr["UPC"].ToString().Trim().Replace("-", "");
                                        }
                                        else
                                        {
                                            pmsk.upc = '#' + dr["UPC"].ToString().Trim().Replace("-", "");
                                            pmsk.sku = '#' + dr["UPC"].ToString().Trim().Replace("-", "");
                                        }
                                        var qty = Convert.ToString(dr["qty"]);
                                        if (!string.IsNullOrEmpty(qty))
                                        {
                                            if (qty.Contains("E"))
                                            { continue; }
                                            var qty1 = Convert.ToDecimal(qty);
                                            pmsk.Qty = Convert.ToInt64(qty1);
                                        }
                                        else
                                        { continue; }
                                        pmsk.pack = 1;
                                        pmsk.StoreProductName = dr["StoreProductName"].ToString().Trim();
                                        pmsk.StoreDescription = dr["StoreProductName"].ToString().Trim();
                                        pmsk.Price = System.Convert.ToDecimal(dr["Price".ToLower()] == DBNull.Value ? 0 : dr["Price"].ToString() == "" ? 0: dr["Price".ToLower()]);
                                        if (StoreId == 10584 || StoreId == 10687 || StoreId == 10688 || StoreId == 10690 || StoreId == 10691 || StoreId == 10841 || StoreId == 10352 || StoreId == 10232 || StoreId == 10477 || StoreId == 11203 || StoreId == 10850)
                                        {
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
                                                pmsk.Start = dr["start"].ToString();
                                                pmsk.End = dr["end"].ToString();
                                            }
                                            else
                                            {
                                                pmsk.Start = "";
                                                pmsk.End = "";
                                            }
                                        }
                                        else
                                        {
                                            pmsk.sprice = 0;
                                            pmsk.Start = "";
                                            pmsk.End = "";
                                        }
                                        if (Tax == 0)
                                        {
                                            pmsk.Tax = Convert.ToDecimal(dr["tax"]);
                                        }
                                        else
                                        {
                                            pmsk.Tax = Tax;
                                        }
                                        pmsk.altupc1 = "";
                                        pmsk.altupc2 = "";
                                        pmsk.altupc3 = "";
                                        pmsk.altupc4 = "";
                                        pmsk.altupc5 = "";
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(dr["ItemLookupCode"].ToString()))
                                        {
                                            var upc = "#" + dr["ItemLookupCode"].ToString().ToLower();
                                            string numberUpc = Regex.Replace(upc, "[^0-9.]", "");
                                            if (numberUpc.Count() > 5)
                                            {
                                                if (!string.IsNullOrEmpty(numberUpc))
                                                {
                                                    pmsk.upc = "#" + numberUpc.Trim().ToLower();
                                                }
                                                else { continue; }
                                            }
                                            else { continue; }
                                        }
                                        else { continue; }
                                        pmsk.Qty = System.Convert.ToDecimal(dr["Quantity".ToLower()] == DBNull.Value ? 0 : dr["Quantity".ToLower()]);
                                        if (!string.IsNullOrEmpty(dr["ItemLookupCode"].ToString()))
                                        {
                                            var sku = "#" + dr.Field<string>("ItemLookupCode").ToLower();
                                            string numberSku = Regex.Replace(sku, "[^0-9.]", "");
                                            if (numberSku.Count() > 5)
                                            {
                                                if (!string.IsNullOrEmpty(numberSku))
                                                {
                                                    pmsk.sku = "#" + numberSku.Trim().ToLower();
                                                }
                                                else { continue; }
                                            }
                                            else { continue; }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        if (!string.IsNullOrEmpty(dr.Field<string>("Description")))
                                        {
                                            pmsk.StoreProductName = dr.Field<string>("Description").Trim();
                                            pmsk.StoreDescription = dr.Field<string>("Description").Trim();
                                        }
                                        else { continue; }
                                        pmsk.Price = System.Convert.ToDecimal(dr["Price".ToLower()] == DBNull.Value ? 0 : dr["Price".ToLower()]);
                                        if (!String.IsNullOrEmpty(dr.Field<string>("saleprice")))
                                        {
                                            pmsk.sprice = System.Convert.ToDecimal(dr["saleprice"] == DBNull.Value ? 0 : dr["saleprice"]);
                                            //pmsk.sprice = 0;
                                        }
                                        else
                                        {
                                            pmsk.sprice = 0;
                                        }

                                        pmsk.pack = 1;
                                        pmsk.Tax = Tax;
                                        if (pmsk.sprice > 0)
                                        {
                                            pmsk.Start = dr["salestartdate"].ToString();
                                            DateTime Startdate = DateTime.ParseExact(pmsk.Start.ToString(), "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                            pmsk.Start = Startdate.ToString("M/dd/yyyy", CultureInfo.InvariantCulture);
                                            pmsk.End = dr["saleenddate"].ToString();
                                            DateTime Enddate = DateTime.ParseExact(pmsk.End.ToString(), "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                            pmsk.End = Enddate.ToString("M/d/yyyy", CultureInfo.InvariantCulture);
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
                                    }


                                    if (pmsk.Qty > 0 && pmsk.Price > 0)
                                    {
                                        prodlist.Add(pmsk);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("" + e.Message);
                            }
                            Console.WriteLine("Generating Rite " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For Rite " + StoreId);

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
                            return "Not generated file for Rite " + StoreId;
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
            return "Completed generating File For Rite" + StoreId;
        }
    }
}
