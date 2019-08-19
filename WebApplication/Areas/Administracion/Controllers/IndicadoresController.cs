using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class IndicadoresController : MasterController
    {
        [ClientAuthorize("MantIndi")]
        public ActionResult Index()
        {
            var indicadores = (from ind in _db.indicadores                               
                               orderby ind.FECHA descending
                               select ind).ToList();

            var id_usu = SesionLogin().ID;
            return View("index",indicadores);
        }
        [ClientAuthorize("MantIndi")]
        public ActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantIndi")]
        public ActionResult Create(indicadores model)
        {
            try
            {
                var idusu = SesionLogin().ID;
                                
                model.usuario_mod = SesionLogin().Sigla;
                model.fecha_mod = DateTime.Now;
                if(model.UF == null)
                {
                    model.UF = (decimal)0.000;
                }
                if (model.IVP == null)
                {
                    model.IVP = (decimal)0.000;
                }
                if (model.DOLAR == null)
                {
                    model.DOLAR = (decimal)0.000;
                }
                if (model.EURO == null)
                {
                    model.EURO = (decimal)0.000;
                }
                if (model.ITCM == null)
                {
                    model.ITCM = (decimal)0.000;
                }
                if (ModelState.IsValid)
                {
                    _db.indicadores.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception ex)
            {
                var innerMessage = (ex.InnerException != null) ? ex.InnerException.HResult : 0;

                if(!(innerMessage == 0))
                {
                    return JsonError("La fecha del indicador ya esta creada, puede editar la que ya existe.");
                }

                return JsonError("Hey !!! hubo un problema.");
            }

            return View();
        }
        [ClientAuthorize("MantIndi")]
        public ActionResult Update(int id)
        {
            indicadores indiaEditar = _db.indicadores.SingleOrDefault(p => p.id == id);
            
            return View("Create", indiaEditar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantIndi")]
        public ActionResult Update(indicadores model)
        {
            try
            {
                model.usuario_mod = SesionLogin().Sigla;
                model.fecha_mod = DateTime.Now;
                if (model.UF == null)
                {
                    model.UF = (decimal)0.000;
                }
                if (model.IVP == null)
                {
                    model.IVP = (decimal)0.000;
                }
                if (model.DOLAR == null)
                {
                    model.DOLAR = (decimal)0.000;
                }
                if (model.EURO == null)
                {
                    model.EURO = (decimal)0.000;
                }
                if (model.ITCM == null)
                {
                    model.ITCM = (decimal)0.000;
                }
                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
                return JsonError("Error");
            }
            catch (Exception ex)
            {
                var innerMessage = (ex.InnerException != null) ? ex.InnerException.HResult : 0;

                if (!(innerMessage == 0))
                {
                    return JsonError("La fecha del indicador ya existe.");
                }
                return JsonError("Hey !!! hubo un problema.");
            }
        }
    }
}