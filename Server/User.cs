using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class User
    {
        public string login { get; set; }
        public string password { get; set; }
        public string src { get; set; }
        public string temp_src {  get; set; }
        public User(string login, string password, string src, string temp_src)
        {
            this.login = login;
            this.password = password;
            this.src = src;

            this.temp_src = temp_src;
        }
    }
}
