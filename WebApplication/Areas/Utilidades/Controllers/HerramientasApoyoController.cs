using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class HerramientasApoyoController : MasterController
    {
        // GET: HerramientasApoyo/HerramientasApoyo

        [ClientAuthorize("MantHera")]
        public ActionResult Index()
        {
            var id_usu = SesionLogin().ID;
            var usuario = _db.Usu.FirstOrDefault(p => p.ID == id_usu);

            var modelo_apoyo = (from her in _db.GS_HerrApoyo
                               join asi in _db.GS_Asig_HrrAp on her.id equals asi.HrrAp into her_asi

                               from asi in her_asi.Where(x => x.usu == id_usu).DefaultIfEmpty()
                               orderby her.nom
                               select her).ToList();            
            return View("Index", modelo_apoyo);
        }
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantHera")]        
        public ActionResult Create(GS_HerrApoyo model)
        {
            var idusu = SesionLogin().ID;
            //var a = (from her in _db.GS_HerrApoyo select her.id).Max();
            
            try
            { 

                if (ModelState.IsValid)
                {
                    _db.GS_HerrApoyo.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }
            }
            catch(Exception e)
            {
                return JsonError("Error");
            }

            return View();
        }

        public ActionResult Update(int id)
        {
            //int id_ex = Int32.Parse(id);
            GS_HerrApoyo apoyoEditar = _db.GS_HerrApoyo.SingleOrDefault(p => p.id == id);
            return View("Create", apoyoEditar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(GS_HerrApoyo model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
                return JsonError("Hey !!! hubo un problema.");

            }
            catch(Exception e)
            {
                return JsonError("Hey !!! hubo un problema.");
            }
        }

        public ActionResult Delete(int id)
        {
            //int id_ex = Int32.Parse(id);

            try
            {
                GS_HerrApoyo apoyoEditar = _db.GS_HerrApoyo.SingleOrDefault(p => p.id == id);

                _db.GS_HerrApoyo.Remove(apoyoEditar);
                _db.SaveChanges();

            }
            catch(Exception e)
            {
                return JsonError("Hey !!! hubo un problema.");
            }

            return View();
        }

    }
}