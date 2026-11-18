using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using OnlineMusicApi.Internal.Contracts.Bilibili;
using OnlineMusicApi.Internal.Infrastructure.Http;

namespace OnlineMusicApi.Internal.Providers.Bilibili.Protocol;

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

    public Task<BilibiliMediaPageDto> SearchMusicAsync(string keyword, int page)
    {
        return SearchVideoAsync(keyword, page);
    }

    public Task<BilibiliMediaPageDto> SearchAlbumAsync(string keyword, int page)
    {
        return SearchVideoAsync(keyword, page);
    }

    public async Task<BilibiliArtistPageDto> SearchArtistAsync(string keyword, int page)
    {
        BilibiliUserSearchResponseDto? response = await SearchBaseAsync(
                keyword,
                page,
                "bili_user",
                BilibiliJsonContext.Default.BilibiliUserSearchResponseDto
            )
            .ConfigureAwait(false);

        BilibiliUserSearchDataDto? data = response?.Data;

        return new BilibiliArtistPageDto
        {
            IsEnd = (data?.NumResults ?? 0) <= page * PageSize,
            Data = data?.Result ?? [],
        };
    }

    public async Task<BilibiliMediaSourceDto?> GetMediaSourceAsync(
        string key,
        string? cid,
        string quality
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        if (string.IsNullOrWhiteSpace(cid))
        {
            BilibiliViewResponseDto? view = await GetViewAsync(key).ConfigureAwait(false);
            cid = view?.Data?.Cid.ToString(CultureInfo.InvariantCulture);
        }

        if (string.IsNullOrWhiteSpace(cid))
        {
            return null;
        }

        QueryCollection query = new() { { "cid", cid }, { "fnval", "16" } };
        if (BilibiliSongMapper.IsAid(key))
        {
            query.Add("aid", key);
        }
        else
        {
            query.Add("bvid", key);
        }

        BilibiliPlayUrlResponseDto? response = await GetJsonAsync(
                "https://api.bilibili.com/x/player/playurl",
                query,
                CommonHeaders,
                BilibiliJsonContext.Default.BilibiliPlayUrlResponseDto
            )
            .ConfigureAwait(false);

        string? url = SelectAudioUrl(response?.Data, quality);
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        string host = new Uri(url).Host;
        Dictionary<string, string> headers = new()
        {
            ["user-agent"] = CommonHeaders[0].Value,
            ["accept"] = "*/*",
            ["host"] = host,
            ["accept-encoding"] = "gzip, deflate, br",
            ["connection"] = "keep-alive",
            ["referer"] = $"https://www.bilibili.com/video/{key}",
        };

        return new BilibiliMediaSourceDto { Url = url, Headers = headers };
    }

    public async Task<BilibiliAlbumSongsDto> GetAlbumSongsAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        BilibiliViewResponseDto? response = await GetViewAsync(key).ConfigureAwait(false);
        BilibiliViewDataDto? data = response?.Data;
        if (data is null)
        {
            return new BilibiliAlbumSongsDto { MusicList = [] };
        }

        List<BilibiliMediaDto> items = [];
        List<BilibiliPageDto> pages = data.Pages ?? [];

        if (pages.Count <= 1)
        {
            items.Add(
                new BilibiliMediaDto
                {
                    Aid = data.Aid,
                    Bvid = data.Bvid,
                    Cid = data.Cid,
                    Title = data.Title,
                    Owner = data.Owner,
                    Pic = data.Pic,
                    PubDate = data.PubDate,
                    DurationText = data.Duration.ToString(CultureInfo.InvariantCulture),
                }
            );
        }
        else
        {
            foreach (BilibiliPageDto page in pages)
            {
                items.Add(
                    new BilibiliMediaDto
                    {
                        Aid = data.Aid,
                        Bvid = data.Bvid,
                        Cid = page.Cid,
                        Title = page.Part,
                        Part = page.Part,
                        Page = page.Page,
                        Owner = data.Owner,
                        Pic = data.Pic,
                        PubDate = data.PubDate,
                        DurationText = page.Duration.ToString(CultureInfo.InvariantCulture),
                    }
                );
            }
        }

        return new BilibiliAlbumSongsDto { MusicList = items };
    }

    public async Task<BilibiliUserCardDto> GetUserCardAsync(string mid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mid);

        return await GetJsonAsync(
                "https://api.bilibili.com/x/web-interface/card",
                ToQueryCollection(new Dictionary<string, object?> { ["mid"] = mid }),
                CommonHeaders,
                BilibiliJsonContext.Default.BilibiliUserCardDto
            )
            .ConfigureAwait(false)
            ?? new BilibiliUserCardDto();
    }

    public async Task<BilibiliMediaPageDto> GetArtistWorksAsync(string mid, int page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mid);
        await EnsureCookieAsync().ConfigureAwait(false);

        string now = DateTimeOffset
            .UtcNow.ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);
        Dictionary<string, object?> baseParams = new()
        {
            ["mid"] = mid,
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

        string wRid = await GetRidAsync(baseParams).ConfigureAwait(false);
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
            { "referer", $"https://space.bilibili.com/{mid}/video" },
            { "cookie", Cookie() },
        };

        BilibiliSpaceResponseDto? response = await GetJsonAsync(
                "https://api.bilibili.com/x/space/wbi/arc/search",
                query,
                headers,
                BilibiliJsonContext.Default.BilibiliSpaceResponseDto
            )
            .ConfigureAwait(false);

        BilibiliSpaceDataDto? data = response?.Data;
        int pn = data?.Page?.PageNumber ?? 0;
        int ps = data?.Page?.PageSize ?? 0;
        int count = data?.Page?.Count ?? 0;

        return new BilibiliMediaPageDto
        {
            IsEnd = pn * ps >= count,
            Data = data?.List?.VList ?? [],
        };
    }

    public async Task<BilibiliViewResponseDto?> GetViewAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        QueryCollection query = [];
        if (BilibiliSongMapper.IsAid(key))
        {
            query.Add("aid", key);
        }
        else
        {
            query.Add("bvid", key);
        }

        return await GetJsonAsync(
                "https://api.bilibili.com/x/web-interface/view",
                query,
                CommonHeaders,
                BilibiliJsonContext.Default.BilibiliViewResponseDto
            )
            .ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }

    private async Task<BilibiliMediaPageDto> SearchVideoAsync(string keyword, int page)
    {
        BilibiliSearchVideoResponseDto? response = await SearchBaseAsync(
                keyword,
                page,
                "video",
                BilibiliJsonContext.Default.BilibiliSearchVideoResponseDto
            )
            .ConfigureAwait(false);

        BilibiliSearchVideoDataDto? data = response?.Data;

        return new BilibiliMediaPageDto
        {
            IsEnd = (data?.NumResults ?? 0) <= page * PageSize,
            Data = data?.Result ?? [],
        };
    }

    private async Task<T?> SearchBaseAsync<T>(
        string keyword,
        int page,
        string searchType,
        JsonTypeInfo<T> typeInfo
    )
        where T : class
    {
        await EnsureCookieAsync().ConfigureAwait(false);

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
        headers.Add("cookie", Cookie());

        return await GetJsonAsync(
                "https://api.bilibili.com/x/web-interface/search/type",
                query,
                headers,
                typeInfo
            )
            .ConfigureAwait(false);
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

        BilibiliFingerResponseDto? response = await GetJsonAsync(
                "https://api.bilibili.com/x/frontend/finger/spi",
                null,
                headers,
                BilibiliJsonContext.Default.BilibiliFingerResponseDto
            )
            .ConfigureAwait(false);

        _buvid3 = response?.Data?.Buvid3;
        _buvid4 = response?.Data?.Buvid4;
    }

    private async Task<(string Img, string Sub)> GetWbiKeysAsync()
    {
        if (
            !string.IsNullOrWhiteSpace(_imgKey)
            && !string.IsNullOrWhiteSpace(_subKey)
            && _wbiSyncDate.Date == DateTime.Today
        )
        {
            return (_imgKey, _subKey);
        }

        BilibiliTicketResponseDto ticket = await GetBiliTicketAsync().ConfigureAwait(false);
        BilibiliNavDto? nav = ticket.Data?.Nav;

        _imgKey = Path.GetFileNameWithoutExtension(nav?.Img ?? string.Empty);
        _subKey = Path.GetFileNameWithoutExtension(nav?.Sub ?? string.Empty);
        _wbiSyncDate = DateTime.Now;

        return (_imgKey, _subKey);
    }

    private async Task<BilibiliTicketResponseDto> GetBiliTicketAsync()
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

        return await SendForJsonAsync(
                HttpMethod.Post,
                "https://api.bilibili.com/bapis/bilibili.api.ticket.v1.Ticket/GenWebTicket",
                query,
                headers,
                null,
                BilibiliJsonContext.Default.BilibiliTicketResponseDto
            )
            .ConfigureAwait(false)
            ?? new BilibiliTicketResponseDto();
    }

    private async Task<string> GetRidAsync(Dictionary<string, object?> parameters)
    {
        (string img, string sub) = await GetWbiKeysAsync().ConfigureAwait(false);
        string mixin = GetMixinKey(img + sub);

        List<string> pairs = [];
        foreach (string key in parameters.Keys.OrderBy(static x => x, StringComparer.Ordinal))
        {
            object? value = parameters[key];
            if (value is null)
            {
                continue;
            }

            string text = RidFilterRegex().Replace(value.ToString() ?? string.Empty, string.Empty);
            pairs.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(text)}");
        }

        return Md5Hex(string.Join("&", pairs) + mixin);
    }

    private Task<T?> GetJsonAsync<T>(
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers,
        JsonTypeInfo<T> typeInfo
    )
        where T : class
    {
        return SendForJsonAsync(HttpMethod.Get, url, query, headers, null, typeInfo);
    }

    private async Task<T?> SendForJsonAsync<T>(
        HttpMethod method,
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers,
        string? content,
        JsonTypeInfo<T> typeInfo
    )
        where T : class
    {
        using HttpResponseMessage response = await _client
            .SendAsync(method, url, query, headers, content, "application/json")
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        string text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize(text, typeInfo);
    }

    private static string? SelectAudioUrl(BilibiliPlayUrlDataDto? data, string quality)
    {
        List<BilibiliAudioDto> audios = data?.Dash?.Audio is { } list
            ? [.. list.OrderBy(static item => item.Bandwidth)]
            : [];

        if (audios.Count > 0)
        {
            int index = quality switch
            {
                "low" => 0,
                "standard" => 1,
                "high" => 2,
                "super" => 3,
                _ => 1,
            };

            BilibiliAudioDto audio = audios[Math.Min(index, audios.Count - 1)];
            return audio.BaseUrl ?? audio.BaseUrlLower;
        }

        return data?.Durl?.FirstOrDefault()?.Url;
    }

    private string Cookie()
    {
        return $"buvid3={_buvid3};buvid4={_buvid4}";
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
}
