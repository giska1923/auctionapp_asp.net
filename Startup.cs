using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Aukcija.Startup))]
namespace Aukcija
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
