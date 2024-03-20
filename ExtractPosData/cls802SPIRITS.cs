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
using iTextSharp;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using System.Net;
using Docnet.Core;
using Docnet.Core.Models;

namespace ExtractPosData
{
    class cls802SPIRITS
    {
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        string rawPricePDF = ConfigurationManager.AppSettings.Get("BaseDirectory")+"\\11174\\"+ $"RAWPDF\\{DateTime.Now.ToString("MMyyyy")}.pdf";    
        public cls802SPIRITS(int StoreId, decimal Tax)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (!Directory.Exists(BaseUrl + "\\" + StoreId + "\\RAWPDF\\"))
            {
                Directory.CreateDirectory((BaseUrl + "\\" + StoreId + "\\RAWPDF\\"));
            }
            try
            {
                SPIRITSConvertRawFile(StoreId, Tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void PdfToCsv(string fileName, int StoreId)
        {

            string strText = string.Empty;
            List<string[]> list = new List<string[]>();

            string[] PdfData = null;
            try
            {
                var sb = new StringBuilder();
                using (var docReader = DocLib.Instance.GetDocReader(fileName, new PageDimensions()))
                {
                    for (var i = 0; i < docReader.GetPageCount(); i++)
                    {
                        using (var pageReader = docReader.GetPageReader(i))
                        {
                            var text = pageReader.GetText();
                            sb.Append($"{text}\n");
                        }
                    }
                }              
                var monthList = new List<string>();
                monthList.Add($"{DateTime.Now.AddMonths(-1).ToString("MMMM")} {DateTime.Now.Year}"); //with special character
                monthList.Add($"{DateTime.Now.AddMonths(-1).ToString("MMMM")} {DateTime.Now.Year}"); //with space
                monthList.Add($"{DateTime.Now.ToString("MMMM")} {DateTime.Now.Year}"); //with special character
                monthList.Add($"{DateTime.Now.ToString("MMMM")} {DateTime.Now.Year}"); //with space
                monthList.Add($"{DateTime.Now.AddMonths(+1).ToString("MMMM")} {DateTime.Now.Year}"); //with special character
                monthList.Add($"{DateTime.Now.AddMonths(+1).ToString("MMMM")} {DateTime.Now.Year}"); //with space

                var Pcat = "";
                var Pcat1 = "";

                #region catlists
                var catList = new List<string>();
                catList.Add("Brandy");
                catList.Add("Cocktails");
                catList.Add("Cordial");
                catList.Add("Fortified Wine");
                catList.Add("Gin");
                catList.Add("Rum");
                catList.Add("Tequila");
                catList.Add("Vodka");
                catList.Add("Whiskey");

                var subCatList = new List<string>();
                subCatList.Add("Brandy Domestic");
                subCatList.Add("Brandy Imported");
                subCatList.Add("Cordial Domestic");
                subCatList.Add("Cocktails");
                subCatList.Add("Cordial Imported");
                subCatList.Add("Fortified Wine Domestic");
                subCatList.Add("Fortified Wine Dry Vermouth");
                subCatList.Add("Fortified Wine Imported");
                subCatList.Add("Fortified Wine Port");
                subCatList.Add("Fortified Wine Sherry");
                subCatList.Add("Fortified Wine Sweet Vermouth");
                subCatList.Add("Gin Domestic");
                subCatList.Add("Gin Flavored");
                subCatList.Add("Gin Imported");
                subCatList.Add("Rum Dark");
                subCatList.Add("Rum Flavored");
                subCatList.Add("Rum Light");
                subCatList.Add("Tequila Anejo");
                subCatList.Add("Tequila Flavored");
                subCatList.Add("Tequila Gold");
                subCatList.Add("Tequila Mezcal");
                subCatList.Add("Tequila Reposado");
                subCatList.Add("Tequila White");
                subCatList.Add("Vodka Domestic");
                subCatList.Add("Vodka Flavored");
                subCatList.Add("Vodka Imported");
                subCatList.Add("Whiskey American");
                subCatList.Add("Whiskey Bourbon");
                subCatList.Add("Whiskey Canadian");
                subCatList.Add("Whiskey Irish");
                subCatList.Add("Whiskey Other");
                subCatList.Add("Whiskey Rye");
                subCatList.Add("Whiskey Scotch");
                #endregion

                //List<string> temp = PdfData.ToList();
                List<string> temp = sb.ToString().Split(Environment.NewLine.ToCharArray()).ToList();
                temp.RemoveAll(r => r.Trim() == "");
                temp.RemoveAll(r => r.Contains("Vermont 802Spirits ") || r.Contains("VT Reg VT Sale Price") || r.Contains("Code Brand Size Price") || monthList.Any(a => a == r) || Regex.IsMatch(r, @"(\d+.of.\d+)"));
                temp.RemoveAll(r => r.Contains("Vermont 802Spirits Current Complete Price List") || r.Contains("VT Reg NH VT Sale Price"));
                temp.RemoveAll(r => r.Contains("Code Brand Size") || r.Contains("VT Reg") || r.Contains("Price NH Price") || r.Contains("VT Sale") || r.Contains("Price Save Proof") || r.Contains("Price") || r.Contains("per OZ") || r.Contains("NH"));

                var VtPrice = "";
                var NhPrice = "";
                var VtSalePrice = "";
                var Save = "";
                //int tempname1Cnt = 0;
                var finalData = new StringBuilder();
                finalData.Append("Code,Brand,Size,Vt Price,NH Price,Vt Sale Price,Save,Proof,per OZ,pcat,pcat1\n");
                var tempname = "";
                var Brand = "";

                for (int i = 0; i < temp.Count; i++)
                {
                    if ((i < temp.Count - 1) && Regex.IsMatch(temp[i + 1].Substring(0, 2), @"([A-Za-z]+)") && !catList.Any(a => a == temp[i + 1]) && !subCatList.Any(a => a == temp[i + 1]) || i < (temp.Count - 1) && temp[i + 1] == Pcat)
                    {
                        temp[i] = temp[i] + " " + temp[i + 1];
                        temp.Remove(temp[i + 1]);
                    }

                    if (Regex.IsMatch(temp[i].Substring(0, 2), @"([A-Za-z]+)") && Regex.IsMatch(temp[i + 1].Substring(0, 2), @"([A-Za-z]+)"))
                    {
                        Pcat = temp[i];
                        Pcat1 = temp[i + 1];
                        temp.RemoveRange(0, i + 2);
                    }
                    if (Regex.IsMatch(temp[i].Substring(0, 4), @"([A-Za-z]+)") && temp[i].Contains(Pcat))
                    {
                        Pcat1 = temp[i];
                        temp.RemoveAt(i);
                    }
                    if (Regex.IsMatch(temp[i].Substring(0, 4), @"([A-Za-z]+)"))
                    {
                        Pcat = temp[i];
                        Pcat1 = "";
                        temp.RemoveAt(i);
                    }
                    if ((i < temp.Count - 1) && Regex.IsMatch(temp[i + 1], @"(\A\d+\.\d+\z)"))
                    {
                        temp[i] = temp[i] + " " + temp[i + 1];
                        temp.Remove(temp[i + 1]);

                    }

                    if ((i < temp.Count - 1) && !catList.Any(a => a == temp[i + 1]) && !subCatList.Any(a => a == temp[i + 1]) && !(Regex.IsMatch(temp[i + 1], @"(\d{5})")))
                    {
                        temp[i] = temp[i] + " " + temp[i + 1];
                        temp.Remove(temp[i + 1]);

                    }
                    #region without status colomn
                    var data = temp[i].Split(' ').ToList();                 
                    var code = data[0].Trim(); 
                    data.Remove(data[0]); 
                    var peroz = data[data.Count - 1];               
                    data.Remove(data[data.Count - 1]);
                    if (data[data.Count - 1] == "New")
                    {
                        data.RemoveAt(data.Count - 1);
                    }
                    else if (data[data.Count - 1] == "Volume")
                    {
                        data.RemoveAt(data.Count - 1);
                        data.RemoveAt(data.Count - 1);
                    }
                    var Proof = data[data.Count - 1];
                    data.RemoveAt(data.Count - 1);

                    int flag = 0;
                    for (int j = 0; j < data.Count; j++)
                    {
                        var regex = Regex.Match(data[j], @"^(\d+\.)\d+$");
                        if (regex.Success)
                        {
                            flag++;
                        }
                    }
                    if (flag == 1)
                    {
                        VtPrice = data[data.Count - 1];
                        NhPrice = "";
                        VtSalePrice = "";
                        Save = "";
                        data.Remove(data[data.Count - 1]);
                    }
                    if (flag == 2)
                    {
                        VtPrice = data[data.Count - 2];
                        NhPrice = data[data.Count - 1];
                        VtSalePrice = "";
                        Save = "";
                        data.Remove(data[data.Count - 2]);
                        data.Remove(data[data.Count - 1]);
                    }
                    if (flag == 3)
                    {
                        VtPrice = data[data.Count - 3];
                        VtSalePrice = data[data.Count - 2];
                        Save = data[data.Count - 1];
                        NhPrice = "";
                        data.Remove(data[data.Count - 3]);
                        data.Remove(data[data.Count - 2]);
                        data.Remove(data[data.Count - 1]);
                    }
                    if (flag == 4)
                    {
                        VtPrice = data[data.Count - 4];
                        NhPrice = data[data.Count - 3];
                        VtSalePrice = data[data.Count - 2];
                        Save = data[data.Count - 1];
                        data.Remove(data[data.Count - 4]);
                        data.Remove(data[data.Count - 3]);
                        data.Remove(data[data.Count - 2]);
                        data.Remove(data[data.Count - 1]);
                    }
                    var Size = data[data.Count - 1];
                    data.Remove(data[data.Count - 1]);

                    Brand = string.Join(" ", data)+""+ tempname;
                    temp.RemoveRange(i, 1);
                    i--;

                    finalData.Append($"{code},\"{Brand}\",\"{Size}\",{VtPrice},{NhPrice},{VtSalePrice},{Save},{Proof},{peroz},{Pcat},{Pcat1}\n");
                    #endregion                    
                }
                File.WriteAllText(BaseUrl + "\\" + StoreId + "\\Raw\\Pricing_Current_Full.csv", finalData.ToString());
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
            }  
        }
        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            DataTable dtResult = new DataTable();
            try
            {
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
                   
                                roww=roww.TrimStart(new Char[] { '0' });

                                dtResult.Rows[r][c] = roww.ToString();
                                c++;
                            }
                            r++;
                        }
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
            }
            return dtResult; //Returning Dattable  
        }
        private void DownloadPdf()
        {
            if (System.IO.File.Exists(rawPricePDF))
            {
                return;
            }
            var url = new Uri("https://802spirits.com/sites/spirits/files/documents/Pricing_Current_Full.pdf");
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, rawPricePDF);
            }
        }
        public string SPIRITSConvertRawFile(int StoreId, decimal Tax)
        {
            DownloadPdf();
            PdfToCsv(rawPricePDF, StoreId);
            DataTable dt = new DataTable();
            DataTable finaldt = new DataTable();
            string Url = "";
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {                                       
                    string[] filePathss = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");
                    if (filePathss != null)
                    {
                        foreach (var itm in filePathss)
                        {
                            if (!itm.Contains("Fullname") && (!Url.Contains("product") || itm.Contains("Pricing_Current_Full")))
                            {
                                Url = itm;
                                dt = ConvertCsvToDataTable(Url);
                                finaldt.Merge(dt);
                            }
                        }
                        try
                        {
                            List<ProductsModel> prodlist = new List<ProductsModel>();
                            List<FullNameProductModel> full = new List<FullNameProductModel>();

                            foreach (DataRow dr in finaldt.Rows)
                            {
                                ProductsModel pmsk = new ProductsModel();
                                FullNameProductModel fname = new FullNameProductModel();

                                pmsk.StoreID = StoreId;
                                if (!string.IsNullOrEmpty(dr["UPC"].ToString()) )
                                {
                                    pmsk.upc = dr["UPC"].ToString();
                                    fname.upc = dr["UPC"].ToString();
                                    pmsk.sku = dr["UPC"].ToString();
                                    fname.sku = dr["UPC"].ToString();
                                }
                                else if (!string.IsNullOrEmpty(dr["Code"].ToString()))
                                {
                                    pmsk.upc = "#111740000" + dr["Code"].ToString();
                                    fname.upc = "#111740000" + dr["Code"].ToString();
                                    pmsk.sku = "#111740000" + dr["Code"].ToString();
                                    fname.sku = "#111740000" + dr["Code"].ToString();
                                }
                                else
                                {
                                    continue;
                                }

                                if (pmsk.upc != "#11174")
                                {
                                    if (!string.IsNullOrEmpty(dr["Qty"].ToString()))
                                    {
                                        decimal qty = Convert.ToDecimal(dr["Qty"]);
                                        pmsk.Qty = System.Convert.ToInt32(qty);
                                    }
                                    else
                                    {
                                        pmsk.Qty = 99;
                                    }
                                }
                              
                                if (!string.IsNullOrEmpty(dr["StoreProductName"].ToString()))
                                {
                                    pmsk.StoreProductName = dr.Field<string>("StoreProductName");
                                    fname.pname = dr.Field<string>("StoreProductName");
                                    pmsk.StoreDescription = dr.Field<string>("StoreProductName");
                                    fname.pdesc = dr.Field<string>("StoreProductName");
                                }
                                else if (!string.IsNullOrEmpty(dr["Brand"].ToString()))
                                {
                                    pmsk.StoreProductName = dr.Field<string>("Brand");
                                    fname.pname = dr.Field<string>("Brand");
                                    pmsk.StoreDescription = dr.Field<string>("Brand");
                                    fname.pdesc = dr.Field<string>("Brand");
                                }
                                var ab = dr.Field<string>("Price");
                                if (!string.IsNullOrEmpty(dr["Price"].ToString()))
                                {
                                    pmsk.Price = System.Convert.ToDecimal(dr["price"]);
                                    fname.Price = System.Convert.ToDecimal(dr["price"]);
                                }
                                else if (!string.IsNullOrEmpty(dr["Vt Price"].ToString()))
                                {
                                    pmsk.Price = System.Convert.ToDecimal(dr["Vt Price"]);
                                    fname.Price = System.Convert.ToDecimal(dr["Vt Price"]);
                                }
                                else
                                {
                                    continue;
                                }
                                if (!string.IsNullOrEmpty(dr["sprice"].ToString()))
                                {
                                    pmsk.sprice = System.Convert.ToDecimal(dr["sprice"]);
                                }
                                else if (!string.IsNullOrEmpty(dr["Vt Sale Price"].ToString()))
                                {
                                    pmsk.sprice = System.Convert.ToDecimal(dr["Vt Sale Price"]);
                                }
                                else
                                {
                                    pmsk.sprice = 0;
                                }
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
                                if (!string.IsNullOrEmpty(dr["Deposit"].ToString()))
                                {
                                    pmsk.Deposit = Convert.ToDecimal(dr["Deposit"]);
                                }
                                else
                                {
                                    pmsk.Deposit = 0;
                                }
                                if (!string.IsNullOrEmpty(dr["pcat"].ToString()))
                                {
                                    fname.pcat = dr.Field<string>("pcat");
                                }
                                else
                                {
                                    fname.pcat = "";
                                }
                                if (!string.IsNullOrEmpty(dr["pcat1"].ToString()))
                                {
                                    fname.pcat1 = dr.Field<string>("pcat1");
                                }
                                else
                                {
                                    fname.pcat1 = "";
                                }
                                fname.pcat2 = "";
                                if (!string.IsNullOrEmpty(dr["uom"].ToString()))
                                {
                                    fname.uom = dr.Field<string>("uom");
                                    pmsk.uom = dr.Field<string>("uom");
                                }
                                else if (!string.IsNullOrEmpty(dr["Size"].ToString()))
                                {
                                    fname.uom = dr.Field<string>("Size");
                                    pmsk.uom = dr.Field<string>("Size");
                                }
                                else
                                {
                                    fname.uom = "";
                                    pmsk.uom = "";
                                }
                                fname.region = "";
                                fname.country = "";
                                if (pmsk.Qty > 0)
                                {
                                    prodlist.Add(pmsk);
                                    full.Add(fname);
                                }
                            }
                            Console.WriteLine("Generating 802SPIRITS " + StoreId + " Product CSV Files.....");
                            string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated For 802SPIRITS " + StoreId);
                            Console.WriteLine();
                            Console.WriteLine("Generating 802SPIRITS " + StoreId + " Fullname CSV Files.....");
                            filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", StoreId, BaseUrl);
                            Console.WriteLine("Fullname File Generated For 802SPIRITS " + StoreId);

                            string[] filePaths = Directory.GetFiles(BaseUrl + "/" + StoreId + "/Raw/");

                            foreach (string filePath in filePaths)
                            {
                                GC.Collect();
                                if (!filePath.Contains("Pricing"))
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("" + e.Message);
                            return "Not generated file for 802SPIRITS " + StoreId;
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
