using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SGC.Startup))]
namespace SGC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

        }
    }
}
