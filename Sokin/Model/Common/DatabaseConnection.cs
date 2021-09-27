using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Model.Common
{
    class DatabaseConnection
    {
        public string DbName { get; set; }
        public string DbSid { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public string DbHost { get; set; }
        public string DBPort { get; set; }
        public string UseSID { get; set; }
        public DatabaseConnection()
        {

        }
        public DatabaseConnection(string DbName, string DbUser, string DbPassword, string DbHost, string DBPort, string DbSid, string UseSID)
        {
            this.DbName = DbName;
            this.DbUser = DbUser;
            this.DbPassword = DbPassword;
            this.DbHost = DbHost;
            this.DBPort = DBPort;
            this.DbSid = DbSid;
            this.UseSID = UseSID;
        }
    }
}
