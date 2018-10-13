using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Webhost.Hangfire
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder builder)
        {
            var services = builder.ApplicationServices;
            var applicationLifetime = services.GetRequiredService<IApplicationLifetime>();
            var hangfireStartup = services.GetRequiredService<IHangfireStartup>();

            applicationLifetime.ApplicationStarted.Register(() => hangfireStartup.EnqueueJobs());

            return builder;
        }
    }
}