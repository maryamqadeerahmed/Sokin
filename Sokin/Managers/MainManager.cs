using Sokin.Helper;
using Sokin.Helper.Database;
using Sokin.Helper.Excel;
using Sokin.Helper.Log;
using Sokin.Helper.Report;
using Sokin.Model.Common;
using Sokin.Model.Core;
using Sokin.Model.Suite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Managers
{
    class MainManager
    {
        #region variables
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private string environment = string.Empty;
        private string build_number = string.Empty;
        private string build_ID = string.Empty;
        private string build_username = string.Empty;
        private string path = string.Empty;
        private string target = string.Empty;
        private string reportPath = string.Empty;
        private string screenshot_path = string.Empty;
        private string log_path = string.Empty;
        private string log_name = string.Empty;
        private string AppConfiguration_ExcelFileName = string.Empty;
        private HelperClass helpObj;

        #region Excel Sheets
        private string pathToExcelFiles = "Configurations/ExcelFiles/";
        private Dictionary<string, string> dataSetDGB = new Dictionary<string, string>(); 
        private string DGBSuiteConfig_ExcelSheetName = "DGBSuiteConfig";
        private string DGBCoreConfig_ExcelSheetName = "DGBCoreConfig";
        private string DGBDBConfig_ExcelSheetName = "DGBDBConfig";
        private Dictionary<string, string> dataSetMM = new Dictionary<string, string>();
        private string MMSuiteConfig_ExcelSheetName = "MMSuiteConfig";
        private string MMCoreConfig_ExcelSheetName = "MMCoreConfig";
        private string MMDBConfig_ExcelSheetName = "MMDBConfig";
        private Dictionary<string, string> dataSetCP = new Dictionary<string, string>();
        private string CorporatePortalConfig_ExcelSheetName = "CorporatePortalConfig";
        private string Environment_ExcelSheetName = "Environment";
        private string ExecutionSet_ExcelSheetName = "ExecutionSet";
        private string TransactionConfig_ExcelSheetName = "TransactionConfig";
        List<ExecutionSetEntry> dataColExecutionSet = new List<ExecutionSetEntry>();
        #endregion
        
        private NLogger nl;

        private DatabaseConnection DBConnection = new DatabaseConnection();
        private DataConnection IrisDataConnection;
        private ReportMethod ReportMethods;
        private string CoreLogPath = string.Empty;
        private string pathToElementFactoryDGB= "Configurations/ElementFactory/DigitalBanking/";
        private string pathToElementFactoryMM= "Configurations/ElementFactory/MobileMoney/";
        private string pathToElementFactoryCP= "Configurations/ElementFactory/CorporatePortal/";
        private Dictionary<string, string> InitialLoginXML;
        private Dictionary<string, string> ChangePasswordXML;
        private Dictionary<string, string> CorporateOnboardingXML;
        
        #endregion

        public void Run()
        {
            #region Initialization
            InitializeLogger();
            LoadParamFromAppConfig();
            LoadParamFromExcel();
            InitializeReportingFrameWork();
            InitializeHelper();
            initializeElementFactory();
            #endregion
            ExecutionSetManager ExecutionSetManager = new ExecutionSetManager(dataColExecutionSet, dataSetCP, IrisDataConnection, helpObj, InitialLoginXML, ChangePasswordXML, CorporateOnboardingXML);
            ExecutionSetManager.Run();
        }

        private void InitializeLogger()
        {
            path = Directory.GetCurrentDirectory();
            target = path + "\\Automation_Report\\" + DateTime.Now.ToString("ddmmyyyyhhmmss");
            log_path = target + "\\Logs";
            nl = new NLogger();
            nl.InitializeLogger(log_path, log_name);
            logger = NLog.LogManager.GetCurrentClassLogger();
        }
        private void InitializeHelper()
        {
            helpObj = new HelperClass(logger, ReportMethods);
        }
        private void InitializeReportingFrameWork()
        {

            logger.Info("InitializeReportFrameWork : " + DateTime.Now.ToString("HH:mm:ss:ffffff"));
            reportPath = target + "\\" + "Report.html";
            // screenshot_path = target + "\\ScreenShots";
            screenshot_path = target + "";
            string pathToLoadConfig = Directory.GetCurrentDirectory();
            pathToLoadConfig = pathToLoadConfig.Replace("\\bin\\Debug", "\\");
            ReportMethods = new ReportMethod(logger, screenshot_path);
            ReportMethods.InitializeExtent(target, reportPath, build_username, build_ID, build_number, environment);
            logger.Info("InitializeReportFrameWork - Completed : " + DateTime.Now.ToString("HH:mm:ss:ffffff"));
        }
        private void LoadParamFromAppConfig()
        {
            AppConfiguration_ExcelFileName = HelperClass.initializeVariable("ExcelFileName");
        }
        private void LoadParamFromExcel()
        {
            #region ENVIRONMENT General 
            List<DataCollection> dataCol = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, Environment_ExcelSheetName);
            environment = ExcelLib.ReadData(dataCol, 1, "Environment");
            build_username = ExcelLib.ReadData(dataCol, 1, "BuildUser");
            build_ID = ExcelLib.ReadData(dataCol, 1, "BuildID");
            build_number = ExcelLib.ReadData(dataCol, 1, "BuildNumber");
            log_name = ExcelLib.ReadData(dataCol, 1, "LogName");
            #endregion

            #region DIGITAL BANKING
            List<DataCollection> dataColDGB = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, DGBSuiteConfig_ExcelSheetName);
            loadMultipleColumnParamsInDictionary(dataColDGB, dataSetDGB);
            dataColDGB = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, DGBDBConfig_ExcelSheetName);
            loadMultipleColumnParamsInDictionary(dataColDGB, dataSetDGB);
            IrisDataConnection = new DataConnection(dataSetDGB["DbHost"], dataSetDGB["UseSid"], dataSetDGB["DbUser"], dataSetDGB["DbPassword"], dataSetDGB["UseSid"], dataSetDGB["DbPort"]);
            dataColDGB = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, DGBCoreConfig_ExcelSheetName);
            loadMultipleColumnParamsInDictionary(dataColDGB, dataSetDGB);
            #endregion DIGITAL BANKING

            #region MOBILE MONEY
            List<DataCollection> dataColMM = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, MMSuiteConfig_ExcelSheetName);
            loadMultipleColumnParamsInDictionary(dataColMM, dataSetMM);
            dataColMM = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, MMDBConfig_ExcelSheetName);
            loadMultipleColumnParamsInDictionary(dataColMM, dataSetMM);
            IrisDataConnection = new DataConnection(dataSetMM["DbHost"], dataSetMM["UseSid"], dataSetMM["DbUser"], dataSetMM["DbPassword"], dataSetMM["UseSid"], dataSetMM["DbPort"]);
            dataColMM = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, DGBCoreConfig_ExcelSheetName);
            loadMultipleColumnParamsInDictionary(dataColMM, dataSetMM);
            #endregion MOBILE MONEY

            #region CORPORATE PORTAL
            List<DataCollection> dataColCP = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, CorporatePortalConfig_ExcelSheetName);
            loadMultipleColumnParamsInDictionary(dataColCP, dataSetCP);
            #endregion CORPORATE PORTAL

            #region Execution Set
            List<DataCollection> dataColExecutionSetDC = ExcelLib.PopulateInCollection(pathToExcelFiles + AppConfiguration_ExcelFileName, ExecutionSet_ExcelSheetName);
            for (int j = 1; j <= dataColExecutionSetDC[0].totalRowCount; j++)
            {
                string ExecutionType = ExcelLib.ReadData(dataColExecutionSetDC, j, "ExecutionType");
                string Executable = ExcelLib.ReadData(dataColExecutionSetDC, j, "Executable");
                string NATURE = ExcelLib.ReadData(dataColExecutionSetDC, j, "NATURE");
                if (!String.IsNullOrEmpty(NATURE))
                {
                    dataColExecutionSet.Add(new ExecutionSetEntry(ExecutionType, Executable, NATURE));
                }
            }
            #endregion
        }
        public void initializeElementFactory()
        {
            InitialLoginXML = helpObj.GetXMLKeyValueDictionary(pathToElementFactoryCP + "InitialLogin.xml");
            ChangePasswordXML = helpObj.GetXMLKeyValueDictionary(pathToElementFactoryCP + "ChangePassword.xml");
            CorporateOnboardingXML = helpObj.GetXMLKeyValueDictionary(pathToElementFactoryDGB + "CorporateOnboarding.xml");
        }

        public void loadMultipleColumnParamsInDictionary(List<DataCollection> dataColSet, Dictionary<string, string> dataSet)
        {
            int i = 0;
            while (i < dataColSet.Count)
            {
                string value = ExcelLib.ReadData(dataColSet, dataColSet[i].rowNumber, dataColSet[i].colName);
                dataSet.Add(dataColSet[i].colName, value);
                i++;
            }
        }
    }
}
