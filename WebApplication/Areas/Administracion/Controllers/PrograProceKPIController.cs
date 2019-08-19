using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;
using System.Data.SqlClient;
using System.Data.Entity.SqlServer;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class PrograProceKPIController : MasterController
    {
        [ClientAuthorize("MantProgProcKPI")]
        public ActionResult Index()
        {
            var selectTipo = (from tiposPro in _db.GS_Act_Prog
                              where tiposPro.grupo == "KPI"
                              select new
                              {
                                  Id = tiposPro.id,
                                  Nom = tiposPro.nom
                              }
                              ).ToList();

            ViewBag.tipoProce = new SelectList(selectTipo, "Id", "Nom");

            var selectClientes = (from gs_clie in _db.Clientes
                                  where gs_clie.Activo == "S"
                                  orderby gs_clie.Nom
                                  select new
                                  {
                                      Id = gs_clie.ID,
                                      Nom = gs_clie.Nom
                                  }
                             ).ToList();
            ViewBag.lstClientes = new SelectList(selectClientes, "Id", "Nom");

            return View();
        }
        [ClientAuthorize("MantProgProcKPI")]
        public JsonResult GetDates(int idTipo)
        {
            try
            {
                var registros = (from gs_cli in _db.Clientes
                                 join gs_regis in _db.GS_ctrl_cump
                                 on gs_cli.ID equals gs_regis.clie
                                 where gs_cli.Activo == "S" &&
                                 gs_regis.tipo == idTipo &&
                                 gs_regis.clasif == "P"
                                 //(gs_regis.periodo >= inicioMes && gs_regis.periodo <= finMes)
                                 select new
                                 {
                                     IdEvent = gs_regis.id,
                                     RutClie = gs_cli.Rut,
                                     NomClie = gs_cli.Nombre_Archivo,
                                     NomCortClie = gs_cli.Nom,
                                     Periodo = gs_regis.periodo,
                                     DiasPlazo = gs_regis.dias_plazo
                                 }
                             ).ToList();



                return Json(new { data = registros }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = "" });
            }
        }

        [ClientAuthorize("MantProgProcKPI")]
        public ActionResult Create(DateTime date, string tipoPro)
        {
            //recibir el tipo de proceso para no volver a seleccionar.-
            try
            {
                var selectClientes = (from gs_clie in _db.Clientes
                                      where gs_clie.Activo == "S"
                                      orderby gs_clie.Nom
                                      select new
                                      {
                                          Id = gs_clie.ID,
                                          Nom = gs_clie.Nom
                                      }
                            ).ToList();
                ViewBag.lstClientes = new SelectList(selectClientes, "Id", "Nom");

                var selectTipo = (from tiposPro in _db.GS_Act_Prog
                                  where tiposPro.grupo == "KPI"
                                  select new
                                  {
                                      Id = tiposPro.id,
                                      Nom = tiposPro.nom
                                  }
                                  ).ToList();

                ViewBag.lstTipoProce = new SelectList(selectTipo, "Id", "Nom");

                //var periodo = DateTime.Parse(date);

                ViewBag.periodoView = date.ToString("yyyy-MM");
                ViewBag.periodo = date;
                ViewBag.tipo = tipoPro;

                return View();
            }
            catch (Exception)
            {
                throw;
            }

        }

        [ClientAuthorize("MantProgProcKPI")]
        [HttpPost]
        public ActionResult Create(GS_ctrl_cump model,DateTime diasPlazo)
        {
            try
            {
                var existRegistro = _db.GS_ctrl_cump.Where(p => (p.periodo == null ? 0 : ((DateTime)p.periodo).Year) == (model.periodo == null ? 0 : ((DateTime)model.periodo).Year) &&
                (p.periodo == null ? 0 : ((DateTime)p.periodo).Month) == (model.periodo == null ? 0 : ((DateTime)model.periodo).Month) && p.clie == model.clie && p.clasif == "P" &&
                p.tipo == model.tipo).ToArray();

                _db.GS_ctrl_cump.RemoveRange(existRegistro);

                double dias_plazo = diasPlazo.Subtract((DateTime)model.periodo).TotalDays;
                model.dias_plazo = (int)dias_plazo;
                model.clasif = "P";
                model.mom = DateTime.Now;
                model.usu = SesionLogin().Sigla;

                _db.GS_ctrl_cump.Add(model);
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }
        [ClientAuthorize("MantProgProcKPI")]
        public ActionResult Edit(string id)
        {
            try
            {
                var selectClientes = (from gs_clie in _db.Clientes
                                      where gs_clie.Activo == "S"
                                      orderby gs_clie.Nom
                                      select new
                                      {
                                          Id = gs_clie.ID,
                                          Nom = gs_clie.Nom
                                      }
                            ).ToList();
                ViewBag.lstClientes = new SelectList(selectClientes, "Id", "Nom");

                var selectTipo = (from tiposPro in _db.GS_Act_Prog
                                  where tiposPro.grupo == "KPI"
                                  select new
                                  {
                                      Id = tiposPro.id,
                                      Nom = tiposPro.nom
                                  }
                              ).ToList();
                int idCump;
                Int32.TryParse(id, out idCump);
                var gs_ctrlCump = _db.GS_ctrl_cump.FirstOrDefault(x => x.id == idCump);


                ViewBag.lstTipoProce = new SelectList(selectTipo, "Id", "Nom");
                DateTime diasPlazo = (DateTime)gs_ctrlCump.periodo;

                var periodo = (DateTime)gs_ctrlCump.periodo;

                ViewBag.diasPlazo = diasPlazo.AddDays((int)gs_ctrlCump.dias_plazo).ToString("yyyy-MM-dd");
                ViewBag.periodoView = periodo.ToString("yyy-MM");
                ViewBag.periodo = periodo;

                return PartialView("Create", gs_ctrlCump);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [ClientAuthorize("MantProgProcKPI")]
        [HttpPost]
        public ActionResult Edit(GS_ctrl_cump model, DateTime diasPlazo)
        {

            try
            {

                var modelCump = _db.GS_ctrl_cump.FirstOrDefault(x => x.id == model.id);
                if (model.tipo != null && model.dias_plazo != null)
                {
                    modelCump.tipo = model.tipo;
                    modelCump.dias_plazo = model.dias_plazo;
                }
                else
                {
                    modelCump.periodo = model.periodo;
                }
                double dias_plazo = diasPlazo.Subtract((DateTime)model.periodo).TotalDays;
                modelCump.dias_plazo = (int)dias_plazo;
                modelCump.mom = DateTime.Now;
                modelCump.usu = SesionLogin().Sigla;
                _db.Entry(modelCump).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception ex)
            {

                return JsonError(ex.Message);
            }
        }

        [ClientAuthorize("MantProgProcKPI")]
        public JsonResult Delete(string id)
        {
            try
            {
                int idCump;
                Int32.TryParse(id, out idCump);

                var modelCump = _db.GS_ctrl_cump.FirstOrDefault(x => x.id == idCump);

                _db.Entry(modelCump).State = System.Data.Entity.EntityState.Deleted;
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }
        [ClientAuthorize("MantProgProcKPI")]
        public JsonResult PeriodoMesDelCliente(int tipo, string date, int? cliente)
        {
            try
            {
                if (cliente == null)
                {
                    return Json(new { data = "client-null" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var datete = DateTime.Parse(date);
                    var existRegistro = _db.GS_ctrl_cump.Where(p => (p.periodo == null ? 0 : ((DateTime)p.periodo).Year) == (datete == null ? 0 : ((DateTime)datete).Year) &&
                        (p.periodo == null ? 0 : ((DateTime)p.periodo).Month) == (datete == null ? 0 : ((DateTime)datete).Month) && p.clie == cliente && p.clasif == "P" &&
                        p.tipo == tipo).ToArray();


                    if (existRegistro.Count() != 0)
                    {
                        return Json(new { data = existRegistro }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
                    }
                }

            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }
        }
    }
}

