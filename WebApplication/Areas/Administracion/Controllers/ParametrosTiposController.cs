using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class ParametrosTiposController : MasterController
    {
        // GET: ParametrosTipos/ParametrosTipos
        [ClientAuthorize("MantPara")]
        public ActionResult Index()
        {

            return View();
        }

        public ActionResult CreateCargos() {

            return View();

        }
        public ActionResult CreateUnidades()
        {

            return View();

        }

        public ActionResult CreateProcesos()
        {

            return View();
        }
        public ActionResult CreateCuentas()
        {

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantPara")]
        public ActionResult CreateCargos(GS_Cargos model)
        {
            try {
                     if (ModelState.IsValid)
                {
                    model.estado = 1;
                    model.usuario_mod = SesionLogin().Sigla;
                    _db.GS_Cargos.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception e) {


                return JsonError("");
            }

            return JsonExito();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantPara")]
        public ActionResult CreateUnidades(parametros model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.tipo = "uni";
                    //model.usuario_mod = SesionLogin().Sigla;
                    _db.parametros.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception e)
            {


                return JsonError("");
            }

            return JsonExito();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantPara")]
        public ActionResult CreateProcesos(GS_Procesos model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //model.estado = 1;
                    //model.usuario_mod = SesionLogin().Sigla;
                    _db.GS_Procesos.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception e)
            {


                return JsonError("");
            }

            return JsonExito();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantPara")]
        public ActionResult CreateCuentas(GS_ClaCtas model, bool vcta)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.vcta = vcta == true ? "S" : "N";
                    _db.GS_ClaCtas.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception e)
            {


                return JsonError("");
            }

            return JsonExito();
        }


        public ActionResult eliminaCargos(int id)
        {
            try
            {
                ////int id_reg = Int32.Parse(id);
                var model = _db.GS_Cargos.FirstOrDefault(x => x.id == id);
                model.usuario_mod = SesionLogin().Sigla;
                model.estado = 0;
                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
            }
            catch (Exception ex)
            {
                ErrorService.LogError(ex);
                return JsonError("Error");
            }

            return View();
        }



        public JsonResult dt_Cargos()
        {
            try
            {
      
                var Cargos = _db.GS_Cargos.Where(x => x.estado == 1).ToArray();

                return Json(new { data = Cargos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return JsonError("");
            }
        }

        public JsonResult dt_Unidades() {

            try
            {
                var unidades = _db.parametros.Where(x => x.tipo == "uni").ToArray();

                return Json(new { data = unidades }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e) {

                return JsonError("");
            }

        }

        public JsonResult dt_procesos()
        {

            try
            {
                var procesos = _db.GS_Procesos.OrderByDescending(x => x.Nom);
                return Json(new { data = procesos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return JsonError("");
            }
           
        }

        public JsonResult dt_tiposCuentas() {
            try
            {
                var tiposCuentas = _db.GS_ClaCtas.OrderByDescending(x => x.Nom).ToList();
                return Json(new { data = tiposCuentas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return JsonError("");
            }
        }
    }
}