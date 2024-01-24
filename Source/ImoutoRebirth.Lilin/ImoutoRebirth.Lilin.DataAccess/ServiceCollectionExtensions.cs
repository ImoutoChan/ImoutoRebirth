using ImoutoRebirth.Common.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLilinDataAccess(this IServiceCollection services, string connectionString) 
        => services.AddPostgresDbContext<LilinDbContext>(connectionString);
}
