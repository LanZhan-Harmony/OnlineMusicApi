using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using OnlineMusicApi.Internal.Providers.FiveSing.Protocol.Extensions;

namespace OnlineMusicApi.Internal.Providers.FiveSing.Protocol;

/// <summary>
/// 5sing 音乐 API
/// </summary>
internal sealed partial class FiveSingMusicApiService : IDisposable
{
    private const int PageSize = 10;

    [GeneratedRegex("<[^>]+>", RegexOptions.Compiled)]
    private static partial Regex HtmlTagRegex();

    private readonly HttpClient _client;
    private readonly bool _ownsClient;

    public FiveSingMusicApiService()
    {
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = true,
        };
        _client = new HttpClient(handler);
        _ownsClient = true;
    }

    public FiveSingMusicApiService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = false;
    }

    public Task<JsonObject> SearchMusicAsync(string query, int page) =>
        SearchAsync(query, page, 0, 0);

    public Task<JsonObject> SearchAlbumAsync(string query, int page) =>
        SearchAsync(query, page, 1, 0);

    public Task<JsonObject> SearchArtistAsync(string query, int page) =>
        SearchAsync(query, page, 2, 1);
public async Task<JsonObject> GetLyricAsync(JsonObject musicItem)
    {
        ArgumentNullException.ThrowIfNull(musicItem);

        var id = GetString(musicItem, "id")
            ?? throw new ArgumentException("musicItem.id 不能为空", nameof(musicItem));
        var typeEname = GetString(musicItem, "typeEname")
            ?? throw new ArgumentException("musicItem.typeEname 不能为空", nameof(musicItem));

        JsonObject res = await GetJsonAsync(
            "http://5sing.kugou.com/fm/m/json/lrc",
            new QueryCollection { { "songId", id }, { "songType", typeEname } },
            null
        );

        return new JsonObject { ["rawLrc"] = GetString(res, "txt") ?? string.Empty };
    }

    public async Task<JsonObject> GetAlbumInfoAsync(JsonObject albumItem)
    {
        ArgumentNullException.ThrowIfNull(albumItem);

        var id = GetString(albumItem, "id")
            ?? throw new ArgumentException("albumItem.id 不能为空", nameof(albumItem));

        JsonObject res = await GetJsonAsync(
            "http://service.5sing.kugou.com/song/getPlayListSong",
            new QueryCollection { { "id", id } },
            BuildMobileHeaders()
        );

        JsonArray musicList = [];
        foreach (JsonNode? item in res["data"]?.AsArray() ?? [])
        {
            if (item is not JsonObject song)
            {
                continue;
            }

            musicList.Add(
                new JsonObject
                {
                    ["id"] = GetString(song, "ID"),
                    ["typeEname"] = GetString(song, "SK"),
                    ["title"] = GetString(song, "SN"),
                    ["artist"] = GetString(song["user"] as JsonObject, "NN"),
                    ["singerId"] = GetString(song["user"] as JsonObject, "ID"),
                    ["album"] = GetString(albumItem, "title"),
                    ["artwork"] = GetString(albumItem, "artwork"),
                }
            );
        }

        return new JsonObject { ["musicList"] = musicList };
    }
