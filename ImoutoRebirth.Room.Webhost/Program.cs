using System.Threading.Tasks;
using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Room.Database;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ImoutoRebirth.Room.Webhost
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateWebHostBuilder(args)
               .Build()
               .MigrateIfNecessary<RoomDbContext>()
               .RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>();
    }
}
