using ModeloCRM;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.CRM.Controllers
{
    public class CRMController : MasterController
    {
        // GET: CRM/CRM
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();
            var list_clien = (from c in _db2.Cliente_CRM
                              join pa in _db2.Pais_CRM on c.ID_Pais equals pa.ID_Pais
                              into palist
                              from pa in palist.DefaultIfEmpty()

                              join p in _db2.Parametros_CRM on c.Industria equals p.Valor
                              into plist
                              from p in plist.DefaultIfEmpty()

                              where p.Grupo == "Industry"
                              select new
                              {
                                  id = c.ID_Cliente,
                                  nombre = c.Nombre,
                                  rut = c.Rut,
                                  pais = pa.Nombre_Pais,
                                  industria = p.Texto,
                                  fec_pri_con = c.Fecha_Primer_Contacto,
                                  tipo_estado = c.Estado
                              });

            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_clien);

            return View(modelo);
        }
        [HttpPost]
        public JsonResult cargaContactos(int? id)
        {
            try
            {
                var list_cont = (from cc in _db2.Contacto_Cliente_CRM
                                 join c in _db2.Contacto_CRM on cc.ID_Contacto equals c.ID_Contacto
                                 where cc.ID_Cliente == id
                                 select new
                                 {
                                     nombre = c.Nombre,
                                     apellido = c.Apellido,
                                     cargo = cc.Cargo,
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
        [HttpPost]
        public JsonResult cargaPersonal(int? id)
        {
            try
            {
                var clie_CRM = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
                if(!(clie_CRM.Nombre == ""))
                {
                    var clie_GS = _db2.Clientes.Where(x => x.Nom.Contains(clie_CRM.Nombre)).First();
                    var clieAsignados = (from param in _db.parametros
                                         join usu in _db.Usu on param.id equals usu.unidad
                                         join car in _db.GS_Cargos on usu.cargo equals car.id
                                         join asig in _db.GS_Asig_Carg_Clie on usu.ID equals asig.usu
                                         where asig.clie == clie_GS.ID && usu.uactivo == "S"
                                         select new
                                         {
                                             nombreUsuario = usu.Nom,
                                             nombreCargo = car.nom,
                                             nombreUnidad = param.nom,
                                             extencion = usu.Anexo
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

        [HttpPost]
        public JsonResult cargaServicios()
        {
            try
            { 
                var list_servicios = (from cli in _db2.Cliente_CRM
                                      join sc in _db2.Servicio_Cliente_CRM on cli.ID_Cliente equals sc.ID_Cliente
                                      join pro in _db2.Propuesta_CRM on sc.ID_Propuesta equals pro.ID_Propuesta
                                      join cue in _db2.Cuestionario_CRM on pro.ID_Cuestionario equals cue.ID_Cuestionario
                                      join par in _db2.Parametros_CRM on pro.Fase equals par.Valor

                                      group new { cli, sc, pro, cue, par } by new { pro.ID_Propuesta, cli.Nombre, par.Texto, pro.Comentario, pro.Fecha_Propuesta, pro.Fecha_Aceptacion, cue.Nombre_Cuestionario } 
                                      into resultSet

                                      select new
                                      {
                                          id_propuesta = resultSet.Key.ID_Propuesta,
                                          nombreCliente = resultSet.Key.Nombre,
                                          nombreStatus = resultSet.Key.Texto,
                                          comentariPropuesta = resultSet.Key.Comentario,
                                          fechaPropuesta = resultSet.Key.Fecha_Propuesta,
                                          fechaAceptacion = resultSet.Key.Fecha_Aceptacion,
                                          nombreCuestionario = resultSet.Key.Nombre_Cuestionario
                                      }).ToList();

                return Json(new { data = list_servicios }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                JsonError(e.Message);
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Create()
        {
            var lista_paises = _db2.Pais_CRM.ToList();
            var lista_parametros = _db2.Parametros_CRM.Where(x => x.Grupo == "Industry" || x.Grupo == "Status").ToList();
            var lista_tipo_contacto = _db2.Tipo_Contacto_CRM.ToList();
            var lista_personal_pso_CS = (from u in _db2.Usu where u.unidad == 71 && u.uactivo == "s" select u).ToList();
            var lista_personal_pso_P = (from u in _db2.Usu where u.unidad == 79 && u.uactivo == "s" && u.cargo == 26 select u).ToList();

            ViewBag.ListCountry = new SelectList(lista_paises, "ID_Pais", "Nombre",1);
            ViewBag.ListIndustry = new SelectList(lista_parametros.Where(x => x.Grupo == "Industry"), "Valor","Texto");
            ViewBag.ListStatus = new SelectList(lista_parametros.Where(x => x.Grupo == "Status"), "Valor", "Texto");
            ViewBag.ListPosition = new SelectList(lista_tipo_contacto, "ID_Tipo_Contacto_CRM", "Nombre");
            ViewBag.ListClientService = new SelectList(lista_personal_pso_CS,"ID","Nom");
            ViewBag.ListPartner = new SelectList(lista_personal_pso_P, "ID", "Nom");
            return View();
        }
        [HttpPost]
        public ActionResult Create(Contacto_Cliente_CRM contacto_cliente)
        {
            var fase = contacto_cliente.Cliente_CRM.Estado;

            if(fase == "STATUSLEAD")
            {
                _db2.Cliente_CRM.Add(contacto_cliente.Cliente_CRM);
                _db2.Contacto_CRM.Add(contacto_cliente.Contacto_CRM);
                _db2.Contacto_Cliente_CRM.Add(contacto_cliente);
                _db2.SaveChanges();
                return JsonExito();
            }
            else if (fase == "STATUSPROSPECT")
            {
                _db2.Cliente_CRM.Add(contacto_cliente.Cliente_CRM);
                _db2.Contacto_CRM.Add(contacto_cliente.Contacto_CRM);
                _db2.Contacto_Cliente_CRM.Add(contacto_cliente);
                _db2.SaveChanges();
                return JsonExito();
            } else if(fase == "STATUSCLIE")
            {
                _db2.Cliente_CRM.Add(contacto_cliente.Cliente_CRM);
                _db2.Contacto_CRM.Add(contacto_cliente.Contacto_CRM);
                _db2.Contacto_Cliente_CRM.Add(contacto_cliente);
                _db2.SaveChanges();
                return JsonExito();
            }
            return JsonExito();
        }

        public ActionResult ListaServicios(int id)
        {
            dynamic modelo = new ExpandoObject();
            //var clie = _db.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
            //var cli = _db.Propuesta_CRM.FirstOrDefault(x => x.ID_Propuesta == id);
            var cli = _db2.Servicio_Cliente_CRM.FirstOrDefault(x => x.ID_Propuesta == id).ID_Cliente;
            var list_servi_grupo = (from ser in _db2.Servicio_CRM
                                    join par in _db2.Parametros_CRM on ser.Valor equals par.Valor
                                    join sc in _db2.Servicio_Cliente_CRM on ser.ID_Servicio equals sc.ID_Servicio
                                    join prop in _db2.Propuesta_CRM on sc.ID_Propuesta equals prop.ID_Propuesta
                                    where prop.ID_Propuesta == id
                                    select new
                                    {
                                        ID_servicio = ser.ID_Servicio,
                                        Grupo = par.Texto,
                                        Nombre = ser.Nombre_Servicio,
                                        FecPropuesta = prop.Fecha_Propuesta,
                                        FecAceptacion = prop.Fecha_Aceptacion
                                    }).OrderBy(x => x.Grupo);
            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_servi_grupo);           

            ViewBag.Nombre = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == cli).Nombre;
            return View(modelo);
        }

        public ActionResult AgregarPropuesta(int id)
        {

            //ViewBag.ListGroup = _db.Parametros_CRM.Where(x => x.Grupo == "Servicio").ToList();
            //ViewBag.LissService = _db.Servicio_CRM.ToList();
            //ViewBag.categorias = new SelectList(_db.parametros.Where(item => item.detalle == "VACAS").OrderBy(item => item.valor), "valor", "texto");
            ViewBag.categorias = new SelectList(_db2.Parametros_CRM.Where(x => x.Grupo == "Document"), "valor", "texto");
            ViewBag.ID_CLiente = id;
            ViewBag.Nombre = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id).Nombre;
            return View();

        }

        public JsonResult ServiciosLista(int id)
        {
            try
            {
                //dynamic modelo = new ExpandoObject();
                var clie = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
                var list_servi_grupo = (from ser in _db2.Servicio_CRM
                                        join par in _db2.Parametros_CRM on ser.Valor equals par.Valor
                                        select new
                                        {
                                            ID_servicio = ser.ID_Servicio,
                                            Grupo = par.Texto,
                                            Nombre = ser.Nombre_Servicio
                                        }).OrderBy(x => x.Grupo).ToList();
                //modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_servi_grupo);

                return Json(new { data = list_servi_grupo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                JsonError(e.Message);
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult AgregarPropuestaCliente(string nom)
        {
            return JsonExito();
        }

        //public ActionResult AgregarServicios(int id)
        //{
        //    try
        //    {
        //        dynamic modelo = new ExpandoObject();

        //        var list_servicios = (from c in _db.Cliente_CRM
        //                            join sc in _db.Servicio_Cliente_CRM on c.ID_Cliente equals sc.ID_Cliente
        //                            join s in _db.Servicio_CRM on sc.ID_Servicio equals s.ID_Servicio
        //                            join pa in _db.Parametros_CRM on s.Grupo equals pa.Valor 
        //                            join p in _db.Parametros_CRM on sc.Tipo_Moneda equals p.Grupo
        //                            select new
        //                            {
        //                                nombre = s.Nombre,
        //                                grupo_servicio = pa.Texto,
        //                                estado = sc.Estado_Activo,
        //                                fec_primer_contacto = sc.Primer_Contacto_Servicio,
        //                                fec_propuesta = sc.Fecha_Propuesta,
        //                                fec_ult_contacto = sc.Fecha_Ultimo_Contacto,
        //                                fec_termino = sc.Fecha_Termino,
        //                                razon_ter = sc.Razon_Termino,
        //                                comentario = sc.Comentario,k
        //                                valor = p.Texto,
        //                                hora = sc.Horas,
        //                                tipo_moneda = p.Texto
        //                            });

        //        modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_servicios);

        //        return View(list_servicios);
        //    }
        //    catch (Exception e)
        //    {
        //        return JsonError("Hey !!! hubo un problema.");
        //    }
        //}
    }
}