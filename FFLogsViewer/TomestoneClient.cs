using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FFLogsViewer.Model;
using Newtonsoft.Json;

namespace FFLogsViewer;

public partial class TomestoneClient
{
    private readonly HttpClient httpClient;

    public TomestoneClient()
    {
        this.httpClient = new HttpClient();
    }

    public async Task<uint?> FetchLodestoneId(string firstName, string lastName, string world)
    {
        try
        {
            var fullName = $"{firstName} {lastName}";
            var url = $"https://tomestone.gg/character-name/{world}/{Uri.EscapeDataString(fullName)}";
            
            var response = await this.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            
            if (response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                var location = response.Headers.Location?.ToString();
                if (location == null)
                {
                    return null;
                }

                var match = LodestoneIdRegex().Match(location);
                if (match.Success && uint.TryParse(match.Groups[1].Value, out var lodestoneId))
                {
                    return lodestoneId;
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching Lodestone ID");
            return null;
        }
    }

    public async Task<TomestoneData?> FetchEncounterData(
        uint lodestoneId, 
        int? ultimateId,
        string encounterSlug,
        string category,
        string expansion,
        string zone)
    {
        try
        {
            // For ultimates, we can get data from the character summary page
            if (category == "ultimate" && ultimateId.HasValue)
            {
                return await FetchUltimateData(lodestoneId, ultimateId.Value);
            }
            
            // For savage/extreme, we need the activity page
            if (category == "savage" || category == "extreme")
            {
                return await FetchSavageData(lodestoneId, encounterSlug, category, expansion, zone);
            }

            return null;
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, $"Error fetching Tomestone data for {encounterSlug}");
            return null;
        }
    }

    private async Task<TomestoneData?> FetchUltimateData(uint lodestoneId, int ultimateId)
    {
        try
        {
            var url = $"https://tomestone.gg/character/{lodestoneId}/dummy";
            var response = await this.httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonMatch = JsonDataRegex().Match(content);
            
            if (!jsonMatch.Success)
            {
                return null;
            }

            dynamic? data = JsonConvert.DeserializeObject(jsonMatch.Groups[1].Value);
            if (data?.props?.headerEncounters == null)
            {
                return null;
            }

            // Check ultimates for clears
            var ultimates = data.props.headerEncounters?.latestExpansion?.ultimate;
            if (ultimates != null)
            {
                foreach (var ultimate in ultimates)
                {
                    if ((int)ultimate.id == ultimateId)
                    {
                        if (ultimate.achievement != null)
                        {
                            var dateTime = ParseDateTime(ultimate.achievement.completedAt);
                            var week = ultimate.achievement.completionWeek?.ToString();
                            return TomestoneData.EncounterCleared(new TomestoneClear(dateTime, week));
                        }
                        else if (ultimate.activity != null)
                        {
                            return TomestoneData.EncounterCleared(new TomestoneClear(null, null));
                        }
                    }
                }
            }

            // Check for progression
            var progTargets = data.props.headerEncounters?.allUltimateProgressionTargets;
            if (progTargets != null)
            {
                foreach (var container in progTargets)
                {
                    foreach (var child in container)
                    {
                        dynamic? ultimate = child.Value;
                        if (ultimate?.encounter?.id != null && (int)ultimate.encounter.id == ultimateId)
                        {
                            var percentStr = ultimate.percent?.ToString();
                            if (percentStr != null)
                            {
                                var percent = TomestoneProgPoint.Percent.From(percentStr);
                                var lockout = new TomestoneProgPoint.Lockout(percent, null, 0);
                                return TomestoneData.EncounterInProgress(new TomestoneProgPoint(new[] { lockout }));
                            }
                        }
                    }
                }
            }

            return TomestoneData.EncounterNotStarted();
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching ultimate data");
            return null;
        }
    }

    private async Task<TomestoneData?> FetchSavageData(
        uint lodestoneId, 
        string encounterSlug,
        string category,
        string expansion,
        string zoneName)
    {
        try
        {
            // Build activity URL
            var categoryParam = category == "savage" ? "savage" : "extreme";
            var zoneParam = zoneName != string.Empty ? zoneName : encounterSlug;
            
            var url = $"https://tomestone.gg/character/{lodestoneId}/dummy/activity?" +
                     $"expansion={expansion}&category={categoryParam}&zone={zoneParam}&" +
                     $"encounter={encounterSlug}&sortType=firstKillTime";

            Service.PluginLog.Debug($"Fetching savage data: {url}");
            
            var response = await this.httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonMatch = JsonDataRegex().Match(content);
            
            if (!jsonMatch.Success)
            {
                return null;
            }

            dynamic? data = JsonConvert.DeserializeObject(jsonMatch.Groups[1].Value);
            var activities = data?.props?.characterPageContent?.activities?.activities?.paginator?.data;
            
            if (activities == null)
            {
                return TomestoneData.EncounterNotStarted();
            }

            // Parse activities for clears or prog
            List<TomestoneProgPoint.Lockout> lockouts = new();
            
            foreach (var activity in activities)
            {
                if (activity?.activity?.killsCount != null && (int)activity.activity.killsCount > 0)
                {
                    var clearTime = ParseDateTime(activity.endTime ?? activity.activity.endTime);
                    return TomestoneData.EncounterCleared(new TomestoneClear(clearTime, null));
                }
                
                // Collect prog points
                if (activity?.activity?.bestPercent != null)
                {
                    var dateTime = ParseDateTime(activity.activity.endTime);
                    var jobId = activity.activity.displayCharacterJobOrSpec?.id != null 
                        ? (uint)(int)activity.activity.displayCharacterJobOrSpec.id 
                        : 0u;
                    
                    var lockout = new TomestoneProgPoint.Lockout(
                        TomestoneProgPoint.Percent.From(activity.activity.bestPercent.ToString()),
                        dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null,
                        jobId);
                    
                    if (lockouts.Count == 0 || !lockouts[^1].Equals(lockout))
                    {
                        lockouts.Add(lockout);
                    }
                    
                    if (lockouts.Count >= 5)
                    {
                        break;
                    }
                }
            }

            if (lockouts.Count > 0)
            {
                return TomestoneData.EncounterInProgress(new TomestoneProgPoint(lockouts));
            }

            return TomestoneData.EncounterNotStarted();
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching savage data");
            return null;
        }
    }

    private static DateTime? ParseDateTime(object? value)
    {
        var stringValue = value?.ToString();
        if (stringValue == null)
        {
            return null;
        }

        try
        {
            return DateTime.ParseExact(
                stringValue, 
                new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff" }, 
                CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex(@"https://tomestone\.gg/character/(\d+)/")]
    private static partial Regex LodestoneIdRegex();

    [GeneratedRegex(@"<script id=""__NEXT_DATA__"" type=""application/json"">(.*?)</script>")]
    private static partial Regex JsonDataRegex();
}
