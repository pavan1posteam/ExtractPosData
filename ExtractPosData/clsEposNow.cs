using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Xaml;
using System.Web.Script.Serialization;
using System.IO;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using RestSharp.Authenticators;
using System.Net;
using System.Runtime.Serialization;
using System.Configuration;
using ExtractPosData.Model;
using ExtractPosData.Models;

namespace ExtractPosData
{
    public class clsEposNow
    {
        string baseUrl = ConfigurationManager.AppSettings.Get("BaseDirectory");
        private string StoreId;
        int page = 1;
        string AccessToken = "";


        public clsEposNow(int StoreId, decimal tax, string BaseUrl, string RefreshToken)
        {
            try
            {
                Console.WriteLine("Generating EposNow " + StoreId + " Product File....");
                Console.WriteLine("Generating EposNow " + StoreId + " Fullname File....");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " EposNow " + StoreId);
            }
        }
        public List<EposnowProdList.Root> EposnowSetting(int StoreId, decimal tax, string BaseUrl, string Token)
        {
  
            List<EposnowProdList.Root> productList = new List<EposnowProdList.Root>();
            for (int i = 1; i <= 25; i++)
            {
                var prodList = EposNowProduct(i, StoreId, tax, BaseUrl, Token);
                if (prodList.Count != 0)
                {
                    productList.AddRange(prodList);
                }
                else
                {
                    break;
                }
            }
            return productList;
        }

        public List<EposnowStockList.Root> EposnowStockSetting(int StoreId, decimal tax, string BaseUrl, string Token)
        {
            List<EposnowStockList.Root> stock = new List<EposnowStockList.Root>();
            for (int i = 1; i <= 25; i++)
            {
                var stockList = EposNowStock(i, StoreId, tax, BaseUrl, Token);
                if (stockList.Count != 0)
                {
                    stock.AddRange(stockList);
                }
                else
                {
                    break;
                }
            }
            return stock;
        }
        public List<EposnowProdList.Root> EposNowProduct(int PageNo, int StoreId, decimal tax, string BaseUrl, string Token)
        {
            List<EposnowProdList.Root> productList = new List<EposnowProdList.Root>(); ;

            string authInfo = Token;
           
            string content = null;
          
            EposnowProdList.Root prod = new EposnowProdList.Root();

                var client = new RestClient(BaseUrl + "Product/?page=" + PageNo + "&limit=200");
                var request = new RestRequest(Method.GET);
                request.AddHeader("Authorization", authInfo);
        
                request.AddHeader("Content-Type", "application/json");
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                IRestResponse response = client.Execute(request);

                var headers = response.Headers.ToList();
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    try
                    {
                        content = response.Content;
                   
                        var result = JsonConvert.DeserializeObject<List<EposnowProdList.Root>>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                        productList = result.ToList();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

           // }
            return productList;
        }


        public List<EposnowStockList.Root> EposNowStock(int PageNo, int StoreId, decimal tax, string BaseUrl, string Token)
        {

            List<JArray> StockList = new List<JArray>(); ;

            List<EposnowStockList.Root> ss = new List<EposnowStockList.Root>();

            string authInfo = Token;

            string content = null;
           
            Root stock = new Root();

            var client = new RestClient(BaseUrl + "ProductStock?page=" + PageNo + "&limit=200");
          
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", authInfo);

            request.AddHeader("Content-Type", "application/json");
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            IRestResponse response = client.Execute(request);

            var headers = response.Headers.ToList();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {

                    content = response.Content;
                  
                    var result = JsonConvert.DeserializeObject<List<EposnowStockList.Root>>(content, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                      
                    ss = result.ToList();


                }
                catch (Exception e)
                { Console.WriteLine(e.Message); }
            }

            return ss;
        }
    }

    public class EposnowProdList
    {
        public class Supplier
        {
            public Root Roots { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string AddressLine1 { get; set; }
            public object AddressLine2 { get; set; }
            public string Town { get; set; }
            public string County { get; set; }
            public string PostCode { get; set; }
            public string ContactNumber { get; set; }
            public object ContactNumber2 { get; set; }
            public object EmailAddress { get; set; }
            public object Type { get; set; }
            public object ReferenceCode { get; set; }
        }

        public class TaxRate
        {
            public int TaxGroupId { get; set; }
            public int TaxRateId { get; set; }
            public int LocationId { get; set; }
            public int Priority { get; set; }
            public double Percentage { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string TaxCode { get; set; }
        }

        public class SalePriceTaxGroup
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<TaxRate> TaxRates { get; set; }
        }

        public class EatOutPriceTaxGroup
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<TaxRate> TaxRates { get; set; }
        }

        public class CostPriceTaxGroup
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<TaxRate> TaxRates { get; set; }
        }

