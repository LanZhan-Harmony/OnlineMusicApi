using OnlineMusicApi;

using OnlineMusicApiClient client = new(
    new OnlineMusicApiOptions { EnablePlaybackProbe = true, GreedyPageFill = true }
);

Console.WriteLine($"已注册音源：{string.Join(", ", client.Providers.SupportedSources)}");

await DemoCloudAsync(client);
await DemoQQAsync(client);
await DemoKuwoAsync(client);
await DemoBilibiliAsync(client);
await DemoFiveSingAsync(client);
await DemoDetailsAsync(client);

static async Task DemoCloudAsync(OnlineMusicApiClient client)
{
    Console.WriteLine();
    Console.WriteLine("[网易云]");
    Console.WriteLine($"能力：{client.Providers.GetCapabilities(MusicSource.CloudMusic)}");

    try
    {
        SongPage page = await client.SearchSongsAsync(
            MusicSource.CloudMusic,
            new SearchQuery
            {
                Keyword = "晴天",
                Limit = 10,
                Offset = 0,
            }
        );

        Console.WriteLine(
            $"本页 {page.Total} 首，HasMore={page.HasMore}，NextOffset={page.NextOffset}"
        );
        if (page.Warning is not null)
        {
            Console.WriteLine($"提示：{page.Warning}");
        }

        foreach (Song song in page.Items)
        {
            string artists = string.Join(" / ", song.Artists.Select(static artist => artist.Name));
            Console.WriteLine($"{song.Title} - {artists} ({song.Album?.Title}, {song.Year})");
        }

        if (page.Items.Count > 0)
        {
            MediaSource media = await client.ResolveMediaAsync(page.Items[0].Id, AudioQuality.High);
            Console.WriteLine($"播放地址：{media.Url}（{media.ActualQuality}）");
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"网易云调用失败：{exception.GetType().Name} | {exception.Message}");
    }
}

static async Task DemoQQAsync(OnlineMusicApiClient client)
{
    Console.WriteLine();
    Console.WriteLine("[QQ音乐]");
    Console.WriteLine($"能力：{client.Providers.GetCapabilities(MusicSource.QQMusic)}");

    try
    {
        SongPage page = await client.SearchSongsAsync(
            MusicSource.QQMusic,
            new SearchQuery
            {
                Keyword = "晴天",
                Limit = 10,
                Offset = 0,
            }
        );

        Console.WriteLine(
            $"本页 {page.Total} 首，HasMore={page.HasMore}，NextOffset={page.NextOffset}"
        );
        if (page.Warning is not null)
        {
            Console.WriteLine($"提示：{page.Warning}");
        }

        foreach (Song song in page.Items)
        {
            string artists = string.Join(" / ", song.Artists.Select(static artist => artist.Name));
            Console.WriteLine($"{song.Title} - {artists} ({song.Album?.Title}, {song.Year})");
        }

        if (page.Items.Count > 0)
        {
            MediaSource media = await client.ResolveMediaAsync(page.Items[0].Id, AudioQuality.High);
            Console.WriteLine($"播放地址：{media.Url}");
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"QQ 调用失败：{exception.GetType().Name} | {exception.Message}");
    }
}

static async Task DemoKuwoAsync(OnlineMusicApiClient client)
{
    Console.WriteLine();
    Console.WriteLine("[酷我音乐]");
    Console.WriteLine($"能力：{client.Providers.GetCapabilities(MusicSource.KuwoMusic)}");

    try
    {
        SongPage page = await client.SearchSongsAsync(
            MusicSource.KuwoMusic,
            new SearchQuery
            {
                Keyword = "晴天",
                Limit = 10,
                Offset = 0,
            }
        );

        Console.WriteLine(
            $"本页 {page.Total} 首，HasMore={page.HasMore}，NextOffset={page.NextOffset}"
        );
        if (page.Warning is not null)
        {
            Console.WriteLine($"提示：{page.Warning}");
        }

        foreach (Song song in page.Items)
        {
            string artists = string.Join(" / ", song.Artists.Select(static artist => artist.Name));
            Console.WriteLine($"{song.Title} - {artists} ({song.Album?.Title}, {song.Year})");
        }

        if (page.Items.Count > 0)
        {
            MediaSource media = await client.ResolveMediaAsync(page.Items[0].Id, AudioQuality.High);
            Console.WriteLine($"播放地址：{media.Url}");
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"酷我调用失败：{exception.GetType().Name} | {exception.Message}");
    }
}

