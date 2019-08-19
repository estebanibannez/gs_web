using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using WebApplication.App_Start;
using WebApplicationMod;

namespace WebApplication.Areas.Operaciones.Controllers
{
    public class IngresoDocumentoController : MasterController
    {
        [ClientAuthorize("MantOper")]
        public ActionResult Index()
        {
            ////Not In devuelve ids que el origen sea distinto de null , entrega sea nulo y finalmente recep sea null
            var not_in = (from g in _db.GS_Documentos
                          where !(g.origen == null) && g.Entrega == null && g.recep == null
                          select g.ID).ToList();
            ////devuelve la vista docs , con usuent ting tst sea nulo con un not in a la lista not in
            var model = (from ni in _db.Docs where ni.UsuEnt == null && ni.ting == "GS-SO" && ni.tst == null && !not_in.Contains(ni.ID) select ni
                          ).ToList();
            //LISTA DE CLIENTES 
            var selectClie = (from c in _db.Clientes where c.Activo == "S" orderby c.Rut select c).ToList();
            ViewBag.clientes = new SelectList(selectClie, "Rut", "Nom");
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-GB");

            return View(model);
        }

        

        [HttpPost]
        [ClientAuthorize("MantOper")]
        public JsonResult Editar(int? id_file)
        {
            try
            {
                var typ = "";
                //devuelve una instancia de objeto según id.
                var model = (from s in _db.Docs
                             where s.ID == id_file
                             select s).FirstOrDefault();
                var emision = (Convert.ToDateTime(model.MovFe)).ToString("dd/MM/yyyy");
                var vencimiento = (Convert.ToDateTime(model.MovFv)).ToString("dd/MM/yyyy");
                var clie = (from d in _db.Clientes
                            where
    d.Rut == model.Rut
                            select d.Tipo).FirstOrDefault();
                typ = clie == "0" ? typ = "afecto" : typ = "exento";
                string cod0 = model.CodAux;
                var nomCorto = (from x in _db.Proveedores
                                where x.Aux.TrimEnd() == cod0
                                select x.Nom).FirstOrDefault();
                var tiDocs = (from e in _db.GS_TDoc
                              where e.TipoCV == "R" || e.TipoCV == "C" && e.Tipo != null && ((typ == "exento" && e.Tipo != "A") || typ == "afecto")
                              select new
                              {
                                  id = e.CodDoc,
                                  nom = e.CodDoc + " " + e.DesDoc,
                                  impu = e.Impto
                              }).ToList();
                return Json(new { Model = model, nomCorto = nomCorto.TrimEnd(), emi = emision, ven = vencimiento, tipos = tiDocs, ty = typ });
            }
            catch (Exception e)
            {
                return JsonError("Hubo un error.!");
            }
        }

