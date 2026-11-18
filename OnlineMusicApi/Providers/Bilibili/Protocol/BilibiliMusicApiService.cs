using OnlineMusicApi.Internal.Infrastructure.Http;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
namespace OnlineMusicApi.Internal.Providers.Bilibili.Protocol;

/// <summary>
/// bilibili 音乐 API
/// </summary>
internal sealed partial class BilibiliMusicApiService : IDisposable
{
    private const int PageSize = 20;

    private static readonly QueryCollection CommonHeaders =
    [
        new(
            "user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.63"
        ),
        new("accept", "*/*"),
        new("accept-encoding", "gzip, deflate, br"),
        new("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6"),
    ];

    private static readonly QueryCollection SearchHeaders =
    [
        new(
            "user-agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.90 Safari/537.36 Edg/89.0.774.63"
        ),
        new("accept", "application/json, text/plain, */*"),
        new("accept-encoding", "gzip, deflate, br"),
        new("origin", "https://search.bilibili.com"),
        new("sec-fetch-site", "same-site"),
        new("sec-fetch-mode", "cors"),
        new("sec-fetch-dest", "empty"),
        new("referer", "https://search.bilibili.com/"),
        new("accept-language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6"),
    ];

    [GeneratedRegex("(<em(.*?)>)|(</em>)", RegexOptions.Compiled)]
    private static partial Regex EmTagRegex();

    [GeneratedRegex("《(.+?)》", RegexOptions.Compiled)]
    private static partial Regex AliasRegex();

    [GeneratedRegex("[!'()*]", RegexOptions.Compiled)]
    private static partial Regex RidFilterRegex();

    private readonly HttpClient _client;
    private readonly bool _ownsClient;

    private string? _buvid3;
    private string? _buvid4;
    private string? _imgKey;
    private string? _subKey;
    private DateTime _wbiSyncDate;

    public BilibiliMusicApiService()
    {
        HttpClientHandler handler = new()
        {
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = true,
        };
        _client = new HttpClient(handler);
        _ownsClient = true;
    }

    public BilibiliMusicApiService(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = false;
    }

    public Task<JsonObject> SearchMusicAsync(string keyword, int page) =>
        SearchAlbumAsync(keyword, page);

    public async Task<JsonObject> SearchAlbumAsync(string keyword, int page)
    {
        JsonObject data = await SearchBaseAsync(keyword, page, "video");
        JsonArray result = [];

        foreach (JsonNode? item in data["result"]?.AsArray() ?? [])
        {
            if (item is JsonObject source)
            {
                result.Add(FormatMedia(source));
            }
        }

        int numResults = data["numResults"]?.GetValue<int>() ?? 0;
        return new JsonObject { ["isEnd"] = numResults <= page * PageSize, ["data"] = result };
    }

    public async Task<JsonObject> SearchArtistAsync(string keyword, int page)
    {
        JsonObject data = await SearchBaseAsync(keyword, page, "bili_user");
        JsonArray result = [];

        foreach (JsonNode? item in data["result"]?.AsArray() ?? [])
        {
            if (item is not JsonObject source)
            {
                continue;
            }

            string? upic = GetString(source, "upic");
            result.Add(
                new JsonObject
                {
                    ["name"] = GetString(source, "uname"),
                    ["id"] = GetString(source, "mid"),
                    ["fans"] = GetString(source, "fans"),
                    ["description"] = GetString(source, "usign"),
                    ["avatar"] =
                        upic?.StartsWith("//", StringComparison.Ordinal) == true
                            ? $"https://{upic[2..]}"
                            : upic,
                    ["worksNum"] = GetString(source, "videos"),
                }
            );
        }

        int numResults = data["numResults"]?.GetValue<int>() ?? 0;
        return new JsonObject { ["isEnd"] = numResults <= page * PageSize, ["data"] = result };
    }

