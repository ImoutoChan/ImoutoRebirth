using System;
using AutoMapper;
using CacheManager.Core;
using EFSecondLevelCache.Core;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Room.Core;
using ImoutoRebirth.Room.DataAccess;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Infrastructure;
using ImoutoRebirth.Room.WebApi;
using ImoutoRebirth.Room.WebApi.Controllers;
using ImoutoRebirth.Room.Webhost.Quartz;
using ImoutoRebirth.Room.Webhost.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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
            services.AddControllers()
                    .AddNewtonsoftJson()
                    .AddApplicationPart(typeof(CollectionsController).Assembly);
            
            services.AddRoomServices();

            services.AddTrueMassTransit(
                RoomSettings.RabbitSettings,
                ReceiverApp.Name,
                с => с.AddRoomServicesForRabbit());

            services.AddRoomCore();
            services.AddRoomDataAccess();
            services.AddRoomDatabase();

            services.AddQuartz()
                    .AddQuartzJob<OverseeJob, OverseeJob.Description>();

            ConfigureDatabaseServices(services);
            ConfigureCacheServices(services);
            ConfigureAutoMapperServices(services);
            ConfigureSwaggerServices(services);
        }

        private void ConfigureDatabaseServices(IServiceCollection services)
        {
            services.AddDbContext<RoomDbContext>(builder
                => builder.UseNpgsql(Configuration.GetConnectionString("RoomDatabase")));
        }

        private void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c 
                => c.SwaggerDoc("v1", new OpenApiInfo
                                {
                                    Title = "ImoutoRebirth.Room API",
                                    Version = "v1"
                                }));
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

        public void ConfigureAutoMapperServices(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ModelAutoMapperProfile), typeof(DtoAutoMapperProfile));
        }

        public void Configure(
            IApplicationBuilder app,
            IHostEnvironment env,
            IMapper mapper)
        {
            //mapper.ConfigurationProvider.AssertConfigurationIsValid();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(builder => builder.MapControllers());
            
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ImoutoRebirth.Room API V1"));

            app.UseQuartz();
        }
    }
}