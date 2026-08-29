using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace ImoutoRebirth.Lilin.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTagAliases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagAliases",
                columns: table => new
                {
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    AliasTagId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedOn = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ModifiedOn = table.Column<Instant>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagAliases", x => new { x.TagId, x.AliasTagId });
                    table.ForeignKey(
                        name: "FK_TagAliases_Tags_AliasTagId",
                        column: x => x.AliasTagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagAliases_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagAliases_AliasTagId",
                table: "TagAliases",
                column: "AliasTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagAliases");
        }
    }
}
