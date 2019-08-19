using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class ContactoController : MasterController
    {
        // GET: Contacto/Contacto
        public ActionResult Index()
        {
            var id_usu = SesionLogin().ID;

            var aux = (from cli in _db.Clientes
                       join per in _db.Permisos on cli.ID equals per.Clie
                       where per.Usu == id_usu
                       select cli).ToList();
            return View(aux);
        }

        public ActionResult Buscar(string id)
        {
            dynamic modelo = new ExpandoObject();
            int id_cliente = Int32.Parse(id);
            var info_cliente = (from cli in _db.Clientes
                                join pass in _db.GS_Pass on cli.ID equals pass.Clie
                                where pass.Clie == id_cliente
                                select new
                                {
                                    Orga = pass.Orga.Trim(),
                                    Usu  = pass.Usu.Trim(),
                                    Pass = pass.Pass.Trim()
                                });
            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(info_cliente);
            return View("Buscar", modelo);
        }
    }
}