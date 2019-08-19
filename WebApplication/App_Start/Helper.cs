using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Web.Mvc;
using WebApplication.Seguridad;
using System.Web.Security;
using WebApplicationMod;
using System.Dynamic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Web.Configuration;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Xml.Xsl;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.tool.xml;
using HtmlAgilityPack;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.css;
using System.Net.Http;
using System.Net.Http.Headers;

namespace WebApplication.App_Start
{
    public  class Helper : Controller
    {

        private GSPSOEntities _db;

        public Helper(GSPSOEntities _db) {
            this._db = _db;
        }

        public static string converRut(string rut, int opcion)
        {
            try
            {
                if (opcion == 1) //convierte de rut cualquiera a tipo ficha
                {
                    rut = rut.Replace(".", "").Replace("-", "");
                    var cant = rut.ToCharArray().Count();
                    var Rut = rut.Substring(0, cant - 1);
                    var flag = true;
                    while (flag)
                    {
                        flag = false;
                        if (Rut.StartsWith("0"))
                        {
                            Rut = Rut.Substring(1, cant - 2);
                            flag = true;
                        }
                    }
                    return Rut;
                }
                else if (opcion == 2) //convierte rut ingresado a formato softland
                {
                    var cant = rut.ToCharArray().Count();
                    while (cant < 13)
                    {
                        rut = rut.Insert(0, "0");
                        cant = rut.ToCharArray().Count();
                    }
                    return rut;
                }
                else // convierte rut a formato visible
                {
                    while (rut.StartsWith("0"))
                    {
                        rut = rut.Substring(1);
                    }
                    return rut;
                }
            }
            catch (Exception e)
            {
                ErrorService.LogError(e);
                return "";
            }
        }

        public string createFile(HttpPostedFileBase file, string tipo_documento, string cliente)
        {
           
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string fileName = new string(Enumerable.Repeat(chars, 5).Select(s => s[random.Next(s.Length)]).ToArray());
            DateTime dateTime = DateTime.Now;
            string year = dateTime.ToString("yyyy");
            string month = dateTime.ToString("MM");
            string relative_path = "";
            string path = "";
            string route = "";
            try
            {
                // if (!IsValidDocument(file)) { return null; }

                fileName = tipo_documento + "_" + dateTime.ToString("yyyyMMddHHmmssfff") + "_" + fileName + Path.GetExtension(file.FileName);
                relative_path = "~/Files/" + cliente + "/" + tipo_documento + "/" + fileName;
                path = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + cliente + "/" + tipo_documento + "/" + fileName);
                route = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + cliente + "/" + tipo_documento);
               
