using System.Collections.Generic;
using BeatmapSaveDataVersion3;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Newtonsoft.Json;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class FakeNotesJSON
    {
        [CustomJSONDataDeserializer.JSONDeserializer("fakeColorNotes")]
        private static bool HandleFakeNotes(CustomBeatmapSaveData.SaveDataCustomDatas customData, List<BeatmapSaveData.ColorNoteData> colorNotes, JsonTextReader reader)
        {
            if (CheckRequirement(customData))
            {
                return true;
            }

            reader.ReadObjectArray(() =>
            {
                CustomBeatmapSaveData.ColorNoteData data = CustomBeatmapSaveData.DeserializeColorNote(reader);
                data.customData[INTERNAL_FAKE_NOTE] = true;
                colorNotes.Add(data);
            });
            return false;
        }

        [CustomJSONDataDeserializer.JSONDeserializer("fakeBombNotes")]
        private static bool HandleFakeBombs(CustomBeatmapSaveData.SaveDataCustomDatas customData, List<BeatmapSaveData.BombNoteData> bombNotes, JsonTextReader reader)
        {
            if (CheckRequirement(customData))
            {
                return true;
            }

            reader.ReadObjectArray(() =>
            {
                CustomBeatmapSaveData.BombNoteData data = CustomBeatmapSaveData.DeserializeBombNote(reader);
                data.customData[INTERNAL_FAKE_NOTE] = true;
                bombNotes.Add(data);
            });
            return false;
        }

        [CustomJSONDataDeserializer.JSONDeserializer("fakeObstacles")]
        private static bool HandleFakeObstacles(CustomBeatmapSaveData.SaveDataCustomDatas customData, List<BeatmapSaveData.ObstacleData> obstacles, JsonTextReader reader)
        {
            if (CheckRequirement(customData))
            {
                return true;
            }

            reader.ReadObjectArray(() =>
            {
                CustomBeatmapSaveData.ObstacleData data = CustomBeatmapSaveData.DeserializeObstacle(reader);
                data.customData[INTERNAL_FAKE_NOTE] = true;
                obstacles.Add(data);
            });
            return false;
        }

        private static bool CheckRequirement(CustomBeatmapSaveData.SaveDataCustomDatas customData)
        {
            return !(customData.beatmapCustomData.Get<List<object>>("_requirements")?.Contains(CAPABILITY) ?? false);
        }
    }
}
