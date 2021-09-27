using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sokin.Model.Core
{
    class Core
    {
        public string IP { get; set; }
        public string LogPath { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        //public Core()
        //{

        //}
        public Core(string IP, string Password, string User, string Path)
        {
            this.IP = IP;
            this.LogPath = Path;
            this.User = User;
            this.Password = Password;
        }
    }
}
