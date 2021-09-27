using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Helper.Database
{
    class DataConnection
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string dataSource;
        public bool UseSID = true;
        private string dbUserId;
        private string dbPassword;
        public string DbHost;
        public string DBPort;
        string connectionString;
        OracleConnection conn;
        OracleCommand cmd;


        public DataConnection(string DbHost, string dataSource, string dbUserId, string dbPassword, string UseSID, string port)
        {
            if (bool.Parse(UseSID))
            {
                this.UseSID = true;
            }
            else
            {
                this.UseSID = false;
            }
            this.DBPort = port;
            this.dataSource = dataSource;
            this.dbUserId = dbUserId;
            this.dbPassword = dbPassword;
            this.connectionString = GenerateConnectionString();
            this.DbHost = DbHost;
            this.connectionString = GenerateConnectionString(DbHost);
        }

        public void ExecuteQuery(string query)
        {
            logger.Info("Executing Query:" + Environment.NewLine + query);

            conn = new OracleConnection(connectionString);
            runQuery(query);
        }


        public List<string> ConnectToData(string query)
        {
            logger.Info("Executing Query:" + Environment.NewLine + query + " at Database SID:" + dataSource + " userid:" + dbUserId + " password:" + dbPassword);
            DataSet ds = new DataSet();

            conn = new OracleConnection(connectionString);
            cmd = conn.CreateCommand();

            cmd.CommandText = query;

            OracleDataAdapter da = new OracleDataAdapter(cmd);
            da.Fill(ds);

            List<string> result = new List<string>();
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                for (int j = 0; j < ds.Tables[0].Columns.Count; j++)
                {
                    result.Add(ds.Tables[0].Rows[i][j].ToString());
                }
            }
            conn.Close();
            foreach (string s in result)
            {
                Console.Write(s + " ");
            }
            logger.Info(Environment.NewLine);
            return result;

        }

        private void BreakConnectionString(string ConnectionString)
        {
            string[] dbstring = ConnectionString.Split(new char[] { '\"', '=', ';' }, StringSplitOptions.RemoveEmptyEntries);
            this.dbUserId = dbstring[3];
            this.dbPassword = dbstring[5];

        }
        private string GenerateConnectionString()
        {
            return "Data Source=" + dataSource + "; User Id=" + dbUserId + "; Password =" + dbPassword + ";";
        }

        private string GenerateConnectionString(string dbip)
        {


            if (UseSID)
            {
                return "Data Source = (DESCRIPTION = " + "(ADDRESS = (PROTOCOL = TCP)(HOST = " + dbip + ")(PORT = " + DBPort + "))" +
                                 "(CONNECT_DATA = " +
                                 "(SERVER = DEDICATED)" +
                                 "(SID = " + dataSource + " )" +
                                 ")" +
                                 ");User Id = " + dbUserId + "; Password = " + dbPassword + ";";

            }
            else
            {
                return "Data Source = (DESCRIPTION = " + "(ADDRESS = (PROTOCOL = TCP)(HOST = " + dbip + ")(PORT = " + DBPort + "))" +
                               "(CONNECT_DATA = " +
                               "(SERVICE_NAME = " + dataSource + " )" +
                               ")" +
                               ");User Id = " + dbUserId + "; Password = " + dbPassword + ";";
            }

        }

        private void runQuery(string query)
        {

            try
            {
                conn.Open();
                cmd = conn.CreateCommand();
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                conn.Close();

            }
            catch (Exception ex)
            {
                logger.Error(ex + ToString() + "Unable to Insert Alert in DB");

            }
        }

        public void insertAlert(string DATETIME, string INSTITUTION, string MODULE, string UI_ERROR, string MESSAGE_BODY, string RECIPIENTS)
        {

            DATETIME = DATETIME.Replace("'", "''");
            INSTITUTION = INSTITUTION.Replace("'", "''");
            MODULE = MODULE.Replace("'", "''");
            UI_ERROR = UI_ERROR.Replace("'", "''");

            MESSAGE_BODY = MESSAGE_BODY.Replace("'", "''");
            RECIPIENTS = RECIPIENTS.Replace("'", "''");

            string Query = @"INSERT INTO TBL_NI_AUTOMATION_ALERT (ALERT_ID, DATETIME, INSTITUTION, MODULE, UI_ERROR, MESSAGE_BODY, RECIPIENTS, IS_FETCH)
VALUES
(AUTOMATION_ALERT_ID.NEXTVAL, '" + DATETIME + "','" + INSTITUTION + "','" + MODULE + "','" + UI_ERROR + "','" + MESSAGE_BODY + "','" + RECIPIENTS + "', '0')";

            ExecuteQuery(Query);
        }
    }
}
