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
    class clsInfoTouchPos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsInfoTouchPos(int StoreId, decimal tax)
        {
            try
            {
                InfoTouchPOSConvertRawFile(StoreId, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static DataTable ConvertTextToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("ProductName", typeof(string));
            dtResult.Columns.Add("Sku", typeof(string));
            dtResult.Columns.Add("Upc", typeof(string));
            dtResult.Columns.Add("Quantity", typeof(decimal));
            dtResult.Columns.Add("Ignore", typeof(int));
            dtResult.Columns.Add("Price", typeof(decimal));

            using (TextFieldParser parser = new TextFieldParser(FileName))
            {
                try
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    int i = 1;
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

                            dtResult.Columns["ignore"].DataType = typeof(decimal);
                            dtResult.Columns["Price"].DataType = typeof(string);
                            string[] rows = parser.ReadFields();
                            dtResult.Rows.Add();
                            int c = 0;
                            foreach (string row in rows)
                            {
                                if (c > 5)
                                    break;
                                var roww = row.Replace('"', ' ').Trim();

                                dtResult.Rows[r][c] = roww.ToString();
                                c++;
                            }
                            r++;
                        }
                        i++;
                    }
                }
                catch(Exception ex)
                {

                }
               
            }
            return dtResult; //Returning Dattable  
        }
        public string InfoTouchPOSConvertRawFile(int StoreId, decimal tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myfile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();
                        string Url = BaseUrl + "/" + StoreId + "/Raw/" + myfile;
                        if (File.Exists(Url))
                        {
                            try
                            {
                                ProductsModel pmsk = new ProductsModel();
                                List<ProductsModel> listProduct = new List<ProductsModel>();
                                FullnameModel full = new FullnameModel();
                                List<FullnameModel> listfull = new List<FullnameModel>();

                                DataTable dt = ConvertTextToDataTable(Url);
                                foreach (DataRow dr in dt.Rows)
                                {
                                    pmsk = new ProductsModel();
                                    full = new FullnameModel();
                                    pmsk.StoreID = StoreId;
                                    if (!string.IsNullOrEmpty(dr["Sku"].ToString()))
                                    {
                                        pmsk.sku = "#" + dr.Field<string>("Sku");
                                        full.sku = "#" + dr.Field<string>("Sku");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (!string.IsNullOrEmpty(dr["Upc"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr.Field<string>("Upc");
                                        full.upc = "#" + dr.Field<string>("Upc");
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.Price = System.Convert.ToDecimal(dr["Price"] == "" ? 0 : dr["Price"]);
                                    full.Price = System.Convert.ToDecimal(dr["Price"] == "" ? 0 : dr["Price"]);
                                    if (pmsk.Price <= 0 || full.Price <= 0)
                                    {
                                        continue;
                                    }

                                    pmsk.StoreProductName = dr.Field<string>("ProductName").Trim();
                                    pmsk.StoreDescription = dr.Field<string>("ProductName").Trim();
                                    full.pname = dr.Field<string>("ProductName").Trim();
                                    full.pdesc = dr.Field<string>("ProductName").Trim();
                                    full.pack = 1;
                                    pmsk.Qty = Convert.ToInt32(dr["Quantity"]) > 0 ? Convert.ToInt32(dr["Quantity"]) : 0;
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    pmsk.sprice = 0;
                                    pmsk.Tax = tax;
                                    pmsk.Start = "";
                                    pmsk.End = "";
                                    pmsk.pack = 1;
                                    listProduct.Add(pmsk);
                                    listfull.Add(full);
                                }

                                Console.WriteLine("Generating InfoTouchPOS " + StoreId + " Product CSV Files.....");
                                Console.WriteLine("Generating InfoTouchPOS " + StoreId + " Fullname CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(listProduct, "PRODUCT", StoreId, BaseUrl);
                                string filename1 = GenerateCSV.GenerateCSVFile(listfull, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For InfoTouchPOS" + StoreId);
                                Console.WriteLine("Fullname File Generated For InfoTouchPOS" + StoreId);

                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                                return "Not Generated file for InfoTouchPOS" + StoreId;
                            }
                        }
                    }
                    else
                    {
                        //(new clsEmail()).sendEmail(DeveloperId, "", "", "No RAW File@" + StoreId + DateTime.UtcNow + " GMT", "No RAW file found" + "<br/>" + "");
                        //return ("Invalid FileName or Raw file Empty!" + StoreId);
                    }
                }
                else
                {
                    return "Invalid sub-Directory " + StoreId;
                }
            }
            else
            {
                return "Invalid Directory" + StoreId;
            }
            return "Completed generating Files ";
        }
    }
}
