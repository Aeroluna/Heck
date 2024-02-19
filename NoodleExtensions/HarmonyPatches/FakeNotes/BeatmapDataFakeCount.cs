using System;
using System.Collections.Generic;
using System.Linq;
using BeatmapSaveDataVersion3;
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
        private static bool Transpiler(ref BeatmapDataBasicInfo __result, BeatmapSaveData beatmapSaveData)
        {
            if (beatmapSaveData is not CustomBeatmapSaveData customBeatmapSaveData
                || !(customBeatmapSaveData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>().Contains(CAPABILITY) ?? false))
            {
                return true;
            }

            string name = customBeatmapSaveData.version2_6_0AndEarlier ? V2_FAKE_NOTE : INTERNAL_FAKE_NOTE;

            int count = beatmapSaveData.colorNotes.Count(n => n.FakeCondition(name));
            int count2 = beatmapSaveData.obstacles.Count(n => n.FakeCondition(name));
            int count3 = beatmapSaveData.bombNotes.Count(n => n.FakeCondition(name));
            List<string> list = beatmapSaveData.basicEventTypesWithKeywords.data
                .Select(basicEventTypesForKeyword => basicEventTypesForKeyword.keyword).ToList();
            __result = new BeatmapDataBasicInfo(4, count, count2, count3, list);
            return false;
        }

        private static bool FakeCondition(this BeatmapSaveData.BeatmapSaveDataItem dataItem, string name)
        {
            try
            {
                bool? fake = ((ICustomData)dataItem).customData.Get<bool?>(name);
                return !fake.HasValue || !fake.Value;
            }
            catch (Exception e)
            {
                Plugin.Log.Error($"Could not parse fake for object [{dataItem.GetType().Name}] at [{dataItem.beat}]");
                Plugin.Log.Error(e);
                return true;
            }
        }
    }
}
