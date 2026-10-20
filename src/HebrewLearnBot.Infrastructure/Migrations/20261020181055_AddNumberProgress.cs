using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HebrewLearnBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNumberProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "number_progress",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    correct_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    wrong_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    streak = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_shown_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_answer_correct = table.Column<bool>(type: "boolean", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_number_progress", x => new { x.user_id, x.mode, x.value });
                });

            migrationBuilder.CreateIndex(
                name: "ix_number_progress_user_id_last_shown_at_utc",
                table: "number_progress",
                columns: new[] { "user_id", "last_shown_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_number_progress_value",
                table: "number_progress",
                column: "value");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "number_progress");
        }
    }
}
