using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class ComprobanteController : MasterController
    {
        // GET: Comprobante/Comprobante
        public ActionResult Index()
        {
            var id_usu = SesionLogin().ID;
            return View();
        }

        public ActionResult BusquedaComprobantes(int[] cliente_id, DateTime fec)
        {
            var fec_format = fec;
            var query = " SELECT CpbNum,cpbest,CpbFec FROM .[SOFTLAND].[cwcpbte] WHERE CpbEst = 'p' AND CpbFec >= " + fec;
            var clientes = _db.Clientes.ToList();
            var clientes_filtrados = clientes.Where(p => cliente_id.Contains(p.ID)).ToList();
            var clientes_habilitados = clientes_filtrados.Select(p => p.Ruta.Split('\\').Last());
            var rutas_concatenadas = string.Join("|", clientes_habilitados.ToArray());
            var Exec_SP = LoadData("exec SP_ExecuteQuery '" + rutas_concatenadas + "', '" + query + "'");
            return Json(new { data = Exec_SP }, JsonRequestBehavior.AllowGet);
            
        }

        public JsonResult ComprobantesPendientes()
        {
            var fec_inicio = DateTime.Now.ToString("yyyy-dd-MM");

            var fec_termino = DateTime.Now.AddMonths(-3).ToString("yyyy-dd-MM");

            var clientes_filtrados = (from clie in _db.Clientes where clie.Activo == "S" && clie.Ruta.Length > 5 && clie.Ruta.Contains(@"W:") select clie).ToList();

            var clientes_habilitados = clientes_filtrados.Select(p => p.Ruta.Split('\\').Last());
            var rutas_concatenadas = string.Join("|", clientes_habilitados.ToArray());
            var query = "SELECT cpbnum,CONVERT(varchar, CpbFec,106) As Fecha,CONVERT(varchar, FechaUlMod,0) As Modificacion,Usuario,(CASE when cpbTip = ''I'' then ''Ingreso'' when cpbtip = ''E'' then ''Egreso'' else ''Traspaso'' end) AS Tipo from .softland.CWCPBTE where cpbest = ''P'' and cpbfec between ''" + fec_termino + "'' and ''" + fec_inicio + "''";

            var Exec_SP = LoadData("exec SP_ExecuteQuery '" + rutas_concatenadas + "', '" + query + "' WITH RECOMPILE ");
            return Json(new { data = Exec_SP }, JsonRequestBehavior.AllowGet);
        }
    }
}