public async Task<JsonObject?> GetMediaSourceAsync(JsonObject musicItem, string quality)
    {
        ArgumentNullException.ThrowIfNull(musicItem);

        var songId = GetString(musicItem, "id");
        var songType = GetString(musicItem, "typeEname");
        if (string.IsNullOrWhiteSpace(songId) || string.IsNullOrWhiteSpace(songType))
        {
            return null;
        }

        JsonObject? root = await GetSongUrlAsync(songId, songType);
        if (root?["data"] is not JsonObject data)
        {
            return null;
        }

        var url = quality switch
        {
            "low" => GetString(data, "lqurl") ?? GetString(data, "lqurl_backup"),
            "standard" => GetString(data, "squrl") ?? GetString(data, "squrl_backup"),
            "high" or "super" => GetString(data, "hqurl") ?? GetString(data, "hqurl_backup"),
            _ => GetString(data, "hqurl") ?? GetString(data, "hqurl_backup"),
        };

        url ??= GetString(data, "squrl") ?? GetString(data, "squrl_backup");
        url ??= GetString(data, "lqurl") ?? GetString(data, "lqurl_backup");

        return string.IsNullOrWhiteSpace(url) ? null : new JsonObject { ["url"] = url };
    }

    private async Task<JsonObject?> GetSongUrlAsync(string songId, string songType)
    {
        string deviceId = Guid.NewGuid().ToString("N");
        SortedDictionary<string, string> parameters = new(StringComparer.Ordinal)
        {
            ["appid"] = "2918",
            ["clientver"] = "1000",
            ["mid"] = deviceId,
            ["uuid"] = deviceId,
            ["dfid"] = "-",
            ["songid"] = songId,
            ["songtype"] = songType,
            ["version"] = "6.6.72",
            ["clienttime"] = DateTimeOffset
                .UtcNow.ToUnixTimeMilliseconds()
                .ToString(CultureInfo.InvariantCulture),
        };

        const string appKey = "5uytoxQewcvIc1gn1PlNF0T2jbbOzRl5";
        StringBuilder signBuilder = new();
        signBuilder.Append(appKey);
        foreach ((string key, string value) in parameters)
        {
            signBuilder.Append(key).Append('=').Append(value);
        }

        signBuilder.Append(appKey);

        string signature = Convert
            .ToHexString(MD5.HashData(Encoding.UTF8.GetBytes(signBuilder.ToString())))
            .ToLowerInvariant();

        QueryCollection query = [with(parameters.Count + 1)];
        foreach ((string key, string value) in parameters)
        {
            query.Add(key, value);
        }

        query.Add("signature", signature);

        return await GetJsonAsync(
            "https://5sservice.kugou.com/song/getsongurl",
            query,
            BuildSongUrlHeaders()
        );
    }

    private static QueryCollection BuildSongUrlHeaders() =>
        new()
        {
            { "Accept", "application/json, text/javascript, */*; q=0.01" },
            { "Accept-Encoding", "gzip, deflate" },
            { "Accept-Language", "zh-CN,zh;q=0.9" },
            { "Referer", "https://5sing.kugou.com/" },
            {
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36"
            },
        };

    public async Task<string> GetArtistPageHtmlAsync(string artistId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);

        return await GetStringAsync(
            $"https://5sing.kugou.com/{Uri.EscapeDataString(artistId)}/default.html",
            null,
            null
        );
    }

    private async Task<JsonObject> SearchAsync(string query, int page, int type, int filter)
    {
        JsonObject res = await GetJsonAsync(
            "http://search.5sing.kugou.com/home/json",
            new QueryCollection
            {
                { "keyword", query },
                { "sort", "1" },
                { "page", page.ToString(CultureInfo.InvariantCulture) },
                { "filter", filter.ToString(CultureInfo.InvariantCulture) },
                { "type", type.ToString(CultureInfo.InvariantCulture) },
            },
            BuildSearchHeaders()
        );

        JsonArray data = [];
        foreach (JsonNode? item in res["list"]?.AsArray() ?? [])
        {
            if (item is not JsonObject source)
            {
                continue;
            }

            data.Add(
                type switch
                {
                    0 => FormatMusicItem(source),
                    1 => FormatAlbumItem(source),
                    2 => FormatArtistItem(source),
                    _ => source.DeepClone(),
                }
            );
        }

        var cur = res["pageInfo"]?["cur"]?.GetValue<int>() ?? 0;
        var totalPages = res["pageInfo"]?["totalPages"]?.GetValue<int>() ?? 0;

        return new JsonObject { ["isEnd"] = cur >= totalPages, ["data"] = data };
    }
