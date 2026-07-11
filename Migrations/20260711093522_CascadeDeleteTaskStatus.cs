using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteTaskStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_board_statuses_StatusId",
                table: "tasks");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletable",
                table: "board_statuses",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_board_statuses_StatusId",
                table: "tasks",
                column: "StatusId",
                principalTable: "board_statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_board_statuses_StatusId",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "IsDeletable",
                table: "board_statuses");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_board_statuses_StatusId",
                table: "tasks",
                column: "StatusId",
                principalTable: "board_statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
