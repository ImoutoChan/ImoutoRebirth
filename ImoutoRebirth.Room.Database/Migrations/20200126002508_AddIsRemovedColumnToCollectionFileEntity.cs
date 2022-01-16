using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Room.Database.Migrations;

public partial class AddIsRemovedColumnToCollectionFileEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsRemoved",
            table: "CollectionFiles",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_CollectionFiles_IsRemoved",
            table: "CollectionFiles",
            column: "IsRemoved");

        migrationBuilder.CreateIndex(
            name: "IX_CollectionFiles_Md5",
            table: "CollectionFiles",
            column: "Md5");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_CollectionFiles_IsRemoved",
            table: "CollectionFiles");

        migrationBuilder.DropIndex(
            name: "IX_CollectionFiles_Md5",
            table: "CollectionFiles");

        migrationBuilder.DropColumn(
            name: "IsRemoved",
            table: "CollectionFiles");
    }
}