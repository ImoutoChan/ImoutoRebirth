using System;
using System.Threading.Tasks;
using AutoMapper;
using CacheManager.Core;
using EFSecondLevelCache.Core;
using Hangfire;
using Hangfire.PostgreSql;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Room.Core.Services;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Repositories;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Infrastructure;
using ImoutoRebirth.Room.Infrastructure.Service;
using ImoutoRebirth.Room.WebApi.Controllers;
using ImoutoRebirth.Room.Webhost.Hangfire;
using ImoutoRebirth.Room.Webhost.Settings;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace ImoutoRebirth.Room.Webhost
{
    public class Startup
    {
        public RoomAppSettings RoomSettings { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            RoomSettings = configuration.Get<RoomAppSettings>();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                    .AddApplicationPart(typeof(CollectionsController).Assembly);

            ConfigureDatabaseServices(services);

            services.AddRoomServices();
            services.AddMassTransitRabbitMqHostedService(
                        ReceiverApp.Name,
                        RoomSettings.RabbitSettings.ToOptions())
                    .AddRoomServicesForRabbit();

            ConfigureCoreServices(services);
            ConfigureCacheServices(services);
            ConfigureHangfireServices(services);
            ConfigureDataAccessServices(services);
            ConfigureAutoMapperServices(services);
            ConfigureSwaggerServices(services);
        }

        private void ConfigureDatabaseServices(IServiceCollection services)
        {
            services.AddDbContext<RoomDbContext>(builder
                => builder.UseNpgsql(Configuration.GetConnectionString("RoomDatabase")));
        }
        private void ConfigureSwaggerServices(
            IServiceCollection services)
        {
            services.AddSwaggerGen(c 
                => c.SwaggerDoc("v1", new Info { Title = "ImoutoRebirth.Room API", Version = "v1" }));
        }
        
        public void ConfigureCoreServices(IServiceCollection services)
        {
            services.AddTransient<IDestinationFolderService, DestinationFolderService>();
            services.AddTransient<ISourceFolderService, SourceFolderService>();
            services.AddTransient<ICollectionFileService, CollectionFileService>();
            services.AddTransient<IFileSystemActualizationService, FileSystemActualizationService>();
        }

        public void ConfigureDataAccessServices(IServiceCollection services)
        {
            services.AddTransient<ICollectionFileRepository, CollectionFileRepository>();
            services.AddTransient<IDbStateService, DbStateService>();
            services.AddTransient<ICollectionRepository, CollectionRepository>();
            services.AddTransient<ISourceFolderRepository, SourceFolderRepository>();
            services.AddTransient<IDestinationFolderRepository, DestinationFolderRepository>();
            services.AddTransient<ICollectionFileCacheService, CollectionFileCacheService>();
        }

        public void ConfigureCacheServices(IServiceCollection services)
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

        public void ConfigureHangfireServices(IServiceCollection services)
        {
            services.AddHangfire(configuration
                => configuration.UsePostgreSqlStorage(Configuration.GetConnectionString("RoomDatabase")));
            services.AddTransient<IHangfireStartup, HangfireStartup>();
        }

        public void ConfigureAutoMapperServices(IServiceCollection services)
        {
            services.AddAutoMapper();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            IMapper mapper)
        {
            //mapper.ConfigurationProvider.AssertConfigurationIsValid();

            MigrateIfNecessary(app).Wait();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseEFSecondLevelCache();
            app.UseHangfireDashboard();
            app.UseHangfireServer();
            app.UseHangfireJobs();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ImoutoRebirth.Room API V1"));
        }

        private static async Task MigrateIfNecessary(IApplicationBuilder app)
        {

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;

                var logger = services.GetRequiredService<ILogger<Startup>>();
                var context = services.GetRequiredService<RoomDbContext>();
                
                await context.Database.MigrateAsync();

                var migrations = await context.Database.GetAppliedMigrationsAsync();
                foreach (var migration in migrations)
                    logger.LogInformation($"Migrated to {migration}");
            }
        }
    }
}