        [HttpPost]
        [ClientAuthorize("MantOper")]
        public JsonResult Edit(GS_Documentos model, string Tipo, string emision, string vencimiento, HttpPostedFileBase file)
        {
            try
            {

                DateTime fechaEm, fechaVe;

                if (!DateTime.TryParse(emision, out fechaEm) || !DateTime.TryParse(vencimiento, out fechaVe))
                {
                    return JsonError("formato de fecha no valido");
                }
                //en caso que se ingrese un documento nuevo, retornara al mensaje que esta dentro del if...
                if (file != null && model.ID == 0)
                {

                    string extension = Path.GetExtension(file.FileName);
                    if (extension != ".pdf")
                    {
                        return JsonError("Tipo de archivo incorrecto , Ingrese pdf");
                    }

                    if (model.TCambio == 0)
                    {
                        return JsonError("El tipo de cambio debe ser mayor a 0");
                    }

                    string auxDoc = model.CodAux.ToString();
                    string numDoc = model.NumDocI.ToString();
                    string tipoDoc = model.CTDCod.ToString();
                    string nombre_docu = auxDoc + "-" + numDoc + "-" + tipoDoc + ".PDF";
                    string rutaG = @"\\atenea\puentesur\PSO\PROYECTOS IT\Impu Doc\Fac-Pro\";
                    string nuevo = Path.Combine(rutaG, nombre_docu);
                    var data = new byte[file.ContentLength];
                    file.InputStream.Read(data, 0, file.ContentLength);
                    GS_Documentos documento = new GS_Documentos();
                    documento.CPBAño = model.CPBAño;
                    documento.CPBMes = model.CPBMes;
                    documento.CodAux = model.CodAux;
                    documento.CTDCod = model.CTDCod;
                    documento.NumDocI = model.NumDocI;
                    documento.MovFe = fechaEm;
                    documento.MovFv = fechaVe;
                    documento.VendCod = "1";
                    documento.MovGlosa = model.MovGlosa;
                    documento.Neto = Tipo != "exento" ? model.Neto : 0;
                    documento.Impu = Tipo != "exento" ? model.Impu : 0;
                    documento.NetoSM = Tipo != "exento" ? Math.Abs((Math.Round(((Convert.ToDouble(model.Neto)) / Convert.ToDouble(model.TCambio)), 5))) : 0;
                    documento.ImpuSM = Tipo != "exento" ? Math.Abs((Math.Round(((Convert.ToDouble(model.Impu)) / Convert.ToDouble(model.TCambio)), 5))) : 0;
                    documento.Exento = model.Exento;
                    documento.Total = model.Total;
                    documento.NomAux = model.NomAux;
                    documento.Archivo = nuevo;
                    documento.Usu = SesionLogin().Nom;
                    documento.CLIENTE = model.CLIENTE;
                    documento.Creacion = DateTime.Now;
                    documento.Scaneo = DateTime.Now;
                    documento.ExentoSM = Math.Abs(Math.Round(((Convert.ToDouble(model.Exento)) / Convert.ToDouble(model.TCambio)), 5));
                    documento.TotalSM = Math.Abs((Math.Round(((Convert.ToDouble(model.Total)) / Convert.ToDouble(model.TCambio)), 5)));
                    documento.TCambio = model.TCambio;
                    documento.tst = null;
                    documento.Entrega = null;
                    documento.UsuEnt = null;
                    documento.recep = null;
                    documento.ting = "GS-SO";
                    documento.fcrea = DateTime.Now;
                    documento.flingdoc = fechaVe;
                    documento.folioint = null;
                    documento.CabGlo = model.CabGlo;
                    documento.MovRe = DateTime.Now;
                    _db.GS_Documentos.Add(documento);
                    _db.SaveChanges();
                    using (var sw = new FileStream(nuevo, FileMode.Create, FileAccess.Write))
                    {
                        sw.Write(data, 0, data.Length);
                    }
                    return JsonExitoMsg("documento guardado");
                }///fin del create.............

                var movglosa = model.MovGlosa.Trim();
                if (model.TCambio == 0)
                {
                    return JsonError("El tipo de cambio debe ser mayor a 0");
                }

                var rutDocs = (from x in _db.Docs where x.ID == model.ID select new { rut = x.Rut }).FirstOrDefault();

                ////verificar que el periodo no se encuentre cerrado
                //var i = (from clie in _db.Clientes where clie.Rut == rutDocs.rut select new { id = clie.ID }).FirstOrDefault();
                //var quer = (from f in _db.GS_ctrl_cump where f.nulo == null && f.tipo == 3 && f.clasif == "R" && f.clie == i.id && (f.periodo.Value).Month == int.Parse(model.CPBMes) && (f.periodo.Value).Year == int.Parse(model.CPBAño) select f).Any();
                //  if (!quer)
                //  {
                //    return JsonError("El periodo de imputación se encuentra cerrado");
                // }
                //var datos = (from x in _db.Clientes where x.Rut == rutDocs.rut select x).ToList();

                var dat = _db.Clientes.FirstOrDefault(x => x.Rut == rutDocs.rut).Ruta.Split('\\').Last();
                var rut_pro = model.CodAux;
                var num_doc = model.NumDocI.ToString();
                //Verificar la existencia del documento en las tablas de GS
                var existe = (from gdoc in _db.GS_Documentos
                              where gdoc.CodAux == model.CodAux && gdoc.NumDocI == model.NumDocI && gdoc.CTDCod == model.CTDCod && model.UsuEnt !=null && model.Entrega != null
                              select new
                              {
                                  codaux = gdoc.CodAux,
                                  numdocI = gdoc.NumDocI,
                                  ctdcod = gdoc.CTDCod
                              }).Any();

                if (existe)
                {
                    return JsonError("Este Auxiliar ya tiene registrado,documento de este tipo y con este número en GS");
                }

                //Verificar la existencia del documento en las tablas de softland
                List<SqlParameter> para = new List<SqlParameter>()
                {
                        new SqlParameter("@Cliente", System.Data.SqlDbType.NVarChar) { Value = dat},
                        new SqlParameter("@Rut_proveedor", System.Data.SqlDbType.NVarChar) { Value = rut_pro},
                        new SqlParameter("@Tipo_Doc", System.Data.SqlDbType.NVarChar) { Value = model.CTDCod},
                        new SqlParameter("@Num_Doc", System.Data.SqlDbType.NVarChar) { Value = num_doc},
                        new SqlParameter("@counts", System.Data.SqlDbType.Int) {Value = 0 }
                };

                var validar = PA_Almacenado(para, "PA_Valida_Existe_Documento");

                if (validar == 1)
                {
                    return JsonError("Este Auxiliar ya tiene registrado, un documento de este tipo y con este numero en Softland ");
                }

                string uparchivo = "";

                var gsDocumento = _db.GS_Documentos.SingleOrDefault(p => p.ID == model.ID);

                if (file != null)
                {
                    var rutaantig = gsDocumento.Archivo;
                    if (model.Archivo != null)
                    {
                        //Borrar original
                        System.IO.File.Delete(rutaantig);
                    }
                    //Generar archivo con el formato codaux+numdoc + tipodoc
                    string auxDoc = model.CodAux.ToString();
                    string numDoc = model.NumDocI.ToString();
                    string tipoDoc = model.CTDCod.ToString();
                    string nombre_docu = auxDoc + "-" + numDoc + "-" + tipoDoc + ".PDF";
                    string rutaG = @"\\atenea\puentesur\PSO\PROYECTOS IT\Impu Doc\Fac-Pro\";
                    var nuevoarch = rutaG + nombre_docu;
                    //Nueva Ruta archivo en la base de datos
                    gsDocumento.Archivo = nuevoarch;
                    var data = new byte[file.ContentLength];
                    file.InputStream.Read(data, 0, file.ContentLength);
                    //Grabar nuevo
                    using (var sw = new FileStream(nuevoarch, FileMode.Create, FileAccess.Write))
                    {
                        sw.Write(data, 0, data.Length);
                    }
                    uparchivo = ", GS_Documentos.Archivo = " + model.Archivo;
                }

                gsDocumento.NumDocI = model.NumDocI;
                gsDocumento.CTDCod = model.CTDCod;
                gsDocumento.Neto = Tipo != "exento" ? model.Neto : 0;
                gsDocumento.Impu = Tipo != "exento" ? model.Impu : 0;
                gsDocumento.NetoSM = Tipo != "exento" ? (Math.Round(((Convert.ToDouble(model.Neto)) / Convert.ToDouble(model.TCambio)), 5)) : 0;
                gsDocumento.ImpuSM = Tipo != "exento" ? (Math.Round(((Convert.ToDouble(model.Impu)) / Convert.ToDouble(model.TCambio)), 5)) : 0;
                gsDocumento.Exento = model.Exento;
                gsDocumento.Total = model.Total;
                gsDocumento.TCambio = model.TCambio;
                gsDocumento.CPBAño = model.CPBAño;
                gsDocumento.CPBMes = model.CPBMes;
                gsDocumento.MovFv = fechaVe;
                gsDocumento.MovFe = fechaEm;
                gsDocumento.ExentoSM = (Math.Round(((Convert.ToDouble(model.Exento)) / Convert.ToDouble(model.TCambio)), 5));
                gsDocumento.TotalSM = (Math.Round(((Convert.ToDouble(model.Total)) / Convert.ToDouble(model.TCambio)), 5));
                gsDocumento.MovGlosa = movglosa;
                string sql = "UPDATE GS_Documentos  SET  GS_Documentos.NumDocI = " + model.NumDocI +
                    ", GS_Documentos.CTDCod = " + model.CTDCod +
                    ", GS_Documentos.Neto = " + model.Neto +
                    ", GS_Documentos.Exento = " + model.Exento +
                    ", GS_Documentos.Impu = " + model.Impu +
                    ", GS_Documentos.Total = " + model.Total +
                    ", GS_Documentos.TCambio = " + model.TCambio +
                    ", GS_Documentos.CPBAño = " + model.CPBAño +
                    ", GS_Documentos.CPBMes = " + model.CPBMes +
                    ", GS_Documentos.MovFv = " + fechaVe +
                    ", GS_Documentos.MovFe = " + fechaEm + ", GS_Documentos.MovGlosa = " + movglosa + uparchivo;

                registrarAccion(sql);
                _db.Entry(gsDocumento).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return JsonExito();
            }

            catch (Exception ex)
            {
                ErrorService.LogError(ex);
                return JsonError("Hubo un error.! " + ex.Message);
            }

        }

