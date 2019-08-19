using System;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using WebApplicationMod;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Dynamic;

namespace WebApplication.Controllers
{
    public class AccountController : _Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) {
                return RedirectToLocal("");
            }
            Response.StatusCode = 295;
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Usu model, string returnUrl, bool RememberMe)
        {
            if (_db.Usu.Any(p => p.Sigla == model.Sigla && p.uactivo == "s")) {
                if (Membership.ValidateUser(model.Sigla, model.pass) || activeDirectoryAuthentication(model.Sigla, model.pass))
                {
                    FormsAuthentication.SetAuthCookie(model.Sigla, true);
                    return RedirectToLocal(returnUrl);
                }
            }
            ModelState.AddModelError("Sigla", "Las Credenciales proporcionadas no son Validas");
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult Perfil()
        {
            var id = (int)SesionLogin().ID;
            dynamic modelo = new ExpandoObject();


            var ruta = "";
            ViewBag.rutaPhoto = ruta = "../Img/fotos2/" + SesionLogin().Sigla + ".jpg";

            var usuario_ = (from u in _db.Usu
                           join uni in _db.parametros on u.unidad equals uni.id into unidad_usuario
                           join carg in _db.GS_Cargos on u.cargo equals carg.id into cargo_usuario
                           join area in _db.parametros on u.unidad equals area.id into area_usuario
                           from uni in unidad_usuario.DefaultIfEmpty()
                           from carg in cargo_usuario.DefaultIfEmpty()
                           from area in area_usuario.DefaultIfEmpty()
                           where u.ID == id
                           select new
                           {
                               area =  area.nom,
                               email = u.email,
                               nombre = u.Nom,
                               anexo = u.Anexo,
                               cargo= carg.nom
                           });


            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(usuario_);

            ViewBag.nomusu = _db.Usu.FirstOrDefault(x => x.ID == id).Nom;
            ViewBag.id = id;
            return View(modelo);
        }

        [HttpPost]
        [ClientAuthorize]
        public ActionResult LogOff()
        {                        
            var Usu = SesionLogin().ID;

            FormsAuthentication.SignOut();
            Session.Abandon();

            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            if (Request.Cookies["AuthCookieClient"] != null)
            {
                HttpCookie cookie2;
                cookie2 = Request.Cookies["AuthCookieClient"];
                cookie2[Usu.ToString()] = null;
                cookie2.Expires = DateTime.Now.AddYears(1);
                Response.Cookies.Add(cookie2);
            }

            SessionStateSection sessionStateSection = (SessionStateSection)WebConfigurationManager.GetSection("system.web/sessionState");
            HttpCookie cookie3 = new HttpCookie(sessionStateSection.CookieName, "");
            cookie3.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie3);
            return RedirectToLocal("");
        }

        [AllowAnonymous]
        public ActionResult changePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> changePassword(string usuario)
        {
            try
            {
                var usua = _db.Usu.FirstOrDefault(x => x.Sigla == usuario);
                if(!(usua == null))
                {
                    var mail = usua.email;
                    var pass = usua.pass;

                    if(!(mail == null || pass == null))
                    {

                        var mensaje = "Estimado Colaborador(a) </br> La clave, para el usuario: " + usua.Nom + " es la siguiente ''" + pass + "''</br>";
                        var detalle = "</br>Podrá acceder al sitio, con su usuario por defecto y clave recién solicitada.</br>Muchas gracias, por comunicarse con IT.</br>";
                        List<Task> sendTask = new List<Task>();
                        sendTask.Add(App_Start.Helper.SendEmail("it@pso.cl", mail, "Contraseña GS Web", mensaje, detalle));
                        await Task.WhenAny(sendTask);
                        return RedirectToAction("changePassword", "Account", new {  id = "ok" });
                    }
                    throw new Exception("No posee clave o email asociado a ese usuario");
                }
                else
                {
                    return RedirectToAction("changePassword", "Account", new { id = "ko" });
                }
            }
            catch(Exception e)
            {
                throw e;
            }            
        }



        private bool activeDirectoryAuthentication(string userName, string password) {
            bool value = false;
            using (DirectoryEntry _entry = new DirectoryEntry("LDAP://192.168.0.11:3268/OU=PSO,OU=PUENTE SUR,DC=PUENTESUR,DC=puente-sur,DC=com"))
            {
                try
                {
                    _entry.Username = userName;
                    _entry.Password = password;
                    DirectorySearcher _searcher = new DirectorySearcher(_entry);
                    _searcher.Filter = "(objectclass=user)";
                    SearchResult _sr = _searcher.FindOne();
                    string _name = _sr.Properties["displayname"][0].ToString();
                    value = true;
                }
                catch {
                    value = false;
                }
            }
            return value;
        }
    }
}