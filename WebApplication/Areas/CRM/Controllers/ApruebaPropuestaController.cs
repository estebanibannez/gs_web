using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.CRM.Controllers
{
    public class ApruebaPropuestaController : MasterController
    {
        // GET: CRM/ApruebaPropuesta
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();
            var list_clientes = (from cl in _db2.Cliente_CRM
                                 join ser_cli in _db2.Servicio_Cliente_CRM on cl.ID_Cliente equals ser_cli.ID_Cliente
                                 join pro in _db2.Propuesta_CRM on ser_cli.ID_Propuesta equals pro.ID_Propuesta
                                 join para in _db2.Parametros_CRM on pro.Fase equals para.Valor
                                 join us in _db2.Usu on cl.ID_Client_Service equals us.ID
                                 join cu in _db2.Cuestionario_CRM on pro.ID_Cuestionario equals cu.ID_Cuestionario
                                 where pro.Fecha_Aceptacion == null && cl.Estado == "1"
                                 group new { cl, us,cu ,pro } by new
                                 { cl.Nombre, cl.Rut, cl.Fecha_Primer_Contacto, us.Nom, cu.Fecha_Cuestionario_Recibido, cu.Fecha_Cuestionario_Enviado, pro.Fecha_Propuesta, pro.ID_Propuesta }
                                 into resultSet
                                 select new
                                 {
                                     Id_Propuesta = resultSet.Key.ID_Propuesta,
                                     Nom_Cliente = resultSet.Key.Nombre,
                                     Rut_Cliente = resultSet.Key.Rut,
                                     Clien_Service = resultSet.Key.Nom,
                                     Fec_Cuestionario_Enviado = resultSet.Key.Fecha_Cuestionario_Enviado,
                                     Fec_Cuestionario_Recibido = resultSet.Key.Fecha_Cuestionario_Recibido,
                                     Fec_Propuesta = resultSet.Key.Fecha_Propuesta
                                 });

            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_clientes);
            return View(modelo);            
        }

        public ActionResult CargarContrato(int id)
        {
            var propuesta = (from ser_cli in _db2.Servicio_Cliente_CRM
                             join ser in _db2.Servicio_CRM on ser_cli.ID_Servicio equals ser.ID_Servicio
                             where ser_cli.ID_Propuesta == id
                             orderby    (ser.ID_Servicio < ser.ID_Padre || ser.ID_Padre == null ? ser.ID_Servicio : ser.ID_Padre),
                                        (ser.ID_Servicio < ser.ID_Padre || ser.ID_Padre == null ? ser.ID_Padre : ser.ID_Servicio)
                             select ser);

            var cli = _db2.Servicio_Cliente_CRM.FirstOrDefault(x => x.ID_Propuesta == id).ID_Cliente;
            var moneda = _db2.Parametros_CRM.Where(x => x.Grupo == "Money").OrderBy(x => x.Texto).ToList();
            //ViewBag.categorias = new SelectList(_db.Parametros_CRM.Where(x => x.Valor == "DOCUMCONT"), "Valor", "Texto");
            ViewBag.Money = new SelectList(moneda, "Valor","Texto");
            ViewBag.ID_CLiente = cli;
            ViewBag.Nombre = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == cli).Nombre;
            return View(propuesta);
        }
    }
}