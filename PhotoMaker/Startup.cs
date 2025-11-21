using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PhotoMaker.Startup))]
namespace PhotoMaker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
