using DTEXml;
using FacturadorElectronico.Facturador;
using FacturadorElectronico.Interfaces;
using FacturadorElectronico.Modelo;
using ModelChaoPdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace WebApplication.Areas.Utilidades.Controllers
{
    public class FacturacionController : MasterController
    {
        // GET: Facturacion/Facturacion
        [ClientAuthorize("MantFacturacion")]
        public ActionResult Index()
        {
            var clientes = (from clie in _db.Clientes
                            join per in _db.Permisos on clie.ID equals per.Clie
                            where clie.Activo == "S"
                            select new { ID = clie.ID, Nom = clie.Nombre_Archivo }
                                 ).OrderBy(x => x.Nom);
            ViewBag.clientes = new SelectList(clientes, "ID", "Nom");

            return View();
        }

        [ClientAuthorize("MantFacturacion")]

        public JsonResult getClientes(string q, string page_limit)
        {
            try
            {
                //var json = _db.sp_getClientes(q, Convert.ToInt32(page_limit)).ToList();
                var json = _db.sp_getProveFactu(q, Convert.ToInt32(page_limit)).ToList();
                return Json(new
                {
                    //data = data.ToList()
                    param = json.ToList()
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return JsonError("Ha ocurrido un error en la busqueda..." + e.Message);
            }

        }

        [HttpPost]
        [ClientAuthorize("MantFacturacion")]
        public JsonResult getDataClie(string id)
        {
            var query = _db.sp_getProveFactu(id, Convert.ToInt32(5)).FirstOrDefault();

            return Json(new { data = query }, JsonRequestBehavior.AllowGet);
        }

        //metodo antiguo con los clientes.
        //public JsonResult getDataClie(int id)
        //{
        //    var query = _db.Clientes.Where(x => x.ID == id);
        //    return Json(new { data = query.ToList() }, JsonRequestBehavior.AllowGet);
        //}


        [HttpPost]
        [ClientAuthorize("MantFacturacion")]
        public JsonResult saveInfo(string receptor, string detalle, string totales, string dscRcgGlobal)
        {
            try
            {

                Encabezado model = new Encabezado();

                Emisor _emisor = new Emisor()
                {
                    DirOrigen = "AV. EL BOSQUE CENTRAL 92 PISO 6",
                    CiudadOrigen = "SANTIAGO",
                    RUTEmisor = "76027048-2",
                    RznSoc = "PUENTE SUR OUTSOURCING S.A.",
                    GiroEmis = "PRESTACIÓN DE SERVICIOS DE CONTABILIDAD Y ADMINISTRACIÓN, ASESORÍA TRIBUTARIA",
                    Telefono = "24961000",
                    Acteco = 732000,
                    CmnaOrigen = "LAS CONDES",
                    Sucursal = "LAS CONDES"

                };

                var fechaHoy = DateTime.Now;
                var fechaFenc = fechaHoy.AddMonths(1);
                //var fechaHoy = DateTime.Now.ToString("yyyy-mm-dd");
                IdDoc _IdDoc = new IdDoc()
                {
                    TipoDTE = 33,
                    Folio = 0,
                    FchEmis = fechaHoy,
                    FchVenc = fechaFenc,
                    //MntBruto = 1 //si va en 1 se asume que es un monto bruto Si no se indica, se  asume los valores en montos Netos.
                };

                Documento docu = new Documento()
                {
                    xml_dte = ""
                };

                Transporte transporte = new Transporte();
               

                var _receptor = JsonConvert.DeserializeObject<List<Receptor>>(receptor).FirstOrDefault();
                var _detalle = JsonConvert.DeserializeObject<List<Detalle>>(detalle).ToList();
                var _totales = JsonConvert.DeserializeObject<List<Totales>>(totales).FirstOrDefault();
                model.Transporte = transporte;
                model.Documento = docu;
                model.Emisor = _emisor;
                model.Detalle = _detalle;
                model.Receptor = _receptor;
                model.Totales = _totales;
                model.IdDoc = _IdDoc;
                model.version = "1.0";
                model.TipoOperacion = "COM";

                //_db.Encabezado.Add(model);
                //_db.SaveChanges();
                facturadorPrueba(model);

                return JsonExitoMsg("Registro correcto.");


            }
            catch (Exception e)
            {
                return JsonError("Ha ocurrido un error..." + e.Message);
            }

        }

        private void facturadorPrueba(Encabezado model)
        {

            IFacturador facturador = new FacturacionCL(new Dictionary<string, string>() {
                {"Clave","plano91098" },
                {"Puerto","10033" },
                {"Rut","1-9" },
                {"Usuario","PSO" }
            });

            ConvertObjectToXml convert = new ConvertObjectToXml(typeof(ChaoPdfModelEntities)
                .GetProperties()
                .Where(prop => prop.PropertyType.IsGenericType)
                .Where(prop => prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(prop => prop.PropertyType.GenericTypeArguments.First())
                .Distinct().Where(p => p.Name != "Documento").ToList());


            var xmlString = convert.convertFromObjectToXml(model);
            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.LoadXml(xmlString);
            ResultFacturador x = facturador.Facturar(xml);

        }



    }
}