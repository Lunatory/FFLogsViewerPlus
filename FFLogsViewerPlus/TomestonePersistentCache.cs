using System;
using System.Collections.Generic;

namespace FFLogsViewerPlus;

[Serializable]
public class TomestonePersistentCache
{
    public Dictionary<string, CachedClearData> ClearedEncounters { get; set; } = new();
    
    [Serializable]
    public class CachedClearData
    {
        public DateTime? ClearDateTime { get; set; }
        public string? CompletionWeek { get; set; }
        public DateTime CachedAt { get; set; }
    }
}