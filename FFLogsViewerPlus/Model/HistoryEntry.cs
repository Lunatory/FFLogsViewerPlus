using System;

namespace FFLogsViewerPlus.Model;

public class HistoryEntry
{
    public string FirstName = null!;
    public string LastName = null!;
    public string WorldName = null!;
    public DateTime LastSeen = DateTime.Now;
}
