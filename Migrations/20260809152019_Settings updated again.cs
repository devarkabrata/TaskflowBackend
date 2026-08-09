using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class Settingsupdatedagain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTaskCreationNotificationEnabled",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTeamMemberNotificationEnabled",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWorkspaceMemberNotificationEnabled",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationOnTaskAssignment",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTaskCreationNotificationEnabled",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "IsTeamMemberNotificationEnabled",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "IsWorkspaceMemberNotificationEnabled",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "NotificationOnTaskAssignment",
                table: "settings");
        }
    }
}
