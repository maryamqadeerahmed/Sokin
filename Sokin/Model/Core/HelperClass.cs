using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Chrome;
using System.Configuration;
using AventStack.ExtentReports;
using System.Xml.Linq;
using Sokin.Helper.Report;

namespace Sokin.Helper
{
    public class HelperClass
    {
            #region Variables
            private IWebDriver driver;
            #endregion
            #region Constructor
            public HelperClass(NLog.Logger logger, ReportMethod reportObj)
            {
                this.logger = logger;
                this.reportObj = reportObj;
            }
            #endregion
            #region Logging
            private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            public void LogInfo(String info)
            {
                logger.Info(info);
            }
            public void LogError(String Error)
            {
                logger.Error(Error);
            }
            public NLog.Logger getLoggerObj()
            {
                return logger;
            }
            #endregion
            #region Report

            private ReportMethod reportObj;
            public ReportMethod getReportObj()
            {
                return reportObj;
            }

            public void ReportInfo(string info, ExtentTest test, string TestCaseID)
            {
                LogInfo(info);
                reportObj.testInfo(test, TestCaseID, info);
            }
            public void ReportPass(string info, ExtentTest test, string TestCaseID)
            {
                LogInfo(info);
                reportObj.testPass(test, TestCaseID, info);
            }
            public void ReportFail(string Error, ExtentTest test, string TestCaseID)
            {
                LogError(Error);
                reportObj.testFail(test, TestCaseID, Error);
            }
            public void ReportFail(IWebDriver driver, string Error, ExtentTest test, string TestCaseID)
            {
                LogError(Error);
                reportObj.testFail(driver, test, TestCaseID, Error);
            }

