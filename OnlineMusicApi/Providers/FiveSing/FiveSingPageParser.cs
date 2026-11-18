using System.Net;
using System.Text.RegularExpressions;

namespace OnlineMusicApi.Internal.Providers.FiveSing;

internal static class FiveSingPageParser
{
    public static Artist ParseArtist(string html, ArtistId id)
    {
        return new Artist
        {
            Id = id,
            Name = Decode(Extract(html, @"OwnerNickName\s*=\s*'([^']*)'")),
            Description = Decode(
                Extract(html, @"<meta\s+name=""description""\s+content=""([^""]*)""")
            ),
            AvatarUrl = Extract(
                html,
                @"<img[^>]+src=""(https://wsingbssdl\.kugou\.com/[^""]+188x188[^""]*)"""
            ),
        };
    }

    private static string Extract(string html, string pattern)
    {
        Match match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string Decode(string value)
    {
        return WebUtility.HtmlDecode(value ?? string.Empty);
    }
}
