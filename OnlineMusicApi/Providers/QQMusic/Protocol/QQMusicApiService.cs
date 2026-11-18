using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using OnlineMusicApi.Internal.Contracts.QQMusic;
using OnlineMusicApi.Internal.Infrastructure.Http;

namespace OnlineMusicApi.Internal.Providers.QQMusic.Protocol;

internal sealed partial class QQMusicApiService : IDisposable
{
    private const int PageSize = 20;

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

    public Task<QQSongPageDto> SearchSongsAsync(string query, int page)
    {
        return SearchSongsCoreAsync(query, page);
    }

    public async Task<QQAlbumPageDto> SearchAlbumsAsync(string query, int page)
    {
        QQMusicuSearchResponseDto response = await SearchMusicuAsync(query, page, 2)
            .ConfigureAwait(false);
        QQMusicuSearchDataDto? data = response.Req1;

        return new QQAlbumPageDto
        {
            IsEnd = (data?.Meta?.Sum ?? 0) <= page * PageSize,
            Data = data?.Body?.Album?.List ?? [],
        };
    }

    public async Task<QQArtistPageDto> SearchArtistsAsync(string query, int page)
    {
        QQMusicuSearchResponseDto response = await SearchMusicuAsync(query, page, 1)
            .ConfigureAwait(false);
        QQMusicuSearchDataDto? data = response.Req1;

        return new QQArtistPageDto
        {
            IsEnd = (data?.Meta?.Sum ?? 0) <= page * PageSize,
            Data = data?.Body?.Singer?.List ?? [],
        };
    }

    public async Task<QQPlaylistPageDto> SearchPlaylistsAsync(string query, int page)
    {
        QQMusicuSearchResponseDto response = await SearchMusicuAsync(query, page, 3)
            .ConfigureAwait(false);
        QQMusicuSearchDataDto? data = response.Req1;

        return new QQPlaylistPageDto
        {
            IsEnd = (data?.Meta?.Sum ?? 0) <= page * PageSize,
            Data = data?.Body?.Playlist?.List ?? [],
        };
    }

    public async Task<QQMediaDto?> GetMediaSourceAsync(string songMid, string? quality = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(songMid);

        QQVKeyResponseDto? response = await GetVKeyAsync(songMid).ConfigureAwait(false);
        string? sip = response?.Req0?.Data?.Sip?.FirstOrDefault();
        string? purl = response?.Req0?.Data?.MidUrlInfo?.FirstOrDefault()?.Purl;

        return string.IsNullOrEmpty(purl) ? null : new QQMediaDto { Url = sip + purl };
    }

    public async Task<QQAlbumSongsDto> GetAlbumSongsAsync(string albumMid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumMid);

        QQAlbumSongListRequestDto request = new()
        {
            Comm = new QQCommRequestDto { Ct = 24, Cv = 10000 },
            AlbumSonglist = new QQAlbumSongListRequestItemDto
            {
                Param = new QQAlbumSongListParamDto
                {
                    AlbumMid = albumMid,
                    Num = 999,
                    Order = 2,
                },
            },
        };

        string json = JsonSerializer.Serialize(
            request,
            QQJsonContext.Default.QQAlbumSongListRequestDto
        );

        string url = ChangeUrlQuery(
            new Dictionary<string, string> { ["data"] = json },
            "https://u.y.qq.com/cgi-bin/musicu.fcg?g_tk=5381&format=json&inCharset=utf8&outCharset=utf-8"
        );

        QQAlbumSongListResponseDto? response = await GetJsonAsync(
                url,
                QQJsonContext.Default.QQAlbumSongListResponseDto
            )
            .ConfigureAwait(false);

        List<QQSongDto> songs =
        [
            .. (response?.AlbumSonglist?.Data?.SongList ?? [])
                .Where(static entry => entry.SongInfo is not null)
                .Select(static entry => entry.SongInfo!),
        ];

