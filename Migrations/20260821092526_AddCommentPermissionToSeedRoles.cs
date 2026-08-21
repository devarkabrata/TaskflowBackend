using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentPermissionToSeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The Comment permission type was added after the initial role seed, so none
            // of the placeholder roles had it. Every seeded role that can Write should
            // also be able to comment on tasks.
            migrationBuilder.Sql(@"
                UPDATE ""roles""
                SET ""Permissions"" = array_append(""Permissions"", 'Comment')
                WHERE ""Id"" IN (
                    '11111111-1111-1111-1111-111111111111',
                    '22222222-2222-2222-2222-222222222222',
                    '33333333-3333-3333-3333-333333333333',
                    '44444444-4444-4444-4444-444444444444'
                )
                AND NOT ('Comment' = ANY(""Permissions""));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""roles""
                SET ""Permissions"" = array_remove(""Permissions"", 'Comment')
                WHERE ""Id"" IN (
                    '11111111-1111-1111-1111-111111111111',
                    '22222222-2222-2222-2222-222222222222',
                    '33333333-3333-3333-3333-333333333333',
                    '44444444-4444-4444-4444-444444444444'
                );
            ");
        }
    }
}
