using System;
using ImoutoRebirth.Meido.Core;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Meido.DataAccess.Migrations;

public partial class InitSourceActualizingStates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        InsertInitial(migrationBuilder, MetadataSource.Yandere);
        InsertInitial(migrationBuilder, MetadataSource.Danbooru);
        InsertInitial(migrationBuilder, MetadataSource.Sankaku);
        InsertInitial(migrationBuilder, MetadataSource.Gelbooru);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData("SourceActualizingStates", "Source", 0);
        migrationBuilder.DeleteData("SourceActualizingStates", "Source", 1);
        migrationBuilder.DeleteData("SourceActualizingStates", "Source", 2);
        migrationBuilder.DeleteData("SourceActualizingStates", "Source", 4);
    }

    private static void InsertInitial(MigrationBuilder migrationBuilder, MetadataSource source)
    {
        migrationBuilder.InsertData(
            table: "SourceActualizingStates",
            columns: new[]
            {
                "Source",
                "AddedOn",
                "LastProcessedNoteUpdateAt",
                "LastProcessedTagHistoryId",
                "LastProcessedTagUpdateAt",
                "LastRequested",
                "ModifiedOn"
            },
            values: new object[]
            {
                (int)source,
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                0,
                DateTimeOffset.Now,
                DateTimeOffset.Now,
                DateTimeOffset.Now
            });
    }
}
