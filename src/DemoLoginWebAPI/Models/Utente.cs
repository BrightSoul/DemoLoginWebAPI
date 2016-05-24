using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoLoginWebAPI.Models
{
    public class Utente
    {
        public int IdUtente { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string Ruolo { get; set; }
    }
}
