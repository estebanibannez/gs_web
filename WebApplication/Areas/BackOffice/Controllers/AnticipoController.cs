using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.BackOffice.Controllers
{
    public class AnticipoController : MasterController
    {
        [ClientAuthorize("MantAnti")]
        public ActionResult Index()
        {
            dynamic modelo = new ExpandoObject();
            //var datos = _db.cab_anticipos.Where(p =>p.estado!= "eliminado").ToList();       
            var usu = SesionLogin().ID;
            var datos = (from clie in _db.Clientes
                         join per in _db.Permisos on clie.ID equals per.Clie
                         join caba in _db.cab_anticipos on clie.Rut equals caba.rut_clie
                         join deta in _db.det_anticipos on caba.id_anticipo equals deta.id_anticipo into gdet
                         join prov in _db.Proveedores on caba.id_prov equals prov.Aux
                         where clie.Activo == "S" && caba.estado != "eliminado"
                         && per.Usu == usu
                         select new
                         {
                             id_anticipo = caba.id_anticipo,
                             Nombre_Archivo = clie.Nombre_Archivo,
                             //rut_cliente = clie.Rut,
                             Rut_proveedor= caba.id_prov,
                             Nom_proveedor= prov.Nom,
                             Glosa= caba.glosa,
                             usuario= caba.usu_sol,
                             monto= gdet.Sum(r => r.monto),
                             comprobante = caba.CpbNum,
                             fec_pago = caba.fec_pago,
                             fec_sol = caba.fec_sol,
                             estado = ((caba.fec_pago == null)? "Impagos" : "Pagados")
                         }
                         
                );

            modelo.listAnonymousToDynamic = WebApplication.App_Start.Helper.listAnonymousToDynamic(datos);

            ViewBag.contadorActivo = datos.Count(p => p.estado == "Pagados");
            ViewBag.contadorInactivo = datos.Count(p => p.estado == "Impagos");
              

            // Solo las tesoreras pueden apobar cargo 19
            ViewBag.unidad = SesionLogin().unidad;
            ViewBag.cargo = SesionLogin().cargo;


            return View(modelo);
        }
        [ClientAuthorize("MantAnti")]
        public ActionResult Create()
        {
            var usu = SesionLogin().ID;
            var qry_clientes = ( from clie in _db.Clientes   
                                 join per in _db.Permisos on clie.ID equals per.Clie
                                 where clie.Activo=="S"
                                 && per.Usu == usu
                                 select new { ID = clie.ID, Nom = clie.Nombre_Archivo }  
                                 ).OrderBy(x => x.Nom);
            var clientes = new SelectList(qry_clientes, "ID","Nom");
            var provedores = new SelectList(_db.Proveedores, "Aux", "Nom");

            ViewBag.cta = new SelectList(_db.Tipo_Anticipos, "id_tanticipo", "nom_tanticipo");
            ViewBag.clientes = clientes;
            ViewBag.provedores = provedores;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantAnti")]
        public ActionResult Create(cab_anticipos model, det_anticipos[] model_det , HttpPostedFileBase[] files,string id_prov, string NumDoccb,string CC)
        {
            try
            {
                if (model == null || model_det == null || files == null) { throw new Exception("Problemas al Cargar la información"); }

                int id_tipoanticipo = int.Parse( model_det[0].cod_cta);
                model.files_anticipos = new List<files_anticipos>();
                var cliente = _db.Clientes.FirstOrDefault(y => y.ID == model.id_clie);
                var extrae_ruta = cliente.Ruta.Split('\\').Last();

                foreach ( var item in files)
                {
                    model.files_anticipos.Add(new files_anticipos()
                    {
                        fecha_mod = DateTime.Now,
                        estado = "A",
                        usu_mod = SesionLogin().Sigla,
                        ruta = helper.createFile(item, "ANT", model.id_clie.ToString())
                    });
                }

                //model.det_anticipos = new List<det_anticipos>();
                //foreach (var item in model_det)
                //{
                //    model.det_anticipos.Add(new det_anticipos() { });
                //}
                // _db.cab_anticipos.Add(model);
                // _db.SaveChanges();

                string numdoc = NumDoccb == null? DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString(): NumDoccb;
                //double.TryParse(DateTime.Now.Day.ToString() + DateTime.Now.Month.ToString(), out numdoc);
                //var cta_cod = _db.Tipo_Anticipos.FirstOrDefault(x => x.id_tanticipo == Cta);
                ObjectParameter cpbnum2 = new ObjectParameter("return_CpbNum", "nvarchar(256)");

                //_db.PA_insAnticipos("QPX", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), cta_cod.TtdCod, "1-1-1-05-40", cta_cod.CtaC, DateTime.Now.ToString("yyyy-MM-dd"), model.monto.ToString(), cpbnum2   );
                //_db.PA_insAnticipos("QPX", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), cta_cod.TtdCod, cta_cod.CtaC, "2-1-1-04-01", DateTime.Now.ToString("yyyy-MM-dd"), model_det.monto.ToString(), numdoc, cpbnum2);

                model.CpbNum = cpbnum2.Value.ToString();
                //model.id_tanticipo = Cta;
                model.fec_anticipo = model.fec_anticipo;
                model.estado = "ingresado";
                //model.Cod_cta= cta_cod.CtaC;
                model.usu_sol = SesionLogin().Sigla;
                model.fecha_mod = DateTime.Now;
                model.usu_mod = SesionLogin().Sigla;
                model.rut_clie = _db.Clientes.FirstOrDefault(y => y.ID== model.id_clie).Rut;
                model.fec_sol = DateTime.Now;
                //model.evidencia = helper.createFile(files,"ANT",model.id_clie.ToString());

                foreach (det_anticipos i in model_det)
                {
                    int dcta = int.Parse(i.cod_cta);
                    var dcta_cod = _db.Tipo_Anticipos.FirstOrDefault(x => x.id_tanticipo == dcta);
                    //i.cab_anticipos = model;
                    i.fecha_mod = DateTime.Now;
                    i.cod_cta = dcta_cod.CtaC;
                    i.TtdCod = dcta_cod.TtdCod;
                    i.usu_mod = SesionLogin().Sigla;
                    model.det_anticipos.Add(i);
                }
                if (ModelState.IsValid)
                {
                    _db.cab_anticipos.Add(model);
                    //_db.det_anticipos.Add(model_det[0]);

                    var id_pro = id_prov.Trim();
                    var nom = SesionLogin().nomususoft.Trim();
                    var provedor = _db.Proveedores.FirstOrDefault(x => x.Aux == id_pro);
                    var rut = Int32.Parse(id_prov.Trim());
                    var completo = rut + "-" + Digito(rut);
                    var nombre = provedor.Nom.Trim();
                    var mail = provedor.Email == null ? "null" : provedor.Email.Trim();   
                    //var mes = DateTime.Now.Month.ToString("00");
                    var mes = model.fec_anticipo.Month.ToString("00");

                    List<SqlParameter> lista_provedor = new List<SqlParameter>()
                    {
                        new SqlParameter("@Cliente", System.Data.SqlDbType.NVarChar) { Value = extrae_ruta},
                        new SqlParameter("@CodAux", System.Data.SqlDbType.NVarChar) { Value = id_pro},
                        new SqlParameter("@NomAux", System.Data.SqlDbType.NVarChar) { Value = nombre},
                        new SqlParameter("@RutAux", System.Data.SqlDbType.NVarChar) { Value = completo},
                        new SqlParameter("@Email", System.Data.SqlDbType.NVarChar) { Value = mail},
                        new SqlParameter("@Usuario", System.Data.SqlDbType.NVarChar) { Value = nom}
                    };

                    var valida = PA_AlacenadoNoReturn(lista_provedor, "PA_validaAntesInsertarDinamico");
                    
                    _db.SaveChanges();
                    var var_tipoAnt = _db.Tipo_Anticipos.FirstOrDefault(a => a.id_tanticipo== id_tipoanticipo);
                    var cuenta_prov = _db.Clientes.FirstOrDefault(y => y.ID == model.id_clie).IRFS == "N" ? "2-1-02-01": "2-1-1-04-01";

                    //_db.PA_insAnticipos("QPX", DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), "AN", "1-1-1-01-01", "2-1-1-04-01", DateTime.Now.ToString("yyyy-MM-dd"), "0", numdoc,model.id_anticipo.ToString(), cpbnum2);
                    _db.PA_insAnticipos(extrae_ruta, model.fec_anticipo.Year.ToString(), mes, var_tipoAnt.TtdCod, var_tipoAnt.CtaC, cuenta_prov, DateTime.Now.ToString("yyyy-MM-dd"), "0", numdoc, model.id_anticipo.ToString(), cpbnum2);
                    var CpbNum = cpbnum2.Value.ToString();
                    //_db.cab_anticipos.Attach(model);
                    //_db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    //_db.SaveChanges();

                    return JsonExitoValor("Comprobante", CpbNum);
                }

                    return JsonError("Hey !!! hubo un problema.");
            }
            catch (DbEntityValidationException e)
            {
                // MvcApplication.LogError(e);
                return JsonError("Hey !!! hubo un problema.");
            }
            catch (Exception e)
            {
                // MvcApplication.LogError(e);
                return JsonError(e.Message);
            }
        }
        [ClientAuthorize("MantAnti")]
        public JsonResult get_provedores(string query)//Busca los provedores por rut en la vista
        {

            var data = from prov in _db.Proveedores
                       where prov.Aux.Contains(query)
                       || prov.Nom.Contains(query)
                       select new
                       { value= prov.Aux, data=prov.Nom.Trim() };

            var jsonString = JsonConvert.SerializeObject(data);
            
            //var myObject = JsonConvert.DeserializeObject<>(jsonString);
            return Json(new
            {
                //data = data.ToList()
                param = data.ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [ClientAuthorize("MantAnti")]
        public ActionResult Archivo(int ID)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                var documento = _db.files_anticipos.FirstOrDefault(p => p.id_anticipo == ID);
            using (FileStream file = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(documento.ruta), FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[file.Length];
                file.Read(bytes, 0, (int)file.Length);
                ms.Write(bytes, 0, (int)file.Length);
            }
            return File(ms.ToArray(), MimeMapping.GetMimeMapping(System.Web.Hosting.HostingEnvironment.MapPath(documento.ruta)), documento.ruta.Split('\\').Last());

                //var file = IYourService.GetFile(fileId);
                //return File(file.FileContents, file.ContentType, file.FileName);
            }
            catch (Exception e)
            {
                return JsonError("Hey !!! hubo un problema.");
            }
        }
        [ClientAuthorize("MantAnti")]
        public ActionResult delete(int ID)
        {
            try
            {
                var model = _db.cab_anticipos.FirstOrDefault(p => p.id_anticipo == ID);
                if (model.fec_pago == null)
                {
                    var gs = _db.GS_Documentos.FirstOrDefault(u => u.ID == model.NumDoc);
                    if ( gs!= null) { 
                        _db.Entry(gs).State = System.Data.Entity.EntityState.Deleted;
                        _db.SaveChanges();
                    }

                    model.estado = "eliminado";
                    if (ModelState.IsValid)
                    {
                        _db.cab_anticipos.Attach(model);
                        _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        _db.SaveChanges();
                        return JsonExito();
                    }


                }
                else {
                    return JsonError("Este anticipo posee Pago :( no se eliminará");
                }


                return JsonError("Hey !!! hubo un problema.");

            }
            catch (DbEntityValidationException e)
            {
                ErrorService.LogError(e);
                return JsonError("Hey !!! hubo un problema.");
            }
        }

        [ClientAuthorize("MantAnti")]
        public JsonResult get_provedoresF(string query)
        {

            var data = from prov in _db.Proveedores
                       where prov.Aux.Contains(query)
                       select new
                       { value = prov.Aux, data = prov.Nom.Trim() };

            //var jsonString = JsonConvert.SerializeObject(data);
            //var myObject = JsonConvert.DeserializeObject<>(jsonString);
            return Json(new
            {
                data = data.ToList()
                //data=jsonString
            }, JsonRequestBehavior.AllowGet);
        }
        [ClientAuthorize("MantAnti")]
        public JsonResult ObtenerCta(int rut)//Obtiene las cuentas 
        {

            //(from clie in _db.Clientes
            // join per in _db.Permisos on clie.ID equals per.Clie
            // where clie.Activo == "S"
            // && per.Usu == usu
            // select new { ID = clie.ID, Nom = clie.Nombre_Archivo }
            //                     ).OrderBy(x => x.Nom);

            var json = (from clie in _db.Clientes
                         join ta in _db.Tipo_Anticipos on clie.Rut   equals  ta.rut_emp
                         where clie.ID == rut
                         select new
                         {
                             nom_tanticipo= ta.nom_tanticipo,
                             id_tanticipo = ta.id_tanticipo,
                             id_cuenta_contable = ta.CtaC
                         }).ToList();

            //var rut_emp = _db.Clientes.Where(x => x.Rut == rut);
            //var data_valores = _db.Tipo_Anticipos.ToList();
            //var json = data_valores.Where(x => x.rut_emp== rut_emp.Select(ux=> ux.Rut).ToString()).Select(v => new
            //{
            //    v.nom_tanticipo,v.id_tanticipo
            //});

            return Json(new { data = json }, JsonRequestBehavior.AllowGet);
        }

        private static string Digito(int rut)
        {
            int suma = 0;
            int multiplicador = 1;
            while (rut != 0)
            {
                multiplicador++;
                if (multiplicador == 8)
                    multiplicador = 2;
                suma += (rut % 10) * multiplicador;
                rut = rut / 10;
            }
            suma = 11 - (suma % 11);
            if (suma == 11)
            {
                return "0";
            }
            else if (suma == 10)
            {
                return "K";
            }
            else
            {
                return suma.ToString();
            }
        }
        
        public JsonResult ObtenerSaldoCuentas(int rut)//Obtiene el saldo de las cuentas 
        {

            var nom_base = _db.Clientes.FirstOrDefault(x => x.ID == rut);
            var extrae_ruta = nom_base.Ruta.Split('\\').Last();

            var query = " SELECT format(Sum(cwmovim.MovDebe) - Sum(cwmovim.MovHaber),''###,###,###,###,###'') as Saldo FROM(softland.cwcpbte RIGHT JOIN softland.cwmovim ON(cwcpbte.CpbAno = cwmovim.CpbAno) AND(cwcpbte.CpbNum = cwmovim.CpbNum)) LEFT JOIN softland.cwpctas ON cwmovim.PctCod = cwpctas.PCCODI where cwcpbte.CpbEst = ''V'' AND cwpctas.PCCONB = ''S'' AND cwcpbte.proceso <> ''CW_GELPC''";


            var Exec_SP = LoadData("exec [SP_ExecuteQuery] '"+ extrae_ruta + "', '"+ query + "'");
            return Json(new { data = Exec_SP }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult ObtenerSaldoCuentasAnticipo(int rut)//Obtiene el saldo de las cuentas 
        {

            var nom_base = _db.Clientes.FirstOrDefault(x => x.ID == rut);
            var extrae_ruta = nom_base.Ruta.Split('\\').Last();

            var query = " SELECT cwmovim.PctCod, cwpctas.PCDESC, format(Sum(cwmovim.MovDebe) - Sum(cwmovim.MovHaber),''###,###,###,###,###'') as Saldo FROM (softland.cwcpbte RIGHT JOIN softland.cwmovim ON (cwcpbte.CpbAno = cwmovim.CpbAno) AND (cwcpbte.CpbNum = cwmovim.CpbNum)) LEFT JOIN softland.cwpctas ON cwmovim.PctCod = cwpctas.PCCODI where cwcpbte.CpbEst=''V'' AND cwpctas.PCCONB=''S'' AND cwcpbte.proceso <>''CW_GELPC'' GROUP BY cwmovim.PctCod, cwpctas.PCDESC, cwcpbte.CpbEst, cwpctas.PCCONB HAVING  (Sum(cwmovim.MovDebe) - Sum(cwmovim.MovHaber)) > 1 ";


            var Exec_SP = LoadData("exec [SP_ExecuteQuery] '" + extrae_ruta + "', '" + query + "'");
            return Json(new { data = Exec_SP }, JsonRequestBehavior.AllowGet);
        }

        [ClientAuthorize("MantAnti")]
        public JsonResult Pagado( int[] idAnticipo )
        {
            try {
                var x = _db.cab_anticipos.Where(p => idAnticipo.Contains(p.id_anticipo)).ToList();
                x.ForEach(p => { p.fec_pago = System.DateTime.Now;  p.usu_pag = SesionLogin().Sigla; });

                _db.SaveChanges();
                return JsonExito();

            }
            catch (Exception e) {
                return JsonError("Opps, ocurrio un problema");
            }
        }

        public JsonResult CentroCosto(int id_cliente)
        {
            var cliente = _db.Clientes.FirstOrDefault(x => x.ID == id_cliente);
            var ruta = cliente.Ruta.Split('\\').Last();
            var Exec_SP = LoadData("exec [PA_Centro_Costos] '" + ruta + "'");
            return Json(new { data = Exec_SP }, JsonRequestBehavior.AllowGet);
        }

    }
}