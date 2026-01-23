namespace FFLogsViewerPlus.Model;

public record TomestoneData
{
    public TomestoneClear? Clear { get; }
    public TomestoneProgPoint? Progress { get; }
    public bool Cleared => this.Clear != null;

    private TomestoneData(TomestoneClear? clear, TomestoneProgPoint? progress)
    {
        this.Clear = clear;
        this.Progress = progress;
    }

    public static TomestoneData EncounterCleared(TomestoneClear clear)
    {
        return new TomestoneData(clear, null);
    }

    public static TomestoneData EncounterInProgress(TomestoneProgPoint progress)
    {
        return new TomestoneData(null, progress);
    }

    public static TomestoneData EncounterNotStarted()
    {
        return new TomestoneData(null, null);
    }
}
