using System;

namespace FFLogsViewerPlus.Model;

public record TomestoneClear(DateTime? DateTime, string? CompletionWeek)
{
    public bool HasInfo => this.DateTime.HasValue;
}
