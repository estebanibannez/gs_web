using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.CRM.Controllers
{
    public class ClientesPropuetasActivasController : MasterController
    {
        // GET: CRM/ClientesPropuetasActivas
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();
            var list_clientes = (from cl in _db2.Cliente_CRM
                                 join ser_cli in _db2.Servicio_Cliente_CRM on cl.ID_Cliente equals ser_cli.ID_Cliente
                                 join pro in _db2.Propuesta_CRM on ser_cli.ID_Propuesta equals pro.ID_Propuesta
                                 join para in _db2.Parametros_CRM on pro.Fase equals para.Valor
                                 join par in _db2.Parametros_CRM on cl.Industria equals par.Valor
                                 join us in _db2.Usu on cl.ID_Client_Service equals us.ID
                                 join pa in _db2.Pais_CRM on cl.ID_Pais equals pa.ID_Pais
                                 where pro.Fecha_Aceptacion != null && cl.Estado == "1"
                                 group new { cl , ser_cli , us , pa , par } by new { cl.ID_Cliente, cl.Nombre, cl.Rut, cl.Fecha_Primer_Contacto, us.Nom, pa.Nombre_Pais, par.Texto }
                                 into resultSet
                                 select new {
                                     Id_cliente = resultSet.Key.ID_Cliente,
                                     Nom_Cliente = resultSet.Key.Nombre,
                                     Rut_Cliente = resultSet.Key.Rut,
                                     Primer_Contacto = resultSet.Key.Fecha_Primer_Contacto,
                                     Clien_Service = resultSet.Key.Nom,   
                                     Pais = resultSet.Key.Nombre_Pais,
                                     Industria = resultSet.Key.Texto
                                 });

            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_clientes);
            return View(modelo);            
        }

        public ActionResult DetallePropuesta(int id)
        {
            var cliente = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
            ViewBag.ID_CLiente = cliente.ID_Cliente;
            ViewBag.Nombre = cliente.Nombre;
            return View();
        }

        public JsonResult Servicios(int id)
        {
            try
            {
                var clie = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
                var list_services = (from ser_cli in _db2.Servicio_Cliente_CRM
                                    join ser in _db2.Servicio_CRM on ser_cli.ID_Servicio equals ser.ID_Servicio
                                    join pro in _db2.Propuesta_CRM on ser_cli.ID_Propuesta equals pro.ID_Propuesta
                                    join para in _db2.Parametros_CRM on ser_cli.Tipo_Moneda equals para.Valor
                                    where ser_cli.ID_Cliente == clie.ID_Cliente && pro.Fecha_Aceptacion != null
                                    orderby (ser.ID_Servicio < ser.ID_Padre || ser.ID_Padre == null ? ser.ID_Servicio : ser.ID_Padre),
                                    (ser.ID_Servicio < ser.ID_Padre || ser.ID_Padre == null ? ser.ID_Padre : ser.ID_Servicio)
                                     select new
                                    {
                                        ID_servicio = ser.ID_Servicio,
                                        Nom_Servicio = ser.Nombre_Servicio,
                                        Valor = ser_cli.valor,
                                        Tipo_Moneda = para.Texto
                                    }).ToList();

                return Json(new { data = list_services }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {                
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Documentos(int id)
        {
            try
            {                
                var clie = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
                var list_document = (from doc in _db2.Documento_CRM
                                    join para in _db2.Parametros_CRM on doc.Categoria equals para.Valor
                                    where doc.ID_Cliente == clie.ID_Cliente && doc.Estado == 1
                                    select new
                                    {
                                        ID_Documento = doc.ID_Documento,
                                        Tipo_Documento = para.Texto,
                                        Fec_Aprobacion = doc.Fecha_Documento
                                    }).OrderBy(x => x.Tipo_Documento).ToList();                

                return Json(new { data = list_document }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {                
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Archivo(string id)
        {
            try
            {
                var doc = Int32.Parse(id);
                MemoryStream ms = new MemoryStream();
                var documento = _db2.Documento_CRM.FirstOrDefault(p => p.ID_Documento == doc);
                using (FileStream file = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(documento.Ruta), FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                }
                return File(ms.ToArray(), MimeMapping.GetMimeMapping(System.Web.Hosting.HostingEnvironment.MapPath(documento.Ruta)));

            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }
        }

        public ActionResult DetalleCliente(int id)
        {
            dynamic modelo = new ExpandoObject();
            var cli = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
            var cliente = (from cl in _db2.Cliente_CRM                        
                           join par in _db2.Parametros_CRM on cl.Industria equals par.Valor
                           join us in _db2.Usu on cl.ID_Client_Service equals us.ID
                           join u in _db2.Usu on cl.ID_Parther equals u.ID
                           join pa in _db2.Pais_CRM on cl.ID_Pais equals pa.ID_Pais
                           //join dir_cli in _db.Direccion_Cliente_CRM on cl.ID_Cliente equals dir_cli.ID_Cliente
                           //join dir in _db.Direccion_CRM on dir
                           where cl.ID_Cliente == cli.ID_Cliente
                           select new {
                               ID_Cliente = cl.ID_Cliente,
                               Rut = cl.Rut,
                               Nombre = cl.Nombre,
                               Industria = par.Texto,
                               Primer_Contacto = cl.Fecha_Primer_Contacto,
                               Perfil = cl.Perfil,
                               Profile = cl.Profile,
                               Nom_Client = us.Nom,
                               Nom_parther = u.Nom,
                               Nom_pais = pa.Nombre_Pais
                           });

            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(cliente);
            ViewBag.Nombre = cli.Nombre;
            ViewBag.ID_CLiente = cli.ID_Cliente;
            return View(modelo);
        }

        public JsonResult cargaPersonal(int? id)
        {
            try
            {
                var clie_CRM = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
                if (!(clie_CRM.Nombre == ""))
                {
                    var clie_GS = _db2.Clientes.Where(x => x.Nom.Contains(clie_CRM.Nombre)).First();
                    var clieAsignados = (from param in _db2.parametros
                                         join usu in _db2.Usu on param.id equals usu.unidad
                                         join car in _db2.GS_Cargos on usu.cargo equals car.id
                                         join asig in _db2.GS_Asig_Carg_Clie on usu.ID equals asig.usu
                                         where asig.clie == clie_GS.ID && usu.uactivo == "S"
                                         select new
                                         {
                                             nombreUsuario = usu.Nom,
                                             nombreCargo = car.nom,
                                             nombreUnidad = param.nom,
                                             extencion = usu.Anexo,
                                             email = usu.email
                                         }).ToList();

                    return Json(new { data = clieAsignados }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                JsonError(e.Message);
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult cargaContactos(int? id)
        {
            try
            {
                var list_cont = (from cc in _db2.Contacto_Cliente_CRM
                                 join c in _db2.Contacto_CRM on cc.ID_Contacto equals c.ID_Contacto
                                 join tc in _db2.Tipo_Contacto_CRM on cc.Cargo equals tc.ID_Tipo_Contacto_CRM
                                 where cc.ID_Cliente == id
                                 select new
                                 {
                                     nombre = c.Nombre,
                                     apellido = c.Apellido,
                                     cargo = tc.Nombre,
                                     email = cc.email,
                                     telefono = cc.Telefono_Personal
                                 }).ToList();
                return Json(new { data = list_cont }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                JsonError(e.Message);
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}