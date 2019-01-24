using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Lilin.DataAccess.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    Source = table.Column<int>(nullable: false),
                    SourceId = table.Column<int>(nullable: true),
                    Label = table.Column<string>(nullable: false),
                    PositionFromLeft = table.Column<int>(nullable: false),
                    PositionFromTop = table.Column<int>(nullable: false),
                    Width = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Color = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    TypeId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    HasValue = table.Column<bool>(nullable: false),
                    Synonyms = table.Column<string>(nullable: true),
                    Count = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_TagTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "TagTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FileTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    TagId = table.Column<Guid>(nullable: false),
                    Value = table.Column<string>(nullable: true),
                    Source = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_FileId",
                table: "FileTags",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_TagId",
                table: "FileTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_Source_TagId",
                table: "FileTags",
                columns: new[] { "Source", "TagId" });

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_TagId_Value",
                table: "FileTags",
                columns: new[] { "TagId", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_FileTags_FileId_TagId_Source",
                table: "FileTags",
                columns: new[] { "FileId", "TagId", "Source" });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_FileId",
                table: "Notes",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TypeId_Name",
                table: "Tags",
                columns: new[] { "TypeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TagTypes_Name",
                table: "TagTypes",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileTags");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TagTypes");
        }
    }
}
