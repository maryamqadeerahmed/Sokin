using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Model.Common
{
    class ExecutionSetEntry
    {
        public string ExecutionType { get; set; }
        public string Executable { get; set; }
        public string NATURE { get; set; }

        public ExecutionSetEntry(string ExecutionType, string Executable, string NATURE)
        {
            this.ExecutionType = ExecutionType;
            this.Executable = Executable;
            this.NATURE = NATURE;
        }
    }
}
