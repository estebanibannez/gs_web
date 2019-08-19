using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplicationMod;

namespace WebApplication.Seguridad
{
    public class SessionUser
    {
        public Usu user { get; set; }
        public List<string> roles { get; set; }
        public DateTime SessionExpires { get; set; }
        public List<Clientes> clientUser { get; set; }
    }
}