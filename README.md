# NetMauiEnterpriceFramework

Generator framework enterprise **.NET 10** untuk menghasilkan aplikasi siap pakai dengan:

- ASP.NET Core Web API
- Blazor WebAssembly
- .NET MAUI Blazor Hybrid (Windows/Android/iOS/MacCatalyst)
- MudBlazor UI
- SQL Server + EF Core
- JWT access/refresh token + session validation
- Dynamic menu, screen, report, dan parameter engine
- Maker-checker approval (Mode A staging)
- SignalR notification + Hangfire background job
- Import Excel dan export Excel/PDF
- Stored procedure + synonym deployment

Nama paket npm: `create-enterprise-agentic-framework`  
Repository: [rayzaisra/NetMauiEnterpriceFramework](https://github.com/rayzaisra/NetMauiEnterpriceFramework)

## Prasyarat

- Node.js 20+
- .NET 10 SDK
- SQL Server dengan TCP 1433 aktif
- .NET MAUI workload bila tidak memakai `--no-maui`
- Docker Desktop bila memakai `--docker`

Panduan lengkap: **[INSTALL.md](INSTALL.md)**

## Instalasi

Dari GitHub:

```bash
npm install -g github:rayzaisra/NetMauiEnterpriceFramework
```

Atau setelah dipublikasikan ke npm:

```bash
npm install -g create-enterprise-agentic-framework
```

Atau tanpa instalasi global:

```bash
npx github:rayzaisra/NetMauiEnterpriceFramework
```

## Membuat aplikasi

Mode interaktif:

```bash
create-enterprise-agentic-framework
```

Mode config:

```bash
create-enterprise-agentic-framework --config scaffold.example.json
```

Contoh non-interaktif:

```bash
create-enterprise-agentic-framework \
  --name MitraPensiun \
  --db mitra_pensiun_db \
  --audit-db mitra_pensiun_log_audit \
  --server localhost,1433 \
  --user sa \
  --password "YourStrong!Passw0rd" \
  --no-maui \
  --tests \
  --docker
```

## Opsi generator

| Opsi | Keterangan |
|---|---|
| `--name` | Nama project/solution |
| `--db` | Nama database utama |
| `--audit-db` | Nama database audit/log |
| `--server` | SQL Server host/port |
| `--user`, `--password` | Kredensial SQL Server |
| `--admin-*` | Data admin awal |
| `--branch` | Kode cabang admin |
| `--logo` | Path logo PNG/JPG/SVG |
| `--config` | Config JSON non-interaktif |
| `--output` | Direktori output |
| `--no-maui` | Tanpa project MAUI |
| `--tests` | Tambah project xUnit dan jalankan test |
| `--docker` | Tambah Dockerfile + docker-compose |
| `--skip-build` | Lewati `dotnet build` |
| `--skip-seed` | Lewati pembuatan/seed database |
| `--force` | Timpa output yang tidak kosong |
| `--dry-run` | Validasi dan tampilkan rencana saja |
| `--yes` | Non-interaktif |

## Struktur hasil generate

| Project | Fungsi |
|---|---|
| `<Project>.Domain` | Entity dan konstanta domain |
| `<Project>.Application` | DTO, interface, service kontrak |
| `<Project>.Infrastructure` | EF Core, service implementasi, seeder |
| `<Project>.Api` | Web API, auth, SignalR, Hangfire, export |
| `<Project>.Shared` | Komponen Razor, halaman, service HTTP |
| `<Project>.Web` | Blazor WebAssembly host |
| `<Project>.App` | .NET MAUI Blazor Hybrid host |

Database yang dibuat:

- database utama aplikasi
- database audit/log terpisah
- 19 stored procedure
- 3 synonym audit

## Setelah generate

```bash
cd <ProjectName>

dotnet run --project <ProjectName>.Api
dotnet run --project <ProjectName>.Web
dotnet run --project <ProjectName>.App
```

Jika memakai `--docker`:

```bash
docker compose up --build
```

## Keamanan

- Ganti password admin awal setelah login pertama.
- Jangan commit `appsettings.Development.json` berisi kredensial nyata.
- Gunakan SQL login dengan hak minimum yang dibutuhkan.
- Approval Mode A: data utama baru ditulis setelah approve.

## Lisensi

MIT — lihat [LICENSE](LICENSE).

---

Dokumen perencanaan internal/AI tidak disertakan dalam repository ini.
