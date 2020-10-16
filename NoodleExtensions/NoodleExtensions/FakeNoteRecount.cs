namespace NoodleExtensions
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using static Plugin;

    internal static class FakeNoteRecount
    {
        private static readonly PropertyAccessor<BeatmapData, int>.Setter _cuttableNotesTypeSetter = PropertyAccessor<BeatmapData, int>.GetSetter("cuttableNotesType");
        private static readonly PropertyAccessor<BeatmapData, int>.Setter _obstaclesCountSetter = PropertyAccessor<BeatmapData, int>.GetSetter("obstaclesCount");
        private static readonly PropertyAccessor<BeatmapData, int>.Setter _bombsCountSetter = PropertyAccessor<BeatmapData, int>.GetSetter("bombsCount");

        internal static void OnCustomBeatmapDataCreated(CustomBeatmapData customBeatmapData)
        {
            IEnumerable<string> requirements = ((List<object>)Trees.at(customBeatmapData.beatmapCustomData, "_requirements"))?.Cast<string>();
            bool noodleRequirement = requirements?.Contains(CAPABILITY) ?? false;

            if (noodleRequirement)
            {
                // Recount for fake notes
                int notesCount = 0;
                int obstaclesCount = 0;
                int bombsCount = 0;
                IReadOnlyList<IReadonlyBeatmapLineData> beatmapLinesData = customBeatmapData.beatmapLinesData;
                for (int i = 0; i < beatmapLinesData.Count; i++)
                {
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLinesData[i].beatmapObjectsData)
                    {
                        if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData)
                        {
                            dynamic customObjectData = beatmapObjectData;
                            dynamic dynData = customObjectData.customData;
                            bool? fake = Trees.at(dynData, FAKENOTE);
                            if (fake.HasValue && fake.Value)
                            {
                                continue;
                            }
                        }

                        if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Note)
                        {
                            ColorType noteType = ((NoteData)beatmapObjectData).colorType;
                            if (noteType == ColorType.ColorA || noteType == ColorType.ColorB)
                            {
                                notesCount++;
                            }
                            else if (noteType == ColorType.None)
                            {
                                bombsCount++;
                            }
                        }
                        else if (beatmapObjectData.beatmapObjectType == BeatmapObjectType.Obstacle)
                        {
                            obstaclesCount++;
                        }
                    }
                }

                BeatmapData beatmapData = customBeatmapData as BeatmapData;
                _cuttableNotesTypeSetter(ref beatmapData, notesCount);
                _obstaclesCountSetter(ref beatmapData, obstaclesCount);
                _bombsCountSetter(ref beatmapData, bombsCount);
            }
        }
    }
}
