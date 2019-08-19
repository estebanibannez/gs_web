using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Tesoreria.Controllers
{
    public class GestionPagosController : MasterController
    {
        [ClientAuthorize("MantGestPag")]
        public ActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception e)
            {
                return JsonError("Error: " + e.Message);
            }
        }

        [ClientAuthorize("MantGestPag")]
        public JsonResult CargaClientes()
        {
            try
            {
                var selectClientes = (from i in _db.InfGestionPago
                                      group i by new
                                      {
                                          i.NombreCorto,
                                          i.IDCLIE
                                      } into igp
                                      select new
                                      {
                                          IdCli = igp.Key.IDCLIE,
                                          NomCorto = igp.Key.NombreCorto,
                                          CountId = igp.Count(),
                                          countFecLim = igp.Count(m => m.PagUrg != null)
                                      }).ToList();
                return Json(new { data = selectClientes }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [ClientAuthorize("MantGestPag")]
        public ActionResult Edit(int id,int? org)
        {
            try
            {
                if (org == null)
                {
                    org = 0;
                }
                var fechaPag = DiaHabilPago();
                var codPag = fechaPag.ToString("ddMM");
                var selectCuentas = (
                        from ctasCtes_gs in _db.GS_CtasCtes
                        join salBan_gs in _db.GS_Saldos_Bancarios
                        on ctasCtes_gs.ID equals salBan_gs.Cta into ctaSal
                        from cta_Sal in ctaSal.DefaultIfEmpty()
                        join mon_gs in _db.Monedas
                        on ctasCtes_gs.Moneda equals mon_gs.Id into ctaMon
                        from cta_Mon in ctaMon.DefaultIfEmpty()
                        join banc_gs in _db.GS_Bancos
                        on ctasCtes_gs.Banco equals banc_gs.ID into ctaBanc
                        from cta_Banc in ctaBanc.DefaultIfEmpty()

                        where ctasCtes_gs.Cliente == id && cta_Mon.Sigla == ("CLP")
                        
                        orderby ctasCtes_gs.Principal
                        select new
                        {
                            IdCta = ctasCtes_gs.ID,
                            DescCta = ctasCtes_gs.DescripCtaCont.Trim(),
                            SiglaMone = cta_Mon.Sigla,
                            IdBanc = cta_Banc.ID,
                            NumCta = ctasCtes_gs.Cta.Trim(),
                            MontoTotSal = ctaSal.Sum(x => x.Monto) == null ? "Sin Monto" : SqlFunctions.StringConvert((double)ctaSal.Sum(x => x.Monto)),
                            MaxFechaSal = ctaSal.Max(x => x.Fecha)

                        }
                    ).ToList();

                List<SelectListItem> listaCuentas = new List<SelectListItem>();
                foreach (var item in selectCuentas)
                {
                    listaCuentas.Add(new SelectListItem() { Text = item.DescCta.ToString()+ ":  *Moneda=[" + item.SiglaMone.ToString() + "]  *N° de Cta=["+ item.NumCta.ToString() + "]  *Monto=["+item.MontoTotSal+"]", Value = item.NumCta.ToString() });
                };
                var listadoCuentas = new SelectList(listaCuentas, "Value", "Text");
                
                
                var selectPago= (
                    from pagos_gs in _db.GS_Pagos
                    join bancos_gs in _db.GS_Bancos 
                    on pagos_gs.Bco equals bancos_gs.ID into pagBan
                    from pag_Ban in pagBan.DefaultIfEmpty()
                    join ctasCtes_gs in _db.GS_CtasCtes
                    on pagos_gs.CtaBco equals ctasCtes_gs.ID into pagCta
                    from pag_Cta in pagCta.DefaultIfEmpty()
                    where pagos_gs.Org == org
                    orderby pagos_gs.Fecha
                    select new
                    {
                        IDPag = pagos_gs.ID,
                        FechaPag = pagos_gs.Fecha,
                        MontoPag = pagos_gs.Monto,
                        NumCta = pag_Cta.Cta,
                        NomBanc = pag_Ban.Nom,
                        MetodoPag = pagos_gs.MPago,
                        DPag = pagos_gs.DPago,
                        IdCta = pag_Cta.ID,
                        IdBanc = pag_Ban.ID,
                        CompPag = pagos_gs.Comp,
                        OkTes = pagos_gs.OkTeso
                    }
                    ).FirstOrDefault();
                return Json(new { dataPag = selectPago, dataCta = listadoCuentas,dataFec = fechaPag,dataCodPag = codPag,datosCuenta = selectCuentas }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { dataPag = "", dataCta = "", dataFec = "", dataCodPag = "", datosCuenta = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ClientAuthorize("MantGestPag")]
        public JsonResult Editar(GS_Pagos model,string numcent,int clie,int? idpag)
        {
            try
            {
                var comp = "";
                if (model.Fecha == null || model.Monto == null || model.MPago ==null ||
                    model.Bco == null || model.CtaBco == null || model.DPago == null)
                {
                    return JsonError("No pueden haber campos sin llenar. Revise el formulario");
                }
                else
                {
                    
                    var pago_gs = _db.GS_Pagos.FirstOrDefault(x => x.ID == idpag);
                    if (pago_gs == null)
                    {
                       comp = "";
                    }
                    else
                    {
                        comp = pago_gs.Comp.ToString();
                    }


                    var cpbEstado = LoadData("exec [sp_selEstadoComprobante] " + clie + ",'" + numcent + "'").FirstOrDefault();

                    if (cpbEstado == null)
                    {
                        return JsonError("El comprobante de este pago no existe. No se puede continuar");
                    }
                    else
                    {
                        var estado = cpbEstado.First().Value.ToString();
                        if (estado == "P")
                        {
                            return JsonError("El comprobante esta pendiente. No se puede continuar");
                        }
                        else
                        {
                            if (idpag == 0)
                            {
                                model.UsuPag = SesionLogin().Sigla;
                                model.NowSol = DateTime.Now;
                                _db.Entry(model).State = System.Data.Entity.EntityState.Added;
                                _db.SaveChanges();
                            }
                            else if (idpag != 0 && comp == "")
                            {
                                pago_gs.Fecha = model.Fecha;
                                pago_gs.Monto = model.Monto;
                                pago_gs.MPago = model.MPago;
                                pago_gs.Bco = model.Bco;
                                pago_gs.CtaBco = model.CtaBco;
                                pago_gs.DPago = model.DPago;
                                pago_gs.UsuPag = SesionLogin().Sigla;
                                pago_gs.NowSol = DateTime.Now;
                                _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                                _db.SaveChanges();
                            }
                            else if (comp != "")
                            {
                                return JsonError("No se puede modificar el documento porque ya esta centralizado");
                            }
                            return JsonExito();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return JsonError("Error: " + e.Message);
            }
        }

        [HttpPost]
        [ClientAuthorize("MantGestPag")]
        public JsonResult DevolverPago(int id)
        {
            try
            {
                var inGest_gs = _db.InfGestionPago.FirstOrDefault(x => x.ID == id);

                if (inGest_gs.MontoPago != inGest_gs.Saldo)
                {
                    return JsonError("Este documento no puede ser devuelto porque tiene pago(s) en curso ");
                }
                else
                {
                    var docs_gs = _db.GS_Documentos.FirstOrDefault(p => p.ID == id);
                    docs_gs.FechaSolPago = null;
                    docs_gs.FecRec = DateTime.Now;
                    docs_gs.UsuRec = SesionLogin().Sigla;

                    _db.Entry(docs_gs).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
            }
            catch (Exception e)
            {
                return JsonError("Error: " + e.Message);
            }
        }

        [HttpPost]
        [ClientAuthorize("MantGestPag")]
        public JsonResult Solicitudes(int id)
        {
            try
            {
                var selectSoli = (from i in _db.InfGestionPago
                                  where i.IDCLIE == id
                                  select i).ToList();
                return Json(new { data = selectSoli }, JsonRequestBehavior.AllowGet);
            }
            catch
            {

                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [ClientAuthorize("MantGestPag")]
        public JsonResult DatosCliente(int id)
        {
            try
            {
                var selectClient = (from i in _db.InfGestionPago
                                      group i by new
                                      {
                                          i.NombreCorto,
                                          i.Ruta,
                                          i.EmailPag,
                                          i.nom,
                                          i.Obs,
                                          i.IDCLIE
                                      } into igp
                                      where igp.Key.IDCLIE == id
                                      select new
                                      {
                                          Nom = igp.Key.NombreCorto,
                                          Ruta = igp.Key.Ruta,
                                          Email = igp.Key.EmailPag,
                                          Medio = igp.Key.nom,
                                          Restricc = igp.Key.Obs
                                      }).FirstOrDefault();

                return Json(new { data = selectClient }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { data="" }, JsonRequestBehavior.AllowGet);
            }
        }

        [ClientAuthorize("MantGestPag")]
        public ActionResult Pdf(int id)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                var documento = _db.GS_Documentos.FirstOrDefault(p => p.ID == id);
                if (System.IO.Directory.Exists(documento.Archivo))
                {
                    var docum = (from cab_ant in _db.cab_anticipos
                                 join file_ant in _db.files_anticipos
                                 on cab_ant.id_anticipo equals file_ant.id_anticipo into cab_file
                                 from cabfile in cab_file.DefaultIfEmpty()
                                 where cab_ant.NumDoc == id
                                 select cabfile
                                ).FirstOrDefault();
                    //var url = @"C:\pruebas\";//----------creacion de carpeta prueba con pdf del local
                    //var fileName = Path.GetFileNameWithoutExtension(docum.ruta);
                    //var ext = Path.GetExtension(docum.ruta);
                    //var Doc_file = url + fileName + ext;
                    var Doc_file = System.Web.Hosting.HostingEnvironment.MapPath(docum.ruta);//Ruta del pdf en el servidor
                    using (FileStream file = new FileStream(Doc_file, FileMode.Open, FileAccess.Read))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, (int)file.Length);
                        ms.Write(bytes, 0, (int)file.Length);
                    }
                    return File(ms.ToArray(), MimeMapping.GetMimeMapping(System.Web.Hosting.HostingEnvironment.MapPath(docum.ruta)));
                }
                else
                {
                    Response.AppendHeader("Content-Disposition", "inline; filename=" + documento.Archivo);
                    //var Doc_file = (documento.Archivo).Replace(@"P:\", @"P:\");
                    var Doc_file = (documento.Archivo).Replace(@"P:\", @"\\atenea\puentesur\"); //Replace (Ubicaion de los archivos en el servidor disco P:\\atenea\puentesur\).                       
                    using (FileStream file = new FileStream(Doc_file, FileMode.Open, FileAccess.Read))
                    {
                        byte[] bytes = new byte[file.Length];
                        file.Read(bytes, 0, (int)file.Length);
                        ms.Write(bytes, 0, (int)file.Length);
                    }
                    return File(ms.ToArray(), MimeMapping.GetMimeMapping(documento.Archivo));
                }
            }
            catch (Exception e)
            {
                return JsonError("Error: " + e.Message);
            }
        }

        private DateTime DiaHabilPago()
        {
            DateTime fecha = DateTime.Today;

            string diaAct = DateTime.Now.ToString("dddd");

            DateTime horaAct = DateTime.Parse(DateTime.Now.ToString("HH:mm:ss"));
            DateTime horaHabil = DateTime.Parse("14:00:00");
            
            if (horaAct > horaHabil && diaAct=="viernes")
            {
                fecha = fecha.AddDays(3);
            }
            else if (horaAct > horaHabil)
            {
                fecha = fecha.AddDays(1);
            }

            return fecha;
        }
    }
}
