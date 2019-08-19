using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class ContrasenaClienteController : MasterController
    {
        // GET: Mantencion/ContrasenaCliente
        public ActionResult Index()
        {
            var usu = SesionLogin().ID;
            var datos = (from clie in _db.Clientes
                         join per in _db.Permisos on clie.ID equals per.Clie
                         join Usuarios in _db.Usu on per.Usu equals Usuarios.ID
                         where Usuarios.ID == usu
                         select clie
                         ).AsParallel();

            return View(datos);
        }
    }
}