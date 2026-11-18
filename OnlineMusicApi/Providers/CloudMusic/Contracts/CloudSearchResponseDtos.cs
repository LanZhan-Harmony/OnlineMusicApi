using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.CloudMusic;

internal sealed class CloudSearchSongsResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public CloudSearchSongsResult? Result { get; set; }
}

internal sealed class CloudSearchSongsResult
{
    [JsonPropertyName("songCount")]
    public int SongCount { get; set; }

    [JsonPropertyName("songs")]
    public List<CloudSearchSongDto>? Songs { get; set; }
}

internal sealed class CloudSearchSongDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("album")]
    public CloudSearchAlbumReferenceDto? Album { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchArtistReferenceDto>? Artists { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}

internal sealed class CloudSearchAlbumsResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public CloudSearchAlbumsResult? Result { get; set; }
}

internal sealed class CloudSearchAlbumsResult
{
    [JsonPropertyName("albumCount")]
    public int AlbumCount { get; set; }

    [JsonPropertyName("albums")]
    public List<CloudSearchAlbumDto>? Albums { get; set; }
}

internal sealed class CloudSearchAlbumDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchArtistReferenceDto>? Artists { get; set; }

    [JsonPropertyName("publishTime")]
    public long PublishTime { get; set; }
}

internal sealed class CloudSearchAlbumReferenceDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("publishTime")]
    public long PublishTime { get; set; }

    [JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }
}

internal sealed class CloudSearchArtistReferenceDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal sealed class CloudSearchArtistsResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public CloudSearchArtistsResult? Result { get; set; }
}

internal sealed class CloudSearchArtistsResult
{
    [JsonPropertyName("artistCount")]
    public int ArtistCount { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchArtistDto>? Artists { get; set; }
}

internal sealed class CloudSearchArtistDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }
}

internal sealed class CloudSearchPlaylistsResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public CloudSearchPlaylistsResult? Result { get; set; }
}

internal sealed class CloudSearchPlaylistsResult
{
    [JsonPropertyName("playlistCount")]
    public int PlaylistCount { get; set; }

    [JsonPropertyName("playlists")]
    public List<CloudSearchPlaylistDto>? Playlists { get; set; }
}

internal sealed class CloudSearchPlaylistDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("coverImgUrl")]
    public string? CoverImgUrl { get; set; }

    [JsonPropertyName("trackCount")]
    public int TrackCount { get; set; }
}

internal sealed class CloudSearchSuggestResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("result")]
    public CloudSearchSuggestResult? Result { get; set; }
}

internal sealed class CloudSearchSuggestResult
{
    [JsonPropertyName("songs")]
    public List<CloudSearchSuggestionItemDto>? Songs { get; set; }

    [JsonPropertyName("albums")]
    public List<CloudSearchSuggestionItemDto>? Albums { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchSuggestionItemDto>? Artists { get; set; }

    [JsonPropertyName("playlists")]
    public List<CloudSearchSuggestionItemDto>? Playlists { get; set; }
}

internal sealed class CloudSearchSuggestionItemDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal sealed class CloudSongUrlResponse
{
    [JsonPropertyName("data")]
    public List<CloudSongUrlItem>? Data { get; set; }
}

internal sealed class CloudSongUrlItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("br")]
    public int BitRate { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

internal sealed class CloudSongDetailResponse
{
    [JsonPropertyName("songs")]
    public List<CloudSongTrackDto>? Songs { get; set; }
}

internal sealed class CloudSongTrackDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("al")]
    public CloudSongAlbumDto? Album { get; set; }

    [JsonPropertyName("ar")]
    public List<CloudSearchArtistReferenceDto>? Artists { get; set; }

    [JsonPropertyName("dt")]
    public long Duration { get; set; }
}

internal sealed class CloudSongAlbumDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }

    [JsonPropertyName("publishTime")]
    public long PublishTime { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchArtistReferenceDto>? Artists { get; set; }
}

internal sealed class CloudAlbumResponse
{
    [JsonPropertyName("album")]
    public CloudAlbumDetailDto? Album { get; set; }

    [JsonPropertyName("songs")]
    public List<CloudSongTrackDto>? Songs { get; set; }
}

internal sealed class CloudAlbumDetailDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("publishTime")]
    public long PublishTime { get; set; }

    [JsonPropertyName("artists")]
    public List<CloudSearchArtistReferenceDto>? Artists { get; set; }
}

internal sealed class CloudLyricResponse
{
    [JsonPropertyName("lrc")]
    public CloudLyricDto? Lrc { get; set; }

    [JsonPropertyName("yrc")]
    public CloudLyricDto? Yrc { get; set; }
}

internal sealed class CloudLyricDto
{
    [JsonPropertyName("lyric")]
    public string? Lyric { get; set; }
}

internal sealed class CloudArtistAlbumResponse
{
    [JsonPropertyName("artist")]
    public CloudArtistDto? Artist { get; set; }

    [JsonPropertyName("hotAlbums")]
    public List<CloudArtistAlbumDto>? Albums { get; set; }
}

internal sealed class CloudArtistDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }

    [JsonPropertyName("albumSize")]
    public int AlbumSize { get; set; }

    [JsonPropertyName("musicSize")]
    public int MusicSize { get; set; }
}

internal sealed class CloudArtistAlbumDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("picUrl")]
    public string? PicUrl { get; set; }

    [JsonPropertyName("publishTime")]
    public long PublishTime { get; set; }
}

internal sealed class CloudArtistDescriptionResponse
{
    [JsonPropertyName("briefDesc")]
    public string? BriefDescription { get; set; }
}

internal sealed class CloudPlaylistDetailResponse
{
    [JsonPropertyName("playlist")]
    public CloudPlaylistDetailDto? Playlist { get; set; }
}

internal sealed class CloudPlaylistDetailDto
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("trackIds")]
    public List<CloudPlaylistTrackIdDto>? TrackIds { get; set; }
}

internal sealed class CloudPlaylistTrackIdDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
}
