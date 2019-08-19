using ModeloCRM;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;

namespace WebApplication.Areas.CRM.Controllers
{
    public class ContactosClientesController : MasterController
    {
        // GET: CRM/ContactosClientes
        [ClientAuthorize("MantCRM")]
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();
            var list_contactos = (from cli in _db2.Cliente_CRM
                                  join para in _db2.Parametros_CRM on cli.Industria equals para.Valor
                                  join us in _db2.Usu on cli.ID_Client_Service equals us.ID
                                  where cli.Estado == "1"
                                  select new {
                                      Id_cliente = cli.ID_Cliente,
                                      Nom_Cliente = cli.Nombre,
                                      Rut = cli.Rut,
                                      Industria = para.Texto,
                                      Profile = cli.Profile,
                                      Client_Service = us.Nom
                                  });
            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(list_contactos);
            return View(modelo);
        }
        [ClientAuthorize("MantCRM")]
        public ActionResult Create()
        {
            var lista_parametros = _db2.Parametros_CRM.Where(x => x.Grupo == "Reference").OrderBy(x => x.Texto).ToList();
            var lista_tipo_contacto = _db2.Tipo_Contacto_CRM.ToList();
            ViewBag.ListReference = new SelectList(lista_parametros, "Valor", "Texto");
            ViewBag.ListPositionInicial = new SelectList(lista_tipo_contacto.Where(x => x.Tipo == "Inicial"), "ID_Tipo_Contacto_CRM", "Nombre");
            ViewBag.ListPositionOperational = new SelectList(lista_tipo_contacto.Where(x => x.Tipo == "Operativo"), "ID_Tipo_Contacto_CRM", "Nombre");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Contacto_Cliente_CRM con_clie, int[] check)
        {
            try
            {
                con_clie.ID_Cliente = 6;                
                con_clie.Fecha_Mod = DateTime.Now;
                con_clie.Usuario_Mod = SesionLogin().Sigla;
                con_clie.Estado = "A";
                _db2.Contacto_Cliente_CRM.Add(con_clie);
                _db2.SaveChanges();

                if (check.Any())
                {
                    List<Contactos_Operacionales_CRM> List_con_op = new List<Contactos_Operacionales_CRM>();
                    foreach (var item in check)
                    {
                        var new_con_op = new Contactos_Operacionales_CRM();
                        new_con_op.ID_Contacto_Cliente = con_clie.ID_Contacto_Cliente;
                        new_con_op.ID_Tipo_Contacto_CRM = item;
                        List_con_op.Add(new_con_op);
                    }
                    //var list_cont = _db.Contactos_Operacionales_CRM.Where(x => x.ID_Contacto_Cliente == con_clie.ID_Contacto_Cliente).ToList();
                    //_db.Contactos_Operacionales_CRM.RemoveRange(list_cont);
                    _db2.Contactos_Operacionales_CRM.AddRange(List_con_op);
                    _db2.SaveChanges();
                }

                return JsonExitoMsg("Creado");
            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }            
        }

        public ActionResult Editar(int id)
        {
            var contacto = _db2.Contacto_Cliente_CRM.FirstOrDefault(x => x.ID_Contacto == id);
            var lista_parametros = _db2.Parametros_CRM.Where(x => x.Grupo == "Reference").ToList();
            var lista_tipo_contacto = _db2.Tipo_Contacto_CRM.ToList();             
            ViewBag.ListB = _db2.Contactos_Operacionales_CRM.Where(x => x.ID_Contacto_Cliente == id).ToList().Select(x => x.ID_Tipo_Contacto_CRM).ToList();
            ViewBag.ListReference = new SelectList(lista_parametros, "Valor", "Texto");
            ViewBag.ListPositionInicial = new SelectList(lista_tipo_contacto.Where(x => x.Tipo == "Inicial"), "ID_Tipo_Contacto_CRM", "Nombre");
            ViewBag.ListPositionOperational = new SelectList(lista_tipo_contacto.Where(x => x.Tipo == "Operativo"), "ID_Tipo_Contacto_CRM", "Nombre");
            return View("Create", contacto);
        }

