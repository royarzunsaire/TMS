using jsreport.Binary;
using jsreport.Local;
using jsreport.MVC;
using System;
using System.IO;
using System.Web.Mvc;

namespace SGC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new JsReportFilterAttribute(new LocalReporting()
                //.Configure(cfg => {
                //    cfg.Extensions.Express = new ExpressConfiguration() { InputRequestLimit = "50mb" };
                //    return cfg;
                //})
                .KillRunningJsReportProcesses()
                .TempDirectory(Path.Combine((string)AppDomain.CurrentDomain.GetData("DataDirectory"), "jsreport-temp"))
                .UseBinary(JsReportBinary.GetBinary())
                .AsUtility()
                .Create()));
            filters.Add(new HandleErrorAttribute());
        }
    }
}
