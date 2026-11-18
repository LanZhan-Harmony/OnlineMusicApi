using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OnlineMusicApi.Internal.Contracts.FiveSing;
using OnlineMusicApi.Internal.Infrastructure.Http;

namespace OnlineMusicApi.Internal.Providers.FiveSing.Protocol;

internal sealed class FiveSingMusicApiService : IDisposable
{
    private const int PageSize = 10;

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

    public Task<FiveSingSongPageDto> SearchMusicAsync(string query, int page)
    {
        return SearchSongsAsync(query, page);
    }

    public Task<FiveSingAlbumPageDto> SearchAlbumAsync(string query, int page)
    {
        return SearchAlbumsAsync(query, page);
    }

    public Task<FiveSingArtistPageDto> SearchArtistAsync(string query, int page)
    {
        return SearchArtistsAsync(query, page);
    }

    public async Task<FiveSingLyricDto> GetLyricAsync(string songId, string typeEname)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(songId);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeEname);

        QueryCollection query = new() { { "songId", songId }, { "songType", typeEname } };
        FiveSingLyricResponseDto? response = await GetJsonAsync(
                "http://5sing.kugou.com/fm/m/json/lrc",
                query,
                null,
                FiveSingJsonContext.Default.FiveSingLyricResponseDto
            )
            .ConfigureAwait(false);

        return new FiveSingLyricDto { RawLrc = response?.Text ?? string.Empty };
    }

    public async Task<FiveSingAlbumSongsDto> GetAlbumInfoAsync(string albumId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);

        QueryCollection query = new() { { "id", albumId } };
        FiveSingAlbumSongsResponseDto? response = await GetJsonAsync(
                "http://service.5sing.kugou.com/song/getPlayListSong",
                query,
                BuildMobileHeaders(),
                FiveSingJsonContext.Default.FiveSingAlbumSongsResponseDto
            )
            .ConfigureAwait(false);

        return new FiveSingAlbumSongsDto { MusicList = response?.Data ?? [] };
    }

    public async Task<FiveSingMediaDto?> GetMediaSourceAsync(
        string songId,
        string typeEname,
        string quality
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(songId);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeEname);

        FiveSingSongUrlResponseDto? response = await GetSongUrlAsync(songId, typeEname)
            .ConfigureAwait(false);
        FiveSingSongUrlDataDto? data = response?.Data;
        if (data is null)
        {
            return null;
        }

        string? url = quality switch
        {
            "low" => First(data.LqUrl, data.LqUrlBackup),
            "standard" => First(data.SqUrl, data.SqUrlBackup),
            "high" or "super" => First(data.HqUrl, data.HqUrlBackup),
            _ => First(data.HqUrl, data.HqUrlBackup),
        };

        url ??= First(data.SqUrl, data.SqUrlBackup);
        url ??= First(data.LqUrl, data.LqUrlBackup);

        return string.IsNullOrWhiteSpace(url) ? null : new FiveSingMediaDto { Url = url };
    }

    public async Task<string> GetArtistPageHtmlAsync(string artistId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(artistId);

        return await GetStringAsync(
                $"https://5sing.kugou.com/{Uri.EscapeDataString(artistId)}/default.html",
                null,
                null
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

    private async Task<FiveSingSongPageDto> SearchSongsAsync(string query, int page)
    {
        FiveSingSearchSongResponseDto? response = await SearchAsync(
                query,
                page,
                0,
                0,
                FiveSingJsonContext.Default.FiveSingSearchSongResponseDto
            )
            .ConfigureAwait(false);
        FiveSingPageInfoDto? info = response?.PageInfo;

        return new FiveSingSongPageDto
        {
            IsEnd = (info?.Current ?? 0) >= (info?.TotalPages ?? 0),
            Data = response?.List ?? [],
        };
    }

    private async Task<FiveSingAlbumPageDto> SearchAlbumsAsync(string query, int page)
    {
        FiveSingSearchAlbumResponseDto? response = await SearchAsync(
                query,
                page,
                1,
                0,
                FiveSingJsonContext.Default.FiveSingSearchAlbumResponseDto
            )
            .ConfigureAwait(false);
        FiveSingPageInfoDto? info = response?.PageInfo;

        return new FiveSingAlbumPageDto
        {
            IsEnd = (info?.Current ?? 0) >= (info?.TotalPages ?? 0),
            Data = response?.List ?? [],
        };
    }

    private async Task<FiveSingArtistPageDto> SearchArtistsAsync(string query, int page)
    {
        FiveSingSearchArtistResponseDto? response = await SearchAsync(
                query,
                page,
                2,
                1,
                FiveSingJsonContext.Default.FiveSingSearchArtistResponseDto
            )
            .ConfigureAwait(false);
        FiveSingPageInfoDto? info = response?.PageInfo;

        return new FiveSingArtistPageDto
        {
            IsEnd = (info?.Current ?? 0) >= (info?.TotalPages ?? 0),
            Data = response?.List ?? [],
        };
    }

    private async Task<T?> SearchAsync<T>(
        string query,
        int page,
        int type,
        int filter,
        JsonTypeInfo<T> typeInfo
    )
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        QueryCollection queryParams = new()
        {
            { "keyword", query },
            { "sort", "1" },
            { "page", page.ToString(CultureInfo.InvariantCulture) },
            { "filter", filter.ToString(CultureInfo.InvariantCulture) },
            { "type", type.ToString(CultureInfo.InvariantCulture) },
        };

        return await GetJsonAsync(
                "http://search.5sing.kugou.com/home/json",
                queryParams,
                BuildSearchHeaders(),
                typeInfo
            )
            .ConfigureAwait(false);
    }

    private async Task<FiveSingSongUrlResponseDto?> GetSongUrlAsync(string songId, string songType)
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
                BuildSongUrlHeaders(),
                FiveSingJsonContext.Default.FiveSingSongUrlResponseDto
            )
            .ConfigureAwait(false);
    }

    private async Task<T?> GetJsonAsync<T>(
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers,
        JsonTypeInfo<T> typeInfo
    )
        where T : class
    {
        string text = await GetStringAsync(url, query, headers).ConfigureAwait(false);
        return JsonSerializer.Deserialize(text, typeInfo);
    }

    private async Task<string> GetStringAsync(
        string url,
        IEnumerable<KeyValuePair<string, string>>? query,
        IEnumerable<KeyValuePair<string, string>>? headers
    )
    {
        using HttpResponseMessage response = await _client
            .SendAsync(
                HttpMethod.Get,
                url,
                query,
                headers,
                content: (string?)null,
                contentType: null
            )
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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

    private static string? First(string? primary, string? backup)
    {
        return !string.IsNullOrWhiteSpace(primary) ? primary : backup;
    }
}
