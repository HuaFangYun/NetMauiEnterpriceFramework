using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace __PROJECT__.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class AddApprovalPayload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "group_id",
                table: "temp_user_app_pending",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "temp_user_app_pending",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "temp_group_app_pending",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "group_id",
                table: "temp_user_app_pending");

            migrationBuilder.DropColumn(
                name: "password",
                table: "temp_user_app_pending");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "temp_group_app_pending");
        }
    }
}
