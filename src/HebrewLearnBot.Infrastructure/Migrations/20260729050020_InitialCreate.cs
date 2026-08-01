using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HebrewLearnBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversation_states",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    conversation_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    step = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state_json = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_states", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "topics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name_normalized = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    selected_mode = table.Column<int>(type: "integer", nullable: false),
                    selected_topic_id = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_settings_topics_selected_topic_id",
                        column: x => x.selected_topic_id,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "words",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hebrew = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hebrew_normalized = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hebrew_vocalized = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    transliteration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    translation_ru = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    translation_normalized = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    topic_id = table.Column<int>(type: "integer", nullable: false),
                    part_of_speech = table.Column<int>(type: "integer", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    external_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_words", x => x.id);
                    table.ForeignKey(
                        name: "FK_words_topics_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "word_progress",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    word_id = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_word_progress", x => new { x.user_id, x.word_id });
                    table.ForeignKey(
                        name: "FK_word_progress_words_word_id",
                        column: x => x.word_id,
                        principalTable: "words",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_topics_name_normalized",
                table: "topics",
                column: "name_normalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_settings_selected_topic_id",
                table: "user_settings",
                column: "selected_topic_id");

            migrationBuilder.CreateIndex(
                name: "ix_word_progress_user_id_last_shown_at_utc",
                table: "word_progress",
                columns: new[] { "user_id", "last_shown_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_word_progress_word_id",
                table: "word_progress",
                column: "word_id");

            migrationBuilder.CreateIndex(
                name: "ix_words_topic_id_is_active",
                table: "words",
                columns: new[] { "topic_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ux_words_external_key",
                table: "words",
                column: "external_key",
                unique: true,
                filter: "external_key IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_words_hebrew_translation",
                table: "words",
                columns: new[] { "hebrew_normalized", "translation_normalized" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conversation_states");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "word_progress");

            migrationBuilder.DropTable(
                name: "words");

            migrationBuilder.DropTable(
                name: "topics");
        }
    }
}
