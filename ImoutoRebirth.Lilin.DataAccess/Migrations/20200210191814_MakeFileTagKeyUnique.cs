using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Lilin.DataAccess.Migrations
{
    public partial class MakeFileTagKeyUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileTags_FileId_TagId_Source",
                table: "FileTags");

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_FileId_TagId_Source",
                table: "FileTags",
                columns: new[] { "FileId", "TagId", "Source" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FileTags_FileId_TagId_Source",
                table: "FileTags");

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_FileId_TagId_Source",
                table: "FileTags",
                columns: new[] { "FileId", "TagId", "Source" });
        }
    }
}
