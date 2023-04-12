using Microsoft.Owin;
using Owin;
using System;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(DoAn.App_Start.Startup))]

namespace DoAn.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.UseFacebookAuthentication(
            appId: "610082339643432",
            appSecret: "ff956408f61a534b362b502851bdaf0b");
        }
    }
}
