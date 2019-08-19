using WebApplicationMod;
using System.Web.Mvc;
using System.Threading;
using System.Globalization;
using System.Web;
using System.Linq;
using System;

namespace WebApplication.Controllers
{

    public class HomeController : MasterController
    {
        [ClientAuthorize]
        public ActionResult Home()
        {
            var id_usu = SesionLogin().ID;
            var ruta = "";


            //ViewBag.rutaPhoto = ruta = "../Img/fotos2/" + SesionLogin().Sigla + ".jpg";
            ViewBag.clieTotal = _db.Clientes.Count(x => x.Activo == "S");
             
            //SesionLogin();
            //string var = HttpContext.User.Identity.Name;
            //var cumple = LoadData(" SELECT (nombre+ ' ' + appaterno + ' ' + apmaterno) as nombre_completo,fechaFiniquito, concat (year(GETDATE ()),'-',month(fechaNacimient),'-',day(fechaNacimient)) as fecha_nacimiento from [PSURSOFTSQL].[PSO].softland.sw_personal where fechaFiniquito > getdate() ");
            //var privilegio = LoadData(" SELECT  [id_usu],[Nom_usu],[pass_usu] ,[id_cliente],[id_tipo_usu],[email],[fechaIngreso] FROM[PAYROLL_PreProd].[dbo].[Usuario] where id_usu='13029615'");

            //ViewBag.TS = cumple;
            //ViewBag.cliente = SesionCliente().Nom_cor_emp;
            
            //SesionLogin();
            //Session.Add("privilegiado", privilegio[0]["id_usu"].ToString());

            //if (SesionLogin().id_usu== "13029615")
            //{
            //    return RedirectToAction("Index", "MantFechasCalendario", new { Area = "Mantencion" });
            //}
            
            var qry_clientes = (from clie in _db.Clientes
                                join per in _db.Permisos on clie.ID equals per.Clie
                                where clie.Activo == "S"
                                && per.Usu == id_usu
                                select new { ID = clie.ID, Nom = clie.Nombre_Archivo }
                     ).OrderByDescending(x => x.Nom).ToList();

            var clientes = new SelectList(qry_clientes, "ID", "Nom");
            var usuario = _db.Usu.FirstOrDefault(p => p.ID == id_usu);

            ViewBag.lista_usu = _db.Usu.Where( p => p.uactivo=="s").OrderBy(p => p.gergen).ToList();
            ViewBag.nomusu = usuario.Nom;
            ViewBag.sigusu = usuario.Sigla;
            ViewBag.clientes = clientes;


            ViewBag.unidad = SesionLogin().unidad;

            return View();
        }



        //Se Supone que va a cambiar el idioma
        [ClientAuthorize]
        public ActionResult ChangeLanguage(string lang)
        {
            if (lang != null)
            {

                //return View("Home");
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            }
            HttpCookie cookie = new HttpCookie("_lang");
            cookie.Value = lang;
            Response.Cookies.Add(cookie);
            return RedirectToLocal("");
        }

        [ClientAuthorize]
        public JsonResult ObtenerSaldoCuentas(int ID)
        {

            var nom_base = _db.Clientes.FirstOrDefault(x => x.ID == ID);
            var extrae_ruta = nom_base.Ruta.Split('\\').Last();

            var query = " SELECT  cwmovim.PctCod, cwpctas.PCDESC, format(Sum(cwmovim.MovDebe)  -Sum(cwmovim.MovHaber),''###,###,###,###,###'') as Saldo    FROM (softland.cwcpbte RIGHT JOIN softland.cwmovim ON (cwcpbte.CpbAno = cwmovim.CpbAno) AND (cwcpbte.CpbNum = cwmovim.CpbNum)) LEFT JOIN softland.cwpctas ON cwmovim.PctCod = cwpctas.PCCODI    where cwcpbte.CpbEst=''V'' AND cwpctas.PCCONB=''S'' AND cwcpbte.proceso <>''CW_GELPC'' GROUP BY cwmovim.PctCod, cwpctas.PCDESC, cwcpbte.CpbEst, cwpctas.PCCONB HAVING  (Sum(cwmovim.MovDebe) - Sum(cwmovim.MovHaber)) > 1 ";

            var Exec_SP = LoadData("exec SP_ExecuteQuery '" + extrae_ruta + "', '" + query + "'");
            return Json(new { data = Exec_SP }, JsonRequestBehavior.AllowGet);
        }





        public JsonResult ObtenerSaldoCuentasBanco()//Obtiene el saldo de las cuentas 
        {
            try { 
                var hoy = DateTime.Today;
                var list_saldo = _db.Saldo_Banco.Where(x => x.Fecha >= hoy && x.Corrientes == "Saldo contable").ToList().Select(s => new { filename = int.Parse(s.filename) , saldo = s.Corrientes_1, fecha = s.Fecha }); ;
                var list_SaldoBanco = (from banco in list_saldo
                                       join  clie in _db.Clientes on banco.filename equals clie.Rut
                                       select new { nom = clie.Nombre_Archivo, saldo = banco.saldo, fecha = banco.fecha }
                                        ).ToList();


                return Json(new { data = list_SaldoBanco }, JsonRequestBehavior.AllowGet);
            }catch(Exception e)
            {
                throw e;
            }
        }

    }
}