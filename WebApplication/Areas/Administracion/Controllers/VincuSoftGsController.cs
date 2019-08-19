using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplicationMod;

namespace WebApplication.Areas.Administracion.Controllers
{
    public class VincuSoftGsController : MasterController
    {
        [ClientAuthorize("MantVincuSoftGs")]
        public ActionResult Index()
        {

            return View();
        }


        [ClientAuthorize("MantVincuSoftGs")]
        public JsonResult listadoClientes()
        {
            try
            {

                var listBD = new List<string>();

                var clientes = _db.Clientes.Where(e => e.Activo == "S" && e.Ruta.Length > 10).Select(e => e.Ruta).ToList();

                foreach (var cliente in clientes)
                {
                    var BD = cliente.Split('\\').Last();
                    listBD.Add(BD);
                }

                //--- PROCEDIMIENTO ---//
                //string StringBD = String.Join("|", listBD);
                //var clientes_SP = LoadData("[dbo].[PA_SelectorBasesCount] '" + StringBD + "', 1");


                //var clientes_SP = LoadData("SELECT C.ID as Id_cliente, D.Rut as rut, D.[Nombre Archivo] AS nom_cliente, count(D.ID) as contador " +
                //                "FROM docs D " +
                //                "JOIN clientes C on D.Rut = C.Rut " +
                //                "WHERE(D.UsuCent IS NULL) AND(D.Centraliza IS NULL) AND(D.UsuImp IS NOT NULL) " +
                //                "GROUP BY " +
                //                "C.ID, " +
                //                "D.Rut, " +
                //                "D.[Nombre Archivo], " +
                //                "D.UsuCent " +
                //                "ORDER BY D.[Nombre Archivo]").ToList();



                var clientes_SP = LoadData("SELECT C.ID as Id_cliente, D.Rut as rut, D.[Nombre Archivo] AS nom_cliente, count(D.ID) as contador " +
                                "FROM  dbo.Usu RIGHT OUTER JOIN " +
                                 "dbo.Permisos ON dbo.Usu.ID = dbo.Permisos.Usu LEFT OUTER JOIN " +
                                 "dbo.Clientes c ON dbo.Permisos.Clie = c.ID " +
                                "inner join docs D on  D.Rut = c.Rut " +
                                "WHERE UsuEnt is not null AND(TipoCV = 'C' or TipoCV = 'R') AND UsuImp is null " +
                                "and Sigla = '"+SesionLogin().Sigla+"' " +
                                "GROUP BY " +
                                "C.ID," +
                                "D.Rut," +
                                "D.[Nombre Archivo], " +
                                "D.UsuCent " +
                                "ORDER BY D.[Nombre Archivo]").ToList();


                List<SelectListItem> ListselectListItem = new List<SelectListItem>();

                foreach (var item in clientes_SP)
                {
                    //ListselectListItem.Add(new SelectListItem() { Text = item["nom_cliente"].ToString() + " " + "[" + item["contador"] + "]", Value = item["Id_cliente"].ToString() });
                    ListselectListItem.Add(new SelectListItem() { Text = item["nom_cliente"].ToString() , Value = item["Id_cliente"].ToString() });
                }

                SelectList selectClientes = new SelectList(ListselectListItem, "Value", "Text");

                var json = selectClientes.Select(x => new { x.Value, x.Text });

                return Json(new { data = json }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception e) {
                throw e;
            }
        }

        [HttpPost]
        [ClientAuthorize("MantVincuSoftGs")]
        public JsonResult GetDataSoftland(int? id) {
            try
            {
                if (id.HasValue)
                {

                    var rutCliente = _db.Clientes.Where(e => e.ID == id).Select(e => e.Rut).First();
                    var ruta = _db.Clientes.Where(e => e.ID == id).Select(e => e.Ruta).FirstOrDefault();

                    var BD = ruta.Split('\\').Last();

                    //--- PROCEDIMIENTO ---//
                    //var clientes_SP = LoadData("[dbo].[PA_SelectorBasesCount] null ,0,'" + id + "'");

                    var clientes_SP = LoadData("SELECT gs.NumCent as NumCent ,gs.CodAux ,gs.NomAux ,gs.CPBMes, gs.CPBAño ,gs.NumDocI, gs.CTDCod, gs.Total, gs.Entrega, gs.Imputacion, gs.ID as ID_gs, soft.NumDoc as num_doc_s, soft.CpbNum as num_comp_s, cpbte.Usuario, cpbte.CpbFec " +
                                         "FROM Docs gs " +
                                         "LEFT JOIN(SELECT cpbNum, CodAux, CpbMes, CpbAno, NumDoc, TtdCod FROM [" + BD + "].[softland].cwmovim GROUP BY cpbNum, CodAux, CpbMes, CpbAno, NumDoc, TtdCod) soft on gs.NumDocI = soft.NumDoc " +
                                         "and gs.CodAux COLLATE SQL_Latin1_General_CP1_CI_AI = soft.CodAux " +
                                         "and gs.CPBMes COLLATE SQL_Latin1_General_CP1_CI_AI = soft.CpbMes " +
                                         "and gs.CPBAño COLLATE SQL_Latin1_General_CP1_CI_AI = soft.CpbAno " +
                                         "LEFT join [" + BD + "].[softland].cwcpbte cpbte on soft.cpbnum = cpbte.cpbnum " +
                                         "WHERE (gs.CLIENTE = " + rutCliente + ")" +
                                         "AND UsuEnt is not null AND (TipoCV ='C' or TipoCV ='R') AND UsuImp is null and soft.TtdCod like 'C%'").ToList();

                    return Json(new { data = clientes_SP }, JsonRequestBehavior.AllowGet);
                }
                else {

                    return Json(new { data = ""}, JsonRequestBehavior.AllowGet);

                }
            }

            catch (Exception e) {

                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);

            }


        }

        [HttpPost]
        [ClientAuthorize("MantVincuSoftGs")]
        public JsonResult VinculaSoftGs(PairValues [] PairValues) {
            try
            {
                //var registros = _db.GS_Documentos.Where(gs => PairValues.Any(p => p.idGs == gs.ID)).ToList();

                //registros.ForEach(p => p.NumCent = PairValues.FirstOrDefault(r => r.idGs == p.ID).NumCent);

                //var query = (from gs in _db.GS_Documentos
                //            where PairValues.Contains(gs.ID)
                //            select gs);

                var lista_idgs = new List<int>();
                var lista_numCent = new List<int>();

                foreach (var element in PairValues)
                {
                   var registro = _db.GS_Documentos.Where(x => x.ID == element.idGs).ToList();

                    foreach (var r in registro) {
                        r.NumCent = element.NumCent;
                        r.UsuImp = element.UsuCent;
                        r.UsuCent = SesionLogin().Sigla;
                        r.Centraliza = element.centraliza;
                    }
                    _db.SaveChanges();
                }

                return JsonExitoMsg("Vinculación éxitosa!");

            }
            catch (Exception e) {

                return JsonError("Error en la Vinculación de los documentos");
            }
        }

        public class PairValues
        {
            public int idGs { get; set; }
            public int NumCent { get; set; }

            public string UsuCent { get; set; }
            public DateTime centraliza { get; set; }
        }
    }
}