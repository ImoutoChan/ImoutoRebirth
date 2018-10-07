﻿// <auto-generated />
using System;
using ImoutoRebirth.Room.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ImoutoRebirth.Room.Database.Migrations
{
    [DbContext(typeof(RoomDbContext))]
    partial class RoomDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ImoutoRebirth.Room.Database.Entities.CollectionEntity", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<DateTimeOffset>("AddedOn");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("IsDeleted")
                        .HasName("IX_CollectionEntity_IsDeleted");

                    b.ToTable("Collections");
                });

            modelBuilder.Entity("ImoutoRebirth.Room.Database.Entities.CollectionFileEntity", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<DateTimeOffset>("AddedOn");

                    b.Property<Guid>("CollectionId");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Md5")
                        .IsRequired()
                        .HasMaxLength(32);

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<string>("OriginalPath");

                    b.Property<string>("Path")
                        .IsRequired();

                    b.Property<long>("Size");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("IsDeleted")
                        .HasName("IX_CollectionFileEntity_IsDeleted");

                    b.ToTable("CollectionFiles");
                });

            modelBuilder.Entity("ImoutoRebirth.Room.Database.Entities.DestinationFolderEntity", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<DateTimeOffset>("AddedOn");

                    b.Property<Guid>("CollectionId");

                    b.Property<string>("FormatErrorSubfolder")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("!FormatError");

                    b.Property<string>("HashErrorSubfolder")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("!HashError");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<string>("Path")
                        .IsRequired();

                    b.Property<bool>("ShouldCreateSubfoldersByHash");

                    b.Property<bool>("ShouldRenameByHash");

                    b.Property<string>("WithoutHashErrorSubfolder")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("!WithoutHashError");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId")
                        .IsUnique();

                    b.HasIndex("IsDeleted")
                        .HasName("IX_DestinationFolderEntity_IsDeleted");

                    b.ToTable("DestinationFolders");
                });

            modelBuilder.Entity("ImoutoRebirth.Room.Database.Entities.SourceFolderEntity", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<DateTimeOffset>("AddedOn");

                    b.Property<Guid>("CollectionId");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<string>("Path")
                        .IsRequired();

                    b.Property<bool>("ShouldAddTagFromFilename");

                    b.Property<bool>("ShouldCheckFormat");

                    b.Property<bool>("ShouldCheckHashFromName");

                    b.Property<bool>("ShouldCreateTagsFromSubfolders");

                    b.Property<string>("SupportedExtensions");

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("IsDeleted")
                        .HasName("IX_SourceFolderEntity_IsDeleted");

                    b.ToTable("SourceFolders");
                });

            modelBuilder.Entity("ImoutoRebirth.Room.Database.Entities.CollectionFileEntity", b =>
                {
                    b.HasOne("ImoutoRebirth.Room.Database.Entities.CollectionEntity", "Collection")
                        .WithMany("Files")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ImoutoRebirth.Room.Database.Entities.DestinationFolderEntity", b =>
                {
                    b.HasOne("ImoutoRebirth.Room.Database.Entities.CollectionEntity", "Collection")
                        .WithOne("DestinationFolder")
                        .HasForeignKey("ImoutoRebirth.Room.Database.Entities.DestinationFolderEntity", "CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ImoutoRebirth.Room.Database.Entities.SourceFolderEntity", b =>
                {
                    b.HasOne("ImoutoRebirth.Room.Database.Entities.CollectionEntity", "Collection")
                        .WithMany("SourceFolders")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
