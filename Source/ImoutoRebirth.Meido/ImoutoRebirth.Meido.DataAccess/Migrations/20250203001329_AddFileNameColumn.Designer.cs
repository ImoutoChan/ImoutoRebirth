﻿// <auto-generated />
using System;
using ImoutoRebirth.Meido.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ImoutoRebirth.Meido.DataAccess.Migrations
{
    [DbContext(typeof(MeidoDbContext))]
    [Migration("20250203001329_AddFileNameColumn")]
    partial class AddFileNameColumn
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ImoutoRebirth.Meido.Domain.ParsingStatusAggregate.ParsingStatus", b =>
                {
                    b.Property<Guid>("FileId")
                        .HasColumnType("uuid");

                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.Property<Instant>("AddedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<string>("FileIdFromSource")
                        .HasColumnType("text");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Md5")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Instant>("ModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<Instant>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("FileId", "Source");

                    b.ToTable("ParsingStatuses");
                });

            modelBuilder.Entity("ImoutoRebirth.Meido.Domain.SourceActualizingStateAggregate.SourceActualizingState", b =>
                {
                    b.Property<int>("Source")
                        .HasColumnType("integer");

                    b.Property<Instant>("AddedOn")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant>("LastProcessedNoteUpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("LastProcessedTagHistoryId")
                        .HasColumnType("integer");

                    b.Property<Instant>("LastProcessedTagUpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant>("LastRequested")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Instant>("ModifiedOn")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Source");

                    b.ToTable("SourceActualizingStates");
                });
#pragma warning restore 612, 618
        }
    }
}
