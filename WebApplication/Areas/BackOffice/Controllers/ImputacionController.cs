using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.BackOffice.Controllers
{
    [ClientAuthorize("MantImput")]
    public class ImputacionController : MasterController
    {
        // GET: Imputacion/Imputacion
        public ViewResult Index()
        {
            var CBDoc = new List<Object>();
            CBDoc.Add(new { Value = "AN", Label = "AN" });
            CBDoc.Add(new { Value = "FR", Label = "FR" });
            CBDoc.Add(new { Value = "NC", Label = "NC" });
            //ViewBag.CTDCod = new SelectList(_db.GS_TDoc.OrderBy(p => p.DesDoc), "CodDoc", "DesDoc");
            ViewBag.CTDCod = _db.GS_TDoc.OrderBy(p => p.DesDoc). ToList();
            ViewBag.CBDoc = new SelectList(CBDoc, "Value", "Label");
            return View();
        }

        public JsonResult GetCuentasContables(int Nivel, string Cliente) {
            var result = LoadData("SELECT [PCCODI] , [PCDESC], PCCCOS, PCAUXI, PCCDOC  From [" + Cliente + "].[softland].cwpctas  Where PCNIVEl =" + Nivel.ToString());
            return Json(new { Data = result });
        }

        public JsonResult GetCentrosCosto(string Cliente)
        {
            var result = LoadData("select CodiCC, isnull(DescCC, '') DescCC, NivelCC, ACTIVO from [" + Cliente + "].[softland].cwtccos where NivelCC = (select max(NivelCC)from[" + Cliente + "].softland.cwtccos )  order by NivelCC asc");
            return Json(new { Data = result });
        }

        public JsonResult GetDocumentosParaImputar()
        {
            try {
                var Sigla = SesionLogin().Sigla;
                var JsonRespond = Json(new
                {
                    data = (from d in _db.GS_Documentos
                            join c in _db.Clientes on d.CLIENTE equals c.Rut
                            join p in _db.Permisos on c.ID equals p.Clie
                            join u in _db.Usu on p.Usu equals u.ID
                            join td in _db.GS_TDoc on d.CTDCod equals td.CodDoc
                            where u.Sigla == Sigla &&
                            (td.TipoCV == "C" || td.TipoCV == "R") && d.UsuImp == null && d.UsuEnt != null &&
                            c.Activo == "S"
                            orderby d.ID descending
                            select new {d, c, td }).ToList().Select(p => new
                            {
                                p.d.ID,
                                p.d.CPBMes,
                                p.d.NomAux,
                                p.d.CTDCod,
                                p.d.NumDocI,
                                Neto = p.d.Neto ?? decimal.Zero,
                                Exento = p.d.Exento ?? decimal.Zero,
                                Impu = p.d.Impu ?? decimal.Zero,
                                Total = p.d.Total ?? 0,
                                MovFe = p.d.MovFe?.ToString("dd-MM-yyyy"),
                                MovFv = p.d.MovFv?.ToString("dd-MM-yyyy"),
                                p.c.Nombre_Archivo,
                                p.d.Usu,
                                Entrega = p.d.Entrega?.ToString("dd-MM-yyyy"),
                                p.d.TCambio,
                                NetoSM = p.d.NetoSM?.ToString("N"),
                                ExentoSM = p.d.ExentoSM?.ToString("N"),
                                ImpuSM = p.d.ImpuSM?.ToString("N"),
                                TotalSM = p.d.TotalSM?.ToString("N"),
                                MovRe = p.d.MovRe?.ToString("dd-MM-yyyy"),
                                p.d.id_encab,
                                p.d.MovGlosa,
                                p.d.Archivo,
                                CPBAno = p.d.CPBAño,
                                Nombre_BD = p.c.Ruta.Split('\\').Last(),
                                p.c.IRFS,
                                p.td.DSD
                            })
                });
                JsonRespond.MaxJsonLength = Int32.MaxValue;
                return JsonRespond;
            }
            catch (Exception e)
            {
                return Json(new { data = "" });
            }
        }

        public JsonResult GetTiposDocumento()
        {
            try
            {
                return Json(new
                {
                    Data = (from td in _db.GS_TDoc
                            select td).ToArray()
                });
            }
            catch (Exception e)
            {
                return Json(new { Data = "" });
            }
        }

        public JsonResult GetDetallesImputacionSugerida(int ID)
        {
            try
            {
                var cliente = (from d in _db.GS_Documentos join c in _db.Clientes on d.CLIENTE equals c.Rut where d.ID == ID select c).FirstOrDefault()?.Ruta.Split('\\').LastOrDefault();

                var query = LoadData("select " +
                            "d.ID, " +
                            "dg.ID, " +
                            "det.CodCta, " +
                            "ctas.PCDESC, " +
                            "det.CCCod, " +
                            "ccos.DescCC, " +
                            "td.DSD, " +
                            "ROUND((100 * det.MontoSM) / (dg.NetoSM + dg.ExentoSM + iif(td.DSD = 1, dg.ImpuSM, 0)), 0) porcentajeSM, " +
                            "ROUND((100 * CONVERT(float, det.Monto)) / (dg.Neto + dg.Exento + iif(td.DSD = 1, dg.Impu, 0)), 0) porcentaje, " +
                            "ROUND((((100 * det.MontoSM) / (dg.NetoSM + dg.ExentoSM + iif(td.DSD = 1, dg.ImpuSM, 0))) * (d.NetoSM + d.ExentoSM + iif(td.DSD = 1, d.ImpuSM, 0))) / 100, 0) MontoSM, " +
                            "ROUND((((100 * CONVERT(float, det.Monto)) / (dg.Neto + dg.Exento + iif(td.DSD = 1, dg.Impu, 0))) * (d.Neto + d.Exento + iif(td.DSD = 1, d.Impu, 0))) / 100, 0) Monto, " +
                            "d.Neto + d.Exento + iif(td.DSD = 1, d.Impu, 0), " +
                            "d.NetoSM + d.ExentoSM + iif(td.DSD = 1, d.ImpuSM, 0), " +
                            "det.Aux, " +
                            "det.NDOC, " +
                            "det.TDOC " +
                            "from GS_Documentos d " +
                            "inner join GS_Documentos dg on dg.ID = " +
                            "        (select top 1 ID from GS_Documentos dh where dh.CodAux = d.CodAux and dh.CLIENTE = d.CLIENTE and dh.CTDCod = d.CTDCod and dh.ID < d.ID and dh.UsuImp is not null order by dh.ID desc) " +
                            "inner join GS_Detalles det on dg.ID = det.Doc " +
                            "inner join GS_TDoc td on dg.CTDCod = td.CodDoc and d.CTDCod = td.CodDoc" +
                            "inner join[" + cliente + "].[softland].cwpctas ctas on ctas.PCCODI = det.CodCta COLLATE Modern_Spanish_CI_AS " +
                            "inner join[" + cliente + "].[softland].cwtccos ccos on ccos.CodiCC = det.CCCod COLLATE Modern_Spanish_CI_AS " +
                            "where d.ID = " + ID.ToString() +
                            "and not (det.Monto > (dg.Neto + dg.Exento + iif(td.DSD = 1, dg.Impu, 0)) or det.MontoSM > (dg.NetoSM + dg.ExentoSM + iif(td.DSD = 1, dg.ImpuSM, 0)))");
                return Json(new
                {
                    Data = query
                });
            }
            catch (Exception e)
            {
                return Json(new { Data = "" });
            }
        }

        public async System.Threading.Tasks.Task<JsonResult> EditImpu(GS_Detalles[] Models)
        {
            using (var tr = _db.Database.BeginTransaction()) {
                try
                {
                    var idDoc = Models.First().Doc;
                    GS_Documentos UpdateGSDocumento = _db.GS_Documentos.FirstOrDefault(p => p.ID == idDoc);
                    UpdateGSDocumento.UsuImp = SesionLogin().Sigla;
                    UpdateGSDocumento.Imputacion = DateTime.Now;
                    _db.GS_Documentos.Add(UpdateGSDocumento);
                    _db.Entry(UpdateGSDocumento).State = System.Data.Entity.EntityState.Modified;
                    _db.GS_Detalles.AddRange(Models);
                    await _db.SaveChangesAsync();
                    _db.pa_insCentralizacionDocumentos(idDoc);
                    await _db.SaveChangesAsync();
                    tr.Commit();
                }
                catch (Exception e)
                {
                    tr.Rollback();
                    return JsonError(e.Message);
                }
            }
            return JsonExito();
        }

        public async System.Threading.Tasks.Task<JsonResult> EditDoc(GS_Documentos Model) {
            try
            {
                var modelEdit = _db.GS_Documentos.FirstOrDefault(p => p.ID == Model.ID);
                modelEdit.MovGlosa = Model.MovGlosa;
                modelEdit.CTDCod = Model.CTDCod;
                modelEdit.CPBMes = Model.CPBMes;
                modelEdit.CPBAño = Model.CPBAño;
                modelEdit.MovFv = Model.MovFv;
                _db.GS_Documentos.Add(modelEdit);
                _db.Entry(modelEdit).State = System.Data.Entity.EntityState.Modified;
                await _db.SaveChangesAsync();
                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }
        }

        public async System.Threading.Tasks.Task<JsonResult> Delete(int[] ID)
        {
            try
            {
                _db.GS_Detalles.RemoveRange(_db.GS_Detalles.Where(p => ID.Contains(p.ID)));
                await _db.SaveChangesAsync();
                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }
        }


        [HttpPost]
        public ActionResult Devolver(int[] idGS)
        {
            try
            {
                var x = _db.GS_Documentos.Where(p => idGS.Contains(p.ID)).ToList();
                x.ForEach(p => { p.Entrega = null; p.UsuEnt = null; });

                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError("Ocurrio un error." + e);
            }

        }



    }
}