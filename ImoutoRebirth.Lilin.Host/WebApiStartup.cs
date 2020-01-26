using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Lilin.Host.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Lilin.Host
{
    public class WebApiStartup : BaseStartup
    {
        private LilinSettings LilinSettings { get; }

        public WebApiStartup(IConfiguration configuration) 
            : base(configuration)
        {
            LilinSettings = configuration.Get<LilinSettings>();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}