            public void ReportFlush()
            {
                reportObj.flushExtent();
            }
            #endregion
            #region Initialize
            public static string initializeVariable(string variableName)
            {
                try
                {
                    try
                    {
                        NLog.LogManager.GetCurrentClassLogger().Info("Initialize: " + variableName);
                        return Environment.GetEnvironmentVariable(variableName).ToString();

                    }
                    catch (Exception ex)
                    {
                        return ConfigurationManager.AppSettings[variableName].ToString();
                    }
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Info("Unable to initialize variable: " + variableName + Environment.NewLine);
                    throw new System.ArgumentException(ex.ToString());
                }

            }
            #endregion
            #region PageLoad
            public void PageLoad(IWebDriver driver)
            {

                IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60.00));
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }
            public void PageLoad(IWebDriver driver, int timeOut)
            {

                IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOut));
                wait.Until(driver1 => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }
            public void PageLoad(IWebDriver driver, By by)
            {

                PageLoad(driver);
                logger.Info("PageLoad; " + by);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementIsVisible(by));
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                wait.Until(ExpectedConditions.ElementToBeClickable(by));
                logger.Info("WaitTime: " + wait);

            }
            public void PageLoad(IWebDriver driver, string waitForString, int timeOut)
            {
                try
                {
                    IWait<IWebDriver> wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOut));
                    wait.Until(ExpectedConditions.ElementIsVisible(By.LinkText(waitForString)));
                }
                catch
                { logger.Info("Unable to find " + waitForString + " at pageload"); }


            }

            #endregion
            #region Finders

            public void scrollElementInToView(IWebDriver driver, By by)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", driver.FindElement(by));
            }
            public bool ElementExistance(IWebDriver driver, By by, int Timeout = 60)
            {
                try
                {
                    PageLoad(driver);
                    Thread.Sleep(1000);
                    FindElement(driver, by, Timeout);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }


            public IWebElement FindElement(IWebDriver driver, By by, int timeOut)
            {
                try
                {
                    logger.Info("FindElement by " + by);
                    if (timeOut > 0)
                    {
                        var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeOut));
                        return wait.Until(drv => {
                            try
                            {
                                return drv.FindElement(by);
                            }
                            catch (Exception ex)
                            {
                                return null;
                            }
                        });
                    }

                    return driver.FindElement(by);

                }
                catch
                {
                    throw new Exception();

                }

            }
            ////private IWebElement FindElement(IWebDriver driver, By by, int timeOut)
            ////{
            ////    try
            ////    {
            ////        if (timeOut > 0)
            ////        {
            ////            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOut));
            ////            return wait.Until(drv => drv.FindElement(by));

            ////        }

            ////    }
            ////    catch
            ////    {
            ////        throw new Exception();
            ////    }
            ////    return driver.FindElement(by);
            ////}
            public string XPathDynamicPosition(String GeneralPath, string text)
            {
                try
                {
                    logger.Info("FindElement by " + GeneralPath);
                    string CompletePath = GeneralPath.Replace("$$REPLACE_HERE$$", text);
                    return CompletePath;

                }
                catch
                {
                    throw new Exception();

                }

            }

            #endregion
            #region Browers

            public IWebDriver SetBrowser(String a)
            {
                switch (a)
                {
                    case "firefox":
                        driver = new FirefoxDriver();
                        logger.Info("Driver Set to FireFox");
                        break;
                    case "chrome":
                        // driver = new ChromeDriver();
                        driver = initializeChromeDriver();
                        logger.Info("Driver Set to Chrome");
                        break;
                    case "IE":
                        driver = new InternetExplorerDriver();
                        logger.Info("Driver Set to IE");
                        break;
                    default:
                        driver = new FirefoxDriver();
                        logger.Info("Driver Set to FireFox");
                        break;
                }

                return driver;

            }

            public IWebDriver initializeFirefoxDriver()
            {

                FirefoxBinary firefoxbinary = new FirefoxBinary();
                FirefoxProfile firefoxprofile = new FirefoxProfile();
                firefoxprofile.SetPreference("xpinstall.signatures.required", false);
                //IWebDriver driver = new FirefoxDriver(firefoxbinary, firefoxprofile, TimeSpan.FromMinutes(2));
                //((IJavaScriptExecutor)driver).ExecuteScript(@"window.resizeTo(screen.width-100,screen.height-100);");
                //driver.Manage().Window.Maximize();
                return driver;
            }
            public IWebDriver initializeChromeDriver()
            {
                ChromeOptions googlechromeOptions = new ChromeOptions();
                ChromeDriverService googlechromesettings = ChromeDriverService.CreateDefaultService();
                googlechromesettings.HideCommandPromptWindow = true;
                googlechromeOptions.SetLoggingPreference(LogType.Browser, LogLevel.All);
                IWebDriver driver = new ChromeDriver(googlechromesettings, googlechromeOptions, TimeSpan.FromMinutes(2));
                ((IJavaScriptExecutor)driver).ExecuteScript(@"window.resizeTo(screen.width-100,screen.height-100);");
                driver.Manage().Window.Maximize();
                logger.Info("Window Maximized");
                return driver;
            }
            public List<LogEntry> getBrowserLogs()
            {
                return driver.Manage().Logs.GetLog(LogType.Browser).ToList();
            }
            public void CurrentWindowHandle()
            {
                String current = driver.CurrentWindowHandle;
                driver.SwitchTo().Window(current);
            }
            public void ChangeHelpObj_Driver(IWebDriver drivernew)
            {
                logger.Info("Changing Driver property for HelpObj");

                driver = drivernew;
            }
            #endregion
            #region String
            public string GetText(IWebDriver driver, By by)
            {
                PageLoad(driver, by);
                FindElement(driver, by, 45);
                scrollElementInToView(driver, by);
                return driver.FindElement(by).Text;
            }
            public string UppercaseFirst(string s)
            {
                if (string.IsNullOrEmpty(s))
                {
                    return string.Empty;
                }
                return char.ToUpper(s[0]) + s.Substring(1);
            }
            public bool VerifyByText(IWebDriver driver, By by, string Text)
            {
                if (driver.FindElement(by).Text.Contains(Text))
                { return true; }
                else
                {
                    return false;
                }
            }
            #endregion
            #region Popup

            public void TerminatePopUp(IWebDriver driver)
            {
                try
                {

                    logger.Info("TerminatePopUp:");
                    Thread.Sleep(1000);
                    IAlert alert = driver.SwitchTo().Alert();
                    alert.Accept();
                    logger.Info("Alert Text:" + alert.Text);
                    logger.Info("Accept");
                    Thread.Sleep(1000);

                }
                catch (NoAlertPresentException ex)
                {

                    logger.Info(ex.ToString());
                    logger.Info("Error in Terminate POPUP TerminatePopUp");
                }
            }
            #endregion
            #region Loaders
            public bool LoadElement(IWebDriver driver, By by, int time)
            {
                try
                {
                    logger.Info("LoadElement; " + by);
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                    wait.Until(ExpectedConditions.ElementIsVisible(by));
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));
                    wait.Until(ExpectedConditions.ElementToBeClickable(by));
                    logger.Info("WaitTime: " + wait);
                    return true;

                }
                catch (Exception ex)
                {

                    logger.Info("Element not loaded!");
                    logger.Info(ex.ToString);
                    return false;
                }
            }


            #endregion
            #region SendKeys
            public IWebDriver SendKeysNumeric(IWebDriver driver, By by, string keys, ExtentTest Node)
            {
                PageLoad(driver);
                Actions Focus = new Actions(driver);


                IWebElement SendKeyToElement = FindElement(driver, by, 45);
                scrollElementInToView(driver, by);
                if (String.IsNullOrEmpty(keys))
                {
                    logger.Info(by + " keys are empty, hence sendkey is not called");
                    Thread.Sleep(1000);
                    return driver;
                }


                Focus.MoveToElement(SendKeyToElement).Build().Perform();
                Focus.MoveToElement(SendKeyToElement).Click().Build().Perform();
                try
                {
                    logger.Info("Going to Send Keys to :" + by + " keys:" + keys);
                    SendKeyToElement.SendKeys(keys);
                    logger.Info("Send Keys to :" + by + " keys:" + keys);

                }
                catch (Exception ex)
                {
                    logger.Info("EXCEPTION while Send Keys ");

                    if ((ex.GetType().Name != "ElementNotInteractableException"))
                    {
                        string infoString = "Provided text was not allowed to be entered by HTML: " + keys + " on  XPATH: " + by;
                        if (Node != null)
                            reportObj.testInfo(Node, "", infoString);
                        logger.Info(infoString);
                    }
                    else
                    {
                        reportObj.testFail(driver, Node, "", ex.Message);
                    }
                    logger.Info(ex.Message);

                }
                Thread.Sleep(2000);
                return driver;

            }
            public IWebDriver SendKeys(IWebDriver driver, By by, string keys, ExtentTest Node)
            {
                PageLoad(driver);
                Actions Focus = new Actions(driver);




                IWebElement SendKeyToElement = FindElement(driver, by, 45);
                scrollElementInToView(driver, by);
                SendKeyToElement.Clear();
                logger.Info(by + " value has been cleared");

                if (String.IsNullOrEmpty(keys))
                {
                    logger.Info(by + " keys are empty, hence sendkey is not called");
                    Thread.Sleep(1000);
                    return driver;
                }
                Focus.MoveToElement(SendKeyToElement).Build().Perform();
                Focus.MoveToElement(SendKeyToElement).Click().Build().Perform();
                try
                {
                    logger.Info("Going to Send Keys to :" + by + " keys:" + keys);
                    SendKeyToElement.SendKeys(keys);
                    logger.Info("Send Keys to :" + by + " keys:" + keys);

                }
                catch (Exception ex)
                {
                    if ((ex.GetType().Name != "ElementNotInteractableException"))
                    {
                        string infoString = "Provided text was not allowed to be entered by HTML: " + keys + " on  XPATH: " + by;
                        if (Node != null)
                            reportObj.testInfo(Node, "", infoString);
                        logger.Info(infoString);
                    }
                    else
                    {
                        reportObj.testFail(driver, Node, "", ex.Message);
                    }
                    logger.Info(ex.Message);

                }
                Thread.Sleep(1000);
                return driver;

            }
            #endregion
            #region Clicks
            public IWebDriver ButtonClick(IWebDriver driver, By by)
            {
                PageLoad(driver);
                logger.Info("Going to JavaClick on :" + by);
                IWebElement ButtonClick = FindElement(driver, by, 90);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click()", ButtonClick);
                Thread.Sleep(1000);
                PageLoad(driver);
                logger.Info("Button Clicked On: " + by);

                return driver;
            }

            public IWebDriver ButtonClickSimple(IWebDriver driver, By by)
            {
                logger.Info("Going to Button Clicked On: " + by);
                PageLoad(driver);
                IWebElement ButtonClick = FindElement(driver, by, 30);
                ButtonClick.Click();
                logger.Info("Button Clicked On: " + by);
                Thread.Sleep(1000);
                PageLoad(driver);
                return driver;
            }
            public IWebDriver ButtonClickSimplePopUpWait(IWebDriver driver, By by)
            {
                logger.Info("Button Clicked On: " + by);
                PageLoad(driver);
                IWebElement ButtonClick = FindElement(driver, by, 30);
                ButtonClick.Click();
                Thread.Sleep(1000);
                return driver;
            }
            public IWebDriver ButtonClickSimpleWithoutWait(IWebDriver driver, By by)
            {
                logger.Info("Button Clicked On: " + by);
                Thread.Sleep(1000);
                IWebElement ButtonClick = driver.FindElement(by);
                ButtonClick.Click();
                Thread.Sleep(1000);
                PageLoad(driver);
                return driver;
            }
            public IWebDriver ButtonClick(IWebDriver driver, IWebElement element)
            {
                logger.Info("Going to Button Clicked On: " + element);
                element.Click();
                Thread.Sleep(1000);
                logger.Info("Button Clicked On: " + element);

                return driver;
            }

            public IWebDriver DoubleClick(IWebDriver driver, By by)
            {

                IWebElement Element = FindElement(driver, by, 60);
                Actions dClickAction = new Actions(driver);
                dClickAction.DoubleClick(Element).Build().Perform();
                Thread.Sleep(1000);
                PageLoad(driver);
                return driver;
            }
            #endregion
            #region DropDown
            public IWebDriver SelectElementByText(IWebDriver driver, By by, string Text)
            {
                logger.Info(by + " Text Entered: " + Text);
                logger.Info(by + " XPath Entered: " + by.ToString());

                Thread.Sleep(1000);
                PageLoad(driver, by);
                SelectElement SelectElementByText = new SelectElement(driver.FindElement(by));
                SelectElementByText.SelectByText(Text);
                Thread.Sleep(1000);
                PageLoad(driver);
                return driver;
            }

            public IWebDriver SelectElementByValue(IWebDriver driver, By by, string Text)
            {
                Thread.Sleep(2000);
                PageLoad(driver, by);
                SelectElement SelectElementByText = new SelectElement(driver.FindElement(by));
                SelectElementByText.SelectByText(Text);
                Thread.Sleep(1000);
                PageLoad(driver);
                return driver;
            }
            #endregion
            #region waiters
            public void WaitForFrame(IWebDriver driver, By by, int timeInSeconds = 90)
            {
                try
                {
                    LogInfo("Waiting For Frame for " + timeInSeconds + " seconds");
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeInSeconds));
                    wait.Until(ExpectedConditions.ElementIsVisible(by));
                }
                catch (Exception ex)
                {
                    LogError(ex.Message);
                }
            }

            #endregion
            #region Setup
            public void TeardownTest(IWebDriver WebDriver)
            {
                try
                {
                    reportObj.flushExtent();
                    WebDriver.Close();
                    WebDriver.Dispose();
                }
                catch (Exception)
                {
                }
            }


            public Dictionary<string, string> GetXMLKeyValueDictionary(string XML)
            {
                XDocument doc = XDocument.Load(XML);
                Dictionary<string, string> dict = doc.Descendants("Element")
                                          .ToDictionary(
                                                        x =>
                                                        {
                                                            Console.WriteLine(x.Attribute("keyword").Value); return x.Attribute("keyword").Value;
                                                        },
                                                        x =>
                                                        {
                                                            Console.WriteLine(x.Attribute("locator").Value); return x.Attribute("locator").Value;
                                                        }
                                                        );
                return dict;
            }
            #endregion
            #region DropDowns
            public IWebDriver DropDownKendo(IWebDriver driver, By bySpan, string byLiString, string text)
            {
                try
                {
                    if (string.IsNullOrEmpty(text))
                        return driver;
                    byLiString = byLiString.Replace("$$REPLACE_HERE$$", text);

                    By byLi = By.XPath(byLiString);

                    PageLoad(driver, bySpan);
                    FindElement(driver, bySpan, 45);
                    scrollElementInToView(driver, bySpan);
                    Thread.Sleep(2000);
                    ButtonClickSimple(driver, bySpan);
                    Thread.Sleep(1000);
                    PageLoad(driver, byLi);
                    ButtonClick(driver, byLi);
                    PageLoad(driver);
                    Thread.Sleep(1000);
                    return driver;
                }
                catch (Exception ex)
                {
                    logger.Info("Failed In Fetching value from DropDown");
                    logger.Info(ex.ToString);
                    return driver;

                }
            }
            public IWebDriver DropDownKendoDataTest(IWebDriver driver, By bySpan, string byLiString, string text)
            {
                if (string.IsNullOrEmpty(text))
                    return driver;
                byLiString = byLiString.Replace("$$REPLACE_HERE$$", text);

                By byLi = By.XPath(byLiString);

                PageLoad(driver, bySpan);
                FindElement(driver, bySpan, 45);
                scrollElementInToView(driver, bySpan);
                Thread.Sleep(2000);
                ButtonClickSimple(driver, bySpan);
                Thread.Sleep(1000);
                PageLoad(driver, byLi);
                PageLoad(driver);
                Thread.Sleep(1000);
                return driver;
            }

            #endregion

            #region XML
            public string getXMLValue(Dictionary<string, string> XML, string key)
            {
                return XML[key];
            }

            #endregion

            #region Frames
            public IWebDriver SwitchToFrame(IWebDriver driver, By by)
            {
                logger.Info("Switch To Frame By: " + by);
                PageLoad(driver);
                driver.SwitchTo().Frame(driver.FindElement(by));
                Thread.Sleep(1000);
                PageLoad(driver);
                return driver;
            }

            public IWebDriver SwitchToDefault(IWebDriver driver)
            {
                logger.Info("Switching To Default");
                PageLoad(driver);
                driver.SwitchTo().ParentFrame();
                driver.SwitchTo().DefaultContent();
                Thread.Sleep(1000);
                PageLoad(driver);
                return driver;
            }
            #endregion
        }
    }
