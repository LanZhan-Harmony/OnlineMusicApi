using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.Bilibili;

internal sealed class BilibiliTicketDataDto
{
    [JsonPropertyName("nav")]
    public BilibiliNavDto? Nav { get; set; }
}
