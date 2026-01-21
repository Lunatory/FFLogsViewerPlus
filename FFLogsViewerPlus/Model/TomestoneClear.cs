using System;

namespace FFLogsViewer.Model;

public record TomestoneClear(DateTime? DateTime, string? CompletionWeek)
{
    public bool HasInfo => this.DateTime.HasValue;
}
