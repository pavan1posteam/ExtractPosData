using ExcelDataReader;
using ExtractPosData.Models;
using S22.Imap;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
namespace ExtractPosData
{
    class ClsXexatouch
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
        string MailId_10110 = ConfigurationManager.AppSettings["MailId_10110"];
        string MailId_11717 = ConfigurationManager.AppSettings["MailId_11717"];
        string ExaTouchDiffmail = ConfigurationManager.AppSettings["ExaTouchDiffmail"];
        string Password = ConfigurationManager.AppSettings["Password"];
        string Hostname = ConfigurationManager.AppSettings["Hostname"];
        int Port = Convert.ToInt16(ConfigurationManager.AppSettings["Port"]);
        string BaseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        public ClsXexatouch(int storeId, decimal tax)
        {
            try
            {
                XexatouchConvertRawFile(storeId, tax);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + storeId + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
            }
        }
        public static DataTable ConvertCsvToDataTable(string FileName)
        {
            FileStream stream = File.Open(FileName, FileMode.Open, FileAccess.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            DataSet result = excelReader.AsDataSet();
            DataTable dt = new DataTable();
            dt = result.Tables[0];
            List<DataRow> list = new List<DataRow>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(dr);
            }
            dt = dt.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();
            for (int i = 0; i < dt.Rows.Count - 1; i++)
            {
                if (dt.Rows[i]["Column0"].ToString() == string.Empty || !Regex.IsMatch(dt.Rows[i]["Column2"].ToString(), @"(\d+)"))
                {
                    DataRow dr = dt.Rows[i];
                    dr.Delete();
                    continue;
                }
            }
            dt.AcceptChanges();
            stream.Close();
            return dt; //Returning Datatable
        }
        public string XexatouchConvertRawFile(int storeid, decimal tax)
        {
            int i = 1;
            if (ExaTouchDiffmail.Contains(storeid.ToString()))
            {
                using (ImapClient client = new ImapClient(Hostname, Port, MailId_11717, Password, AuthMethod.Login, true))
                {
                    IEnumerable<uint> uids = client.Search(SearchCondition.Subject("CONFIDENTIAL Exatouch Items report"));
                    IEnumerable<MailMessage> messages = client.GetMessages(uids,
                        (Bodypart part) =>
                        {
                            if (part.Disposition.Type == ContentDispositionType.Attachment)
                            {
                                if (part.Type == ContentType.Application) //&& part.Subtype == "Items"
                                {

                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            return true;
                        });
                    foreach (var msg in messages)
                    {
                        int l = messages.Count();
                        foreach (var attachment in msg.Attachments)
                        {
                            int z = i++;
                            if (z == l)
                            {
                                byte[] allBytes = new byte[attachment.ContentStream.Length];
                                int bytesRead = attachment.ContentStream.Read(allBytes, 0, (int)attachment.ContentStream.Length);
                                string destinationFile = BaseUrl + "/" + storeid + "/RAW/" + attachment.Name;
                                BinaryWriter writer = new BinaryWriter(new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
                                writer.Write(allBytes);
                                writer.Close();
                            }
                        }
                    }

                }
            }
            else
            {
                using (ImapClient client = new ImapClient(Hostname, Port, MailId_10110, Password, AuthMethod.Login, true))
                {
                    IEnumerable<uint> uids = client.Search(SearchCondition.Subject("CONFIDENTIAL Exatouch Items report"));
                    IEnumerable<MailMessage> messages = client.GetMessages(uids,
                        (Bodypart part) =>
                        {
                            if (part.Disposition.Type == ContentDispositionType.Attachment)
                            {
                                if (part.Type == ContentType.Application) //&& part.Subtype == "Items"
                                {

                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                            return true;
                        });
                    foreach (var msg in messages)
                    {
                        int l = messages.Count();
                        foreach (var attachment in msg.Attachments)
                        {
                            int z = i++;
                            if (z == l)
                            {
                                byte[] allBytes = new byte[attachment.ContentStream.Length];
                                int bytesRead = attachment.ContentStream.Read(allBytes, 0, (int)attachment.ContentStream.Length);
                                string destinationFile = BaseUrl + "/" + storeid + "/RAW/" + attachment.Name;
                                BinaryWriter writer = new BinaryWriter(new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
                                writer.Write(allBytes);
                                writer.Close();
                            }
                        }
                    }
                }
            }   
            if (Directory.Exists(BaseUrl))
            {
                if (Directory.Exists(BaseUrl + "/" + storeid + "/RAW/"))
                {
                    var directory = new DirectoryInfo(BaseUrl + "/" + storeid + "//Raw");
                    if (directory.GetFiles().FirstOrDefault() != null)
                    {
                        var myFile = (from f in directory.GetFiles()
                                      orderby f.LastWriteTime descending
                                      select f).First();
                        string Url = BaseUrl + "/" + storeid + "/Raw/" + myFile;
                        if (File.Exists(Url))
                        {
                            try
                            {
                                DataTable dt = ConvertCsvToDataTable(Url);
                                var dtr = from s in dt.AsEnumerable() select s;
                                List<ProductsModel> prodlist = new List<ProductsModel>();
                                List<FullNameProductModel> full = new List<FullNameProductModel>();
                                foreach (DataRow dr in dt.Rows)
                                {
                                    ProductsModel pmsk = new ProductsModel();
                                    FullNameProductModel fname = new FullNameProductModel();
                                    pmsk.StoreID = storeid;
                                    if (!string.IsNullOrEmpty(dr["Column2"].ToString()))
                                    {
                                        pmsk.upc = "#" + dr["Column2"].ToString();
                                        fname.upc = "#" + dr["Column2"].ToString();
                                        pmsk.sku = "#" + dr["Column2"].ToString();
                                        fname.sku = "#" + dr["Column2"].ToString();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (!string.IsNullOrEmpty(dr["Column0"].ToString()))
                                    {
                                        pmsk.StoreProductName = dr.Field<string>("Column0");
                                        fname.pname = dr.Field<string>("Column0");
                                        pmsk.StoreDescription = dr.Field<string>("Column0");
                                        fname.pdesc = dr.Field<string>("Column0");
                                    }
                                    var Qtyy = long.Parse(dr.Field<dynamic>("Column8").ToString());
                                    pmsk.Qty = Convert.ToInt32(Qtyy) > 0 ? Convert.ToInt32(Qtyy) : 0;
                                    var x = dr["Column9"].ToString().Replace("$", "");
                                    if (x != "")
                                    {
                                        var prc = Convert.ToDecimal(x);
                                        pmsk.Price = Convert.ToDecimal(prc);
                                        fname.Price = Convert.ToDecimal(prc);
                                        if (pmsk.Price <= 0 || fname.Price <= 0)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    pmsk.sprice = System.Convert.ToDecimal(null);
                                    if (pmsk.sprice > 0)
                                    {
                                        pmsk.Start = DateTime.Now.ToString("mm/dd/yyyy");
                                        pmsk.End = DateTime.Now.AddDays(1).ToString("mm/dd/yyyy");
                                    }
                                    else
                                    {
                                        pmsk.Start = "";
                                        pmsk.End = "";
                                    }
                                    pmsk.sprice = 0;
                                    pmsk.pack = 1;
                                    fname.pack = 1;
                                    pmsk.Tax = tax;
                                    pmsk.altupc1 = "";
                                    pmsk.altupc2 = "";
                                    pmsk.altupc3 = "";
                                    pmsk.altupc4 = "";
                                    pmsk.altupc5 = "";
                                    pmsk.Deposit = 0;
                                    if (!string.IsNullOrEmpty(dr["Column3"].ToString()))
                                    {
                                        fname.pcat = dr.Field<string>("Column3");
                                    }
                                    else
                                    {
                                        fname.pcat = "";
                                    }
                                    if (!string.IsNullOrEmpty(dr["Column5"].ToString()))
                                    {
                                        fname.pcat1 = dr.Field<string>("Column5");
                                    }
                                    else
                                    {
                                        fname.pcat1 = "";
                                    }
                                    fname.pcat2 = "";                                    
                                    if (!string.IsNullOrEmpty(dr["Column7"].ToString()))
                                    {
                                        fname.uom = dr.Field<string>("Column7");
                                        pmsk.uom = dr.Field<string>("Column7");
                                    }
                                    else
                                    {
                                        fname.uom = "";
                                        pmsk.uom = "";
                                    }
                                    fname.region = "";
                                    fname.country = "";
                                    prodlist.Add(pmsk);
                                    full.Add(fname);
                                }
                                Console.WriteLine("Generating XexatouchPos " + storeid + " Product CSV Files.....");
                                string filename = GenerateCSV.GenerateCSVFile(prodlist, "PRODUCT", storeid, BaseUrl);
                                Console.WriteLine("Product File Generated For XexatouchPos " + storeid);
                                Console.WriteLine();
                                Console.WriteLine("Generating XexatouchPos " + storeid + " Fullname CSV Files.....");
                                filename = GenerateCSV.GenerateCSVFile(full, "FULLNAME", storeid, BaseUrl);
                                Console.WriteLine("Fullname File Generated For XexatouchPos " + storeid);
                                string[] filePaths = Directory.GetFiles(BaseUrl + "/" + storeid + "/Raw/");
                                foreach (string filePath in filePaths)
                                {
                                    string destpath = filePath.Replace(@"/Raw/", @"/RawDeleted/" + DateTime.Now.ToString("yyyyMMddhhmmss"));
                                    File.Move(filePath, destpath);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + storeid + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                                return "Not generated file for XexatouchPos " + storeid;
                            }
                        }
                        else
                        {
                            return "Ínvalid FileName" + storeid;
                        }
                    }
                    else
                    {
                        return "Invalid Sub-Directory" + storeid;
                    }
                }
            }
            else
            {
                return "Invalid Directory" + storeid;
            }
            return "Completed generating Files For XexatouchPos" + storeid;
        }
    }
}