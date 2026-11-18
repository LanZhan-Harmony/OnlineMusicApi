using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliSpaceDataDto
{
    [JsonPropertyName("list")]
    public BilibiliSpaceListDto? List { get; set; }

    [JsonPropertyName("page")]
    public BilibiliSpacePageDto? Page { get; set; }
}
