using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace WebApplication.Areas.Tesoreria.Controllers
{
    public class CentralizarPagosController : MasterController
    {
        // GET: CentralizarPagos/CentralizarPagos
        [ClientAuthorize("CentPagos")]
        public ActionResult Index()
        {

            return View();
        }

        [HttpPost]
        [ClientAuthorize("CentPagos")]
        public JsonResult getClientes()
        {
            var query = (from bank in _db.GS_Bancos
                         join pag in _db.GS_Pagos on bank.ID equals pag.Bco
                         join ct in _db.GS_CtasCtes on pag.CtaBco equals ct.ID
                         join docs in _db.Docs on pag.Org equals docs.ID
                         where docs.ObsPagRealizo == null
                         && pag.Comp == null
                         && docs.TipChe == 2
                         group pag by new
                         {
                             num = docs.IDCLIE,
                             cliente = docs.Nombre_Archivo,
                             //docs.Ruta,
                             //docs.EmailPag,
                             docs.TipChe
                         } into g
                         select new
                         {
                             tipChe = g.Key.TipChe,
                             nom_cliente = g.Key.cliente,
                             count = g.Count(),
                             id_cliente = (int)g.Key.num
                         }).ToList();

            return Json(new { data = query, JsonRequestBehavior.AllowGet });
        }
        //[HttpPost]
        public JsonResult getCtas(int? id)
        {
            var data = (from ctas in _db.GS_Ctas
                        join cctas in _db.GS_ClaCtas on ctas.Cta equals cctas.ID
                        where (ctas.Clie == id) && (cctas.Nom.Trim() == "Honorarios" || cctas.Nom.Trim() == "Proveedor")
                        select new { id = ctas.ID, codCta = ctas.CodCta.Trim(), id_clie = ctas.Clie, ctaID = ctas.ID, txtEsp = ctas.Esp.Trim(), txtIng = ctas.Ing.Trim(), tipo = cctas.Nom }).ToList();

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ClientAuthorize("CentPagos")]
        public JsonResult getDataDocum(int? id)
        {
            var query = (from bco in _db.GS_Bancos
                         join pagos in _db.GS_Pagos on bco.ID equals pagos.Bco
                         join ccts in _db.GS_CtasCtes on pagos.CtaBco equals ccts.ID
                         join docs in _db.Docs on pagos.Org equals docs.ID
                         where (docs.ObsPagRealizo == null) && (pagos.Comp == null) && (pagos.Comp == null) && (docs.IDCLIE == id)
                         orderby docs.CodAux, pagos.MPago, pagos.DPago
                         select new
                         {
                             id = pagos.ID,
                             emailPag = docs.EmailPag.Trim(),
                             fecha = pagos.Fecha,
                             monto = pagos.Monto,
                             numDoc = pagos.DPago,
                             numBanco = bco.ID,
                             NomBanco = ccts.DescripCtaCont.Trim(),
                             cuenta = ccts.Cta.Trim(),
                             formaPago = pagos.MPago.Trim(),
                             numcta = docs.Cta.Trim(),
                             idBanco = docs.ID,
                             nomBanco = docs.NomBan.Trim(),
                             nomAux = docs.NomAux.Trim(),
                             codAux = docs.CodAux.Trim(),
                             email = docs.Email.Trim(),
                             xls = pagos.GXls.Trim(),
                             ctdCod = docs.CTDCod.Trim(),
                             tipoCv = docs.TipoCV.Trim(),
                             rotuloDoc = docs.RotuloDoc.Trim()
                         }).ToList();


            return Json(new { data = query }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [ClientAuthorize("CentPagos")]
        public ActionResult centPagos(int? id, string data, string op)
        {
            try
            {
                var cliente = _db.Clientes.FirstOrDefault(x => x.ID == id);
                var nom_bd = cliente.Ruta.Split('\\').Last();

                var listado = JsonConvert.DeserializeObject<List<pagosCent>>(data).ToList();
                List<String> strIDs = new List<String>();

                var nom_usu = SesionLogin().Sigla;

                foreach (var item in listado)
                {
                    strIDs.Add(item.id.ToString());
                    //var Exec_SP = LoadData("exec [pa_CentralizacionPagos] '" + nom_bd + "', '" + i.id + "','"+ SesionLogin().nomcorto +"'");

                }
                var listaIds = string.Join("|", strIDs.ToArray());

                List<SqlParameter> lista_centralizacion_pagos = new List<SqlParameter>()
                {
                        new SqlParameter("@base", System.Data.SqlDbType.NVarChar) { Value = nom_bd},
                        new SqlParameter("@nom_usu", System.Data.SqlDbType.NVarChar) { Value = nom_usu},
                        new SqlParameter("@IDClie", System.Data.SqlDbType.Int) { Value = (int)cliente.ID},
                        new SqlParameter("@ID_Pagos", System.Data.SqlDbType.NVarChar) { Value = listaIds},
                        new SqlParameter("@TipoAsiento", System.Data.SqlDbType.NVarChar) { Value = op},
                        new SqlParameter("@counts", System.Data.SqlDbType.Int) {Value = 0 }
                };

                var numCent = PA_Almacenado(lista_centralizacion_pagos, "PA_CentralizacionPagos");

                return Json(new { data = numCent, JsonRequestBehavior.AllowGet });

            }
            catch (Exception e)
            {
                return JsonError(e.Message);

            }
        }


        class pagosCent
        {
            public string NomBanco { get; set; }
            public string codAux { get; set; }
            public string ctdCod { get; set; }
            public string cuenta { get; set; }
            public string emailPag { get; set; }
            public DateTime fecha { get; set; }
            public string formaPago { get; set; }
            public int id { get; set; }
            public int idBanco { get; set; }
            public double monto { get; set; }
            public string nomAux { get; set; }
            public int numBanco { get; set; }
            public int numDoc { get; set; }
            public string numcta { get; set; }
            public string rotuloDoc { get; set; }
            public string tipoCv { get; set; }
            public string xls { get; set; }

        }
    }
}