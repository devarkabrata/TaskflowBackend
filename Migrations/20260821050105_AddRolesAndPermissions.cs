using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Roles table, seeded with placeholder roles (reusing the old TeamRole
            //    names/permission split) so team_members/invitations have something to
            //    point RoleId at. Replace these rows once the real 6-7 role set is defined.
            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsEnable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Permissions = table.Column<string[]>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO ""roles"" (""Id"", ""Name"", ""Description"", ""IsEnable"", ""Permissions"", ""CreatedAt"", ""UpdatedAt"") VALUES
                    ('11111111-1111-1111-1111-111111111111', 'Admin', 'Placeholder role — full access. Replace with the real role set.', true, ARRAY['Read','Write','Delete','Manage']::text[], NOW(), NOW()),
                    ('22222222-2222-2222-2222-222222222222', 'PM', 'Placeholder role — full access. Replace with the real role set.', true, ARRAY['Read','Write','Delete','Manage']::text[], NOW(), NOW()),
                    ('33333333-3333-3333-3333-333333333333', 'TL', 'Placeholder role — read/write/delete. Replace with the real role set.', true, ARRAY['Read','Write','Delete']::text[], NOW(), NOW()),
                    ('44444444-4444-4444-4444-444444444444', 'Developer', 'Placeholder role — read/write. Replace with the real role set.', true, ARRAY['Read','Write']::text[], NOW(), NOW());
            ");

            // 2. Add RoleId as nullable first so existing rows can be backfilled from
            //    the old Role enum column before we require it and drop that column.
            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "team_members",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoleId",
                table: "invitations",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""team_members"" SET ""RoleId"" = '11111111-1111-1111-1111-111111111111' WHERE ""Role"" = 'Admin';
                UPDATE ""team_members"" SET ""RoleId"" = '22222222-2222-2222-2222-222222222222' WHERE ""Role"" = 'PM';
                UPDATE ""team_members"" SET ""RoleId"" = '33333333-3333-3333-3333-333333333333' WHERE ""Role"" = 'TL';
                UPDATE ""team_members"" SET ""RoleId"" = '44444444-4444-4444-4444-444444444444' WHERE ""Role"" = 'Developer';
            ");

            migrationBuilder.Sql(@"
                UPDATE ""invitations"" SET ""RoleId"" = '11111111-1111-1111-1111-111111111111' WHERE ""Role"" = 'Admin';
                UPDATE ""invitations"" SET ""RoleId"" = '22222222-2222-2222-2222-222222222222' WHERE ""Role"" = 'PM';
                UPDATE ""invitations"" SET ""RoleId"" = '33333333-3333-3333-3333-333333333333' WHERE ""Role"" = 'TL';
                UPDATE ""invitations"" SET ""RoleId"" = '44444444-4444-4444-4444-444444444444' WHERE ""Role"" = 'Developer';
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                table: "team_members",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                table: "invitations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "Role",
                table: "team_members");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "invitations");

            migrationBuilder.CreateIndex(
                name: "IX_team_members_RoleId",
                table: "team_members",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_RoleId",
                table: "invitations",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_invitations_roles_RoleId",
                table: "invitations",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_team_members_roles_RoleId",
                table: "team_members",
                column: "RoleId",
                principalTable: "roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_invitations_roles_RoleId",
                table: "invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_team_members_roles_RoleId",
                table: "team_members");

            migrationBuilder.DropIndex(
                name: "IX_team_members_RoleId",
                table: "team_members");

            migrationBuilder.DropIndex(
                name: "IX_invitations_RoleId",
                table: "invitations");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "team_members",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "invitations",
                type: "text",
                nullable: true);

            // Best-effort: maps back via the current roles.Name — accurate as long as the
            // seeded/placeholder role names haven't been changed since this migration ran.
            migrationBuilder.Sql(@"
                UPDATE ""team_members"" tm SET ""Role"" = r.""Name"" FROM ""roles"" r WHERE tm.""RoleId"" = r.""Id"";
                UPDATE ""invitations"" i SET ""Role"" = r.""Name"" FROM ""roles"" r WHERE i.""RoleId"" = r.""Id"";
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "team_members",
                type: "text",
                nullable: false,
                defaultValue: "Developer",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "invitations",
                type: "text",
                nullable: false,
                defaultValue: "Developer",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "team_members");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "invitations");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
