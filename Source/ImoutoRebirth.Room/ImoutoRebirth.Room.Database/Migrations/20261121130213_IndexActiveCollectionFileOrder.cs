using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImoutoRebirth.Room.Database.Migrations
{
    public partial class IndexActiveCollectionFileOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CollectionFiles_AddedOn_Id",
                table: "CollectionFiles",
                columns: new[] { "AddedOn", "Id" },
                filter: "NOT \"IsRemoved\"");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFiles_CollectionId_AddedOn_Id",
                table: "CollectionFiles",
                columns: new[] { "CollectionId", "AddedOn", "Id" },
                filter: "NOT \"IsRemoved\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionFiles_AddedOn_Id",
                table: "CollectionFiles");

            migrationBuilder.DropIndex(
                name: "IX_CollectionFiles_CollectionId_AddedOn_Id",
                table: "CollectionFiles");
        }
    }
}
