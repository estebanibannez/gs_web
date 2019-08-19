using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Seguridad;
using iTextSharp.text.pdf;
using System.Data.SqlClient;
using System.Data;
using WebApplication.App_Start;
using System.Text.RegularExpressions;
using System.Web.Routing;
using System.IO;
using iTextSharp.text;
using AutenticacionPersonalizada.Utilidades;
using System.Drawing.Imaging;
using WebApplicationMod;
using iText.Html2pdf;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Xml;
using ModeloCRM;

namespace WebApplication
{
    public class _Controller : Controller
    {
        protected GSPSOEntities _db = new GSPSOEntities();
        protected GSPSOCRMEntities _db2 = new GSPSOCRMEntities();

        protected WebApplicationMod.Usu SesionLogin()
        {
            if ((Session["usuario"] as WebApplicationMod.Usu) == null)
            {
                IdentityPersonalizado mu = HttpContext.User.Identity as IdentityPersonalizado;
                Session["usuario"] = mu.user;
            }
            return Session["usuario"] as WebApplicationMod.Usu;
        }

        protected List<string> SesionLoginPermisos()
        {
            if ((Session["SesionLoginPermisos"] as List<string>) == null)
            {
                IdentityPersonalizado mu = HttpContext.User.Identity as IdentityPersonalizado;
                Session["SesionLoginPermisos"] = mu.roles;
            }
            return Session["SesionLoginPermisos"] as List<string>;
        }

        protected List<WebApplicationMod.Clientes> SesionClientesPermi()
        {
            if ((Session["SesionClientesPermi"] as List<WebApplicationMod.Clientes>) == null)
            {
                IdentityPersonalizado mu = HttpContext.User.Identity as IdentityPersonalizado;
                Session["SesionClientesPermi"] = mu.clientsUser;
            }
            return Session["SesionClientesPermi"] as List<WebApplicationMod.Clientes>;
        }

        //protected Usu SesionEmpresas()
        //{
        //    if ((Session["usuario"] as Usu) != null)
        //    {

        //        UsuarioMembership mu = (UsuarioMembership)Membership.GetUser(HttpContext.User.Identity.Name, false);
        //        var usuario = SesionLogin().ID;

        //        Session["empresas"] = (from clie in _db.Clientes
        //                              join per in _db.Permisos on clie.ID equals per.Clie
        //                              where clie.Activo == "S"
        //                              && per.Usu == usuario
        //                              select new { Rut = clie.Rut, Nom = clie.Nombre_Archivo }).ToList();
        //    }
        //    return Session["empresas"] as Usu;
        //}

