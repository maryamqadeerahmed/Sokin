using AventStack.ExtentReports;
using OpenQA.Selenium;
using Sokin.Helper;
using Sokin.Helper.Database;
using Sokin.Helper.Excel;
using Sokin.Model.CorporatePortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sokin.FeatureFiles.CorporatePortal.ChangePassword
{
    class ChangePassword
    {
        private string pathToExcelFiles = "FeatureFiles/CorporatePortal/ChangePassword/ExcelFiles/";
        private string TestCases_ExcelFileName = "ChangePassword.xlsx";
        private string ExecutionSet_ExcelSheetName = "TestCases";
        private TestCaseChangePassword setTestCase;
        private IWebDriver WebDriver;
        private Dictionary<string, string> dataSet;
        private ExtentTest TestMaster;
        private DataConnection IrisDataConnection;
        private HelperClass HelpObj;
        private Dictionary<string, string> ChangePasswordXML;
        private List<TestCaseChangePassword> TestCasesExecutionList = new List<TestCaseChangePassword>();

        public ChangePassword(Dictionary<string, string> dataSet, DataConnection IrisDataConnection, HelperClass HelpObj, Dictionary<string, string> ChangePasswordXML)
        {
            this.dataSet = dataSet;
            this.IrisDataConnection = IrisDataConnection;
            this.HelpObj = HelpObj;
            this.ChangePasswordXML = ChangePasswordXML;
        }

        public string FnChangePassword(IWebDriver WebDriver, Dictionary<string, string> dataSet)
        {
            this.WebDriver = WebDriver;
            TestMaster = HelpObj.getReportObj().CreateTest("Corporate Portal Login Test Cases Positive");
            string returnVal = "DEF";
            Run();
            //try
            //{
            //    Run();
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
            ExtractTestCases();
            #region Executable Entries Extraction
            List<TestCaseChangePassword> TestCases = (from entry in TestCasesExecutionList
                                                    where entry.Executable.ToUpper().Equals("Y")
                                                    select entry).ToList();
            CommonFlowOnce();
            #endregion Executable Entries Extraction
            foreach (TestCaseChangePassword TestCase in TestCases)
            {
                CommonFlow(TestCase);
                if (TestCase.TestCaseType.ToLower() == "negative")
                {
                    TestCaseExecutionFlowNegative(TestCase);
                }
                else if (TestCase.TestCaseType.ToLower() == "positive")
                {
                    TestCaseExecutionFlowPositive(TestCase);
                }
                else
                {
                    Console.WriteLine("Change Password Test Case Type is neither Poistive nor Negative");
                    break;
                }
            }
        }

        public void TestCaseExecutionFlowPositive(TestCaseChangePassword TestCase)
        {
            try
            {
                Console.WriteLine("Password changed successfully");
            }
            catch (Exception ex)
            {
                HelpObj.LogError("Error: " + HelpObj.GetText(WebDriver, (By.XPath(getXMLValue(ChangePasswordXML, "ChangePasswordErrorPopup")))));
                HelpObj.ReportFail(WebDriver, "Failed To Execute Positive Test", TestMaster, TestCase.TestCaseID);
                HelpObj.ReportFail(WebDriver, ex.Message, TestMaster, TestCase.TestCaseID);
            }
        }

        public void TestCaseExecutionFlowNegative(TestCaseChangePassword TestCase)
        {
            try
            {
                bool result = HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ChangePasswordErrorPopupTextOK")), 1 * 60); 
                if (!result)
                {
                    result = HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "PasswordPolicyErrorIcons")), 1 * 60);
                    if (!result)
                    {
                        result = HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "NewAndConfirmPasswordUnmatched")), 1 * 60);
                        if (!result)
                        {
                            HelpObj.LogInfo("New Password and Confirm New Password match.");
                        }
                        else
                        {
                            HelpObj.LogInfo("New Password and Confirm New Password doesn't match.");
                        }
                    }
                    else
                    {
                        HelpObj.LogInfo("Password Policy doesn't satisfy" );
                    }
                }
                else
                {
                    HelpObj.ButtonClickSimple(WebDriver, (By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ChangePasswordErrorPopupTextOK"))));
                    HelpObj.LogError("Error: Old Password is invalid");
                }
            }
            catch (Exception ex)
            {
                HelpObj.LogError("Error: Change Password Test Case in Non - Negative");
                HelpObj.ReportFail(WebDriver, "Failed To Execute Negative Test", TestMaster, TestCase.TestCaseID);
                HelpObj.ReportFail(WebDriver, ex.Message, TestMaster, TestCase.TestCaseID);
            }
        }

        public void CommonFlow(TestCaseChangePassword TestCase)
        {
            HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "OldPasswordField")), 2 * 60);
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "OldPasswordField")), TestCase.OldPassword, TestMaster);
            HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "NewPasswordField")), 2 * 60);
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "NewPasswordField")), TestCase.NewPassword, TestMaster);
            HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ConfirmPasswordField")), 2 * 60);
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ConfirmPasswordField")), TestCase.ConfirmPassword, TestMaster);
            HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ChangePasswordSubmitButton")), 2 * 60);
            HelpObj.ButtonClickSimple(WebDriver, (By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ChangePasswordSubmitButton"))));
        }

        public void CommonFlowOnce()
        {

            Thread.Sleep(2000);
            HelpObj.PageLoad(WebDriver);
            WebDriver.Navigate().GoToUrl(dataSet["URL"]);
            WebDriver.Manage().Window.Maximize();
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "CorporateIdentity")), dataSet["CorporateId"], TestMaster);
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "CorporateUsername")), dataSet["Username"], TestMaster);
            HelpObj.SendKeys(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "CorporatePassword")), dataSet["Password"], TestMaster);
            HelpObj.ButtonClickSimple(WebDriver, (By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "CorporateLoginButton"))));
            HelpObj.LoadElement(WebDriver, By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ChangePasswordTab")), 2 * 60);
            Thread.Sleep(5000);
            HelpObj.ButtonClickSimple(WebDriver, (By.XPath(HelpObj.getXMLValue(ChangePasswordXML, "ChangePasswordTab"))));
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
                setTestCase = new TestCaseChangePassword(testcase["TestCaseID"], testcase["TestCaseDescription"], testcase["Executable"], testcase["TestCaseType"], testcase["ExpectedResult"], testcase["OldPassword"], testcase["NewPassword"], testcase["ConfirmPassword"]);
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
                WebDriver.FindElement(By.XPath(ChangePasswordXML[field])).Clear();
            else
            {
                WebDriver.FindElement(By.XPath(ChangePasswordXML[field])).Clear();
                HelpObj.SendKeys(WebDriver, By.XPath(ChangePasswordXML[field]), value, test);
                HelpObj.PageLoad(WebDriver);
                Thread.Sleep(1000);
            }
        }
    }
}
