using AventStack.ExtentReports;
using OpenQA.Selenium;
using Sokin.Helper;
using Sokin.Helper.Database;
using Sokin.Helper.Excel;
using Sokin.Model.Common;
using Sokin.Model.CorporatePortal;
using Sokin.Model.Suite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sokin.FeatureFiles.CorporatePortal.InitialLogin
{
    class InitialLogin
    {
        private string pathToExcelFiles = "FeatureFiles/CorporatePortal/InitialLogin/ExcelFiles/";
        private string TestCases_ExcelFileName = "InitialLogin.xlsx";
        private string ExecutionSet_ExcelSheetName = "Positive";
        private TestCaseInitialLogin setTestCase;
        private IWebDriver WebDriver;
        private Dictionary<string, string> dataSet;
        private ExtentTest TestMaster;
        private DataConnection IrisDataConnection;
        private HelperClass HelpObj;
        private Dictionary<string, string> InitialLoginXML;
        private List<TestCaseInitialLogin> TestCasesExecutionList = new List<TestCaseInitialLogin>();

        public InitialLogin(Dictionary<string, string> dataSet, DataConnection IrisDataConnection, HelperClass HelpObj, Dictionary<string, string> InitialLoginXML)
        {
            this.dataSet = dataSet;
            this.IrisDataConnection = IrisDataConnection;
            this.HelpObj = HelpObj;
            this.InitialLoginXML = InitialLoginXML;
        }
        
        public string Login(IWebDriver WebDriver, Dictionary<string, string> dataSet)
        {
            this.WebDriver = WebDriver;
            TestMaster = HelpObj.getReportObj().CreateTest("Corporate Portal Login Test Cases Positive");
            string returnVal = "ABC";
            //try
            //{
                Run();
            //}
            //catch (Exception ex)
            //{
            //    returnVal = "Error occurred while logging in Corporate Portal Dashboard." + Environment.NewLine + "" + ex.ToString();
            //    HelpObj.LogError(returnVal);
            //}
            return returnVal;
        }

        public void Run()
        {
            #region Executable Entries Extraction
            ExtractTestCases();
            List<TestCaseInitialLogin> TestCases = (from entry in TestCasesExecutionList
                                             where entry.Executable.ToUpper().Equals("Y")
                                             select entry).ToList();

            #endregion Executable Entries Extraction
            foreach (TestCaseInitialLogin TestCase in TestCases)
            {
                if (TestCase.TestCaseType.ToLower() == "negative")
                {
                    TestCaseExecutionFlowNegative(TestCase);
                }
                else
                {
                    TestCaseExecutionFlowPositive(TestCase);
                }
            }
        }

        public void TestCaseExecutionFlowPositive(TestCaseInitialLogin TestCase)
        {
            CommonFlow(TestCase);
            HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporateLogoutButton")), 2 * 60);
            Thread.Sleep(5000);
            try
            {
                //HelpObj.FindElement(WebDriver, By.XPath(getXMLValue(InitialLoginXML, "CorporateLogoutButton")),5000);
                HelpObj.ButtonClickSimple(WebDriver, (By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporateLogoutButton"))));
            }
            catch (Exception)
            {
                HelpObj.LogError("Error: " + HelpObj.GetText(WebDriver, (By.XPath(getXMLValue(InitialLoginXML, "CorporateLoginErrorDiv")))));
            }
        }

        public void TestCaseExecutionFlowNegative(TestCaseInitialLogin TestCase)
        {
            CommonFlow(TestCase);
            HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporateLoginErrorButton")), 2 * 60);
            Thread.Sleep(5000);
            try
            {
                //HelpObj.FindElement(WebDriver, By.XPath(getXMLValue(InitialLoginXML, "CorporateLogoutButton")),5000);
                HelpObj.ButtonClickSimple(WebDriver, (By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporateLoginErrorButton"))));
            }
            catch (Exception)
            {
                HelpObj.LogError("Error: " + HelpObj.GetText(WebDriver, (By.XPath(getXMLValue(InitialLoginXML, "CorporateLoginErrorButton")))));
            }
        }

        public void CommonFlow(TestCaseInitialLogin TestCase)
        {
            Thread.Sleep(2000);
            HelpObj.PageLoad(WebDriver);
            WebDriver.Navigate().GoToUrl(dataSet["URL"]);
            WebDriver.Manage().Window.Maximize();
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporateIdentity")), TestCase.CorporateID, TestMaster);
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporateUsername")), TestCase.Username, TestMaster);
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporatePassword")), TestCase.Password, TestMaster);
            HelpObj.ButtonClickSimple(WebDriver, (By.XPath(HelpObj.getXMLValue(InitialLoginXML, "CorporateLoginButton"))));
        }

        public void ExtractTestCases()
        {
            List<DataCollection> dataColExecutionSet = ExcelLib.PopulateInCollection(pathToExcelFiles + TestCases_ExcelFileName, ExecutionSet_ExcelSheetName);
            int index;
            int columnCount = dataColExecutionSet.Count / dataColExecutionSet[0].totalRowCount;
            // traversing rows
            for (int j = 0; j < dataColExecutionSet[0].totalRowCount; j++)
            {

                Dictionary<string, string> testcase = new Dictionary<string, string>();
                // dataColExecutionSet.Count contains the count of cells i.e. rowCount * columnCount
                for (int k = 0; k < columnCount; k++)
                {
                    // fill data in testcase till row changes
                    index = (columnCount * j) + k;
                    testcase.Add(dataColExecutionSet[index].colName, dataColExecutionSet[index].colValue);
                }
                setTestCase = new TestCaseInitialLogin(testcase["TestCaseID"], testcase["TestCaseDescription"], testcase["Executable"], testcase["TestCaseType"], testcase["ExpectedResult"], testcase["CorporateID"], testcase["Username"], testcase["Password"]);
                TestCasesExecutionList.Add(setTestCase);
            }
        }

        public string getXMLValue(Dictionary<string, string> XMLFile, string key)
        {
            return XMLFile[key];
        }
        private void setValue(string field, string value, ExtentTest test)
        {
            if (string.IsNullOrWhiteSpace(value) == true)
                WebDriver.FindElement(By.XPath(InitialLoginXML[field])).Clear();
            else
            {
                WebDriver.FindElement(By.XPath(InitialLoginXML[field])).Clear();
                HelpObj.SendKeys(WebDriver, By.XPath(InitialLoginXML[field]), value, test);
                HelpObj.PageLoad(WebDriver);
                Thread.Sleep(1000);
            }
        }
    }
}
