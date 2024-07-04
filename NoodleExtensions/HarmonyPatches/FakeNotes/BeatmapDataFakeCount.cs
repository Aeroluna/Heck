using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using static NoodleExtensions.NoodleController;
using BeatmapSaveData = BeatmapSaveDataVersion3.BeatmapSaveData;
#if LATEST
using _BeatmapSaveDataItemV3 = BeatmapSaveDataVersion3.BeatmapSaveDataItem;
#else
using _BeatmapSaveDataItemV3 = BeatmapSaveDataVersion3.BeatmapSaveData.BeatmapSaveDataItem;
#endif

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch]
    internal static class BeatmapDataFakeCount
    {
        [HarmonyPrefix]
#if LATEST
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader),
            nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJson))]
#else
        [HarmonyPatch(typeof(BeatmapDataLoader), nameof(BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData))]
#endif
        private static bool PrefixV3(
            ref BeatmapDataBasicInfo? __result,
#if LATEST
            string beatmapJson)
        {
#else
            BeatmapSaveData beatmapSaveData)
        {
            if (beatmapSaveData is not Version3CustomBeatmapSaveData customBeatmapSaveData
                || !(customBeatmapSaveData.beatmapCustomData.Get<List<object>>("_requirements")?.Cast<string>().Contains(CAPABILITY) ?? false))
            {
                return true;
            }
#endif

#if LATEST
            if (string.IsNullOrEmpty(beatmapJson))
            {
                __result = null;
                return false;
            }

            // double deserialization WOOHOO!!!
            // why did they stop storing the save data...
            BeatmapSaveData beatmapSaveData = Version3CustomBeatmapSaveData.Deserialize(beatmapJson);

            const string name = INTERNAL_FAKE_NOTE;
#else
            string name = new Version(customBeatmapSaveData.version).IsVersion2() ? V2_FAKE_NOTE : INTERNAL_FAKE_NOTE;
#endif

            int count = beatmapSaveData.colorNotes.Count(n => n.FakeConditionV3(name));
            int count2 = beatmapSaveData.obstacles.Count(n => n.FakeConditionV3(name));
            int count3 = beatmapSaveData.bombNotes.Count(n => n.FakeConditionV3(name));
#if LATEST
            __result = new BeatmapDataBasicInfo(4, count, count2, count3);
#else
            List<string> list = beatmapSaveData.basicEventTypesWithKeywords.data
                .Select(basicEventTypesForKeyword => basicEventTypesForKeyword.keyword).ToList();
            __result = new BeatmapDataBasicInfo(4, count, count2, count3, list);
#endif
            return false;
        }

        private static bool FakeConditionV3(this _BeatmapSaveDataItemV3 dataItem, string name)
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

#if LATEST
        [HarmonyPrefix]
        [HarmonyPatch(
            typeof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader),
            nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveDataJson))]
        private static bool PrefixV2(ref BeatmapDataBasicInfo? __result, string beatmapSaveDataJson)
        {
            if (string.IsNullOrEmpty(beatmapSaveDataJson))
            {
                __result = null;
                return false;
            }

            BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData beatmapSaveData = Version2_6_0AndEarlierCustomBeatmapSaveData.Deserialize(beatmapSaveDataJson);

            BeatmapSaveDataVersion2_6_0AndEarlier.NoteData[] totalnotes =
                beatmapSaveData.notes.Where(n => n.FakeConditionV2(V2_FAKE_NOTE)).ToArray();
            int count = totalnotes.Count(n => n.type != BeatmapSaveDataVersion2_6_0AndEarlier.NoteType.Bomb);
            int count2 = beatmapSaveData.obstacles.Count(n => n.FakeConditionV2(V2_FAKE_NOTE));
            int count3 = totalnotes.Length - count;
            __result = new BeatmapDataBasicInfo(4, count, count2, count3);
            return false;
        }

        private static bool FakeConditionV2(this BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveDataItem dataItem, string name)
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
#endif
    }
}
