using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class ReinicioController : MasterController
    {
        // GET: Reinicio/Reinicio
        [ClientAuthorize("MantRein")]
        public ActionResult Index()
        {
            var id_usu = SesionLogin().ID;
            var clie = (from cli in _db.Clientes
                        join per in _db.Permisos_Softland on cli.ID equals per.Clie
                        where per.Usu == id_usu && cli.Activo == "S"
                        orderby cli.Nom
                        select cli).ToList();


            return View(clie);
        }
        [ClientAuthorize("MantRein")]
        public ActionResult ReinicioPass(int id)
        {
            try
            {
                var idusu = SesionLogin().ID;
                var usuario_filtradoCorto = _db.Usu.FirstOrDefault(x => x.ID == idusu).nomususoft.Trim();

                var filtroCliente = _db.Clientes.FirstOrDefault(p => p.ID == id);
                var extraerRuta = filtroCliente.Ruta.Split('\\').Last();

                List<SqlParameter> cliente_reinicio_pass = new List<SqlParameter>()
                {
                new SqlParameter("@Cliente", System.Data.SqlDbType.NVarChar) { Value = extraerRuta},
                new SqlParameter("@Usuario", System.Data.SqlDbType.NVarChar) { Value = usuario_filtradoCorto},
                new SqlParameter("@counts", System.Data.SqlDbType.Int) {Value = 0 }
                };

                var reinicio_pass = PA_Almacenado(cliente_reinicio_pass, "PA_updatepassword");

                if (reinicio_pass == 1)
                {
                    return JsonExito();
                }
            }
            catch(Exception e)
            {
                return JsonError("No se pudo reiniciar la clave");
            }

            return JsonExito();
        }
        [ClientAuthorize("MantRein")]
        public ActionResult ReinicioPassTodas()
        {
            try
            {
                var idusu = SesionLogin().ID;

                var usuario_softland = _db.Usu.FirstOrDefault(x => x.ID == idusu).nomususoft.Trim();

                var clie = (from cli in _db.Clientes
                            join per in _db.Permisos_Softland on cli.ID equals per.Clie
                            where per.Usu == idusu && cli.Activo == "S"
                            orderby cli.Nom
                            select cli).ToList();

                var clientes_rutas = clie.Select(p => p.Ruta.Split('\\').Last());

                var rutas_concatenadas = string.Join("|", clientes_rutas.ToArray());

                List<SqlParameter> lista_update_password = new List<SqlParameter>()
                {
                        new SqlParameter("@Cliente", System.Data.SqlDbType.NVarChar) { Value = rutas_concatenadas},
                        new SqlParameter("@Usuario", System.Data.SqlDbType.NVarChar) { Value = usuario_softland},
                        new SqlParameter("@counts", System.Data.SqlDbType.Int) {Value = 0 }
                };

                var update_pass = PA_Almacenado(lista_update_password, "PA_updatepassword");

                if (update_pass == clientes_rutas.Count())
                {
                    return JsonExito();
                }
            }
            catch (Exception e)
            {
                return JsonError("No se pudo reiniciar la clave");
            }

            return View();
        }
    }
}