        public class Root
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public double CostPrice { get; set; }
            public bool IsCostPriceIncTax { get; set; }
            public double SalePrice { get; set; }
            public bool IsSalePriceIncTax { get; set; }
            public double EatOutPrice { get; set; }
            public bool IsEatOutPriceIncTax { get; set; }
            public int CategoryId { get; set; }
            public string Barcode { get; set; }
            public int SalePriceTaxGroupId { get; set; }
            public int EatOutPriceTaxGroupId { get; set; }
            public int CostPriceTaxGroupId { get; set; }
            public object BrandId { get; set; }
            public int SupplierId { get; set; }
            public object PopupNoteId { get; set; }
            public object UnitOfSale { get; set; }
            public object VolumeOfSale { get; set; }
            public object VariantGroupId { get; set; }
            public object MultipleChoiceNoteId { get; set; }
            public object Size { get; set; }
            public object Sku { get; set; }
            public bool SellOnWeb { get; set; }
            public bool SellOnTill { get; set; }
            public string OrderCode { get; set; }
            public object SortPosition { get; set; }
            public object RrPrice { get; set; }
            public int ProductType { get; set; }
            public object TareWeight { get; set; }
            public object ArticleCode { get; set; }
            public bool IsTaxExemptable { get; set; }
            public object ReferenceCode { get; set; }
            public bool IsVariablePrice { get; set; }
            public bool IsArchived { get; set; }
            public object ColourId { get; set; }
            public object MeasurementDetails { get; set; }
            public Supplier Supplier { get; set; }
            public SalePriceTaxGroup SalePriceTaxGroup { get; set; }
            public EatOutPriceTaxGroup EatOutPriceTaxGroup { get; set; }
            public CostPriceTaxGroup CostPriceTaxGroup { get; set; }
            public List<object> ProductTags { get; set; }
            public List<object> ProductUdfs { get; set; }
            public List<object> ProductLocationAreaPrices { get; set; }
            public List<object> ProductImages { get; set; }
            public bool IsMultipleChoiceProductOptional { get; set; }
        }
    }
    public class EposnowStockList
    {
        public class ProductStockBatch
        {
            public int Id { get; set; }
            public int ProductStockId { get; set; }
            public DateTime CreatedDate { get; set; }
            public int CurrentStock { get; set; }
            public int CurrentVolume { get; set; }
            public double CostPrice { get; set; }
            public int? SupplierId { get; set; }
            public object CostPriceMeasurementSchemeItemId { get; set; }
            public object CostPriceMeasurementUnitVolume { get; set; }
            public object CostPriceUnitFactor { get; set; }
            public object CostPriceUnit { get; set; }
            public object StockMeasurementSchemeItemId { get; set; }
            public object StockUnit { get; set; }
            public object StockFactor { get; set; }
            public object MeasurementDetails { get; set; }
        }

        public class Root
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int LocationId { get; set; }
            public int MinStock { get; set; }
            public int MaxStock { get; set; }
            public object MinimumOrderAmount { get; set; }
            public object MultipleOrderAmount { get; set; }
            public List<ProductStockBatch> ProductStockBatches { get; set; }
        }
    }
    public class EposnowCatList
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Root
        {
            public int Id { get; set; }
            public object ParentId { get; set; }
            public object RootParentId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public object ImageUrl { get; set; }
            public object PopupNoteId { get; set; }
            public bool IsWet { get; set; }
            public bool ShowOnTill { get; set; }
            public object ReferenceCode { get; set; }
            public object PopupNote { get; set; }
            public List<object> Children { get; set; }
            public object SortPosition { get; set; }
            public object ReportingCategoryId { get; set; }
            public string NominalCode { get; set; }
        }


    }


    public class EposnowCsvProducts
    {
        string BasePath = ConfigurationManager.AppSettings.Get("BaseDirectory");

        public EposnowCsvProducts(int StoreId, decimal tax, string BaseUrl, string Token)
        {
            productForCSV(StoreId, tax, BaseUrl, Token);
        }
        public void productForCSV(int storeid, decimal tax, string BaseUrl, string Token)
        {
            clsEposNow products = new clsEposNow(storeid, tax, BaseUrl, Token);
       
            var productList = products.EposnowSetting(storeid, tax, BaseUrl, Token);
            var StockList = products.EposnowStockSetting(storeid, tax, BaseUrl, Token);
          

            List<ProductsModel> pf = new List<ProductsModel>();
            List<FullNameProductModel> fn = new List<FullNameProductModel>();
            try
            {

                var prodList = (from b in productList
                                join a in StockList on b.Id equals a.ProductId
                          
                                select new
                                {
                                    storeid = storeid,
                                    upc = b.Barcode == null ? "" : b.Barcode,
                                    qty = a.ProductStockBatches.ToList().Count<=0?0:a.ProductStockBatches.ToList().FirstOrDefault().CurrentStock,
                                    sku = b.Barcode == null ? "" : b.Barcode,
                                    pack = b.Id,
                                    StoreProductName = b.Name,
                                    StoreDescription = b.Name,
                                    price = b.SalePrice,
                                    sprice = 0,
                                    start = "",
                                    end = "",
                                    tax = tax,
                                    altupc1 = "",
                                    altupc2 = "",
                                    altupc3 = "",
                                    altupc4 = "",
                                    altupc5 = ""
                                }).Distinct().Select(x => new ProductsModel()
                                {
                                    StoreID = x.storeid,
                                    upc = x.upc,
                                   Qty = Convert.ToInt64(x.qty),
                                    sku = x.sku,
                                    pack = 1,
                                    StoreProductName = x.StoreProductName,
                                    StoreDescription = x.StoreDescription,
                                    Price = Convert.ToDecimal(x.price),
                                    sprice =0,
                                    Start = x.start,
                                    End = x.end,
                                    Tax = x.tax,
                                    altupc1 = x.altupc1,
                                    altupc2 = x.altupc2,
                                    altupc3 = x.altupc3,
                                    altupc4 = x.altupc4,
                                    altupc5 = x.altupc5
                                }).ToList();

          
                foreach (var item in prodList)
                {
                    try
                    {
                        ProductsModel pdf = new ProductsModel();
                        FullNameProductModel fnf = new FullNameProductModel();

                        pdf.StoreID = storeid;


                        decimal result;
                        string upc = "";
                        if (item.upc == "") { continue; }
                        else
                        {
                            upc = item.upc.ToString();
                        }

                        Decimal.TryParse(upc, System.Globalization.NumberStyles.Float, null, out result);
                        upc = result.ToString();

                        if (upc == "" || upc == "0")
                        {
                            pdf.upc = "";


                            fnf.upc = "";
                        }
                   
                        else
                        {
                            pdf.upc = "#" + upc;

                            fnf.upc = "#" + upc;
                            pdf.sku = "#" + upc;
                            fnf.sku = "#" + upc;
                        }

                        pdf.Qty = item.Qty;
                        pdf.pack = item.pack;
                        pdf.StoreProductName = item.StoreProductName.ToString();
                        pdf.StoreDescription = item.StoreProductName.ToString();
                        if (item.Price <= 0) { continue; }
                        else
                        {
                            pdf.Price = Convert.ToDecimal(item.Price);
                            fnf.Price = Convert.ToDecimal(item.Price);
                        }
                        pdf.sprice = item.sprice;
                       
                       pdf.Tax = tax;
                       
                        pdf.altupc1 = "";
                        pdf.altupc2 = "";
                        pdf.altupc3 = "";
                        pdf.altupc4 = "";
                        pdf.altupc4 = "";
                        pdf.altupc5 = "";

                       fnf.pname = item.StoreProductName.ToString();
                      
                        fnf.pdesc = item.StoreProductName.ToString();

                        fnf.pack = 1;                                                
                        fnf.pcat2 = "";
                        fnf.country = "";
                        fnf.region = "";
                        string short_name = "";
                        if(item.StoreDescription.Count()>10)
                        {
                            short_name = item.StoreDescription.Substring(5, 5);
                        }                        
                        if (!string.IsNullOrEmpty(pdf.upc) && pdf.Price > 0 && pdf.Qty > 0 && short_name != " DAN-")
                        {
                            pf.Add(pdf);
                            fn.Add(fnf);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally 
                    { }
                }
                if (storeid == 11395)
                {
                    var prodLists = (from b in productList
                                     where b.CategoryId == 565448
                                     select new ProductsModel
                                     {
                                         StoreID = storeid,
                                         upc = b.Barcode == null ? "" : '#' + b.Barcode,
                                         Qty = 999,
                                         sku = b.Barcode == null ? "" : '#' + b.Barcode,
                                         pack = 1,
                                         StoreProductName = b.Name,
                                         StoreDescription = b.Name,
                                         Price = (decimal)b.SalePrice,
                                         sprice = 0,
                                         Start = "",
                                         End = "",
                                         Tax = tax,
                                         altupc1 = "",
                                         altupc2 = "",
                                         altupc3 = "",
                                         altupc4 = "",
                                         altupc5 = ""
                                     }).ToList();
                    pf.AddRange(prodLists);
                    pf = pf.AsEnumerable()
                                       .GroupBy(x => x.upc)
                                       .Select(y => y.First())
                                       .ToList();
                    GenerateCSV.GenerateCSVFile(pf, "PRODUCT", storeid, BasePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
            }
                GenerateCSV.GenerateCSVFile(pf, "PRODUCT", storeid, BasePath);               
                GenerateCSV.GenerateCSVFile(fn, "FULLNAME", storeid, BasePath);
                Console.WriteLine();
                Console.WriteLine("Product FIle Generated For EposNow " + storeid);
                Console.WriteLine("Fullname FIle Generated For EposNow " + storeid);
          
        }

    }
}