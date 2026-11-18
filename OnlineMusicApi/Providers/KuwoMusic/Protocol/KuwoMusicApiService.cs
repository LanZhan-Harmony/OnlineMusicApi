using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using OnlineMusicApi.Internal.Providers.KuwoMusic.Protocol.Extensions;

namespace OnlineMusicApi.Internal.Providers.KuwoMusic.Protocol;

/// <summary>
/// 酷我音乐 API
/// </summary>
internal sealed partial class KuwoMusicApiService : IDisposable
{
    private const int PageSize = 20;

    private readonly HttpClient _client;
    private readonly bool _ownsClient;

    public KuwoMusicApiService()
    {
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = true,
        };
        _client = new HttpClient(handler);
        _ownsClient = true;
    }

    public KuwoMusicApiService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = false;
    }

    public Task<JsonObject> SearchMusicAsync(string query, int page) =>
        SearchAsync(query, page, "music");

    public Task<JsonObject> SearchAlbumAsync(string query, int page) =>
        SearchAsync(query, page, "album");

    public Task<JsonObject> SearchArtistAsync(string query, int page) =>
        SearchAsync(query, page, "artist");

    public Task<JsonObject> SearchMusicSheetAsync(string query, int page) =>
        SearchAsync(query, page, "playlist");

    public async Task<JsonObject?> GetMediaSourceAsync(JsonObject musicItem, string? quality)
    {
        ArgumentNullException.ThrowIfNull(musicItem);

        var id = GetString(musicItem, "id");
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        JsonObject res = await GetJsonAsync(
            "https://antiserver.kuwo.cn/anti.s",
            new QueryCollection
            {
                { "type", "convert_url3" },
                { "rid", id },
                { "format", "mp3" },
            }
        );

        var url = GetString(res, "url");
        return string.IsNullOrWhiteSpace(url) ? null : new JsonObject { ["url"] = url };
    }

    public async Task<string> GetArtistPageHtmlAsync(string artistId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);

        return await GetStringResponseAsync(
            HttpMethod.Get,
            $"https://www.kuwo.cn/singer_detail/{Uri.EscapeDataString(artistId)}",
            null
        );
    }

    public async Task<string> GetAlbumPageHtmlAsync(string albumId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);

        return await GetStringResponseAsync(
            HttpMethod.Get,
            $"https://www.kuwo.cn/album_detail/{Uri.EscapeDataString(albumId)}",
            null
        );
    }

    public async Task<JsonObject> GetArtistWorksAsync(JsonObject artistItem, int page, string type)
    {
        ArgumentNullException.ThrowIfNull(artistItem);

        return type switch
        {
            "album" => await GetArtistAlbumWorksAsync(artistItem, page),
            _ => new JsonObject { ["isEnd"] = true, ["data"] = new JsonArray() },
        };
    }

    public async Task<JsonObject> GetLyricAsync(JsonObject musicItem)
    {
        ArgumentNullException.ThrowIfNull(musicItem);

        var id = GetString(musicItem, "id")
            ?? throw new ArgumentException("musicItem.id 不能为空", nameof(musicItem));

        JsonObject res = await GetJsonAsync(
            "http://m.kuwo.cn/newh5/singles/songinfoandlrc",
            new QueryCollection { { "musicId", id }, { "httpStatus", "1" } }
        );

        StringBuilder lines = new();
        foreach (JsonNode? item in res["data"]?["lrclist"]?.AsArray() ?? [])
        {
            if (item is not JsonObject lyricLine)
            {
                continue;
            }

            var time = GetString(lyricLine, "time") ?? string.Empty;
            var lyric = GetString(lyricLine, "lineLyric") ?? string.Empty;
            if (lines.Length > 0)
            {
                lines.Append('\n');
            }

            lines.Append('[').Append(time).Append(']').Append(lyric);
        }

        return new JsonObject { ["rawLrc"] = lines.ToString() };
    }

    public async Task<JsonObject> GetAlbumInfoAsync(JsonObject albumItem)
    {
        ArgumentNullException.ThrowIfNull(albumItem);

        var albumId = GetString(albumItem, "id")
            ?? throw new ArgumentException("albumItem.id 不能为空", nameof(albumItem));

        JsonObject res = await GetJsonAsync(
            "http://search.kuwo.cn/r.s",
            new QueryCollection
            {
                { "pn", "0" },
                { "rn", "100" },
                { "albumid", albumId },
                { "stype", "albuminfo" },
                { "sortby", "0" },
                { "alflac", "1" },
                { "show_copyright_off", "1" },
                { "pcmp4", "1" },
                { "encoding", "utf8" },
                { "plat", "pc" },
                { "thost", "search.kuwo.cn" },
                { "vipver", "MUSIC_9.1.1.2_BCS2" },
                { "devid", "38668888" },
                { "newver", "1" },
                { "pcjson", "1" },
            }
        );

        JsonArray musicList = [];
        foreach (JsonNode? song in res["musiclist"]?.AsArray() ?? [])
        {
            if (song is not JsonObject item || !MusicListFilter(item))
            {
                continue;
            }

            musicList.Add(
                new JsonObject
                {
                    ["id"] = GetString(item, "id"),
                    ["artwork"] = GetString(albumItem, "artwork") ?? GetString(res, "img"),
                    ["title"] = HtmlDecode(GetString(item, "name")),
                    ["artist"] = HtmlDecode(GetString(item, "artist")),
                    ["album"] = HtmlDecode(GetString(item, "album")),
                    ["albumId"] = albumId,
                    ["artistId"] = GetString(item, "artistid"),
                    ["formats"] = GetString(item, "formats"),
                    ["duration"] = ParseInt(GetString(item, "duration")) * 1000L,
                }
            );
        }

        return new JsonObject { ["musicList"] = musicList };
    }

    public async Task<JsonArray> GetMusicSheetSongsAsync(string sheetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetId);

        var page = 1;
        var totalPage = 30;
        JsonArray musicList = [];

        while (page < totalPage)
        {
            try
            {
                JsonObject data = await GetMusicSheetResponseByIdAsync(sheetId, page, 80);
                var total = data["total"]?.GetValue<int>() ?? 0;
                totalPage = Math.Max(1, (int)Math.Ceiling(total / 80d));

                foreach (
                    JsonNode? item in data["musicList"]?.AsArray()
                        ?? data["musiclist"]?.AsArray()
                        ?? []
                )
                {
                    if (item is not JsonObject song || !MusicListFilter(song))
                    {
                        continue;
                    }

                    musicList.Add(
                        new JsonObject
                        {
                            ["id"] = GetString(song, "id"),
                            ["title"] = HtmlDecode(GetString(song, "name")),
                            ["artist"] = HtmlDecode(GetString(song, "artist")),
                            ["album"] = HtmlDecode(GetString(song, "album")),
                            ["albumId"] = GetString(song, "albumid"),
                            ["artistId"] = GetString(song, "artistid"),
                            ["formats"] = GetString(song, "formats"),
                            ["duration"] = ParseInt(GetString(song, "duration")) * 1000L,
                        }
                    );
                }
            }
            catch
            {
                // 与原实现一致：单页失败时忽略并继续。
            }

            await Task.Delay(Random.Shared.Next(200, 301));
            page++;
        }

        return musicList;
    }

    private async Task<JsonObject> SearchAsync(string query, int page, string searchType)
    {
        JsonObject res = await GetJsonAsync(
            "http://search.kuwo.cn/r.s",
            new QueryCollection
            {
                { "all", query },
                { "ft", searchType },
                { "itemset", "web_2013" },
                { "client", "kt" },
                { "pn", (page - 1).ToString(CultureInfo.InvariantCulture) },
                { "rn", PageSize.ToString(CultureInfo.InvariantCulture) },
                { "rformat", "json" },
                { "encoding", "utf8" },
                { "pcjson", "1" },
            }
        );

        JsonArray mapped = [];
        IEnumerable<JsonNode?> list = searchType switch
        {
            "music" => res["abslist"]?.AsArray() ?? [],
            "album" => res["albumlist"]?.AsArray() ?? [],
            "artist" => res["abslist"]?.AsArray() ?? [],
            "playlist" => res["abslist"]?.AsArray() ?? [],
            _ => [],
        };

        foreach (JsonNode? item in list)
        {
            if (item is not JsonObject source)
            {
                continue;
            }

            if (searchType is "music" && !MusicListFilter(source))
            {
                continue;
            }

            mapped.Add(
                searchType switch
                {
                    "music" => FormatMusicItem(source),
                    "album" => FormatAlbumItem(source),
                    "artist" => FormatArtistItem(source),
                    "playlist" => FormatMusicSheet(source),
                    _ => source.DeepClone(),
                }
            );
        }

        var pn = ParseInt(GetString(res, "PN"));
        var rn = ParseInt(GetString(res, "RN"));
        var total = ParseInt(GetString(res, "TOTAL"));

        return new JsonObject { ["isEnd"] = (pn + 1) * rn >= total, ["data"] = mapped };
    }

    private async Task<JsonObject> GetArtistAlbumWorksAsync(JsonObject artistItem, int page)
    {
        var id = GetString(artistItem, "id")
            ?? throw new ArgumentException("artistItem.id 不能为空", nameof(artistItem));

        JsonObject res = await GetJsonAsync(
            "http://search.kuwo.cn/r.s",
            new QueryCollection
            {
                { "pn", (page - 1).ToString(CultureInfo.InvariantCulture) },
                { "rn", PageSize.ToString(CultureInfo.InvariantCulture) },
                { "artistid", id },
                { "stype", "albumlist" },
                { "sortby", "1" },
                { "alflac", "1" },
                { "show_copyright_off", "1" },
                { "pcmp4", "1" },
                { "encoding", "utf8" },
                { "plat", "pc" },
                { "thost", "search.kuwo.cn" },
                { "vipver", "MUSIC_9.1.1.2_BCS2" },
                { "devid", "38668888" },
                { "newver", "1" },
                { "pcjson", "1" },
            }
        );

        JsonArray albums = [];
        foreach (JsonNode? item in res["albumlist"]?.AsArray() ?? [])
        {
            if (item is not JsonObject album)
            {
                continue;
            }

            albums.Add(FormatAlbumItem(album));
        }

        var pn = ParseInt(GetString(res, "pn"));
        var total = ParseInt(GetString(res, "total"));
        return new JsonObject { ["isEnd"] = (pn + 1) * PageSize >= total, ["data"] = albums };
    }

    private async Task<JsonObject> GetMusicSheetResponseByIdAsync(string id, int page, int pageSize)
    {
        return await GetJsonAsync(
            "http://nplserver.kuwo.cn/pl.svc",
            new QueryCollection
            {
                { "op", "getlistinfo" },
                { "pid", id },
                { "pn", (page - 1).ToString(CultureInfo.InvariantCulture) },
                { "rn", pageSize.ToString(CultureInfo.InvariantCulture) },
                { "encode", "utf8" },
                { "keyset", "pl2012" },
                { "vipver", "MUSIC_9.1.1.2_BCS2" },
                { "newver", "1" },
            }
        );
    }

    private async Task<JsonObject> GetJsonAsync(
        string url,
        IEnumerable<KeyValuePair<string, string>>? query = null
    )
    {
        var text = await GetStringResponseAsync(HttpMethod.Get, url, query);

        try
        {
            JsonNode? node = JsonNode.Parse(text);
            if (node is JsonObject jsonObject)
            {
                return jsonObject;
            }

            if (node is JsonArray jsonArray)
            {
                return new JsonObject { ["array"] = jsonArray };
            }
        }
        catch
        {
            // 某些接口偶发返回非 JSON；保持空对象避免调用方崩溃。
        }

        return [];
    }

    private async Task<string> GetStringResponseAsync(
        HttpMethod method,
        string url,
        IEnumerable<KeyValuePair<string, string>>? query
    )
    {
        QueryCollection headers = new()
        {
            { "User-Agent", ChooseUserAgent() },
            { "Referer", "http://www.kuwo.cn" },
        };

        using HttpResponseMessage response = await _client.SendAsync(
            method,
            url,
            query,
            headers,
            content: (string?)null,
            contentType: null
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static JsonObject FormatMusicItem(JsonObject source)
    {
        var musicRid = GetString(source, "MUSICRID")
            ?.Replace("MUSIC_", string.Empty, StringComparison.Ordinal)
            ?? string.Empty;

        var duration =
            ParseInt(GetString(source, "DURATION") ?? GetString(source, "duration")) * 1000L;

        return new JsonObject
        {
            ["id"] = musicRid,
            ["artwork"] = ArtworkShortToLong(GetString(source, "web_albumpic_short")),
            ["title"] = HtmlDecode(GetString(source, "NAME")),
            ["artist"] = HtmlDecode(GetString(source, "ARTIST")),
            ["album"] = HtmlDecode(GetString(source, "ALBUM")),
            ["albumId"] = GetString(source, "ALBUMID"),
            ["artistId"] = GetString(source, "ARTISTID"),
            ["formats"] = GetString(source, "FORMATS"),
            ["duration"] = duration,
        };
    }

    private static JsonObject FormatAlbumItem(JsonObject source) =>
        new()
        {
            ["id"] = GetString(source, "albumid"),
            ["artist"] = HtmlDecode(GetString(source, "artist")),
            ["title"] = HtmlDecode(GetString(source, "name")),
            ["artwork"] = GetString(source, "img") ?? ArtworkShortToLong(GetString(source, "pic")),
            ["description"] = HtmlDecode(GetString(source, "info")),
            ["date"] = GetString(source, "pub"),
            ["artistId"] = GetString(source, "artistid"),
        };

    private static JsonObject FormatArtistItem(JsonObject source) =>
        new()
        {
            ["id"] = GetString(source, "ARTISTID"),
            ["avatar"] = GetString(source, "hts_PICPATH"),
            ["name"] = HtmlDecode(GetString(source, "ARTIST")),
            ["artistId"] = GetString(source, "ARTISTID"),
            ["description"] = HtmlDecode(GetString(source, "desc")),
            ["worksNum"] = GetString(source, "SONGNUM"),
        };

    private static JsonObject FormatMusicSheet(JsonObject source) =>
        new()
        {
            ["id"] = GetString(source, "playlistid"),
            ["title"] = HtmlDecode(GetString(source, "name")),
            ["artist"] = HtmlDecode(GetString(source, "nickname")),
            ["artwork"] = GetString(source, "pic"),
            ["playCount"] = GetString(source, "playcnt"),
            ["description"] = HtmlDecode(GetString(source, "intro")),
            ["worksNum"] = GetString(source, "songnum"),
        };

    private static bool MusicListFilter(JsonObject item) =>
        !string.Equals(
            GetString(item["payInfo"] as JsonObject, "listen_fragment"),
            "1",
            StringComparison.Ordinal
        );

    private static string? ArtworkShortToLong(string? albumPicShort)
    {
        if (string.IsNullOrWhiteSpace(albumPicShort))
        {
            return null;
        }

        var firstSlash = albumPicShort.IndexOf('/', StringComparison.Ordinal);
        return firstSlash == -1
            ? null
            : $"https://img4.kuwo.cn/star/albumcover/256{albumPicShort[firstSlash..]}";
    }

    private static string HtmlDecode(string? text) => WebUtility.HtmlDecode(text ?? string.Empty);

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

    private static int ParseInt(string? value) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : 0;

    private static string ChooseUserAgent() =>
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36";

    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }
}
