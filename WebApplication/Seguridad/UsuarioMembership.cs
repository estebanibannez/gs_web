using AutenticacionPersonalizada.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebApplicationMod;

namespace WebApplication.Seguridad
{
    public class UsuarioMembership : MembershipUser
    {
        public int id {
            get { return user.ID; }
            set { }
        }

        public List<string> roles { get; set; }
        public Usu user { get; set; }
        public List<Clientes> clientsUser { get; set; }

        public UsuarioMembership(Usu us, List<string> rol, List<Clientes> clientUse)
        {
            user = us;
            roles = rol;
            clientsUser = clientUse;
        }
    }
}