using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace __PROJECT__.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class RemoveUserProfileColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kode_cabang",
                table: "um_user_app");

            migrationBuilder.DropColumn(
                name: "nomor_induk_karyawan",
                table: "um_user_app");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "kode_cabang",
                table: "um_user_app",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nomor_induk_karyawan",
                table: "um_user_app",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
