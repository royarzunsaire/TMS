using System.Web.Optimization;

namespace SGC
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
          
            /*STYLES*/
            BundleTable.EnableOptimizations = false;



            bundles.Add(new StyleBundle("~/Content/font-awesome").Include("~/Content/plugins/font-awesome/css/font-awesome.css"));

            bundles.Add(new StyleBundle("~/Content/simple-icons").Include("~/Content/plugins/simple-line-icons/simple-line-icons.min.css"));
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/plugins/bootstrap/css/bootstrap.min.css"));
            bundles.Add(new StyleBundle("~/Content/select2").Include("~/Content/plugins/select2/css/select2.min.css"));
            bundles.Add(new StyleBundle("~/Content/select2boostrap").Include("~/Content/plugins/select2/css/select2-bootstrap.min.css"));
            bundles.Add(new StyleBundle("~/Content/component").Include("~/Content/template/css/components.min.css"));
            bundles.Add(new StyleBundle("~/Content/pluginscss").Include("~/Content/template/css/plugins.min.css"));
            bundles.Add(new StyleBundle("~/Content/login3").Include("~/Content/css/login-3.css"));
            bundles.Add(new StyleBundle("~/Content/datatables-css").Include("~/Content/DataTables/css/jquery.dataTables.min.css"));
            //bundles.Add(new StyleBundle("~/Content/").Include(""));
            //bundles.Add(new StyleBundle("~/Content/").Include(""));
            //bundles.Add(new StyleBundle("~/Content/").Include(""));
            //bundles.Add(new StyleBundle("~/Content/").Include(""));
            //bundles.Add(new StyleBundle("~/Content/").Include(""));
            //bundles.Add(new StyleBundle("~/Content/").Include(""));
            bundles.Add(new StyleBundle("~/Content/bootstrap-switch").Include("~/Content/plugins/bootstrap-switch/css/bootstrap-switch.min.css"));
            bundles.Add(new StyleBundle("~/Content/uniform").Include("~/Content/plugins/uniform/css/uniform.default.css"));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Content/plugins/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive-ajax.js",
                "~/Scripts/jquery.validate*",
                "~/Scripts/jquery.validate.unobtrusive.js",
                "~/Scripts/validarRut.js",
                "~/Content/plugins/jquery-validation/js/jquery.validate.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-migrate").Include("~/Content/plugins/jquery-migrate.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include("~/Content/plugins/jquery-ui/jquery-ui.min.js"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Content/plugins/bootstrap/js/bootstrap.min.js",
                "~/Content/bootstrap-hover-dropdown/bootstrap-hover-dropdown.min.js",
                "~/Content/plugins/bootstrap-switch/js/bootstrap-switch.min.js",
                "~/Content/plugins/bootstrap-daterangepicker/moment.min.js",
                "~/Content/plugins/bootstrap-daterangepicker/daterangepicker.js"
                ));
            bundles.Add(new ScriptBundle("~/bundles/jquery-plugins").Include(
            "~/Content/plugins/jquery-slimscroll/jquery.slimscroll.min.js",
            "~/Content/plugins/jquery.blockui.min.js",
            "~/Content/plugins/jquery.cokie.min.js",
            "~/Content/plugins/uniform/jquery.uniform.min.js",
            "~/Content/plugins/jquery.pulsate.min.js"

            ));
            bundles.Add(new ScriptBundle("~/scripts/datatables").Include("~/Scripts/DataTables/jquery.dataTables.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/template").Include(
                "~/Content/template/scripts/app.js",
                "~/Content/template/layout/scripts/layout.js",
                "~/Content/template/layout/scripts/demo.js"
                ));





        }
    }
}