        protected ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Home", "Home", new {area = "", returnUrl = returnUrl });
        }
    }

    [CustomLayoutAjax]
    public class MasterController : _Controller
    {
        protected Helper helper;

        public MasterController()
        {
            helper = new Helper(_db);
        }

        protected List<Dictionary<string, object>> LoadData(string sqlSelect, params object[] sqlParameters)
        {
            var table = new List<Dictionary<string, object>>();
            _db.Database.Connection.Open();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sqlSelect;
                cmd.CommandTimeout = 600;
                foreach (var param in sqlParameters)
                    cmd.Parameters.Add(param);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader[i];
                        table.Add(row);
                    }
                }
            }
            _db.Database.Connection.Close();
            return table;
        }
        protected JsonResult JsonExitoMsg(string msg)
        {
            return Json(new { success = true, error = false, mensaje = msg }, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult JsonExitoValor(string msg,string numero)
        {
            return Json(new { success = true, error = false, mensaje = msg , numero = numero}, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult JsonExito()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        protected JsonResult JsonError(string msg)
        {
            return Json(new { success = false, error = true, mensaje = msg }, JsonRequestBehavior.AllowGet);
        }

        protected ActionResult getFile(int id_doc, string type, string client, string fecha, string id_usu)
        {
            string value = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string modulo11 = helper.digitoVerificador(Int64.Parse(value));
            string code = SeguridadUtilidades.Encriptar(value + ":" + modulo11 + ":" + id_doc.ToString() + ":" + type + ":" + client + ":" + fecha + ":"+id_usu+":") ;
            RouteValueDictionary values = new RouteValueDictionary();
            values.Add("code", HttpUtility.UrlEncode(code));
            values.Add("Area", "");
            ViewBag.Routes_values = values;
            if (type.Equals("other"))
            {
                ViewBag.MimeType = "other";
            }
            else
            {
                ViewBag.MimeType = "application/pdf";
            }
            return View("getFile");
        }


        [AllowAnonymous]
        public ActionResult getFileData(string code)
        {
            var code_array = SeguridadUtilidades.Desencriptar(HttpUtility.UrlDecode(code)).Split(':');
            string validacion = code_array[0];
            string modulo11 = code_array[1];
            if (!helper.validaDV(validacion, modulo11)) { return null; }
            int id_doc = Int32.Parse(code_array[2]);
            string type = code_array[3];

            MemoryStream ms = new MemoryStream();        

            try
            {
                switch (type)
                {
                    case "other":

                        var documento = _db.GS_Documentos.FirstOrDefault(p => p.ID == id_doc);
                        Response.AppendHeader("Content-Disposition", "inline; filename=" + documento.Archivo);
                        var Doc_file = (documento.Archivo).Replace(@"P:\", @"\\atenea\puentesur\"); //Replace (Ubicaion de los archivos en el servidor disco P:\\atenea\puentesur\).                       
                        using (FileStream file = new FileStream(Doc_file, FileMode.Open, FileAccess.Read))
                        {
                            byte[] bytes = new byte[file.Length];
                            file.Read(bytes, 0, (int)file.Length);
                            ms.Write(bytes, 0, (int)file.Length);
                        }

                        return File(ms.ToArray(), MimeMapping.GetMimeMapping(documento.Archivo));                       
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                return Json(ErrorService.LogError(e));
            }
        }


        protected int PA_Almacenado(List<SqlParameter> parametros, string sp_almacenado)
        {
            int result = new int();
            _db.Database.Connection.Open();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sp_almacenado;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddRange(parametros.ToArray());
                cmd.Parameters["@counts"].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                result = (int)cmd.Parameters["@counts"].Value;
            }
            _db.Database.Connection.Close();
            return result;
        }

        protected int PA_AlacenadoNoReturn(List<SqlParameter> parametros, string sp_almacenado)
        {
            int result = new int();
            _db.Database.Connection.Open();
            using (var cmd = _db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = sp_almacenado;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddRange(parametros.ToArray());
                //cmd.Parameters["@counts"].Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
            }
            _db.Database.Connection.Close();
            return result;
        }




        [AllowAnonymous]
        public ActionResult DocuXml(int idCab)
        {

            try { 

                var Org_XML = _db.Documento.FirstOrDefault(z => z.id_cab == idCab).xml_dte.ToString();

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Org_XML);
                var resc_dte = doc.GetElementsByTagName("DTE")[0];
                
                XmlDocument NuevoDoc = new XmlDocument();

                XmlNode importNewsItem = NuevoDoc.ImportNode(resc_dte, true);
                NuevoDoc.AppendChild(importNewsItem);

                var xml_string = NuevoDoc.OuterXml;

                var html =  helper.XmlFo(xml_string.Replace("http://www.sii.cl/SiiDte", "").Replace("<SetDTE>", "").Replace("</SetDTE>", ""));


                return Content(html);
            }
            catch(Exception)
            {
                return null;
            }         
        }

        [AllowAnonymous]
        public ActionResult DTE(int idCab, int id_c, string tipo_doc, int monto, string cre)
        {

            try
            {
                DateTime fecha = DateTime.ParseExact(cre, "yyyyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture);

                var valida = _db.GS_Documentos.FirstOrDefault(a => a.CLIENTE == id_c && a.CTDCod == tipo_doc && a.Total == monto && a.id_encab == idCab && a.Creacion.Value.Year == fecha.Year
                && a.Creacion.Value.Month == fecha.Month && a.Creacion.Value.Day == fecha.Day && a.Creacion.Value.Hour == fecha.Hour && a.Creacion.Value.Minute == fecha.Minute
                );

                if ( valida != null) { 
                var Org_XML = _db.Documento.FirstOrDefault(z => z.id_cab == idCab).xml_dte.ToString();

                var html = helper.XmlFo(Org_XML.Replace("xmlns=\"http://www.sii.cl/SiiDte\" ", "").Replace("<SetDTE>", "").Replace("</SetDTE>", ""));

                return Content(html);
                }
                else
                {
                    return JsonError("Documento no encontrado");
                }

            }
            catch (Exception)
            {
                return null;
            }


        }

        [AllowAnonymous]
        public ActionResult DocuXmlPDF(int idCab)
        {

            try
            {

                var Org_XML = _db.Documento.FirstOrDefault(z => z.id_cab == idCab).xml_dte.ToString();
                var html = helper.XmlFo4Pdf(Org_XML.Replace("xmlns=\"http://www.sii.cl/SiiDte\" ", "").Replace("\r\n", ""));

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Org_XML);
                var Ted= doc.GetElementsByTagName("TED")[0].OuterXml;
                MemoryStream ms = new MemoryStream();

                // Convierto el HTML en PDF 
                ConverterProperties properties = new ConverterProperties();
                properties.SetBaseUri("http://gs.pso.cl/Master/DocuXml?idCab=4923");
                HtmlConverter.ConvertToPdf(html, ms, properties);

                // Genero el PDF417
                MemoryStream msIMG = new MemoryStream();
                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(ms.ToArray());
                PdfStamper stamper = new PdfStamper(reader, msIMG);
                int n = reader.NumberOfPages;
                var msIMG2 = GenerateBarCodeZXing(Ted);
                //Inserto la imagen del PDF417
                iTextSharp.text.Image QR = iTextSharp.text.Image.GetInstance(msIMG2, ImageFormat.Png);
                QR.SetAbsolutePosition(10, 200);
                stamper.GetOverContent(1).AddImage(QR);
                stamper.FormFlattening = false;



                //msIMG.Flush(); // Don't know if this is necessary
                //msIMG.Position = 0;
                //msIMG.Close();
                
                //iTextSharp.text.pdf.PdfReader reader2 = new iTextSharp.text.pdf.PdfReader(msIMG.ToArray());

                MemoryStream msMargen = new MemoryStream();
                //WriterProperties writerProperties = new WriterProperties();
                Document doc2 = new Document(iTextSharp.text.PageSize.LETTER, 200, 0, 100, 0);
                iTextSharp.text.pdf.PdfWriter pdfWriter = iTextSharp.text.pdf.PdfWriter.GetInstance(doc2, msMargen);



                doc2.Open();
                pdfWriter.Open();
                PdfContentByte cb = pdfWriter.DirectContent;

                var page =pdfWriter.GetImportedPage(reader, 1);

                cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);

                //pdfWriter.SetMargins(0, 0, 0, 0);
                //pdfWriter.Close();

                //ms.Close();
                //msIMG.Close();
                doc2.Close();
                stamper.Close();
                reader.Close();
               
                return File(msIMG.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            catch (Exception)
            {                
                return null;
            }


        }

        private Bitmap GenerateBarCodeZXing(string data)
        {

            //IBarcodeReader reader = new ZXing.BarcodeReader();
            var writer = new ZXing.BarcodeWriter
            {
                //ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H,
                Format = BarcodeFormat.PDF_417,
                Options = new EncodingOptions { Height = 90, Width = 90, Margin = 10  }, //optional, 
                

            };

           // writer.Options.dim

            writer.Renderer = new ZXing.Rendering.BitmapRenderer();
            var imgBitmap = writer.Write(data);
            var size = new Size(450, 109);

            Bitmap original = new Bitmap(imgBitmap, size);

            using (var stream = new MemoryStream())
            {
                imgBitmap.Save(stream, ImageFormat.Jpeg);
                return original;
            }
        }

        [HttpGet]
        [ActionName("XAMARIN_Login")]
        [AllowAnonymous]
        // GET: api/Login/5  
        public ActionResult Xamarin_login(string username, string password)
        {
            var user = _db.Usu.Where(x => x.Sigla == username && x.pass == password).FirstOrDefault();
            if (user == null)
            {
                return HttpNotFound();
               // return Request.CreateResponse(HttpStatusCode.Unauthorized, "Please Enter valid UserName and Password");
            }
            else
            {

                return null;
            }
        }

        [HttpGet]
        [ActionName("Xamarin_RendicionUP")]
        // GET: api/Login/5  
        public ActionResult Xamarin_RendicionUP(string username, string password)
        {
            var user = _db.Usu.Where(x => x.Sigla == username && x.pass == password).FirstOrDefault();
            if (user == null)
            {
                return HttpNotFound();
                // return Request.CreateResponse(HttpStatusCode.Unauthorized, "Please Enter valid UserName and Password");
            }
            else
            {

                return null;
            }
        }

        [HttpGet]
        [ActionName("XAMARIN_EQUIPO")]
        // GET: api/Login/5  
        public ActionResult Xamarin_EQUIPO(int ID)
        {
            try
            {
                var clieAsignados = (from param in _db.parametros
                                     join usu in _db.Usu on param.id equals usu.unidad
                                     join car in _db.GS_Cargos on usu.cargo equals car.id
                                     join asig in _db.GS_Asig_Carg_Clie on usu.ID equals asig.usu
                                     where asig.clie == ID && usu.uactivo == "S"
                                     select new
                                     {
                                         nombreUsuario = usu.Nom,
                                         nombreCargo = car.nom,
                                         nombreUnidad = param.nom,
                                         sigla= usu.Sigla
                                     }).ToList();
                return Json(clieAsignados , JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return JsonError("");
            }
        }        
    }
}