        [HttpPost]
        public ActionResult Editar(Contacto_Cliente_CRM con_clie, int[] check)
        {
            try
            {
                var cli = _db2.Contacto_Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == con_clie.ID_Cliente);
                cli.Fecha_Mod = DateTime.Now;
                cli.Usuario_Mod = SesionLogin().Sigla;
                cli.Estado = "A";
                cli.Contacto_CRM.Nombre = con_clie.Contacto_CRM.Nombre;
                cli.Contacto_CRM.Apellido = con_clie.Contacto_CRM.Apellido;
                cli.Contacto_CRM.Rut = con_clie.Contacto_CRM.Rut;
                cli.Telefono_Personal = con_clie.Telefono_Personal;
                cli.Telefono_Trabajo = con_clie.Telefono_Trabajo;
                cli.email = con_clie.email;
                cli.Fax = con_clie.Fax;
                cli.Cargo = con_clie.Cargo;
                _db2.Entry(cli).State = System.Data.Entity.EntityState.Modified;
                _db2.SaveChanges();

                if (check.Any())
                {
                    List<Contactos_Operacionales_CRM> List_con_op = new List<Contactos_Operacionales_CRM>();
                    foreach (var item in check)
                    {
                        var new_con_op = new Contactos_Operacionales_CRM();
                        new_con_op.ID_Contacto_Cliente = cli.ID_Contacto_Cliente;
                        new_con_op.ID_Tipo_Contacto_CRM = item;
                        List_con_op.Add(new_con_op);
                    }
                    var list_cont = _db2.Contactos_Operacionales_CRM.Where(x => x.ID_Contacto_Cliente == cli.ID_Contacto_Cliente).ToList();
                    if (list_cont.Any())
                    {
                        _db2.Contactos_Operacionales_CRM.RemoveRange(list_cont);
                    }
                    _db2.Contactos_Operacionales_CRM.AddRange(List_con_op);
                    _db2.SaveChanges();
                }
                return JsonExitoMsg("Editado");
            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }
        }

        public ActionResult Eliminar(int id)
        {
            var contacto_cliente = _db2.Contacto_Cliente_CRM.FirstOrDefault(x => x.ID_Contacto_Cliente == id);
            contacto_cliente.Estado = "E";
            contacto_cliente.Fecha_Mod = DateTime.Now;
            contacto_cliente.Usuario_Mod = SesionLogin().Sigla;
            _db.Entry(contacto_cliente).State = System.Data.Entity.EntityState.Modified;
            _db.SaveChanges();
            return JsonExitoMsg("Eliminacion");
        }

        public ActionResult Asociacion(int id)
        {
            dynamic modelo = new ExpandoObject();
            var cli = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id);
            var lista_contactos = (from con in _db2.Contacto_CRM
                                   join con_cli in _db2.Contacto_Cliente_CRM on con.ID_Contacto equals con_cli.ID_Contacto
                                   where (con_cli.ID_Cliente == 6 || con_cli.ID_Cliente == cli.ID_Cliente) && con_cli.Estado == "A"
                                   orderby con.Nombre
                                   select new {
                                       ID_Cliente = con_cli.ID_Cliente,
                                       ID_Contacto_Cli = con_cli.ID_Contacto_Cliente,
                                       Nom_Contacto = con.Nombre + " " + con.Apellido
                                   });
            var list = lista_contactos.Where(x => x.ID_Cliente == cli.ID_Cliente).Select(p => p.ID_Contacto_Cli).ToList();

            ViewBag.Nombre = cli.Nombre;
            ViewBag.Id = cli.ID_Cliente;
            modelo.list_contact = new MultiSelectList(lista_contactos, "ID_Contacto_Cli", "Nom_Contacto", list);
            return View(modelo);
        }

        public JsonResult cargaContactos(int? id)
        {
            try
            {
                var list_cont = (from cc in _db2.Contacto_Cliente_CRM
                                 join c in _db2.Contacto_CRM on cc.ID_Contacto equals c.ID_Contacto
                                 join para in _db2.Tipo_Contacto_CRM on cc.Cargo equals para.ID_Tipo_Contacto_CRM
                                 where cc.Estado == "A"
                                 select new
                                 {
                                     id_contacto = cc.ID_Contacto,
                                     nombre = c.Nombre,
                                     apellido = c.Apellido,
                                     cargo = para.Nombre,
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

        public ActionResult Asociacion_Cliente_Contacto(int id,int[] contacto_id)
        {
            try
            {
                var existen = _db2.Contacto_Cliente_CRM.Where(x => contacto_id.Contains(x.ID_Contacto_Cliente)).ToList();
                var no_existen = _db2.Contacto_Cliente_CRM.Where(x => !contacto_id.Contains(x.ID_Contacto_Cliente)).Where(x => x.ID_Cliente == id).ToList();
                if (existen.Any())
                {
                    existen.ForEach(x => { x.ID_Cliente = id; x.Fecha_Mod = DateTime.Now; x.Usuario_Mod = SesionLogin().Sigla; });
                    _db.SaveChanges();
                }
                if (no_existen.Any())
                {
                    no_existen.ForEach(x => { x.ID_Cliente = 6; x.Fecha_Mod = DateTime.Now; x.Usuario_Mod = SesionLogin().Sigla; });
                    _db.SaveChanges();
                }
                return JsonExitoMsg("Asignacion");
            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }
        }
    }
}