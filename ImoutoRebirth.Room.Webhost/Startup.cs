using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AutoMapper;
using ImoutoRebirth.Common;
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
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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

            services.AddQuartzJob<OverseeJob, OverseeJob.Description>();

            ConfigureDatabaseServices(services);
            ConfigureAutoMapperServices(services);
            ConfigureSwaggerServices(services);
        }

        private void ConfigureDatabaseServices(IServiceCollection services)
        {
            services.AddDbContext<RoomDbContext>(builder
                => builder.UseNpgsql(Configuration.GetConnectionString("RoomDatabase")));
        }

        private static void ConfigureSwaggerServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.CustomOperationIds(e =>
                {
                    var actionDescriptor = (ControllerActionDescriptor) e.ActionDescriptor;
                    return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}";
                });
                c.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Title = "ImoutoRebirth.Room API",
                    Version = "v1.0"
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{typeof(CollectionsController).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        private static string Capitalize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentNullException(nameof(input));
            var length = input.Length;

            if (string.IsInterned(input) == null)
            {
                var span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(input.AsSpan()), length);
                input.AsSpan(0, 1).ToUpperInvariant(span.Slice(0, 1));
                input.AsSpan(1).ToLowerInvariant(span.Slice(1));

                return input;
            }

            char[] pool = null;

            var tempArray
                = length <= 512
                    ? stackalloc char[length]
                    : pool = ArrayPool<char>.Shared.Rent(length);

            input.AsSpan(0, 1).ToUpperInvariant(tempArray);
            input.AsSpan(1).ToLowerInvariant(tempArray[1..]);

            var result = tempArray.ToString();

            if (pool != null)
                ArrayPool<char>.Shared.Return(pool);

            return result;
        }

        private static void ConfigureAutoMapperServices(IServiceCollection services)
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
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseEndpoints(builder => builder.MapControllers());
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "ImoutoRebirth.Room API V1.0");
            });
        }
    }
}