using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ImoutoRebirth.Meido.DataAccess.Migrations;

public partial class AddLastRequestedToSourceActualizingState : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "LastRequested",
            table: "SourceActualizingStates",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastRequested",
            table: "SourceActualizingStates");
    }
}