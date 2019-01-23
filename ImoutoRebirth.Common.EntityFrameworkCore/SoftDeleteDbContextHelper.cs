using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ImoutoRebirth.Common.EntityFrameworkCore
{
    public class SoftDeleteDbContextHelper<TEntityBase> where TEntityBase : class
    {
        private const string _isDeletedProperty = "IsDeleted";
        private static readonly MethodInfo _propertyMethod =
            typeof(EF)
               .GetMethod(nameof(EF.Property), BindingFlags.Static | BindingFlags.Public)
              ?.MakeGenericMethod(typeof(bool));
        
        public void OnBeforeSaveChanges(ChangeTracker tracker)
        {
            foreach (var entry in tracker
                                 .Entries<TEntityBase>()
                                 .Where(e => e.State == EntityState.Deleted))
            {
                entry.Property(_isDeletedProperty).CurrentValue = true;
                entry.State = EntityState.Modified;
            }
        }

        public void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(TEntityBase).IsAssignableFrom(entity.ClrType))
                    continue;

                entity.AddProperty(_isDeletedProperty, typeof(bool));

                modelBuilder
                   .Entity(entity.ClrType)
                   .HasQueryFilter(GetIsDeletedRestriction(entity.ClrType))
                   .HasIndex(_isDeletedProperty)
                   .HasName($"IX_{entity.ClrType.Name}_{_isDeletedProperty}");
            }
        }

        private static LambdaExpression GetIsDeletedRestriction(Type type)
        {
            var param = Expression.Parameter(type, "it");
            var prop = Expression.Call(_propertyMethod, param, Expression.Constant(_isDeletedProperty));
            var condition = Expression.MakeBinary(ExpressionType.Equal, prop, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, param);
            return lambda;
        }
    }
}