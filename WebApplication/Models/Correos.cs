using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace WebApplication.Models
{
    public class Correos
    {
        SmtpClient smtp = new SmtpClient();
        public Correos()
        {
            try
            {

                smtp.EnableSsl = true;
                ServicePointManager.ServerCertificateValidationCallback = (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["AdminUser"], System.Configuration.ConfigurationManager.AppSettings["AdminPassword"]);
               
                //--- credeciales con sobrecarga "agrego el dominio" ----//
                //smtp.Credentials = new NetworkCredential(System.Configuration.ConfigurationManager.AppSettings["AdminUser"], System.Configuration.ConfigurationManager.AppSettings["AdminPassword"], "puentesur");
               
                //---- Probando correos con Gmail ----//
                //smtp.Host = "smtp.gmail.com";
                //smtp.Port = 587;

                //------Correo con Puente sur----//
                smtp.Host = "192.168.0.31";
                smtp.Port = 25;

                //----saco el host y el puerto desde WEB.config ---//
                //smtp.Host = WebConfigurationManager.AppSettings["SMTPName"];
                //smtp.Port = int.Parse(WebConfigurationManager.AppSettings["SMTPPort"]);
            }
            catch (Exception e)
            {
            }

        }

        public void MandarCorreo(MailMessage Mensaje)
        {
            smtp.Send(Mensaje);
            smtp.Dispose();
        }
    }
}