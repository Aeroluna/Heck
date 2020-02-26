using BS_Utils.Utilities;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Harmony;
using System.Collections.Generic;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPriority(Priority.Normal)]
    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("ProcessBasicNotesInTimeRow")]
    internal class BeatmapDataLoaderProcessBasicNotesInTimeRow
    {
        public static void Postfix(List<NoteData> notes)
        {
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                if (notes[i] is CustomNoteData customData)
                {
                    dynamic dynData = customData.customData;
                    float? _flipY = (float?)Trees.at(dynData, "_flipY");
                    if (_flipY.HasValue) notes[i].SetProperty("flipYSide", _flipY.Value, typeof(NoteData));
                    float? _flip = (float?)Trees.at(dynData, "_flipRow");
                    if (_flip.HasValue)
                    {
                        dynData.flipLineIndex = _flip.Value;
                        notes.Remove(notes[i]);
                    }
                }
                else return;
            }
            if (notes.Count != 2) return;
            List<float> lineIndexes = new List<float>();
            List<float> lineLayers = new List<float>();
            // CustomJSONData
            for (int i = 0; i < notes.Count; i++)
            {
                if (notes[i] is CustomNoteData customData)
                {
                    dynamic dynData = customData.customData;
                    lineIndexes.Add(((float?)Trees.at(dynData, "_startRow")).GetValueOrDefault(notes[i].lineIndex - 2));
                    lineLayers.Add(((float?)Trees.at(dynData, "_startHeight")).GetValueOrDefault((float)notes[i].noteLineLayer));
                }
            }
            if (notes[0].noteType != notes[1].noteType && ((notes[0].noteType == NoteType.NoteA && lineIndexes[0] > lineIndexes[1]) ||
                (notes[0].noteType == NoteType.NoteB && lineIndexes[0] < lineIndexes[1])))
            {
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i] is CustomNoteData customData)
                    {
                        // apparently I can use customData to store my own variables in noteData, neat
                        dynamic dynData = customData.customData;
                        dynData.flipLineIndex = lineIndexes[1 - i];

                        float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                        if ((lineIndexes[i] > lineIndexes[1 - i] && lineLayers[i] < lineLayers[1 - i]) || (lineIndexes[i] < lineIndexes[1 - i] &&
                            lineLayers[i] > lineLayers[1 - i]))
                        {
                            flipYSide *= -1f;
                        }
                        notes[i].SetProperty("flipYSide", flipYSide, typeof(NoteData));
                    }
                }
            }
            else
            {
                for (int i = 0; i < notes.Count; i++)
                {
                    notes[i].SetProperty("flipYSide", 0, typeof(NoteData));
                }
            }
        }
    }
}