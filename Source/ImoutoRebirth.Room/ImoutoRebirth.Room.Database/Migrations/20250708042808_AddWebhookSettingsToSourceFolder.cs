using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImoutoRebirth.Room.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookSettingsToSourceFolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebhookUploadUrl",
                table: "SourceFolders",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsWebhookUploadEnabled",
                table: "SourceFolders",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebhookUploadUrl",
                table: "SourceFolders");

            migrationBuilder.DropColumn(
                name: "IsWebhookUploadEnabled",
                table: "SourceFolders");
        }
    }
}
