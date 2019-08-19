using ModeloCRM;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.CRM.Controllers
{
    public class CuestionarioClienteController : MasterController
    {
        // GET: CRM/CuestionarioCliente
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();
            var list_clientes = (from cl in _db2.Cliente_CRM
                                 join cu in _db2.Cuestionario_CRM on cl.ID_Cliente equals cu.ID_Cliente
                                 where cu.Fecha_Cuestionario_Enviado == null && cl.Estado == "1"
                                 select cl.ID_Cliente).ToList();

            var list_clientes_sin_Cuestionario = (from cl in _db2.Cliente_CRM
                                                  join pa in _db2.Pais_CRM on cl.ID_Pais equals pa.ID_Pais
                                                  join para in _db2.Parametros_CRM on cl.Industria equals para.Valor
                                                  join us in _db2.Usu on cl.ID_Client_Service equals us.ID
                                                  where (!list_clientes.Contains(cl.ID_Cliente))
                                                  select new {
                                                      Id_cliente = cl.ID_Cliente,
                                                      Nom_Cliente = cl.Nombre,
                                                      Rut_Cliente = cl.Rut,
                                                      Primer_Contacto = cl.Fecha_Primer_Contacto,
                                                      Clien_Service = us.Nom, 
                                                      Pais = pa.Nombre_Pais,
                                                      Industria = para.Texto
                                                  });
            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_clientes_sin_Cuestionario);
            return View(modelo);
        }

        public ActionResult AgregarCuestionario(int id)
        {
            //var propuesta = _db.Propuesta_CRM.FirstOrDefault(x => x.ID_Propuesta == id);
            var cli = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
            ViewBag.categorias = new SelectList(_db2.Parametros_CRM.Where(x => x.Valor == "DOCUMQUEST"), "valor", "texto");
            ViewBag.ID_CLiente = cli.ID_Cliente;
            ViewBag.Nombre = cli.Nombre;
            return View();
        }
    }
}