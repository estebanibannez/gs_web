using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class AuxiliaresController : MasterController
    {
        // GET: Auxiliares/Auxiliares
        public ActionResult Index()
        {
            var list_proveedores = (from pro in _db.Proveedores                                      
                                      select pro).AsParallel().ToList();
            return View(list_proveedores);
        }

        [ClientAuthorize("MantAuxi")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantAuxi")]
        public ActionResult Create(Proveedores model)
        {
            try
            {

                var lista_provedores = (from p in _db.Proveedores where p.Aux == model.Aux select p).ToList() ;

                if (lista_provedores.Any()) { throw new Exception("El Rut del proveedor ya existe"); }

                if (model.Aux == null || model.Nom == null || model.Aux.Length > 10)
                {
                    return JsonError("El Rut Auxiliar o Nombre Auxiliar son invalido");
                }

                if(model.Cta != null)
                {
                    model.Cta = model.Cta.TrimEnd();
                }
                if(model.Email != null)
                {
                    model.Email = model.Email.TrimEnd();
                }

                model.Aux.TrimEnd();
                model.Nom.TrimEnd();

                if (ModelState.IsValid)
                {
                    _db.Proveedores.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception e)
            {
                return JsonError("Oooops un Error. "+ e.Message);
            }

            return View();
        }
        [ClientAuthorize("MantAuxi")]
        public ActionResult Update(int id)
        {
            Proveedores pro = _db.Proveedores.FirstOrDefault(x => x.ID == id);

            var aux = pro.Aux.TrimEnd();
            var nom = pro.Nom.TrimEnd();

            pro.Aux = aux;
            pro.Nom = nom;

            return View("Create", pro);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantAuxi")]
        public ActionResult Update(Proveedores model)
        {           

            try
            {
                if (model.Aux == null || model.Nom == null || model.Aux.Length < 1)
                {
                    return JsonError("El Rut Auxiliar o Nombre Auxiliar son invalido");
                }

                if (model.Cta != null)
                {
                    model.Cta = model.Cta.TrimEnd();
                }
                if (model.Email != null)
                {
                    model.Email = model.Email.TrimEnd();
                }

                model.Aux =  model.Aux.TrimEnd();
                model.Nom = model.Nom.TrimEnd();

                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
                return JsonError("Oooops un Error");
            }
            catch (Exception e)
            {
                return JsonError("Oooops un Error");
            }
                
        }

        [ClientAuthorize("MantAuxi")]
        public ActionResult Delete(int id)
        {
            var model = _db.Proveedores.FirstOrDefault(p => p.ID == id);

            if (model == null)
            {
                return RedirectToAction("Create");
            }

            try
            {

                if (ModelState.IsValid)
                {
                    _db.Proveedores.Remove(model);
                    _db.SaveChanges();
                    return JsonExito();
                }


            }
            catch (Exception e)
            {
                return JsonError("Oooops unError");
            }
            return View();
        }       

    }
}