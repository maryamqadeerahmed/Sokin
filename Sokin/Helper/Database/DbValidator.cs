using AventStack.ExtentReports;
using Sokin.Helper.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Helper.Database
{
    class DbValidator
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ReportMethod ReportObj;
        public DbValidator(ReportMethod ReportObj, NLog.Logger logger)
        {
            this.ReportObj = ReportObj;
            this.logger = logger;
        }
        public void validateQueryFromExcel(DataConnection DC, string query, string column, string columnValue, string description, string txnRefNo, ExtentTest node, string ISSUER_RESPONSE_CODE = "")
        {
            logger.Info("validating Query ");
            if (query == null || query.Equals(""))
            {
                logger.Info(description + " || Skipping " + column + " Query" + query + ", as query is empty");
                return;
            }
            if (query.Contains("{TXNREFTOREPLACE}"))
            {
                query = query.Replace("{TXNREFTOREPLACE}", txnRefNo);
            }
            if (columnValue.Contains("{ISSUER_RESPONSE_CODE_TOREPLACE}"))
            {
                columnValue = columnValue.Replace("{ISSUER_RESPONSE_CODE_TOREPLACE}", ISSUER_RESPONSE_CODE);
            }

            try
            {
                logger.Info(description + " || Validating " + column + " Query" + query);
                string[] dataList = DC.ConnectToData(query).ToArray();
                string[] ValueList = columnValue.Split(',');
                string[] columnList = column.Split(',');

                if ((dataList.Count() < 1) && (!(ValueList.Contains("null"))))
                {
                    throw new Exception("Query Returned null");
                }


                logger.Info("Query returned ");
                ReportObj.testInfo(node, "query" + ",", query);

                if (!(dataList.Count() < 1))
                {
                    foreach (string item in dataList)
                    {
                        logger.Info(item);
                    }
                    for (int i = 0; i < ValueList.Length; i++)
                    {
                        if (dataList[i].Equals(ValueList[i]) || (String.IsNullOrEmpty(dataList[i]) && (String.IsNullOrEmpty(ValueList[i]) || ValueList[i].ToLower().Equals("null"))))
                        {
                            logger.Info(dataList[i] + " is equal to : " + ValueList[i]);
                            ReportObj.testPass(node, "" + " " + description + " >>  " + columnList[i] + " validated successful from db actual, expected:", dataList[i] + ", " + ValueList[i]);
                        }
                        else if (dataList[i].Equals(ValueList[i]) == false)
                        {
                            ReportObj.testFail(node, "" + " " + description + " >>  " + columnList[i] + " did not validated successfully from db actual, expected: ", dataList[i] + ", " + ValueList[i]);
                        }
                    }

                }
                else
                {
                    ReportObj.testPass(node, "" + " " + description + " >>  " + columnList[0] + " validated successful from db actual, expected:", " null " + ValueList[0]);

                }

            }
            catch (Exception ex)
            {
                ReportObj.testFail(node, "" + column + " did not validated successfully from db", column + ", " + ex.Message);

            }
        }

        public void executeDeleteQueryFromExcel(DataConnection DC, string query, string description, string UniqueValue, ExtentTest node)
        {
            query = ReplaceUniqueToQuery(query, UniqueValue);

            try
            {
                DC.ExecuteQuery(query);
                ReportObj.testInfo(node, "" + query + " executed successfully ", "");

            }
            catch (Exception ex)
            {
                ReportObj.testFail(node, "" + query + " did not execute successfully from db", ", " + ex.Message);
            }
        }

        public string[] ExecuteSelectQuery(DataConnection DC, string query)
        {
            logger.Info("Executing Query ");
            if (query == null || query.Equals(""))
            {
                logger.Info(" Skipping  Query" + query + ", as query is empty");
                return null;
            }

            try
            {
                logger.Info(" Validating Query" + query);

                string[] dataList = DC.ConnectToData(query).ToArray();

                if ((dataList.Count() < 1))
                {
                    throw new Exception("Query Returned null");
                }
                logger.Info("Query returned " + dataList);
                return dataList;
            }
            catch (Exception ex)
            {
                throw new Exception("Query select execution failed");
            }
        }
        public string ReplaceUniqueToQuery(string query, string UniqueValue)
        {
            if (query == null || query.Equals(""))
            {
                logger.Info(" || Skipping " + UniqueValue + " Query" + query + ", as query is empty");
                return query;
            }
            if (query.Contains("{VALUETOREPLACE}"))
            {
                query = query.Replace("{VALUETOREPLACE}", UniqueValue);
                return query;
            }
            return query;
        }
    }
}
