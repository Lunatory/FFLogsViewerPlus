using Newtonsoft.Json;

namespace FFLogsViewerPlus.Model.GameData;

public class Data
{
    [JsonProperty("worldData")]
    public WorldData? WorldData { get; set; }
}
