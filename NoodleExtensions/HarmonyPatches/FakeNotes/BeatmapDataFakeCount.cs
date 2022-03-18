using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch]
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    internal static class BeatmapDataFakeCount
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData))]
        private static bool Transpiler(ref BeatmapDataBasicInfo __result, CustomBeatmapSaveData beatmapSaveData)
        {
            if (!(beatmapSaveData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>().Contains(CAPABILITY) ?? false))
            {
                return true;
            }

            int count = beatmapSaveData.colorNotes.Count(n => !((CustomBeatmapSaveData.ColorNoteData)n).customData.Get<bool?>(FAKE_NOTE) is true);
            int count2 = beatmapSaveData.obstacles.Count(n => !((CustomBeatmapSaveData.ObstacleData)n).customData.Get<bool?>(FAKE_NOTE) is true);
            int count3 = beatmapSaveData.bombNotes.Count(n => !((CustomBeatmapSaveData.BombNoteData)n).customData.Get<bool?>(FAKE_NOTE) is true);
            List<string> list = beatmapSaveData.basicEventTypesWithKeywords.data.Select(basicEventTypesForKeyword => basicEventTypesForKeyword.keyword).ToList();
            __result = new BeatmapDataBasicInfo(4, count, count2, count3, list);
            return false;
        }
    }
}
