using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Common.Quartz.Extensions
{
    public static class QuartzApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseQuartz(this IApplicationBuilder builder)
        {
            var quartz = builder.ApplicationServices.GetRequiredService<QuartzStartup>();
            var lifetime = builder.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            lifetime.ApplicationStarted.Register(quartz.Start);
            lifetime.ApplicationStopped.Register(quartz.Stop);

            return builder;
        }
    }
}