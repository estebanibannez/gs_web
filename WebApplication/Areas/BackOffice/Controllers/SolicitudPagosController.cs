using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.BackOffice.Controllers
{
    public class SolicitudPagosController : MasterController
    {
        
        [ClientAuthorize("MantSolPag")]
        public ViewResult Index()
        {
            try
            {
                string usuario = SesionLogin().Sigla;
                var auxClientesxUsu = (from gs_permisos in _db.Permisos
                                       join gs_usuarios in _db.Usu on gs_permisos.Usu equals gs_usuarios.ID
                                       join gs_clientes in _db.Clientes on gs_permisos.Clie equals gs_clientes.ID
                                       where gs_usuarios.uactivo == "S" && gs_usuarios.Sigla == usuario
                                       orderby gs_clientes.Nom 
                                       select new
                                       {
                                           gs_clientes.ID,
                                           gs_clientes.Nombre_Archivo
                                       });
                var clientesxUsu = new SelectList(auxClientesxUsu, "ID", "Nombre_Archivo");
                ViewBag.ListaClientes = clientesxUsu;

                return View();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [ClientAuthorize("MantSolPag")]
        public JsonResult BuscarDocs(int? idCliente,string nomAux, int? numDoc)
        {
            try
            {
                dynamic sql = new ExpandoObject();
               
                    sql = (from gs_docs in _db.GS_Documentos
                           join gs_clientes in _db.Clientes on gs_docs.CLIENTE equals gs_clientes.Rut
                           join gs_tdocs in _db.GS_TDoc on gs_docs.CTDCod equals gs_tdocs.CodDoc
                           join gs_prov in _db.Proveedores on gs_docs.CodAux equals gs_prov.Aux
                           join gs_bco in _db.GS_Bancos on gs_prov.Bco equals gs_bco.ID into prov_bco
                           from provBco in prov_bco.DefaultIfEmpty()

                           where gs_docs.NumCent != null && gs_docs.FechaSolPago == null
                           && (gs_tdocs.TipoCV == "C" || gs_tdocs.TipoCV == "R")
                           && gs_clientes.ID == idCliente
                           && (nomAux == "" || (gs_prov.Nom.Contains(nomAux)))
                           && (numDoc == null || (gs_docs.NumDocI == numDoc))

                           select new
                           {
                               NomArch = gs_clientes.Nombre_Archivo,
                               Mes = gs_docs.CPBMes,
                               Año = gs_docs.CPBAño,
                               Rotulo = gs_tdocs.RotuloDoc,
                               NumDoc = gs_docs.NumDocI,
                               Auxiliar = gs_prov.Aux,
                               NomPro = gs_prov.Nom,
                               MovF = gs_docs.MovFe,
                               Totl = gs_docs.Total,
                               MontPag = gs_docs.MontoPago,
                               NumCe = gs_docs.NumCent,
                               Arch = gs_docs.Archivo,
                               TipCV = gs_tdocs.TipoCV,
                               IdDoc = gs_docs.ID,
                               NomBco = provBco.Nom,
                               CtaProv = gs_prov.Cta,
                               ObSolPago = gs_docs.ObsSolPago,
                               ObPagRea = gs_docs.ObsPagRealizo,
                               EmailProv = gs_prov.Email,
                               FechPagUrg = gs_docs.PagUrg,
                               FormaPago = gs_docs.FP,
                               gs_docs.id_encab
                           }
                        ).ToList();
                
                if (sql.Count == 0)
                {
                    return Json(new { data = new { } }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { data = sql }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { data = new { } }, JsonRequestBehavior.AllowGet);
            }
        }

        [ClientAuthorize("MantSolPag")]
        public JsonResult BuscarAnticipos(int? idclie, string codaux)
        {
            try
            {
                var sql1 = LoadData("exec [sp_selAnticiposProveedor] "+idclie+",'"+codaux+"'");

                return Json(new { data = sql1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { data = new { } }, JsonRequestBehavior.AllowGet);
            }
            
        }
        
        [ClientAuthorize("MantSolPag")]
        public ActionResult Editar(int id)
        {
            try
            {

                var model = _db.GS_Documentos.FirstOrDefault(x => x.ID == id);

                var auxnombreAux = (from gs_doc in _db.GS_Documentos
                                    join gs_prov in _db.Proveedores on gs_doc.CodAux equals gs_prov.Aux
                                    where gs_doc.ID == id
                                    select gs_prov.Nom).FirstOrDefault();
                var nombreAux = auxnombreAux.ToString();
                ViewBag.NombreAuxiliar = nombreAux;

                return Json(new { Model = model });
            }
            catch (Exception e)
            {
                return JsonError("Error: "+e.Message);
            }
            
        }
        
        [HttpPost]
        [ClientAuthorize("MantSolPag")]
        public ActionResult Edit(GS_Documentos model,int SaldoxAnticipo)
        {
            try
            {
                int idMD = model.ID;
                var docu_gs = _db.GS_Documentos.FirstOrDefault(x => x.ID == idMD);
                var auxTotal = docu_gs.Total;
                var dateAndTime = DateTime.Now;
                var dateMin = dateAndTime.Date;
                var dateMax = dateMin.AddMonths(1);

                if (model.PagUrg != null && (model.PagUrg < dateMin || model.PagUrg > dateMax))
                {
                    return JsonError("La 'fecha limite' solo puede ser a partir de hoy y no mayor a un mes");
                }else

                if (SaldoxAnticipo==auxTotal)
                {
                    return JsonError("el valor de 'Anticipo' no puede ser 0");
                }
                else if (SaldoxAnticipo <0 )
                {
                    return JsonError("el valor de 'Saldo' no puede ser negativo");
                }
                else 
                {
                    var FormaPago = model.FP;
                    var ObSolPag = model.ObsSolPago;
                    var FechaLimite = model.PagUrg;
                    var Monto = SaldoxAnticipo;
                    docu_gs.FP = null;
                    docu_gs.ObsSolPago = ObSolPag;
                    docu_gs.ObsPagRealizo = SaldoxAnticipo == 0 ? "SI" : null;
                    docu_gs.PagUrg = FechaLimite;
                    docu_gs.FechaSolPago = DateTime.Now;
                    docu_gs.MontoPago = Monto;
                    docu_gs.UsuSolPago = SesionLogin().Sigla;


                    _db.Entry(docu_gs).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();

                    return JsonExitoMsg("Formulario");
                }
            }
            catch(Exception e)
            {
                return Json(ErrorService.LogError(e));
            }
        }

        [ClientAuthorize("MantSolPag")]
        public ActionResult Delete(string id)
        {
            try
            {
                int id_doc = Int32.Parse(id);
                var modelDoc = _db.GS_Documentos.FirstOrDefault(p => p.ID == id_doc);
                modelDoc.FP = null;
                modelDoc.ObsSolPago = null;
                modelDoc.ObsPagRealizo = null;
                modelDoc.PagUrg = null;
                modelDoc.FechaSolPago = null;
                modelDoc.MontoPago = null;
                modelDoc.UsuSolPago = null;

                if (ModelState.IsValid)
                {
                   _db.Entry(modelDoc).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    return JsonExito();
                }
                else
                {
                    return JsonError("Modelo ingresado no valido");
                }
            }
            catch (Exception e)
            {
                return JsonError("Error: "+e.Message);
            }
        }

        [ClientAuthorize("MantSolPag")]
        public ActionResult Pdf(int id)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                var documento = _db.GS_Documentos.FirstOrDefault(p => p.ID == id);
                if (documento.Archivo == "...")
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
                return JsonError("Hubo un error.! " + e.Message);
            }
        }
    }
}
