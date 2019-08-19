using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using WebApplication.App_Start;
using WebApplicationMod;

namespace WebApplication.Areas.Operaciones.Controllers
{
    public class CapturaFacturasElecsController : MasterController
    {
        [ClientAuthorize("MantCapFacElec")]
        public ActionResult Index()
        {
            
            try
            {
                string usuario = SesionLogin().Sigla;
                var selectClients = (
                    from gs_usu in _db.Usu
                    join gs_permis in _db.Permisos
                    on gs_usu.ID equals gs_permis.Usu
                    join gs_clients in _db.Clientes
                    on gs_permis.Clie equals gs_clients.ID
                    where gs_usu.Sigla == usuario
                    orderby gs_clients.Nombre_Archivo
                    select new
                    {
                        IdCli = gs_clients.ID,
                        NombreCli = gs_clients.Nombre_Archivo,
                        RutaCli = gs_clients.Ruta
                    }
                ).ToList();

                var listarClientes = new SelectList(selectClients, "IdCli", "NombreCli");
                ViewBag.ListaClientes = listarClientes;

                return View();
            }
            catch (Exception e)
            {
                return JsonError("Error: " + e.Message);
            }
        }

        [ClientAuthorize("MantCapFacElec")]
        public JsonResult CargarTiposDocs()
        {
            try
            {
                var selectTipDocs = (
                        from gs_tipDocs in _db.GS_TDoc
                        where gs_tipDocs.FacElec == "S"
                        select new
                        {
                            Descrip = gs_tipDocs.DesDoc,
                            Codig = gs_tipDocs.CodDoc,
                            Tip = gs_tipDocs.Tipo
                        }
                    ).ToList();

                return Json(new { data = selectTipDocs }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ClientAuthorize("MantCapFacElec")]
        public JsonResult ValidacionCamposExcel(DatosExcel[] data, int clie, string tipCod)
        {
            try
            {
                List<ValidaList> validaciones = new List<ValidaList>();
                
                foreach (var item in data)
                {
                    var valTotal = false; //si no calzan la suma del exento, neto e iva con el total se manda mensaje
                    var valNumFact = false;//si "EXISTE" el documento se enviara mensaje 
                    var valRutExt = false;// si el rut pernetece a un extranjero enviar un mensaje y activar el boton crea aux
                    var valExistAux = false;//si "NO" existe aux enviar mensaje y activar boton crea aux
                    var valExistCta = false;//si "NO" existe la cuenta enviar mensaje
                    var valExistCCosto = false;//si "NO" existe el centro de costo se enviara mensaje
                    var valFechaEmi = false;
                    var valRazonSo = false;

                    //validacion del total
                    if (item.Total != 0)
                    {
                        if (item.Total == item.Exento + item.Neto + item.Iva)
                        {
                            valTotal = true;
                        }
                    }
                    //validacion existencia del documento en softland
                    var nomCli = _db.Clientes.FirstOrDefault(x => x.ID == clie).Ruta.Split('\\').Last();
                    var rutPro = "";
                    if (item.Rut != null) { rutPro = item.Rut.Substring(0, item.Rut.Length - 2); } else { rutPro = "0"; };//ojo que el rut debe ir sin el digitoverificador ni guion
                        var numDoc = item.NumFactura;

                        List<SqlParameter> paraDocSoft = new List<SqlParameter>()
                    {
                        new SqlParameter("@Cliente", System.Data.SqlDbType.NVarChar){Value = nomCli},
                        new SqlParameter("@Rut_proveedor", System.Data.SqlDbType.NVarChar){Value = rutPro},
                        new SqlParameter("@Tipo_Doc", System.Data.SqlDbType.NVarChar){Value = tipCod},
                        new SqlParameter("@Num_Doc", System.Data.SqlDbType.NVarChar){Value = numDoc},
                        new SqlParameter("@counts", System.Data.SqlDbType.Int){Value = 0}
                    };

                        var validaDoc = PA_Almacenado(paraDocSoft, "PA_Valida_Existe_Documento");

                        if (validaDoc == 1)
                        {
                            valNumFact = true;
                        }

                        var similarFact = false;
                        var aux = "";
                        foreach (var i in data)
                        {
                            aux = i.NumFactura;
                            if (item != i)
                            {
                                if (aux == item.NumFactura)
                                {
                                    similarFact = true;
                                }
                            }
                        }

                        if (similarFact == true)
                        {
                            valNumFact = true;
                        }

                        //validar que razon social no llegue vacio
                        if (item.RazonSocial != null)
                        {
                            if (item.RazonSocial.Length >= 2)
                            {
                                valRazonSo = true;
                            }
                        }


                        //Validar que no sea un rut extranjero

                        if (item.Rut != null)
                        {
                            if (item.Rut == "555555555")
                            {
                                valRutExt = true;
                            }
                            else
                            {
                                if (item.Rut == null)
                                {
                                    valExistAux = false;
                                }
                                else
                                {
                                    List<SqlParameter> paraAux = new List<SqlParameter>(){
                            new SqlParameter("@Cliente", System.Data.SqlDbType.NVarChar) { Value = nomCli },
                            new SqlParameter("@Rut_proveedor", System.Data.SqlDbType.NVarChar) { Value = rutPro },
                            new SqlParameter("@counts", System.Data.SqlDbType.Int){Value = 0}
                        };
                                    var validaAux = PA_Almacenado(paraAux, "PA_Valida_Existe_Auxiliar");
                                    if (validaAux == 1)
                                    {
                                        valExistAux = true;
                                    }
                                }

                            }
                        }

                        //validar existencia de la cuenta
                        var cuenta = item.Cuenta;

                        List<SqlParameter> paraCuenta = new List<SqlParameter>()
                    {
                        new SqlParameter("@Cliente",System.Data.SqlDbType.NVarChar){Value=nomCli },
                        new SqlParameter("@Cuenta",System.Data.SqlDbType.NVarChar){Value=cuenta },
                        new SqlParameter("@counts", System.Data.SqlDbType.Int){Value = 0}
                    };

                        var validaCta = PA_Almacenado(paraCuenta, "PA_Valida_Existe_Cuenta");

                        if (validaCta == 1)
                        {
                            valExistCta = true;
                        }

                        //Validar existencia de centro de costo

                        var cenCosto = item.CCosto != "" ? item.CCosto : "0";

                        List<SqlParameter> paraCCosto = new List<SqlParameter>()
                    {
                        new SqlParameter("@Cliente",System.Data.SqlDbType.NVarChar){Value = nomCli},
                        new SqlParameter("@CCosto",System.Data.SqlDbType.NVarChar){Value = cenCosto },
                        new SqlParameter("@counts", System.Data.SqlDbType.Int){Value = 0}
                    };

                        var validaCCosto = PA_Almacenado(paraCCosto, "PA_Valida_Existe_CCosto");

                        if (validaCCosto == 1)
                        {
                            valExistCCosto = true;
                        }



                        try
                        {
                            var fecha = DateTime.Parse(item.FechaEmision);
                            valFechaEmi = true;
                        }
                        catch
                        {
                            valFechaEmi = false;
                        }

                        validaciones.Add(new ValidaList()
                        {
                            ValTot = valTotal,
                            ValFact = valNumFact,
                            ValAux = valExistAux,
                            ValExtra = valRutExt,
                            ValCta = valExistCta,
                            ValCCost = valExistCCosto,
                            ValFechEmi = valFechaEmi,
                            ValRazSol = valRazonSo
                        });
                    }

                return Json(new { listaValidaciones = validaciones });
            }
            catch (Exception e)
            {
                return JsonError("Error: " + e.Message);
            }
        }
        
        [HttpPost]
        [ClientAuthorize("MantCapFacElec")]
        public JsonResult Create(string[] auxisCrear,int clie)
        {
            try
            {
                foreach (var item in auxisCrear)
                {
                    if (item.Contains("undefined"))
                    {
                        return JsonError("No se puede crear un proveedor sin su nombre(Razón Social) o sin su rut");
                    }
                }
                var nomCli = _db.Clientes.FirstOrDefault(x => x.ID == clie).Ruta.Split('\\').Last();

                var listaAuxis = string.Join("|", auxisCrear);

                List<SqlParameter> para = new List<SqlParameter>()
                {
                    new SqlParameter("@Cliente",System.Data.SqlDbType.NVarChar){Value=nomCli },
                    new SqlParameter("@Auxs",System.Data.SqlDbType.NVarChar){Value= listaAuxis},
                    new SqlParameter("@counts", System.Data.SqlDbType.Int){Value = 0}
                };

                var validaInsert = PA_Almacenado(para, "PA_INS_AUXS_SOLO_SOFTLAND");

                if (validaInsert == 0)
                {
                    return JsonError("Hubo un problema con la creacion del auxiliar. Verifique el rut ingresado");
                }

                return JsonExito();
            }
            catch(Exception e)
            {
                return JsonError("Error"+e.Message);
            }
        }

        [HttpPost]
        [ClientAuthorize("MantCapFacElec")]
        public ActionResult CargaSoftland(DatosExcel[] data, int? clie,ValidaList[] validaciones,DateTime periodo, string tipoDoc)
        {
            try
            {
                foreach (var item in validaciones)
                {
                    if (item.ValTot==false || item.ValFact==true || item.ValExtra==true/* || item.ValCta==false || item.ValCCost==false*/||item.ValFechEmi==false)
                    {
                        return JsonError("No se puede cargar a softland debido a que tiene errores en su plantilla, revise su documento");
                    }
                }

                var cuentaIvaVentas = (from gs_ctas in _db.GS_Ctas
                                    where gs_ctas.Cta == 28 && gs_ctas.Clie == clie
                                    select gs_ctas.CodCta.Trim()
                                    
                                    ).FirstOrDefault();

                if (cuentaIvaVentas == null)
                {
                    return JsonError("Solicite al administrador la creación de la IVA Debito");
                }

                var cuentaCliente = (from gs_ctas in _db.GS_Ctas
                                     where gs_ctas.Cta == 27 && gs_ctas.Clie == clie
                                     select gs_ctas.CodCta.Trim()
                                     
                                     ).FirstOrDefault();
                if (cuentaCliente == null)
                {
                    return JsonError("Solicite al administrador la creación de la cuenta del Cliente");
                }


                var cuentaCargo = (from gs_ctas in _db.GS_Ctas
                                          where gs_ctas.Cta == 30 && gs_ctas.Clie == clie
                                          select gs_ctas.CodCta.Trim()
                                          
                                     ).FirstOrDefault();
                if (cuentaCargo == null)
                {
                    return JsonError("Solicite al administrador la creación de la cuenta Contable Ventas");
                }

                var cuentaCentCost = (from gs_ctas in _db.GS_Ctas
                                      where gs_ctas.Cta == 29 && gs_ctas.Clie == clie
                                      select gs_ctas.CodCta.Trim()
                                      
                                     ).FirstOrDefault();

                if (cuentaCentCost == null)
                {
                    return JsonError("Solicite al administrador la creación de la cuenta Contable Ventas");
                }
                var nomCli = _db.Clientes.FirstOrDefault(x => x.ID == clie).Ruta.Split('\\').Last();
                var anio = periodo.ToString("yyyy");
                var mes = periodo.ToString("MM");


                List<SqlParameter> para = new List<SqlParameter>() {
                    new SqlParameter("@base",System.Data.SqlDbType.NVarChar){Value=nomCli },
                    new SqlParameter("@anio",System.Data.SqlDbType.NVarChar){Value=anio },
                    new SqlParameter("@mes",System.Data.SqlDbType.NVarChar){Value=mes },
                    new SqlParameter("@counts",System.Data.SqlDbType.Int){Value=0 }
                };

                var numComp = PA_Almacenado(para, "Get_Comprobante_Rtn");

                using (var db = new GSPSOEntities())
                {
                    db.Database.Connection.Open();
                    DataTable InsCwcpbte = new DataTable();
                    //crea columnas cpbte
                    InsCwcpbte.Columns.Add(new DataColumn("CpbAno", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbNum", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("AreaCod", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbFec", typeof(DateTime)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbMes", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbEst", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbTip", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbNui", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbGlo", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbImp", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbCon", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("Sistema", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("Proceso", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("Usuario", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbNormaIFRS", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbNormaTrib", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbAnoRev", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("CpbNumRev", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("FechaUiMod", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("SistemaMod", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("ProcesoMod", typeof(string)));
                    InsCwcpbte.Columns.Add(new DataColumn("TipoLog", typeof(string)));

                    InsCwcpbte.Rows.Add(anio, numComp.ToString(), "000", periodo, mes, "P", "T", "00000000",
                        "Centralizacion de ventas del mes", "S", "S", "CW", "Comprobante", "softland", "S", "S", "0000", "00000000",
                        null,null,null,null);

                    DataTable InsCwmovim = new DataTable();
                    //Crear Columnas cwmovim
                    InsCwmovim.Columns.Add(new DataColumn("CpbAno", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CpbNum", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovNum", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("PctCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CpbFec", typeof(DateTime)));
                    InsCwmovim.Columns.Add(new DataColumn("CpbMes", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CcCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovFe", typeof(DateTime)));
                    InsCwmovim.Columns.Add(new DataColumn("MovFv", typeof(DateTime)));
                    InsCwmovim.Columns.Add(new DataColumn("MovDebe", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("MovHaber", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("MovDebeMa", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("MovHaberMa", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("MovGlosa", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovEquiv", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("FecPag", typeof(DateTime)));
                    InsCwmovim.Columns.Add(new DataColumn("CodAux", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("TtdCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("NumDoc", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovTipDocRef", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovNumDocRef", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("AreaCod", typeof(string)));
                    //////////////////HASTA ESTAS COLUMNAS UTILIZARE YO 22//////////////////
                    InsCwmovim.Columns.Add(new DataColumn("CvCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("VendCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("UbicCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CajCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("IfCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovIfCant", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("DgaCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovDgCant", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("TipDocCb", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("NumDocCb", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("MonCod", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovNumCar", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovTC", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovNC", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovIPr", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MovAEquiv", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CODCPAG", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CbaNumMov", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("CbaAnoC", typeof(int)));
                    InsCwmovim.Columns.Add(new DataColumn("GrabaDLib", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CpbOri", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CodBanco", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CodCtaCte", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("MtoTotal", typeof(float)));
                    InsCwmovim.Columns.Add(new DataColumn("Cuota", typeof(int)));
                    InsCwmovim.Columns.Add(new DataColumn("CuotaRef", typeof(int)));
                    InsCwmovim.Columns.Add(new DataColumn("Marca", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("FecEmisionCh", typeof(DateTime)));
                    InsCwmovim.Columns.Add(new DataColumn("PagueseA", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("Impreso", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("DliCoInt_Aperturas", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("Nro_Operacion", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("FormaDePag", typeof(int)));
                    InsCwmovim.Columns.Add(new DataColumn("CpbNormaIFRS", typeof(string)));
                    InsCwmovim.Columns.Add(new DataColumn("CpbNormaTrib", typeof(string)));

                    DataTable InsDetli = new DataTable();

                    var afecTipo = _db.GS_TDoc.FirstOrDefault(x => x.CodDoc == tipoDoc).Tipo;
                    //Crea columnas en detli
                    InsDetli.Columns.Add(new DataColumn("CpbAno", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("CpbNum", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("MovNum", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("TtdCod", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("MonCod", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("DocumentoNulo", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("DlinDoc", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliFeDoc", typeof(DateTime)));
                    InsDetli.Columns.Add(new DataColumn("CodAux", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("DliMto01", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto02", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto03", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto10", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("Monto", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("CpbFec", typeof(DateTime)));
                    InsDetli.Columns.Add(new DataColumn("DetlDesde", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("DetlHasta", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("AreaCod", typeof(string)));
                    ///////////////////////////hasta aca//////////////
                    InsDetli.Columns.Add(new DataColumn("DliCoInt", typeof(string)));
                    InsDetli.Columns.Add(new DataColumn("DliMto04", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto05", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto06", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto07", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto08", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMto09", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd01", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd02", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd03", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd04", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd05", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd06", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd07", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd08", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd09", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliMtoAd10", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("MovEquiv", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("LoteDespacho", typeof(int)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv01", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv02", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv03", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv04", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv05", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv06", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv07", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv08", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv09", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("DliEquiv10", typeof(float)));
                    InsDetli.Columns.Add(new DataColumn("LibroMonedaBase", typeof(string)));
                    var countMovim = 0;
                    var countDetli = 0;

                    foreach (var item in data)
                    {
                        /// inserta cta cargo
                        /// 
                        if (tipoDoc.Substring(0, 1) == "N")
                        {
                            InsCwmovim.Rows.Add(anio, numComp.ToString(), countMovim, cuentaCargo, periodo, mes,
                            cuentaCentCost.ToString(), DateTime.Parse(item.FechaEmision), DateTime.Parse(item.FechaEmision).AddMonths(1),
                            item.Exento + item.Neto, 0, item.Exento + item.Neto,0 , tipoDoc + " " + item.NumFactura + " " + item.RazonSocial + " " + item.FechaEmision,
                            1, DateTime.Parse(item.FechaEmision), Helper.converRut(item.Rut, 1), tipoDoc, item.NumFactura,
                            tipoDoc, item.NumFactura, "000", null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null);
                        }
                        else
                        {   //De acuerdo al tipo de documento, se voltea el debe por haber
                            InsCwmovim.Rows.Add(anio, numComp.ToString(), countMovim, cuentaCargo, periodo, mes,
                            cuentaCentCost.ToString(), DateTime.Parse(item.FechaEmision), DateTime.Parse(item.FechaEmision).AddMonths(1),
                            0, item.Exento + item.Neto, 0, item.Exento + item.Neto, tipoDoc + " " + item.NumFactura + " " + item.RazonSocial + " " + item.FechaEmision,
                            1, DateTime.Parse(item.FechaEmision), Helper.converRut(item.Rut, 1), tipoDoc, item.NumFactura,
                            tipoDoc, item.NumFactura, "000", null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null);
                        }
                        

                        countMovim = countMovim + 1;


                        //inserta cuenta cliente
                        InsCwmovim.Rows.Add(anio, numComp.ToString(), countMovim, cuentaCliente, periodo, mes,
                            cuentaCentCost.ToString(), DateTime.Parse(item.FechaEmision), DateTime.Parse(item.FechaEmision).AddMonths(1),
                            item.Total, 0, item.Total, 0, tipoDoc + " " + item.NumFactura + " " + item.RazonSocial + " " + item.FechaEmision,
                            1, DateTime.Parse(item.FechaEmision), Helper.converRut(item.Rut, 1), tipoDoc, item.NumFactura,
                            tipoDoc, item.NumFactura, "000", null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null);

                        countMovim = countMovim + 1;

                        if (item.Iva > 0)
                        {
                            //inserta cuenta iva debito
                            InsCwmovim.Rows.Add(anio, numComp.ToString(), countMovim, cuentaIvaVentas, periodo, mes,
                                    cuentaCentCost.ToString(), DateTime.Parse(item.FechaEmision), DateTime.Parse(item.FechaEmision).AddMonths(1),
                                    0, item.Iva, 0, item.Iva, tipoDoc + " " + item.NumFactura + " " + item.RazonSocial + " " + item.FechaEmision,
                                    1, DateTime.Parse(item.FechaEmision), Helper.converRut(item.Rut, 1), tipoDoc, item.NumFactura,
                                    tipoDoc, item.NumFactura, "000", null, null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null);

                            countMovim = countMovim + 1;

                        }

                        if (afecTipo == "E")
                        {   //deacuerdo al tipo se cambia el delitmo 01 por el 02
                            InsDetli.Rows.Add(anio, numComp.ToString(), countDetli, tipoDoc, "01", "N", int.Parse(item.NumFactura),
                            DateTime.Parse(item.FechaEmision), Helper.converRut(item.Rut, 1), 0, item.Neto, item.Iva, item.Total,
                            item.Total, periodo, "00000000" + item.NumFactura, "00000000" + item.NumFactura, "000", null, null, null,
                            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null, null, null);
                            countDetli = countDetli + 1;
                        }
                        else
                        {
                            InsDetli.Rows.Add(anio, numComp.ToString(), countDetli, tipoDoc, "01", "N", int.Parse(item.NumFactura),
                            DateTime.Parse(item.FechaEmision), Helper.converRut(item.Rut, 1), item.Neto, 0, item.Iva, item.Total,
                            item.Total, periodo, "00000000" + item.NumFactura, "00000000" + item.NumFactura, "000", null, null, null,
                            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                            null, null, null, null, null, null, null, null, null, null);

                            countDetli = countDetli + 1;
                        }
                        
                    }
                    //comunicacion con la base de datos
                    using (var conn = db.Database.Connection as SqlConnection)
                    {
                        SqlCommand cmd = new SqlCommand("PA_INS_CENT_EXCEL", conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        List<SqlParameter> param = new List<SqlParameter>()
                        {
                            new SqlParameter(){
                                ParameterName = "@ListCpbte",
                                SqlDbType = SqlDbType.Structured,
                                Value = InsCwcpbte,
                                TypeName = "ListaCwcpbte",
                                
                            },
                            new SqlParameter(){
                                ParameterName = "@ListMovim",
                                SqlDbType = SqlDbType.Structured,
                                Value = InsCwmovim,
                                TypeName = "ListaCwmovim",

                            },
                            new SqlParameter(){
                                ParameterName = "@ListDetli",
                                SqlDbType = SqlDbType.Structured,
                                Value = InsDetli,
                                TypeName = "ListaCwdetli",

                            },
                            new SqlParameter("@base",SqlDbType.NVarChar){ Value = nomCli}
                        };
                        cmd.Parameters.AddRange(param.ToArray());

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) { };
                        };
                    };
                };

                    return JsonExito();
            }
            catch (Exception e)
            {
                return JsonError("Error: "+e.Message);
            }
        }

        [ClientAuthorize("MantCapFacElec")]
        public ActionResult DescargarPlantilla()
        {
            string rutaPlantilla = @"P:\PSO\GS\Reportes\mcfe.xlsx";
            var Doc_file = (rutaPlantilla).Replace(@"P:\", @"\\atenea\puentesur\").Trim(); //Replace (Ubicaion de los archivos en el servidor disco P:\\atenea\puentesur\).        
            return File(Doc_file, System.Web.MimeMapping.GetMimeMapping("mcfe"), Doc_file.Split('\\').Last().Trim());
        }


        public class DatosExcel
        {
            public string FechaEmision { get; set; }
            public string Tipo { get; set; }
            public string NumFactura { get; set; }
            public string FechaVcto { get; set; }
            public string RazonSocial { get; set; }
            public string Rut { get; set; }
            public int Exento { get; set; }
            public int Neto { get; set; }
            public int Iva { get; set; }
            public int Total { get; set; }
            public string Cuenta { get; set; }
            public string CCosto { get; set; }
        }

        public class ValidaList
        {
            public bool ValTot { get; set; }
            public bool ValFact { get; set; }
            public bool ValExtra { get; set; }
            public bool ValAux { get; set; }
            public bool ValCta { get; set; }
            public bool ValCCost { get; set; }
            public bool ValFechEmi { get; set; }
            public bool ValRazSol { get; set; }
        }
    }
}
