using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations.ArchiveDB
{
    /// <inheritdoc />
    public partial class AddStatusToArchivedTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "archived_tasks",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "archived_tasks");
        }
    }
}
