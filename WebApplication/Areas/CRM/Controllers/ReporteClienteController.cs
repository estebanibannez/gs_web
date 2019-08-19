//using ModeloCRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Areas.CRM.Controllers
{
    public class ReporteClienteController : MasterController
    {
        // GET: CRM/ReporteCliente
        public ActionResult Index()
        {
            return View();
        }
    }
}