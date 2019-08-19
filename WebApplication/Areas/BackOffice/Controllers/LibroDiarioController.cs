using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.BackOffice.Controllers
{
    public class LibroDiarioController : MasterController
    {
        // GET: LibroDiario/LibroDiario

        [ClientAuthorize("LibroDiario")]
        public ActionResult Index()
        {
            //var clientes = _db.Clientes.Where(e => e.Activo == "S" && e.Ruta.Length > 10).Select(e => e.Ruta).ToList();
            var usuSesion = SesionLogin().ID;
            ViewBag.Clientes = (from clie in _db.Clientes
                                join per in _db.Permisos on clie.ID equals per.Clie
                                where clie.Activo == "S"
                                && per.Usu == usuSesion
                                select new { ID = clie.ID, Nom = clie.Nombre_Archivo }
                              ).OrderBy(x => x.Nom);

           
            return View();
        }


        [HttpPost]
        [ClientAuthorize("LibroDiario")]
        public JsonResult getLibro(int? id, int? ano, string mes)
        {

            try
            {
                if (id.HasValue)
                {
                    //Valido que el usuario tenga permiso para ver esta empresa 
                    if (SesionClientesPermi().Any( z => z.ID== id) == false) { return JsonError("Ocurrio un error."); }

                    var rutCliente = _db.Clientes.Where(e => e.ID == id).Select(e => e.Rut).First();
                    var ruta = _db.Clientes.Where(e => e.ID == id).Select(e => e.Ruta).FirstOrDefault();
                    var BD = ruta.Split('\\').Last();

                    string a_emplazar = "AND (cwcpbte.CpbMes = '" + mes + "' )";

                    string str = "SELECT cwcpbte.CpbAno, cwcpbte.CpbNum, cwcpbte.CpbFec, cwcpbte.CpbMes, cwcpbte.CpbEst, cwcpbte.CpbTip, " +
                            "cwcpbte.CpbGlo, cwcpbte.Usuario, cwmovim.PctCod, cwmovim.CcCod, cwmovim.TipDocCb, cwmovim.NumDocCb, cwmovim.CodAux, cwmovim.TtdCod,  cwpctas.PCDESC, " +
                            "cwmovim.NumDoc, cwmovim.MovFe, cwmovim.MovFv, cwmovim.MovTipDocRef, cwmovim.MovNumDocRef, cwmovim.MovDebe, cwmovim.MovHaber, cwmovim.MovGlosa  " +
                            "FROM [" + BD + "].[softland].cwcpbte LEFT JOIN [" + BD + "].[softland].cwmovim ON(cwcpbte.CpbNum = cwmovim.CpbNum) AND(cwcpbte.CpbAno = cwmovim.CpbAno) " +
                            " LEFT JOIN [" + BD + "].[softland].cwpctas on cwmovim.PctCod = cwpctas.PCCODI   " +
                            "WHERE (cwcpbte.CpbAno = " + ano + ") ";
                    if ((mes != "0") && (mes != string.Empty))
                    {
                        str += a_emplazar;
                        var datos = LoadData(str);


                        var JsonReturn = Json(new { data = datos.ToList() }, JsonRequestBehavior.AllowGet);
                        JsonReturn.MaxJsonLength = Int32.MaxValue;
                        return JsonReturn;
                    }
                    else
                    {
                        var datos = LoadData(str);
                        var JsonReturn = Json(new { data = datos.ToList() }, JsonRequestBehavior.AllowGet);
                        JsonReturn.MaxJsonLength = Int32.MaxValue;
                        return JsonReturn;
                    }
                }
                else
                {
                    return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return JsonError("Ocurrio un error." + e);
            }
        }
    }
}
