# Database

Folder ini berisi script database untuk `__PROJECT__`.

## Isi

```
00_create_databases.sql
03_synonyms.sql
stored-procedures/
```

Schema tabel dan seed utama dibuat oleh **EF Core migration + DbInitializer** saat API pertama kali dijalankan. Script di folder ini melengkapi database:

- `00_create_databases.sql` — membuat database utama dan `log_audit`.
- `03_synonyms.sql` — membuat synonym `tb_audit_trail`, `tb_audit_trail_detail`, `tb_log` di database utama.
- `stored-procedures/` — 19 proc framework (paging, approval, staging, profil, notifikasi).

## Database

- `__DB_NAME__` — database utama.
- `__AUDIT_DB_NAME__` — `tb_audit_trail`, `tb_audit_trail_detail`, `tb_log`.

`tb_log` dan `tb_audit_trail` di database utama adalah **synonym** ke `__AUDIT_DB_NAME__`, bukan tabel duplikat.

## Deploy manual

```powershell
sqlcmd -S localhost,1433 -U sa -P "..." -d master -i database/00_create_databases.sql
sqlcmd -S localhost,1433 -U sa -P "..." -d __DB_NAME__ -i database/03_synonyms.sql

Get-ChildItem database/stored-procedures/*.sql | Sort-Object Name | ForEach-Object {
    sqlcmd -S localhost,1433 -U sa -P "..." -d __DB_NAME__ -i $_.FullName
}
```

Gunakan `CREATE OR ALTER`; semua script aman dijalankan ulang.
