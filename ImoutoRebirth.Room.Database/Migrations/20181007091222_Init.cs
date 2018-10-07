using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ImoutoRebirth.Room.Database.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionFiles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    CollectionId = table.Column<long>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    Md5 = table.Column<string>(maxLength: 32, nullable: false),
                    Size = table.Column<long>(nullable: false),
                    OriginalPath = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionFiles_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DestinationFolders",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    CollectionId = table.Column<long>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    ShouldCreateSubfoldersByHash = table.Column<bool>(nullable: false),
                    ShouldRenameByHash = table.Column<bool>(nullable: false),
                    FormatErrorSubfolder = table.Column<string>(nullable: false, defaultValue: "!FormatError"),
                    HashErrorSubfolder = table.Column<string>(nullable: false, defaultValue: "!HashError"),
                    WithoutHashErrorSubfolder = table.Column<string>(nullable: false, defaultValue: "!WithoutHashError"),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DestinationFolders_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SourceFolders",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    AddedOn = table.Column<DateTimeOffset>(nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(nullable: false),
                    CollectionId = table.Column<long>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    ShouldCheckFormat = table.Column<bool>(nullable: false),
                    ShouldCheckHashFromName = table.Column<bool>(nullable: false),
                    ShouldCreateTagsFromSubfolders = table.Column<bool>(nullable: false),
                    ShouldAddTagFromFilename = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    SupportedExtensions = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SourceFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SourceFolders_Collections_CollectionId",
                        column: x => x.CollectionId,
                        principalTable: "Collections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFiles_CollectionId",
                table: "CollectionFiles",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_IsDeleted",
                table: "CollectionFiles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_IsDeleted",
                table: "Collections",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationFolders_CollectionId",
                table: "DestinationFolders",
                column: "CollectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IsDeleted",
                table: "DestinationFolders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_SourceFolders_CollectionId",
                table: "SourceFolders",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_IsDeleted",
                table: "SourceFolders",
                column: "IsDeleted");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionFiles");

            migrationBuilder.DropTable(
                name: "DestinationFolders");

            migrationBuilder.DropTable(
                name: "SourceFolders");

            migrationBuilder.DropTable(
                name: "Collections");
        }
    }
}