static async Task DemoBilibiliAsync(OnlineMusicApiClient client)
{
    Console.WriteLine();
    Console.WriteLine("[哔哩哔哩]");
    Console.WriteLine($"能力：{client.Providers.GetCapabilities(MusicSource.Bilibili)}");

    try
    {
        SongPage page = await client.SearchSongsAsync(
            MusicSource.Bilibili,
            new SearchQuery
            {
                Keyword = "晴天",
                Limit = 10,
                Offset = 0,
            }
        );

        Console.WriteLine(
            $"本页 {page.Total} 首，HasMore={page.HasMore}，NextOffset={page.NextOffset}"
        );
        if (page.Warning is not null)
        {
            Console.WriteLine($"提示：{page.Warning}");
        }

        foreach (Song song in page.Items)
        {
            string artists = string.Join(" / ", song.Artists.Select(static artist => artist.Name));
            Console.WriteLine($"{song.Title} - {artists} ({song.Album?.Title}, {song.Year})");
        }

        if (page.Items.Count > 0)
        {
            MediaSource media = await client.ResolveMediaAsync(page.Items[0].Id, AudioQuality.High);
            Console.WriteLine($"播放地址：{media.Url}");
            Console.WriteLine(
                $"播放请求头：{string.Join("; ", media.Headers.Select(pair => $"{pair.Key}={pair.Value}"))}"
            );
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"Bilibili 调用失败：{exception.GetType().Name} | {exception.Message}");
    }
}

static async Task DemoFiveSingAsync(OnlineMusicApiClient client)
{
    Console.WriteLine();
    Console.WriteLine("[5sing]");
    Console.WriteLine($"能力：{client.Providers.GetCapabilities(MusicSource.FiveSing)}");

    try
    {
        SongPage page = await client.SearchSongsAsync(
            MusicSource.FiveSing,
            new SearchQuery
            {
                Keyword = "晴天",
                Limit = 10,
                Offset = 0,
            }
        );

        Console.WriteLine(
            $"本页 {page.Total} 首，HasMore={page.HasMore}，NextOffset={page.NextOffset}"
        );
        if (page.Warning is not null)
        {
            Console.WriteLine($"提示：{page.Warning}");
        }

        foreach (Song song in page.Items)
        {
            string artists = string.Join(" / ", song.Artists.Select(static artist => artist.Name));
            Console.WriteLine($"{song.Title} - {artists} ({song.Album?.Title}, {song.Year})");
        }

        if (page.Items.Count > 0)
        {
            try
            {
                MediaSource media = await client.ResolveMediaAsync(
                    page.Items[0].Id,
                    AudioQuality.High
                );
                Console.WriteLine($"播放地址：{media.Url}");
            }
            catch (OnlineMusicApiException exception)
            {
                Console.WriteLine($"播放失败：{exception.GetType().Name} | {exception.Message}");
            }
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine($"5sing 调用失败：{exception.GetType().Name} | {exception.Message}");
    }
}

static async Task DemoDetailsAsync(OnlineMusicApiClient client)
{
    Console.WriteLine();
    Console.WriteLine("[详情验证]");

    await DemoArtistAsync(client, MusicSource.QQMusic, "周杰伦");
    await DemoArtistAsync(client, MusicSource.KuwoMusic, "黄诗扶");
    await DemoArtistAsync(client, MusicSource.FiveSing, "心然");
    await DemoArtistAsync(client, MusicSource.Bilibili, "周杰伦");

    await DemoAlbumAsync(client, MusicSource.QQMusic, "人间不值得");
    await DemoAlbumAsync(client, MusicSource.KuwoMusic, "今日无事");
}

static async Task DemoArtistAsync(OnlineMusicApiClient client, MusicSource source, string keyword)
{
    try
    {
        ArtistPage page = await client.SearchArtistsAsync(
            source,
            new SearchQuery { Keyword = keyword, Limit = 1 }
        );

        if (page.Items.Count == 0)
        {
            Console.WriteLine($"{source} 艺术家搜索无结果");
            return;
        }

        Artist artist = await client.GetArtistAsync(page.Items[0].Id);
        Console.WriteLine(
            $"{source} Artist: {artist.Name} | 专辑={artist.AlbumCount} | 歌曲={artist.SongCount} | 头像={(artist.AvatarUrl.Length > 0)} | 简介长度={artist.Description.Length}"
        );
    }
    catch (Exception exception)
    {
        Console.WriteLine(
            $"{source} Artist 失败：{exception.GetType().Name} | {exception.Message}"
        );
    }
}

static async Task DemoAlbumAsync(OnlineMusicApiClient client, MusicSource source, string keyword)
{
    try
    {
        AlbumPage page = await client.SearchAlbumsAsync(
            source,
            new SearchQuery { Keyword = keyword, Limit = 1 }
        );

        if (page.Items.Count == 0)
        {
            Console.WriteLine($"{source} 专辑搜索无结果");
            return;
        }

        Album album = await client.GetAlbumAsync(page.Items[0].Id);
        string artists = string.Join(" / ", album.Artists.Select(static artist => artist.Name));
        Console.WriteLine(
            $"{source} Album: {album.Title} | {artists} | 年份={album.Year} | 歌曲数={album.SongCount} | 封面={(album.CoverUrl.Length > 0)} | 简介长度={album.Description.Length}"
        );
    }
    catch (Exception exception)
    {
        Console.WriteLine($"{source} Album 失败：{exception.GetType().Name} | {exception.Message}");
    }
}
