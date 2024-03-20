using ExtractPosData.Models;
using Microsoft.VisualBasic.FileIO;
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
    class clsSpiritsFinewine_Keystroke
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsSpiritsFinewine_Keystroke(int StoreId, decimal tax)
        {
            try
            {
                KeystrokeConvertRawFile(StoreId, tax);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public DataTable ConvertTextToDataTable(string filename)                   /////tckt#7530- 15-04-2021
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("sku");
            dt.Columns.Add("upc");
            dt.Columns.Add("inv_scancode");
            dt.Columns.Add("name");
            dt.Columns.Add("listprice");
            dt.Columns.Add("baseprice");
            dt.Columns.Add("sprice");
            dt.Columns.Add("sib_baseprice");
            dt.Columns.Add("cat");
            dt.Columns.Add("qty");
            dt.Columns.Add("pi1_description");
            dt.Columns.Add("pi1_description2");
            dt.Columns.Add("pi1_description3");
            dt.Columns.Add("pi1_description4");
            dt.Columns.Add("pi1_description5");
            dt.Columns.Add("inv_onhand2");
            dt.Columns.Add("sil_lastsold2");
            dt.Columns.Add("pi1_description22");
            dt.Columns.Add("pi1_description222");
            dt.Columns.Add("pi1_description32");
            dt.Columns.Add("pi1_description42");
            dt.Columns.Add("pi1_description52");
            dt.Columns.Add("inv_onhand3");
            dt.Columns.Add("sil_lastsold3");
            dt.Columns.Add("pi1_description33");
            dt.Columns.Add("pi1_description23");
            dt.Columns.Add("pi1_description34");
            dt.Columns.Add("pi1_description435");
            dt.Columns.Add("pi1_description534");
            dt.Columns.Add("inv_onhand4");
            dt.Columns.Add("sil_lastsold4");
            dt.Columns.Add("pi1_description4444");
            dt.Columns.Add("pi1_description24444");
            dt.Columns.Add("pi1_description34444");
            dt.Columns.Add("pi1_description4444444");
            dt.Columns.Add("pi2_description54444");
            dt.Columns.Add("pi3_description4444");
            dt.Columns.Add("pi4_description24444");
            dt.Columns.Add("pi5_description34444");
            dt.Columns.Add("uom");
            dt.Columns.Add("pi7_description54444");
            dt.Columns.Add("pi8_description54444");
            dt.Columns.Add("pi9_description4444");
            dt.Columns.Add("pi10_description24444");
            dt.Columns.Add("pi11_description34444");
            dt.Columns.Add("pi12_description4444444");
            dt.Columns.Add("pi13_description54444");
            dt.Columns.Add("pi14_description54444");
            dt.Columns.Add("pi15_description4444");
            dt.Columns.Add("pi16_description24444");
            dt.Columns.Add("pi17_description34444");
            dt.Columns.Add("pi18_description4444444");
            dt.Columns.Add("pi19_description54444");
            dt.Columns.Add("pi43_description54444");
            dt.Columns.Add("pi44_description54444");
            dt.Columns.Add("pi45_description4444");
            dt.Columns.Add("pi46_description24444");
            dt.Columns.Add("pi47_description34444");
            dt.Columns.Add("pi48_description4444444");
            dt.Columns.Add("pi49_description54444");
            dt.Columns.Add("pi50_description5");
            dt.Columns.Add("pi51_description54");
            dt.Columns.Add("pi52_description4");
            dt.Columns.Add("pi53_description24");
            dt.Columns.Add("pi54_description3");
            dt.Columns.Add("pi55_description444");
            dt.Columns.Add("pi56_description544");
            dt.Columns.Add("pi57_description544");
            dt.Columns.Add("pi58_description54");
            dt.Columns.Add("pi59_description4");
            dt.Columns.Add("pi60_description24");
            dt.Columns.Add("pi61_description344");
            dt.Columns.Add("pi62_description444");
            dt.Columns.Add("pi63_description5");
            dt.Columns.Add("pi64_description544");
            dt.Columns.Add("pi65_description54");
            dt.Columns.Add("pi66_description4444");
            dt.Columns.Add("pi67_description24444");
            dt.Columns.Add("pi68_description34444");
            dt.Columns.Add("pi69_description4444444");
            dt.Columns.Add("pi70_description54444");
            dt.Columns.Add("pi71_description5");
            dt.Columns.Add("pi72_description54");
            dt.Columns.Add("pi73_description4");
            dt.Columns.Add("pi74_description24");
            dt.Columns.Add("pi75_description3");
            dt.Columns.Add("pi76_description444");
            dt.Columns.Add("pi77_description544");
            dt.Columns.Add("pi78_description544");
            dt.Columns.Add("pi79_description54");
            dt.Columns.Add("pi80_description4");
            dt.Columns.Add("pi81_description24");
            dt.Columns.Add("pi82_description344");
            dt.Columns.Add("pi83_description444");
            dt.Columns.Add("pi84_description5");
            dt.Columns.Add("pi85_description544");
            dt.Columns.Add("pi86_description54");
            dt.Columns.Add("pi87_description44");
            dt.Columns.Add("pi88_description244");
            dt.Columns.Add("pi89_description34");
            dt.Columns.Add("pi90_description44");
            dt.Columns.Add("pi91_description54");
            dt.Columns.Add("pi92_description24");
            dt.Columns.Add("pi93_description344");
            dt.Columns.Add("pi94_description444");
            dt.Columns.Add("pi95_description5");
            dt.Columns.Add("pi96_description544");
            dt.Columns.Add("pi97_description54");
            dt.Columns.Add("pi98_description44");
            dt.Columns.Add("pi99_description244");
            dt.Columns.Add("pi100_description34");
            dt.Columns.Add("pi101_description44");
            dt.Columns.Add("pi102_description54");
            dt.Columns.Add("pi103_description54");
            dt.Columns.Add("pi104_description24");
            dt.Columns.Add("pi105_description344");
            dt.Columns.Add("pi106_description444");
            dt.Columns.Add("pi107_description5");
            dt.Columns.Add("pi108_description544");
            dt.Columns.Add("pi109_description54");
            dt.Columns.Add("pi110_description44");
            dt.Columns.Add("pi111_description244");
            dt.Columns.Add("pi112_description34");
            dt.Columns.Add("pi113_description44");
            dt.Columns.Add("pi114_description54");
            dt.Columns.Add("pi115_description54");
            dt.Columns.Add("pi116_description54");
            dt.Columns.Add("pi117_description24");
            dt.Columns.Add("pi118_description344");
            dt.Columns.Add("pi119_description444");
            dt.Columns.Add("pi120_description5");
            dt.Columns.Add("pi121_description544");
            dt.Columns.Add("pi122_description54");
            dt.Columns.Add("pi123_description44");
            dt.Columns.Add("pi124_description244");
            dt.Columns.Add("pi125_description34");
            dt.Columns.Add("pi126_description44");
            dt.Columns.Add("pi127_description54");
            dt.Columns.Add("pi128_description44");
            dt.Columns.Add("pi129_description244");
            dt.Columns.Add("pi130_description34");
            dt.Columns.Add("pi131_description44");
            dt.Columns.Add("pi132_description54");
            dt.Columns.Add("pi133_description54");
            dt.Columns.Add("pi134_description54");
            dt.Columns.Add("pi135_description24");
            dt.Columns.Add("pi136_description344");
            dt.Columns.Add("pi137_description444");
            dt.Columns.Add("pi138_description5");
            dt.Columns.Add("pi139_description544");
            dt.Columns.Add("pi140_description54");
            dt.Columns.Add("pi141_description44");
            dt.Columns.Add("pi142_description244");
            dt.Columns.Add("pi143_description34");
            dt.Columns.Add("pi144_description44");
            dt.Columns.Add("pi145_description54");

            try
            {
                //string s = "Test,te,st,,,test,test";
                //s = s.Replace(",", "# ");
                //s = Regex.Replace(s,@"[^\w\d\s]",","); 
                //Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                //String[] Fields = CSVParser.Split(s);


                //// clean up the fields (remove " and leading spaces)
                //for (int i = 0; i < Fields.Length; i++)
                //{
                //    Fields[i] = Fields[i].TrimStart(' ', '"');
                //    Fields[i] = Fields[i].TrimEnd('"');
                //}

                

                //string result = TruncateCommas(s);

                //Console.WriteLine(result);


                var parser = new TextFieldParser(filename);
                parser.SetDelimiters(",");
                int r = 0;
                while (!parser.EndOfData)
                {
                    string LineValue = parser.ReadLine();
                    string[] checkRow = LineValue.Split(',');
                    if (checkRow.Count() > 135)
                    {
                        LineValue = RemoveCommaFromText(LineValue);
                    }

                    if (LineValue.IndexOf(", ") > 0)
                    {
                        LineValue = LineValue.Replace(", ", "#! ");
                    }
                    if (LineValue.IndexOf("'\'") > 0)
                    {
                        LineValue = LineValue.Replace("'\'", "#! ");
                    }
                    string[] rows = LineValue.Split(',');
                    var rowsList = rows.ToList();
                    dt.Rows.Add();
                    int c = 0;
                    foreach (string row in rowsList)
                    {
                        var roww = row.Replace('"', ' ').Trim().Replace("$", string.Empty).Replace("#! ", ", ");

                        dt.Rows[r][c] = roww.ToString();
                        c++;
                    }
                    r++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return dt;
        }
        private static string RemoveCommaFromText(string text)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && (i + 1) < text.Length)
                {
                    char c1 = text[i - 1];
                    char c2 = text[i];
                    char c3 = text[i + 1];

                    if (Char.IsLetter(c1) && c2 == ',' && Char.IsLetter(c3))
                    {
                        c2 = ' ';
                    }
                    sb.Append(c2);
                }
                else
                {
                    sb.Append(text[i]);
                }
            }
            return sb.ToString();
        }

        public string KeystrokeConvertRawFile(int StoreId, decimal tax)
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

                        string Url = BaseUrl + "/" + StoreId + "/Raw/" + myFile;
                        if (File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = ConvertTextToDataTable(Url);
                                var c = dt.Rows.Count;

                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullnameModel> full = new List<FullnameModel>();

                                foreach (DataRow dr in dt.Rows)
                                {
                                    try
                                    {
                                        ProductsModel pmsk = new ProductsModel();
                                        FullnameModel fname = new FullnameModel();

                                        pmsk.StoreID = StoreId;
                                        string upc = Regex.Replace(dr["upc"].ToString(), @"[^0-9]+", "");
                                        if (!string.IsNullOrEmpty(upc))
                                        {
                                            pmsk.upc = "#" + upc.ToString();
                                            fname.upc = "#" + upc.ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }

                                        string SKU = Regex.Replace(dr["sku"].ToString(), @"[^0-9]+", "");
                                        if (!string.IsNullOrEmpty(SKU))
                                        {
                                            pmsk.sku = "#" + SKU.ToString();
                                            fname.sku = "#" + SKU.ToString();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        int outt;
                                        var y = dr["qty"].ToString();
                                        var IsNumeric = Int32.TryParse(y, out outt);
                                        if (IsNumeric)
                                        {
                                            if (y != "")
                                            {
                                                var Qtyy = Convert.ToDecimal(y);

                                                pmsk.Qty = Convert.ToInt32(Qtyy);
                                            }
                                            else
                                            {
                                                pmsk.Qty = Convert.ToInt32(null);
                                            }
                                        }
                                        else { continue; }
                                        pmsk.StoreProductName = dr.Field<string>("name").Replace("=", "");
                                        fname.pname = dr.Field<string>("name").Replace("=", "");
                                        pmsk.StoreDescription = dr.Field<string>("name").Trim().Replace("=", "");
                                        fname.pdesc = dr.Field<string>("name").Replace("=", "");
                                        var X = dr["baseprice"].ToString();

                                        if (X != "")
                                        {
                                            var PRC = Convert.ToDecimal(X);

                                            pmsk.Price = Convert.ToDecimal(PRC);
                                            fname.Price = Convert.ToDecimal(PRC);
                                        }
                                        else
                                        {
                                            pmsk.Price = Convert.ToDecimal(null);
                                            fname.Price = Convert.ToDecimal(null);
                                        }
                                        pmsk.sprice = 0;
                                        pmsk.pack = Convert.ToInt32(1);
                                        fname.pack = Convert.ToInt32(1);
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

                                        string cat = dr.Field<string>("cat");

                                        if (StoreId == 11341)   // As per ticket 12492
                                        {
                                            string cat1 = dr.Field<string>("uom");
                                            if (cat1 == "MIXES")
                                            {
                                                pmsk.Tax = Convert.ToDecimal(0.07375);
                                            }
                                            else
                                            {
                                                pmsk.Tax = tax;
                                            }
                                        }
                                        else if (StoreId == 11736) //ticket #22146
                                        {
                                            if (cat=="3") // Beer
                                            {
                                                pmsk.Tax = Convert.ToDecimal(0.09125);
                                            }
                                            if (cat == "2") // Wine
                                            {
                                                pmsk.Tax = Convert.ToDecimal(0.11125);
                                            }
                                            if (cat == "1") // Liquor
                                            {
                                                pmsk.Tax = Convert.ToDecimal(0.11125);
                                            }
                                            if (cat == "6") // Mixers and More
                                            {
                                                pmsk.Tax = Convert.ToDecimal(0.08125);
                                            }
                                        }
                                        else
                                        {
                                            pmsk.Tax = tax;
                                        }

                                        
                                        if (StoreId == 11341)            // As per ticket 12492
                                        {
                                            fname.pcat = dr.Field<string>("uom");
                                        }
                                        else
                                        {
                                            fname.pcat = "";
                                        }
                                        fname.pcat1 = "";
                                        fname.pcat2 = "";

                                        if (StoreId == 11341)            // As per ticket 12492
                                        {
                                            fname.uom = "";
                                        }
                                        else
                                        {
                                            fname.pcat = dr.Field<string>("uom");
                                        }
                                        fname.region = "";
                                        fname.country = "";
                                        if (pmsk.Price > 0 && pmsk.Qty > 0 && cat != "4")
                                        {
                                            prodlist.Add(pmsk);
                                            full.Add(fname);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
                                }
                                Console.WriteLine("Generating KeystrokePos " + StoreId + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                                Console.WriteLine("Product File Generated For KeystrokePos " + StoreId);
                                Console.WriteLine("Generating KeystrokePos " + StoreId + " Fullname CSV Files.....");
                                string filename1 = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                                Console.WriteLine("Fullname File Generated For KeystrokePos " + StoreId);

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
                                //(new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                                //return "Not generated file for KeystrokePos " + StoreId;
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
}
