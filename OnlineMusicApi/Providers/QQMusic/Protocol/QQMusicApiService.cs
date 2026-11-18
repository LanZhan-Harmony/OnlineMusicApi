using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using OnlineMusicApi.Internal.Providers.QQMusic.Protocol.Extensions;

namespace OnlineMusicApi.Internal.Providers.QQMusic.Protocol;

/// <summary>
/// QQ音乐 API
/// </summary>
internal sealed partial class QQMusicApiService : IDisposable
{
    private const int PageSize = 20;

    private static readonly Dictionary<int, string> SearchTypeMap = new()
    {
        [0] = "song",
        [2] = "album",
        [1] = "singer",
        [3] = "songlist",
    };

    private static readonly Dictionary<string, (string Prefix, string Extension)> MediaTypeMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["m4a"] = ("C400", ".m4a"),
            ["128"] = ("M500", ".mp3"),
            ["320"] = ("M800", ".mp3"),
            ["ape"] = ("A000", ".ape"),
            ["flac"] = ("F000", ".flac"),
        };

    [GeneratedRegex("callback\\(|MusicJsonCallback\\(|jsonCallback\\(|\\)$", RegexOptions.Compiled)]
    private static partial Regex JsonpWrapperRegex();

    private readonly HttpClient _client;
    private readonly bool _ownsClient;

    public QQMusicApiService()
    {
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            UseCookies = true,
        };
        _client = new HttpClient(handler);
        _ownsClient = true;
    }

    public QQMusicApiService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = false;
    }

    public Task<JsonObject> SearchMusicAsync(string query, int page) => SearchAsync(query, page, 0);

    public Task<JsonObject> SearchAlbumAsync(string query, int page) => SearchAsync(query, page, 2);

    public Task<JsonObject> SearchArtistAsync(string query, int page) =>
        SearchAsync(query, page, 1);

    public Task<JsonObject> SearchMusicSheetAsync(string query, int page) =>
        SearchAsync(query, page, 3);

    public async Task<JsonObject?> GetMediaSourceAsync(JsonObject musicItem, string? quality = null)
    {
        ArgumentNullException.ThrowIfNull(musicItem);

        var songMid = GetString(musicItem, "songmid");
        if (string.IsNullOrEmpty(songMid))
        {
            return null;
        }

        var mediaType = quality switch
        {
            "low" => "m4a",
            "high" => "320",
            "super" => "flac",
            _ => "128",
        };

        // 与 qq.ts 保持一致：先走不带 filename 的 CgiGetVkey，请求可播放链接。
        JsonObject? vKey = await GetPlayableSourceUrlAsync(songMid);
        var sip = vKey?["req_0"]?["data"]?["sip"]?.AsArray().FirstOrDefault()?.ToString();
        var purl = vKey
            ?["req_0"]?["data"]?["midurlinfo"]?.AsArray()
            .FirstOrDefault()
            ?["purl"]?.ToString();

        // 若仍无 purl，再回退到按质量拼接 filename 的旧逻辑。
        if (string.IsNullOrEmpty(purl))
        {
            JsonObject? qualityVKey = await GetSourceUrlAsync(songMid, mediaType);
            sip = qualityVKey?["req_0"]?["data"]?["sip"]?.AsArray().FirstOrDefault()?.ToString();
            purl = qualityVKey
                ?["req_0"]?["data"]?["midurlinfo"]?.AsArray()
                .FirstOrDefault()
                ?["purl"]?.ToString();
        }

        if (string.IsNullOrEmpty(sip) || string.IsNullOrEmpty(purl))
        {
            return null;
        }

        return new JsonObject { ["url"] = $"{sip}{purl}" };
    }

    public async Task<JsonObject> GetAlbumInfoAsync(JsonObject albumItem)
    {
        ArgumentNullException.ThrowIfNull(albumItem);

        var albumMid = GetString(albumItem, "albumMID");
        if (string.IsNullOrEmpty(albumMid))
        {
            throw new ArgumentException("albumMID 不能为空", nameof(albumItem));
        }

        JsonObject payload = new()
        {
            ["comm"] = new JsonObject { ["ct"] = 24, ["cv"] = 10000 },
            ["albumSonglist"] = new JsonObject
            {
                ["method"] = "GetAlbumSongList",
                ["param"] = new JsonObject
                {
                    ["albumMid"] = albumMid,
                    ["albumID"] = 0,
                    ["begin"] = 0,
                    ["num"] = 999,
                    ["order"] = 2,
                },
                ["module"] = "music.musichallAlbum.AlbumSongList",
            },
        };

        var url = ChangeUrlQuery(
            new Dictionary<string, string> { ["data"] = payload.ToJsonString() },
            "https://u.y.qq.com/cgi-bin/musicu.fcg?g_tk=5381&format=json&inCharset=utf8&outCharset=utf-8"
        );
        JsonObject res = await GetJsonAsync(url);

        JsonArray musicList = [];
        foreach (JsonNode? item in res["albumSonglist"]?["data"]?["songList"]?.AsArray() ?? [])
        {
            if (item is JsonObject entry && entry["songInfo"] is JsonObject song)
            {
                musicList.Add(FormatMusicItem(song));
            }
        }

        return new JsonObject { ["musicList"] = musicList };
    }

    public Task<JsonObject> GetArtistWorksAsync(JsonObject artistItem, int page, string type) =>
        type switch
        {
            "album" => GetArtistAlbumsAsync(artistItem, page),
            _ => Task.FromResult(new JsonObject { ["isEnd"] = true, ["data"] = new JsonArray() }),
        };

    public async Task<JsonObject> GetLyricAsync(JsonObject musicItem)
    {
        ArgumentNullException.ThrowIfNull(musicItem);

        var songMid = GetString(musicItem, "songmid");
        if (string.IsNullOrEmpty(songMid))
        {
            throw new ArgumentException("songmid 不能为空", nameof(musicItem));
        }

        var url =
            $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={Uri.EscapeDataString(songMid)}&pcachetime={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&g_tk=5381&loginUin=0&hostUin=0&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0";
        JsonObject res = await GetJsonpAsync(url);

        var lyric = DecodeLyricBase64(GetString(res, "lyric"));
        var trans = DecodeLyricBase64(GetString(res, "trans"));

        JsonObject result = new() { ["rawLrc"] = lyric ?? string.Empty };
        if (!string.IsNullOrEmpty(trans))
        {
            result["translation"] = trans;
        }

        return result;
    }

    public async Task<JsonArray> GetMusicSheetSongsAsync(string sheetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetId);

        var url =
            $"https://i.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&utf8=1&disstid={Uri.EscapeDataString(sheetId)}&loginUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0";
        JsonObject res = await GetJsonpAsync(url, referer: "https://y.qq.com/n/yqq/playlist");

        JsonArray list = [];
        foreach (
            JsonNode? item in res["cdlist"]?.AsArray().FirstOrDefault()?["songlist"]?.AsArray()
                ?? []
        )
        {
            if (item is JsonObject song)
            {
                list.Add(FormatMusicItem(song));
            }
        }

        return list;
    }

    private async Task<JsonObject> SearchAsync(string query, int page, int searchType)
    {
        JsonObject baseResult = await SearchBaseAsync(query, page, searchType);
        JsonArray data = baseResult["data"] as JsonArray ?? [];
        JsonArray mapped = [];

        foreach (JsonNode? item in data)
        {
            if (item is not JsonObject source)
            {
                continue;
            }

            mapped.Add(
                searchType switch
                {
                    0 => FormatMusicItem(source),
                    2 => FormatAlbumItem(source),
                    1 => FormatArtistItem(source),
                    3 => FormatMusicSheetItem(source),
                    _ => source.DeepClone(),
                }
            );
        }

        return new JsonObject
        {
            ["isEnd"] = baseResult["isEnd"]?.GetValue<bool>() ?? true,
            ["data"] = mapped,
        };
    }

    private async Task<JsonObject> SearchBaseAsync(string query, int page, int searchType)
    {
        JsonObject payload = new()
        {
            ["req_1"] = new JsonObject
            {
                ["method"] = "DoSearchForQQMusicDesktop",
                ["module"] = "music.search.SearchCgiService",
                ["param"] = new JsonObject
                {
                    ["num_per_page"] = PageSize,
                    ["page_num"] = page,
                    ["query"] = query,
                    ["search_type"] = searchType,
                },
            },
        };

        JsonObject res = await PostJsonAsync("https://u.y.qq.com/cgi-bin/musicu.fcg", payload);

        var sum = res["req_1"]?["data"]?["meta"]?["sum"]?.GetValue<int>() ?? 0;
        var listName = SearchTypeMap.GetValueOrDefault(searchType, "song");
        JsonArray list = res["req_1"]?["data"]?["body"]?[listName]?["list"]?.AsArray() ?? [];

        return new JsonObject { ["isEnd"] = sum <= page * PageSize, ["data"] = list.DeepClone() };
    }

    private async Task<JsonObject?> GetSourceUrlAsync(string id, string type)
    {
        if (!MediaTypeMap.TryGetValue(type, out (string Prefix, string Extension) typeObj))
        {
            typeObj = MediaTypeMap["128"];
        }

        var guid = Random.Shared.Next(1, 10_000_000).ToString(CultureInfo.InvariantCulture);
        var filename = $"{typeObj.Prefix}{id}{typeObj.Extension}";

        JsonObject payload = new()
        {
            ["req_0"] = new JsonObject
            {
                ["module"] = "vkey.GetVkeyServer",
                ["method"] = "CgiGetVkey",
                ["param"] = new JsonObject
                {
                    ["filename"] = new JsonArray(filename),
                    ["guid"] = guid,
                    ["songmid"] = new JsonArray(id),
                    ["songtype"] = new JsonArray(0),
                    ["uin"] = string.Empty,
                    ["loginflag"] = 1,
                    ["platform"] = "20",
                },
            },
            ["comm"] = new JsonObject
            {
                ["uin"] = string.Empty,
                ["format"] = "json",
                ["ct"] = 19,
                ["cv"] = 0,
                ["authst"] = string.Empty,
            },
        };

        var url = ChangeUrlQuery(
            new Dictionary<string, string>
            {
                ["-"] = "getplaysongvkey",
                ["g_tk"] = "5381",
                ["loginUin"] = string.Empty,
                ["hostUin"] = "0",
                ["format"] = "json",
                ["inCharset"] = "utf8",
                ["outCharset"] = "utf-8",
                ["notice"] = "0",
                ["platform"] = "yqq.json",
                ["needNewCode"] = "0",
                ["data"] = payload.ToJsonString(),
            },
            "https://u.y.qq.com/cgi-bin/musicu.fcg"
        );

        return await GetJsonAsync(url);
    }

    private async Task<JsonObject?> GetPlayableSourceUrlAsync(string songMid)
    {
        JsonObject payload = new()
        {
            ["req_0"] = new JsonObject
            {
                ["module"] = "vkey.GetVkeyServer",
                ["method"] = "CgiGetVkey",
                ["param"] = new JsonObject
                {
                    ["guid"] = Random
                        .Shared.Next(1, 10_000_000)
                        .ToString(CultureInfo.InvariantCulture),
                    ["songmid"] = new JsonArray(songMid),
                    ["songtype"] = new JsonArray(0),
                    ["uin"] = "0",
                    ["loginflag"] = 1,
                    ["platform"] = "20",
                },
            },
            ["comm"] = new JsonObject
            {
                ["uin"] = "0",
                ["format"] = "json",
                ["ct"] = 24,
                ["cv"] = 0,
            },
        };

        var url =
            $"https://u.y.qq.com/cgi-bin/musicu.fcg?data={Uri.EscapeDataString(payload.ToJsonString())}";
        return await GetJsonAsync(url);
    }

    private async Task<JsonObject> GetArtistAlbumsAsync(JsonObject artistItem, int page)
    {
        var singerMid = GetString(artistItem, "singerMID");
        if (string.IsNullOrEmpty(singerMid))
        {
            throw new ArgumentException("singerMID 不能为空", nameof(artistItem));
        }

        JsonObject payload = new()
        {
            ["comm"] = new JsonObject { ["ct"] = 24, ["cv"] = 0 },
            ["singerAlbum"] = new JsonObject
            {
                ["method"] = "get_singer_album",
                ["param"] = new JsonObject
                {
                    ["singermid"] = singerMid,
                    ["order"] = "time",
                    ["begin"] = (page - 1) * PageSize,
                    ["num"] = PageSize,
                    ["exstatus"] = 1,
                },
                ["module"] = "music.web_singer_info_svr",
            },
        };

        var url = ChangeUrlQuery(
            new Dictionary<string, string> { ["data"] = payload.ToJsonString() },
            "https://u.y.qq.com/cgi-bin/musicu.fcg"
        );
        JsonObject res = await GetJsonAsync(url);

        JsonArray albums = [];
        foreach (JsonNode? item in res["singerAlbum"]?["data"]?["list"]?.AsArray() ?? [])
        {
            if (item is JsonObject album)
            {
                albums.Add(FormatAlbumItem(album));
            }
        }

        var total = res["singerAlbum"]?["data"]?["total"]?.GetValue<int>() ?? 0;
        return new JsonObject { ["isEnd"] = total <= page * PageSize, ["data"] = albums };
    }

    private async Task<JsonObject> GetJsonAsync(string url, string referer = "https://y.qq.com")
    {
        var response = await GetStringResponseAsync(HttpMethod.Get, url, null, referer: referer);
        return JsonNode.Parse(response)?.AsObject() ?? [];
    }

    private async Task<JsonObject> PostJsonAsync(
        string url,
        JsonObject payload,
        string referer = "https://y.qq.com"
    )
    {
        var response = await GetStringResponseAsync(
            HttpMethod.Post,
            url,
            null,
            payload.ToJsonString(),
            "application/json",
            referer
        );
        return JsonNode.Parse(response)?.AsObject() ?? [];
    }

    private async Task<JsonObject> GetJsonpAsync(string url, string referer = "https://y.qq.com")
    {
        var response = await GetStringResponseAsync(HttpMethod.Get, url, null, referer: referer);
        return ParseJsonp(response) ?? [];
    }

    private async Task<string> GetStringResponseAsync(
        HttpMethod method,
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        string? content = null,
        string? contentType = null,
        string? referer = null
    )
    {
        QueryCollection headers = new() { { "User-Agent", ChooseUserAgent() } };
        if (!string.IsNullOrEmpty(referer))
        {
            headers.Add("Referer", referer);
        }

        using HttpResponseMessage response = await _client.SendAsync(
            method,
            url,
            query,
            headers,
            content,
            contentType
        );
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static JsonObject? ParseJsonp(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var json = JsonpWrapperRegex().Replace(raw, string.Empty);
        return JsonNode.Parse(json)?.AsObject();
    }

    private static JsonObject FormatMusicItem(JsonObject source)
    {
        JsonObject? albumObject = source["album"] as JsonObject;
        var albumId = GetString(source, "albumid") ?? GetString(albumObject, "id");
        var albumMid = GetString(source, "albummid") ?? GetString(albumObject, "mid");
        var albumName = GetString(source, "albumname") ?? GetString(albumObject, "title");

        JsonArray singers = source["singer"]?.AsArray() ?? [];
        var singerNames = string.Join(
            ", ",
            singers
                .OfType<JsonObject>()
                .Select(s => GetString(s, "name"))
                .Where(name => !string.IsNullOrEmpty(name))!
        );

        JsonArray singerItems = [];
        foreach (JsonObject singer in singers.OfType<JsonObject>())
        {
            singerItems.Add(
                new JsonObject
                {
                    ["id"] = GetString(singer, "id"),
                    ["mid"] = GetString(singer, "mid"),
                    ["name"] = GetString(singer, "name"),
                }
            );
        }

        var duration = int.TryParse(
            GetString(source, "interval"),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int seconds
        )
            ? seconds * 1000L
            : 0L;

        return new JsonObject
        {
            ["id"] = GetString(source, "id") ?? GetString(source, "songid"),
            ["songmid"] = GetString(source, "mid") ?? GetString(source, "songmid"),
            ["title"] = GetString(source, "title") ?? GetString(source, "songname"),
            ["artist"] = singerNames,
            ["artwork"] = string.IsNullOrEmpty(albumMid)
                ? null
                : $"https://y.gtimg.cn/music/photo_new/T002R800x800M000{albumMid}.jpg",
            ["album"] = albumName,
            ["lrc"] = GetString(source, "lyric"),
            ["albumid"] = albumId,
            ["albummid"] = albumMid,
            ["duration"] = duration,
            ["singers"] = singerItems,
        };
    }

    private static JsonObject FormatAlbumItem(JsonObject source)
    {
        var albumMid = GetString(source, "albumMID") ?? GetString(source, "album_mid");

        return new JsonObject
        {
            ["id"] = GetString(source, "albumID") ?? GetString(source, "albumid"),
            ["albumMID"] = albumMid,
            ["title"] = GetString(source, "albumName") ?? GetString(source, "album_name"),
            ["artwork"] = GetString(source, "albumPic")
                ?? (
                    string.IsNullOrEmpty(albumMid)
                        ? null
                        : $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{albumMid}.jpg"
                ),
            ["date"] = GetString(source, "publicTime") ?? GetString(source, "pub_time"),
            ["singerID"] = GetString(source, "singerID") ?? GetString(source, "singer_id"),
            ["artist"] = GetString(source, "singerName") ?? GetString(source, "singer_name"),
            ["singerMID"] = GetString(source, "singerMID") ?? GetString(source, "singer_mid"),
            ["description"] = GetString(source, "desc"),
        };
    }

    private static JsonObject FormatArtistItem(JsonObject source) =>
        new()
        {
            ["name"] = GetString(source, "singerName"),
            ["id"] = GetString(source, "singerID"),
            ["singerMID"] = GetString(source, "singerMID"),
            ["avatar"] = GetString(source, "singerPic"),
            ["worksNum"] = GetString(source, "songNum"),
        };

    private static JsonObject FormatMusicSheetItem(JsonObject source) =>
        new()
        {
            ["title"] = GetString(source, "dissname"),
            ["createAt"] = GetString(source, "createtime"),
            ["description"] = GetString(source, "introduction"),
            ["playCount"] = GetString(source, "listennum"),
            ["worksNums"] = GetString(source, "song_count"),
            ["artwork"] = GetString(source, "imgurl"),
            ["id"] = GetString(source, "dissid"),
            ["artist"] = GetString(source["creator"] as JsonObject, "name"),
        };

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

    private static string ChangeUrlQuery(IReadOnlyDictionary<string, string> query, string baseUrl)
    {
        var index = baseUrl.IndexOf('?', StringComparison.Ordinal);
        var url = index == -1 ? baseUrl : baseUrl[..index];
        Dictionary<string, string> currentQuery = [with(StringComparer.Ordinal)];

        if (index != -1)
        {
            var queryString = baseUrl[(index + 1)..];
            foreach (var item in queryString.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var temp = item.Split('=', 2, StringSplitOptions.None);
                var key = temp[0];
                var value = temp.Length > 1 ? Uri.UnescapeDataString(temp[1]) : string.Empty;
                currentQuery[key] = value;
            }
        }

        foreach ((string? key, string? value) in query)
        {
            currentQuery[key] = value;
        }

        StringBuilder builder = new();
        foreach ((string? key, string? value) in currentQuery)
        {
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(value));
        }

        return builder.Length > 0 ? $"{url}?{builder}" : url;
    }

    private static string? DecodeLyricBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            var bytes = Convert.FromBase64String(value);
            return WebUtility.HtmlDecode(Encoding.UTF8.GetString(bytes));
        }
        catch
        {
            return null;
        }
    }

    public async Task<JsonObject?> GetArtistPageAsync(string artistMid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistMid);

        string html = await GetStringResponseAsync(
            HttpMethod.Get,
            $"https://y.qq.com/n/ryqq_v2/singer/{Uri.EscapeDataString(artistMid)}",
            null,
            referer: "https://y.qq.com/"
        );

        return ExtractInitialData(html);
    }

    public async Task<JsonObject?> GetAlbumPageAsync(string albumMid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumMid);

        string html = await GetStringResponseAsync(
            HttpMethod.Get,
            $"https://y.qq.com/n/ryqq_v2/albumDetail/{Uri.EscapeDataString(albumMid)}",
            null,
            referer: "https://y.qq.com/"
        );

        return ExtractInitialData(html);
    }

    private static JsonObject? ExtractInitialData(string html)
    {
        const string marker = "window.__INITIAL_DATA__";
        int index = html.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        int assignment = html.IndexOf('=', index);
        int end = html.IndexOf("</script>", assignment, StringComparison.Ordinal);
        if (assignment < 0 || end < 0)
        {
            return null;
        }

        string json = html[(assignment + 1)..end].Trim();
        if (json.EndsWith(';'))
        {
            json = json[..^1];
        }

        json = json.Replace("undefined", "null", StringComparison.Ordinal);
        return JsonNode.Parse(json)?.AsObject();
    }

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
