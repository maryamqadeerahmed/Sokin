using Sokin.Helper;
using Sokin.Helper.Database;
using Sokin.Helper.Excel;
using Sokin.Model.Suite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.FeatureFiles.DigitalBankingBackOffice.CorporateOnboarding
{
    class CorporateOnboarding
    {
        private Dictionary<string, string> dataSet;
        private DataConnection IrisDataConnection;
        private HelperClass HelpObj;
        private Dictionary<string, string> CorporateOnboardingXML;
        public CorporateOnboarding(Dictionary<string, string> dataSet, DataConnection IrisDataConnection, HelperClass HelpObj, Dictionary<string, string>  CorporateOnboardingXML)
        {
            this.dataSet = dataSet;
            this.IrisDataConnection = IrisDataConnection;
            this.HelpObj = HelpObj;
            this.CorporateOnboardingXML = CorporateOnboardingXML;
        }
        public void AddCorporate()
        {
            Console.WriteLine("I am inside add corporate");
        }
    }
}
