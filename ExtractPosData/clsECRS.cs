using ExtractPosData.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExtractPosData
{
    class clsECRS
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        public clsECRS(string PosFileName, int StoreId, decimal tax,decimal liquortax,decimal liquortaxperlit)
        {
            EcrsConvertRawFile(PosFileName, StoreId, tax, liquortax, liquortaxperlit);
        }
        public string EcrsConvertRawFile(string PosFileName, int StoreId, decimal tax,decimal liquortax,decimal liquortaxperlit)
        {
            int rowcount =0;
            List<ProductModelEcrs> prodlist = new List<ProductModelEcrs>();
            List<FullFileModel> fulllist = new List<FullFileModel>();
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
                            dt.Columns.Add("inv_pk");
                            dt.Columns.Add("dpt_name");
                            dt.Columns.Add("inv_scancode");
                            dt.Columns.Add("brd_name");
                            dt.Columns.Add("inv_name");
                            dt.Columns.Add("inv_size");
                            dt.Columns.Add("inv_receiptalias");
                            dt.Columns.Add("sib_baseprice");
                            dt.Columns.Add("inv_onhand");
                            dt.Columns.Add("sil_lastsold");
                            dt.Columns.Add("pi1_description");
                            dt.Columns.Add("pi1_description2");
                            dt.Columns.Add("pi1_description3");
                            dt.Columns.Add("pi1_description4");
                            dt.Columns.Add("pi1_description5");                            
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
                                            //if (i == 0)
                                            //{
                                            //    for (int j = 0; j < rowValues.Count(); j++)
                                            //    {
                                            //        dt.Columns.Add(rowValues[j]); //add headers  
                                            //    }
                                            //}
                                            //else
                                            //{
                                                DataRow dr = dt.NewRow();
                                                for (int k = 0; k < rowValues.Count(); k++)
                                                {
                                                    
                                                    dr[k] = rowValues[k].ToString();

                                                }
                                                dt.Rows.Add(dr); //add other rows  
                                           // }
                                        }
                                    }
                                }
                            }
                            //var dtr = from s in dt.AsEnumerable() select s;
                            int count = 0;
                            //string tmp;
                            foreach (DataRow dr in dt.Rows)
                            {
                                FullFileModel fpd = new FullFileModel();
                                string deptname = dr["dpt_name"].ToString();                          
                                count++;
                                //Console.WriteLine(count);
                                ProductModelEcrs pd = new ProductModelEcrs();
                                pd.StoreID = StoreId;
                                pd.upc = "#" + dr["inv_scancode"].ToString();
                                rowcount++;
                                //dr.Field<string>("inv_onhand");//.Replace("\"","");
                                //pd.Qty = System.Convert.ToDecimal(dr[7] == DBNull.Value || dr[7] == " " ? "0.0" : dr[7]); 
                                decimal y;
                                pd.Qty = Decimal.TryParse(dr["inv_onhand"].ToString(), out y) ? y : 0;
                                pd.sku = "#"+dr["inv_pk"].ToString();

                                pd.StoreProductName = dr.Field<string>("pi1_description").Trim()+" "+dr.Field<string>("inv_name").Trim();
                                pd.StoreDescription = dr.Field<string>("pi1_description").Trim()+" "+dr.Field<string>("inv_name").Trim();
                                pd.size=dr.Field<string>("inv_size").Trim();
                                decimal x;
                                pd.price = Decimal.TryParse(dr["sib_baseprice"].ToString(), out x) ? x : 0; //System.Convert.ToDecimal(dr["sib_baseprice"] == DBNull.Value ? 0 : dr["sib_baseprice"]);
                                pd.sprice = 0;
                                pd.Start = "";
                                pd.End = "";
                                //if (pd.sku == "#58413")
                                //{
                                //    Console.WriteLine(count);
                                //}                              
                                if (deptname.StartsWith("Z-"))
                                {
                                    if (!string.IsNullOrEmpty(pd.size))
                                    {
                                        //string val = pd.Size.Substring(pd.Size.Length - 1).ToUpper();
                                        if (pd.price != 0)
                                        {
                                            if (pd.size.Substring(pd.size.Length - 1).ToUpper() == "L")
                                            {
                                                if (pd.size.Substring(pd.size.Length - 2).ToUpper() == "ML")
                                                {
                                                    pd.uom = pd.size.Substring(pd.size.Length - 2).ToUpper();
                                                    //string val = pd.Size.Substring(0, pd.Size.Length - 2).Trim();
                                                    pd.Tax = (liquortax * pd.price + Convert.ToDecimal(String.Concat(pd.size.Where(p => p == '.' || Char.IsDigit(p)))) / 1000 * liquortaxperlit) / pd.price;
                                                }
                                                else if (pd.size.Length > 6)
                                                {
                                                    if (pd.size.Substring(pd.size.Length - 7).ToUpper() == "OZ FILL")
                                                    {
                                                        pd.uom = "OZ";
                                                        pd.Tax = (liquortax * pd.price + Convert.ToDecimal(String.Concat(pd.size.Where(p => p == '.' || Char.IsDigit(p)))) * Convert.ToDecimal(0.0295735296875) * liquortaxperlit) / pd.price;

                                                    }
                                                }
                                                else if (pd.size.Length > 3)
                                                {
                                                    if (pd.size.Substring(pd.size.Length - 3).ToUpper() == "MKL")
                                                    {
                                                        pd.uom = "MKL";
                                                        pd.Tax = (liquortax * pd.price + Convert.ToDecimal(String.Concat(pd.size.Where(p => p == '.' || Char.IsDigit(p)))) / 1000 * liquortaxperlit) / pd.price;

                                                    }
                                                }
                                                else
                                                {
                                                    pd.uom = pd.size.Substring(pd.size.Length - 1).ToUpper();
                                                    string pr = String.Concat(pd.size.Where(p => p == '.' || Char.IsDigit(p))).Trim();
                                                    pd.Tax = liquortax; 
                                                }
                                            }
                                            else if (pd.size.Substring(pd.size.Length - 2).ToUpper() == "OZ")
                                            {
                                                pd.uom = "OZ";
                                                pd.Tax = (liquortax * pd.price + Convert.ToDecimal(String.Concat(pd.size.Where(p => p == '.' || Char.IsDigit(p)))) * Convert.ToDecimal(0.0295735296875) * liquortaxperlit) / pd.price;
                                            }
                                            else if (pd.size.Length > 6)
                                            {
                                                if (pd.size.Substring(pd.size.Length - 6).ToUpper() == "GALLON")
                                                {

                                                    //tmp = pd.Size.Substring(0, pd.Size.Length - 6).Trim();
                                                    pd.Tax = tax; //(liquortax * pd.Price + Convert.ToDecimal((String.Concat(pd.Size.Where(p => p == '.' || Char.IsDigit(p))))) / 1000 * Convert.ToDecimal(3.78541) * liquortaxperlit) / pd.Price;
                                                }
                                                else if (pd.size.Substring(pd.size.Length - 7).ToUpper() == "GALLONS")
                                                {
                                                    pd.Tax = tax;//(liquortax * pd.Price + Convert.ToDecimal((String.Concat(pd.Size.Where(p => p == '.' || Char.IsDigit(p))))) / 1000 * Convert.ToDecimal(3.78541) * liquortaxperlit) / pd.Price;
                                                }
                                                
                                            }
                                            else if (pd.size.Substring(pd.size.Length - 1).ToUpper() == "M")
                                            {

                                                pd.Tax = (liquortax * pd.price + Convert.ToDecimal(pd.size.Substring(0, pd.size.Length - 1).Trim()) / 1000 * liquortaxperlit) / pd.price;
                                            }
                                            else if (pd.size.Substring(pd.size.Length - 1).ToUpper() == "G")
                                            {
                                                pd.Tax = tax;
                                            }
                                            else if (pd.size.Length > 3)
                                            {
                                                if (pd.size.Substring(pd.size.Length - 3).ToUpper() == "GAL")
                                                {
                                                    pd.Tax = tax;
                                                }
                                            }
                                            else
                                            {
                                                decimal val;
                                                string v = String.Concat(pd.size.Where(p => p == '.' || Char.IsDigit(p)));
                                                Decimal.TryParse(v, out val);
                                                pd.Tax = (liquortax * pd.price + val / 1000 * liquortaxperlit) / pd.price;

                                            }
                                            if (pd.Tax == 0)
                                            {
                                              //  pd.Tax = (liquortax * pd.Price + Convert.ToDecimal(String.Concat(pd.Size.Where(p => p == '.' || Char.IsDigit(p)))) / 1000 * liquortaxperlit) / pd.Price;
                                            }

                                        }
                                        else {
                                            pd.Tax = 0; 
                                        }
                                    }
                                }
                                else
                                {
                                    if(!string.IsNullOrEmpty(pd.size.Trim())){
                                        
                                    if (pd.size.Substring((pd.size.Length < 1 ? 1 : pd.size.Length) - 1).ToUpper() == "L")
                                    {
                                        if (pd.size.Substring(pd.size.Length - 2).ToUpper() == "ML")
                                        {
                                            pd.uom = pd.size.Substring(pd.size.Length - 2).ToUpper();
                                        }
                                        else
                                        {
                                            pd.uom = pd.size.Substring(pd.size.Length - 1).ToUpper();
                                        }
                                    }
                                    else if (pd.size.Substring((pd.size.Length < 2 ? 2 : pd.size.Length) - 2).ToUpper() == "OZ")
                                    {
                                        pd.uom = "OZ";                           
                                    }

                                    else if (pd.size.Length > 6)
                                    {
                                        if (pd.size.Substring((pd.size.Length < 6 ? 6 : pd.size.Length) - 6).ToUpper() == "GALLON")
                                        {
                                            pd.uom = "GALLON";
                                        }
                                    }
                                    else
                                    {
                                        pd.uom = "";
                                    }
                                }
                                    pd.Tax = tax;
                                }
                                //pd.altupc1 = "";
                                //pd.altupc2 = "";
                                //pd.altupc3 = "";
                                //pd.altupc4 = "";
                                //pd.altupc5 = "";
                                pd.Tax = Math.Round(pd.Tax, 5);
                                fpd.StoreID = pd.StoreID;
                                fpd.sku = pd.sku;
                                fpd.prodname = pd.StoreProductName;
                                fpd.pcat = deptname;
                                fpd.upc = pd.upc;
                                fpd.pack = pd.pack;
                                fpd.price = pd.price;
                                fpd.descript = pd.StoreDescription;
                                if (pd.Qty > 0 && pd.price > 0)
                                {
                                    prodlist.Add(pd);
                                    fulllist.Add(fpd);
                                }
                            }

                            Console.WriteLine("Generating ECRS " + StoreId + " Product CSV Files.....");
                            Console.WriteLine("Generating ECRS " + StoreId + " FullFile CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseURL);
                            string fullfilename = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseURL);
                                
                        }
                        catch (Exception e)
                        {
                         Console.WriteLine("" + e.Message);
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                            return "Not Generated File for ECRS " + StoreId;
                        }
                        
                    }
                }
                
            }
            Console.WriteLine("Product File Generated For ECRS (10196)");
            Console.WriteLine("FullName File Generated For ECRS (10196)");
            return "";
        }
    }
    public class FullFileModel {

        public int StoreID { get; set; }
        public string sku { get; set; }
        public int pack { get; set; }
        public string prodname { get; set; }
        public string descript { get; set; }
        public string pcat { get; set; }
        public string upc { get; set; }
        public decimal price { get; set; }
    }
}
