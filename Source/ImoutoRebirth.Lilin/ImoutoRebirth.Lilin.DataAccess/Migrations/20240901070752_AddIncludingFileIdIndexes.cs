using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImoutoRebirth.Lilin.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIncludingFileIdIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_FileTags_TagId_Including_FileId"
                ON public."FileTags" ("TagId") INCLUDE ("FileId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_FileTags_TagId_Value_Including_FileId"
                ON public."FileTags" ("TagId", "Value") INCLUDE ("FileId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_Tags_Lower_Name"
                ON public."Tags" (lower("Name"));
                """);

            migrationBuilder.Sql(
                """
                CREATE EXTENSION IF NOT EXISTS pg_trgm;
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_Tags_Lower_Name_GIN" ON "Tags" USING GIN (lower("Name") gin_trgm_ops);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_Tags_Lower_Name_GIN";
                """);

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_Tags_Lower_Name";
                """);

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_FileTags_TagId_Value_Including_FileId";
                """);

            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS "IX_FileTags_TagId_Including_FileId";
                """);

            migrationBuilder.Sql(
                """
                DROP EXTENSION IF EXISTS pg_trgm;
                """);
        }
    }
}
