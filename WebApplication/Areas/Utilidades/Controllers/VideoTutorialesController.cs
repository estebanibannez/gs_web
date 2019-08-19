using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class VideoTutorialesController : MasterController
    {
        // GET: VideoTutoriales/VideoTutoriales
        public ActionResult Index()
        {
            var categorias = (from ctvideos in _db.Categoria_Videos select new { id_categoria = ctvideos.ID, nombre_categoria = ctvideos.nombre_categoria }).ToList();
            ViewBag.listadoCategorias = categorias;
            ViewBag.unidad = SesionLogin().unidad;
            return View();
        }

        [HttpPost]
        [ClientAuthorize("MantTutorial")]
        public JsonResult getVideos(int? id)
        {
            try
            {
                if (id != null)
                {
                    var listaVideos = (from v in _db.VideosTutoriales where v.id_categoria == id && v.estado == 1 select new { id_video = v.ID_video, ruta_video = v.ruta_video, ruta_poster = v.ruta_poster, descripcion = v.descripcion_video , titulo_video = v.titulo_video}).ToList();

                    return Json(new { data = listaVideos }, JsonRequestBehavior.AllowGet);
                }
                else {
                    var listaVideos = (from v in _db.VideosTutoriales where v.estado == 1 select new { id_video = v.ID_video, ruta_video = v.ruta_video, ruta_poster = v.ruta_poster, descripcion = v.descripcion_video , titulo_video = v.titulo_video }).ToList();

                    return Json(new { data = listaVideos }, JsonRequestBehavior.AllowGet);
                }
               
            }
            catch (Exception e) {
                return JsonError("Ha ocurrido un error.");
            }
        }

        [HttpPost]
        [ClientAuthorize("MantTutorial")]
        public JsonResult sendVideosGs(int? id_video)
        {
            try
            {
                var model = new LogViewsVideos();
                model.id_video = id_video;
                model.id_usu = SesionLogin().ID;
                model.fecha = DateTime.Now;
                model.nombre_usuario = SesionLogin().Sigla;
                if (ModelState.IsValid)
                {
                    _db.LogViewsVideos.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }

            }
            catch (Exception e) {
                return JsonError("Ocurrio un Error.");
            }
            return Json("");
        }
        [ClientAuthorize("MantTutorial")]
        public JsonResult DeleteVideo(int? id)
        {
            try
            {
                var model = _db.VideosTutoriales.FirstOrDefault(x => x.ID_video == id);
                model.estado = 0;
                if (ModelState.IsValid)
                {
                    _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExitoMsg("Video eliminado con éxito.");
                }
            return JsonError("Hey !!! hubo un problema.");
            }
            catch (Exception e) {

                return JsonError("Ha ocurrido un error.");
            }

        }
        [ClientAuthorize("MantTutorial")]
        [HttpPost]
        public JsonResult updateVideo(VideosTutoriales video)
        {
            try
            {
                var model = _db.VideosTutoriales.FirstOrDefault(x => x.ID_video == video.ID_video);/* Some more results here*/
                model.titulo_video = video.titulo_video;
                model.descripcion_video = video.descripcion_video;
                _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception e)
            {
                return JsonExitoMsg("Ha ocurrido un error..");
            }
        }
    }
}