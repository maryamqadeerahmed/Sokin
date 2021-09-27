using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Model.CorporatePortal
{
    class TestCaseChangePassword
    {
        public string TestCaseID { get; set; }
        public string TestCaseDescription { get; set; }
        public string Executable { get; set; }
        public string TestCaseType { get; set; }
        public string ExpectedResult { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public TestCaseChangePassword(string TestCaseID, string TestCaseDescription, string Executable, string TestCaseType, string ExpectedResult, string OldPassword, string NewPassword, string ConfirmPassword)
        {
            this.TestCaseID = TestCaseID;
            this.TestCaseDescription = TestCaseDescription;
            this.Executable = Executable;
            this.TestCaseType = TestCaseType;
            this.ExpectedResult = ExpectedResult;
            this.OldPassword = OldPassword;
            this.NewPassword = NewPassword;
            this.ConfirmPassword = ConfirmPassword;
        }
    }
}
