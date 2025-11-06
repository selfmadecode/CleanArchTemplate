using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models
{
    public class SmtpConfigSettings
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Password { get; set; }

        public string UserName { get; set; }

        public string Mail { get; set; }

        public string DisplayName { get; set; }
    }
}