    public async Task<JsonObject?> GetMediaSourceAsync(JsonObject musicItem, string quality)
    {
        ArgumentNullException.ThrowIfNull(musicItem);

        string? cid = GetString(musicItem, "cid");
        if (string.IsNullOrWhiteSpace(cid))
        {
            JsonObject cidRes = await GetCidAsync(
                GetString(musicItem, "bvid"),
                GetString(musicItem, "aid")
            );
            cid = GetString(cidRes["data"] as JsonObject, "cid");
        }

        if (string.IsNullOrWhiteSpace(cid))
        {
            return null;
        }

        QueryCollection query = new() { { "cid", cid }, { "fnval", "16" } };
        string? bvid = GetString(musicItem, "bvid");
        string? aid = GetString(musicItem, "aid");
        if (!string.IsNullOrWhiteSpace(bvid))
        {
            query.Add("bvid", bvid);
        }
        else if (!string.IsNullOrWhiteSpace(aid))
        {
            query.Add("aid", aid);
        }

        JsonObject res = await GetJsonAsync(
            "https://api.bilibili.com/x/player/playurl",
            query,
            CommonHeaders
        );
        if (res["data"] is not JsonObject data)
        {
            return null;
        }

        string? url = null;
        if (data["dash"] is JsonObject dash && dash["audio"] is JsonArray audios)
        {
            List<JsonObject> audioList =
            [
                .. audios.OfType<JsonObject>().OrderBy(a => a["bandwidth"]?.GetValue<int>() ?? 0),
            ];

            if (audioList.Count > 0)
            {
                int index = quality switch
                {
                    "low" => 0,
                    "standard" => 1,
                    "high" => 2,
                    "super" => 3,
                    _ => 1,
                };
                index = Math.Min(index, audioList.Count - 1);
                url = GetString(audioList[index], "baseUrl")
                    ?? GetString(audioList[index], "base_url");
            }
        }
        else
        {
            url = data["durl"]?.AsArray().FirstOrDefault()?["url"]?.ToString();
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        string host = new Uri(url).Host;
        string referValue = $"https://www.bilibili.com/video/{bvid ?? aid ?? string.Empty}";

        return new JsonObject
        {
            ["url"] = url,
            ["headers"] = new JsonObject
            {
                ["user-agent"] = CommonHeaders[0].Value,
                ["accept"] = "*/*",
                ["host"] = host,
                ["accept-encoding"] = "gzip, deflate, br",
                ["connection"] = "keep-alive",
                ["referer"] = referValue,
            },
        };
    }

    public async Task<JsonObject> GetAlbumInfoAsync(JsonObject albumItem)
    {
        ArgumentNullException.ThrowIfNull(albumItem);

        JsonObject cidRes = await GetCidAsync(
            GetString(albumItem, "bvid"),
            GetString(albumItem, "aid")
        );
        JsonObject? data = cidRes["data"] as JsonObject;
        string? cid = GetString(data, "cid");
        JsonArray? pages = data?["pages"]?.AsArray();

        JsonArray musicList = [];
        if (pages is null || pages.Count <= 1)
        {
            JsonObject copy = CloneObject(albumItem);
            if (!string.IsNullOrWhiteSpace(cid))
            {
                copy["cid"] = cid;
            }
            musicList.Add(copy);
        }
        else
        {
            foreach (JsonNode? page in pages)
            {
                if (page is not JsonObject p)
                {
                    continue;
                }

                JsonObject copy = CloneObject(albumItem);
                copy["cid"] = GetString(p, "cid");
                copy["title"] = GetString(p, "part");
                copy["duration"] = DurationToSec(p["duration"]);
                copy["id"] = GetString(p, "cid");

                musicList.Add(copy);
            }
        }

        return new JsonObject { ["musicList"] = musicList };
    }

    public async Task<JsonObject> GetUserCardAsync(string mid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mid);

        return await GetJsonAsync(
            "https://api.bilibili.com/x/web-interface/card",
            ToQueryCollection(new Dictionary<string, object?> { ["mid"] = mid }),
            CommonHeaders
        );
    }

