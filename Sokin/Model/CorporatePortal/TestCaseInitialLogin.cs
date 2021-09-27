using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Model.CorporatePortal
{
    class TestCaseInitialLogin
    {
        public string TestCaseID { get; set; }
        public string TestCaseDescription { get; set; }
        public string Executable { get; set; }
        public string TestCaseType { get; set; }
        public string ExpectedResult { get; set; }
        public string CorporateID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public TestCaseInitialLogin(string TestCaseID, string TestCaseDescription, string Executable, string TestCaseType, string ExpectedResult, string CorporateID, string Username, string Password)
        {
            this.TestCaseID = TestCaseID;
            this.TestCaseDescription = TestCaseDescription;
            this.Executable = Executable;
            this.TestCaseType = TestCaseType;
            this.ExpectedResult = ExpectedResult;
            this.CorporateID = CorporateID;
            this.Username = Username;
            this.Password = Password;
        }
    }
}