                Directory.CreateDirectory(route);
                var data = new byte[file.ContentLength];
                file.InputStream.Read(data, 0, file.ContentLength);
                using (var sw = new FileStream(path, FileMode.Create))
                {
                    sw.Write(data, 0, data.Length);
                }
                return relative_path;
            }
            catch(Exception ex)
            {
                ErrorService.LogError(ex);
                return null;
            }
        }


        private bool IsValidDocument(HttpPostedFileBase file)
        {
            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg", ".pdf", ".xls", ".xlsx", ".doc", ".docx" };
            string[] contentType = new string[] { "image",
                "application/vnd.ms-excel",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/pdf" };
            if (contentType.Any(item => file.ContentType.EndsWith(item, StringComparison.OrdinalIgnoreCase)) && 
                formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string digitoVerificador(Int64 rut)
        {
            Int64 Digito;
            int Contador;
            Int64 Multiplo;
            Int64 Acumulador;
            string RutDigito;

            Contador = 2;
            Acumulador = 0;

            while (rut != 0)
            {
                Multiplo = (rut % 10) * Contador;
                Acumulador = Acumulador + Multiplo;
                rut = rut / 10;
                Contador = Contador + 1;
                if (Contador == 8)
                {
                    Contador = 2;
                }
            }

            Digito = 11 - (Acumulador % 11);
            RutDigito = Digito.ToString().Trim();
            if (Digito == 10)
            {
                RutDigito = "K";
            }
            if (Digito == 11)
            {
                RutDigito = "0";
            }
            return (RutDigito);
        }


        public bool validaDV(string numVal, string modulo11)
        {
            Int64 numVal_int = 0;
            if(Int64.TryParse(numVal, out numVal_int))
            {
                if(digitoVerificador(numVal_int) == modulo11)
                {
                    return true;
                }
            }
            return false;
        }

        /*
         Nos permite recibir una lista anonima y poder pasarla a un modelo dinamico
         el cual se le pueden agregar mas modelos.
             */
        public static List<ExpandoObject> listAnonymousToDynamic(IQueryable info_cliente) {

            List<ExpandoObject> joinData = new List<ExpandoObject>();

            foreach (var item in info_cliente)
            {
                IDictionary<string, object> itemExpando = new ExpandoObject();
                foreach (PropertyDescriptor property
                         in
                         TypeDescriptor.GetProperties(item.GetType()))
                {
                    itemExpando.Add(property.Name, property.GetValue(item));
                }
                joinData.Add(itemExpando as ExpandoObject);
            }
            return joinData;
        }
        //envio mails funcionando la otra wa! ,se quita este..-
        public static async Task<bool> EnvioMail(MailMessage mail)
        {
            try
            {
                SmtpClient smtp = new SmtpClient();
                smtp.EnableSsl = false;
                ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("it", "00 It 11","puentesur");
                //smtp.Credentials = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["AdminUser"], System.Configuration.ConfigurationManager.AppSettings["AdminPassword"], "puentesur");
                smtp.Host = "192.168.0.31";
                smtp.Port = 25;
                
                await smtp.SendMailAsync(mail);
                return true;
            }
            catch(Exception) {
                return false;
            }
        }

        public static async Task SendEmail(string from, string to, string subject, string mensaje, string detalle)
        {
            try
            {
                //System.Threading.Tasks.TaskCanceledException token = new System.Threading.Tasks.TaskCanceledException();
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(to);
                mail.Subject = subject;

                SmtpClient client = new SmtpClient();
                client.EnableSsl = false;
                mail.IsBodyHtml = true;

                string htmlBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Shared/MailTemplate.xhtml"));
                mail.Body = htmlBody.Replace("|mensaje1", mensaje).Replace("|mensaje2", detalle);

                ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("it", "00 It 11", "puentesur");
                //client.Credentials = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["usermail"], System.Configuration.ConfigurationManager.AppSettings["passmail"], "puentesur");
                client.Host = "192.168.0.31";
                client.Port = 25;
                await client.SendMailAsync(mail);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
		
        public string XmlFo(string Docu)
        {

            XmlDocument xmlDocument = new XmlDocument();
            var foXsl = System.Web.Hosting.HostingEnvironment.MapPath("~/templates/factura.xsl");
            xmlDocument.LoadXml(Docu);
            var ms = new MemoryStream();


            var myXslTrans = new XslCompiledTransform();
            myXslTrans.Load(foXsl);
            myXslTrans.Transform(xmlDocument,null ,ms);
            ms.Position = 0;
            string result = Encoding.UTF8.GetString(ms.ToArray());

            return result;
            //return File(ms.ToArray(), "text/html", "Factura.html");

        }

        public string XmlFo4Pdf(string Docu)
        {

            XmlDocument xmlDocument = new XmlDocument();
            var foXsl = System.Web.Hosting.HostingEnvironment.MapPath("~/templates/facturaNoCSS.xsl");
            xmlDocument.LoadXml(Docu);
            var ms = new MemoryStream();


            var myXslTrans = new XslCompiledTransform();
            myXslTrans.Load(foXsl);
            myXslTrans.Transform(xmlDocument, null, ms);
            ms.Position = 0;
            string result = Encoding.UTF8.GetString(ms.ToArray());

            return result;
            //return File(ms.ToArray(), "text/html", "Factura.html");

        }


    }
}