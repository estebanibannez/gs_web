using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class BibliotecaController : MasterController
    {
        // GET: Biblioteca/biblioteca
        [ClientAuthorize("MantBibl")]
        public ActionResult Index()
        {
            try
            {
                var clasificacion = (from clas in _db.GS_Clasif_Arch
                                     orderby clas.Nombre ascending
                                     select clas).ToList();

                ViewBag.clasificacion = new SelectList(clasificacion, "ID", "Nombre");
                return View();
            }
            catch (Exception)
            {
                throw;
            }
        }
        [ClientAuthorize("MantBibl")]
        public ActionResult consultatabla(string id_clasificacion, string idsub_clasificacion)
        {
            var cond1 = "";
            var cond2 = "";
            try
            {
                if (id_clasificacion != "")
                {
                    cond1 = "AND dbo.GS_Clasif_Arch.ID =" + id_clasificacion + "";
                }

                if (idsub_clasificacion != "")
                {
                    cond2 = "AND dbo.GS_SubClasif_Arch.ID =" + idsub_clasificacion + "";
                }

                var query = LoadData("SELECT dbo.Biblioteca_GS_Web.ID AS ID," +
                                    "dbo.GS_Clasif_Arch.ID AS ID_clasificacion," +
                                    "dbo.GS_SubClasif_Arch.ID AS ID_subclasificacion," +
                                    "dbo.Biblioteca_GS_Web.Nombre AS nombre, " +
                                    "dbo.GS_Clasif_Arch.Nombre AS clasificacion, " +
                                    "dbo.GS_SubClasif_Arch.Nombre AS subclasificacion, " +
                                    "dbo.Biblioteca_GS_Web.Descripcion AS descripcion, " +
                                    "dbo.Biblioteca_GS_Web.Ruta AS ruta, " +
                                    "dbo.Biblioteca_GS_Web.UsuMod AS usumod, " +
                                    "dbo.Biblioteca_GS_Web.FechaMod AS fechamod " +
                                    "FROM dbo.GS_Clasif_Arch " +
                                    "RIGHT OUTER JOIN dbo.GS_SubClasif_Arch ON dbo.GS_Clasif_Arch.ID = dbo.GS_SubClasif_Arch.Clasif " +
                                    "RIGHT OUTER JOIN dbo.Biblioteca_GS_Web ON dbo.GS_SubClasif_Arch.ID = dbo.Biblioteca_GS_Web.Tipo " +
                                    "WHERE (dbo.Biblioteca_GS_Web.ID <> 0) " + cond1 + " " + cond2 + " ORDER BY dbo.Biblioteca_GS_Web.Nombre");

                return Json(new { data = query.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [ClientAuthorize("MantBibl")]
        public ActionResult Descarga(string id)
        {
            try
            {
                var ID = Convert.ToInt32(id);
                var document = _db.Biblioteca_GS_Web.FirstOrDefault(p => p.ID == ID);
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = document.Nombre,
                    Inline = false,
                };
                var Doc_file = (document.Ruta).Replace(@"P:\", @"\\atenea\puentesur\").Trim(); //Replace (Ubicaion de los archivos en el servidor disco P:\\atenea\puentesur\).        
                return File(Doc_file, System.Web.MimeMapping.GetMimeMapping(document.Nombre.Trim()), document.Ruta.Split('\\').Last().Trim());
            }
            catch (Exception ex)
            {
                return JsonError("No se pudo cargar el archivo");
            }
            
        }

        [ClientAuthorize("MantBibl","MantAdminDocsBiblio")]
        [HttpPost]
        public JsonResult getSubClasif(string id_clasificacion)
        {
            try
            {
                if (id_clasificacion == null || id_clasificacion == "")
                {
                    var subClasificacion = (from sub in _db.GS_SubClasif_Arch
                                            join clas in _db.GS_Clasif_Arch on sub.Clasif equals clas.ID
                                            //where (sub.Clasif == id)
                                            select sub);
                    //id_clasificacion = "";
                    return Json(new { data = subClasificacion.ToList() }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    var id = Convert.ToInt32(id_clasificacion);
                    //var subClasificacion = (from sub in _db.GS_SubClasif_Arch
                    //                        join clas in _db.GS_Clasif_Arch on sub.Clasif equals clas.ID
                    //                        where (sub.Clasif == id)
                    //                        select sub).ToList();

                    var subClasificacion = (from sub in _db.GS_SubClasif_Arch
                                            where (sub.Clasif == id)
                                            select new
                                            {
                                                ID = sub.ID,
                                                Nombre = sub.Nombre
                                            }).ToList();

                    return Json(new { data = subClasificacion }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [ClientAuthorize("MantAdminDocsBiblio")]
        public ActionResult Create()
        {
            try
            {
                var clasificacion = (from clas in _db.GS_Clasif_Arch
                                     orderby clas.Nombre ascending
                                     select clas).ToList();
                ViewBag.clasificacion = new SelectList(clasificacion, "ID", "Nombre");

                return View();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantAdminDocsBiblio")]
        public JsonResult Create(Biblioteca_GS_Web model,HttpPostedFileBase files,string clasificacion_create)
        {
            try
            {
                if (files==null)
                {
                    return JsonError("Debe cargar un archivo para la creacion del registro...");
                }
                int id_clasi = Int32.Parse(clasificacion_create);
                var pathDefault = @"\\atenea\puentesur\PSO\GS\Biblioteca GS\";
                //var pathDefault = @"C:\TempArchCorreo\";//directorio de pruebas..
                
                var gs_clasif = _db.GS_Clasif_Arch.SingleOrDefault(z => z.ID == id_clasi);
                var gs_subclasif = _db.GS_SubClasif_Arch.SingleOrDefault(x => x.ID == model.Tipo);
                var fileName = files.FileName;

                
                pathDefault = pathDefault + gs_clasif.Nombre + "\\" + gs_subclasif.Nombre + "\\";
                System.IO.Directory.CreateDirectory(pathDefault);
                pathDefault = Path.Combine(pathDefault, fileName);

                var data = new byte[files.ContentLength];
                files.InputStream.Read(data, 0, files.ContentLength);
                using (var ar = new FileStream(pathDefault, FileMode.Create, FileAccess.Write))
                {
                    ar.Write(data, 0, data.Length);
                }
                model.Ruta = pathDefault;
                model.UsuMod = SesionLogin().Sigla;
                model.FechaMod = DateTime.Now;
                _db.Biblioteca_GS_Web.Add(model);
                _db.SaveChanges();

                return JsonExito();
            }
            catch (Exception ex)
            {
                return JsonError("Error Al Crear...");
            }
        }
        //get
        [ClientAuthorize("MantAdminDocsBiblio")]
        public ActionResult Update(string id)
        {
            try
            {
                int id_ex = Int32.Parse(id);
                Biblioteca_GS_Web archivoAEditar = _db.Biblioteca_GS_Web.SingleOrDefault(p => p.ID == id_ex);
                var idSubCla = _db.GS_SubClasif_Arch.SingleOrDefault(z => z.ID == archivoAEditar.Tipo);
                var idClasifi = _db.GS_Clasif_Arch.SingleOrDefault(x => x.ID == idSubCla.Clasif);
                archivoAEditar.Nombre = archivoAEditar.Nombre.Trim();
                archivoAEditar.Ruta = archivoAEditar.Ruta.Trim();
                archivoAEditar.Descripcion = archivoAEditar.Descripcion.Trim();
                var clasificacion = (from clas in _db.GS_Clasif_Arch
                                     orderby clas.Nombre descending
                                     select clas).ToList();
                ViewBag.clasificacion = new SelectList(clasificacion, "ID", "Nombre");
                ViewBag.subclasificacionSelected = idSubCla.ID;
                ViewBag.clasificacionSelected = idClasifi.ID;

                return View("Create", archivoAEditar);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //post
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantAdminDocsBiblio")]
        public JsonResult Update(Biblioteca_GS_Web model, HttpPostedFileBase files, string clasificacion_create)
        {
            try
            {
                int id_ = model.ID;
                var archiv_gs = _db.Biblioteca_GS_Web.FirstOrDefault(x => x.ID == id_);
                if (files!=null)
                {
                    int id_clasi = Int32.Parse(clasificacion_create);

                    var gs_subclasif = _db.GS_SubClasifDoc.SingleOrDefault(x => x.id == model.Tipo);
                    var gs_clasif = _db.GS_ClasifDoc.SingleOrDefault(z => z.id == id_clasi);
                    var fileName = files.FileName;

                    var pathDefault = @"\\atenea\puentesur\PSO\GS\Biblioteca GS\";
                    //var pathDefault = @"C:\TempArchCorreo\";//directorio de pruebas..

                    pathDefault = pathDefault + gs_clasif.Nom + "\\" + gs_subclasif.nom + "\\";
                    System.IO.Directory.CreateDirectory(pathDefault);
                    pathDefault = Path.Combine(pathDefault, fileName);

                    var data = new byte[files.ContentLength];
                    files.InputStream.Read(data, 0, files.ContentLength);
                    using (var ar = new FileStream(pathDefault, FileMode.Create, FileAccess.Write))
                    {
                        ar.Write(data, 0, data.Length);
                    }
                    //System.IO.File.Delete(archiv_gs.Ruta);
                    
                    model.Ruta = pathDefault;
                }
                else
                {
                    model.Ruta = archiv_gs.Ruta;
                }

                archiv_gs.Nombre = model.Nombre;
                archiv_gs.Descripcion = model.Descripcion;
                archiv_gs.Ruta = model.Ruta;
                archiv_gs.Tipo = model.Tipo;
                archiv_gs.UsuMod = SesionLogin().Sigla;
                archiv_gs.FechaMod = DateTime.Now;

                _db.Entry(archiv_gs).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return JsonExito();
            }
            catch (Exception ex)
            {
                return JsonError("Error Al Modificar...");
            }
        }

        [ClientAuthorize("MantAdminDocsBiblio")]
        public ActionResult Delete(string id)
        {
            try
            {
                int id_ex = Int32.Parse(id);
                var model = _db.Biblioteca_GS_Web.FirstOrDefault(p => p.ID == id_ex);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                if (ModelState.IsValid)
                {
                    System.IO.File.Delete(model.Ruta);
                    _db.Biblioteca_GS_Web.Remove(model);
                    _db.SaveChanges();
                }

                return JsonExito();
            }
            catch (Exception ex)
            {
                return JsonError("Error al eliminar documento...");
            }
            
        }
    }
}