using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Room.Database.Migrations
{
    public partial class AddUniqueIndexOnCollectionIdAndMd5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CollectionFiles_CollectionId_Md5_IsRemoved",
                table: "CollectionFiles",
                columns: new[] { "CollectionId", "Md5", "IsRemoved" },
                unique: true,
                filter: "NOT \"IsRemoved\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionFiles_CollectionId_Md5_IsRemoved",
                table: "CollectionFiles");
        }
    }
}