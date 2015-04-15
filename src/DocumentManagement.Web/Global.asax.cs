using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using DocumentManagement.Service;

namespace DocumentManagement.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            AutoMapperService.Setup();
        }
    }
}
