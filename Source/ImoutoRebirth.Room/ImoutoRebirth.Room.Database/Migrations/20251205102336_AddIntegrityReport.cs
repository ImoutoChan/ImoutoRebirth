using System;
using ImoutoRebirth.Room.Domain.IntegrityAggregate;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace ImoutoRebirth.Room.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrityReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:integrity_status", "hash_mismatch,missing,ok")
                .Annotation("Npgsql:Enum:report_status", "building,cancelled,completed,created,paused");

            migrationBuilder.CreateTable(
                name: "IntegrityReports",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedOn = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<ReportStatus>(type: "report_status", nullable: false),
                    ExpectedTotalFileCount = table.Column<int>(type: "integer", nullable: false),
                    ProcessedFileCount = table.Column<int>(type: "integer", nullable: false),
                    ExportToFolder = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrityReports", x => x.ReportId);
                });

            migrationBuilder.CreateTable(
                name: "IntegrityReportCollections",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionName = table.Column<string>(type: "text", nullable: false),
                    ProcessedFileCount = table.Column<int>(type: "integer", nullable: false),
                    ExpectedTotalFileCount = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    ReportExportedFiles = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrityReportCollections", x => new { x.ReportId, x.CollectionId });
                    table.ForeignKey(
                        name: "FK_IntegrityReportCollections_IntegrityReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "IntegrityReports",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IntegrityReportFileStatuses",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpectedFullPath = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: false),
                    ExpectedHash = table.Column<string>(type: "text", nullable: false),
                    ActualHash = table.Column<string>(type: "text", nullable: true),
                    ReadingProblem = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<IntegrityStatus>(type: "integrity_status", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrityReportFileStatuses", x => new { x.ReportId, x.CollectionId, x.FileId });
                    table.ForeignKey(
                        name: "FK_IntegrityReportFileStatuses_IntegrityReportCollections_Repo~",
                        columns: x => new { x.ReportId, x.CollectionId },
                        principalTable: "IntegrityReportCollections",
                        principalColumns: new[] { "ReportId", "CollectionId" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrityReportFileStatuses");

            migrationBuilder.DropTable(
                name: "IntegrityReportCollections");

            migrationBuilder.DropTable(
                name: "IntegrityReports");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:integrity_status", "hash_mismatch,missing,ok")
                .OldAnnotation("Npgsql:Enum:report_status", "building,cancelled,completed,created,paused");
        }
    }
}
