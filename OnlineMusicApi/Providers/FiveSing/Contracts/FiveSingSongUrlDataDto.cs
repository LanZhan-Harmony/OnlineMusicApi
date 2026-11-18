using System.Text.Json.Serialization;

namespace OnlineMusicApi.Internal.Contracts.FiveSing;

internal sealed class FiveSingSongUrlDataDto
{
    [JsonPropertyName("squrl")]
    public string? SqUrl { get; set; }

    [JsonPropertyName("squrl_backup")]
    public string? SqUrlBackup { get; set; }

    [JsonPropertyName("hqurl")]
    public string? HqUrl { get; set; }

    [JsonPropertyName("hqurl_backup")]
    public string? HqUrlBackup { get; set; }

    [JsonPropertyName("lqurl")]
    public string? LqUrl { get; set; }

    [JsonPropertyName("lqurl_backup")]
    public string? LqUrlBackup { get; set; }
}
