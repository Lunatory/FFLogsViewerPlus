using System.Collections.Generic;

namespace FFLogsViewer;

public static class TomestoneEncounterMapping
{
    // Maps FFLogs encounter IDs to Tomestone encounter info
    // Use parameters that match TomestoneViewer's approach with Inertia API
    private static readonly Dictionary<int, (int? TomestoneId, string Slug, string Category, string Expansion, string Zone)> EncounterMap = new()
    {
        // ===== ULTIMATES =====
        { 1079, (5651, "futures-rewritten-ultimate", "ultimates", "dawntrail", "ultimates") },      // FRU
        { 1077, (4652, "the-omega-protocol-ultimate", "ultimates", "endwalker", "ultimates") },     // TOP
        { 1068, (4652, "the-omega-protocol-ultimate", "ultimates", "endwalker", "ultimates") },     // TOP (alternate)
        { 1076, (4651, "dragonsongs-reprise-ultimate", "ultimates", "endwalker", "ultimates") },    // DSR
        { 1065, (4651, "dragonsongs-reprise-ultimate", "ultimates", "endwalker", "ultimates") },    // DSR (alternate)
        { 1075, (3651, "the-epic-of-alexander-ultimate", "ultimates", "shadowbringers", "ultimates") },  // TEA
        { 1062, (3651, "the-epic-of-alexander-ultimate", "ultimates", "shadowbringers", "ultimates") },  // TEA (alternate)
        { 1050, (3651, "the-epic-of-alexander-ultimate", "ultimates", "shadowbringers", "ultimates") },  // TEA (alternate)
        { 1074, (2652, "the-weapons-refrain-ultimate", "ultimates", "stormblood", "ultimates") },    // UWU
        { 1061, (2652, "the-weapons-refrain-ultimate", "ultimates", "stormblood", "ultimates") },    // UWU (alternate)
        { 1048, (2652, "the-weapons-refrain-ultimate", "ultimates", "stormblood", "ultimates") },    // UWU (alternate)
        { 1042, (2652, "the-weapons-refrain-ultimate", "ultimates", "stormblood", "ultimates") },    // UWU (alternate)
        { 1073, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimates", "stormblood", "ultimates") }, // UCOB
        { 1060, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimates", "stormblood", "ultimates") }, // UCOB (alternate)
        { 1047, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimates", "stormblood", "ultimates") }, // UCOB (alternate)
        { 1039, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimates", "stormblood", "ultimates") }, // UCOB (alternate)

        // ===== DAWNTRAIL SAVAGE =====
        // AAC Heavyweight
        { 101, (null, "vamp-fatale", "raids", "dawntrail", "aac-heavyweight-savage") },           // M9S
        { 102, (null, "red-hot-deep-blue", "raids", "dawntrail", "aac-heavyweight-savage") },     // M10S
        { 103, (null, "the-tyrant", "raids", "dawntrail", "aac-heavyweight-savage") },            // M11S
        { 104, (null, "lindwurm", "raids", "dawntrail", "aac-heavyweight-savage") },              // M12S P1
        { 105, (null, "lindwurm-ii", "raids", "dawntrail", "aac-heavyweight-savage") },           // M12S P2
        
        /// OLDER TIERS - COMMENTED OUT FOR PERFORMANCE
        /*
        // AAC Cruiserweight
        { 97, (null, "dancing-green", "raids", "dawntrail", "aac-cruiserweight-savage") },        // M5S
        { 98, (null, "sugar-riot", "raids", "dawntrail", "aac-cruiserweight-savage") },           // M6S
        { 99, (null, "brute-abombinator", "raids", "dawntrail", "aac-cruiserweight-savage") },    // M7S
        { 100, (null, "howling-blade", "raids", "dawntrail", "aac-cruiserweight-savage") },       // M8S
        
        // AAC Light-heavyweight
        { 89, (null, "black-cat", "raids", "dawntrail", "aac-light-heavyweight-savage") },        // M1S
        { 90, (null, "honey-b-lovely", "raids", "dawntrail", "aac-light-heavyweight-savage") },   // M2S
        { 91, (null, "brute-bomber", "raids", "dawntrail", "aac-light-heavyweight-savage") },     // M3S
        { 92, (null, "wicked-thunder", "raids", "dawntrail", "aac-light-heavyweight-savage") },   // M4S P1
        { 93, (null, "wicked-thunder-ii", "raids", "dawntrail", "aac-light-heavyweight-savage") }, // M4S P2

        // ===== ENDWALKER SAVAGE =====
        // Anabaseios
        { 83, (null, "kokytos", "raids", "endwalker", "anabaseios-savage") },              // P9S
        { 84, (null, "pandaemonium", "raids", "endwalker", "anabaseios-savage") },         // P10S
        { 85, (null, "themis", "raids", "endwalker", "anabaseios-savage") },               // P11S
        { 86, (null, "athena", "raids", "endwalker", "anabaseios-savage") },               // P12S P1
        { 87, (null, "pallas-athena", "raids", "endwalker", "anabaseios-savage") },        // P12S P2
        
        // Abyssos
        { 77, (null, "proto-carbuncle", "raids", "endwalker", "abyssos-savage") },         // P5S
        { 78, (null, "hegemone", "raids", "endwalker", "abyssos-savage") },                // P6S
        { 79, (null, "agdistis", "raids", "endwalker", "abyssos-savage") },                // P7S
        { 80, (null, "hephaistos-i", "raids", "endwalker", "abyssos-savage") },            // P8S P1
        { 81, (null, "hephaistos-ii", "raids", "endwalker", "abyssos-savage") },           // P8S P2
        
        // Asphodelos
        { 71, (null, "erichthonios", "raids", "endwalker", "asphodelos-savage") },         // P1S
        { 72, (null, "hippokampos", "raids", "endwalker", "asphodelos-savage") },          // P2S
        { 73, (null, "phoinix", "raids", "endwalker", "asphodelos-savage") },              // P3S
        { 74, (null, "hesperos-i", "raids", "endwalker", "asphodelos-savage") },           // P4S P1
        { 75, (null, "hesperos-ii", "raids", "endwalker", "asphodelos-savage") },          // P4S P2

        // ===== SHADOWBRINGERS SAVAGE =====
        // Eden's Promise
        { 67, (null, "cloud-of-darkness", "raids", "shadowbringers", "edens-promise-savage") },    // E9S
        { 68, (null, "shadowkeeper", "raids", "shadowbringers", "edens-promise-savage") },         // E10S
        { 69, (null, "fatebreaker", "raids", "shadowbringers", "edens-promise-savage") },          // E11S
        { 70, (null, "oracle-of-darkness", "raids", "shadowbringers", "edens-promise-savage") },   // E12S
        
        // Eden's Verse
        { 61, (null, "ramuh", "raids", "shadowbringers", "edens-verse-savage") },                  // E5S
        { 62, (null, "garuda-ifrit", "raids", "shadowbringers", "edens-verse-savage") },           // E6S
        { 63, (null, "the-idol-of-darkness", "raids", "shadowbringers", "edens-verse-savage") },   // E7S
        { 64, (null, "shiva", "raids", "shadowbringers", "edens-verse-savage") },                  // E8S
        
        // Eden's Gate
        { 55, (null, "eden-prime", "raids", "shadowbringers", "edens-gate-savage") },              // E1S
        { 56, (null, "voidwalker", "raids", "shadowbringers", "edens-gate-savage") },              // E2S
        { 57, (null, "leviathan", "raids", "shadowbringers", "edens-gate-savage") },               // E3S
        { 58, (null, "titan", "raids", "shadowbringers", "edens-gate-savage") },                   // E4S

        // ===== STORMBLOOD SAVAGE =====
        // Alphascape
        { 47, (null, "chaos", "raids", "stormblood", "alphascape-savage") },                    // O9S
        { 48, (null, "midgardsormr", "raids", "stormblood", "alphascape-savage") },             // O10S
        { 49, (null, "omega", "raids", "stormblood", "alphascape-savage") },                    // O11S
        { 50, (null, "omega-m-and-omega-f", "raids", "stormblood", "alphascape-savage") },      // O12S
        
        // Sigmascape
        { 42, (null, "phantom-train", "raids", "stormblood", "sigmascape-savage") },            // O5S
        { 43, (null, "demon-chadarnook", "raids", "stormblood", "sigmascape-savage") },         // O6S
        { 44, (null, "guardian", "raids", "stormblood", "sigmascape-savage") },                 // O7S
        { 45, (null, "kefka", "raids", "stormblood", "sigmascape-savage") },                    // O8S
        
        // Deltascape
        { 37, (null, "alte-roite", "raids", "stormblood", "deltascape-savage") },               // O1S
        { 38, (null, "catastrophe", "raids", "stormblood", "deltascape-savage") },              // O2S
        { 39, (null, "halicarnassus", "raids", "stormblood", "deltascape-savage") },            // O3S
        { 40, (null, "exdeath", "raids", "stormblood", "deltascape-savage") },                  // O4S
         */
        ///
    };

    public static bool TryGetTomestoneInfo(int fflogsEncounterId, out int? tomestoneId, out string slug, out string category, out string expansion, out string zone)
    {
        if (EncounterMap.TryGetValue(fflogsEncounterId, out var info))
        {
            tomestoneId = info.TomestoneId;
            slug = info.Slug;
            category = info.Category;
            expansion = info.Expansion;
            zone = info.Zone;
            return true;
        }

        tomestoneId = null;
        slug = string.Empty;
        category = string.Empty;
        expansion = string.Empty;
        zone = string.Empty;
        return false;
    }

    public static bool HasTomestoneSupport(int fflogsEncounterId)
    {
        return EncounterMap.ContainsKey(fflogsEncounterId);
    }

    public static bool IsUltimate(int fflogsEncounterId)
    {
        return EncounterMap.TryGetValue(fflogsEncounterId, out var info) && info.Category == "ultimates";
    }

    public static bool IsSavage(int fflogsEncounterId)
    {
        return EncounterMap.TryGetValue(fflogsEncounterId, out var info) && info.Category == "raids";
    }
}
