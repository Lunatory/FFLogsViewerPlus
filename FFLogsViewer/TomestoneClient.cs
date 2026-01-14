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
    private readonly HttpClient httpClientNoRedirect;
    private readonly Dictionary<uint, string> characterUrlCache = new();

    public TomestoneClient()
    {
        this.httpClient = new HttpClient();

        // Create a separate client that doesn't follow redirects for the character-name endpoint
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };
        this.httpClientNoRedirect = new HttpClient(handler);
    }

    public async Task<uint?> FetchLodestoneId(string firstName, string lastName, string world)
    {
        try
        {
            var fullName = $"{firstName} {lastName}";
            var url = $"https://tomestone.gg/character-name/{world}/{Uri.EscapeDataString(fullName)}";

            Service.PluginLog.Debug($"Fetching Lodestone ID from: {url}");

            // Use the no-redirect client to get the 302 response
            var response = await this.httpClientNoRedirect.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == System.Net.HttpStatusCode.Found ||
                response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                response.StatusCode == System.Net.HttpStatusCode.MovedPermanently)
            {
                var location = response.Headers.Location?.ToString();
                if (location == null)
                {
                    Service.PluginLog.Error("Failed to fetch lodestoneId: location not returned in headers");
                    return null;
                }

                Service.PluginLog.Debug($"Redirect location: {location}");

                var match = LodestoneIdRegex().Match(location);
                if (match.Success && uint.TryParse(match.Groups[1].Value, out var lodestoneId))
                {
                    // Cache the full character URL
                    this.characterUrlCache[lodestoneId] = location;

                    Service.PluginLog.Info($"Found Lodestone ID: {lodestoneId} for {firstName} {lastName}@{world}");
                    return lodestoneId;
                }
                else
                {
                    Service.PluginLog.Error($"Failed to parse lodestoneId from location: {location}");
                    return null;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Service.PluginLog.Warning($"Character not found on Tomestone: {firstName} {lastName}@{world}");
                return null;
            }
            else
            {
                Service.PluginLog.Error($"Unexpected status code when fetching lodestoneId: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching Lodestone ID");
            return null;
        }
        finally
        {
            // Ensure we dispose the response
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
            // Use cached character URL if available, otherwise construct basic URL
            var url = this.characterUrlCache.TryGetValue(lodestoneId, out var cachedUrl)
                ? cachedUrl
                : $"https://tomestone.gg/character/{lodestoneId}";

            Service.PluginLog.Debug($"Fetching ultimate data from: {url}");

            var response = await this.httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Service.PluginLog.Warning($"Failed to fetch character page: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            // Debug: log first 500 chars to see what we're getting
            Service.PluginLog.Debug($"Page content preview: {content.Substring(0, Math.Min(500, content.Length))}");

            var jsonMatch = JsonDataRegex().Match(content);

            if (!jsonMatch.Success)
            {
                Service.PluginLog.Warning("Failed to find JSON data in character page");
                // Try alternative regex patterns
                var altMatch = Regex.Match(content, @"<script[^>]*id=""__NEXT_DATA__""[^>]*>([^<]+)</script>", RegexOptions.Singleline);
                if (altMatch.Success)
                {
                    Service.PluginLog.Info("Found JSON with alternative regex");
                    var jsonData = altMatch.Groups[1].Value;
                    return await ParseUltimateFromJson(jsonData, ultimateId);
                }
                return null;
            }

            return await ParseUltimateFromJson(jsonMatch.Groups[1].Value, ultimateId);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching ultimate data");
            return null;
        }
    }

    private async Task<TomestoneData?> ParseUltimateFromJson(string jsonData, int ultimateId)
    {
        dynamic? data = JsonConvert.DeserializeObject(jsonData);
        if (data?.props?.pageProps?.headerEncounters == null)
        {
            Service.PluginLog.Warning("No headerEncounters found in character data");
            return null;
        }

        // Check ultimates for clears
        var ultimates = data.props.pageProps.headerEncounters?.latestExpansion?.ultimate;
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
                        Service.PluginLog.Info($"Found clear for ultimate {ultimateId}");
                        return TomestoneData.EncounterCleared(new TomestoneClear(dateTime, week));
                    }
                    else if (ultimate.activity != null)
                    {
                        Service.PluginLog.Info($"Found clear (no achievement) for ultimate {ultimateId}");
                        return TomestoneData.EncounterCleared(new TomestoneClear(null, null));
                    }
                }
            }
        }

        // Check for progression
        var progTargets = data.props.pageProps.headerEncounters?.allUltimateProgressionTargets;
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
                            Service.PluginLog.Info($"Found prog for ultimate {ultimateId}: {percentStr}");
                            return TomestoneData.EncounterInProgress(new TomestoneProgPoint(new[] { lockout }));
                        }
                    }
                }
            }
        }

        Service.PluginLog.Debug($"No data found for ultimate {ultimateId}");
        return TomestoneData.EncounterNotStarted();
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
            // Build activity URL using the cached character URL base
            var baseUrl = this.characterUrlCache.TryGetValue(lodestoneId, out var cachedUrl)
                ? cachedUrl
                : $"https://tomestone.gg/character/{lodestoneId}";

            var categoryParam = category == "savage" ? "savage" : "extreme";
            var zoneParam = zoneName != string.Empty ? zoneName : encounterSlug;

            var url = $"{baseUrl}/activity?" +
                     $"expansion={expansion}&category={categoryParam}&zone={zoneParam}&" +
                     $"encounter={encounterSlug}&sortType=firstKillTime";

            Service.PluginLog.Debug($"Fetching savage data from: {url}");

            var response = await this.httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Service.PluginLog.Warning($"Failed to fetch activity page: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();

            // Debug: log first 500 chars
            Service.PluginLog.Debug($"Activity page content preview: {content.Substring(0, Math.Min(500, content.Length))}");

            var jsonMatch = JsonDataRegex().Match(content);

            if (!jsonMatch.Success)
            {
                Service.PluginLog.Warning("Failed to find JSON data in activity page");
                // Try alternative regex
                var altMatch = Regex.Match(content, @"<script[^>]*id=""__NEXT_DATA__""[^>]*>([^<]+)</script>", RegexOptions.Singleline);
                if (altMatch.Success)
                {
                    Service.PluginLog.Info("Found JSON with alternative regex");
                    return await ParseSavageFromJson(altMatch.Groups[1].Value, encounterSlug);
                }
                return null;
            }

            return await ParseSavageFromJson(jsonMatch.Groups[1].Value, encounterSlug);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching savage data");
            return null;
        }
    }

    private async Task<TomestoneData?> ParseSavageFromJson(string jsonData, string encounterSlug)
    {
        dynamic? data = JsonConvert.DeserializeObject(jsonData);
        var activities = data?.props?.pageProps?.characterPageContent?.activities?.activities?.paginator?.data;

        if (activities == null)
        {
            Service.PluginLog.Debug($"No activities found for {encounterSlug}");
            return TomestoneData.EncounterNotStarted();
        }

        // Parse activities for clears or prog
        List<TomestoneProgPoint.Lockout> lockouts = new();

        foreach (var activity in activities)
        {
            if (activity?.activity?.killsCount != null && (int)activity.activity.killsCount > 0)
            {
                var clearTime = ParseDateTime(activity.endTime ?? activity.activity.endTime);
                Service.PluginLog.Info($"Found clear for {encounterSlug}");
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
            Service.PluginLog.Info($"Found prog for {encounterSlug}: {lockouts[0].Percent}");
            return TomestoneData.EncounterInProgress(new TomestoneProgPoint(lockouts));
        }

        Service.PluginLog.Debug($"No data found for {encounterSlug}");
        return TomestoneData.EncounterNotStarted();
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
