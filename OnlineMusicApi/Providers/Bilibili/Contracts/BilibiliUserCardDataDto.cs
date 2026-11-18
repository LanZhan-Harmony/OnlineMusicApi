using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliUserCardDataDto
{
    [JsonPropertyName("card")]
    public BilibiliUserCardInfoDto? Card { get; set; }
}
