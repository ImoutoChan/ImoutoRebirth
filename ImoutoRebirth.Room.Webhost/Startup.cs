using System;
using AutoMapper;
using CacheManager.Core;
using EFSecondLevelCache.Core;
using Hangfire;
using Hangfire.PostgreSql;
using ImoutoRebirth.Room.Core.Services;
using ImoutoRebirth.Room.Core.Services.Abstract;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.DataAccess.Cache;
using ImoutoRebirth.Room.DataAccess.Repositories;
using ImoutoRebirth.Room.DataAccess.Repositories.Abstract;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Infrastructure.Service;
using ImoutoRebirth.Room.Webhost.Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Webhost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            ConfigureDatabaseServices(services);
            ConfigureInfrastructureServices(services);
            ConfigureCoreServices(services);
            ConfigureCacheServices(services);
            ConfigureHangfireServices(services);
            ConfigureDataAccessServices(services);
            ConfigureAutoMapperServices(services);
        }

        private void ConfigureDatabaseServices(
            IServiceCollection services)
        {
            services.AddDbContext<RoomDbContext>(builder
                => builder.UseNpgsql(Configuration.GetConnectionString("RoomDatabase")));
        }

        public void ConfigureInfrastructureServices(
            IServiceCollection services)
        {
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IImageService, ImageService>();
        }

        public void ConfigureCoreServices(
            IServiceCollection services)
        {
            services.AddTransient<IDestinationFolderService, DestinationFolderService>();
            services.AddTransient<ISourceFolderService, SourceFolderService>();
            services.AddTransient<ICollectionFileService, CollectionFileService>();
            services.AddTransient<IFileSystemActualizationService, FileSystemActualizationService>();
        }

        public void ConfigureDataAccessServices(
            IServiceCollection services)
        {
            services.AddTransient<ICollectionFileRepository, CollectionFileRepository>();
            services.AddTransient<IDbStateService, DbStateService>();
            services.AddTransient<ICollectionRepository, CollectionRepository>();
            services.AddTransient<ICollectionFileCacheService, CollectionFileCacheService>();
        }

        public void ConfigureCacheServices(
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

        public void ConfigureHangfireServices(
            IServiceCollection services)
        {
            services.AddHangfire(configuration
                => configuration.UsePostgreSqlStorage(Configuration.GetConnectionString("RoomDatabase")));
            services.AddTransient<IHangfireStartup, HangfireStartup>();
        }

        public void ConfigureAutoMapperServices(
            IServiceCollection services)
        {
            services.AddAutoMapper(expression => expression.AddProfile(typeof(ModelAutoMapperProfile)));
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env)
        {
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
        }
    }
}