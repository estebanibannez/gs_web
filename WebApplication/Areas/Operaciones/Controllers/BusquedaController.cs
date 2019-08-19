using AutenticacionPersonalizada.Utilidades;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplicationMod;

namespace WebApplication.Areas.Operaciones.Controllers
{
    public class BusquedaController : MasterController
    {
        // GET: Busqueda/Busqueda
        [ClientAuthorize("MantBusq")]
        public ActionResult Index()
        {
            var id_usu = SesionLogin().ID;

            var qry_clientes = (from clie in _db.Clientes
                                join per in _db.Permisos on clie.ID equals per.Clie
                                where clie.Activo == "S"
                                && per.Usu == id_usu
                                orderby clie.Nombre_Archivo
                                select new { Rut = clie.Rut, Nom = clie.Nombre_Archivo }).ToList();

            var rutcliente = new SelectList(qry_clientes, "Nom", "Nom");
            var provedores = new SelectList(_db.Proveedores, "Aux", "Nom");

            ViewBag.rutcliente = rutcliente;
            ViewBag.provedores = provedores;
            
            return View();
        }
        [ClientAuthorize("MantBusq")]
        public ActionResult consulta(string cliente, string auxiliar, string numdoc, string nfolio, string tipo, string inicio, string fin)
        {
           
            var cond4 = "";
            var cond5 = "";

            try
            {
                var cond1 = (cliente != "") ? "AND cliente =N'" + cliente + "'" : "";
                var cond2 = (auxiliar != "") ? "AND auxiliar LIKE'%" + auxiliar + "%'" : "";
                var cond3 = (numdoc != "") ? "AND NumDocI =" + numdoc + "" : "";
                var cond6 = (nfolio != "") ? "AND FolioSolFac=" + nfolio + "" : "";

                if (inicio != "" && fin != "")
                {
                    var _ini = Convert.ToDateTime(inicio).ToString("yyyy-MM-dd");
                    var _fin = Convert.ToDateTime(fin).ToString("yyyy-MM-dd");
                    cond4 = "AND periodo_imputacion >= CONVERT(DATETIME,'" + _ini + " 00:00:00',102) AND periodo_imputacion <= CONVERT(DATETIME,'" + _fin + " 23:59:59',102)";
                }

            switch (tipo)
            {
                case "all":
                    cond5 = "";
                    break;
                case "compras":
                    cond5 = "AND (TipoCV =N'C' OR TipoCV = N'R')";
                    break;
                case "ventas":
                    cond5 = "AND TipoCV ='V'";
                    break;
                default:
                    Console.WriteLine("Error tipo");
                    break;
            }
               
                var SQL = LoadData("SELECT * FROM consulta_documentos WHERE (ID <> 0) AND (NOT(cliente is NULL)) " + cond1 + " " + cond2 + " " + cond3 + " " + cond4 + " " + cond5 + " " + cond6 + " ORDER BY cliente, MovFe desc");
                var JsonReturn = Json(new { data = SQL.ToList() }, JsonRequestBehavior.AllowGet);
                JsonReturn.MaxJsonLength = Int32.MaxValue;
                
                return JsonReturn;

            }
            catch (Exception e) {

                return JsonError("Ocurrió un problema con su solicitud");
           
            }

        }
        [ClientAuthorize("MantBusq")]
        public ActionResult DetallePagos(int ID)
        {
            Console.WriteLine(ID);
            try
            {
                var SQL = LoadData("SELECT * FROM detallepagos WHERE Org =" + ID + "");
            }
            catch(Exception e) {
                JsonError("Error en detalle del pago.");
            }
              return JsonError("ha ocurrido un problema con su solicitud");
        }
        [ClientAuthorize("MantBusq")]
        public ActionResult detallecontabilizacion(string id) {
            try
 
            {
                //child 
                var SQL = LoadData("SELECT * FROM detalle_contabilizacion WHERE ([Folio GS] = " + id + ")");
                //child II
                var SQL2 = LoadData("SELECT * FROM detallepagos WHERE Org =" + id + "");
              
                return Json(new { data = SQL.ToList(), data2 = SQL2.ToList() }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return JsonError("Ocurrió un problema con su solicitud");
            }
        }
        [ClientAuthorize("MantBusq")]
        public ActionResult PDF(string id)
        {
            try
            {
                var doc = Int32.Parse(id);


                    return getFile(doc,"other", "", "", "");
                
            }
            catch (Exception e)
            {
                return JsonError("ha ocurrido un problema con su solicitud");
            }
        }

        [ClientAuthorize("MantBusq")]
        public ActionResult ArchivoAnticipo(string id)
        {
            try
            {
                var doc = Int32.Parse(id);

                var idAnticipo = LoadData("select ca.id_anticipo from GS_Documentos gd inner join cab_anticipos ca  on gd.CLIENTE = ca.rut_clie and gd.MovFe = ca.fec_anticipo and gd.NumCent = ca.CpbNum  where ID=" + doc + "");

                var x = idAnticipo.FirstOrDefault()["id_anticipo"].ToString();

                return RedirectToAction("Archivo", "Anticipo", new {area= "Anticipo", ID = x });


            }
            catch (Exception e)
            {
                return JsonError("ha ocurrido un problema con su solicitud");
            }
        }

    }
}
    