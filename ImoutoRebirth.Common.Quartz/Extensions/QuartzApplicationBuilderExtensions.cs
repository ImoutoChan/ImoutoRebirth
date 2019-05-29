using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Common.Quartz.Extensions
{
    public static class QuartzApplicationBuilderExtensions
    {
        [Obsolete("Use [Web]HostBuilderExtensions.UseQuartz instead.")]
        public static IApplicationBuilder UseQuartz(this IApplicationBuilder builder)
        {
            var quartz = builder.ApplicationServices.GetRequiredService<Quartz.QuartzHostedService>();
            var lifetime = builder.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            lifetime.ApplicationStarted.Register(quartz.Start);
            lifetime.ApplicationStopped.Register(quartz.Stop);

            return builder;
        }
    }
}