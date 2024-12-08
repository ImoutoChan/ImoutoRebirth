using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;

namespace ImoutoRebirth.Common.EntityFrameworkCore;

public static class DbContextExtensions
{
    private static readonly ConcurrentDictionary<Type, (string Select, string With)> OpenJsonCache = [];

    public static IQueryable<T> AsQueryable<T>(this DbContext dbContext, IEnumerable<T> items)
        where T : class
    {
        var (select, with) = OpenJsonCache.GetOrAdd(typeof(T), static (type, context) =>
        {
            var service = context.GetService<IRelationalTypeMappingSource>();

            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
                .Select(x =>
                {
                    var storeType = service.FindMapping(x.PropertyType)?.StoreType
                                    ?? throw new InvalidOperationException("No store type");

                    return new { x.Name, StoreType = storeType };
                })
                .ToArray();

            var select = string.Join(", ", properties.Select(x => $"t.\"{x.Name}\""));
            var with = string.Join(", ", properties.Select(x => $"\"{x.Name}\" {x.StoreType}"));

            return (select, with);
        }, dbContext);

        var json = JsonSerializer.Serialize(items);
        var jsonParam = new NpgsqlParameter("json", NpgsqlDbType.Jsonb) { Value = json };

        var sql = $"SELECT {select} FROM jsonb_to_recordset(@json::jsonb) AS t({with})";

        return dbContext.Database.SqlQueryRaw<T>(sql, jsonParam);
    }
}
