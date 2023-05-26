using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImoutoRebirth.Lilin.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddBasicTagTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('7e22156e-9d88-4109-a8dd-48df4349fa6c', '2019-12-31 00:16:45.456207 +00:00', '2019-12-31 00:16:45.456247 +00:00', 'LocalMeta', 16741916) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('9990adee-10e8-470f-8327-dfd10f1a2c19', '2019-12-31 00:16:45.601982 +00:00', '2019-12-31 00:16:45.601984 +00:00', 'General', 16741916) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('78ddf7e0-9d81-4991-8ebd-e74686ae0fff', '2020-02-02 19:05:24.200799 +00:00', '2020-02-02 19:05:24.200800 +00:00', 'Genre', 16741916) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('d62ee8c8-ff14-44ea-ad17-373afc552681', '2020-02-02 19:03:47.459751 +00:00', '2020-02-02 19:03:47.459760 +00:00', 'Faults', 0) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('92a8359b-cd6b-4658-9e1e-a64254cad248', '2020-02-02 19:03:51.780347 +00:00', '2020-02-02 19:03:51.780348 +00:00', 'Studio', 16723405) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('9bef004d-b6e6-4c3e-a83b-fbaaeadb67b3', '2020-02-02 19:05:26.424309 +00:00', '2020-02-02 19:05:26.424310 +00:00', 'Meta', 6029567) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('0eda8338-109c-4a50-a484-240f4ce0160d', '2019-12-31 00:16:45.597369 +00:00', '2019-12-31 00:16:45.597372 +00:00', 'Copyright', 11141290) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('60f7d87b-16d4-435f-991a-d22a12eb8886', '2019-12-31 00:26:59.876563 +00:00', '2019-12-31 00:26:59.876572 +00:00', 'Character', 43520) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('59832e0f-2539-43af-ac56-e7f330406ff4', '2019-12-31 00:16:45.593021 +00:00', '2019-12-31 00:16:45.593028 +00:00', 'Artist', 11141120) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('ef1e616a-8341-4d0b-ab54-1a9f6dd31aed', '2020-02-02 19:02:10.919269 +00:00', '2020-02-02 19:02:10.919276 +00:00', 'Circle', 0) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('2f31591b-ce56-4287-b028-03da5b7595cf', '2019-12-31 00:16:49.955996 +00:00', '2019-12-31 00:16:49.955996 +00:00', 'Medium', 2459067) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."TagTypes" ("Id", "AddedOn", "ModifiedOn", "Name", "Color") VALUES ('926cfa74-0656-40b7-bfb2-7f3ee0867c8e', '2020-09-13 19:23:31.378210 +00:00', '2020-09-13 19:23:31.378218 +00:00', 'Location', 16741916) ON CONFLICT ("Id") DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO public."Tags" ("Id", "AddedOn", "ModifiedOn", "TypeId", "Name", "HasValue", "Synonyms", "Count") VALUES ('5ab6f880-3e98-4901-82f7-68bcfd31de31', '2020-02-29 21:59:02.040070 +00:00', '2020-02-29 21:59:02.040075 +00:00', '7e22156e-9d88-4109-a8dd-48df4349fa6c', 'Favorite', false, null, 0) ON CONFLICT ("Id") DO NOTHING;
                INSERT INTO public."Tags" ("Id", "AddedOn", "ModifiedOn", "TypeId", "Name", "HasValue", "Synonyms", "Count") VALUES ('ba2e7898-bef6-452a-9eb4-c936a8653b37', '2020-02-29 21:51:36.802530 +00:00', '2020-02-29 21:51:36.802538 +00:00', '7e22156e-9d88-4109-a8dd-48df4349fa6c', 'Rate', true, null, 0) ON CONFLICT ("Id") DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
