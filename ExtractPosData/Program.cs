using ExtractPosData.Models;
using System;
using System.Configuration;

namespace ExtractPosData/// Abhishek sir Project 
{
    class Program
    {
        private static void Main(string[] args)
        {
            string DeveloperId = ConfigurationManager.AppSettings["DeveloperId"];
            string SurfLiquor_KeyStroke = ConfigurationManager.AppSettings["SurfLiquor_KeyStroke"];
            string SpiritsFinewine_Keystroke = ConfigurationManager.AppSettings["SpiritsFinewine_Keystroke"];
            string SigmansEcrs = ConfigurationManager.AppSettings["SigmansEcrs"];
            string Ecrs_21Packages = ConfigurationManager.AppSettings["Ecrs_21Packages"];
            string ECRS = ConfigurationManager.AppSettings["ECRS"];
            string ECRSMacadoodles = ConfigurationManager.AppSettings["ECRSMacadoodles"];
            string ECRS_10392 = ConfigurationManager.AppSettings["ECRS_10392"];
            string Catapult_11430_11435 = ConfigurationManager.AppSettings["Catapult_11430_11435"];
            string ECRSCATAPULT_XML = ConfigurationManager.AppSettings["ECRSCATAPULT_XML"];
            string POMODOCLOUD = ConfigurationManager.AppSettings["POMODOCLOUD"];
            string Spirits2000 = ConfigurationManager.AppSettings["Spirits2000"];

            try
            {
                POSSettings pOSSettings = new POSSettings();
                pOSSettings.IntializeStoreSettings();
                foreach (POSSetting current in pOSSettings.PosDetails)
                {
                    try
                    {
                        if (current.PosName.ToUpper() == "KEYSTROKE")
                        {
                            if (SurfLiquor_KeyStroke.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                ClsSurfLiquor_KeyStroke clsSurfLiquor_KeyStroke = new ClsSurfLiquor_KeyStroke(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                            else if (SpiritsFinewine_Keystroke.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsSpiritsFinewine_Keystroke clsSpiritsFinewine_Keystroke = new clsSpiritsFinewine_Keystroke(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                            else
                            {
                                clsKeyStroke clsKeyStroke = new clsKeyStroke(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                        }
                        else if (current.PosName.ToUpper() == "ECRS")
                        {
                            if (SigmansEcrs.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsSigmansEcrs clsSigmansEcrs = new clsSigmansEcrs(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                            if (Ecrs_21Packages.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                Ecrs_21Package clsECRS = new Ecrs_21Package(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                            else if (ECRS.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsECRS clsECRS = new clsECRS(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.liquortax, current.StoreSettings.POSSettings.liquortaxrateperlitre);
                                Console.WriteLine();
                            }
                            if (ECRSMacadoodles.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsECRSMacadoodles clsECRSMacadoodles = new clsECRSMacadoodles(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                            else if (ECRS_10392.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsECRS_10392 clsECRS = new clsECRS_10392(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                        }
                        else if (current.PosName.ToUpper() == "SHOPKEEP")
                        {
                            ShopKeep shopKeep = new ShopKeep(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "MAGSTAR")
                        {
                            MagStar magStar = new MagStar(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "GIFTLOGIC")
                        {
                            ClsGiftLogic clsGiftLogic = new ClsGiftLogic(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "RITE")
                        {
                            clsRite clsRite = new clsRite(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "VISION")
                        {
                            clsVision clsVision = new clsVision(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.IsMarkUpPrice, current.StoreSettings.POSSettings.MarkUpValue, current.StoreSettings.POSSettings.LiquorDiscount, current.StoreSettings.POSSettings.WineDiscount);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "LIQUORREGISTERMARKET")
                        {
                            clsLiqReg_Market_11181 LIQUORREGISTERMARKET = new clsLiqReg_Market_11181(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.POSSettings.PosFileName2, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "PROFITEER")
                        {
                            clsProfiteer clsProfiteer = new clsProfiteer(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.StoreMapId);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "RANDALLS")
                        {
                            clsRandalls clsRandalls = new clsRandalls(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "JMSC")
                        {
                            clsLiquorLand clsLiquorLand = new clsLiquorLand(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "LUKAS")
                        {
                            clsLukas clsLukas = new clsLukas(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "WINE")
                        {
                            clsWine clsWine = new clsWine(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "MODISOFT")
                        {
                            //if (current.StoreSettings.StoreId == 11792)
                            {
                                clsModisoft clsModisoft = new clsModisoft(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.LiquorDiscount, current.StoreSettings.POSSettings.WineDiscount);
                                Console.WriteLine();
                            }

                        }
                        else if (current.PosName.ToUpper() == "EHOPPER")
                        {
                            clsEHopper clsEHopper = new clsEHopper(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "PETRASOFT SMART")
                        {
                            clsPetrasoftSmart clsPetrasoftSmart = new clsPetrasoftSmart(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "QUICKBOOKS")
                        {
                            clsQuickBooks clsquickbooks = new clsQuickBooks(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "GOLDENKEY")
                        {
                            clsGoldenKey clsGoldenKey = new clsGoldenKey(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "EPOS")
                        {
                            clsEpos clsEpos = new clsEpos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "SPIRITS")
                        {
                            if (Spirits2000.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsSpirits2000 clsEpos = new clsSpirits2000(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                            else
                            {
                                clsSpirits clsEpos = new clsSpirits(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                        }
                        else if (current.PosName.ToUpper() == "CSTOREPRO")
                        {
                            clsCStorePro clsEpos = new clsCStorePro(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "MODUSTECH")
                        {
                            clsModusTech clsEpos = new clsModusTech(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "CREPOS")
                        {
                            clsCREPos clsEpos = new clsCREPos(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "CITRIXPOS")
                        {
                            clsCitrixPos clscitrix = new clsCitrixPos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "BOTTLEPOS")
                        {
                            clsBottlePos clsBottlePos = new clsBottlePos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "NRSPOS")
                        {
                            clsNRSPos NRSPOS = new clsNRSPos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.categoriess, current.StoreSettings.POSSettings.IsMarkUpPrice, current.StoreSettings.POSSettings.MarkUpValue);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "JCR_ECRS")
                        {
                            clsJCR_ECRS JCRECRSPOS = new clsJCR_ECRS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.StoreMapId);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "MICROBIZ")
                        {
                            clsMicrobiz microbiz = new clsMicrobiz(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.POSSettings.PosFileName2, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "SSCS")
                        {
                            SSCS SS = new SSCS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "EPOSNOW")
                        {
                            if (!current.StoreSettings.POSSettings.IsApi)
                            {
                                clsEposNow_FlatFile epsflat = new clsEposNow_FlatFile(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.IsMarkUpPrice, current.StoreSettings.POSSettings.MarkUpValue);
                                Console.WriteLine();
                            }
                        }
                        else if (current.PosName.ToUpper() == "LIQUORREGISTERMARKET")
                        {
                            clsLiqReg_Market_11181 LiquorReg_Market = new clsLiqReg_Market_11181(current.StoreSettings.POSSettings.PosFileName, current.StoreSettings.POSSettings.PosFileName2, current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "MAXTRIX")
                        {
                            clsMaxtrix_POS SS = new clsMaxtrix_POS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "INFOTOUCHPOS")
                        {
                            clsInfoTouchPos infoTouch = new clsInfoTouchPos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "RETAILEDGE")
                        {
                            clsRetaillEdge clsRetaillEdge = new clsRetaillEdge(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "RETAILPOSS")
                        {
                            clsRetailPOSS RetailPOSS = new clsRetailPOSS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "CATAPULT")
                        {
                            if (Catapult_11430_11435.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsCatapult_11430_11435 catapult = new clsCatapult_11430_11435(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.StoreMapId);
                                Console.WriteLine();
                            }
                            else if (ECRSCATAPULT_XML.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsCatapultXML catapultXML = new clsCatapultXML(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.winetax, current.StoreSettings.POSSettings.liquortax, current.StoreSettings.POSSettings.beertax);
                            }
                            else
                            {
                                clsCatapult catapult = new clsCatapult(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.liquortax, current.StoreSettings.POSSettings.beertax, current.StoreSettings.POSSettings.winetax, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                        }
                        else if (current.PosName.ToUpper() == "POMODO")
                        {
                            if (POMODOCLOUD.Contains(current.StoreSettings.StoreId.ToString()))
                            {
                                clsPOMODOCLOUD pomodocloud = new clsPOMODOCLOUD(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                            else
                            {
                                clsPOMODO pomodo = new clsPOMODO(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                                Console.WriteLine();
                            }
                        }
                        else if (current.PosName.ToUpper() == "ALOHAPOS")
                        {
                            clsAloha aLOHAPOS = new clsAloha(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "POSSYSTEMSINC")
                        {
                            clsPossystemsinc inc = new clsPossystemsinc(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "CETECH")
                        {
                            clsCetech CeTech = new clsCetech(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "LIGHTNING")
                        {
                            clsLightningPos Lightning = new clsLightningPos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "PCAMERICA")
                        {
                            clsPCAmerica PCAmerica = new clsPCAmerica(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "EZPOS")
                        {
                            clsEZ_POS EZPOS = new clsEZ_POS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "ADVENTPOSFLAT")
                        {
                            clsAdventPOS_flatfile AdentPOS = new clsAdventPOS_flatfile(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.MarkUpValue);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "FRONTIER")
                        {
                            clsFrontierPOS Frontier = new clsFrontierPOS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.StoreMapId);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "POSNATION")
                        {
                            clsPOSNation POSnation = new clsPOSNation(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "NEIMANNFOODSPOS")
                        {
                            clsNeimannFoodsPOS NeimannFoodsPOS = new clsNeimannFoodsPOS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "BEVERAGES2U")
                        {
                            clsBeverages2u clsBeverages2U = new clsBeverages2u(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "STCRPOS")
                        {
                            clsSTCRPOS STCR = new clsSTCRPOS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "RAPIDRMSMAPPINGTOOL")
                        {
                            clsRAPIDRMSMappingTool RAPIDRMS = new clsRAPIDRMSMappingTool(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax, current.StoreSettings.POSSettings.StoreMapId);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "VERIFONEORRUBYPOS")
                        {
                            clsVerifoneOrRubyPosXml VerifoneOrRubyPosXml = new clsVerifoneOrRubyPosXml(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if (current.PosName.ToUpper() == "TIGERPOS")
                        {
                            clsTigerPos clsTigerPos = new clsTigerPos(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                        else if(current.PosName.ToUpper()== "CUSTOMSOFTWAREPOS")
                        {
                            clsCustomSoftwarePOS customSoftware = new clsCustomSoftwarePOS(current.StoreSettings.StoreId, current.StoreSettings.POSSettings.tax);
                            Console.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                new clsEmail().sendEmail(DeveloperId, "", "", "Error in ExtractPOS@" + DateTime.UtcNow + " GMT", ex.Message + "<br/>" + ex.StackTrace);
                Console.WriteLine(ex.Message);
            }
            finally
            {
            }
        }
    }
}
