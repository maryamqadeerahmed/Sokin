using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Model.Suite
{
    class PortalUser
    {
        public string CorporateID { get; set; }
        public string PortalUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string DualAuth { get; set; }
        public PortalUser(string PortalUrl, string CorporateID, string UserName, string Password, string Role, string DualAuth)
        {
            this.CorporateID = CorporateID;
            this.PortalUrl = PortalUrl;
            this.UserName = UserName;
            this.Password = Password;
            this.Role = Role;
            this.DualAuth = DualAuth;
        }
    }
}
