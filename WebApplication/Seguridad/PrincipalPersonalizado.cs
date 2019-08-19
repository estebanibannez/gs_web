using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace WebApplication.Seguridad
{
    public class PrincipalPersonalizado : IPrincipal
    {
        public IIdentity Identity { get; private set; }

        public bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }

        public IdentityPersonalizado MiIdentidadPersonalizada
        {
            get { return (IdentityPersonalizado)Identity; }
            set { Identity = value; }
        }

        public PrincipalPersonalizado(IdentityPersonalizado identity)
        {
            Identity = identity;
        }
    }
}