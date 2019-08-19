using ModeloCRM;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.CRM.Controllers
{
    public class PropuestaServicioController : MasterController
    {
        // GET: CRM/PropuestaServicio
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();

            var list_clientes = (from cl in _db2.Cliente_CRM
                                                  join pa in _db2.Pais_CRM on cl.ID_Pais equals pa.ID_Pais
                                                  join para in _db2.Parametros_CRM on cl.Industria equals para.Valor
                                                  join us in _db.Usu on cl.ID_Client_Service equals us.ID                                                  
                                                  select new
                                                  {
                                                      Id_cliente = cl.ID_Cliente,
                                                      Nom_Cliente = cl.Nombre,
                                                      Rut_Cliente = cl.Rut,
                                                      Primer_Contacto = cl.Fecha_Primer_Contacto,
                                                      Clien_Service = us.Nom,
                                                      Pais = pa.Nombre_Pais,
                                                      Industria = para.Texto
                                                  });

            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_clientes);
            return View(modelo);
        }

        public ActionResult AgregarPropuesta (int id)
        {
            var cli = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
            ViewBag.categorias = new SelectList(_db2.Parametros_CRM.Where(x => x.Valor == "DOCUMPROPO"), "valor", "texto");
            ViewBag.ID_CLiente = cli.ID_Cliente;
            ViewBag.Nombre = cli.Nombre;
            return View();
        }

        public JsonResult ServiciosLista(int id)
        {
            try
            {
                var clie = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
                var list_servi_grupo = (from ser in _db2.Servicio_CRM
                                        orderby (ser.ID_Servicio < ser.ID_Padre || ser.ID_Padre == null ? ser.ID_Servicio : ser.ID_Padre),
                                                (ser.ID_Servicio < ser.ID_Padre || ser.ID_Padre == null ? ser.ID_Padre : ser.ID_Servicio)
                                        select new {
                                            ID_servicio = ser.ID_Servicio,
                                            ID_Padre = ser.ID_Padre,
                                            Nombre = ser.Nombre_Servicio
                                        }).ToList();
                

                return Json(new { data = list_servi_grupo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {                
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}