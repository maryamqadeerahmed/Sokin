using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Helper.Report
{
    public class ReportMethod
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ExtentReports extent;
        private string screenshot_path;
        public ReportMethod(NLog.Logger logger, string screenshot_path)
        {
            this.logger = logger;
            this.screenshot_path = screenshot_path;
        }
        public ExtentReports getExtent()
        {
            return extent;
        }
        public void flushExtent()
        {
            extent.Flush();
        }
        public ExtentTest CreateTest(string desc)
        {
            return extent.CreateTest(desc);
        }
        public void InitializeExtent(string target, string reportPath, string build_username, string build_ID, string build_number, string environment)
        {
            var reporter = new ExtentHtmlReporter(reportPath);
            extent = new ExtentReports();
            extent.AddSystemInfo("Build username", build_username);
            extent.AddSystemInfo("Build id", build_ID);
            extent.AddSystemInfo("Build number", build_number);
            extent.AddSystemInfo("Report Folder", target);
            extent.AddSystemInfo("Environment", environment);
            extent.AttachReporter(reporter);
        }
        public void testFail(IWebDriver driver, ExtentTest test, string test_case_id, string test_fail_details)
        {
            try
            {
                if (string.IsNullOrEmpty(test_case_id))
                    test_case_id = "ex";
                logger.Info("Test Failed: Test Case ID: " + test_case_id + " <br>" + test_fail_details);
                test.Log(Status.Fail, "Test Case ID: " + test_case_id + " <br>" + test_fail_details);
                string screenShotPath = Capture(driver, "screenshot_" + DateTime.Now.ToString("yyyymmddhhmiss") + test_case_id, screenshot_path);
                logger.Info("Test Failed: Screenshot Path " + screenShotPath);
                test.Info("details", MediaEntityBuilder.CreateScreenCaptureFromPath(screenShotPath).Build());
            }
            catch { }
            extent.Flush();
        }
        public void testFail(ExtentTest test, string test_case_id, string test_fail_details)
        {
            logger.Error("Test Failed: Test Case: " + test_case_id + " <br>" + test_fail_details);
            test.Log(Status.Fail, "Test Case: " + test_case_id + " <br>" + test_fail_details);
            extent.Flush();
        }
        public void testInfo(ExtentTest test, string test_case_id, string test_fail_details)
        {
            logger.Info("Test Info: Test Case: " + test_case_id + " <br>" + test_fail_details);
            test.Log(Status.Info, "Test Case: " + test_case_id + " <br>" + test_fail_details);
            extent.Flush();
        }
        public void testPass(ExtentTest test, string test_case_id, string test_pass_details)
        {
            logger.Info("Test Passed: Test Case: " + test_case_id + " <br>" + test_pass_details);
            test.Log(Status.Pass, "Test Case: " + test_case_id + " <br>" + test_pass_details);
            extent.Flush();
        }
        public static string Capture(IWebDriver driver, string screenShotName, string screenShotPath)
        {
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            Console.WriteLine("===========CAPTURING SCREEN SHOT========");
            logger.Info("===========CAPTURING SCREEN SHOT========");
            Console.WriteLine("SCREEN SHOT NAME: " + screenShotName);
            logger.Info("SCREEN SHOT NAME: " + screenShotName);
            logger.Info("SCREEN SHOT NAME: " + screenShotPath);
            ITakesScreenshot ts = (ITakesScreenshot)driver;
            Screenshot screenshot = ts.GetScreenshot();
            string pth = System.Reflection.Assembly.GetCallingAssembly().CodeBase;
            screenshot.SaveAsFile(screenShotPath + "\\" + screenShotName + ".png", ScreenshotImageFormat.Png);
            return screenShotName + ".png";
        }
    }
}