    public async Task<JsonObject> GetArtistWorksAsync(JsonObject artistItem, int page, string type)
    {
        _ = type;
        await EnsureCookieAsync();

        string id = GetString(artistItem, "id")
            ?? throw new ArgumentException("artistItem.id 不能为空", nameof(artistItem));

        string now = DateTimeOffset
            .UtcNow.ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);
        Dictionary<string, object?> baseParams = new()
        {
            ["mid"] = id,
            ["ps"] = 30,
            ["tid"] = 0,
            ["pn"] = page,
            ["web_location"] = 1550101,
            ["order_avoided"] = true,
            ["order"] = "pubdate",
            ["keyword"] = string.Empty,
            ["platform"] = "web",
            ["dm_img_list"] = "[]",
            ["dm_img_str"] = "V2ViR0wgMS4wIChPcGVuR0wgRVMgMi4wIENocm9taXVtKQ",
            ["dm_cover_img_str"] =
                "QU5HTEUgKE5WSURJQSwgTlZJRElBIEdlRm9yY2UgR1RYIDE2NTAgKDB4MDAwMDFGOTEpIERpcmVjdDNEMTEgdnNfNV8wIHBzXzVfMCwgRDNEMTEpR29vZ2xlIEluYy4gKE5WSURJQS",
            ["dm_img_inter"] = "{\"ds\":[],\"wh\":[0,0,0],\"of\":[0,0,0]}",
            ["wts"] = now,
        };

        string wRid = await GetRidAsync(baseParams);
        QueryCollection query = ToQueryCollection(baseParams);
        query.Add("w_rid", wRid);

        QueryCollection headers = new()
        {
            { "user-agent", CommonHeaders[0].Value },
            { "accept", "*/*" },
            { "accept-encoding", "gzip, deflate, br, zstd" },
            { "origin", "https://space.bilibili.com" },
            { "sec-fetch-site", "same-site" },
            { "sec-fetch-mode", "cors" },
            { "sec-fetch-dest", "empty" },
            { "referer", $"https://space.bilibili.com/{id}/video" },
            { "cookie", $"buvid3={_buvid3};buvid4={_buvid4}" },
        };

        JsonObject res = await GetJsonAsync(
            "https://api.bilibili.com/x/space/wbi/arc/search",
            query,
            headers
        );
        JsonObject resultData = res["data"] as JsonObject ?? [];

        JsonArray mapped = [];
        foreach (JsonNode? item in resultData["list"]?["vlist"]?.AsArray() ?? [])
        {
            if (item is JsonObject source)
            {
                mapped.Add(FormatMedia(source));
            }
        }

        int pn = resultData["page"]?["pn"]?.GetValue<int>() ?? 0;
        int ps = resultData["page"]?["ps"]?.GetValue<int>() ?? 0;
        int count = resultData["page"]?["count"]?.GetValue<int>() ?? 0;

