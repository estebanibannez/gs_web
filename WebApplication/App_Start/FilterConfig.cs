using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplicationMod;
using System.Collections.Generic;
using System.Linq;
using System;
using WebApplication.Seguridad;
using System.Web.Security;
using WebApplication.App_Start;
using WebApplication.Controllers;

namespace WebApplication
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new ActionFilterUserPermi());
        }
    }


    public class ActionFilterUserPermi : ActionFilterAttribute, IActionFilter
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)

        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated) {
                if ((filterContext.HttpContext.Session["SesionLoginPermisos"] as List<string>) == null)
                {
                    IdentityPersonalizado mu = (filterContext.HttpContext.User.Identity as IdentityPersonalizado);
                    filterContext.Controller.ViewBag.permisos = mu.roles;
                    filterContext.Controller.ViewBag.nomusu = mu.user.Nom;
                    //FilterContext.Controller.ViewBag.nomusu = mu.user.Nom;
                    filterContext.Controller.ViewBag.rutaPhoto = "../Img/fotos2/" + mu.user.Sigla + ".jpg";
                }
                else {
                    filterContext.Controller.ViewBag.permisos = filterContext.HttpContext.Session["SesionLoginPermisos"] as List<string>;
                }
            }
        }
    }

    public class CustomLayoutAjaxAttribute : ActionFilterAttribute, IResultFilter
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var viewResult = filterContext.Result as ViewResult;
            if (viewResult != null)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    viewResult.MasterName = null;
                }
                else
                {
                    viewResult.MasterName = "~/Views/Shared/_Layout.cshtml";
                }
            }
        }
    }

    public class ClientAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedroles;
        public ClientAuthorizeAttribute(params string[] roles)
        {
            this.allowedroles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {

            bool Authentication = base.AuthorizeCore(httpContext);
            bool Authorization = false;

            if (Authentication && allowedroles.Count() > 0)
            {
                var mu = (httpContext.User.Identity as IdentityPersonalizado);
                Authorization = mu.roles.Any(p => allowedroles.Contains(p));
                Authentication = Authorization;
                if (!Authorization) { throw new HttpException(403, "Forbidden"); }
            }
            return Authentication;
        }

        protected override HttpValidationStatus OnCacheAuthorization(HttpContextBase httpContext) {
            var result = base.OnCacheAuthorization(httpContext);

            return result;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                var redirect = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                    { "area", "" },
                    { "controller", "Account" },
                    { "action", "Login" },
                    { "ReturnUrl", filterContext.HttpContext.Request.RawUrl }
                    });

                filterContext.Result = redirect;
            }
        }
    }
}
