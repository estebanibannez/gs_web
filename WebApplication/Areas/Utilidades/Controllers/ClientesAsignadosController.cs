using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class ClientesAsignadosController : MasterController
    {
        // GET: ClientesAsignados/ClientesAsignados
        [ClientAuthorize("MantCliA")]
        public ActionResult Index()
        {
            var listadoUsuarios = (from usu in _db.Usu where usu.uactivo =="S"
                                   select new { usu.ID, usu.Nom}).OrderBy(x => x.Nom);
            var listadoClientes = (from clie in _db.Clientes where clie.Activo == "S"
                           select new { clie.ID , clie.Nom }).OrderBy(x => x.Nom).ToList();
            ViewBag.listadoClientes = listadoClientes;
            ViewBag.listadoUsuarios = listadoUsuarios;
            return View();
        }


        public JsonResult dt_ClieAsignados(int? id)
        {
            try
            {
                var clieAsignados =  (from param in _db.parametros
                                     join usu in _db.Usu on param.id equals usu.unidad
                                     join car in _db.GS_Cargos on usu.cargo equals car.id
                                     join asig in _db.GS_Asig_Carg_Clie on usu.ID equals asig.usu
                                     where asig.clie == id && usu.uactivo == "S"
                                     select new
                                     {
                                        nombreUsuario = usu.Nom,
                                        nombreCargo = car.nom,
                                        nombreUnidad = param.nom
                                     }).ToList();
                return Json(new { data = clieAsignados }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return JsonError("");
            }
        }

        public JsonResult dt_empAsignadas(int? id) {

            try {

                var usuAsignados = (from asig in _db.GS_Asig_Carg_Clie
                                    join clie in _db.Clientes on asig.clie equals clie.ID
                                    join usu in _db.Usu on asig.usu equals usu.ID
                                    join param in _db.parametros on usu.unidad equals param.id
                                    where asig.usu == id

                                    select new
                                    {
                                        nombreCliente = clie.Nom

                                    }).ToList();

                return Json(new { data = usuAsignados }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e) {
                return JsonError("");
            }
        }

    }
}