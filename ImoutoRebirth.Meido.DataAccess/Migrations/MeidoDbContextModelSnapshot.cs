﻿// <auto-generated />
using System;
using ImoutoRebirth.Meido.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ImoutoRebirth.Meido.DataAccess.Migrations
{
    [DbContext(typeof(MeidoDbContext))]
    partial class MeidoDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ImoutoRebirth.Meido.Core.ParsingStatus.ParsingStatus", b =>
                {
                    b.Property<Guid>("FileId");

                    b.Property<int>("Source");

                    b.Property<DateTimeOffset>("AddedOn");

                    b.Property<string>("ErrorMessage");

                    b.Property<int?>("FileIdFromSource");

                    b.Property<string>("Md5")
                        .IsRequired();

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.Property<int>("Status");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("FileId", "Source");

                    b.ToTable("ParsingStatuses");
                });

            modelBuilder.Entity("ImoutoRebirth.Meido.Core.SourceActualizingState.SourceActualizingState", b =>
                {
                    b.Property<int>("Source");

                    b.Property<DateTimeOffset>("AddedOn");

                    b.Property<DateTimeOffset>("LastProcessedNoteUpdateAt");

                    b.Property<int>("LastProcessedTagHistoryId");

                    b.Property<DateTimeOffset>("LastProcessedTagUpdateAt");

                    b.Property<DateTimeOffset>("LastRequested");

                    b.Property<DateTimeOffset>("ModifiedOn");

                    b.HasKey("Source");

                    b.ToTable("SourceActualizingStates");
                });
#pragma warning restore 612, 618
        }
    }
}
