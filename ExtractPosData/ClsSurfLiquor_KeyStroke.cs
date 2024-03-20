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

namespace ExtractPosData
{
    class ClsSurfLiquor_KeyStroke
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public ClsSurfLiquor_KeyStroke(string FileName, int StoreId, decimal Tax)
        {
            try
            {
                SurfLiquorConvertRawFile(FileName, StoreId, Tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertTextToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            using (TextFieldParser parser = new TextFieldParser(FileName))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters("\t");

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
                            var roww = row.Replace(" , ", " ").Trim()
                                            .Replace(",", "").Trim()
                                               .Replace("$", string.Empty).Trim()
                                                  .Replace("/", string.Empty).Trim()
                                                  .Replace("000 SPECTACULAR", "0").Trim();

                            dtResult.Rows[r][c] = roww.ToString();

                            c++;
                        }
                        r++;
                    }
                    i++;
                }
            }
            return dtResult;
        }
        public string SurfLiquorConvertRawFile(string PosFileName, int StoreId, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    string Url = BaseUrl + "/" + StoreId + "/Raw/" + PosFileName;
                    if (File.Exists(Url))
                    {
                        try
                        {
                            DataTable dt = ConvertTextToDataTable(Url);


                            var dtr = from s in dt.AsEnumerable() select s;
                            List<ProductModel> prodlist = new List<ProductModel>();
                            foreach (DataRow dr in dt.Rows)
                            {
                                ProductModel pmsk = new ProductModel();
                                pmsk.StoreID = StoreId;
                                if (!string.IsNullOrEmpty(dr["Product Code"].ToString()))
                                {
                                    pmsk.upc = "#" + dr.Field<string>("Product Code").ToString();
                                }
                                else
                                {
                                    pmsk.upc = dr.Field<string>("Product Code").ToString();
                                }
                                pmsk.Qty = System.Convert.ToDecimal(dr["QOH"] == DBNull.Value ? 0 : dr["QOH"]);
                                pmsk.sku = "#" + dr.Field<string>("Stock#");
                                if (!string.IsNullOrEmpty(dr.Field<string>("Description")))
                                {
                                    //pmsk.StoreProductName = dr.Field<string>("Description");
                                    if (dr.Field<string>("Description").Contains("*"))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Description");
                                    }
                                    else
                                    { continue; }

                                }
                                else
                                {
                                    continue;
                                }
                                if (dr.Field<string>("Description").Contains("*"))
                                {
                                    pmsk.StoreDescription = dr.Field<string>("Description");
                                }
                                else
                                { continue; }
                                decimal price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                if (price > 0)
                                {
                                    pmsk.Price = price;
                                }
                                else
                                {
                                    continue;
                                }
                                pmsk.sprice = 0;
                                pmsk.pack = 1;
                                pmsk.Tax = Tax;
                                pmsk.Start = "";
                                pmsk.End = "";
                                pmsk.altupc1 = "";
                                pmsk.altupc2 = "";
                                pmsk.altupc3 = "";
                                pmsk.altupc4 = "";
                                pmsk.altupc5 = "";
                                if (pmsk.Qty > 0)
                                {
                                    prodlist.Add(pmsk);
                                }
                            }
                            Console.WriteLine("Generating SurfLiquor " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For SurfLiquor (10284)");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("" + e.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                            return "Not generated file for" + StoreId;
                        }
                    }
                    else
                    {
                        return "Ínvalid FileName Or Raw Folder is Empty! " + StoreId;
                    }
                }
                else
                {
                    return "Invalid Sub-Directory" + StoreId;
                }
            }
            else
            {
                return "Invalid Directory";
            }
            return "Completed generating File";
        }

    }
}
