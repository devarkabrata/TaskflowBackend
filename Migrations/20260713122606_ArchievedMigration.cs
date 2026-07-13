using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class ArchievedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "tasks",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchievable",
                table: "board_statuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "IsArchievable",
                table: "board_statuses");
        }
    }
}
