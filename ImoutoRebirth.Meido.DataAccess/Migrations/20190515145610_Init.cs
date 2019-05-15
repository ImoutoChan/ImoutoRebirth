using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Meido.DataAccess.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParsingStatuses",
                columns: table => new
                {
                    FileId = table.Column<Guid>(nullable: false),
                    Source = table.Column<int>(nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    Md5 = table.Column<string>(nullable: false),
                    FileIdFromSource = table.Column<int>(nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    ErrorMessage = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParsingStatuses", x => new { x.FileId, x.Source });
                });

            migrationBuilder.CreateTable(
                name: "SourceActualizingStates",
                columns: table => new
                {
                    Source = table.Column<int>(nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    LastProcessedTagHistoryId = table.Column<int>(nullable: false),
                    LastProcessedTagUpdateAt = table.Column<DateTimeOffset>(nullable: false),
                    LastProcessedNoteUpdateAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceActualizingStates", x => x.Source);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParsingStatuses");

            migrationBuilder.DropTable(
                name: "SourceActualizingStates");
        }
    }
}
