using System.Collections.Generic;

namespace FFLogsViewer;

public static class TomestoneEncounterMapping
{
    // Maps FFLogs encounter IDs to Tomestone encounter info
    // For ultimates: TomestoneId is used (numeric ID)
    // For savage/extreme: Slug is used with expansion and zone info
    private static readonly Dictionary<int, (int? TomestoneId, string Slug, string Category, string Expansion, string Zone)> EncounterMap = new()
    {
        // ===== ULTIMATES =====
        { 1079, (5651, "futures-rewritten-ultimate", "ultimate", "dt", "") },      // FRU
        { 1077, (4652, "the-omega-protocol-ultimate", "ultimate", "ew", "") },     // TOP
        { 1068, (4652, "the-omega-protocol-ultimate", "ultimate", "ew", "") },     // TOP (alternate)
        { 1076, (4651, "dragonsongs-reprise-ultimate", "ultimate", "ew", "") },    // DSR
        { 1065, (4651, "dragonsongs-reprise-ultimate", "ultimate", "ew", "") },    // DSR (alternate)
        { 1075, (3651, "the-epic-of-alexander-ultimate", "ultimate", "shb", "") },  // TEA
        { 1062, (3651, "the-epic-of-alexander-ultimate", "ultimate", "shb", "") },  // TEA (alternate)
        { 1050, (3651, "the-epic-of-alexander-ultimate", "ultimate", "shb", "") },  // TEA (alternate)
        { 1074, (2652, "the-weapons-refrain-ultimate", "ultimate", "sb", "") },    // UWU
        { 1061, (2652, "the-weapons-refrain-ultimate", "ultimate", "sb", "") },    // UWU (alternate)
        { 1048, (2652, "the-weapons-refrain-ultimate", "ultimate", "sb", "") },    // UWU (alternate)
        { 1042, (2652, "the-weapons-refrain-ultimate", "ultimate", "sb", "") },    // UWU (alternate)
        { 1073, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimate", "sb", "") }, // UCOB
        { 1060, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimate", "sb", "") }, // UCOB (alternate)
        { 1047, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimate", "sb", "") }, // UCOB (alternate)
        { 1039, (2651, "the-unending-coil-of-bahamut-ultimate", "ultimate", "sb", "") }, // UCOB (alternate)

        // ===== DAWNTRAIL SAVAGE =====
        { 101, (null, "vamp-fatale", "savage", "dt", "arcadion") },           // M9S
        { 102, (null, "red-hot-deep-blue", "savage", "dt", "arcadion") },     // M10S
        { 103, (null, "the-tyrant", "savage", "dt", "arcadion") },            // M11S
        { 104, (null, "lindwurm", "savage", "dt", "arcadion") },              // M12S P1
        { 105, (null, "lindwurm-ii", "savage", "dt", "arcadion") },           // M12S P2
        { 95, (null, "black-cat", "savage", "dt", "arcadion") },              // M5S
        { 96, (null, "honey-b-lovely", "savage", "dt", "arcadion") },         // M6S
        { 97, (null, "brute-bomber", "savage", "dt", "arcadion") },           // M7S
        { 98, (null, "queen-eternal", "savage", "dt", "arcadion") },          // M8S P1
        { 99, (null, "the-minstrelsy", "savage", "dt", "arcadion") },         // M8S P2
        { 89, (null, "black-cat", "savage", "dt", "arcadion") },              // M1S
        { 90, (null, "honey-b-lovely", "savage", "dt", "arcadion") },         // M2S
        { 91, (null, "brute-bomber", "savage", "dt", "arcadion") },           // M3S
        { 92, (null, "wicked-thunder", "savage", "dt", "arcadion") },         // M4S P1
        { 93, (null, "wicked-thunder-ii", "savage", "dt", "arcadion") },      // M4S P2

        // ===== ENDWALKER SAVAGE =====
        { 83, (null, "kokytos", "savage", "ew", "anabaseios") },              // P9S
        { 84, (null, "pandaemonium", "savage", "ew", "anabaseios") },         // P10S
        { 85, (null, "themis", "savage", "ew", "anabaseios") },               // P11S
        { 86, (null, "athena", "savage", "ew", "anabaseios") },               // P12S P1
        { 87, (null, "pallas-athena", "savage", "ew", "anabaseios") },        // P12S P2
        { 77, (null, "kokytos", "savage", "ew", "abyssos") },                 // P5S
        { 78, (null, "cachexia", "savage", "ew", "abyssos") },                // P6S
        { 79, (null, "agdistis", "savage", "ew", "abyssos") },                // P7S
        { 80, (null, "hephaistos-i", "savage", "ew", "abyssos") },            // P8S P1
        { 81, (null, "hephaistos-ii", "savage", "ew", "abyssos") },           // P8S P2
        { 71, (null, "erichthonios", "savage", "ew", "asphodelos") },         // P1S
        { 72, (null, "hippokampos", "savage", "ew", "asphodelos") },          // P2S
        { 73, (null, "phoinix", "savage", "ew", "asphodelos") },              // P3S
        { 74, (null, "hesperos-i", "savage", "ew", "asphodelos") },           // P4S P1
        { 75, (null, "hesperos-ii", "savage", "ew", "asphodelos") },          // P4S P2

        // ===== SHADOWBRINGERS SAVAGE =====
        { 67, (null, "cloud-of-darkness", "savage", "shb", "edens-promise") },    // E9S
        { 68, (null, "shadowkeeper", "savage", "shb", "edens-promise") },         // E10S
        { 69, (null, "fatebreaker", "savage", "shb", "edens-promise") },          // E11S
        { 70, (null, "oracle-of-darkness", "savage", "shb", "edens-promise") },   // E12S
        { 61, (null, "ramuh", "savage", "shb", "edens-verse") },                  // E5S
        { 62, (null, "garuda-ifrit", "savage", "shb", "edens-verse") },           // E6S
        { 63, (null, "the-idol-of-darkness", "savage", "shb", "edens-verse") },   // E7S
        { 64, (null, "shiva", "savage", "shb", "edens-verse") },                  // E8S
        { 55, (null, "eden-prime", "savage", "shb", "edens-gate") },              // E1S
        { 56, (null, "voidwalker", "savage", "shb", "edens-gate") },              // E2S
        { 57, (null, "leviathan", "savage", "shb", "edens-gate") },               // E3S
        { 58, (null, "titan", "savage", "shb", "edens-gate") },                   // E4S

        // ===== STORMBLOOD SAVAGE =====
        { 47, (null, "chaos", "savage", "sb", "alphascape") },                    // O9S
        { 48, (null, "midgardsormr", "savage", "sb", "alphascape") },             // O10S
        { 49, (null, "omega", "savage", "sb", "alphascape") },                    // O11S
        { 50, (null, "omega-m-and-omega-f", "savage", "sb", "alphascape") },      // O12S
        { 42, (null, "phantom-train", "savage", "sb", "sigmascape") },            // O5S
        { 43, (null, "demon-chadarnook", "savage", "sb", "sigmascape") },         // O6S
        { 44, (null, "guardian", "savage", "sb", "sigmascape") },                 // O7S
        { 45, (null, "kefka", "savage", "sb", "sigmascape") },                    // O8S
        { 37, (null, "alte-roite", "savage", "sb", "deltascape") },               // O1S
        { 38, (null, "catastrophe", "savage", "sb", "deltascape") },              // O2S
        { 39, (null, "halicarnassus", "savage", "sb", "deltascape") },            // O3S
        { 40, (null, "exdeath", "savage", "sb", "deltascape") },                  // O4S
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
        return EncounterMap.TryGetValue(fflogsEncounterId, out var info) && info.Category == "ultimate";
    }

    public static bool IsSavage(int fflogsEncounterId)
    {
        return EncounterMap.TryGetValue(fflogsEncounterId, out var info) && info.Category == "savage";
    }
}
