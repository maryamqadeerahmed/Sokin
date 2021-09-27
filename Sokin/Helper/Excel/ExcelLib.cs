using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Helper.Excel
{
    class ExcelLib
    {
        private static List<DataCollection> allData = new List<DataCollection>();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static DataTable ExcelToDataTable(string fileName, string sheetName)
        {
            using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            // reader.GetDouble(0);
                        }
                    } while (reader.NextResult());

                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });

                    DataTableCollection table = result.Tables;
                    DataTable resultTable = table[sheetName];
                    return resultTable;
                }
            }
        }

        //creates new collection every time and populate data in it
        public static List<DataCollection> PopulateInCollection(string fileName, string sheetName)
        {
            List<DataCollection> dataCol = new List<DataCollection>();
            dataCol.Clear();
            DataTable table = ExcelToDataTable(fileName, sheetName);

            for (int row = 1; row <= table.Rows.Count; row++)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {

                    DataCollection dtTable = new DataCollection()
                    {
                        totalRowCount = table.Rows.Count,
                        rowNumber = row,
                        colName = table.Columns[col].ColumnName,
                        colValue = table.Rows[row - 1][col].ToString()
                    };
                    dataCol.Add(dtTable);
                }
            }
            return dataCol;
        }
        //utilizes old collection every time and append data in it
        public static List<DataCollection> PopulateAllInCollection(string fileName, string sheetName)
        {
            DataTable table = ExcelToDataTable(fileName, sheetName);

            for (int row = 1; row <= table.Rows.Count; row++)
            {
                for (int col = 0; col < table.Columns.Count; col++)
                {

                    DataCollection dtTable = new DataCollection()
                    {
                        totalRowCount = table.Rows.Count,
                        rowNumber = row,
                        colName = table.Columns[col].ColumnName,
                        colValue = table.Rows[row - 1][col].ToString()
                    };
                    allData.Add(dtTable);
                }
            }
            return allData;
        }
        public static String ReadData(List<DataCollection> dataCol, int rowNumber, string columnName)
        {
            try
            {
                //Retriving Data using LINQ to reduce much of iterations
                string data = (from colData in dataCol
                               where colData.colName == columnName && colData.rowNumber == rowNumber
                               select colData.colValue).SingleOrDefault();

                if (data.ToString() == null || data.ToString().Length == 0)
                {
                    return null;
                }

                return data.ToString();
            }
            catch (Exception e)
            {
                logger.Info(e.Message);
                return null;
            }
        }

    }

    public class DataCollection
    {
        public int totalRowCount { get; set; }
        public int rowNumber { get; set; }
        public string colName { get; set; }
        public string colValue { get; set; }
    }
}
