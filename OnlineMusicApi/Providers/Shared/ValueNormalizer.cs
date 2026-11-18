using System.Globalization;
using System.Text;

namespace OnlineMusicApi.Internal.Providers.Shared;

internal static class ValueNormalizer
{
    public static string First(params string?[] values)
    {
        return FirstOrNull(values) ?? string.Empty;
    }

    public static string? FirstOrNull(params string?[] values)
    {
        foreach (string? value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return null;
    }

    public static int? ToInt(string? value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)
            ? result
            : null;
    }

    public static TimeSpan? ToDuration(int? milliseconds)
    {
        return milliseconds is > 0 ? TimeSpan.FromMilliseconds(milliseconds.Value) : null;
    }

    public static string NormalizeKeyword(string value)
    {
        value = (value ?? string.Empty).Normalize(NormalizationForm.FormKC).Trim();

        if (value.Length == 0 || value.EnumerateRunes().Count() > 80 || value.Any(char.IsControl))
        {
            throw new ArgumentException("请输入 1 至 80 个字符的搜索词。", nameof(value));
        }

        return value;
    }

    public static int NormalizeOffset(int offset)
    {
        return Math.Max(offset, 0);
    }

    public static int NormalizeLimit(int limit)
    {
        return Math.Clamp(limit, 1, 50);
    }
}