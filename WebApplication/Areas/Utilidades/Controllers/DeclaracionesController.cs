using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class DeclaracionesController : MasterController
    {
        // GET: Declaraciones/Declaraciones
        [ClientAuthorize("MantDecl")]
        public ActionResult Index()
        {
            var cliPass = (from clie in _db.Clientes
                           join pass in _db.GS_Pass on clie.ID equals pass.Clie
                           where pass.Orga == "SII"
                           orderby clie.Nom
                           select clie).ToList();
            var list = new SelectList(cliPass, "ID", "Nom");
            var id_usu = SesionLogin().ID;
            ViewBag.ListaClientes = list;
            return View("Index");
        }

        [HttpPost]
        [ClientAuthorize("MantDecl")]
        public async Task<JsonResult> GetToken() {
            var x = await PostFormUrlEncoded("http://api.puente-sur.cl/Authorize", new Dictionary<string, string> { { "UserName", "PSO" }, { "Password", "zm7P#637Yp6N+)R" }, { "grant_type", "password" } }).
                ContinueWith((httpResponseMessage) =>
                    {
                        return JObject.Parse(httpResponseMessage.Result.Content.ReadAsStringAsync().Result);
                    }
                );
            try {
                return Json(new Dictionary<string, string> { { "access_token", x["access_token"].ToString() }, { "token_type", x["token_type"].ToString() }, { "expires_in", x["expires_in"].ToString() } });
            } catch {
                return JsonError("Error de Authentificacion");
            }
            
        }


        private static async Task<HttpResponseMessage> PostFormUrlEncoded(string url, IEnumerable<KeyValuePair<string, string>> postData)
        {
            using (var httpClient = new HttpClient())
            {
                using (var content = new FormUrlEncodedContent(postData))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    return await httpClient.PostAsync(url, content);
                }
            }
        }
    }
}