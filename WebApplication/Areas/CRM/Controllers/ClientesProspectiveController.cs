using ModeloCRM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.CRM.Controllers
{
    public class ClientesProspectiveController : MasterController
    {
        // GET: CRM/ClientesProspective
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
                                 where /*pro.Fecha_Aceptacion == null &&*/ cl.Estado == "1"
                                 group new { cl, ser_cli, us, pa, par } 
                                 by new { cl.ID_Cliente, cl.Nombre, cl.Rut, cl.Fecha_Primer_Contacto, us.Nom, pa.Nombre_Pais, par.Texto }
                                 into resultSet
                                 select new
                                 {
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


        public ActionResult Create()
        {
            //var lista_tipo_contacto = _db.Tipo_Contacto_CRM.ToList();
            //ViewBag.ListReference = new SelectList(lista_parametros, "Valor", "Texto");
            //ViewBag.ListPosition = new SelectList(lista_tipo_contacto, "ID_Tipo_Contacto_CRM", "Nombre");
            //var lista_Clientes_Cliente = (from cli_c in _db.Cliente_CRM where cli_c.Estado == "1" && cli_c.ID_Cliente_Cliente != null select cli_c).ToList();
            var lista_Clientes_Cliente = _db2.Cliente_CRM.Where(x => x.ID_Cliente_Cliente == null).ToList();//Validar solo clientes que sean definidos como clientes y no entidades
            var lista_paises = _db2.Pais_CRM.ToList();
            var lista_personal_pso_CS = (from u in _db2.Usu where u.unidad == 71 && u.uactivo == "s" select u).ToList();
            var lista_personal_pso_P = (from u in _db2.Usu where u.unidad == 79 && u.uactivo == "s" && u.cargo == 26 select u).ToList();
            var lista_parametros = _db2.Parametros_CRM.Where(x => x.Grupo == "Industry").ToList();
            var lista_regiones = _db2.Region_CRM.OrderBy(x => x.Nom_Region).ToList();
            //var lista_region = _db.Region_CRM.ToList();
            var lista_referencia = _db2.Parametros_CRM.Where(x => x.Grupo == "Reference").OrderBy(x => x.Texto).ToList();
            var lista_Clie_Enti = _db2.Parametros_CRM.Where(x => x.Grupo == "Clie_Enti").ToList();
            ViewBag.ListEntidadCliente = new SelectList(lista_Clie_Enti, "Valor", "Texto");
            ViewBag.ListReference = new SelectList(lista_referencia, "Valor", "Texto");
            ViewBag.ListCountry = new SelectList(lista_paises, "ID_Pais", "Nombre_Pais", 1);
            ViewBag.ListIndustry = new SelectList(lista_parametros, "Valor", "Texto");
            ViewBag.ListClientService = new SelectList(lista_personal_pso_CS, "ID", "Nom");
            ViewBag.ListPartner = new SelectList(lista_personal_pso_P, "ID", "Nom");
            ViewBag.ListaClienteCliente = new SelectList(lista_Clientes_Cliente, "Id_Cliente", "Nombre");
            ViewBag.ListRegiones = new SelectList(lista_regiones, "ID_Region", "Nom_Region");
            //ViewBag.ListComuna = new SelectList(lista_comuna, "ID_Comuna", "Nombre_Comuna");
            //ViewBag.ListRegion = new SelectList(lista_region, "ID_Region", "Nombre_Region");
            return View();
        }

        [HttpPost]
        public ActionResult Create(ModeloCRM.Direccion_CRM con_clie, int Cod_Comuna)
        {
            try
            {
                con_clie.Cliente_CRM.Estado = "1";
                con_clie.Cliente_CRM.Fecha_Mod = DateTime.Now;
                con_clie.Cliente_CRM.Usuario_Mod = SesionLogin().Sigla;
                con_clie.Usu_Mod = SesionLogin().Sigla;
                con_clie.Fecha_Mod = DateTime.Now;
                con_clie.ID_Comuna = Cod_Comuna;
                con_clie.Casa_Matriz = true;
                _db2.Direccion_CRM.Add(con_clie);
                _db2.SaveChanges();
                return JsonExitoMsg("Creado");
            }
            catch(Exception e)
            {
                return JsonError(e.Message);
            }            
        }

        public ActionResult Editar(int id)
        {
            var cli = _db2.Direccion_CRM.FirstOrDefault(x => x.ID_Cliente == id);
            var lista_paises = _db2.Pais_CRM.ToList();
            var lista_personal_pso_CS = (from u in _db2.Usu where u.unidad == 71 && u.uactivo == "s" select u).ToList();
            var lista_personal_pso_P = (from u in _db2.Usu where u.unidad == 79 && u.uactivo == "s" && u.cargo == 26 select u).ToList();
            var lista_parametros = _db2.Parametros_CRM.Where(x => x.Grupo == "Industry").ToList();
            var lista_referencia = _db2.Parametros_CRM.Where(x => x.Grupo == "Reference").OrderBy(x => x.Texto).ToList();
            var lista_Clie_Enti = _db2.Parametros_CRM.Where(x => x.Grupo == "Clie_Enti").ToList();
            var lista_Clientes_Cliente = _db2.Cliente_CRM.Where(x => x.ID_Cliente_Cliente == null).ToList();
            ViewBag.ListaClienteCliente = new SelectList(lista_Clientes_Cliente, "Id_Cliente", "Nombre");
            ViewBag.ListCountry = new SelectList(lista_paises, "ID_Pais", "Nombre_Pais", cli.Cliente_CRM.ID_Pais);
            ViewBag.ListIndustry = new SelectList(lista_parametros, "Valor", "Texto", cli.Cliente_CRM.Industria);
            ViewBag.ListClientService = new SelectList(lista_personal_pso_CS, "ID", "Nom", cli.Cliente_CRM.ID_Client_Service);
            ViewBag.ListPartner = new SelectList(lista_personal_pso_P, "ID", "Nom", cli.Cliente_CRM.ID_Parther);
            ViewBag.ListReference = new SelectList(lista_referencia, "Valor", "Texto", cli.Cliente_CRM.Referencia_Cliente);
            ViewBag.ListEntidadCliente = new SelectList(lista_Clie_Enti, "Valor", "Texto",cli.Cliente_CRM.ID_Cliente_Cliente);
            //ViewBag.ListRegiones = new SelectList(lista_regiones, "ID_Region", "Nom_Region");
            return View("~/Areas/CRM/Views/ClientesProspective/Create.cshtml", cli);
        }

        [HttpPost]
        public ActionResult Editar(ModeloCRM.Direccion_CRM con_clie)
        {
            try
            {
                var cli = _db2.Direccion_CRM.FirstOrDefault(x => x.ID_Cliente == con_clie.ID_Cliente);
                cli.Cliente_CRM.Nombre = con_clie.Cliente_CRM.Nombre;
                cli.Cliente_CRM.Rut = con_clie.Cliente_CRM.Rut;
                cli.Cliente_CRM.ID_Pais = con_clie.Cliente_CRM.ID_Pais;
                cli.Cliente_CRM.Industria = con_clie.Cliente_CRM.Industria;
                cli.Cliente_CRM.ID_Parther = con_clie.Cliente_CRM.ID_Parther;
                cli.Cliente_CRM.ID_Client_Service = con_clie.Cliente_CRM.ID_Client_Service;
                cli.Pagina_Web = con_clie.Pagina_Web;
                cli.Cliente_CRM.Profile = con_clie.Cliente_CRM.Profile;
                cli.Cliente_CRM.Perfil = con_clie.Cliente_CRM.Perfil;
                cli.Cliente_CRM.Fecha_Primer_Contacto = con_clie.Cliente_CRM.Fecha_Primer_Contacto;
                //cli.Cliente_CRM.ID_Cliente = con_clie.ID_Cliente;
                //cli.Cliente_CRM.Estado = con_clie.Cliente_CRM.Estado;
                cli.Cliente_CRM.Fecha_Mod = DateTime.Now;
                cli.Cliente_CRM.Usuario_Mod = SesionLogin().Sigla;
                con_clie.Usu_Mod = SesionLogin().Sigla;
                con_clie.Fecha_Mod = DateTime.Now;
                _db2.Entry(cli).State = System.Data.Entity.EntityState.Modified;
                _db2.SaveChanges();
                return JsonExitoMsg("Editado");
            }
            catch(Exception e)
            {
                return JsonError(e.Message);
            }
        }

        public JsonResult ObtenerRegiones()
        {         
            var list_reg = _db2.Region_CRM.ToList().OrderBy(x => x.Nom_Region);
            var json = list_reg.Select(x => new { x.ID_Region, x.Nom_Region });
            return Json(new { data = json }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerProvincia(int? id)
        {
            var list_prov = _db2.Provincias_CRM.Where(x => x.ID_Region == id).ToList().OrderBy(x => x.Nom_Provincia);
            var json = list_prov.Select(x =>  new { x.ID_Provincia, x.Nom_Provincia});
            return Json(new { data = json }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerComuna(int? id)
        {
            var list_comu = _db2.Comunas_CRM.Where(x => x.ID_Provincia == id).ToList().OrderBy(x => x.Nom_Comuna);
            var json = list_comu.Select(x => new { x.ID_Comuna, x.Nom_Comuna});
            return Json(new { data = json }, JsonRequestBehavior.AllowGet);
        }
        
        public JsonResult ObtenerDirecciones(int? id)
        {
            try {
                var json = (from dire in _db2.Direccion_CRM 
                            join com in _db2.Comunas_CRM on dire.ID_Comuna equals com.ID_Comuna
                            join pro in _db2.Provincias_CRM on com.ID_Provincia equals pro.ID_Provincia
                            join reg in _db2.Region_CRM on pro.ID_Region equals reg.ID_Region                            
                            where dire.ID_Cliente == id /*&& dire.Estado == "A"*/
                            orderby dire.Casa_Matriz
                            select new
                            {
                                id = dire.ID_Direccion,                                                                                               
                                direc = dire.Direccion,
                                matriz = dire.Casa_Matriz,
                                nume = dire.Numeracion,
                                piso = dire.Piso,
                                comuna = com.Nom_Comuna,
                                id_comuna = com.ID_Comuna,
                                provi = pro.Nom_Provincia,
                                id_provincia = pro.ID_Provincia,
                                region = reg.Nom_Region,
                                id_region = reg.ID_Region
                            });

                //var json = json_.Select(x => new { x.id, comuna_cont = x.comuna + "-" + x.id_comuna, region_cont = x.region + "-" + x.id_region, x.direc });

                return Json(new { data = json }, JsonRequestBehavior.AllowGet);
            }catch(Exception e)
            {
                return JsonError(e.Message);
            }
        }

        public ActionResult Direcciones(int? id)
        {
            //var lista_comuna = _db.Comunas_CRM.ToList();
            //var lista_region = _db.Region_CRM.ToList();
            //ViewBag.ListComuna = new SelectList(lista_comuna, "ID_Comuna", "Nombre_Comuna");
            //ViewBag.ListRegion = new SelectList(lista_region, "ID_Region", "Nombre_Region");
            var lista_regiones = _db2.Region_CRM.OrderBy(x => x.Nom_Region).ToList();
            ViewBag.ID_Cliente = id;
            ViewBag.ListRegiones = new SelectList(lista_regiones, "ID_Region", "Nom_Region");
            ViewBag.Nombre = _db2.Cliente_CRM.FirstOrDefault(x => x.ID_Cliente == id).Nombre;
            return View();
        }

        public ActionResult Crear_Direcciones(ModeloCRM.Direccion_CRM[] model, int id_cliente, string casa_matriz)
        {
            try
            {
                var direcciones_antes = _db2.Direccion_CRM.Where(x => x.ID_Cliente == id_cliente).ToList();
                var lista_direcciones = model.Select(x => x.ID_Direccion).ToList();
                //var direcciones_agregar = model.Select(x => x.ID_Direccion == 0);
                var eliminar_direcciones = direcciones_antes.Where(x => !lista_direcciones.Contains(x.ID_Direccion)).ToList();
                //var agregar_direcciones = direcciones_antes.Where(x => lista_direcciones.Contains(x.ID_Direccion)).ToList();
                //var listado = JsonConvert.DeserializeObject<List<PairValues>>(casa_matriz).ToList();
                //var direcciones = (from dire in _db.Direccion_CRM 
                //                   where dire.ID_Cliente == id_cliente
                //                   select dire.Id_Direccion).ToList();
                ////.Where(p => !softland_id.Contains(p.ID)
                //var dire_a_elminar = _db.Direccion_CRM.Where(x => direcciones.Contains(x.Id_Direccion)).ToList();
                string string_numeros = new String(casa_matriz.Where(Char.IsDigit).ToArray());
                var intList = string_numeros.Select(digit => int.Parse(digit.ToString())).ToList();
                //var ids_direcciones_originales = _db.Direccion_CRM.Where(x => x.ID_Cliente == id_cliente).ToList();
                List<ModeloCRM.Direccion_CRM> lista_direciones_add = new List<ModeloCRM.Direccion_CRM>();
                int contador = 0;

                foreach (var i in model)
                {
                    if(i.ID_Direccion == 0)
                    {
                        var nueva_dire = new ModeloCRM.Direccion_CRM();
                        nueva_dire.ID_Cliente = id_cliente;
                        nueva_dire.Casa_Matriz = intList[contador] == 1 ? true : false;                        
                        nueva_dire.Direccion = i.Direccion;
                        nueva_dire.ID_Comuna = i.ID_Comuna;
                        nueva_dire.Numeracion = i.Numeracion;
                        nueva_dire.Piso = i.Piso;
                        nueva_dire.Fecha_Mod = DateTime.Now;
                        nueva_dire.Usu_Mod = SesionLogin().Sigla;
                        lista_direciones_add.Add(nueva_dire);
                    }
                    contador = contador + 1;
                }
                _db2.Direccion_CRM.RemoveRange(eliminar_direcciones);
                _db2.SaveChanges();
                _db2.Direccion_CRM.AddRange(lista_direciones_add);
                _db2.SaveChanges();
                return JsonExito();
            }
            catch(Exception e)
            {
                JsonError(e.Message);
            }

            return JsonExito();
        }
        //public class PairValues
        //{
        //    public int id { get; set; }           
        //}
    }
}