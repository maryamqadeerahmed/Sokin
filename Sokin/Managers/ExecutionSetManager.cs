using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using Sokin.FeatureFiles.CorporatePortal.ChangePassword;
using Sokin.FeatureFiles.CorporatePortal.InitialLogin;
using Sokin.FeatureFiles.DigitalBankingBackOffice.CorporateOnboarding;
using Sokin.Helper;
using Sokin.Helper.Database;
using Sokin.Helper.Excel;
using Sokin.Model.Common;
using Sokin.Model.Core;
using Sokin.Model.CorporatePortal;
using Sokin.Model.Suite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Managers
{
    class ExecutionSetManager
    {
        #region variables
        private IWebDriver WebDriver;
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private DataConnection IrisDataConnection;
        private HelperClass HelpObj;
        private Exception Error = new Exception();
        private Dictionary<string, string> InitialLoginXML;
        private Dictionary<string, string> CorporateOnboardingXML;
        private Dictionary<string, string> ChangePasswordXML;
        private DbValidator dbValidator;
        private Core modelCore;
        private Dictionary<string, string> dataSet;
        private List<ExecutionSetEntry> dataColExecutionSet;
        private List<TestCaseChangePassword> TestCasesChangePassword = new List<TestCaseChangePassword>();
        #endregion

        #region Constructor
        public ExecutionSetManager(List<ExecutionSetEntry> dataColExecutionSet, Dictionary<string, string> dataSet, DataConnection IrisDataConnection, HelperClass HelpObj, Dictionary<string, string> InitialLoginXML, Dictionary<string, string> ChangePasswordXML, Dictionary<string, string> CorporateOnboardingXML)
        {
            this.HelpObj = HelpObj;
            this.logger = HelpObj.getLoggerObj();
            this.dataSet = dataSet;
            this.InitialLoginXML = InitialLoginXML;
            this.ChangePasswordXML = ChangePasswordXML;
            this.CorporateOnboardingXML = CorporateOnboardingXML;
            this.IrisDataConnection = IrisDataConnection;
            this.dataColExecutionSet = dataColExecutionSet;
        }
        #endregion

        #region Run
        public void Run()
        {
            initializeWebdriver("google");
            callerMethod();
        }
        #endregion

        #region Main Methods

        private IWebDriver initializeWebdriver(string browserName)
        {
            if (browserName == "google")
            {
                WebDriver = new ChromeDriver();
            }
            else if (browserName == "firefox")
            {
                FirefoxOptions options = new FirefoxOptions()
                {
                    AcceptInsecureCertificates = false
                };
                WebDriver = new FirefoxDriver(options);
            }
            else
            {
                WebDriver = new ChromeDriver();
            }
            return WebDriver;
        }

        private void callerMethod()
        {
            try
            {
                #region Executable Entries Extraction

                List<ExecutionSetEntry> SuiteExecutionSetEntries = (from dataColExecutionSetEntry in dataColExecutionSet
                                                                    where dataColExecutionSetEntry.Executable.Equals("Y") && dataColExecutionSetEntry.NATURE.ToUpper().Equals("CORPORATEPORTAL")
                                                                    select dataColExecutionSetEntry).ToList();

               /* List<ExecutionSetEntry> transactionExecutionSetEntries = (from dataColExecutionSetEntry in dataColExecutionSet
                                                                          where dataColExecutionSetEntry.Executable.Equals("Y") && dataColExecutionSetEntry.NATURE.ToUpper().Equals("TRANSACTION")
                                                                          select dataColExecutionSetEntry).ToList(); */
                #endregion

                #region variable creation
                List<String> TXN_ListACC;
                List<String> TXN_ListCARD;

                int Flagreport = 0;
                #endregion


                #region SUITE
                try
                {
                    for (int i = 0; i < SuiteExecutionSetEntries.Count(); i++)
                    {
                        if (SuiteExecutionSetEntries[i].ExecutionType.Equals("CorporateOnboarding"))
                        {
                            CorporateOnboarding addCorp = new CorporateOnboarding(dataSet, IrisDataConnection, HelpObj, CorporateOnboardingXML);
                            addCorp.AddCorporate();

                        }
                        if (SuiteExecutionSetEntries[i].ExecutionType.Equals("InitialLogin"))
                        {
                            InitialLogin initLogin = new InitialLogin(dataSet, IrisDataConnection, HelpObj, InitialLoginXML);
                            initLogin.Login(WebDriver, dataSet);
                        }

                        if (SuiteExecutionSetEntries[i].ExecutionType.Equals("ChangePassword"))
                        {
                            ChangePassword changePassword = new ChangePassword(dataSet, IrisDataConnection, HelpObj, ChangePasswordXML);
                            changePassword.FnChangePassword(WebDriver, dataSet);
                        }


                        //if (SuiteExecutionSetEntries[i].ExecutionType.Equals("Merchant_Addition"))
                        //{
                        //    Merchant merchant = new Merchant(PortalUser, CheckerUser, IrisDataConnection, HelpObj, mdlTransactionConfig.Browser, portalSimulator, MerchantOnBoardingXML, PortalXML);

                        //    merchant.Merchant_AdditionChecker();

                        //}


                        //if (SuiteExecutionSetEntries[i].ExecutionType.Equals("Merchant_Addition_Negative"))
                        //{
                        //    Merchant merchant = new Merchant(PortalUser, CheckerUser, IrisDataConnection, HelpObj, mdlTransactionConfig.Browser, portalSimulator, MerchantOnBoardingXML, PortalXML);
                        //    merchant.Merchant_Addition_Negative();
                        //}

                        //if (SuiteExecutionSetEntries[i].ExecutionType.Equals("Reports"))
                        //{
                        //    Flagreport = 1;
                        //    Account account = new Account(mdlTransactionConfig, IrisDataConnection, HelpObj, mdlCore, merchantSimulator, intrumentSimulator, genInquirySimulator, AccountXML);
                        //    TXN_ListACC = account.Positive();
                        //    Card card = new Card(mdlTransactionConfig, IrisDataConnection, HelpObj, mdlCore, merchantSimulator, intrumentSimulator, genInquirySimulator, CardXML);
                        //    TXN_ListCARD = card.Positive();

                        //    Reconciliation Recon = new Reconciliation(PortalUser, CheckerUser, IrisDataConnection, HelpObj, mdlTransactionConfig.Browser, portalSimulator, SettlementXML, MonitoringXML);
                        //    Recon.Positive();
                        //    Reports report = new Reports(PortalUser, CheckerUser, IrisDataConnection, HelpObj, mdlTransactionConfig.Browser, portalSimulator, ReportsXML, SuiteLogs);
                        //    report.Positive(TXN_ListACC, TXN_ListCARD);
                        //    report.Negative();
                        //}

                    }
                }
                catch (Exception ex)
                {
                    HelpObj.LogInfo("Error occured while executing suite test cases !! error: " + ex.ToString());
                }
                #endregion

               /* #region TRANSACTION
                try
                {
                    for (int i = 0; i < transactionExecutionSetEntries.Count(); i++)
                    {
                        try
                        {
                            if (transactionExecutionSetEntries[i].ExecutionType.Contains("Account"))
                            {
                                Account account = new Account(mdlTransactionConfig, IrisDataConnection, HelpObj, mdlCore, merchantSimulator, intrumentSimulator, genInquirySimulator, AccountXML);
                                if (transactionExecutionSetEntries[i].ExecutionType.Equals("Account_Positive"))
                                {
                                    if (Flagreport == 0) { account.Positive(); }
                                }
                                if (transactionExecutionSetEntries[i].ExecutionType.Equals("Account_Negative"))
                                { account.Negative(); }
                            }

                            if (transactionExecutionSetEntries[i].ExecutionType.Contains("Card"))
                            {
                                Card card = new Card(mdlTransactionConfig, IrisDataConnection, HelpObj, mdlCore, merchantSimulator, intrumentSimulator, genInquirySimulator, CardXML);

                                if (transactionExecutionSetEntries[i].ExecutionType.Equals("Card_Positive"))
                                {
                                    if (Flagreport == 0) { card.Positive(); }
                                }
                                if (transactionExecutionSetEntries[i].ExecutionType.Equals("Card_Negative"))
                                {
                                    card.Negative();
                                }
                            }

                            if (transactionExecutionSetEntries[i].ExecutionType.Contains("Inquiry"))
                            {

                                //if (transactionExecutionSetEntries[i].ExecutionType.Equals("Inquiry_Positive"))
                                //{
                                //    Card card = new Card(mdlTransactionConfig, IrisDataConnection, HelpObj, mdlCore, merchantSimulator, intrumentSimulator, genInquirySimulator, CardXML);
                                //    Account account = new Account(mdlTransactionConfig, IrisDataConnection, HelpObj, mdlCore, merchantSimulator, intrumentSimulator, genInquirySimulator, AccountXML);
                                //    card.CardFlowInquiry();
                                //    account.AccInquiryFlow();
                                //}
                                //else 
                                if (transactionExecutionSetEntries[i].ExecutionType.Equals("Inquiry_Negative"))
                                {
                                    genInquirySimulator.GeneralInquiryNegative();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            HelpObj.LogInfo("Error occured while executing transaction test Entry: " + transactionExecutionSetEntries[i].ExecutionType + "  !! error: " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    HelpObj.LogInfo("Error occured while executing transaction test cases !! error: " + ex.ToString());
                }
                #endregion  */
            }
            catch (Exception ex)
            {
                HelpObj.LogInfo("TransactionSetManager error: " + ex.ToString());
            }
            HelpObj.getReportObj().flushExtent();
        }
        #endregion
   }
}