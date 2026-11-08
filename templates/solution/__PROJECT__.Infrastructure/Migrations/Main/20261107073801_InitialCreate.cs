using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace __PROJECT__.Infrastructure.Migrations.Main
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "enum_approval_type",
                columns: table => new
                {
                    approval_type_code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    approval_type_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_approval_type", x => x.approval_type_code);
                });

            migrationBuilder.CreateTable(
                name: "enum_gender",
                columns: table => new
                {
                    gender_code = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    gender_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    gender_T24_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gender_dukcapil_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_gender", x => x.gender_code);
                });

            migrationBuilder.CreateTable(
                name: "enum_menu_icon",
                columns: table => new
                {
                    menu_icon_code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    menu_icon_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    menu_icon_name_alternate = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_menu_icon", x => x.menu_icon_code);
                });

            migrationBuilder.CreateTable(
                name: "enum_menu_parent_level",
                columns: table => new
                {
                    parent_level_code = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    parent_level_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_menu_parent_level", x => x.parent_level_code);
                });

            migrationBuilder.CreateTable(
                name: "enum_status_approval",
                columns: table => new
                {
                    status_code = table.Column<int>(type: "int", nullable: false),
                    status_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_status_approval", x => x.status_code);
                });

            migrationBuilder.CreateTable(
                name: "enum_track_list",
                columns: table => new
                {
                    tr_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    tr_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    level_seq = table.Column<int>(type: "int", nullable: true),
                    tr_short_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    tr_filter_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enum_track_list", x => x.tr_code);
                });

            migrationBuilder.CreateTable(
                name: "proc_seq",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    proc_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    proc_seq = table.Column<long>(type: "bigint", nullable: true),
                    proc_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proc_seq", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rf_default_value",
                columns: table => new
                {
                    def_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    def_name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    def_value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rf_default_value", x => x.def_code);
                });

            migrationBuilder.CreateTable(
                name: "rf_files_upload_type",
                columns: table => new
                {
                    files_upload_type_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    files_upload_type_T24_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    files_upload_type_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rf_files_upload_type", x => x.files_upload_type_code);
                });

            migrationBuilder.CreateTable(
                name: "rf_group_access_report_set",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    group_id = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    report_set_id = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rf_group_access_report_set", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rf_message",
                columns: table => new
                {
                    message_code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    message_name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rf_message", x => x.message_code);
                });

            migrationBuilder.CreateTable(
                name: "tb_notification_type",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    short_text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    long_text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_send_mail = table.Column<bool>(type: "bit", nullable: true),
                    is_send_web = table.Column<bool>(type: "bit", nullable: true),
                    href = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_notification_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_parameter_system",
                columns: table => new
                {
                    table_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    param_maker_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    param_approval_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    is_param_maker = table.Column<bool>(type: "bit", nullable: true),
                    is_param_approval = table.Column<bool>(type: "bit", nullable: true),
                    is_param_upload = table.Column<bool>(type: "bit", nullable: true),
                    icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_parameter_system", x => x.table_name);
                });

            migrationBuilder.CreateTable(
                name: "tb_parameter_sytem_temporary",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    table_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_parameter_sytem_temporary", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_report_set",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_list = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    filter_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    detail_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    detail_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_list_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_report_set", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_screen_menu_child",
                columns: table => new
                {
                    screen_menu_child_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url_action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url_action_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modal_field = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modal_detail_field_query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modal_detail_field_query_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modal_save_process_query = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    modal_save_process_query_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_tb_screen_menu_child", x => x.screen_menu_child_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_screen_menu_import_excel_process",
                columns: table => new
                {
                    screen_menu_import_excel_process_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    table_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    field_input = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    with_approval = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_tb_screen_menu_import_excel_process", x => x.screen_menu_import_excel_process_id);
                });

            migrationBuilder.CreateTable(
                name: "temp_group_app_pending",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    group_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    group_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    approval_type_id = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temp_group_app_pending", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "temp_reset_password",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    new_password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    new_password_non_encrypt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temp_reset_password", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "temp_user_app_pending",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    kode_cabang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gender_id = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    nomor_induk_karyawan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temp_user_app_pending", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "um_group_app",
                columns: table => new
                {
                    group_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    group_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_group_app", x => x.group_id);
                });

            migrationBuilder.CreateTable(
                name: "um_menu",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    menu_unique = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    menu_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    menu_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    parent_level_id = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    menu_unique_parent = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    menu_icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    order_seq = table.Column<int>(type: "int", nullable: true),
                    component_type = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_menu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "um_user_app",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    kode_cabang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gender_id = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    nomor_induk_karyawan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    last_password_change_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_user_app", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "um_user_app_session",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    session_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    connection_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    last_activity = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_user_app_session", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "um_user_audit_trail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_user_audit_trail", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "um_user_audit_trail_detail",
                columns: table => new
                {
                    um_user_audit_trail_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    column_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    value_before = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_user_audit_trail_detail", x => new { x.um_user_audit_trail_id, x.column_name });
                });

            migrationBuilder.CreateTable(
                name: "um_user_group",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    group_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    can_add_edit = table.Column<bool>(type: "bit", nullable: false),
                    can_delete = table.Column<bool>(type: "bit", nullable: false),
                    can_read_only = table.Column<bool>(type: "bit", nullable: false),
                    responsible_to_user_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_user_group", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_notification",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    notification_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    receiver_user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    reff_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_seen = table.Column<bool>(type: "bit", nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_notification_tb_notification_type_notification_type_id",
                        column: x => x.notification_type_id,
                        principalTable: "tb_notification_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_parameter_system_detail",
                columns: table => new
                {
                    table_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    field_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    order_seq = table.Column<int>(type: "int", nullable: false),
                    field_desc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    field_key = table.Column<bool>(type: "bit", nullable: true),
                    field_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    field_reff = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    is_mandatory = table.Column<bool>(type: "bit", nullable: true),
                    is_group_by = table.Column<bool>(type: "bit", nullable: true),
                    is_orderable = table.Column<bool>(type: "bit", nullable: true),
                    add_class = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_parameter_system_detail", x => new { x.table_name, x.field_name, x.order_seq });
                    table.ForeignKey(
                        name: "FK_tb_parameter_system_detail_tb_parameter_system_table_name",
                        column: x => x.table_name,
                        principalTable: "tb_parameter_system",
                        principalColumn: "table_name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_parameter_sytem_temporary_detail",
                columns: table => new
                {
                    tb_parameter_sytem_temporary_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    column_name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    value_before = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_parameter_sytem_temporary_detail", x => new { x.tb_parameter_sytem_temporary_id, x.column_name });
                    table.ForeignKey(
                        name: "FK_tb_parameter_sytem_temporary_detail_tb_parameter_sytem_temporary_tb_parameter_sytem_temporary_id",
                        column: x => x.tb_parameter_sytem_temporary_id,
                        principalTable: "tb_parameter_sytem_temporary",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_screen_menu",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    has_button_load = table.Column<bool>(type: "bit", nullable: true),
                    has_button_process = table.Column<bool>(type: "bit", nullable: true),
                    has_button_generate_txt = table.Column<bool>(type: "bit", nullable: true),
                    has_button_export_excel = table.Column<bool>(type: "bit", nullable: true),
                    has_button_add = table.Column<bool>(type: "bit", nullable: true),
                    has_button_delete = table.Column<bool>(type: "bit", nullable: true),
                    has_action_row = table.Column<bool>(type: "bit", nullable: true),
                    has_button_import_excel = table.Column<bool>(type: "bit", nullable: true),
                    field_filter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_load = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_load_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_process = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_process_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_export_txt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_export_txt_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    screen_menu_import_excel_process_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    screen_menu_child_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    query_delete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    query_delete_param = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_screen_menu", x => x.id);
                    table.ForeignKey(
                        name: "FK_tb_screen_menu_tb_screen_menu_child_screen_menu_child_id",
                        column: x => x.screen_menu_child_id,
                        principalTable: "tb_screen_menu_child",
                        principalColumn: "screen_menu_child_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_screen_menu_tb_screen_menu_import_excel_process_screen_menu_import_excel_process_id",
                        column: x => x.screen_menu_import_excel_process_id,
                        principalTable: "tb_screen_menu_import_excel_process",
                        principalColumn: "screen_menu_import_excel_process_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "um_group_menu",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    group_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    menu_id = table.Column<long>(type: "bigint", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_group_menu", x => x.id);
                    table.ForeignKey(
                        name: "FK_um_group_menu_um_menu_menu_id",
                        column: x => x.menu_id,
                        principalTable: "um_menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "um_user_app_flag",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_first_login = table.Column<bool>(type: "bit", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_login = table.Column<bool>(type: "bit", nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    last_login_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_password_changed_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_locked_out_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    failed_password_attempt_count = table.Column<int>(type: "int", nullable: true),
                    failed_password_attempt_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    session_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_user_app_flag", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_um_user_app_flag_um_user_app_user_id",
                        column: x => x.user_id,
                        principalTable: "um_user_app",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "um_user_refresh_token",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    refresh_token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    expiry_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_revoked = table.Column<bool>(type: "bit", nullable: false),
                    device_info = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    last_used_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    revoked_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    revoked_reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    modified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    deleted_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status_approval = table.Column<int>(type: "int", nullable: true),
                    is_locked = table.Column<bool>(type: "bit", nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: true),
                    approved_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    approved_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    approval_type_id = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_um_user_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_um_user_refresh_token_um_user_app_user_id",
                        column: x => x.user_id,
                        principalTable: "um_user_app",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_notification_notification_type_id",
                table: "tb_notification",
                column: "notification_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_screen_menu_screen_menu_child_id",
                table: "tb_screen_menu",
                column: "screen_menu_child_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_screen_menu_screen_menu_import_excel_process_id",
                table: "tb_screen_menu",
                column: "screen_menu_import_excel_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_um_group_menu_menu_id",
                table: "um_group_menu",
                column: "menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_um_menu_menu_unique",
                table: "um_menu",
                column: "menu_unique",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_um_user_refresh_token_user_id",
                table: "um_user_refresh_token",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "enum_approval_type");

            migrationBuilder.DropTable(
                name: "enum_gender");

            migrationBuilder.DropTable(
                name: "enum_menu_icon");

            migrationBuilder.DropTable(
                name: "enum_menu_parent_level");

            migrationBuilder.DropTable(
                name: "enum_status_approval");

            migrationBuilder.DropTable(
                name: "enum_track_list");

            migrationBuilder.DropTable(
                name: "proc_seq");

            migrationBuilder.DropTable(
                name: "rf_default_value");

            migrationBuilder.DropTable(
                name: "rf_files_upload_type");

            migrationBuilder.DropTable(
                name: "rf_group_access_report_set");

            migrationBuilder.DropTable(
                name: "rf_message");

            migrationBuilder.DropTable(
                name: "tb_notification");

            migrationBuilder.DropTable(
                name: "tb_parameter_system_detail");

            migrationBuilder.DropTable(
                name: "tb_parameter_sytem_temporary_detail");

            migrationBuilder.DropTable(
                name: "tb_report_set");

            migrationBuilder.DropTable(
                name: "tb_screen_menu");

            migrationBuilder.DropTable(
                name: "temp_group_app_pending");

            migrationBuilder.DropTable(
                name: "temp_reset_password");

            migrationBuilder.DropTable(
                name: "temp_user_app_pending");

            migrationBuilder.DropTable(
                name: "um_group_app");

            migrationBuilder.DropTable(
                name: "um_group_menu");

            migrationBuilder.DropTable(
                name: "um_user_app_flag");

            migrationBuilder.DropTable(
                name: "um_user_app_session");

            migrationBuilder.DropTable(
                name: "um_user_audit_trail");

            migrationBuilder.DropTable(
                name: "um_user_audit_trail_detail");

            migrationBuilder.DropTable(
                name: "um_user_group");

            migrationBuilder.DropTable(
                name: "um_user_refresh_token");

            migrationBuilder.DropTable(
                name: "tb_notification_type");

            migrationBuilder.DropTable(
                name: "tb_parameter_system");

            migrationBuilder.DropTable(
                name: "tb_parameter_sytem_temporary");

            migrationBuilder.DropTable(
                name: "tb_screen_menu_child");

            migrationBuilder.DropTable(
                name: "tb_screen_menu_import_excel_process");

            migrationBuilder.DropTable(
                name: "um_menu");

            migrationBuilder.DropTable(
                name: "um_user_app");
        }
    }
}
