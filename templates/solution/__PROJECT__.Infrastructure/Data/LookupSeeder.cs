using System.Reflection;
using System.Text;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using __PROJECT__.Domain.Entities;
using __PROJECT__.Domain.Enums;
using __PROJECT__.Infrastructure.Data;

namespace __PROJECT__.Infrastructure.Data;

public static class LookupSeeder
{
    public static async Task SeedAsync(__PROJECT__DbContext context, CancellationToken cancellationToken = default)
    {
        await SeedMenuIconsAsync(context, cancellationToken);
        await SeedStatusApprovalsAsync(context, cancellationToken);
        await SeedApprovalTypesAsync(context, cancellationToken);
        await SeedGendersAsync(context, cancellationToken);
        await SeedTrackListAsync(context, cancellationToken);
        await SeedFileUploadTypesAsync(context, cancellationToken);
    }

    private static async Task SeedMenuIconsAsync(__PROJECT__DbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.enum_menu_icon.Select(e => e.menu_icon_code).ToHashSetAsync(cancellationToken);
        var source = ReadSeed("enum_menu_icon");
        foreach (var item in source.RootElement.EnumerateArray())
        {
            var code = GetString(item, "menu_icon_code");
            if (string.IsNullOrWhiteSpace(code) || existing.Contains(code)) continue;
            await context.enum_menu_icon.AddAsync(new enum_menu_icon
            {
                menu_icon_code = code,
                menu_icon_name = GetString(item, "menu_icon_name"),
                menu_icon_name_alternate = GetString(item, "menu_icon_name_alternate"),
            }, cancellationToken);
            existing.Add(code);
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedStatusApprovalsAsync(__PROJECT__DbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.enum_status_approval.Select(e => e.status_code).ToHashSetAsync(cancellationToken);
        var source = ReadSeed("enum_status_approval");
        foreach (var item in source.RootElement.EnumerateArray())
        {
            var code = GetInt(item, "status_code");
            if (code == null || existing.Contains(code.Value)) continue;
            await context.enum_status_approval.AddAsync(new enum_status_approval
            {
                status_code = code.Value,
                status_name = GetString(item, "status_name"),
            }, cancellationToken);
            existing.Add(code.Value);
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedApprovalTypesAsync(__PROJECT__DbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.enum_approval_type.Select(e => e.approval_type_code).ToHashSetAsync(cancellationToken);
        var source = ReadSeed("enum_approval_type");
        foreach (var item in source.RootElement.EnumerateArray())
        {
            var code = GetString(item, "approval_type_code");
            if (string.IsNullOrWhiteSpace(code) || existing.Contains(code)) continue;
            await context.enum_approval_type.AddAsync(new enum_approval_type
            {
                approval_type_code = code,
                approval_type_name = GetString(item, "approval_type_name"),
                is_active = GetBool(item, "is_active") ?? true,
            }, cancellationToken);
            existing.Add(code);
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedGendersAsync(__PROJECT__DbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.enum_gender.Select(e => e.gender_code).ToHashSetAsync(cancellationToken);
        var source = ReadSeed("enum_gender");
        foreach (var item in source.RootElement.EnumerateArray())
        {
            var code = GetString(item, "gender_code");
            if (string.IsNullOrWhiteSpace(code) || existing.Contains(code)) continue;
            await context.enum_gender.AddAsync(new enum_gender
            {
                gender_code = code,
                gender_name = GetString(item, "gender_name"),
                gender_T24_code = GetString(item, "gender_T24_code"),
                gender_dukcapil_code = GetString(item, "gender_dukcapil_code"),
            }, cancellationToken);
            existing.Add(code);
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTrackListAsync(__PROJECT__DbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.enum_track_list.Select(e => e.tr_code).ToHashSetAsync(cancellationToken);
        var source = ReadSeed("enum_track_list");
        foreach (var item in source.RootElement.EnumerateArray())
        {
            var code = GetString(item, "tr_code");
            if (string.IsNullOrWhiteSpace(code) || existing.Contains(code)) continue;
            await context.enum_track_list.AddAsync(new enum_track_list
            {
                tr_code = code,
                tr_name = GetString(item, "tr_name"),
                is_active = GetBool(item, "is_active") ?? true,
                level_seq = GetInt(item, "level_seq"),
                tr_short_name = GetString(item, "tr_short_name"),
                tr_filter_name = GetString(item, "tr_filter_name"),
            }, cancellationToken);
            existing.Add(code);
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedFileUploadTypesAsync(__PROJECT__DbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.rf_files_upload_type.Select(e => e.files_upload_type_code).ToHashSetAsync(cancellationToken);
        var source = ReadSeed("rf_files_upload_type");
        var now = DateTime.Now;
        foreach (var item in source.RootElement.EnumerateArray())
        {
            var code = GetString(item, "files_upload_type_code");
            if (string.IsNullOrWhiteSpace(code) || existing.Contains(code)) continue;
            await context.rf_files_upload_type.AddAsync(new rf_files_upload_type
            {
                files_upload_type_code = code,
                files_upload_type_T24_code = GetString(item, "files_upload_type_T24_code"),
                files_upload_type_name = GetString(item, "files_upload_type_name"),
                statusApproval = Const.StatusApproval.APPROVED,
                isActive = true,
                isDeleted = false,
                createdBy = "system",
                createdDate = now,
            }, cancellationToken);
            existing.Add(code);
        }
        await context.SaveChangesAsync(cancellationToken);
    }

    private static JsonDocument ReadSeed(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames()
            .FirstOrDefault(e => e.EndsWith($".Seeds.{name}.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Seed resource not found: {name}");

        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Seed stream not found: {name}");
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var json = reader.ReadToEnd().TrimStart('\uFEFF');
        return JsonDocument.Parse(json);
    }

    private static string? GetString(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null) return null;
        return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
    }

    private static int? GetInt(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)) return number;
        return int.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    private static bool? GetBool(JsonElement item, string name)
    {
        if (!item.TryGetProperty(name, out var value)) return null;
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => bool.TryParse(value.ToString(), out var parsed) ? parsed : null,
        };
    }
}
