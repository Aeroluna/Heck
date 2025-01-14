using System;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using static NoodleExtensions.NoodleController;
#if !PRE_V1_37_1
using BeatmapSaveDataVersion2_6_0AndEarlier;
#else
using System.Collections.Generic;
using BeatmapSaveDataVersion3;
using CustomJSONData;
#endif

namespace NoodleExtensions.HarmonyPatches.FakeNotes;

[HeckPatch]
internal static class BeatmapDataFakeCount
{
#if !PRE_V1_37_1
    // We only need to patch v2 maps for fake note counting,
    // v3 maps have fake objects in a separate array that doesn't get counted by vanilla!
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

        BeatmapSaveData beatmapSaveData = Version2_6_0AndEarlierCustomBeatmapSaveData.Deserialize(beatmapSaveDataJson);

        BeatmapSaveDataVersion2_6_0AndEarlier.NoteData[] totalNotes =
            beatmapSaveData.notes.Where(n => n.FakeConditionV2(V2_FAKE_NOTE)).ToArray();
        int count = totalNotes.Count(n => n.type != NoteType.Bomb);
        int count2 = beatmapSaveData.obstacles.Count(n => n.FakeConditionV2(V2_FAKE_NOTE));
        int count3 = totalNotes.Length - count;
        __result = new BeatmapDataBasicInfo(
            4,
            count,
#if !PRE_V1_39_1
            count,
#endif
            count2,
            count3);
        return false;
    }

    private static bool FakeConditionV2(this BeatmapSaveDataItem dataItem, string name)
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
#else
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BeatmapDataLoader), nameof(BeatmapDataLoader.GetBeatmapDataBasicInfoFromSaveData))]
    private static bool PrefixV3(
        ref BeatmapDataBasicInfo? __result,
        BeatmapSaveData beatmapSaveData)
    {
        if (beatmapSaveData is not Version3CustomBeatmapSaveData customBeatmapSaveData ||
            !(customBeatmapSaveData
                  .beatmapCustomData.Get<List<object>>("_requirements")
                  ?.Cast<string>()
                  .Contains(CAPABILITY) ??
              false))
        {
            return true;
        }

        string name = new Version(customBeatmapSaveData.version).IsVersion2() ? V2_FAKE_NOTE : INTERNAL_FAKE_NOTE;
        int count = beatmapSaveData.colorNotes.Count(n => n.FakeConditionV3(name));
        int count2 = beatmapSaveData.obstacles.Count(n => n.FakeConditionV3(name));
        int count3 = beatmapSaveData.bombNotes.Count(n => n.FakeConditionV3(name));
        List<string> list = beatmapSaveData
            .basicEventTypesWithKeywords.data
            .Select(basicEventTypesForKeyword => basicEventTypesForKeyword.keyword)
            .ToList();
        __result = new BeatmapDataBasicInfo(4, count, count2, count3, list);
        return false;
    }

    private static bool FakeConditionV3(this BeatmapSaveData.BeatmapSaveDataItem dataItem, string name)
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
