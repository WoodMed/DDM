using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(BP_DistributionMatrix.Startup))]

// Files related to ASP.NET Identity duplicate the Microsoft ASP.NET Identity file structure and contain initial Microsoft comments.

namespace BP_DistributionMatrix
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {

        }
    }
}