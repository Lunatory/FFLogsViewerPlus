using Newtonsoft.Json;

namespace FFLogsViewerPlus.Model.GameData;

public class Errors
{
    [JsonProperty("message")]
    public string? Message { get; set; }
}
