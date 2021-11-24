using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches
{
    [HarmonyPatch(typeof(BeatmapObjectsInTimeRowProcessor))]
    [HarmonyPatch("ProcessAllNotesInTimeRow")]
    internal static class NotesInTimeRowProcessorProcessAllNotesInTimeRow
    {
        [UsedImplicitly]
        private static void Prefix(List<NoteData> notesInTimeRow)
        {
            if (notesInTimeRow.FirstOrDefault() is not CustomNoteData)
            {
                return;
            }

            List<CustomNoteData> notesToSetFlip = new();

            Dictionary<float, List<CustomNoteData>> notesInColumns = new();
            foreach (NoteData t in notesInTimeRow)
            {
                CustomNoteData noteData = (CustomNoteData)t;
                Dictionary<string, object?> dynData = noteData.customData;

                IEnumerable<float?>? position = dynData.GetNullableFloats(POSITION)?.ToList();
                float lineIndex = position?.ElementAtOrDefault(0) ?? (noteData.lineIndex - 2);
                float lineLayer = position?.ElementAtOrDefault(1) ?? (float)noteData.noteLineLayer;

                if (!notesInColumns.TryGetValue(lineIndex, out List<CustomNoteData> list))
                {
                    list = new List<CustomNoteData>(1);
                    notesInColumns.Add(lineIndex, list);
                }

                bool flag = false;
                for (int k = 0; k < list.Count; k++)
                {
                    IEnumerable<float?>? listPosition = list[k].customData.GetNullableFloats(POSITION);
                    float listLineLayer = listPosition?.ElementAtOrDefault(1) ?? (float)list[k].noteLineLayer;
                    if (!(listLineLayer > lineLayer))
                    {
                        continue;
                    }

                    list.Insert(k, noteData);
                    flag = true;
                    break;
                }

                if (!flag)
                {
                    list.Add(noteData);
                }

                // Flippy stuff
                IEnumerable<float?>? flip = dynData.GetNullableFloats(FLIP)?.ToList();
                float? flipX = flip?.ElementAtOrDefault(0);
                float? flipY = flip?.ElementAtOrDefault(1);
                if (flipX.HasValue || flipY.HasValue)
                {
                    if (flipX.HasValue)
                    {
                        dynData["flipLineIndex"] = flipX.Value;
                    }

                    if (flipY.HasValue)
                    {
                        dynData["flipYSide"] = flipY.Value;
                    }
                }
                else if (!dynData.ContainsKey("flipYSide"))
                {
                    notesToSetFlip.Add(noteData);
                }
            }

            foreach (KeyValuePair<float, List<CustomNoteData>> keyValue in notesInColumns)
            {
                List<CustomNoteData> list2 = keyValue.Value;
                for (int m = 0; m < list2.Count; m++)
                {
                    list2[m].customData["startNoteLineLayer"] = m;
                }
            }

            // Process flip data
            notesToSetFlip.ForEach(c => c.customData["flipYSide"] = 0);
        }
    }

    [HarmonyPatch(typeof(BeatmapObjectsInTimeRowProcessor))]
    [HarmonyPatch("ProcessColorNotesInTimeRow")]
    internal static class NotesInTimeRowProcessorProcessColorNotesInTimeRow
    {
        [UsedImplicitly]
        private static void Prefix(List<NoteData> colorNotesData)
        {
            if (colorNotesData.FirstOrDefault() is not CustomNoteData)
            {
                return;
            }

            int customNoteCount = colorNotesData.Count;
            if (customNoteCount != 2)
            {
                return;
            }

            float[] lineIndexes = new float[2];
            float[] lineLayers = new float[2];
            for (int i = 0; i < customNoteCount; i++)
            {
                Dictionary<string, object?> dynData = ((CustomNoteData)colorNotesData[i]).customData;
                IEnumerable<float?>? position = dynData.GetNullableFloats(POSITION)?.ToList();
                lineIndexes[i] = position?.ElementAtOrDefault(0) ?? (colorNotesData[i].lineIndex - 2);
                lineLayers[i] = position?.ElementAtOrDefault(1) ?? (float)colorNotesData[i].noteLineLayer;
            }

            if (colorNotesData[0].colorType == colorNotesData[1].colorType ||
                ((colorNotesData[0].colorType != ColorType.ColorA || !(lineIndexes[0] > lineIndexes[1])) &&
                 (colorNotesData[0].colorType != ColorType.ColorB || !(lineIndexes[0] < lineIndexes[1]))))
            {
                return;
            }

            {
                for (int i = 0; i < customNoteCount; i++)
                {
                    // apparently I can use customData to store my own variables in noteData, neat
                    // ^ comment from a very young and naive aero
                    Dictionary<string, object?> dynData = ((CustomNoteData)colorNotesData[i]).customData;
                    dynData["flipLineIndex"] = lineIndexes[1 - i];

                    float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                    if ((lineIndexes[i] > lineIndexes[1 - i] &&
                         lineLayers[i] < lineLayers[1 - i]) ||
                        (lineIndexes[i] < lineIndexes[1 - i] &&
                         lineLayers[i] > lineLayers[1 - i]))
                    {
                        flipYSide *= -1f;
                    }

                    dynData["flipYSide"] = flipYSide;
                }
            }
        }
    }
}
