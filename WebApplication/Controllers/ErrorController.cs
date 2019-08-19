using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    public class ErrorController : _Controller
    {
        // GET: Error
        [AllowAnonymous]
        public ActionResult Error(Exception exception)
        {
            HttpException httpException = exception as HttpException;

            if (httpException == null)
            {
                ViewBag.http_code = 500;
                HttpContext.Response.StatusCode = 500;
            }
            else
            {
                ViewBag.http_code = httpException.GetHttpCode();
                HttpContext.Response.StatusCode = httpException.GetHttpCode();
            }
            ViewBag.SHOW_ERROR_DETAILS = true;
            ViewBag.NeedLayaout = "N";
            ViewBag.codeError = ErrorService.LogError(exception);
            ViewBag.message = ErrorService.LogErrorMessage(exception);
            return View("Error");
        }


        [AllowAnonymous]
        public ActionResult e404()
        {
            HttpContext.Response.StatusCode = 404;
            ViewBag.NeedLayaout = "N";
            return View("Error_404");
        }
    }
}