using System.Web.Mvc;
using System.Web.Routing;

namespace SGC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.MapRoute(
                 "GetFileFromBlob",
                 "Relator/GetFileFromBlob/{tipoArchivo}/{fileName}",
                 new { controller = "Relator", action = "GetFileFromBlob" });
            routes.MapRoute(
                  "uploadForm",
                  "Relator/UploadTestFile/{tipoArchivo}",
                  new { controller = "Relator", action = "UploadTestFile" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default2",
                url: "{controller}/{action}/{id}/{id2}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default3",
                url: "{controller}/{action}/{id}/{id2}/{id3}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id2 = UrlParameter.Optional, id3 = UrlParameter.Optional }
            );
        }
    }
}
