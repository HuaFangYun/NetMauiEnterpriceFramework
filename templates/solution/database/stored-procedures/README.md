# Stored Procedures

19 proc framework:

- `usp_v2_get_user_list`
- `usp_v2_get_list_user_flag`
- `usp_v2_get_group_list`
- `usp_v2_get_user_group_list`
- `usp_v2_get_menu_list`
- `usp_v2_get_waiting_approval_users`
- `usp_v2_get_waiting_approval_groups`
- `usp_v2_get_waiting_approval_user_groups`
- `usp_v2_get_waiting_approval_menus`
- `usp_v2_get_parameter_pending_approval`
- `usp_v2_get_recent_notification`
- `usp_v2_get_list_notification`
- `usp_v2_my_profile`
- `usp_v2_um_menu_approval`
- `usp_v2_um_user_group_approval`
- `usp_parameter_sytem_save`
- `usp_parameter_sytem_detail_save`
- `usp_parameter_sytem_approval`
- `usp_screen_menu_approval`

Semua proc list memakai signature paging standar:

```sql
@PageNumber, @PageSize, @SearchTerm, @SortColumn, @SortDirection, @TotalCount OUTPUT
```

## Catatan

- Gunakan `CREATE OR ALTER`; script idempotent.
- Search memakai parameter (`Ma'ruf` aman).
- Sort column di-whitelist.
- `usp_parameter_sytem_approval` adalah implementasi SQL; aplikasi saat ini memakai C# `ParameterEngineService` sebagai jalur utama.
- `usp_screen_menu_approval` saat ini single-table; payload child/import multi-table ditangani C#/proc khusus pada fase dynamic screen.
- Audit approval menulis ke `[__AUDIT_DB_NAME__].[dbo].[tb_log]`.

## Deploy

Jalankan setelah schema + seed:

```powershell
Get-ChildItem *.sql | Sort-Object Name | ForEach-Object {
    sqlcmd -S localhost,1433 -U sa -P "..." -d __DB_NAME__ -i $_.FullName
}
```
