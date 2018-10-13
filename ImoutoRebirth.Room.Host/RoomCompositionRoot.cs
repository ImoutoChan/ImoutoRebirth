using System;
using CacheManager.Core;
using EFSecondLevelCache.Core;
using ImoutoRebirth.Room.Core.Services;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Host.Environment;
using ImoutoRebirth.Room.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Host
{
    public class RoomCompositionRoot
    {
        private IEnvironmentProvider _environment;
        private IConfiguration _configuration;
        private RoomAppSettings _settings;
        private ServiceProvider _serviceProvider;

        public void Init(
            IEnvironmentProvider environmentProvider,
            IConfiguration configuration)
        {
            _environment = environmentProvider;
            _configuration = configuration;
            _settings = configuration.Get<RoomAppSettings>();

            var services = new ServiceCollection();

            RegisterDependencies(services);

            _serviceProvider = services.BuildServiceProvider();

            EFServiceProvider.ApplicationServices = _serviceProvider;
        }

        private void RegisterDependencies(ServiceCollection services)
        {
            services.AddDbContext<RoomDbContext>(builder
                => builder.UseNpgsql(_configuration.GetConnectionString("RoomDatabase")));

            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IImageService, ImageService>();

            services.AddTransient<IDestinationFolderService, DestinationFolderService>();
            services.AddTransient<ISourceFolderService, SourceFolderService>();

            RegisterCache(services);
        }

        public void RegisterCache(
            IServiceCollection services)
        {
            services.AddEFSecondLevelCache();

            // Add an in-memory cache service provider
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration),
                new CacheManager.Core.ConfigurationBuilder()
                   .WithJsonSerializer()
                   .WithMicrosoftMemoryCacheHandle()
                   .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
                   .Build());
        }
    }
}