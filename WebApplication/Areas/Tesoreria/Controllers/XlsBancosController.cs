using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Tesoreria.Controllers
{
    public class XlsBancosController : MasterController
    {
        // GET: XlsBancos/XlsBancos
        [ClientAuthorize("XlsBancos")]
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [ClientAuthorize("XlsBancos")]
        public JsonResult getClientes() {

            var clie = (from cctas in _db.GS_CtasCtes
                        join p in _db.GS_Pagos on cctas.ID equals p.CtaBco
                        join b in _db.GS_Bancos on p.Bco equals b.ID
                        join d in _db.Docs on p.Org equals d.ID
                        where d.ObsPagRealizo == null && p.Comp == null
                        group p by new
                        {
                            nom = d.Nombre_Archivo,
                            id_clie = d.IDCLIE,
                            mpago = p.MPago
                        } into g
                        where g.Key.mpago == "TR"
                        select new
                        {
                            conteo = g.Count(),
                            nom_cliente = g.Key.nom,
                            id_cliente = (int)g.Key.id_clie
                        }
                      ).ToList();

            return Json(new { data = clie , JsonRequestBehavior.AllowGet });

        }
        [ClientAuthorize("XlsBancos")]
        [HttpPost]
        public JsonResult getData(int? id, int? idrep)
        {
            try
            {
                if (idrep == 0)
                {
                    var query = (from cctas in _db.GS_CtasCtes
                                 join p in _db.GS_Pagos on cctas.ID equals p.CtaBco
                                 join b in _db.GS_Bancos on p.Bco equals b.ID
                                 join d in _db.Docs on p.Org equals d.ID
                                 where d.ObsPagRealizo == null && p.Comp == null && p.MPago == "TR"
                                 && d.IDCLIE == id

                                 select new
                                 {
                                     id = p.ID,
                                     fecha = p.Fecha,
                                     monto = p.Monto,
                                     numDoc = p.DPago,
                                     nomBanco = cctas.DescripCtaCont,
                                     cuenta = cctas.Cta,
                                     mpago = p.MPago,
                                     cta = d.Cta, //cuenta proveedor
                                     codBanco = cctas.Banco,
                                     nomAux = d.NomAux,
                                     codAux = d.CodAux, //Rut
                                     xls = p.GXls,
                                     glosa = d.CabGlo,
                                     total = d.Total,
                                     moneda = cctas.Moneda,
                                     correo = d.Email
                                 }
                           ).ToList();
                    return Json(new { data = query, JsonRequestBehavior.AllowGet });
                }
                if (idrep == 1)
                {
                    var query = (from cctas in _db.GS_CtasCtes
                                 join p in _db.GS_Pagos on cctas.ID equals p.CtaBco
                                 join b in _db.GS_Bancos on p.Bco equals b.ID
                                 join d in _db.Docs on p.Org equals d.ID
                                 where d.ObsPagRealizo == null && p.Comp == null && p.MPago == "TR"
                                 && d.IDCLIE == id
                                 //group p by new {
                                 //    d.CodAux, d.NomAux , cctas , d.Rut
                                 //} into g
                                 group new { d, p , cctas} by new { d.CodAux } into g
                                 select new 
                                 {
                                    
                                     fecha = g.Max(c => c.p.Fecha),
                                     monto = g.Sum(c => c.p.Monto),
                                     numDoc = g.Max(c => c.p.DPago),
                                     nomBanco = g.Max(c => c.cctas.DescripCtaCont),
                                     cuenta = g.Max(c=>c.cctas.Cta),
                                     mpago = g.Max(c=> c.p.MPago),
                                     cta = g.Max(c=> c.d.Cta), //cuenta proveedor
                                     codBanco = g.Max(c=>c.cctas.Banco),
                                     nomAux = g.Max(c=> c.d.NomAux),
                                     codAux = g.Key.CodAux,
                                     glosa = g.Max(c=> c.d.CabGlo),
                                     moneda = g.Max(c=> c.cctas.Moneda),
                                     correo = g.Max(c=> c.d.Email)

                                 }
                              
                           ).ToList();

                    return Json(new { data = query, JsonRequestBehavior.AllowGet });

                } else {
                    return Json(new { data = "", JsonRequestBehavior.AllowGet });
                }
            }

            catch (Exception e)
            {
                // MvcApplication.LogError(e);
                return JsonError(e.Message);
            }

        }
    }
}