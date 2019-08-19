using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;



namespace WebApplication.Areas.Utilidades.Controllers
{
    public class EventoController : MasterController
    {
        // GET: Evento/Evento
        public ActionResult Index()

        {
            var id_usu = SesionLogin().ID;
            var emp_usu = (_db.Permisos.Where(x => x.Usu == id_usu).Select(id => id.Clie).ToList());
            var evento = _db.evento.Where(ele => emp_usu.Contains(ele.id_cliente) && ele.estado == "1").Where(ele => ele.id_usuario == id_usu).ToList();
            
            return View(evento);
        }

        public ActionResult Create()
        {
            var usuario = SesionLogin().ID;

            var aux = (from cli in _db.Clientes
                       join per in _db.Permisos on cli.ID equals per.Clie
                       where per.Usu == usuario
                       select cli).ToList();

            var clientes = new SelectList(aux, "ID", "Nom");
            ViewBag.Clientes = clientes;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(evento model)
        {
            try
            {
                var idusu = SesionLogin().ID;
                DateTime fec = DateTime.Today;
                

                model.usuario_mod = SesionLogin().Sigla;
                model.fecha_mod = DateTime.Now;
                model.id_usuario = idusu;
                model.estado = "1";
                model.id_usuario = idusu;
                //!(model.fecha.ToString().Equals(fec.ToString())) || 
                if (model.fecha < fec)
                {
                    return JsonError("Fecha ingresada no debe ser anterior a la de hoy.");
                }

                if (ModelState.IsValid)
                {
                    _db.evento.Add(model);
                    _db.SaveChanges();
                    return JsonExitoMsg("Evento");
                }               

            }
            catch (Exception ex)
            {
                ErrorService.LogError(ex);
                return JsonError("Hey !!! hubo un problema.");
            }

            return View();
        }


        public ActionResult Update(string id)
        {

            int id_ex = Int32.Parse(id);
            var idusu = SesionLogin().ID;

            evento eventoEditar = _db.evento.SingleOrDefault(p => p.id == id_ex);

            var aux = (from cli in _db.Clientes
                       join per in _db.Permisos on cli.ID equals per.Clie
                       where per.Usu == idusu
                       select cli).ToList();


            var clientes = new SelectList(aux,"ID", "Nom");
            ViewBag.Clientes = clientes;

            return View("Create",eventoEditar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(evento model)
        {
            try
            {
                //var idusu = SesionLogin().ID;

                model.usuario_mod = SesionLogin().Sigla;
                model.fecha_mod = DateTime.Now;
                model.id_usuario = SesionLogin().ID;
                model.estado = "1";
                if (model.fecha < DateTime.Now)
                {
                    return JsonError("Fecha ingresada no debe ser anterior a la de hoy.");
                }

                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExitoMsg("Actualizado");
                }
                return JsonError("Error");
            }
            catch (Exception ex)
            {
                ErrorService.LogError(ex);
                return JsonError("Hey !!! hubo un problema.");
            }
        }


        public ActionResult Delete(string id)
        {
            int id_ex = Int32.Parse(id);

            var model = _db.evento.FirstOrDefault(p => p.id == id_ex);

            if (model == null)
            {
                return RedirectToAction("Create");
            }

            try
            {
                model.usuario_mod = SesionLogin().Sigla;
                model.fecha_mod = DateTime.Now;
                model.estado = "0";

                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExitoMsg("Eliminado");
                }

            }
            catch (Exception ex)
            {
                ErrorService.LogError(ex);
                return JsonError("Hey !!! hubo un problema.");
            }

            return View();
        }

        public ActionResult Listado()
        {
            var idusu = SesionLogin().ID;

            var emp_usu = (_db.Permisos.Where(x => x.Usu == idusu).Select(id => id.Clie).ToList());
            var evento = _db.evento.Where(ele => emp_usu.Contains(ele.id_cliente) && ele.estado == "1").Where(ele => ele.id_usuario == idusu).ToList();

            return View(evento);

        }

    }
}