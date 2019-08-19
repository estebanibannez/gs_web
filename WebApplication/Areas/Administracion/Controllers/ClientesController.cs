using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class ClientesController : MasterController
    {
        // GET: Clientes/Clientes

        [ClientAuthorize("MantClie")]
        public ActionResult Index()
        {
            var id_usu = SesionLogin().ID;
            var listadoclientes = (from clie in _db.l_clientes where clie.ID > 0  select clie ).ToList();
            ViewBag.seleccionado = "Exento";

            var conA = listadoclientes.Count(p => p.Activo == "S");
            var conI = listadoclientes.Count(p => p.Activo == "N");

            ViewBag.contadorActivo = conA;
            ViewBag.contadorInactivo = conI;
            return View(listadoclientes);
        }

        [ClientAuthorize("MantClie")]
        public ActionResult Create()
        {
            Selectores(null);
            return PartialView("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantClie")]
        public ActionResult Create(WebApplicationMod.Clientes model, string chk_activo, string chk_irfs, string chk_adm_cheq, string chk_contdolares, string chk_eeff, string chk_tesoreria, string chk_payroll, string chk_idioma, string chk_impu, string chk_snringles, string ivaselect)
        {
            // Almaceno el usuario creador de la empresa.
            model.Usu = "Jackermann";

            //Almaceno en el modelo la fecha de creación de la empresa.
            var fecha = DateTime.Now.ToString("yyyy-MM-dd");
            model.fingreso = DateTime.ParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            // convierto los Checkbox de (str bool) a string y se los paso al modelo.
            model.Activo = chk_activo == "true" ? "S" : "N";
            model.IRFS = chk_irfs == "true" ? "S" : "N";
            model.AdmCheque = chk_adm_cheq == "true" ? "S" : "N";
            model.CTD = chk_contdolares == "true" ? "S" : "N";
            model.publicar_eeff = chk_eeff == "true" ? "S" : "N";
            model.TES = chk_tesoreria == "true" ? "S" : "N";
            //model.RRHH = chk_payroll == "true" ? "S" : "N";
            model.RRHH = chk_payroll == "true" ? "S" : "N";
            model.IIng = chk_idioma == "true" ? "S" : "N";
            model.impsug = chk_impu == "true" ? "S" : "N";
            model.snr_ingl = chk_snringles == "true" ? "S" : "N";
            model.Tipo = ivaselect;

            try
            {
                if (ModelState.IsValid)
                {
                    _db.Clientes.Add(model);
                    _db.SaveChanges();
                    return JsonExito();
                }
            }
            catch (Exception ex)
            {
                ErrorService.LogError(ex);
                return JsonError("Error");
            }

            return View();
        }

        [ClientAuthorize("MantClie")]
        public ActionResult Edit(int id)
        {
            var model = _db.Clientes.FirstOrDefault(p => p.ID == id);
            
            if (model == null) return RedirectToAction("Create");
           
            Selectores(model);

            ////retorna el formulario con el modelo asociado, para edición
            return PartialView("Create", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ClientAuthorize("MantClie")]
        public ActionResult Edit(WebApplicationMod.Clientes model, string chk_activo, string chk_irfs, string chk_adm_cheq, string chk_contdolares, string chk_eeff, string chk_tesoreria, string chk_payroll, string chk_idioma, string chk_impu, string chk_snringles, string ivaselect)
        {
            try
           {
                // Almaceno el usuario modificador de la empresa.
                model.Usu = "Jackermann";

                //  convierto Checkbox en S o N
                model.Activo = chk_activo == "true" ? "S" : "N";
                model.IRFS = chk_irfs == "true" ? "S" : "N";
                model.AdmCheque = chk_adm_cheq == "true" ? "S" : "N";
                model.CTD = chk_contdolares == "true" ? "S" : "N";
                model.publicar_eeff = chk_eeff == "true" ? "S" : "N";
                model.TES = chk_tesoreria == "true" ? "S" : "N";
                //model.RRHH = chk_payroll == "true" ? "S" : "N";
                model.RRHH = chk_payroll == "true" ? "S" : "N";
                model.IIng = chk_idioma == "true" ? "S" : "N";
                model.impsug = chk_impu == "true" ? "S" : "N";
                model.snr_ingl = chk_snringles == "true" ? "S" : "N";
                model.Tipo = ivaselect;

                   if (ModelState.IsValid)
                    {
                         _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                         _db.SaveChanges();
                          return JsonExitoMsg(model.ID.ToString());
                    }
               return JsonError("Hey !!! hubo un problema.");
            }
            catch (Exception ex)
            {
                ErrorService.LogError(ex);
                return JsonError("Hey !!! hubo un problema.");
           }
        }

        [ClientAuthorize("MantClie")]
        public ActionResult elimina(int? id) {

            //if (tienePermiso("PERUSUINGHH")) { return PartialView("denegaPermiso"); }
            string estado = "N";
            var model = _db.Clientes.SingleOrDefault(item => item.ID == id);

            if (id == null)
            {
                return JsonError("Opps, ocurrio un problema");
            }
           
            try
            {
                model.Activo = estado;
                if (ModelState.IsValid)
            {
                    //_db.Clientes.Remove(eliminocliente);
                _db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                _db.SaveChanges();
                return JsonExito();
            }
            }
            catch (Exception e)
            {
                return JsonError("Error");
            }

            return JsonError("Opps, ocurrio un problema");


        }
       
        private void Selectores(WebApplicationMod.Clientes model)
        {
          if (model != null) {

                if(model.Tipo != null)
                    ViewBag.iva = model.Tipo;
                else
                    ViewBag.iva = "0";

                //if (model.CC != null) // Centro de Costo
                //    //ViewBag.CC = new SelectList(_db2.cwtccos.OrderBy(item => item.CodiCC), "CodiCC", "CodiCC", model.CC.TrimEnd());
                //    ViewBag.CC = new SelectList(_db2.cwtccos.OrderBy(item => item.CodiCC), "CodiCC", "CodiCC", model.CC);

                //else
                //    ViewBag.CC = new SelectList(_db2.cwtccos.OrderBy(item => item.CodiCC), "CodiCC", "CodiCC");

                if (model.MPago != null) // Cheque - Transf - Vale Vista
                    ViewBag.mediopago = new SelectList(_db.GS_Medios_Pago.OrderBy(item => item.ID), "ID", "nom", model.MPago);
                else
                    ViewBag.mediopago = new SelectList(_db.GS_Medios_Pago.OrderBy(item => item.ID), "ID", "nom");

                if (model.TCFac != null)
                    ViewBag.moneda1 = new SelectList(_db.Monedas.OrderBy(o => o.Id), "Id", "Descrip", model.TCFac); // $ - Euro - Pesos
                else
                    ViewBag.moneda1 = new SelectList(_db.Monedas.OrderBy(o => o.Id ), "Id", "Descrip");

                if (model.TCFac1 != null)
                    ViewBag.moneda2 = new SelectList(_db.Monedas.OrderBy(o => o.Id ), "Id", "Descrip", model.TCFac1);
                else
                    ViewBag.moneda2 = new SelectList(_db.Monedas.OrderBy(o => o.Id), "Id", "Descrip");

                if (model.TipChe != null)
                    ViewBag.tipo = new SelectList(_db.GS_TIPMP.OrderBy(o => o.ID), "ID", "Nom", model.TipChe); // continuo - Manual
                else
                    ViewBag.tipo = new SelectList(_db.GS_TIPMP.OrderBy(o => o.ID), "ID", "Nom"); // continuo - Manual

                if (model.gerencia_bo != null)
                    ViewBag.gerenciaBO = new SelectList(_db.parametros.Where(p => p.tipo == "gebo" || (p.tipo == "std")), "id", "nom", model.gerencia_bo);
                else
                    ViewBag.gerenciaBO = new SelectList(_db.parametros.Where(p => p.tipo == "gebo" || (p.tipo == "std")), "id", "nom");

                if (model.gerencia_ss != null)
                    ViewBag.gerenciaSS = new SelectList(_db.parametros.Where(p => p.tipo == "gess" || (p.tipo == "std")), "id", "nom", model.gerencia_ss);
                else
                    ViewBag.gerenciaSS = new SelectList(_db.parametros.Where(p => p.tipo == "gess" || (p.tipo == "std")), "id", "nom");

                if (model.cartera_tes != null)
                    ViewBag.tesoreria = new SelectList(_db.parametros.Where(p => p.tipo == "ct" || (p.tipo == "std")), "id", "nom", model.cartera_tes);
                else
                    ViewBag.tesoreria = new SelectList(_db.parametros.Where(p => p.tipo == "ct" || (p.tipo == "std")), "id", "nom");

                if (model.cartera_pay != null)
                    ViewBag.payroll = new SelectList(_db.parametros.Where(p => p.tipo == "cpy" || (p.tipo == "std")), "id", "nom", model.cartera_pay);
                else
                    ViewBag.payroll = new SelectList(_db.parametros.Where(p => p.tipo == "cpy" || (p.tipo == "std")), "id", "nom");

                if (model.cartera_oper != null)
                    ViewBag.operaciones = new SelectList(_db.parametros.Where(p => p.tipo == "cop"), "id", "nom", model.cartera_oper);
                else
                    ViewBag.operaciones = new SelectList(_db.parametros.Where(p => p.tipo == "cop"), "id", "nom");

                if (model.gbo != null)
                    ViewBag.grupoBO = new SelectList(_db.parametros.Where(p => p.tipo == "gbo" || (p.tipo == "std")), "id", "nom", model.gbo);
                else
                    ViewBag.grupoBO = new SelectList(_db.parametros.Where(p => p.tipo == "gbo" || (p.tipo == "std")), "id", "nom");

                if (model.kpif29 != null)// Grupo Back Office
                    ViewBag.kpiF29 = new SelectList(_db.parametros.Where(p => p.tipo == "KPIF29" || (p.tipo == "std")), "id", "nom", model.kpif29);
                else
                    ViewBag.kpiF29 = new SelectList(_db.parametros.Where(p => p.tipo == "KPIF29" || (p.tipo == "std")), "id", "nom");

            } else { 

                ViewBag.iva = "0";
                //ViewBag.CC = new SelectList(_db2.cwtccos.OrderBy(item => item.CodiCC), "CodiCC", "CodiCC");
                ViewBag.mediopago = new SelectList(_db.GS_Medios_Pago.OrderBy(item => item.ID), "ID", "nom");
                ViewBag.moneda1 = new SelectList(_db.Monedas.OrderBy(item => item.Id ), "Id", "Descrip");
                ViewBag.moneda2 = new SelectList(_db.Monedas.OrderBy(item => item.Id), "Id", "Descrip");
                ViewBag.tipo = new SelectList(_db.GS_TIPMP.OrderBy(item => item.ID ), "ID", "Nom"); // continuo - Manual
                ViewBag.gerenciaBO = new SelectList(_db.parametros.Where(item => item.tipo == "gebo" || (item.tipo == "std")), "id", "nom");
                ViewBag.gerenciaSS = new SelectList(_db.parametros.Where(item => item.tipo == "gess" || (item.tipo == "std")), "id", "nom");
                ViewBag.tesoreria = new SelectList(_db.parametros.Where(item => item.tipo == "ct" || (item.tipo == "std")), "id", "nom");
                ViewBag.payroll = new SelectList(_db.parametros.Where(item => item.tipo == "cpy" || (item.tipo == "std")), "id", "nom");
                ViewBag.operaciones = new SelectList(_db.parametros.Where(item => item.tipo == "cop"), "id", "nom");
                ViewBag.grupoBO = new SelectList(_db.parametros.Where(item => item.tipo == "gbo" || (item.tipo == "std")), "id", "nom");
                ViewBag.kpiF29 = new SelectList(_db.parametros.Where(item => item.tipo == "KPIF29" || (item.tipo == "std")), "id", "nom");
            }
        }

        [ClientAuthorize("MantClie")]
        public ActionResult iniciaBeta(int id)
        {
            try
            {
                var cli = _db.Clientes.FirstOrDefault(x => x.ID == id);
                var extrae_ruta = cli.Ruta.Split('\\').Last();                
                if(createTableInDatabase(extrae_ruta)) { 
                    return JsonExito();
                }
                else
                {
                    throw new Exception("Problemas al Inicializar el Beta");
                }
            }
            catch (Exception ex)
            {
                return JsonError(ex.Message);
            }            
        }
		
		private bool createTableInDatabase(string base_)
        {
            try
            {     
                string conec = @"Provider=Microsoft.Jet.OLEDB.4.0; Data Source= W:\SOFTLAND\DATOS\" + base_ + @"\SoDatos.mdb ; Persist Security Info=False";
                OleDbConnection conn = new OleDbConnection();
                conn.ConnectionString = conec;
                conn.Open();
                List<string> lista = new List<string>();
                string CREATE_Ctas251 = " CREATE TABLE Ctas251 ([ID] COUNTER NULL, [Cta] TEXT(18) NULL, [Año] LONG NULL, [Mes] TEXT(3) NULL, [Monto] LONG NULL) ";
                string CREATE_detalle = " CREATE TABLE Detalle ([ID] COUNTER NULL, [Doc] LONG NULL, [CodCta] TEXT(15) NULL, [CCCod] TEXT(8) NULL, [Monto] TEXT(8) NULL) ";
                string CREATE_Documentos = " CREATE TABLE Documentos ([ID] COUNTER NULL, [CPBAño] TEXT(4) NULL, [CPBMes] TEXT(2) NULL, [CodAux] TEXT(10) NULL, [CTDCod] TEXT(2) NULL, [NumDocI] LONG NULL, [MovFe] DATETIME NULL, [MovFv] DATETIME NULL, [VendCod] TEXT(4) NULL, [MovGlosa] TEXT(60) NULL, [Neto] LONG NULL, [Exento] LONG NULL, [Impu] LONG NULL, [Total] LONG NULL, [NumDocF] TEXT(50) NULL, [Entrega] DATETIME NULL, [UsuEnt] TEXT(10) NULL, [Centraliza] DATETIME NULL); ";
                string INDEX = " CREATE UNIQUE INDEX PrimaryKey ON Documentos ([CodAux] ASC, [CTDCod] ASC, [NumDocI] Asc ) WITH PRIMARY; ";
                string DELETE_TDOC = " Delete* From TBDOC ";
                string CREATE_TBDOC = " CREATE TABLE TBDOC ([CodDoc] TEXT(2) NULL, [DesDoc] TEXT(60) NULL, [Tipo] TEXT(2) NULL, [Impto] CURRENCY NULL, [CamA] TEXT(4) NULL, [Decu] TEXT(1) NULL, [SumImp] TEXT(1) NULL, [DSD] TEXT(1) NULL, [DocRes] TEXT(50) NULL, [RotuloDoc] TEXT(10) NULL); ";
                lista.Add(CREATE_Ctas251);
                lista.Add(CREATE_detalle);
                lista.Add(CREATE_Documentos);
                lista.Add(INDEX);
                lista.Add(CREATE_TBDOC);
                lista.Add(DELETE_TDOC);    
                OleDbCommand cmmd = new OleDbCommand("",conn);
                var tdoc = _db.TDoc.ToList();
                foreach (var item in lista)
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        try
                        {                            
                            cmmd.CommandText = item;
                            cmmd.ExecuteNonQuery();
                            //MessageBox.Show("Add!");
                            //conn.Close();
                        }
                        catch (OleDbException expe)
                        {
                            conn.Close();
                            throw new Exception(expe.Message);
                            //MessageBox.Show(expe.Message);
                            //conn.Close();
                        }
                    }
                    else
                    {                       
                        throw new Exception("Error en la conexión");
                    }
                }
                foreach (var item in tdoc)
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        string INSERT_TBDOC = " INSERT INTO TBDOC ( [CodDoc], [DesDoc], [Tipo], [Impto], [CamA], [Decu], [SumImp], [DSD], [DocRes], [RotuloDoc] ) VALUES ('" + item.CodDoc + "','" + item.DesDoc + "','" + item.Tipo + "','" + item.Impto + "','" + item.CamA + "','" + item.Decu + "','" + item.SumImp + "','" + item.DSD + "','" + item.DocRes + "','" + item.RotuloDoc + "') ";
                        cmmd.CommandText = INSERT_TBDOC;
                        cmmd.ExecuteNonQuery();
                        //MessageBox.Show("Add!");
                    }
                    else
                    {
                        return false;
                    }
                }
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}


