using System.Reflection;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lamia.Infrastructure;
using ImoutoRebirth.Lamia.IntegrationTests.Framework;
using ImoutoRebirth.Lamia.MessageContracts;
using ImoutoRebirth.Lamia.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ImoutoRebirth.Lamia.IntegrationTests.Fixtures;

[CollectionDefinition("ApplicationFactory")]
public class LamiaApplicationCollection : ICollectionFixture<LamiaApplicationFactory<Program>>;

public class LamiaApplicationFactory<TProgram>
    : GenericHostApplicationFactory<TProgram> where TProgram : class
{
    public string TestsLocation
        => new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;

    public IServiceScope GetScope() => Services.CreateScope();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // services.Configure<FFmpegOptions>(x => x.Path = @"C:\Program Files\FFmpeg");

            services.AddMassTransitTestHarness(
                configure: с
                    => с.AddLamiaMassTransitSetup()
                        .AddCommand<ExtractFileMetadataCommand>(),
                shouldAutoConfigureEndpoints: true,
                consumingServiceName: "lamia",
                cleanExistingMassTransitServices: true,
                typeof(ExtractFileMetadataCommandConsumer).Assembly);
        });

        return base.CreateHost(builder);
    }
}
