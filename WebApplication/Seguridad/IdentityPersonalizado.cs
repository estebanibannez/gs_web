using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using WebApplicationMod;

namespace WebApplication.Seguridad
{
    public class IdentityPersonalizado : IIdentity
    {
        public string Name
        {
            get
            {
                return user.Sigla;
            }
        }

        public string AuthenticationType
        {
            get
            {
                return Identity.AuthenticationType;
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return Identity.IsAuthenticated;
            }
        }

        public static explicit operator IdentityPersonalizado(ClaimsIdentity v)
        {
            throw new NotImplementedException();
        }

        public IIdentity Identity { get; set; }

        public Usu user { get; set; }
        
        public List<string> roles { get; set; }

        public List<Clientes> clientsUser { get; set; }

        public IdentityPersonalizado(IIdentity identity)
        {
            this.Identity = identity;
            var us = Membership.GetUser(identity.Name) as UsuarioMembership;
            user = us.user;
            clientsUser = us.clientsUser;
            roles = us.roles;
        }
    }
}