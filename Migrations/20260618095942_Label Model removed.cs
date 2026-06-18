using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskFlowBackend.Migrations
{
    /// <inheritdoc />
    public partial class LabelModelremoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "labels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "labels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_labels", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "labels",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), "feature" },
                    { new Guid("11111111-0000-0000-0000-000000000002"), "bug" },
                    { new Guid("11111111-0000-0000-0000-000000000003"), "design" },
                    { new Guid("11111111-0000-0000-0000-000000000004"), "docs" },
                    { new Guid("11111111-0000-0000-0000-000000000005"), "infra" },
                    { new Guid("11111111-0000-0000-0000-000000000006"), "refactor" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_labels_Name",
                table: "labels",
                column: "Name",
                unique: true);
        }
    }
}
