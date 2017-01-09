using System.Web;
using System.Web.Optimization;

namespace PP.Signicat.CredentialManager
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include("~/Scripts/jquery-{version}.js",
                "~/Scripts/jquery-{version}.min.js",
                "~/Scripts/DataTables/jquery.dataTables.js",
                "~/Scripts/bootstrap.js",
                "~/Scripts/respond.js",
                "~/Scripts/moment-with-locales*",
                "~/Scripts/bootstrap-datetimepicker*",
                "~/Scripts/locales/bootstrap-datepicker.no.min.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-datetimepicker*",
                      "~/Content/DataTables/css/jquery.dataTables*",
                      "~/Content/bootstrap-theme.css",
                      "~/Content/font-awesome.css",
                      "~/Content/jquery-ui.css",
                      "~/Content/site.css"));
        }
    }
}
