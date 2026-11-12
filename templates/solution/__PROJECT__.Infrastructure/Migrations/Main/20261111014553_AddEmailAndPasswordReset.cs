using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace __PROJECT__.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class AddEmailAndPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "remember_me",
                table: "um_user_refresh_token",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_email_outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    provider_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    recipient = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    body_html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    body_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    reference_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    reference_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    next_attempt_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_email_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_email_provider",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    provider_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    provider_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    configuration_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    from_email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    from_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    priority = table.Column<int>(type: "int", nullable: true),
                    last_tested_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_test_status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    last_test_message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_email_provider", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_email_template",
                columns: table => new
                {
                    template_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    body_html = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    body_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_email_template", x => x.template_key);
                });

            migrationBuilder.CreateTable(
                name: "tb_password_reset_token",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by_ip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_password_reset_token", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_email_outbox_status_next_attempt_at",
                table: "tb_email_outbox",
                columns: new[] { "status", "next_attempt_at" });

            migrationBuilder.CreateIndex(
                name: "IX_tb_email_provider_priority",
                table: "tb_email_provider",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "IX_tb_password_reset_token_token_hash",
                table: "tb_password_reset_token",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_password_reset_token_user_id",
                table: "tb_password_reset_token",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_email_outbox");

            migrationBuilder.DropTable(
                name: "tb_email_provider");

            migrationBuilder.DropTable(
                name: "tb_email_template");

            migrationBuilder.DropTable(
                name: "tb_password_reset_token");

            migrationBuilder.DropColumn(
                name: "remember_me",
                table: "um_user_refresh_token");
        }
    }
}
