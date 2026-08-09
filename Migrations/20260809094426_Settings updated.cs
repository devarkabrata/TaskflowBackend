using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class Settingsupdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationOnMemberAddToTeam",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationOnMemberAddToWorkspace",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationOnMemberAddToTeam",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "NotificationOnMemberAddToWorkspace",
                table: "settings");
        }
    }
}
