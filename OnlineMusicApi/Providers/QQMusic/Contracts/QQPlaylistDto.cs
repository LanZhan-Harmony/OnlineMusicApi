using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.QQMusic;

internal sealed class QQPlaylistDto
{
    [JsonPropertyName("dissid")]
    public long DissId { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("dissname")]
    public string? DissName { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("imgurl")]
    public string? ImgUrl { get; set; }

    [JsonPropertyName("artwork")]
    public string? Artwork { get; set; }

    [JsonPropertyName("song_count")]
    public int? SongCount { get; set; }

    [JsonPropertyName("listennum")]
    public int? ListenNum { get; set; }

    [JsonPropertyName("worksNums")]
    public int? WorksNums { get; set; }

    [JsonPropertyName("creator")]
    public string? Creator { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("createAt")]
    public string? CreateAt { get; set; }

    [JsonPropertyName("program_desc")]
    public string? ProgramDesc { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
