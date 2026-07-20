using System.Web.Http;
using System.Web.UI.WebControls;
namespace spGenerator
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling =
    Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            // Enable CORS
            var cors = new System.Web.Http.Cors.EnableCorsAttribute(
                origins: "http://localhost:3000",
                headers: "*",
                methods: "*");
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = System.Web.Http.RouteParameter.Optional }
            );

        }
    }
}