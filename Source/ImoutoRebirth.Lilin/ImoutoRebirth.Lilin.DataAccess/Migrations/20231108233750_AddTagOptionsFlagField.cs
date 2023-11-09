using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImoutoRebirth.Lilin.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTagOptionsFlagField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Options",
                table: "Tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Options",
                table: "Tags");
        }
    }
}
