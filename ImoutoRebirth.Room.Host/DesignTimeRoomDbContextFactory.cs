using System;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ImoutoRebirth.Room.Host
{
    public class DesignTimeRoomDbContextFactory : IDesignTimeDbContextFactory<RoomDbContext>
    {
        public RoomDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                      .AddJsonFile("appsettings.json", true)
                                      .AddJsonFile("appsettings.Development.json", true)
                                      .Build();

            var connectionString = configuration.GetConnectionString("RoomDatabase");

            var builder = new DbContextOptionsBuilder<RoomDbContext>();

            builder.UseNpgsql(connectionString);

            return new RoomDbContext(builder.Options);
        }
    }
}