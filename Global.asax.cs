using SGC.Utils.ScheduledTask;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace SGC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Database.SetInitializer(new MigrateDatabaseToLatestVersion<SGC.Models.InsecapContext, Configuration>(useSuppliedContext:true));

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            JobScheduler.StartAsync().GetAwaiter().GetResult();
        }
    }
}
