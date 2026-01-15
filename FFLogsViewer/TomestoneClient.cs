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
    private const int RetryCount = 3;

    private readonly HttpClient httpClient;
    private readonly HttpClient httpClientNoRedirect;
    /// Add cache for character slug
    private readonly Dictionary<uint, string> characterSlugCache = new();
    ///
    private string? inertiaVersion;

    public TomestoneClient()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = false,
            AllowAutoRedirect = false
        };
        this.httpClient = new HttpClient(handler);

        this.httpClientNoRedirect = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false
        });
    }

    public async Task<uint?> FetchLodestoneId(string firstName, string lastName, string world)
    {
        try
        {
            var fullName = $"{firstName} {lastName}";
            var url = $"https://tomestone.gg/character-name/{world}/{Uri.EscapeDataString(fullName)}";

            Service.PluginLog.Debug($"Fetching Lodestone ID from: {url}");

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

                var match = LodestoneIdRegex().Match(location);
                if (match.Success && uint.TryParse(match.Groups[1].Value, out var lodestoneId))
                {
                    /// Cache the character slug (e.g., "noelle-abeille")
                    var slugMatch = System.Text.RegularExpressions.Regex.Match(location, @"/character/\d+/([^/?]+)");
                    if (slugMatch.Success)
                    {
                        this.characterSlugCache[lodestoneId] = slugMatch.Groups[1].Value;
                    }
                    ///

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
            if (category == "ultimates" && ultimateId.HasValue)
            {
                return await FetchUltimateData(lodestoneId, ultimateId.Value);
            }

            if (category == "raids")
            {
                return await FetchSavageData(lodestoneId, encounterSlug, expansion, zone);
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
            /// Get character slug from cache
            if (!this.characterSlugCache.TryGetValue(lodestoneId, out var characterSlug))
            {
                Service.PluginLog.Warning($"No character slug cached for lodestone ID {lodestoneId}");
                characterSlug = "dummy"; // fallback
            }

            // For ultimates, we need to fetch from activity page to get prog data
            // The headerEncounters endpoint only shows clears, not progression
            var url = $"https://tomestone.gg/character/{lodestoneId}/{characterSlug}/activity?" +
                     $"category=ultimates&" +
                     $"encounter={GetUltimateSlug(ultimateId)}&" +
                     $"expansion={GetUltimateExpansion(ultimateId)}&" +
                     $"league=all&" +
                     $"zone=ultimates";
            ///

            var response = await GetDynamicData(url, "characterPageContent");

            if (response == null)
            {
                return null;
            }

            /// Use the same parsing logic as savage
            return ParseActivityFromJson(response, ultimateId.ToString());
            ///
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching ultimate data");
            return null;
        }
    }

    /// Add helper methods to get ultimate info
    private string GetUltimateSlug(int ultimateId)
    {
        return ultimateId switch
        {
            5651 => "futures-rewritten-ultimate",
            4652 => "the-omega-protocol-ultimate",
            4651 => "dragonsongs-reprise-ultimate",
            3651 => "the-epic-of-alexander-ultimate",
            2652 => "the-weapons-refrain-ultimate",
            2651 => "the-unending-coil-of-bahamut-ultimate",
            _ => ""
        };
    }

    private string GetUltimateExpansion(int ultimateId)
    {
        return ultimateId switch
        {
            5651 => "dawntrail",
            4652 or 4651 => "endwalker",
            3651 => "shadowbringers",
            2652 or 2651 => "stormblood",
            _ => ""
        };
    }


    private async Task<TomestoneData?> FetchSavageData(
        uint lodestoneId,
        string encounterSlug,
        string expansion,
        string zoneName)
    {
        try
        {
            // Match the URL format that works in browser: category, encounter, expansion, league, sortType, zone
            var url = $"https://tomestone.gg/character/{lodestoneId}/dummy/activity?" +
                     $"category=raids&" +
                     $"encounter={encounterSlug}&" +
                     $"expansion={expansion}&" +
                     $"league=all&" +
                     $"sortType=firstKillTime&" +
                     $"zone={zoneName}";

            var response = await GetDynamicData(url, "characterPageContent");

            if (response == null)
            {
                return null;
            }

            return ParseActivityFromJson(response, encounterSlug);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching savage data");
            return null;
        }
    }

    private async Task<dynamic?> GetDynamicData(string uri, string partialData)
    {
        for (var i = 0; i < RetryCount; i++)
        {
            if (this.inertiaVersion == null)
            {
                var success = await RefreshInertiaVersion();
                if (!success)
                {
                    continue;
                }
            }

            try
            {
                var localInertiaVersion = this.inertiaVersion;
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("accept", "text/html, application/xhtml+xml");
                request.Headers.Add("accept-language", "en-US,en;q=0.9");
                request.Headers.Add("x-inertia", "true");
                request.Headers.Add("x-inertia-version", localInertiaVersion);
                request.Headers.Add("x-inertia-partial-component", "Characters/Character");
                request.Headers.Add("x-inertia-partial-data", partialData);

                var response = await this.httpClient.SendAsync(request);
                var jsonContent = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Inertia version changed, refresh and retry
                    Service.PluginLog.Info("Inertia version conflict, refreshing...");
                    this.inertiaVersion = null;
                    continue;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Service.PluginLog.Warning($"Not found: {uri}");
                    return null;
                }
                else if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Service.PluginLog.Warning($"Request to {uri} failed with {response.StatusCode}");
                    continue;
                }

                Service.PluginLog.Debug($"Successfully fetched data from {uri}");
                return JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, $"Error making dynamic request to {uri}");
            }
        }

        return null;
    }

    private async Task<bool> RefreshInertiaVersion()
    {
        try
        {
            Service.PluginLog.Debug("Fetching Inertia version...");
            var response = await this.httpClient.GetAsync("https://tomestone.gg/");
            var content = await response.Content.ReadAsStringAsync();

            // Parse the inertia version from the HTML
            var versionStart = content.IndexOf("&quot;version&quot;:&quot;");
            if (versionStart == -1)
            {
                Service.PluginLog.Error("Could not find Inertia version in HTML");
                return false;
            }

            versionStart += "&quot;version&quot;:&quot;".Length;
            var versionEnd = content.IndexOf("&quot;", versionStart);
            if (versionEnd == -1)
            {
                Service.PluginLog.Error("Could not parse Inertia version");
                return false;
            }

            this.inertiaVersion = content.Substring(versionStart, versionEnd - versionStart);
            Service.PluginLog.Info($"Fetched Inertia version: {this.inertiaVersion}");
            return true;
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error fetching Inertia version");
            return false;
        }
    }

    private TomestoneData? ParseUltimateFromJson(dynamic? data, int ultimateId)
    {
        try
        {
            // Add debug logging to see the structure
            var propsData = data?.props;
            if (propsData == null)
            {
                Service.PluginLog.Warning("No props found in ultimate data");
                return null;
            }

            var headerEncounters = propsData?.headerEncounters;
            if (headerEncounters == null)
            {
                Service.PluginLog.Warning("No headerEncounters found in character data - might be disabled");
                return null;
            }

            Service.PluginLog.Debug($"HeaderEncounters structure: {JsonConvert.SerializeObject(headerEncounters)}");

            var latestExpansion = headerEncounters?.latestExpansion;
            Service.PluginLog.Debug($"LatestExpansion exists: {latestExpansion != null}");

            // Check ultimates for clears
            var ultimates = headerEncounters?.latestExpansion?.ultimate;
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
            var progTargets = headerEncounters?.allUltimateProgressionTargets;
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
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, $"Error parsing ultimate data for ID {ultimateId}");
            return null;
        }
    }

    private TomestoneData? ParseActivityFromJson(dynamic? data, string encounterSlug)
    {
        try
        {
            /// Add debug logging
            Service.PluginLog.Debug($"Parsing savage data for {encounterSlug}");
            Service.PluginLog.Debug($"Data structure: {Newtonsoft.Json.JsonConvert.SerializeObject(data?.props?.characterPageContent?.activities, Newtonsoft.Json.Formatting.None)}");
            ///

            // Check if activities are available (might be disabled)
            var activitiesObj = data?.props?.characterPageContent?.activities?.activities;
            if (activitiesObj == null)
            {
                Service.PluginLog.Debug($"Activities disabled or not found for {encounterSlug}");
                return TomestoneData.EncounterNotStarted();
            }

            /// Fix: Add the missing .activities level
            var activities = activitiesObj?.activities?.paginator?.data;
            ///
            if (activities == null)
            {
                Service.PluginLog.Debug($"No paginator data found for {encounterSlug}");
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
                    /// Fix: explicitly handle nullable DateTime
                    var endTimeObj = activity.activity.endTime;
                    DateTime? dateTime = ParseDateTime(endTimeObj);
                    DateOnly? dateOnly = dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
                    ///

                    var jobId = activity.activity.displayCharacterJobOrSpec?.id != null
                        ? (uint)(int)activity.activity.displayCharacterJobOrSpec.id
                        : 0u;

                    var lockout = new TomestoneProgPoint.Lockout(
                        TomestoneProgPoint.Percent.From(activity.activity.bestPercent.ToString()),
                        /// Use the dateOnly we just created
                        dateOnly,
                        ///
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
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, $"Error parsing savage data for {encounterSlug}");
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
}
