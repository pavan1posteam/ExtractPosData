using ExtractPosData.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ExtractPosData
{
    class clsVerifoneOrRubyPosXml
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string baseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public clsVerifoneOrRubyPosXml(int StoreId, decimal tax)
        {
            try
            {
                ConvertRawFile(StoreId, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        static string RemoveLeadingZeros(string input)
        {
            int maxLeadingZeros = 2;

            if (input.Length > maxLeadingZeros)
            {
                int count = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    if (input[i] == '0')
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (count > maxLeadingZeros)
                {
                    input = input.Substring(count - maxLeadingZeros);
                }
            }

            return input;
        }
        public string ConvertRawFile(int StoreId, decimal Tax)
        {
            string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + StoreId + "/Raw/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + StoreId + "/Raw/");
                    var myFile = (from f in directory.GetFiles("*.xml")
                                  orderby f.LastWriteTime descending
                                  select f).First();

                    string Url = BaseUrl + "/" + StoreId + "/Raw/" + myFile;
                    if (File.Exists(Url))
                    {
                        try
                        {                           

                            var doc = XDocument.Load(Url);
                            var items = doc.Root
                                            .Elements()  // Select all child elements of the root
                                            .Select(node => new
                                            {
                                                ItemName = (string)node.Element("description"),
                                                ItemUpc = (string)node.Element("upc"),
                                                Department = (int)node.Element("department"),
                                                Price = (decimal)node.Element("price"),
                                            })
                                            .ToList();


                            //var json = JsonConvert.SerializeXmlNode(xmlDoc).Replace("@", "");
                            //json = "{" + json.Substring(26, json.Length - 26);
                            //var itms = JsonConvert.DeserializeObject<Root>(json);

                            List<ProductsModel> prodlist = new List<ProductsModel>();
                            List<FullNameProductModel> fulllist = new List<FullNameProductModel>();

                            ProductsModel prod = new ProductsModel();
                            FullNameProductModel fname = new FullNameProductModel();
                            foreach (var item in items)
                            {
                                prod = new ProductsModel();
                                fname = new FullNameProductModel();

                                prod.StoreID = StoreId;
                                string upc = RemoveLeadingZeros(item.ItemUpc);
                                if(upc.Length>=10)
                                {
                                    upc = upc.Trim('0');
                                }
                                prod.upc = '#' + upc;
                                fname.upc = '#' + upc;
                                prod.sku = '#' + upc;
                                fname.sku = '#' + upc;
                                prod.Qty = Convert.ToInt32(9999);
                                prod.pack = 1;
                                fname.pack = 1;
                                prod.StoreProductName = item.ItemName;
                                prod.StoreDescription = item.ItemName;
                                fname.pname = item.ItemName;
                                fname.pdesc = item.ItemName;
                                prod.Price = item.Price;
                                fname.Price = item.Price;                                    
                                fname.uom = "";
                                fname.pcat = "";
                                fname.pcat1 = "";
                                fname.pcat2 = "";
                                fname.country = "";
                                fname.region = "";
                                prod.Start = "";
                                prod.End = "";
                                prod.Tax = Convert.ToDecimal(Tax);
                                prod.altupc1 = "";
                                prod.altupc2 = "";
                                prod.altupc3 = "";
                                prod.altupc4 = "";
                                prod.altupc5 = "";
                                if (prod.Price > 0)
                                {
                                    prodlist.Add(prod);
                                    fulllist.Add(fname);
                                }
                            }
                            Console.WriteLine("Generating VerifoneOrRuby Pos " + StoreId + " Product CSV Files.....");
                            //Console.WriteLine("Generating VerifoneOrRuby Pos " + StoreId + " Full Name CSV Files.....");
                            string pfilename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", StoreId, BaseUrl);
                            //string filename = GenerateCSV.GenerateCSVFile(fulllist, "FULLNAME", StoreId, BaseUrl);
                            Console.WriteLine("Product File Generated VerifoneOrRuby Pos " + StoreId);
                            //Console.WriteLine("Full Name File Generated For VerifoneOrRuby Pos " + StoreId);

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
                            (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in clsVerifoneOrRubyPosXml@" + StoreId + DateTime.UtcNow + " GMT", e.Message + "<br/>" + e.StackTrace);
                            return "Not generated file for clsVerifoneOrRubyPosXml " + StoreId;
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
            return "Completed generating File For clsVerifoneOrRubyPosXml" + StoreId;
        }
    }
}
