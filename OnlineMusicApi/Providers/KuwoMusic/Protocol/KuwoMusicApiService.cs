using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OnlineMusicApi.Internal.Contracts.KuwoMusic;
using OnlineMusicApi.Internal.Infrastructure.Http;

namespace OnlineMusicApi.Internal.Providers.KuwoMusic.Protocol;

internal sealed class KuwoMusicApiService : IDisposable
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

    public async Task<KuwoSongPageDto> SearchMusicAsync(string query, int page)
    {
        KuwoMusicSearchResponseDto? response = await SearchAsync(
                query,
                page,
                "music",
                KuwoJsonContext.Default.KuwoMusicSearchResponseDto
            )
            .ConfigureAwait(false);

        return new KuwoSongPageDto
        {
            IsEnd = IsEnd(response?.PageNumber, response?.PageSize, response?.Total, page),
            Data = response?.AbsList?.Where(IsPlayableSong).ToList() ?? [],
        };
    }

    public async Task<KuwoAlbumPageDto> SearchAlbumsAsync(string query, int page)
    {
        KuwoAlbumSearchResponseDto? response = await SearchAsync(
                query,
                page,
                "album",
                KuwoJsonContext.Default.KuwoAlbumSearchResponseDto
            )
            .ConfigureAwait(false);

        return new KuwoAlbumPageDto
        {
            IsEnd = IsEnd(response?.PageNumber, response?.PageSize, response?.Total, page),
            Data = response?.AlbumList ?? [],
        };
    }

    public async Task<KuwoArtistPageDto> SearchArtistsAsync(string query, int page)
    {
        KuwoArtistSearchResponseDto? response = await SearchAsync(
                query,
                page,
                "artist",
                KuwoJsonContext.Default.KuwoArtistSearchResponseDto
            )
            .ConfigureAwait(false);

        return new KuwoArtistPageDto
        {
            IsEnd = IsEnd(response?.PageNumber, response?.PageSize, response?.Total, page),
            Data = response?.AbsList ?? [],
        };
    }

    public async Task<KuwoPlaylistPageDto> SearchPlaylistsAsync(string query, int page)
    {
        KuwoPlaylistSearchResponseDto? response = await SearchAsync(
                query,
                page,
                "playlist",
                KuwoJsonContext.Default.KuwoPlaylistSearchResponseDto
            )
            .ConfigureAwait(false);

        return new KuwoPlaylistPageDto
        {
            IsEnd = IsEnd(response?.PageNumber, response?.PageSize, response?.Total, page),
            Data = response?.AbsList ?? [],
        };
    }

    public async Task<KuwoMediaDto?> GetMediaSourceAsync(string songId, string? quality)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(songId);

        QueryCollection query = new()
        {
            { "type", "convert_url3" },
            { "rid", songId },
            { "format", "mp3" },
        };

        KuwoMediaResponseDto? response = await GetJsonAsync(
                "https://antiserver.kuwo.cn/anti.s",
                query,
                KuwoJsonContext.Default.KuwoMediaResponseDto
            )
            .ConfigureAwait(false);

        return string.IsNullOrWhiteSpace(response?.Url)
            ? null
            : new KuwoMediaDto { Url = response.Url };
    }

    public async Task<string> GetArtistPageHtmlAsync(string artistId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);

        return await GetStringResponseAsync(
                HttpMethod.Get,
                $"https://www.kuwo.cn/singer_detail/{Uri.EscapeDataString(artistId)}",
                null
            )
            .ConfigureAwait(false);
    }

    public async Task<string> GetAlbumPageHtmlAsync(string albumId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);

        return await GetStringResponseAsync(
                HttpMethod.Get,
                $"https://www.kuwo.cn/album_detail/{Uri.EscapeDataString(albumId)}",
                null
            )
            .ConfigureAwait(false);
    }

    public async Task<KuwoLyricDto> GetLyricAsync(string songId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(songId);

        QueryCollection query = new() { { "musicId", songId }, { "httpStatus", "1" } };
        KuwoLyricResponseDto? response = await GetJsonAsync(
                "http://m.kuwo.cn/newh5/singles/songinfoandlrc",
                query,
                KuwoJsonContext.Default.KuwoLyricResponseDto
            )
            .ConfigureAwait(false);

        StringBuilder lines = new();
        foreach (KuwoLyricLineDto line in response?.Data?.LrcList ?? [])
        {
            if (lines.Length > 0)
            {
                lines.Append('\n');
            }

            lines
                .Append('[')
                .Append(line.Time ?? string.Empty)
                .Append(']')
                .Append(line.LineLyric ?? string.Empty);
        }

        return new KuwoLyricDto { RawLrc = lines.ToString() };
    }

    public async Task<KuwoAlbumSongsDto> GetAlbumInfoAsync(string albumId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);

        QueryCollection query = BuildAlbumInfoQuery(albumId);
        KuwoAlbumInfoResponseDto? response = await GetJsonAsync(
                "http://search.kuwo.cn/r.s",
                query,
                KuwoJsonContext.Default.KuwoAlbumInfoResponseDto
            )
            .ConfigureAwait(false);

        return new KuwoAlbumSongsDto
        {
            MusicList = response?.MusicList?.Where(IsPlayableSong).ToList() ?? [],
        };
    }

    public async Task<List<KuwoSongDto>> GetMusicSheetSongsAsync(string sheetId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetId);

        List<KuwoSongDto> songs = [];
        int page = 1;
        int totalPage = 30;

        while (page < totalPage)
        {
            try
            {
                KuwoPlaylistResponseDto? data = await GetMusicSheetResponseByIdAsync(
                        sheetId,
                        page,
                        80
                    )
                    .ConfigureAwait(false);

                int total = ParseInt(data?.Total);
                totalPage = Math.Max(1, (int)Math.Ceiling(total / 80d));
                songs.AddRange(
                    (data?.MusicList ?? data?.MusicListLower ?? []).Where(IsPlayableSong)
                );
            }
            catch
            {
                // 与原实现一致：单页失败时忽略并继续。
            }

            await Task.Delay(Random.Shared.Next(200, 301)).ConfigureAwait(false);
            page++;
        }

        return songs;
    }

    public async Task<KuwoAlbumPageDto> GetArtistAlbumsAsync(string artistId, int page)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);

        QueryCollection query = BuildArtistAlbumQuery(artistId, page);
        KuwoArtistAlbumResponseDto? response = await GetJsonAsync(
                "http://search.kuwo.cn/r.s",
                query,
                KuwoJsonContext.Default.KuwoArtistAlbumResponseDto
            )
            .ConfigureAwait(false);

        return new KuwoAlbumPageDto
        {
            IsEnd = IsEnd(
                response?.PageNumber,
                PageSize.ToString(CultureInfo.InvariantCulture),
                response?.Total,
                page
            ),
            Data = response?.AlbumList ?? [],
        };
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _client.Dispose();
        }
    }

    private async Task<T?> SearchAsync<T>(
        string query,
        int page,
        string searchType,
        JsonTypeInfo<T> typeInfo
    )
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        return await GetJsonAsync(
                "http://search.kuwo.cn/r.s",
                BuildSearchQuery(query, page, searchType),
                typeInfo
            )
            .ConfigureAwait(false);
    }

    private async Task<KuwoPlaylistResponseDto?> GetMusicSheetResponseByIdAsync(
        string id,
        int page,
        int pageSize
    )
    {
        QueryCollection query = new()
        {
            { "op", "getlistinfo" },
            { "pid", id },
            { "pn", (page - 1).ToString(CultureInfo.InvariantCulture) },
            { "rn", pageSize.ToString(CultureInfo.InvariantCulture) },
            { "encode", "utf8" },
            { "keyset", "pl2012" },
            { "vipver", "MUSIC_9.1.1.2_BCS2" },
            { "newver", "1" },
        };

        return await GetJsonAsync(
                "http://nplserver.kuwo.cn/pl.svc",
                query,
                KuwoJsonContext.Default.KuwoPlaylistResponseDto
            )
            .ConfigureAwait(false);
    }

    private async Task<T?> GetJsonAsync<T>(
        string url,
        QueryCollection? query,
        JsonTypeInfo<T> typeInfo
    )
        where T : class
    {
        string text = await GetStringResponseAsync(HttpMethod.Get, url, query)
            .ConfigureAwait(false);
        string json = ExtractJson(text);
        return JsonSerializer.Deserialize(json, typeInfo);
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

        using HttpResponseMessage response = await _client
            .SendAsync(method, url, query, headers, content: (string?)null, contentType: null)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private static QueryCollection BuildSearchQuery(string query, int page, string searchType)
    {
        return new QueryCollection
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
        };
    }

    private static QueryCollection BuildAlbumInfoQuery(string albumId)
    {
        return new QueryCollection
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
        };
    }

    private static QueryCollection BuildArtistAlbumQuery(string artistId, int page)
    {
        return new QueryCollection
        {
            { "pn", (page - 1).ToString(CultureInfo.InvariantCulture) },
            { "rn", PageSize.ToString(CultureInfo.InvariantCulture) },
            { "artistid", artistId },
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
        };
    }

    private static string ExtractJson(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : text;
    }

    private static bool IsPlayableSong(KuwoSongDto song)
    {
        return !string.Equals(song.PayInfo?.ListenFragment, "1", StringComparison.Ordinal);
    }

    private static bool IsEnd(
        string? pageNumber,
        string? pageSize,
        string? total,
        int requestedPage
    )
    {
        int pn = ParseInt(pageNumber);
        int rn = ParseInt(pageSize);
        int count = ParseInt(total);

        if (count <= 0)
        {
            return true;
        }

        return (pn + 1) * rn >= count;
    }

    private static int ParseInt(string? value)
    {
        return int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int number
        )
            ? number
            : 0;
    }

    private static string ChooseUserAgent()
    {
        return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36";
    }
}
