using System.Text;

using __PROJECT__.Application.DTOs.Dynamic;

namespace __PROJECT__.Infrastructure.Dynamic;

public class DynamicParamToken
{
    public string Tag { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsNamed => !string.IsNullOrWhiteSpace(Name);
}

public static class FieldFilterParser
{
    public static List<DynamicFieldDto> Parse(string? fieldFilter)
    {
        var fields = new List<DynamicFieldDto>();
        if (string.IsNullOrWhiteSpace(fieldFilter))
        {
            return fields;
        }

        foreach (var raw in SplitTokens(fieldFilter))
        {
            var token = raw.Trim();
            if (token.Length == 0)
            {
                continue;
            }

            if (token.StartsWith('[') && token.EndsWith(']'))
            {
                token = token[1..^1];
            }

            var pipe = token.IndexOf('|');
            if (pipe <= 0)
            {
                continue;
            }

            var id = token[..pipe].Trim();
            var rest = token[(pipe + 1)..];
            var colon = rest.IndexOf(':');
            var label = colon < 0 ? rest.Trim() : rest[..colon].Trim();
            var typeAndExtra = colon < 0 ? "v" : rest[(colon + 1)..];
            var typeColon = typeAndExtra.IndexOf(':');
            var type = (typeColon < 0 ? typeAndExtra : typeAndExtra[..typeColon]).Trim().ToLowerInvariant();
            var extra = typeColon < 0 ? null : Unescape(typeAndExtra[(typeColon + 1)..].Trim());

            if (id.Length == 0)
            {
                continue;
            }

            fields.Add(new DynamicFieldDto
            {
                Id = id,
                Label = label.Length == 0 ? id : label,
                Type = type.Length == 0 ? "v" : type,
                Extra = extra,
            });
        }

        return fields;
    }

    public static List<DynamicParamToken> ParseParams(string? queryParam)
    {
        var tokens = new List<DynamicParamToken>();
        if (string.IsNullOrWhiteSpace(queryParam))
        {
            return tokens;
        }

        foreach (var raw in SplitTokens(queryParam))
        {
            var token = raw.Trim().Trim('[', ']').Trim();
            if (token.Length == 0)
            {
                continue;
            }

            var equals = token.IndexOf('=');
            tokens.Add(equals < 0
                ? new DynamicParamToken { Tag = token }
                : new DynamicParamToken
                {
                    Tag = token[..equals].Trim(),
                    Name = token[(equals + 1)..].Trim(),
                });
        }

        return tokens;
    }

    public static List<string> SplitTokens(string value)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuote = false;

        for (var index = 0; index < value.Length; index++)
        {
            var ch = value[index];
            if (ch == '\'')
            {
                if (inQuote && index + 1 < value.Length && value[index + 1] == '\'')
                {
                    current.Append("''");
                    index++;
                    continue;
                }

                inQuote = !inQuote;
                current.Append(ch);
                continue;
            }

            if (!inQuote && ch == ';')
            {
                AddToken(result, current);
                continue;
            }

            if (!inQuote && ch == ']' && index + 1 < value.Length && value[index + 1] == '[')
            {
                var lookahead = value[(index + 2)..];
                var pipe = lookahead.IndexOf('|');
                var close = lookahead.IndexOf(']');
                if (pipe > 0 && (close < 0 || pipe < close))
                {
                    AddToken(result, current);
                    index++;
                    continue;
                }
            }

            current.Append(ch);
        }

        AddToken(result, current);
        return result;
    }

    public static string Unescape(string value)
        => value.Replace("\\]", "]", StringComparison.Ordinal).Replace("\\\\", "\\", StringComparison.Ordinal);

    private static void AddToken(List<string> result, StringBuilder current)
    {
        var token = current.ToString().Trim();
        if (token.Length > 0)
        {
            result.Add(token);
        }

        current.Clear();
    }
}