        return new QQAlbumSongsDto { MusicList = songs };
    }

    public async Task<QQLyricDto> GetLyricAsync(string songMid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(songMid);

        string url =
            $"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={Uri.EscapeDataString(songMid)}&pcachetime={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&g_tk=5381&loginUin=0&hostUin=0&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0";

        QQLyricResponseDto? response = await GetJsonpAsync(
                url,
                QQJsonContext.Default.QQLyricResponseDto
            )
            .ConfigureAwait(false);

        return new QQLyricDto
        {
            RawLrc = DecodeLyricBase64(response?.Lyric) ?? string.Empty,
            Translation = DecodeLyricBase64(response?.Trans),
        };
    }

    public async Task<List<QQSongDto>> GetMusicSheetSongsAsync(string sheetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetId);

        string url =
            $"https://i.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&utf8=1&disstid={Uri.EscapeDataString(sheetId)}&loginUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0";

        QQPlaylistResponseDto? response = await GetJsonpAsync(
                url,
                QQJsonContext.Default.QQPlaylistResponseDto,
                referer: "https://y.qq.com/n/yqq/playlist"
            )
            .ConfigureAwait(false);

        return response?.Cdlist?.FirstOrDefault()?.Songlist ?? [];
    }

    public async Task<QQAlbumPageDto> GetArtistAlbumsAsync(string singerMid, int page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(singerMid);

        QQSingerAlbumRequestDto request = new()
        {
            Comm = new QQCommRequestDto { Ct = 24, Cv = 0 },
            SingerAlbum = new QQSingerAlbumRequestItemDto
            {
                Param = new QQSingerAlbumParamDto
                {
                    SingerMid = singerMid,
                    Begin = (page - 1) * PageSize,
                    Num = PageSize,
                    ExStatus = 1,
                },
            },
        };

        string json = JsonSerializer.Serialize(
            request,
            QQJsonContext.Default.QQSingerAlbumRequestDto
        );

        string url = ChangeUrlQuery(
            new Dictionary<string, string> { ["data"] = json },
            "https://u.y.qq.com/cgi-bin/musicu.fcg"
        );

        QQSingerAlbumResponseDto? response = await GetJsonAsync(
                url,
                QQJsonContext.Default.QQSingerAlbumResponseDto
            )
            .ConfigureAwait(false);
        QQSingerAlbumDataDto? data = response?.SingerAlbum?.Data;

        return new QQAlbumPageDto
        {
            IsEnd = (data?.Total ?? 0) <= page * PageSize,
            Data = data?.List ?? [],
        };
    }

    public async Task<QQSingerPageDto?> GetArtistPageAsync(string artistMid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistMid);

        string html = await GetStringResponseAsync(
                HttpMethod.Get,
                $"https://y.qq.com/n/ryqq_v2/singer/{Uri.EscapeDataString(artistMid)}",
                null,
                referer: "https://y.qq.com/"
            )
            .ConfigureAwait(false);

        return ExtractInitialData(html, QQJsonContext.Default.QQSingerPageDto);
    }

    public async Task<QQAlbumDetailPageDto?> GetAlbumPageAsync(string albumMid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumMid);

        string html = await GetStringResponseAsync(
                HttpMethod.Get,
                $"https://y.qq.com/n/ryqq_v2/albumDetail/{Uri.EscapeDataString(albumMid)}",
                null,
                referer: "https://y.qq.com/"
            )
            .ConfigureAwait(false);

        return ExtractInitialData(html, QQJsonContext.Default.QQAlbumDetailPageDto);
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }

    private async Task<QQSongPageDto> SearchSongsCoreAsync(string query, int page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        string url =
            $"https://c.y.qq.com/soso/fcgi-bin/client_search_cp?p={page.ToString(CultureInfo.InvariantCulture)}&n={PageSize}&w={Uri.EscapeDataString(query)}&format=json&cr=1&new_json=1";

        QQSearchSongResponseDto? response = await GetJsonAsync(
                url,
                QQJsonContext.Default.QQSearchSongResponseDto
            )
            .ConfigureAwait(false);

        return new QQSongPageDto
        {
            IsEnd = (response?.Data?.Song?.TotalNum ?? 0) <= page * PageSize,
            Data = response?.Data?.Song?.List ?? [],
        };
    }

    private async Task<QQMusicuSearchResponseDto> SearchMusicuAsync(
        string query,
        int page,
        int searchType
    )
    {
        QQMusicuSearchRequestDto request = new()
        {
            Req1 = new QQMusicuSearchRequestItemDto
            {
                Param = new QQMusicuSearchParamDto
                {
                    NumPerPage = PageSize,
                    PageNum = page,
                    Query = query,
                    SearchType = searchType,
                },
            },
        };

        string json = JsonSerializer.Serialize(
            request,
            QQJsonContext.Default.QQMusicuSearchRequestDto
        );

        QQMusicuSearchResponseDto? response = await PostJsonAsync(
                "https://u.y.qq.com/cgi-bin/musicu.fcg",
                json,
                QQJsonContext.Default.QQMusicuSearchResponseDto
            )
            .ConfigureAwait(false);

        return response ?? new QQMusicuSearchResponseDto();
    }

    private async Task<QQVKeyResponseDto?> GetVKeyAsync(string songMid)
    {
        QQVKeyRequestDto request = new()
        {
            Req0 = new QQVKeyRequestItemDto
            {
                Param = new QQVKeyParamDto
                {
                    Guid = Random.Shared.Next(1, 10_000_000).ToString(CultureInfo.InvariantCulture),
                    SongMid = [songMid],
                },
            },
            Comm = new QQCommRequestDto
            {
                Uin = "0",
                Format = "json",
                Ct = 24,
                Cv = 0,
            },
        };

        string json = JsonSerializer.Serialize(request, QQJsonContext.Default.QQVKeyRequestDto);

        string url = $"https://u.y.qq.com/cgi-bin/musicu.fcg?data={Uri.EscapeDataString(json)}";

        return await GetJsonAsync(url, QQJsonContext.Default.QQVKeyResponseDto)
            .ConfigureAwait(false);
    }

    private async Task<T?> GetJsonAsync<T>(
        string url,
        JsonTypeInfo<T> typeInfo,
        string referer = "https://y.qq.com"
    )
        where T : class
    {
        string response = await GetStringResponseAsync(HttpMethod.Get, url, null, referer: referer)
            .ConfigureAwait(false);
        return JsonSerializer.Deserialize(response, typeInfo);
    }

    private async Task<T?> PostJsonAsync<T>(
        string url,
        string json,
        JsonTypeInfo<T> typeInfo,
        string referer = "https://y.qq.com"
    )
        where T : class
    {
        string response = await GetStringResponseAsync(
                HttpMethod.Post,
                url,
                null,
                json,
                "application/json",
                referer
            )
            .ConfigureAwait(false);
        return JsonSerializer.Deserialize(response, typeInfo);
    }

    private async Task<T?> GetJsonpAsync<T>(
        string url,
        JsonTypeInfo<T> typeInfo,
        string referer = "https://y.qq.com"
    )
        where T : class
    {
        string response = await GetStringResponseAsync(HttpMethod.Get, url, null, referer: referer)
            .ConfigureAwait(false);
        string? json = ParseJsonp(response);
        return json is null ? null : JsonSerializer.Deserialize(json, typeInfo);
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

        using HttpResponseMessage response = await _client
            .SendAsync(method, url, query, headers, content, contentType)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private static string? ParseJsonp(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return JsonpWrapperRegex().Replace(raw, string.Empty);
    }

    private static string? DecodeLyricBase64(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        catch (FormatException)
        {
            return null;
        }
    }

    private static T? ExtractInitialData<T>(string html, JsonTypeInfo<T> typeInfo)
        where T : class
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
        return JsonSerializer.Deserialize(json, typeInfo);
    }

    private static string ChangeUrlQuery(Dictionary<string, string> query, string url)
    {
        if (query.Count == 0)
        {
            return url;
        }

        StringBuilder builder = new(url);
        builder.Append(url.Contains('?', StringComparison.Ordinal) ? '&' : '?');

        bool first = true;
        foreach ((string key, string value) in query)
        {
            if (!first)
            {
                builder.Append('&');
            }

            first = false;
            builder
                .Append(Uri.EscapeDataString(key))
                .Append('=')
                .Append(Uri.EscapeDataString(value));
        }

        return builder.ToString();
    }

    private static string ChooseUserAgent() =>
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36";
}
