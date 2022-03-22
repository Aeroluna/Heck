using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using UnityEngine;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(BeatmapObjectsInTimeRowProcessor))]
    internal static class ProcessNotesNoodleDataInTimeRow
    {
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(BeatmapObjectsInTimeRowProcessor.HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice))]
        private static IEnumerable<CodeInstruction> PepegaClamp(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Ldelem_Ref))
                .Insert(
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Ldc_I4_3),
                    new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => Mathf.Clamp(0, 0, 0))))
                .InstructionEnumeration();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapObjectsInTimeRowProcessor.HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice))]
        private static void ProcessAllNotesInTimeRowPatch(BeatmapObjectsInTimeRowProcessor.TimeSliceContainer<BeatmapDataItem> allObjectsTimeSlice)
        {
            List<CustomNoteData> notesToSetFlip = new();
            IEnumerable<NoteData> notesInTimeRow = allObjectsTimeSlice.items.OfType<NoteData>();
            Dictionary<float, List<CustomNoteData>> notesInColumns = new();
            foreach (NoteData t in notesInTimeRow)
            {
                if (t is not CustomNoteData noteData)
                {
                    continue;
                }

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
                        dynData[INTERNAL_FLIPLINEINDEX] = flipX.Value;
                    }

                    if (flipY.HasValue)
                    {
                        dynData[INTERNAL_FLIPYSIDE] = flipY.Value;
                    }
                }
                else if (!dynData.ContainsKey(INTERNAL_FLIPYSIDE))
                {
                    notesToSetFlip.Add(noteData);
                }
            }

            foreach (KeyValuePair<float, List<CustomNoteData>> keyValue in notesInColumns)
            {
                List<CustomNoteData> list2 = keyValue.Value;
                for (int m = 0; m < list2.Count; m++)
                {
                    list2[m].customData[INTERNAL_STARTNOTELINELAYER] = m;
                }
            }

            // Process flip data
            notesToSetFlip.ForEach(c => c.customData[INTERNAL_FLIPYSIDE] = 0);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BeatmapObjectsInTimeRowProcessor.HandleCurrentTimeSliceColorNotesDidFinishTimeSlice))]
        private static void ProcessColorNotesInTimeRowPatch(BeatmapObjectsInTimeRowProcessor.TimeSliceContainer<NoteData> currentTimeSlice)
        {
            IReadOnlyList<NoteData> colorNotesData = currentTimeSlice.items;
            int customNoteCount = colorNotesData.Count;
            if (customNoteCount != 2)
            {
                return;
            }

            float[] lineIndexes = new float[2];
            float[] lineLayers = new float[2];
            for (int i = 0; i < customNoteCount; i++)
            {
                if (colorNotesData[i] is not CustomNoteData noteData)
                {
                    continue;
                }

                Dictionary<string, object?> dynData = noteData.customData;
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
                    if (colorNotesData[i] is not CustomNoteData noteData)
                    {
                        continue;
                    }

                    // apparently I can use customData to store my own variables in noteData, neat
                    // ^ comment from a very young and naive aero
                    Dictionary<string, object?> dynData = noteData.customData;
                    dynData[INTERNAL_FLIPLINEINDEX] = lineIndexes[1 - i];

                    float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                    if ((lineIndexes[i] > lineIndexes[1 - i] &&
                         lineLayers[i] < lineLayers[1 - i]) ||
                        (lineIndexes[i] < lineIndexes[1 - i] &&
                         lineLayers[i] > lineLayers[1 - i]))
                    {
                        flipYSide *= -1f;
                    }

                    dynData[INTERNAL_FLIPYSIDE] = flipYSide;
                }
            }
        }
    }
}
