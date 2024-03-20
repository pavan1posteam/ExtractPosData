using ExtractPosData.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace ExtractPosData
{
    class clsSquarePos
    {
        string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];

        public string strcategory = "";
        public clsSquarePos(int StoreId, string accessToken, string DeveloperId, string categories, decimal tax, string LocationId)
        {
            Console.WriteLine("Generating SquarePOS " + StoreId + " Product File.......");
            strcategory = categories;
            try
            {
                string baseUrl = ConfigurationManager.AppSettings["SquareAPIBaseUrl"];
                string folderPath = ConfigurationManager.AppSettings["BaseDirectory"];
                ProductList(accessToken, baseUrl, StoreId, folderPath, DeveloperId, tax, LocationId);
                Console.WriteLine("Product File Generated For Square Pos " + StoreId);
            }
            catch (Exception ex)
            {
                if (StoreId == 10430)
                {
                    Console.WriteLine("" + ex.Message);
                }
                else
                {
                    Console.WriteLine("" + ex.Message);
                    (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                }
            }
            finally { }
        }
        private string getStoreSlug(string accessToken, string apiUrl, string baseUrl)
        {
            string content = null;
            try
            {
                var client = new RestClient(baseUrl + apiUrl);
                var request = new RestRequest(Method.GET);

                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                content = response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
                //(new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
            }
            finally { }
            return content;
        }
        private string getStoreSlug(string accessToken, string apiUrl, string baseUrl, string cursor)
        {
            string content = null;
            try
            {
                var client = new RestClient(baseUrl + apiUrl + "&cursor=" + cursor);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                content = response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
                // (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
            }
            finally { }
            return content;
        }
        private string postStoreSlug(string accessToken, string baseUrl, string apiUrl, string cursor, string LocationId)
        {
            string content = null;
            try
            {
                string str = "{\"location_ids\":[\"" + LocationId + "\"]," +
                                  "\"cursor\":\"" + cursor + "\"}";
                var client = new RestClient(baseUrl + apiUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", str, ParameterType.RequestBody);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                content = response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
            }
            finally { }
            return content;
        }
        private QueryResult getStoreSlug1(string accessToken, string apiUrl, string Url)
        {
            string content = null;
            QueryResult obj = new QueryResult();
            try
            {
                apiUrl = string.IsNullOrEmpty(Url) ? apiUrl : Url;
                var client = new RestClient(apiUrl);
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", "Bearer " + accessToken);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);
                var headers = response.Headers.ToList();
                var batchToken = headers.Find(x => x.Name == "Link");

                obj.Response = response.Content;
                if (batchToken != null)
                {
                    obj.Url = batchToken.Value.ToString().Split(';')[0].Split('<', '>')[1];
                }
                content = response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
                // (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
            }
            finally { }
            return obj;
        }
        private List<locations> ListLocation(string BaseUrl, string ApiUrl, string AccessToken)
        {
            string content = getStoreSlug(AccessToken, ApiUrl, BaseUrl);
            SquareLocations location = JsonConvert.DeserializeObject<SquareLocations>(content);
            return location.locations;

        }
        private List<ProductCategory> ListCategory(string BaseUrl, string ApiUrl, string AccessToken)
        {
            string val = getStoreSlug(AccessToken, ApiUrl, BaseUrl);
            SquareCategory Pcat = JsonConvert.DeserializeObject<SquareCategory>(val);

            return Pcat.objects;
        }
        private List<Tax> ListTax(string BaseUrl, string ApiUrl, string AccessToken)
        {
            string tax1 = getStoreSlug(AccessToken, ApiUrl, BaseUrl);
            SquareTax listtax = JsonConvert.DeserializeObject<SquareTax>(tax1);
            return listtax.objects;
        }
        private List<Product> ProductItem(string BaseUrl, string ApiUrl, string AccessToken)
        {
            string cursor = "";
            List<Product> list = new List<Product>();
            do
            {
                string val = getStoreSlug(AccessToken, ApiUrl, BaseUrl, cursor);
                SquareProductItem SpItem = JsonConvert.DeserializeObject<SquareProductItem>(val);
                cursor = SpItem.Cursor;
                list.AddRange(SpItem.objects);
            } while (!string.IsNullOrEmpty(cursor));
            string s = JsonConvert.SerializeObject(list);
            return list;
        }
        private List<Count> InvCount(string BaseUrl, string AccessToken, string LocationId)
        {
            string cursor = "";
            List<Inventory> listInventory = new List<Inventory>();
            List<Count> countList = new List<Count>();
            Count count = new Count();
            try
            {
                do
                {
                    string queryresults = postStoreSlug(AccessToken, BaseUrl, "/v2/" + "inventory/counts/batch-retrieve", cursor, LocationId);
                    Inventory SpItem = JsonConvert.DeserializeObject<Inventory>(queryresults);
                    cursor = SpItem.cursor;
                    countList.AddRange(SpItem.counts);
                } while (!string.IsNullOrEmpty(cursor));
                string s = JsonConvert.SerializeObject(countList);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return countList;
        }
        private List<Inventory> Inventory(string BaseUrl, string AccessToken, string LocationId)
        {
            string Url = "";
            List<locations> obj = ListLocation(BaseUrl, "/v2/locations", AccessToken);
            List<Inventory> listInventory = new List<Inventory>();
            List<Count> countList = new List<Count>();
            Count count = new Count();
            QueryResult queryresult;
            foreach (var i in obj)
            {
                do
                {
                    if (string.IsNullOrEmpty(LocationId))
                    {
                        queryresult = getStoreSlug1(AccessToken, BaseUrl + "/v1/" + i.id + "/inventory", Url);
                    }
                    else
                    {
                        //queryresult = getStoreSlug1(AccessToken, BaseUrl + "/v1/" + LocationId + "/inventory", Url);
                        queryresult = getStoreSlug1(AccessToken, BaseUrl + "/v2/" + "inventory/HJPPRHVFOAEOEI53OI5ICBXO/?location_ids=89KAYE2KG1KNY", Url);
                    }
                    //QueryResult queryresult = getStoreSlug1(AccessToken, BaseUrl + "/v2/" + "/inventory/" + "/batch-change", Url);
                    var lists = JsonConvert.DeserializeObject<Inventory>(queryresult.Response);
                    var ans = lists.counts[0];
                    countList.Add(ans);
                    var list = JsonConvert.DeserializeObject<List<Inventory>>(queryresult.Response);
                    Url = queryresult.Url;
                    listInventory.AddRange(list);
                } while (!string.IsNullOrEmpty(Url));
            }
            return listInventory;
        }
        private List<v1Items> ListV1Items(string BaseUrl, string AccessToken, string LocationId)
        {
            string Url = "";
            List<v1Items> listv1items = new List<v1Items>();
            QueryResult queryresult;
            try
            {
                List<locations> obj = ListLocation(BaseUrl, "/v2/locations", AccessToken);
                foreach (var i in obj)
                {
                    do
                    {
                        if (string.IsNullOrEmpty(LocationId))
                        {
                            queryresult = getStoreSlug1(AccessToken, BaseUrl + "/v1/" + i.id + "/items", Url);
                        }
                        else
                        {
                            queryresult = getStoreSlug1(AccessToken, BaseUrl + "/v1/" + LocationId + "/items", Url);
                        }
                        var list = JsonConvert.DeserializeObject<List<v1Items>>(queryresult.Response);
                        Url = queryresult.Url;
                        listv1items.AddRange(list);
                    } while (!string.IsNullOrEmpty(Url));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally { }
            return listv1items;
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public static string GenerateCSV1(DataTable dt, string Filename)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                int count = 1;
                int totalColumns = dt.Columns.Count;
                foreach (DataColumn dr in dt.Columns)
                {
                    sb.Append(dr.ColumnName);

                    if (count != totalColumns)
                    {
                        sb.Append(",");
                    }

                    count++;
                }

                sb.AppendLine();

                string value = String.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    for (int x = 0; x < totalColumns; x++)
                    {
                        value = dr[x].ToString();

                        if (value.Contains(",") || value.Contains("\""))
                        {
                            value = '"' + value.Replace("\"", "\"\"") + '"';
                        }

                        sb.Append(value);

                        if (x != (totalColumns - 1))
                        {
                            sb.Append(",");
                        }
                    }

                    sb.AppendLine();
                }
                //string filename = "Final.csv";
                File.WriteAllText("Upload\\" + Filename, sb.ToString());
                return Filename;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return "";
        }
        public void ProductList(string accessToken, string baseUrl, int StoreId, string folderPath, string DeveloperId, decimal tax, string LocationId)
        {
            try
            {
                List<locations> llocations = ListLocation(baseUrl, "/v2/locations", accessToken);
                List<ProductCategory> lcategory = ListCategory(baseUrl, "/v2/catalog/list?types=category", accessToken);
                List<Tax> ltax = ListTax(baseUrl, "/v2/catalog/list?types=tax", accessToken);
                List<Product> litems = ProductItem(baseUrl, "/v2/catalog/list?types=item", accessToken);
                List<Count> listInv = InvCount(baseUrl, accessToken, LocationId);
                List<Inventory> linventory = null;// Inventory(baseUrl, accessToken, LocationId);
                List<v1Items> lv1items = null;//ListV1Items(baseUrl, accessToken, LocationId);
                List<SquareProductModel> listproduct = new List<SquareProductModel>();
                List<QuantityList> qtylist = new List<QuantityList>();
                List<SquareCSvProductModelDemo> listDemo = new List<SquareCSvProductModelDemo>();

                List<ProductList> prdlists = new List<ProductList>();
                foreach (var item in litems)
                {
                    try
                    {
                        if (item.item_data.variations != null)
                        {
                            var cnt = item.item_data.variations.Count;
                            for (int i = 0; i < cnt; i++)
                            {
                                ProductList prdlist = new ProductList();

                                prdlist.varitionId = item.item_data.variations[i].id;//72R6EMATHICMMXHFPMEHATPW
                                prdlist.upc = item.item_data.variations[i].item_variation_data.upc;
                                prdlist.sku = item.item_data.variations[i].item_variation_data.sku;
                                prdlist.size = item.item_data.variations[i].item_variation_data.name;
                                prdlist.prodname = item.item_data.name;
                                if (StoreId == 11361)              //08-03-2022 #13275
                                {
                                    prdlist.prodname = item.item_data.variations[i].item_variation_data.name;
                                }
                                prdlist.description = item.item_data.description;
                                if (StoreId == 11361)
                                {
                                    prdlist.description = item.item_data.variations[i].item_variation_data.name;
                                }
                                prdlist.category_id = item.item_data.category_id;
                                prdlist.amount = item.item_data.variations[i].item_variation_data.price_money == null ? 0 : item.item_data.variations[i].item_variation_data.price_money.amount;
                                prdlists.Add(prdlist);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                if (StoreId == 10488)
                {
                    SquareCSvProductModelDemo demo = new SquareCSvProductModelDemo();
                    var tt = (from p in prdlists
                              join r in listInv on p.varitionId equals r.catalog_object_id into rt
                              from tf in rt.DefaultIfEmpty()
                              join l in listInv on p.varitionId equals l.catalog_object_id
                              where l.state == "IN_STOCK"
                              select new SquareCSvProductModelDemo
                              {
                                  storeid = StoreId,
                                  StoreProductName = p.prodname == null ? "" : p.prodname,
                                  StoreDescription = p.prodname == null ? "" : p.prodname,
                                  upc = p.sku == null ? "" : p.sku,
                                  sku = p.sku == null ? "" : p.sku,
                                  Tax = tax,
                                  Price = p.amount == null ? 0 : p.amount,
                                  state = l.state,
                                  qty = l.quantity == null ? 0 : l.quantity == "" ? 0 : Convert.ToInt32(Convert.ToDouble(l.quantity)),                                
                                  categoryid = p.category_id == null ? "" : p.category_id,
                                  pack = 1,
                                  sprice = 0,
                                  altupc1 = "",
                                  altupc2 = "",
                                  altupc3 = "",
                                  altupc4 = "",
                                  altupc5 = ""
                              }).Distinct().ToList(); 
                    listDemo.AddRange(tt);

                    listDemo = listDemo.GroupBy(x => x.upc).Select(y => y.LastOrDefault()).ToList();

                    var productfiles = (from c in listDemo
                                        where (c.categoryid == "37HKGII6MMZNMEMSVVQ5ZMDD" || c.categoryid == "5GB55XOSZ75GYZUXWR5YWUZ2" || c.categoryid == "ZZCDLQC67QQKQRRPRR4CCAFE"
                                        || c.categoryid == "YQUGMXDH4VBT2HEXFOWGGZ2B" || c.categoryid == "AGDSS4A765NCLQFL6RIU777B" || c.categoryid == "4Q35NC2Z4DABUWGBBPWMOREG" || c.categoryid == "VMBMO4KAWT4OGOZ35OEOOO4C"
                                        || c.categoryid == "S6MVR6ZVJXLQKC5LDFRCPVTD" || c.categoryid == "OX6AN6PZJ33TPZ6PF44WHH4H" || c.categoryid == "G4ENHLEZGAD3XMJI5JYBN4W4" || c.categoryid == "4ZGOGIKYUYFQGPXZYDK7ZAM5"
                                        || c.categoryid == "5RYNZDGLVS3CGMZTYNDF3MHA" || c.categoryid == "GAYJOHFX4MRHGA3XWCASYATU" || c.categoryid == "XB4ZIXECVIINYRAWQTX37WYG" || c.categoryid == "P3KV6KXMXEXBNIMNKT2W2WA2"
                                        || c.categoryid == "I43L5AN75EQMBO3FCACAV3IW" || c.categoryid == "CEHIHBCRL5SKZOWNS45Y5P7L" || c.categoryid == "HCU7JNSZAWETQ6YTDLM37M2R" || c.categoryid == "PO5HILO5DYJXEMKQSTK3T6I3"
                                        || c.categoryid == "JKGC3JSID5BMRAX7L654XMOF" || c.categoryid == "47QVB2OV74TAABD27O226DK3" || c.categoryid == "IBCZARGMWJAMRGVMKK7WCDGI" || c.categoryid == "2K6MWTK3Z3WOXLXKV35IPOP5"
                                        || c.categoryid == "56JKIBA6FRUV6AX4YJUKASNB" || c.categoryid == "028930bd-c91b-4aa0-b5f2-eee39b597bb6" || c.categoryid == "OG7VQVGTEWWNPA3IF4EO7CSH" || c.categoryid == "COM23SE2XN6FBXJOMNBPFI5A")   ///tckt 7647 made categoryid:COM23SE2XN6FBXJOMNBPFI5A-Isapp-GiftBaskets
                                        select new SquareCSvProductModel
                                        {
                                            storeid = c.storeid,
                                            upc = "#" + c.upc,
                                            StoreProductName = c.StoreProductName,
                                            StoreDescription = c.StoreProductName,
                                            sku = "#" + c.sku,
                                            qty = c.qty,
                                            pack = c.pack,
                                            Price = (decimal)c.Price / 100,
                                            sprice = c.sprice,
                                            Tax = c.categoryid == "37HKGII6MMZNMEMSVVQ5ZMDD" || c.categoryid == "XB4ZIXECVIINYRAWQTX37WYG" || c.categoryid == "4ZGOGIKYUYFQGPXZYDK7ZAM5" || c.categoryid == "OG7VQVGTEWWNPA3IF4EO7CSH" ? 0 : tax,
                                            Start = "",
                                            End = "",
                                            altupc1 = "",
                                            altupc2 = "",
                                            altupc3 = "",
                                            altupc4 = "",
                                            altupc5 = "",
                                        }).ToList();
                    GenerateCSV.GenerateCSVFile(productfiles, "PRODUCT", StoreId, folderPath);
                }
                else if (StoreId == 11361)
                {
                    try
                    {
                        SquareCSvProductModelDemo demo = new SquareCSvProductModelDemo();
                        var tt = (from p in prdlists
                                  join r in listInv on p.varitionId equals r.catalog_object_id into rt
                                  from tf in rt.DefaultIfEmpty()
                                  join l in listInv on p.varitionId equals l.catalog_object_id
                                  where l.state == "IN_STOCK"  //08/02/2022 #10123
                                  select new SquareCSvProductModelDemo
                                  {
                                      storeid = StoreId,
                                      StoreProductName = p.prodname == null ? "" : p.prodname,
                                      StoreDescription = p.prodname == null ? "" : p.prodname,
                                      uom = p.size,
                                      upc = p.sku == null ? "" : p.sku,
                                      sku = p.sku == null ? "" : p.sku,
                                      Tax = tax,
                                      Price = p.amount,
                                      state = l.state,
                                      qty = l.quantity == null ? 0 : l.quantity == "" ? 0 : Convert.ToInt32(Convert.ToDouble(l.quantity)),
                                      categoryid = p.category_id == null ? "" : p.category_id,
                                      pack = 1,
                                      sprice = 0,
                                      altupc1 = "",
                                      altupc2 = "",
                                      altupc3 = "",
                                      altupc4 = "",
                                      altupc5 = ""
                                  }).Distinct().ToList();
                        listDemo.AddRange(tt);

                        listDemo = listDemo.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();

                        if (!string.IsNullOrEmpty(strcategory))
                        {
                            List<storecat> clscat = JsonConvert.DeserializeObject<List<storecat>>(strcategory);
                            if (clscat != null)
                            {
                                var tquery = litems.Where(x => clscat.Any(y => y.catid == x.item_data.category_id));
                            }
                        }

                        var productfile = (from c in listDemo
                                           where (c.upc ?? "").All(char.IsDigit)
                                           select new SquareCSvProductModels
                                           {
                                               storeid = c.storeid,
                                               upc = "#" + c.upc,
                                               uom = c.uom,
                                               StoreProductName = c.StoreProductName,
                                               StoreDescription = c.StoreProductName,
                                               sku = "#" + c.sku,
                                               qty = c.qty,
                                               pack = c.pack,
                                               Price = (decimal)c.Price / 100,
                                               sprice = (decimal)c.sprice,
                                               Tax = c.Tax,
                                               Start = "",
                                               End = "",
                                               altupc1 = "",
                                               altupc2 = "",
                                               altupc3 = "",
                                               altupc4 = "",
                                               altupc5 = "",

                                           }).ToList();


                        GenerateCSV.GenerateCSVFile(productfile, "PRODUCT", StoreId, folderPath);
                    }
                    catch (Exception ex)
                    {

                    }

                }
                else
                {
                    try
                    {
                        SquareCSvProductModelDemo demo = new SquareCSvProductModelDemo();
                        var tt = (from p in prdlists
                                  join r in listInv on p.varitionId equals r.catalog_object_id into rt
                                  from tf in rt.DefaultIfEmpty()
                                  join l in listInv on p.varitionId equals l.catalog_object_id
                                  where l.state == "IN_STOCK"  //08/02/2022 #10123
                                  select new SquareCSvProductModelDemo
                                  {
                                      storeid = StoreId,
                                      StoreProductName = p.prodname == null ? "" : p.prodname,
                                      StoreDescription = p.prodname == null ? "" : p.prodname,
                                      upc = p.sku == null ? "" : p.sku,
                                      sku = p.sku == null ? "" : p.sku,
                                      Tax = tax,
                                      Price = p.amount,
                                      state = l.state,
                                      qty = l.quantity == null ? 0 : l.quantity == "" ? 0 : Convert.ToInt32(Convert.ToDouble(l.quantity)),
                                      categoryid = p.category_id == null ? "" : p.category_id,
                                      pack = 1,
                                      sprice = 0,
                                      altupc1 = "",
                                      altupc2 = "",
                                      altupc3 = "",
                                      altupc4 = "",
                                      altupc5 = ""
                                  }).Distinct().ToList();
                        listDemo.AddRange(tt);

                        listDemo = listDemo.GroupBy(x => x.upc).Select(y => y.FirstOrDefault()).ToList();

                        if (!string.IsNullOrEmpty(strcategory))
                        {
                            List<storecat> clscat = JsonConvert.DeserializeObject<List<storecat>>(strcategory);
                            if (clscat != null)
                            {
                                var tquery = litems.Where(x => clscat.Any(y => y.catid == x.item_data.category_id));
                            }
                        }

                        var productfile = (from c in listDemo
                                           where (c.upc ?? "").All(char.IsDigit)
                                           select new SquareCSvProductModel
                                           {
                                               storeid = c.storeid,
                                               upc = "#" + c.upc,
                                               StoreProductName = c.StoreProductName,
                                               StoreDescription = c.StoreProductName,
                                               sku = "#" + c.sku,
                                               qty = c.qty,
                                               pack = c.pack,
                                               Price = (decimal)c.Price / 100,
                                               sprice = (decimal)c.sprice,
                                               Tax = c.Tax,
                                               Start = "",
                                               End = "",
                                               altupc1 = "",
                                               altupc2 = "",
                                               altupc3 = "",
                                               altupc4 = "",
                                               altupc5 = "",

                                           }).ToList();


                        GenerateCSV.GenerateCSVFile(productfile, "PRODUCT", StoreId, folderPath);
                    }
                    catch (Exception ex)
                    {

                    }

                }
                //var fullnamequery = (from p in tqtyquery
                //                     join t in lcategory on p.CategoryName.ToUpper() equals t.category_data.name.ToUpper() into tf
                //                     from rt in tf.DefaultIfEmpty()
                //                     where (p.Upc ?? "").All(char.IsDigit) && rt.category_data.name != null && p.CategoryName != null
                //                     select new FullNameProductModel
                //                     {
                //                         upc = "#" + p.Upc,
                //                         pname = p.StoreProductName,
                //                         pdesc = p.StoreProductName,
                //                         pcat = rt == null ? "" : rt.category_data.name,
                //                         pcat1 = "",
                //                         pcat2 = "",
                //                         Price = (decimal)p.Price,
                //                         uom = "",
                //                         country = "",
                //                         region = ""
                //                     }).ToList();



                //GenerateCSV.GenerateCSVFile(fullnamequery, "FULLNAME", StoreId, folderPath);
            }
            catch (Exception ex)
            {
                if (StoreId == 10430)
                {
                    Console.WriteLine("" + ex.Message);
                }
                else
                {
                    Console.WriteLine(ex.Message);
                    (new clsEmail()).sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + StoreId + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                }
            }
            finally { }
        }
        public class Demo
        {
            public string VId { get; set; }
            public int Qty { get; set; }
        }
    }
}
