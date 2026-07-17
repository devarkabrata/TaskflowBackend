using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class CommentFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicIds",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "ImagePublicIds",
                table: "comments",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.AddColumn<string[]>(
                name: "ImageUrls",
                table: "comments",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");
        }
    }
}
