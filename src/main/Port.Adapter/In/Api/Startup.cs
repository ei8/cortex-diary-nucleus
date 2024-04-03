using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Owin;

namespace ei8.Cortex.Diary.Nucleus.Port.Adapter.In.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.AddHttpClient();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(buildFunc => buildFunc.UseNancy(o => o.Bootstrapper = new CustomBootstrapper(app.ApplicationServices)));
        }
    }
}
