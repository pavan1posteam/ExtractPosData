using ExtractPosData.Models;
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
    public class clsKeyStroke
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsKeyStroke(string FileName, int StoreId, decimal Tax)
        {
            try
            {
                string val = ConvertRawFile(FileName, StoreId, Tax);
                Console.WriteLine(val);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private string ConvertRawFile(string PosFileName, int StoreId, decimal tax)
        {
            string BaseURL = ConfigurationManager.AppSettings.Get("BaseDirectory");

            if (Directory.Exists(BaseURL))
            {
                if (Directory.Exists(BaseURL + "/" + StoreId + "/Raw/"))
                {
                    string Url = BaseURL + "/" + StoreId + "/Raw/" + PosFileName;
                    if (File.Exists(Url))
                    {
                        try
                        {
                            DataTable dt = new DataTable();
                            string Fulltext;
                            using (StreamReader reader = new StreamReader(Url))
                            {
                                while (!reader.EndOfStream)
                                {
                                    Fulltext = reader.ReadToEnd().ToString(); //read full file text  
                                    string[] rows = Fulltext.Split('\n'); //split full file text into rows  
                                    for (int i = 0; i < rows.Count() - 1; i++)
                                    {
                                        string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values  
                                        {
                                            if (i == 0)
                                            {
                                                for (int j = 0; j < rowValues.Count(); j++)
                                                {
                                                    dt.Columns.Add(rowValues[j]); //add headers  
                                                }
                                            }
                                            else
                                            {
                                                DataRow dr = dt.NewRow();
                                                for (int k = 0; k < rowValues.Count(); k++)
                                                {
                                                    dr[k] = rowValues[k].ToString();
                                                }
                                                dt.Rows.Add(dr); //add other rows  
                                            }
                                        }
                                    }
                                }
                            }
                            // restart:
                            var dtr = from s in dt.AsEnumerable() select s;
                            List<ProductModel> prodlist = new List<ProductModel>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                ProductModel pd = new ProductModel();
                                if (StoreId != 10870)
                                {

                                    pd.StoreID = StoreId;
                                    pd.upc = "#" + dr[4].ToString();
                                    var Qty = System.Convert.ToDecimal(dr["QOH"] == DBNull.Value ? 0 : dr["QOH"]);
                                    pd.Qty = Convert.ToInt32(Qty) > 0 ? Convert.ToInt32(Qty) : 0;
                                    pd.sku = "#" + dr.Field<string>("SKU");
                                    if (!string.IsNullOrEmpty(dr.Field<string>("Description")))
                                    {
                                        pd.StoreProductName = dr.Field<string>("Description");
                                    }
                                    else
                                    {
                                        continue;
                                        //goto restart;
                                    }
                                    pd.StoreDescription = "";
                                    pd.Price = System.Convert.ToDecimal(dr["Price"] == DBNull.Value ? 0 : dr["Price"]);
                                    if (pd.Price <= 0)
                                    {
                                        continue;
                                    }

                                    pd.sprice = 0;
                                    pd.Start = "";
                                    pd.End = "";
                                    pd.Tax = tax / 100;
                                    pd.altupc1 = "";
                                    pd.altupc2 = "";
                                    pd.altupc3 = "";
                                    pd.altupc4 = "";
                                    pd.altupc5 = "";
                                    prodlist.Add(pd);
                                }
                            }
                            Console.WriteLine("Generating KeyStroke " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseURL);
                            // mFtp ftp = new mFtp();
                            //ftp.Upload("Uploading" + filename);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("" + e.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                            return "Not Generated file for KeyStroke " + StoreId;

                        }
                    }
                    else
                    {
                        return "Invalid FileName or Raw Folder is Empty! " + StoreId;
                    }
                }
                else
                {
                    return "Invalid Sub Directory" + StoreId;
                }
            }
            else
            {
                return "Invalid Directory" + StoreId;
            }

            return "Completed Generating File of KeyStroke(10113)";
        }

    }
}
