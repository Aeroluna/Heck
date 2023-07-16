using System.Collections.Generic;
using BeatmapSaveDataVersion3;
using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class FakeNotesJSON
    {
        [CustomJSONDataDeserializer.JSONDeserializer("fakeColorNotes")]
        private static bool HandleFakeNotes(
            CustomBeatmapSaveData.SaveDataCustomDatas customData,
            List<BeatmapSaveData.ColorNoteData> colorNotes,
            JsonTextReader reader)
        {
            if (CheckRequirement(customData))
            {
                return true;
            }

            List<BeatmapSaveData.ColorNoteData> newNotes = new();
            CustomBeatmapSaveData.DeserializeColorNoteArray(reader, newNotes);
            newNotes.ForEach(n => ((CustomBeatmapSaveData.ColorNoteData)n).customData[INTERNAL_FAKE_NOTE] = true);
            colorNotes.AddRange(newNotes);
            return false;
        }

        [CustomJSONDataDeserializer.JSONDeserializer("fakeBombNotes")]
        private static bool HandleFakeBombs(
            CustomBeatmapSaveData.SaveDataCustomDatas customData,
            List<BeatmapSaveData.BombNoteData> bombNotes,
            JsonTextReader reader)
        {
            if (CheckRequirement(customData))
            {
                return true;
            }

            List<BeatmapSaveData.BombNoteData> newBombs = new();
            CustomBeatmapSaveData.DeserializeBombNoteArray(reader, newBombs);
            newBombs.ForEach(n => ((CustomBeatmapSaveData.BombNoteData)n).customData[INTERNAL_FAKE_NOTE] = true);
            bombNotes.AddRange(newBombs);
            return false;
        }

        [CustomJSONDataDeserializer.JSONDeserializer("fakeObstacles")]
        private static bool HandleFakeObstacles(
            CustomBeatmapSaveData.SaveDataCustomDatas customData,
            List<BeatmapSaveData.ObstacleData> obstacles,
            JsonTextReader reader)
        {
            if (CheckRequirement(customData))
            {
                return true;
            }

            List<BeatmapSaveData.ObstacleData> newObstacles = new();
            CustomBeatmapSaveData.DeserializeObstacleArray(reader, newObstacles);
            newObstacles.ForEach(n => ((CustomBeatmapSaveData.ObstacleData)n).customData[INTERNAL_FAKE_NOTE] = true);
            obstacles.AddRange(newObstacles);
            return false;
        }

        [CustomJSONDataDeserializer.JSONDeserializer("fakeBurstSliders")]
        private static bool HandleFakeBurstSliders(
            CustomBeatmapSaveData.SaveDataCustomDatas customData,
            List<BeatmapSaveData.BurstSliderData> burstSliders,
            JsonTextReader reader)
        {
            if (CheckRequirement(customData))
            {
                return true;
            }

            List<BeatmapSaveData.BurstSliderData> newBurstSliders = new();
            CustomBeatmapSaveData.DeserializeBurstSliderArray(reader, newBurstSliders);
            newBurstSliders.ForEach(n => ((CustomBeatmapSaveData.BurstSliderData)n).customData[INTERNAL_FAKE_NOTE] = true);
            burstSliders.AddRange(newBurstSliders);
            return false;
        }

        private static bool CheckRequirement(CustomBeatmapSaveData.SaveDataCustomDatas customData)
        {
            return !(customData.beatmapCustomData.Get<List<object>>("_requirements")?.Contains(CAPABILITY) ?? false);
        }
    }
}
