using System.Web.Mvc;

namespace WebApplication.Areas.Utilidades
{
    public class UtilidadesAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Utilidades";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Utilidades_default",
                "Utilidades/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}