private async Task<JsonObject> GetJsonAsync(
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers
    )
    {
        var text = await GetStringAsync(url, query, headers);
        return JsonNode.Parse(text)?.AsObject() ?? [];
    }

    private async Task<string> GetStringAsync(
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers
    )
    {
        using HttpResponseMessage response = await _client.SendAsync(
            HttpMethod.Get,
            url,
            query,
            headers,
            content: (string?)null,
            contentType: null
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static QueryCollection BuildSearchHeaders() =>
        new()
        {
            {
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36"
            },
            { "Host", "search.5sing.kugou.com" },
            { "Accept", "application/json, text/javascript, */*; q=0.01" },
            { "Accept-Encoding", "gzip, deflate" },
            { "Accept-Language", "zh-CN,zh;q=0.9" },
            { "Referer", "http://search.5sing.kugou.com/home/index" },
        };

    private static QueryCollection BuildCommonHeaders() =>
        new()
        {
            { "Accept", "*/*" },
            { "Accept-Encoding", "gzip, deflate" },
            { "Accept-Language", "zh-CN,zh;q=0.9" },
            { "Host", "service.5sing.kugou.com" },
            { "Referer", "http://5sing.kugou.com/" },
            {
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36"
            },
        };

    private static QueryCollection BuildMobileHeaders() =>
        new()
        {
            { "Accept", "application/json, text/plain, */*" },
            { "Accept-Encoding", "gzip, deflate" },
            { "Accept-Language", "zh-CN,zh;q=0.9" },
            { "Cache-Control", "no-cache" },
            { "Host", "service.5sing.kugou.com" },
            { "Origin", "http://5sing.kugou.com" },
            { "Referer", "http://5sing.kugou.com/" },
            {
                "User-Agent",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1"
            },
        };

    private static JsonObject FormatMusicItem(JsonObject source) =>
        new()
        {
            ["id"] = GetString(source, "songId"),
            ["title"] = HtmlToText(GetString(source, "songName")),
            ["artist"] = GetString(source, "singer"),
            ["singerId"] = GetString(source, "singerId"),
            ["album"] = GetString(source, "typeName"),
            ["type"] = GetString(source, "type"),
            ["typeName"] = GetString(source, "typeName"),
            ["typeEname"] = GetString(source, "typeEname"),
        };

    private static JsonObject FormatAlbumItem(JsonObject source) =>
        new()
        {
            ["id"] = GetString(source, "songListId"),
            ["artist"] = GetString(source, "userName"),
            ["title"] = HtmlToText(GetString(source, "title")),
            ["artwork"] = GetString(source, "pictureUrl"),
            ["description"] = GetString(source, "content"),
            ["date"] = GetString(source, "createTime"),
        };

    private static JsonObject FormatArtistItem(JsonObject source) =>
        new()
        {
            ["id"] = GetString(source, "id"),
            ["name"] = HtmlToText(GetString(source, "nickName")),
            ["fans"] = GetString(source, "fans"),
            ["avatar"] = GetString(source, "pictureUrl"),
            ["description"] = GetString(source, "description"),
            ["worksNum"] = GetString(source, "totalSong"),
        };

    private static string HtmlToText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return WebUtility.HtmlDecode(HtmlTagRegex().Replace(html, string.Empty));
    }

    private static string? GetString(JsonObject? source, string key)
    {
        if (
            source is null
            || !source.TryGetPropertyValue(key, out JsonNode? value)
            || value is null
        )
        {
            return null;
        }

        return value switch
        {
            JsonValue jsonValue when jsonValue.TryGetValue<string>(out var text) => text,
            JsonValue jsonValue when jsonValue.TryGetValue<int>(out var intValue) =>
                intValue.ToString(CultureInfo.InvariantCulture),
            JsonValue jsonValue when jsonValue.TryGetValue<long>(out var longValue) =>
                longValue.ToString(CultureInfo.InvariantCulture),
            JsonValue jsonValue when jsonValue.TryGetValue<double>(out var doubleValue) =>
                doubleValue.ToString(CultureInfo.InvariantCulture),
            JsonValue jsonValue when jsonValue.TryGetValue<bool>(out var boolValue) =>
                boolValue.ToString(),
            _ => value.ToJsonString(),
        };
    }
public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }
}
