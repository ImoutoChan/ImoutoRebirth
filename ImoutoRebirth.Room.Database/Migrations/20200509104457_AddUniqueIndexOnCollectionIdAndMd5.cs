using ImoutoRebirth.Room.Database.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Room.Database.Migrations
{
    public partial class AddUniqueIndexOnCollectionIdAndMd5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string collectionFilesName = nameof(RoomDbContext.CollectionFiles);
            const string collectionIdName = nameof(CollectionFileEntity.CollectionId);
            const string md5Name = nameof(CollectionFileEntity.Md5);
            const string isRemovedName = nameof(CollectionFileEntity.IsRemoved);

            migrationBuilder.Sql(
                @$"CREATE UNIQUE INDEX ""IX_CollectionFiles_CollectionId_Md5_IsRemoved""
                    ON ""{collectionFilesName}"" (""{collectionIdName}"", ""{md5Name}"", ""{isRemovedName}"")
                  WHERE NOT ""{isRemovedName}"";");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionFiles_CollectionId_Md5_IsRemoved",
                table: "CollectionFiles");
        }
    }
}