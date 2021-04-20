using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ImoutoRebirth.Common.EntityFrameworkCore
{
    internal static class SoftDeleteDbContextHelper
    {
        internal static readonly MethodInfo EfPropertyMethod =
            typeof(EF)
                .GetMethod(nameof(EF.Property), BindingFlags.Static | BindingFlags.Public)
                ?.MakeGenericMethod(typeof(bool))!;
    }

    public class SoftDeleteDbContextHelper<TEntityBase> where TEntityBase : class
    {
        private const string IsDeletedProperty = "IsDeleted";

        public void OnBeforeSaveChanges(ChangeTracker tracker)
        {
            foreach (var entry in tracker
                                 .Entries<TEntityBase>()
                                 .Where(e => e.State == EntityState.Deleted))
            {
                entry.Property(IsDeletedProperty).CurrentValue = true;
                entry.State = EntityState.Modified;
            }
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(TEntityBase).IsAssignableFrom(entity.ClrType))
                    continue;

                entity.AddProperty(IsDeletedProperty, typeof(bool));

                modelBuilder
                   .Entity(entity.ClrType)
                   .HasQueryFilter(GetIsDeletedRestriction(entity.ClrType))
                   .HasIndex(IsDeletedProperty)
                   .HasDatabaseName($"IX_{entity.ClrType.Name}_{IsDeletedProperty}");
            }
        }

        private static LambdaExpression GetIsDeletedRestriction(Type type)
        {
            var param = Expression.Parameter(type, "it");
            var prop = Expression.Call(
                SoftDeleteDbContextHelper.EfPropertyMethod,
                param,
                Expression.Constant(IsDeletedProperty));

            var condition = Expression.MakeBinary(ExpressionType.Equal, prop, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, param);
            return lambda;
        }
    }
}