        [ClientAuthorize("MantOper")]
        private void registrarAccion(string query)
        {
            int numero = (query.Contains("DELETE") ? 43 : 1);
            ctrl_acciones ctrlAcci_gs = new ctrl_acciones();
            ctrlAcci_gs.usu = SesionLogin().Sigla;
            ctrlAcci_gs.modulo = numero;
            ctrlAcci_gs.accion = query;
            ctrlAcci_gs.mom = DateTime.Now;
            _db.ctrl_acciones.Add(ctrlAcci_gs);
            _db.SaveChanges();
        }
        //[HttpPost]
        [ClientAuthorize("MantOper")]
        public ActionResult Delete(int? id)
        {
            try
            {
                var query = _db.GS_Documentos.FirstOrDefault(x => x.ID == id);
                string sql = "DELETE FROM GS_Documentos WHERE GS_Documentos.id = " + id;
                _db.GS_Documentos.Remove(query);
                registrarAccion(sql);
                _db.SaveChanges();
                return JsonExitoMsg("El registro a sido Eliminado con éxito!");
            }
            catch (Exception e)
            {

                return JsonError("Hubo un error.!");
            }
        }


        [ClientAuthorize("MantOper", "MantImput")]
        public ActionResult Pdf(int id)
        {
            try
            {

                //Busca pdf físico
                MemoryStream ms = new MemoryStream();
                var documento = _db.GS_Documentos.FirstOrDefault(p => p.ID == id);
                Response.AppendHeader("Content-Disposition", "inline; filename=" + documento.Archivo);
                var Doc_file = (documento.Archivo).Replace(@"P:\", @"\\atenea\puentesur\"); //Replace (Ubicaion de los archivos en el servidor disco P:\\atenea\puentesur\).                       
                using (FileStream file = new FileStream(Doc_file, FileMode.Open, FileAccess.Read))
                {
                    byte[] bytes = new byte[file.Length];
                    file.Read(bytes, 0, (int)file.Length);
                    ms.Write(bytes, 0, (int)file.Length);
                }

                return File(ms.ToArray(), MimeMapping.GetMimeMapping(documento.Archivo));
            }
            catch (Exception e)
            {

                return JsonError(e.Message);
            }

        }


        [HttpPost]
        [ClientAuthorize("MantOper")]
        public JsonResult setImputacion(int[] data)
        {
            string nombre = SesionLogin().Sigla;
            try
            {
                if (data == null)
                {
                    return JsonError("No se selecciono nada");
                }

                var imp = "";
                List<int> lista = data.ToList();
                var s = (_db.GS_Documentos.Where(mo => lista.Contains(mo.ID))).ToList();
                var impue = _db.Docs.Where(mo => lista.Contains(mo.ID)).ToList();
                foreach (var items in s)
                {
                    ////Pasar a estado de imputacion
                    items.UsuEnt = nombre;
                    items.Entrega = DateTime.Now;
                    _db.Entry(items).State = System.Data.Entity.EntityState.Modified;
                    _db.SaveChanges();
                    ////////validar si existe el auxiliar(proveedor) en softland, si no existe, se creara...
                    var clientes = _db.Clientes.FirstOrDefault(x => x.Rut == items.CLIENTE);
                    var proveedores = _db.Proveedores.FirstOrDefault(z => z.Aux == items.CodAux);
                    Helper h = new Helper(_db);
                    var codaux = Int32.Parse(proveedores.Aux.Trim());
                    var rutAux = (string.Format("{0:#,##0.##}", codaux)) + "-" + h.digitoVerificador(Int64.Parse(proveedores.Aux));
                    var email = proveedores.Email == null || proveedores.Email == "" ? "sincorreo" : proveedores.Email;

                    List<SqlParameter> para = new List<SqlParameter>()
                    {
                        new SqlParameter("@Cliente",System.Data.SqlDbType.NVarChar){Value=clientes.Ruta.Split('\\').Last()},
                        new SqlParameter("@CodAux",System.Data.SqlDbType.NVarChar){Value=codaux},
                        new SqlParameter("@NomAux",System.Data.SqlDbType.NVarChar){Value= proveedores.Nom},
                        new SqlParameter("@RutAux",System.Data.SqlDbType.NVarChar){Value=rutAux },
                        new SqlParameter("@Email",System.Data.SqlDbType.NVarChar){Value=email },
                        new SqlParameter("@Usuario",System.Data.SqlDbType.NVarChar){Value=SesionLogin().Sigla },
                    };

                    var sp = PA_AlacenadoNoReturn(para, "PA_validaAntesInsertarDinamico");

                    foreach (var ele in impue)
                    {
                        if (ele.ID == items.ID)
                        {
                            imp = ele.impsug;
                            break;
                        }
                    }
                    //verificar si tiene imputacion sugerida
                    if (imp == "S")
                    {
                        //Buscar el ultimo documento ingresado del auxiliar

                        var consu = (from tdoc in _db.GS_TDoc
                                     join d in _db.Docs on tdoc.CodDoc equals d.CTDCod
                                     into d2
                                     from d in d2.DefaultIfEmpty()
                                     orderby d.Creacion descending
                                     where d.CodAux == items.CodAux && d.CTDCod == items.CTDCod && d.CLIENTE == items.CLIENTE && d.UsuImp != null
                                     select new
                                     {
                                         NumDocI = d.NumDocI,
                                         CodAux = d.CodAux,
                                         CTDCod = d.CTDCod,
                                         id = d.ID,
                                         Total = d.Total,
                                         Neto = d.Neto,
                                         Exento = d.Exento,
                                         Dsd = d.DSD
                                     }).FirstOrDefault();

                        //var documento = consu.id;/////no esta LLEGANDO EL ULTIMO DOCUMENTO DEL DOC ID=337431, SE CAMBIO HACIA DENTRO DEL IF Y SE MANDARA MENSAJE CORRESPONDIENTE
                        if (consu != null)
                        {
                            var documento = consu.id;/////no esta LLEGANDO EL ULTIMO DOCUMENTO DEL DOC ID=337431
                            var Monto_doc_ant = consu.Dsd == "1" ? consu.Total : (int)consu.Neto + (int)consu.Exento.Value;
                            //Buscar la imputacion anterior
                            var con = (from y in _db.GS_Detalles where y.Doc == documento select new { id = y.ID, codcta = y.CodCta, monto = y.Monto, CCCod = y.CCCod }).ToList();

                            double sumaMontoDocAnt = 0;
                            
                            foreach (var it in con)
                            {
                                var montoDetalle = Double.Parse(it.monto);
                                sumaMontoDocAnt =+  montoDetalle;
                            }

                            if (Monto_doc_ant != sumaMontoDocAnt)
                            {
                                items.UsuEnt = null;
                                items.Entrega = null;
                                _db.Entry(items).State = System.Data.Entity.EntityState.Modified;
                                _db.SaveChanges();
                                return JsonError("Los montos de la imputacion sugerida son mayor o menor al 100%");
                            }
                            
                            //Monto de documento a imputar                    
                            var monto_doc = consu.Dsd == "1" ? (int)items.Total : (int)items.Neto.Value + (int)items.Exento.Value;

                            var validaCta = 0;
                            if (con.Count() != 0)
                            {
                                //Verificar que la cuenta de imputacion exista y nose ha eliminado.

                                List<SqlParameter> paraCuenta = new List<SqlParameter>()
                                {
                                    new SqlParameter("@Cliente",System.Data.SqlDbType.NVarChar){Value=clientes.Ruta.Split('\\').Last().Trim()},
                                    new SqlParameter("@Cuenta",System.Data.SqlDbType.NVarChar){Value=con.First().codcta.Trim()},
                                    new SqlParameter("@counts", System.Data.SqlDbType.Int){Value = 0}
                                };

                                validaCta = PA_Almacenado(paraCuenta, "PA_Valida_Existe_Cuenta");
                            }

                            if (validaCta == 1)
                            {
                                List<GS_Detalles> listDet = new List<GS_Detalles>();
                                GS_Detalles detalles = new GS_Detalles();
                                foreach (var item in con)
                                {
                                    double factor = double.Parse(item.monto) / (double)(Monto_doc_ant.Value);
                                    double monto = Math.Round(factor, 4) * monto_doc;
                                    double montoSM = monto / (double)items.TCambio;

                                    detalles.Doc = documento;
                                    detalles.CodCta = item.codcta;
                                    detalles.CCCod = item.CCCod;
                                    detalles.Monto = monto_doc.ToString();
                                    detalles.MontoSM = montoSM;
                                    listDet.Add(detalles);

                                }
                                _db.GS_Detalles.AddRange(listDet);

                                //liberar para centralizar
                                items.Imputacion = DateTime.Now;
                                items.UsuImp = "SIS";//Tiene que ir "SIS" ya que en el mantenedor centralizacion se usa con una condicion
                                //se le da el valor del id a docsinimp 
                                var detalle = _db.GS_Detalles.FirstOrDefault(po => po.Doc == documento);
                                detalle.Doc =items.ID;
                                _db.Entry(detalle).State = System.Data.Entity.EntityState.Modified;
                                ///--00
                                _db.Entry(items).State = System.Data.Entity.EntityState.Modified;
                                _db.SaveChanges();
                            }
                            else
                            {
                                return JsonError("Cuenta de imputacion no Existe");
                            }

                        }else
                        {


                            return JsonError("El siguente documento no contiene una imputacion sugerida.<br/><h5> - ID: "+items.ID+"<br/> - N° de factura: " 
                                + items.NumDocI+ "</h5> Por lo tanto, quedara en el modulo de Imputacion");
                        }
                        
                    }
                }
                
                return JsonExitoMsg("Proceso Realizado Existosamente");
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }


        }

        [HttpPost]
        [ClientAuthorize("MantOper")]
        public JsonResult getProveedor(string codaux)
        {
            try
            {
                bool success = true;
                var nomCorto = (from x in _db.Proveedores
                                where x.Aux.TrimEnd() == codaux
                                select x.Nom).FirstOrDefault();
                var model = (from s in _db.Docs
                             where s.CodAux == codaux
                             select s).FirstOrDefault();
                if (nomCorto == null)
                {
                    success = false;
                }

                if (success)
                {
                    var nom = model.NomAux;
                    var nomcort = nomCorto;
                    return Json(new { success = success, nombre = nom, nombrecorto = nomcort });
                }
                return Json(new { success = success });
            }
            catch (Exception e)
            {

                return JsonError("Error");
            }
        }

        [HttpPost]
        [ClientAuthorize("MantOper")]
        public JsonResult GetEditarManual(int rutClie)
        {
            try
            {
                var typ = "";
                var query = (from e in _db.Clientes where e.Rut == rutClie select new { nombreArc = e.Nombre_Archivo, ruta = e.Ruta, tipo = e.Tipo, id = e.ID, consi = e.Consi, ctd = e.CTD, impsug = e.impsug }).FirstOrDefault();
                typ = query.tipo == "0" ? typ = "afecto" : typ = "exento";
                var tiDocs = (from e in _db.GS_TDoc
                              where e.TipoCV == "R" || e.TipoCV == "C" && e.Tipo != null && ((typ == "exento" && e.Tipo != "A") || typ == "afecto")
                              select new
                              {
                                  id = e.CodDoc,
                                  nom = e.CodDoc + " " + e.DesDoc,
                                  impu = e.Impto
                              }).ToList();

                return Json(new { nombreArc = query.nombreArc, ruta = query.ruta, tipo = typ, id = query.id, consi = query.consi, ctd = query.ctd, impsug = query.impsug, tipos = tiDocs });
            }
            catch (Exception e)
            {
                return JsonError("ups hubo un error");
            }


        }

        //////////////carga tabla datatable
        [ClientAuthorize("MantOper")]
        public JsonResult CargaDocs()
        {
            try
            {
                var not_in = (from g in _db.GS_Documentos
                              where !(g.origen == null) && g.Entrega == null && g.recep == null
                              select g.ID).ToList();

                var model = (from ni in _db.Docs where ni.UsuEnt == null && ni.ting == "GS-SO" && ni.tst == null && !not_in.Contains(ni.ID) select ni
                          ).ToList();

                return Json(new { data = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}