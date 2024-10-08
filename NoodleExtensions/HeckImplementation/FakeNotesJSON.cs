﻿using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;
using static NoodleExtensions.NoodleController;
#if !PRE_V1_37_1
using _BombNoteData = BeatmapSaveDataVersion3.BombNoteData;
using _BurstSliderData = BeatmapSaveDataVersion3.BurstSliderData;
using _ColorNoteData = BeatmapSaveDataVersion3.ColorNoteData;
using _ObstacleData = BeatmapSaveDataVersion3.ObstacleData;
#else
using _BombNoteData = BeatmapSaveDataVersion3.BeatmapSaveData.BombNoteData;
using _BurstSliderData = BeatmapSaveDataVersion3.BeatmapSaveData.BurstSliderData;
using _ColorNoteData = BeatmapSaveDataVersion3.BeatmapSaveData.ColorNoteData;
using _ObstacleData = BeatmapSaveDataVersion3.BeatmapSaveData.ObstacleData;
#endif

namespace NoodleExtensions;

internal class FakeNotesJson
{
    [CustomJSONDataDeserializer.JSONDeserializer("fakeBombNotes")]
    private static bool HandleFakeBombs(
#if PRE_V1_37_1
        Version3CustomBeatmapSaveData.SaveDataCustomDatas customData,
#endif
        List<_BombNoteData> bombNotes,
        JsonTextReader reader)
    {
#if PRE_V1_37_1
        if (CheckRequirement(customData))
        {
            return true;
        }
#endif

        List<_BombNoteData> newBombs = [];
        Version3CustomBeatmapSaveData.DeserializeBombNoteArray(reader, newBombs);
        newBombs.ForEach(
            n => ((Version3CustomBeatmapSaveData.BombNoteSaveData)n).customData[INTERNAL_FAKE_NOTE] = true);
        bombNotes.AddRange(newBombs);
        return false;
    }

    [CustomJSONDataDeserializer.JSONDeserializer("fakeBurstSliders")]
    private static bool HandleFakeBurstSliders(
#if PRE_V1_37_1
        Version3CustomBeatmapSaveData.SaveDataCustomDatas customData,
#endif
        List<_BurstSliderData> burstSliders,
        JsonTextReader reader)
    {
#if PRE_V1_37_1
        if (CheckRequirement(customData))
        {
            return true;
        }
#endif

        List<_BurstSliderData> newBurstSliders = [];
        Version3CustomBeatmapSaveData.DeserializeBurstSliderArray(reader, newBurstSliders);
        newBurstSliders.ForEach(
            n => ((Version3CustomBeatmapSaveData.BurstSliderSaveData)n).customData[INTERNAL_FAKE_NOTE] = true);
        burstSliders.AddRange(newBurstSliders);
        return false;
    }

    [CustomJSONDataDeserializer.JSONDeserializer("fakeColorNotes")]
    private static bool HandleFakeNotes(
#if PRE_V1_37_1
        Version3CustomBeatmapSaveData.SaveDataCustomDatas customData,
#endif
        List<_ColorNoteData> colorNotes,
        JsonTextReader reader)
    {
#if PRE_V1_37_1
        if (CheckRequirement(customData))
        {
            return true;
        }
#endif

        List<_ColorNoteData> newNotes = [];
        Version3CustomBeatmapSaveData.DeserializeColorNoteArray(reader, newNotes);
        newNotes.ForEach(
            n => ((Version3CustomBeatmapSaveData.ColorNoteSaveData)n).customData[INTERNAL_FAKE_NOTE] = true);
        colorNotes.AddRange(newNotes);
        return false;
    }

    [CustomJSONDataDeserializer.JSONDeserializer("fakeObstacles")]
    private static bool HandleFakeObstacles(
#if PRE_V1_37_1
        Version3CustomBeatmapSaveData.SaveDataCustomDatas customData,
#endif
        List<_ObstacleData> obstacles,
        JsonTextReader reader)
    {
#if PRE_V1_37_1
        if (CheckRequirement(customData))
        {
            return true;
        }
#endif

        List<_ObstacleData> newObstacles = [];
        Version3CustomBeatmapSaveData.DeserializeObstacleArray(reader, newObstacles);
        newObstacles.ForEach(
            n => ((Version3CustomBeatmapSaveData.ObstacleSaveData)n).customData[INTERNAL_FAKE_NOTE] = true);
        obstacles.AddRange(newObstacles);
        return false;
    }

#if PRE_V1_37_1
    private static bool CheckRequirement(Version3CustomBeatmapSaveData.SaveDataCustomDatas customData)
    {
        return !(customData.beatmapCustomData.Get<List<object>>("_requirements")?.Contains(CAPABILITY) ?? false);
    }
#endif
}
