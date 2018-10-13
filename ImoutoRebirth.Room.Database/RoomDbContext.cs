﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using ImoutoRebirth.Room.Database.Entities;
using ImoutoRebirth.Room.Database.Entities.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ImoutoRebirth.Room.Database
{
    public class RoomDbContext : DbContext
    {
        private const string _isDeletedProperty = "IsDeleted";
        private static readonly MethodInfo _propertyMethod = 
            typeof(EF)
               .GetMethod(nameof(EF.Property), BindingFlags.Static | BindingFlags.Public)
              ?.MakeGenericMethod(typeof(bool));

        public DbSet<CollectionEntity> Collections { get; set; }

        public DbSet<SourceFolderEntity> SourceFolders { get; set; }

        public DbSet<DestinationFolderEntity> DestinationFolders { get; set; }

        public DbSet<CollectionFileEntity> CollectionFiles { get; set; }

        public RoomDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildSourceFolderEntity(modelBuilder);
            BuildDestinationFolderEntity(modelBuilder);
            BuildCollectionFileEntity(modelBuilder);

            SetupSoftDelete(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            FillUpdateDates();
            SoftDelete();

            var changedEntityNames = GetChangedNames();

            var result = base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            InvalidateSecondLevelCache(changedEntityNames);

            return result;
        }

        private void InvalidateSecondLevelCache(string[] changedEntityNames)
        {
            this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);
        }

        private string[] GetChangedNames()
        {
            ChangeTracker.DetectChanges();
            return this.GetChangedEntityNames();
        }

        private void SetupSoftDelete(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(EntityBase).IsAssignableFrom(entity.ClrType))
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
            var parm = Expression.Parameter(type, "it");
            var prop = Expression.Call(_propertyMethod, parm, Expression.Constant(_isDeletedProperty));
            var condition = Expression.MakeBinary(ExpressionType.Equal, prop, Expression.Constant(false));
            var lambda = Expression.Lambda(condition, parm);
            return lambda;
        }

        private void SoftDelete()
        {
            foreach (var entry in ChangeTracker
                                 .Entries<EntityBase>()
                                 .Where(e => e.State == EntityState.Deleted))
            {
                entry.Property(_isDeletedProperty).CurrentValue = true;
                entry.State = EntityState.Modified;
            }
        }

        private void FillUpdateDates()
        {
            foreach (var entityEntry in ChangeTracker.Entries<EntityBase>())
            {
                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        entityEntry.Entity.AddedOn = DateTimeOffset.Now;
                        entityEntry.Entity.ModifiedOn = DateTimeOffset.Now;
                        break;
                    case EntityState.Modified:
                        entityEntry.Entity.ModifiedOn = DateTimeOffset.Now;
                        break;
                }
            }
        }

        private static void BuildDestinationFolderEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
               .Entity<DestinationFolderEntity>()
               .Property(x => x.FormatErrorSubfolder);

            modelBuilder
               .Entity<DestinationFolderEntity>()
               .Property(x => x.HashErrorSubfolder);

            modelBuilder
               .Entity<DestinationFolderEntity>()
               .Property(x => x.WithoutHashErrorSubfolder);
        }

        private static void BuildSourceFolderEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SourceFolderEntity>()
                        .Property<string>("SupportedExtensions")
                        .HasField("_supportedExtensions");
        }

        private static void BuildCollectionFileEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CollectionFileEntity>()
                        .HasIndex(entity => entity.Path);
        }
    }
}