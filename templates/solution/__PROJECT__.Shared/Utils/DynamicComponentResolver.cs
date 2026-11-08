using System.Collections.Concurrent;

namespace __PROJECT__.Shared.Utils;

public class DynamicComponentResolver
{
    private readonly ConcurrentDictionary<string, Type?> _cache = new();

    public Type? ResolveComponentType(string? componentTypeName)
    {
        if (string.IsNullOrWhiteSpace(componentTypeName))
        {
            return null;
        }

        return _cache.GetOrAdd(componentTypeName, name =>
        {
            if (name.Equals("DynamicScreen", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(__PROJECT__.Shared.Pages.Dynamic.DynamicScreen);
            }

            if (name.Equals("DynamicReport", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(__PROJECT__.Shared.Pages.Dynamic.DynamicReport);
            }

            if (name.Equals("DynamicUrlView", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(__PROJECT__.Shared.Pages.Dynamic.DynamicUrlView);
            }

            try
            {
                return Type.GetType(name, throwOnError: false);
            }
            catch
            {
                return null;
            }
        });
    }
}

public static class RouteResolver
{
    public static string ToBlazorRoute(string? legacyUrl)
    {
        if (string.IsNullOrWhiteSpace(legacyUrl))
        {
            return "/";
        }

        if (legacyUrl.StartsWith('/'))
        {
            return legacyUrl;
        }

        var segments = legacyUrl.Split('/');
        return "/" + string.Join('/', segments.Select(ToKebabCase));
    }

    private static string ToKebabCase(string input)
    {
        return string.Concat(input.Select((ch, index) =>
            index > 0 && char.IsUpper(ch) && !char.IsUpper(input[index - 1])
                ? "-" + ch
                : ch.ToString()))
            .ToLowerInvariant();
    }
}
