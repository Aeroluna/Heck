using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("ProcessBasicNotesInTimeRow")]
    internal class BeatmapDataLoaderProcessBasicNotesInTimeRow
    {
        private static void Postfix(List<NoteData> notes)
        {
            List<CustomNoteData> customNotes = Trees.tryNull(() => notes.Cast<CustomNoteData>().ToList());
            if (customNotes == null) return;
            for (int i = customNotes.Count - 1; i >= 0; i--)
            {
                dynamic dynData = customNotes[i].customData;
                float?[] _flip = ((List<object>)Trees.at(dynData, FLIP))?.Select(n => n.ToNullableFloat()).ToArray();
                float? _flipX = _flip?.ElementAtOrDefault(0);
                float? _flipY = _flip?.ElementAtOrDefault(1);
                if (_flipX.HasValue || _flipY.HasValue)
                {
                    if (_flipX.HasValue) dynData.flipLineIndex = _flipX.Value;
                    if (_flipY.HasValue) dynData.flipYSide = _flipY.Value;
                    Logger.Log(_flipY?.ToString() ?? "null");
                    customNotes.Remove(customNotes[i]);
                }
            }

            customNotes.ForEach(c => c.customData.flipYSide = 0);

            if (customNotes.Count == 2)
            {
                float[] lineIndexes = new float[2];
                float[] lineLayers = new float[2];
                for (int i = 0; i < customNotes.Count; i++)
                {
                    dynamic dynData = customNotes[i].customData;
                    float?[] _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat()).ToArray();
                    float? _startRow = _position?.ElementAtOrDefault(0);
                    float? _startHeight = _position?.ElementAtOrDefault(1);

                    lineIndexes[i] = _startRow.GetValueOrDefault(customNotes[i].lineIndex - 2);
                    lineLayers[i] = _startHeight.GetValueOrDefault((float)customNotes[i].noteLineLayer);
                }
                if (customNotes[0].noteType != customNotes[1].noteType && ((customNotes[0].noteType == NoteType.NoteA && lineIndexes[0] > lineIndexes[1]) ||
                    (customNotes[0].noteType == NoteType.NoteB && lineIndexes[0] < lineIndexes[1])))
                {
                    for (int i = 0; i < customNotes.Count; i++)
                    {
                        // apparently I can use customData to store my own variables in noteData, neat
                        dynamic dynData = customNotes[i].customData;
                        dynData.flipLineIndex = lineIndexes[1 - i];

                        float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                        if ((lineIndexes[i] > lineIndexes[1 - i] && lineLayers[i] < lineLayers[1 - i]) || (lineIndexes[i] < lineIndexes[1 - i] &&
                            lineLayers[i] > lineLayers[1 - i]))
                        {
                            flipYSide *= -1f;
                        }
                        dynData.flipYSide = flipYSide;
                    }
                }
            }
        }
    }
}