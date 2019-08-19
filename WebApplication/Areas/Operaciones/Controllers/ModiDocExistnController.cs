using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Operaciones.Controllers
{
    public class ModiDocExistnController : MasterController
    {
        // GET: ModiDocExistn/ModiDocExistn
        [ClientAuthorize("MantModiDoc")]
        public ActionResult Index()
        {
            try
            {
                var id_usu = SesionLogin().ID;

                var ruta = "";
                ViewBag.rutaPhoto = ruta = "../Img/fotos2/" + SesionLogin().Sigla + ".jpg";
                ViewBag.nomusu = SesionLogin().Nom;

                var auxTip = (from tdoc in _db.GS_TDoc
                              select tdoc.CodDoc).ToList();

                var tipoDocu = new SelectList(auxTip, "CodDoc");
                ViewBag.ListaTiposDoc = tipoDocu;

                return View();
            }
            catch (Exception e)
            {
                return JsonError("Error: " + e);
            }
        }

        // GET: ModiDocExistn/ModiDocExistn/Edit/5
        [ClientAuthorize("MantModiDoc")]
        public ActionResult Update(int? id)
        {
            try
            {
                dynamic modelo = new ExpandoObject();

                //var idPar = Int32.Parse(id);
                var documento = _db.GS_Documentos.FirstOrDefault(x => x.ID == id);

                //nombre del cliente para la view
                var auxnombreCli = (from gs_doc in _db.GS_Documentos
                                    join gs_clientes in _db.Clientes on gs_doc.CLIENTE equals gs_clientes.Rut
                                    where gs_doc.ID == id
                                    select gs_clientes.Nom).FirstOrDefault();
                var nombreCli = auxnombreCli.ToString();
                ViewBag.NombreCliente = nombreCli;

                //lista de tipos de documentos para view
                var auxTip = (from tdoc in _db.GS_TDoc
                              select tdoc.CodDoc).ToList();
                
                var tipoDocu = new SelectList(auxTip, "CodDoc");
                ViewBag.ListaTiposDoc = tipoDocu;

                return View(documento);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index");
            }
            
        }

        //POST: ModiDocExistn/ModiDocExistn/Edit/5
        [ClientAuthorize("MantModiDoc")]
        public ActionResult UpdateDoc(GS_Documentos model, HttpPostedFileBase files)
        {
            try
            {
                int id_ = model.ID;
                var docu_gs = _db.GS_Documentos.FirstOrDefault(x => x.ID == id_);

                //valida movRe y Año imputacion
                if (model.MovRe!=null)
                {
                    var fechaAux = model.MovRe;
                    var fechVal = DateTime.Now;
                    var fechaCom = DateTime.Parse("2014-01-01");
                    if (fechaAux < fechaCom || fechaAux > fechVal)
                        return JsonError("La 'Fecha de Recepcion' es menor a 2014 o es mayor a la 'fecha actual' ");
                }
                else if (model.CPBAño!=null)
                {
                    var añoAux = Int32.Parse(model.CPBAño);
                    int añoVal = DateTime.Now.Year;
                    if (añoAux < 2010 || añoAux > añoVal)
                    { 
                        return JsonError("El 'Año de Imputacion' es menor a '2010' o es mayor a la 'fecha actual'" );
                    }
                }
                    //variables para controlar si se hizo alguna modificacion

                    var numCentCtrl = docu_gs.NumCent;
                    var folioFacCtrl = docu_gs.FolioSolFac;
                    var movReCtrl = docu_gs.MovRe;
                    var cpbMesCtrl = docu_gs.CPBMes;
                    var cpbAñoCtrl = docu_gs.CPBAño;
                    var ctdCodCtrl = docu_gs.CTDCod;
                    var archivoCtrl = docu_gs.Archivo;

                    var auxTip = docu_gs.CTDCod;
                    string oldNumCent = docu_gs.NumCent.ToString();
                    docu_gs.NumCent = model.NumCent;
                    docu_gs.FolioSolFac = model.FolioSolFac;
                    docu_gs.MovRe = model.MovRe;
                    docu_gs.CPBMes = model.CPBMes;
                    docu_gs.CPBAño = model.CPBAño;
                    docu_gs.CTDCod = model.CTDCod;
                    string auxDoc = docu_gs.CodAux.ToString();
                    string numDoc = docu_gs.NumDocI.ToString();
                    string tipoDoc = docu_gs.CTDCod.ToString();
                    string nombre_docu = auxDoc + "-" + numDoc + "-" + tipoDoc + ".PDF";
                    string urlArchivo = docu_gs.Archivo;
                    string ruta = @"P:\PSO\PROYECTOS IT\Impu Doc\Fac-Pro\";
                    var path = ruta + nombre_docu;
                    docu_gs.Archivo = path; // se guarda la nueva ruta en archivo y urlArchivo guarda la anterior para hacer su posterior eliminacion...

                    if (numCentCtrl == docu_gs.NumCent 
                        && folioFacCtrl == docu_gs.FolioSolFac 
                        && movReCtrl== docu_gs.MovRe 
                        && cpbMesCtrl== docu_gs.CPBMes
                        && cpbAñoCtrl== docu_gs.CPBAño
                        && ctdCodCtrl== docu_gs.CTDCod
                        && archivoCtrl== docu_gs.Archivo
                        && files == null)
                    {
                        return JsonExitoMsg("SinCambios");
                    }
                    else
                    {
                        if (oldNumCent!=model.NumCent.ToString())
                        {
                            ControlModNumComprobante(oldNumCent, model.NumCent.ToString());
                        }
                        if (files == null && auxTip == model.CTDCod)
                        {
                            ControlUpdate(id_,docu_gs.CPBAño, docu_gs.CPBMes, docu_gs.FolioSolFac.ToString(), docu_gs.CTDCod, docu_gs.NumCent.ToString(), docu_gs.MovRe.ToString(), docu_gs.Archivo);
                            
                            _db.Entry(docu_gs).State = System.Data.Entity.EntityState.Modified;
                            _db.SaveChanges();

                            return JsonExitoMsg("Actualizado");
                        }
                        else if (files == null && auxTip != model.CTDCod)
                        {
                            

                            if (ModelState.IsValid)
                            {
                                //guarda la instruccion en ctrl_accion
                                ControlUpdate(id_, docu_gs.CPBAño, docu_gs.CPBMes, docu_gs.FolioSolFac.ToString(), docu_gs.CTDCod, docu_gs.NumCent.ToString(), docu_gs.MovRe.ToString(), docu_gs.Archivo);

                                System.IO.File.Move(urlArchivo, path);
                                _db.Entry(docu_gs).State = System.Data.Entity.EntityState.Modified;
                                _db.SaveChanges();

                                System.IO.File.Delete(urlArchivo);
                            }
                            else
                            {
                                return JsonError("Modelo ingresado no valido");
                            }

                            return JsonExitoMsg("Actualizado");
                        }

                        else
                        {
                            string auxExt = files.FileName;
                            string ext = Path.GetExtension(auxExt).ToLower();
                            if (ext != ".pdf")
                            {
                                return JsonError("No es compatible la extension del archivo. Solo Pdf");
                            }
                            else
                            {
                                var data = new byte[files.ContentLength];
                                files.InputStream.Read(data, 0, files.ContentLength);

                                using (var sw = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                                {
                                    sw.Write(data, 0, data.Length);
                                }

                                if (ModelState.IsValid)
                                {
                                    //guarda la instruccion en ctrl_accion
                                    ControlUpdate(id_, docu_gs.CPBAño, docu_gs.CPBMes, docu_gs.FolioSolFac.ToString(), docu_gs.CTDCod, docu_gs.NumCent.ToString(), docu_gs.MovRe.ToString(), docu_gs.Archivo);
                                    _db.Entry(docu_gs).State = System.Data.Entity.EntityState.Modified;
                                    _db.SaveChanges();

                                    if (urlArchivo != path)
                                    {
                                        System.IO.File.Delete(urlArchivo);
                                    }
                                }
                                else
                                {
                                    return JsonError("Modelo ingresado no valido");
                                }
                                return JsonExitoMsg("Actualizado");
                            }
                        }
                    }
                   
            }
            catch (Exception e)
            {
                return Json(ErrorService.LogError(e));
            }
        }
        
        // POST: ModiDocExistn/ModiDocExistn/Delete/5
        [ClientAuthorize("MantModiDoc")]
        public ActionResult EliminarDoc(string id)
        {
            try
            {
                int id_ex = Int32.Parse(id);
                var model = _db.GS_Documentos.FirstOrDefault(p => p.ID == id_ex);
                var url = model.Archivo;

                if (model == null)
                {
                    return JsonError("Opps, ocurrio un problema. Contactece con el Administrador");
                }

                if (ModelState.IsValid)
                {
                    ControlDelete(id_ex);
                    _db.Entry(model).State = System.Data.Entity.EntityState.Deleted;
                    _db.SaveChanges();
                    if (url!= null)
                    {
                        System.IO.File.Delete(url);
                    }
                    return JsonExitoMsg("Eliminado");
                }
            }
            catch
            {
                return JsonError("Error");
            }
            return JsonError("Hey !!! hubo un problema.");
        }

        [ClientAuthorize("MantModiDoc")]
        public JsonResult ConsultarDocPorDatos(string codAuxiliar,string numDoc,string tipoDoc)
        {
            
            try
            {
                var num = Int32.Parse(numDoc);
                dynamic sql = new ExpandoObject();

                if (codAuxiliar == null && numDoc == null && tipoDoc == null)
                {
                    throw new Exception("Hubo un error en los parametros");
                }
                else
                {
                    sql = (from gs_doc in _db.GS_Documentos
                           join gs_clientes in _db.Clientes on gs_doc.CLIENTE equals gs_clientes.Rut
                               where gs_doc.CodAux == codAuxiliar && gs_doc.NumDocI == num && gs_doc.CTDCod == tipoDoc
                               select new
                               {
                                   ID_gs = gs_doc.ID,
                                   Cliente = gs_clientes.Nom,
                                   Cod_Aux = gs_doc.CodAux,
                                   TipoDocu = gs_doc.CTDCod,
                                   NumDoc = gs_doc.NumDocI,
                                   Monto = gs_doc.Total,
                                   Mes = gs_doc.CPBMes,
                                   Año = gs_doc.CPBAño,
                                   Recepcionado = gs_doc.MovRe,
                                   Archiv = gs_doc.Archivo
                               }).ToList();

                    if (sql.Count == 0)
                    {
                        return JsonError("No se Encontro el Documento con los datos ingresados");
                    }
                    else
                    {
                        return Json(new { data = sql }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }            
        }
        [ClientAuthorize("MantModiDoc")]
        public JsonResult ConsultarDocPorID(string idp)
        {
            try
            {
                var idPar = Int32.Parse(idp);
                dynamic sql = new ExpandoObject();

                if (idPar == 0)
                {
                    throw new Exception("Hubo un error en los parametros");
                }
                else
                {
                    sql = (from gs_doc in _db.GS_Documentos
                           join gs_clientes in _db.Clientes on gs_doc.CLIENTE equals gs_clientes.Rut
                           where gs_doc.ID == idPar
                           select new
                           {
                               ID_gs = gs_doc.ID,
                               Cliente = gs_clientes.Nom,
                               Cod_Aux = gs_doc.CodAux,
                               TipoDocu = gs_doc.CTDCod,
                               NumDoc = gs_doc.NumDocI,
                               Monto = gs_doc.Total,
                               Mes = gs_doc.CPBMes,
                               Año = gs_doc.CPBAño,
                               Recepcionado = gs_doc.MovRe
                           }).ToList();

                    if (sql.Count == 0)
                    {
                        return JsonError("No se Encontro el Documento con el id ingresado");
                    }
                    else
                    {
                        return Json(new { data = sql }, JsonRequestBehavior.AllowGet);
                    }
                }             
            }
            catch (Exception e)
            {
                return JsonError(e.Message);
            }
        }
        [ClientAuthorize("MantModiDoc")]
        private void ControlUpdate(int id, string año, string mes, string folioSolFac, string tipo, string numCent, string movRe, string archivo)
        {//controla la eliminacion o actualizacion de los documentos e inserta el registro en ctrl_acciones
            try
            {
                ctrl_acciones ctrlAcci_gs = new ctrl_acciones();
                string sql = "UPDATE GS_Documentos  SET GS_Documentos.CPBAño = " + año +
                                        ", GS_Documentos.CPBMes = " + mes +
                                        ", GS_Documentos.FolioSolFac = " + folioSolFac +
                                        ", GS_Documentos.CTDCod = " + tipo +
                                        ", GS_Documentos.NumCent = " + numCent +
                                        ", GS_Documentos.MovRe = " + movRe +
                                        ", GS_Documentos.Archivo = " + archivo +
                                        " WHERE ((GS_Documentos.ID=" + id + "))";
                ctrlAcci_gs.usu = SesionLogin().Sigla;
                ctrlAcci_gs.modulo = 1;
                ctrlAcci_gs.accion = sql;
                ctrlAcci_gs.mom = DateTime.Now;
                _db.ctrl_acciones.Add(ctrlAcci_gs);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                JsonError("No se pudo guardar el cambio");
            }

        }

        [ClientAuthorize("MantModiDoc")]
        private void ControlDelete(int id)
        {//controla la eliminacion o actualizacion de los documentos e inserta el registro en ctrl_acciones
            try
            {
                ctrl_acciones ctrlAcci_gs = new ctrl_acciones();
                string sql = "DELETE FROM GS_Documentos WHERE GS_Documentos.id = " + id;

                ctrlAcci_gs.usu = SesionLogin().Sigla;
                ctrlAcci_gs.modulo = 43;
                ctrlAcci_gs.accion = sql;
                ctrlAcci_gs.mom = DateTime.Now;
                _db.ctrl_acciones.Add(ctrlAcci_gs);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                JsonError("No se pudo guardar el cambio");
            }

        }
        [ClientAuthorize("MantModiDoc")]
        private void ControlModNumComprobante(string oldNum,string newNum)
        {
            if (oldNum != newNum)
            {
                GS_ModNumComp modNumComp_gs = new GS_ModNumComp();
                modNumComp_gs.usuario = SesionLogin().Sigla;
                modNumComp_gs.Folio_old = oldNum;
                modNumComp_gs.Folio_new = newNum;
                modNumComp_gs.momento = DateTime.Now;
                _db.GS_ModNumComp.Add(modNumComp_gs);
                _db.SaveChanges();
            }
        }
    }
}
