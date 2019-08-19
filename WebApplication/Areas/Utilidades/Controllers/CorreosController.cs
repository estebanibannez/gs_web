using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Net;
using System.Web.Mvc;
using System.Web.Configuration;
using WebApplication.Models;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class CorreosController : MasterController
    {
        // GET: Correos/Correos
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Correos()
        {
            return View();
        }


        [HttpPost, ValidateInput(false)]
        public JsonResult EnviarCorreo(string Destinatario, string CC, string Asunto, string MensajeCorreo, HttpPostedFileBase fileUploader)
        {
            try
            {
                WebApplication.Models.Correos Cr = new WebApplication.Models.Correos();
                MailMessage Mensaje = new MailMessage();
                if (Destinatario != "")
                {
                    string[] destinatarios = Destinatario.Split(';');
                    foreach (string destinatario in destinatarios)
                    {
                        Mensaje.To.Add(new MailAddress(destinatario.Trim()));
                    }
                }
                if (CC != "")
                {
                    string[] _cc = CC.Split(';');
                    foreach (string c in _cc)
                    {
                        Mensaje.CC.Add(CC.Trim());
                    }
                }
                //Correo de origen
                Mensaje.From = new MailAddress(WebConfigurationManager.AppSettings["AdminUser"],"PSO");
                //Mensaje.From = new MailAddress("it@pso.cl");
                Mensaje.Subject = Asunto;
                Mensaje.SubjectEncoding = System.Text.Encoding.UTF8;
                Mensaje.Body = MensajeCorreo;
                if (fileUploader != null)
                {
                    string fileName = Path.GetFileName(fileUploader.FileName);
                    Mensaje.Attachments.Add(new Attachment(fileUploader.InputStream, fileName));
                }
                Mensaje.BodyEncoding = System.Text.Encoding.UTF8;
                Mensaje.IsBodyHtml = true;

                /* Enviar */

                //smtp.Send(Mensaje);
                //smtp.Dispose();
                Cr.MandarCorreo(Mensaje);
                return JsonExitoMsg("Enviado");
            }
            catch (Exception ex)
            {
                return JsonError("No enviado");
            }

        }

        //[HttpPost, ValidateInput(false)]
        //[ValidateAntiForgeryToken]
        //[AllowAnonymous]
        //metodo envia correo asincrono prueba. con Helper.- (metodo que utiliza payroll) ,
        public async Task<ActionResult> EnviandoCorreo(string Destinatario, string CC, string Asunto, string MensajeCorreo, HttpPostedFileBase fileUploader)
        {
            try
            {
                MailMessage mail = new MailMessage();

                if (Destinatario != "")
                {
                    string[] destinatarios = Destinatario.Split(';');
                    foreach (string destinatario in destinatarios)
                    {
                        mail.To.Add(new MailAddress(destinatario.Trim()));
                    }
                }
                if (CC != "")
                {
                    string[] _cc = CC.Split(';');
                    foreach (string c in _cc)
                    {
                        mail.CC.Add(CC.Trim());
                    }
                }

                if (fileUploader != null)
                {
                    string fileName = Path.GetFileName(fileUploader.FileName);
                    mail.Attachments.Add(new Attachment(fileUploader.InputStream, fileName));
                }
                mail.From = new MailAddress(WebConfigurationManager.AppSettings["AdminUser"], "PSO");
                mail.Body = MensajeCorreo;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Subject = Asunto;
                mail.SubjectEncoding = System.Text.Encoding.UTF8;

                var C = App_Start.Helper.EnvioMail(mail);
             
                await Task.WhenAny(C);
                return JsonExitoMsg("Enviado");
            }
            catch (Exception e)
            {
                //App_Start.ErrorService.LogError(e);
                throw e;
                //return RedirectToAction("Login", "Account");
            }
        }
    }
}