        return new JsonObject { ["isEnd"] = pn * ps >= count, ["data"] = mapped };
    }

    private async Task<JsonObject> GetCidAsync(string? bvid, string? aid)
    {
        QueryCollection query = [];
        if (!string.IsNullOrWhiteSpace(bvid))
        {
            query.Add("bvid", bvid);
        }
        else if (!string.IsNullOrWhiteSpace(aid))
        {
            query.Add("aid", aid);
        }

        return await GetJsonAsync(
            "https://api.bilibili.com/x/web-interface/view",
            query,
            CommonHeaders
        );
    }

    private async Task<JsonObject> SearchBaseAsync(string keyword, int page, string searchType)
    {
        await EnsureCookieAsync();

        QueryCollection query = new()
        {
            { "context", string.Empty },
            { "page", page.ToString(CultureInfo.InvariantCulture) },
            { "order", string.Empty },
            { "page_size", PageSize.ToString(CultureInfo.InvariantCulture) },
            { "keyword", keyword },
            { "duration", string.Empty },
            { "tids_1", string.Empty },
            { "tids_2", string.Empty },
            { "__refresh__", "true" },
            { "_extra", string.Empty },
            { "highlight", "1" },
            { "single_column", "0" },
            { "platform", "pc" },
            { "from_source", string.Empty },
            { "search_type", searchType },
            { "dynamic_offset", "0" },
        };

        QueryCollection headers = [.. SearchHeaders];
        headers.Add("cookie", $"buvid3={_buvid3};buvid4={_buvid4}");

        JsonObject res = await GetJsonAsync(
            "https://api.bilibili.com/x/web-interface/search/type",
            query,
            headers
        );
        return res["data"]?.AsObject() ?? [];
    }

    private async Task EnsureCookieAsync()
    {
        if (!string.IsNullOrWhiteSpace(_buvid3) && !string.IsNullOrWhiteSpace(_buvid4))
        {
            return;
        }

        QueryCollection headers = new()
        {
            {
                "User-Agent",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1 Edg/114.0.0.0"
            },
        };

        JsonObject res = await GetJsonAsync(
            "https://api.bilibili.com/x/frontend/finger/spi",
            null,
            headers
        );
        JsonObject? data = res["data"] as JsonObject;
        _buvid3 = GetString(data, "b_3");
        _buvid4 = GetString(data, "b_4");
    }

    private async Task<(string img, string sub)> GetWbiKeysAsync()
    {
        if (
            !string.IsNullOrWhiteSpace(_imgKey)
            && !string.IsNullOrWhiteSpace(_subKey)
            && _wbiSyncDate.Date == DateTime.Today
        )
        {
            return (_imgKey, _subKey);
        }

        JsonObject data = await GetBiliTicketAsync();
        JsonObject nav = data["nav"] as JsonObject ?? [];

        _imgKey = Path.GetFileNameWithoutExtension(GetString(nav, "img") ?? string.Empty);
        _subKey = Path.GetFileNameWithoutExtension(GetString(nav, "sub") ?? string.Empty);
        _wbiSyncDate = DateTime.Now;

        return (_imgKey, _subKey);
    }

    private async Task<JsonObject> GetBiliTicketAsync()
    {
        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string hexSign = HmacSha256Hex("XgwSnGZ1p", $"ts{ts}");

        QueryCollection query = new()
        {
            { "key_id", "ec02" },
            { "hexsign", hexSign },
            { "context[ts]", ts.ToString(CultureInfo.InvariantCulture) },
            { "csrf", string.Empty },
        };

        QueryCollection headers = new()
        {
            {
                "User-Agent",
                "Mozilla/5.0 (X11; Linux x86_64; rv:109.0) Gecko/20100101 Firefox/115.0"
            },
        };

        JsonObject res = await SendForJsonAsync(
            HttpMethod.Post,
            "https://api.bilibili.com/bapis/bilibili.api.ticket.v1.Ticket/GenWebTicket",
            query,
            headers,
            content: null
        );
        return res["data"]?.AsObject() ?? [];
    }

    private async Task<string> GetRidAsync(Dictionary<string, object?> parameters)
    {
        (string? img, string? sub) = await GetWbiKeysAsync();
        string mixin = GetMixinKey(img + sub);

        List<string> pairs = [];
        foreach (string key in parameters.Keys.OrderBy(x => x, StringComparer.Ordinal))
        {
            object? value = parameters[key];
            if (value is null)
            {
                continue;
            }

            string text = value.ToString() ?? string.Empty;
            text = RidFilterRegex().Replace(text, string.Empty);
            pairs.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(text)}");
        }

        string f = string.Join("&", pairs);
        return Md5Hex(f + mixin);
    }

    private async Task<JsonObject> GetJsonAsync(
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers
    ) => await SendForJsonAsync(HttpMethod.Get, url, query, headers, null);

    private async Task<JsonObject> SendForJsonAsync(
        HttpMethod method,
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers,
        string? content
    )
    {
        using HttpResponseMessage response = await _client.SendAsync(
            method,
            url,
            query,
            headers,
            (string?)content,
            "application/json"
        );
        response.EnsureSuccessStatusCode();
        string text = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(text)?.AsObject() ?? [];
    }

    private static int DurationToSec(JsonNode? duration)
    {
        if (duration is null)
        {
            return 0;
        }

        if (duration is JsonValue number && number.TryGetValue<int>(out int seconds))
        {
            return seconds;
        }

        string text = duration.ToString();
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int numeric))
        {
            return numeric;
        }

        string[] parts = text.Split(':', StringSplitOptions.RemoveEmptyEntries);
        int sum = 0;
        foreach (string part in parts)
        {
            if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v))
            {
                continue;
            }
            sum = 60 * sum + v;
        }

        return sum;
    }

    private static JsonObject FormatMedia(JsonObject source)
    {
        string rawTitle = GetString(source, "title") ?? string.Empty;
        string title = WebUtility.HtmlDecode(EmTagRegex().Replace(rawTitle, string.Empty));
        string? pic = GetString(source, "pic");

        JsonObject result = new()
        {
            ["id"] = GetString(source, "cid")
                ?? GetString(source, "bvid")
                ?? GetString(source, "aid"),
            ["aid"] = GetString(source, "aid"),
            ["bvid"] = GetString(source, "bvid"),
            ["artist"] = GetString(source, "author")
                ?? GetString(source["owner"] as JsonObject, "name"),
            ["title"] = title,
            ["alias"] = AliasRegex().Match(title) is { Success: true } m ? m.Groups[1].Value : null,
            ["album"] = GetString(source, "bvid") ?? GetString(source, "aid"),
            ["artwork"] =
                pic?.StartsWith("//", StringComparison.Ordinal) == true ? "http:" + pic : pic,
            ["duration"] = DurationToSec(source["duration"]),
            ["tags"] = GetString(source, "tag") is { Length: > 0 } tag
                ? new JsonArray(
                    tag.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => JsonValue.Create(s.Trim()))
                        .ToArray()
                )
                : null,
            ["date"] = FormatDate(GetString(source, "pubdate") ?? GetString(source, "created")),
        };

        return result;
    }

    private static QueryCollection ToQueryCollection(Dictionary<string, object?> source)
    {
        QueryCollection query = [with(source.Count)];
        foreach ((string? key, object? value) in source)
        {
            if (value is null)
            {
                continue;
            }

            query.Add(key, value.ToString() ?? string.Empty);
        }

        return query;
    }

    private static JsonObject CloneObject(JsonObject source)
    {
        JsonObject clone = [];
        foreach ((string? key, JsonNode? value) in source)
        {
            clone[key] = value?.DeepClone();
        }

        return clone;
    }

    private static string GetMixinKey(string text)
    {
        int[] map = new[]
        {
            46,
            47,
            18,
            2,
            53,
            8,
            23,
            32,
            15,
            50,
            10,
            31,
            58,
            3,
            45,
            35,
            27,
            43,
            5,
            49,
            33,
            9,
            42,
            19,
            29,
            28,
            14,
            39,
            12,
            38,
            41,
            13,
            37,
            48,
            7,
            16,
            24,
            55,
            40,
            61,
            26,
            17,
            0,
            1,
            60,
            51,
            30,
            4,
            22,
            25,
            54,
            21,
            56,
            59,
            6,
            63,
            57,
            62,
            11,
            36,
            20,
            34,
            44,
            52,
        };

        StringBuilder sb = new();
        foreach (int i in map)
        {
            if (i < text.Length)
            {
                sb.Append(text[i]);
            }
        }

        return sb.Length > 32 ? sb.ToString()[..32] : sb.ToString();
    }

    private static string HmacSha256Hex(string key, string message)
    {
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(key));
        byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Md5Hex(string text)
    {
        byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
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
            JsonValue jsonValue when jsonValue.TryGetValue<string>(out string? text) => text,
            JsonValue jsonValue when jsonValue.TryGetValue<int>(out int intValue) =>
                intValue.ToString(CultureInfo.InvariantCulture),
            JsonValue jsonValue when jsonValue.TryGetValue<long>(out long longValue) =>
                longValue.ToString(CultureInfo.InvariantCulture),
            JsonValue jsonValue when jsonValue.TryGetValue<double>(out double doubleValue) =>
                doubleValue.ToString(CultureInfo.InvariantCulture),
            JsonValue jsonValue when jsonValue.TryGetValue<bool>(out bool boolValue) =>
                boolValue.ToString(),
            _ => value.ToJsonString(),
        };
    }

    private static string? FormatDate(string? unixSeconds)
    {
        if (
            !long.TryParse(
                unixSeconds,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long ts
            )
        )
        {
            return null;
        }

        return DateTimeOffset
            .FromUnixTimeSeconds(ts)
            .ToLocalTime()
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }
}
