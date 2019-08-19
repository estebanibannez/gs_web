using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;
using System.Dynamic;
using System.Data.Entity;
using System.Data.SqlClient;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class UsuarioController : MasterController
    {
        // GET: Usuario/Usuario

        [ClientAuthorize("MantUsu")]
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();

            var lista_usuarios = (from usu in _db.Usu
                                  join usu_jefe in _db.Usu on usu.jefatura equals usu_jefe.ID into usu_usu_jefe
                                  join para_grupo in _db.parametros on usu.grupo equals para_grupo.id into usu_para_grupo
                                  join para_ger in _db.parametros on usu.ger equals para_ger.id into usu_para_ger
                                  join para_gerger in _db.parametros on usu.gergen equals para_gerger.id into usu_para_gerger
                                  join para_super in _db.parametros on usu.superv equals para_super.id into usu_para_super
                                  join para_unidad in _db.parametros on usu.unidad equals para_unidad.id into usu_para_unidad
                                  join car in _db.GS_Cargos on usu.cargo equals car.id into usu_car

                                  from usu_jefe in usu_usu_jefe.DefaultIfEmpty()
                                  from para_grupo in usu_para_grupo.DefaultIfEmpty()
                                  from para_ger in usu_para_ger.DefaultIfEmpty()
                                  from para_gerger in usu_para_gerger.DefaultIfEmpty()
                                  from para_super in usu_para_super.DefaultIfEmpty()
                                  from para_unidad in usu_para_unidad.DefaultIfEmpty()
                                  from car in usu_car.DefaultIfEmpty()
                                  orderby usu.Nom
                                  select new
                                  {
                                      IdUsuario = usu.ID,
                                      NombreUsu = usu.Nom,
                                      SiglaUsu = ((usu.Sigla.Trim().Length) < 4) ? " " : usu.Sigla.TrimStart().TrimEnd(),
                                      AnexoUsu = usu.Anexo,
                                      NomCorto = usu.nomcorto,
                                      NomSoftland = usu.nomususoft.Trim(),
                                      NomJefe = usu_jefe.Nom,
                                      Grupo = para_grupo.nom,
                                      Gerencia = para_ger.nom,
                                      Gerger = para_gerger.nom,
                                      NomSupervisor = para_super.nom,
                                      Estado = ((usu.uactivo.Trim() == "s") ? "Activos" : "Inactivos"),
                                      Unidad = para_unidad.nom,
                                      NomCargo = car.nom
                                  });

            var conA = lista_usuarios.Count(p => p.Estado == "Activos");
            var conI = lista_usuarios.Count(p => p.Estado == "Inactivos");

            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(lista_usuarios); 

            ViewBag.contadorActivo = conA;
            ViewBag.contadorInactivo = conI;
            ViewBag.sigusu = SesionLogin().Sigla;

            return View("Index", modelo);
        }

        [ClientAuthorize("MantUsu")]
        public ActionResult Create()
        {
            var aux1 = (from para in _db.parametros
                        orderby para.nom
                        select para).ToList();

            var aux2 = (from usu in _db.Usu
                        orderby usu.Nom
                        select usu).ToList();

            var aux3 = (from usu in _db.Usu
                        join usu_jefe in _db.Usu on usu.jefatura equals usu_jefe.ID
                        orderby usu_jefe.Nom
                        select usu_jefe).ToList();

            var aux4 = (from car in _db.GS_Cargos
                        orderby car.nom
                        select car).ToList();          

            var gerente = new SelectList(aux1.Where(p => p.tipo == "gg"), "id", "Nom");// Lista que se filtra por el tipo parametro "gg"
            var gerencia = new SelectList(aux1.Where(p => p.tipo == "gess" || (p.tipo == "gebo")), "id", "Nom");// Lista que se filtra por el tipo parametro "gess" o "gebo"
            var grupo = new SelectList(aux1.Where(p => p.tipo == "ct" || (p.tipo == "cpy") || (p.tipo == "gbo")), "id", "Nom");// Lista que se filtra por el tipo parametro "ct" o "cpy" o "gbo"
            var supervisor = new SelectList(aux1.Where(p => p.tipo == "sup"), "id", "Nom");// Lista que se filtra por el tipo parametro "sup"
            var uninidad = new SelectList(aux1.Where(p => p.tipo == "uni"), "id", "Nom");// Lista que se filtra por el tipo parametro "uni"            
            var usuarios = new SelectList(aux2, "ID", "nom");
            var jefes = new SelectList(aux3, "ID", "nom");
            var carg = new SelectList(aux4, "id", "nom");
            var estado = new SelectList(aux2.Select(p => p.uactivo), "id", "Nom");
           
            ViewBag.ListaGerente = gerente;
            ViewBag.ListaGerencia = gerencia;
            ViewBag.ListaGrupo = grupo;
            ViewBag.ListaSupervisor = supervisor;
            ViewBag.ListaUnidad = uninidad;
            ViewBag.ListaUsu = usuarios;
            ViewBag.ListaCargos = carg;            
            ViewBag.ListaEstado = estado;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantUsu")]
        public ActionResult Create(Usu model, bool estado)
        {
            if (estado == true)
            {
                model.uactivo = "s";
            }
            else
            {
                model.uactivo = "n";
            }

            var ultimo = (from usu in _db.Usu select usu.ID).Max();

            try
            {

                if (model.Perfil == null)
                {
                    model.Perfil = 0;
                }
                if (model.Anexo == null)
                {
                    model.Anexo = 0;
                }
                                    
                model.ID = ultimo + 1;
                model.IP = null;
                model.valpubeeff = "N";
                

                if (ModelState.IsValid)
                {
                    _db.Usu.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception e)
            {
                return JsonError("Hey !!! hubo un problema.");
            }

            return View();
        }

        [ClientAuthorize("MantUsu")]
        public ActionResult Update(string id)
        {
            int id_ex = Int32.Parse(id);

            Usu usuarioEditar = _db.Usu.SingleOrDefault(p => p.ID == id_ex);

            var listaParametros = (from para in _db.parametros
                                   orderby para.nom
                                   select para).ToList();

            var listaUsuarios = (from usu in _db.Usu
                                 orderby usu.Nom
                                 select usu).ToList();

            var listaJefes = (from usu in _db.Usu
                              join usu_jefe in _db.Usu on usu.jefatura equals usu_jefe.ID
                              orderby usu_jefe.Nom
                              select usu_jefe).ToList();

            var listaCargos = (from car in _db.GS_Cargos
                               orderby car.nom
                               select car).ToList();

            var gerente = new SelectList(listaParametros.Where(p => p.tipo == "gg"), "id", "Nom");// Lista que se filtra por el tipo parametro "gg"
            var gerencia = new SelectList(listaParametros.Where(p => p.tipo == "gess" || (p.tipo == "gebo")), "id", "Nom");// Lista que se filtra por el tipo parametro "gess" o "gebo"
            var grupo = new SelectList(listaParametros.Where(p => p.tipo == "ct" || (p.tipo == "cpy") || (p.tipo == "gbo")), "id", "Nom");// Lista que se filtra por el tipo parametro "ct" o "cpy" o "gbo"
            var supervisor = new SelectList(listaParametros.Where(p => p.tipo == "sup"), "id", "Nom");// Lista que se filtra por el tipo parametro "sup"
            var uninidad = new SelectList(listaParametros.Where(p => p.tipo == "uni"), "id", "Nom");// Lista que se filtra por el tipo parametro "uni"            
            var usuarios = new SelectList(listaUsuarios, "ID", "nom");
            var jefes = new SelectList(listaJefes, "ID", "nom");
            var carg = new SelectList(listaCargos, "id", "nom");

            ViewBag.ListaGerente = gerente;
            ViewBag.ListaGerencia = gerencia;
            ViewBag.ListaGrupo = grupo;
            ViewBag.ListaSupervisor = supervisor;
            ViewBag.ListaUnidad = uninidad;
            ViewBag.ListaCargos = carg;
            ViewBag.ListaUsu = usuarios;

            return View("Create", usuarioEditar);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantUsu")]
        public ActionResult Update(Usu model, bool estado)
        {            
            try
            {
                model.uactivo = estado == true ? "s" : "n"; 
                model.valpubeeff = "N";
                model.IP = null;
                model.Perfil = 0;

                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }

                return JsonError("Hey !!! hubo un problema.");
            }
            catch
            {
                return JsonError("Hey !!! hubo un problema.");
            }
        }

        [ClientAuthorize("MantUsu")]
        public ActionResult Delete(string id)
        {
            string aux = "n";
            int id_ex = Int32.Parse(id);
            var model = _db.Usu.FirstOrDefault(p => p.ID == id_ex);

            if (model == null)
            {
                return RedirectToAction("Create");
            }
            try
            {
                model.uactivo = aux;

                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return JsonError("Hey !!! hubo un problema.");
            }

            return View();
        }

        [ClientAuthorize("MantUsu")]
        public ActionResult EmpresasAsignadasGS(string id)
        {
            dynamic modelo = new ExpandoObject();
            int id_ex = Int32.Parse(id);            

            var usuarioFiltrado = (from usuari in _db.Usu
                                   where usuari.ID == id_ex                                   
                                   select new
                                   {
                                       IDUsuario = usuari.ID,
                                       Nombre = usuari.Nom
                                   });

            var listaEmpresaGS = (from cli in _db.Clientes
                                  join per_ in _db.Permisos on cli.ID equals per_.Clie into cli_per

                                  from per in cli_per.Where(x => x.Usu == id_ex).DefaultIfEmpty()
                                  where cli.Activo == "S"
                                  orderby cli.Nom
                                  select new
                                  {
                                      ClieID = cli.ID,
                                      NomArchivo = cli.Nom,
                                      Estado = ((per == null) ? 0 : 1)
                                  });

            var listaSoftl = (from cli in _db.Clientes
                              join perSo in _db.Permisos_Softland on cli.ID equals perSo.Clie into clie_perSo

                              from perSo in clie_perSo.Where(x => x.Usu == id_ex).DefaultIfEmpty()
                              where cli.Activo == "S" && cli.Ruta.Length > 5
                              orderby cli.Nom
                              select new
                              {
                                  ClieID = cli.ID,
                                  NomArchivo = cli.Nom,
                                  Estado = ((perSo == null) ? 0 : 1)
                              });

            var listaApoyo = (from her in _db.GS_HerrApoyo
                              join asi in _db.GS_Asig_HrrAp on her.id equals asi.HrrAp into her_asi

                              from asi in her_asi.Where(x => x.usu == id_ex).DefaultIfEmpty()
                              orderby her.nom
                              select new
                              {
                                  HerID = her.id,
                                  NomHerramienta = her.nom,
                                  Estado = ((asi == null) ? 0 : 1)
                              });

            //modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(usuarioFiltrado);
            //modelo.Softland = WebApplication.App_Start.Helper.listAnonymousToDynamic(listaSoftl);
            //modelo.Apoyo = WebApplication.App_Start.Helper.listAnonymousToDynamic(listaApoyo);

            var listaEmpresaGSSelected = listaEmpresaGS.Where(p => p.Estado == 1).Select(p => p.ClieID).ToList();
            var listasSoftlandSelect = listaSoftl.Where(p => p.Estado == 1).Select(p => p.ClieID).ToList();
            var listaHerramientas = listaApoyo.Where(p => p.Estado == 1).Select(p => p.HerID).ToList();
            modelo.listaEmpresa = new MultiSelectList(listaEmpresaGS, "ClieID", "NomArchivo", listaEmpresaGSSelected);
            modelo.listaSoftland = new MultiSelectList(listaSoftl, "ClieID", "NomArchivo", listasSoftlandSelect);
            modelo.listaHerramientas = new MultiSelectList(listaApoyo, "HerID", "NomHerramienta", listaHerramientas);

            ViewBag.id_usu = id_ex;
            return View("EmpresasAsignadas", modelo);
        }
        [ClientAuthorize("MantUsu")]
        public ActionResult ReinicioPass(string id)
        {
            int id_ex = Int32.Parse(id);
            var usuario_filtradoCompleto = _db.Usu.FirstOrDefault(x => x.ID == id_ex).Nom;
            var usuario_filtradoCorto = _db.Usu.FirstOrDefault(x => x.ID == id_ex).nomususoft.Trim();
            dynamic modelo = new ExpandoObject();
            var listaSoftl = (from cli in _db.Clientes
                              join perSo in _db.Permisos_Softland on cli.ID equals perSo.Clie into clie_perSo

                              from perSo in clie_perSo.Where(x => x.Usu == id_ex).DefaultIfEmpty()
                              where cli.Activo == "S" && cli.Ruta.Length > 5
                              orderby cli.Nom
                              select new
                              {
                                  ClieID = cli.ID,
                                  Nombre = cli.Nom,
                                  NomArchivo = cli.Nombre_Archivo,
                                  Estado = ((perSo == null) ? 0 : 1)
                              });

            var listaSoftlandFiltrrada = listaSoftl.Where(p => p.Estado == 1);
            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(listaSoftlandFiltrrada);

            ViewBag.nombreCompleto = usuario_filtradoCompleto;
            ViewBag.nombreCorto = usuario_filtradoCorto;
            ViewBag.id_usu = id_ex;
            return View("ReinicioPass", modelo);
        }

        [ClientAuthorize("MantUsu")]
        public ActionResult ReinicioPasswordSoftland(string nombreCorto, string id_usu, string[] check)
        {
            try
            {
                int id_ex = Int32.Parse(id_usu);
                List<Int32> empresas_id = new List<Int32>();
                List<String> rutas = new List<String>();
                //var nom_usu = _db.Usu.FirstOrDefault(x => x.ID == id_ex).nomususoft.Trim();

                foreach (var item in check)
                {
                    if (!(item.SequenceEqual("false")))
                    {
                        int id_clie = Int32.Parse(item);
                        empresas_id.Add(id_clie);
                    }
                }

                var cliente_lista = _db.Clientes.Where(p => empresas_id.Contains(p.ID)).ToList();

                foreach (var item in cliente_lista)
                {
                    var extrae_ruta = item.Ruta.Split('\\').Last();
                    rutas.Add(extrae_ruta);
                }

                var rutas_concatenadas = string.Join("|", rutas.ToArray());

                List<SqlParameter> lista_update_password = new List<SqlParameter>()
                {
                        new SqlParameter("@Cliente", System.Data.SqlDbType.NVarChar) { Value = rutas_concatenadas},
                        new SqlParameter("@Usuario", System.Data.SqlDbType.NVarChar) { Value = nombreCorto},
                        new SqlParameter("@counts", System.Data.SqlDbType.Int) {Value = 0 }
                };

                var update_pass = PA_Almacenado(lista_update_password, "PA_updatepassword");

                if (update_pass == rutas.Count())
                {
                    return JsonExito();
                }

            }
            catch (Exception e)
            {
                return JsonError("No se pudo reiniciar la clave");
            }

            return JsonExito();
        }

        [ClientAuthorize("MantUsu")]

        public ActionResult AsignarPermisoGsWeb(int? id)
        {
            dynamic modelo = new ExpandoObject();


            var listaPermisosGs = (from par in _db.Parametros_GS_WEB
                                   join per in _db.Permisos_GS_WEB on par.ID_Parametro equals per.ID_Parametro into grp

                                   from per in grp.Where(x => x.ID_Usu == id).DefaultIfEmpty()
                                   select new
                                   {
                                       id = par.ID_Parametro,
                                       NomParam = par.Descripcion,
                                       Estado = ((per == null) ? 0 : 1)
                                   }).ToList();

            var listaPermisosGSSelected = listaPermisosGs.Where(p => p.Estado == 1).Select(p => p.id).ToList();

            ViewBag.id_usu = id;
            modelo.listadoPermisos = new MultiSelectList(listaPermisosGs, "id", "NomParam", listaPermisosGSSelected);

            return View("AsignarPermisoGsWeb", modelo);

        }
        [ClientAuthorize("MantUsu")]
        [HttpPost]
        public JsonResult AsignarPermisoGs(int[] PermisosGsWeb, int id_usu)
        {
            try
            {

                //creo lista de nuevos permisos
                List<Permisos_GS_WEB> listaPermisos = new List<Permisos_GS_WEB>();

                //verifico permisos actuales en gs
                var modelPermi = _db.Permisos_GS_WEB.Where(p => p.ID_Usu == id_usu).OrderBy(p => p.ID_Parametro).Select(x=> x.ID_Parametro).ToList();

                var permiDelete = modelPermi.Except(PermisosGsWeb.OrderBy(x => x)).FirstOrDefault();
                var permiAdd = PermisosGsWeb.Except(modelPermi).FirstOrDefault();
                if (permiAdd != 0) {

                    listaPermisos.Add(new Permisos_GS_WEB() { ID_Usu = id_usu , ID_Parametro = permiAdd });

                    _db.Permisos_GS_WEB.AddRange(listaPermisos);
                    _db.SaveChanges();

                    return JsonExitoValor("Agregado éxitosamente","1");
                }

                if (permiDelete != 0) {

                    var model = _db.Permisos_GS_WEB.Where(x => x.ID_Parametro == permiDelete && x.ID_Usu == id_usu).ToList();
                    _db.Permisos_GS_WEB.RemoveRange(model);
                    _db.SaveChanges();

                    return JsonExitoValor("Eliminado éxitosamente", "0");

                }

                return Json(new { data = "", JsonRequestBehavior.AllowGet });

            }
            catch (Exception e)
            {
                return JsonError("Ha ocurrido un error " + e);
            }
           

        }


        [ClientAuthorize("MantUsu")]
        public ActionResult AsignarAUsuario(int id_usu, int[] empresa_id, int[] softland_id, int[] herramientas_id)
        {
            try
            {
                var softland = PermisosSoftland(id_usu, softland_id);

                if (softland == "2")
                {
                    return JsonError("No se pueden agregar permisos a un usuario que no existe en la matriz IFRS2013");
                }

                var herramientas = HerramientasApoyo(id_usu, herramientas_id);
                var permisosgs = PermisosGS(id_usu, empresa_id);

                if (permisosgs == "9" && herramientas == "9" && softland == "9")
                {
                    return JsonExitoMsg("No se han realizado coambios.");
                }

                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError("Upss... Un problema al realizar tu solicitud.");
            }
        }

        //Permisos Softland SP
        [ClientAuthorize("MantUsu")]
        public String PermisosSoftland(int id_usu, int[] softland_id)
        {            
            var usuario_validar = _db.Usu.FirstOrDefault(p => p.ID == id_usu).nomususoft.Trim(); //NOS PERMITE OBTENER EL NOMBRE DEL USUARIO SOFTLAND
            var modeloPermisoSoftland = _db.Permisos_Softland.Where(p => p.Usu == id_usu).OrderBy(p => p.Clie);
            try
            {
                if (!modeloPermisoSoftland.Select(p => p.Clie).ToList().SequenceEqual(softland_id.OrderBy(p => p).ToList()))
                {
                    if (!_db.v_UsuarioIFRS2013.Any(p => p.Usuario == usuario_validar))
                    {                        
                        return "2";
                    }
                    var id_clientes_acce_eliminar = modeloPermisoSoftland.Where(p => !softland_id.Contains(p.ID)).Select(q => q.Clie); //Eliminar Eliminar En la tabla GS_Permisos_Softland
                    var id_clientes_acce_agregar = softland_id.Where(p => !modeloPermisoSoftland.Select(q => q.ID).Contains(p));//Agregar A la tabla GS_Permisos_Softland

                    var clientes = _db.Clientes;

                    var clientes_acce_agregar = clientes.Where(p => id_clientes_acce_agregar.Contains(p.ID)).ToList();  
                    var clientes_acce_eliminar = clientes.Where(p => id_clientes_acce_eliminar.Contains(p.ID)).ToList();

                    var sp_agregar = clientes_acce_agregar.Where(p => !clientes_acce_eliminar.Contains(p)).ToList();
                    var sp_eliminar = clientes_acce_eliminar.Where(p => !clientes_acce_agregar.Contains(p)).ToList();

                    if (!(sp_agregar.Count == 0))
                    {
                        var clientes_habilitados = sp_agregar.Select(p => p.Ruta.Split('\\').Last());
                        var rutas_concatenadas = string.Join("|", clientes_habilitados.ToArray());
                        var clientes_habilitados_ = sp_agregar.Select(p => p.ID);
                        var id_concatenados = string.Join("|", clientes_habilitados_.ToArray());

                        var id_usuario = id_usu.ToString();

                        List<SqlParameter> lista_parametros_insert = new List<SqlParameter>() {

                                new SqlParameter("@Cliente_ruta", System.Data.SqlDbType.NVarChar) { Value = rutas_concatenadas},
                                new SqlParameter("@Usuario", System.Data.SqlDbType.NVarChar) { Value = usuario_validar},
                                new SqlParameter("@Cliente_id", System.Data.SqlDbType.NVarChar) { Value = id_concatenados},
                                new SqlParameter("@id_usu", System.Data.SqlDbType.Int) { Value = id_usuario},
                                new SqlParameter("@counts", System.Data.SqlDbType.Int) {Value = 0 }
                        };

                        var cliente_insert = PA_Almacenado(lista_parametros_insert, "PA_permisosGS_Softland");
                        
                    }

                    if(!(sp_eliminar.Count == 0))
                    {
                        var clientes_habilitados = sp_eliminar.Select(p => p.Ruta.Split('\\').Last());
                        var rutas_concatenadas = string.Join("|", clientes_habilitados.ToArray());
                        var clientes_habilitados_ = sp_eliminar.Select(p => p.ID);
                        var id_concatenados = string.Join("|", clientes_habilitados_.ToArray());

                        var id_usuario = id_usu.ToString();
                       
                        List<SqlParameter> ListaUpdateBoqueo = new List<SqlParameter>() {

                                new SqlParameter("@Cliente_ruta", System.Data.SqlDbType.NVarChar) { Value = rutas_concatenadas},
                                new SqlParameter("@Usuario", System.Data.SqlDbType.NVarChar) { Value = usuario_validar},
                                new SqlParameter("@Cliente_id", System.Data.SqlDbType.NVarChar) { Value = id_concatenados},
                                new SqlParameter("@id_usu", System.Data.SqlDbType.Int) { Value = id_usuario},
                                new SqlParameter("@counts", System.Data.SqlDbType.Int) {Value = 0 }
                        };

                        var cliente_delete = PA_Almacenado(ListaUpdateBoqueo, "PA_bloqueoSoftland_GS");
                        
                    }

                    return "1";

                }

                return "9";

            }
            catch (Exception e)
            {
                return "20";
            }
           
        }

        //Permisos de Apoyo Herramientas
        [ClientAuthorize("MantUsu")]
        public String HerramientasApoyo(int id_usu, int[] herramientas_id)
        {
            List<GS_Asig_HrrAp> listaHerramientas = new List<GS_Asig_HrrAp>();
            var modeloHerramientas = _db.GS_Asig_HrrAp.Where(p => p.usu == id_usu).OrderBy(q => q.HrrAp);

            try
            {
                var id_clientes_acce_eliminar = modeloHerramientas.Where(p => !herramientas_id.Contains(p.id)).Select(q => q.HrrAp).Cast<int>().ToList();
                
                if (!id_clientes_acce_eliminar.SequenceEqual(herramientas_id.OrderBy(p => p).ToList()))
                {

                    foreach (var item in herramientas_id)
                    {
                        listaHerramientas.Add(new GS_Asig_HrrAp() { HrrAp = item, usu = id_usu });
                    }
                    var listaAgregar = listaHerramientas.OrderBy(p => p.HrrAp).ToList();

                    _db.GS_Asig_HrrAp.RemoveRange(modeloHerramientas);
                    _db.SaveChanges();
                    _db.GS_Asig_HrrAp.AddRange(listaHerramientas);
                    _db.SaveChanges();

                    return "1";
                }
                return "9";
            }
            catch (Exception e)
            {
                return "20";
            }
        }

        //Ingresar Permisos en GS
        [ClientAuthorize("MantUsu")]
        public String PermisosGS(int id_usu, int[] empresa_id)
        {
            List<Permisos> listaPermisos = new List<Permisos>();
            var modeloPermisosGS = _db.Permisos.Where(p => p.Usu == id_usu).OrderBy(p => p.Clie);//CREADO PARA BORRAR CON UNA LISTA LOS PERMISOS DE UN USUARIO EN GS

            try
            {
                if(!modeloPermisosGS.Select(p => p.Clie).ToList().SequenceEqual(empresa_id.OrderBy(p => p).ToList())) {
                                      
                    foreach (var item in empresa_id)
                    {
                        listaPermisos.Add(new Permisos() { Clie = item, Modulo = "1", Usu = id_usu });//AGREGAR A LA LISTA UNA NUEVA INSTANCIA DE PERMISOS                        
                    }
                    _db.Permisos.RemoveRange(modeloPermisosGS);
                    _db.Permisos.AddRange(listaPermisos);//INSERTAR CON LISTA LOS PERMISOS AL SISTEMA GS
                    _db.SaveChanges();

                    return "1";

                }
                return "9";

            }
            catch (Exception e)
            {
                return "20";
            }
        }
    }
}
