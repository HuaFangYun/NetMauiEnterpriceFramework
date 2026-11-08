using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace __PROJECT__.Infrastructure.Migrations.Audit
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_audit_trail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    entity = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    table_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_audit_trail", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_audit_trail_detail",
                columns: table => new
                {
                    tb_audit_trail_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    column_name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    value_before = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_audit_trail_detail", x => new { x.tb_audit_trail_id, x.column_name });
                });

            migrationBuilder.CreateTable(
                name: "tb_log",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    entity = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    log_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    log_msg = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_log", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_audit_trail");

            migrationBuilder.DropTable(
                name: "tb_audit_trail_detail");

            migrationBuilder.DropTable(
                name: "tb_log");
        }
    }
}
