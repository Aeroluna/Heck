using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(BeatmapObjectsInTimeRowProcessor))]
    internal static class ProcessNotesNoodleDataInTimeRow
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> _getItemsMethods = new();

        [HarmonyTranspiler]
        [HarmonyPatch("HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice")]
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
        [HarmonyPatch("HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice")]
        private static void ProcessAllNotesInTimeRowPatch(BeatmapObjectsInTimeRowProcessor __instance, object allObjectsTimeSlice)
        {
            bool v2 = NoodleBeatmapObjectsInTimeRowProcessor.GetV2(__instance);
            List<CustomNoteData> notesToSetFlip = new();
            IEnumerable<NoteData> notesInTimeRow = AccessContainerItems<BeatmapDataItem>(allObjectsTimeSlice).OfType<NoteData>();
            Dictionary<float, List<CustomNoteData>> notesInColumns = new();
            foreach (NoteData t in notesInTimeRow)
            {
                if (t is not CustomNoteData noteData)
                {
                    continue;
                }

                CustomData customData = noteData.customData;

                IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : POSITION)?.ToList();
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
                    IEnumerable<float?>? listPosition = list[k].customData.GetNullableFloats(v2 ? V2_POSITION : POSITION);
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
                IEnumerable<float?>? flip = customData.GetNullableFloats(v2 ? V2_FLIP : FLIP)?.ToList();
                float? flipX = flip?.ElementAtOrDefault(0);
                float? flipY = flip?.ElementAtOrDefault(1);
                if (flipX.HasValue || flipY.HasValue)
                {
                    if (flipX.HasValue)
                    {
                        customData[INTERNAL_FLIPLINEINDEX] = flipX.Value;
                    }

                    if (flipY.HasValue)
                    {
                        customData[INTERNAL_FLIPYSIDE] = flipY.Value;
                    }
                }
                else if (!customData.ContainsKey(INTERNAL_FLIPYSIDE))
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
            notesToSetFlip.ForEach(c =>
            {
                c.customData[INTERNAL_FLIPYSIDE] = 0;
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch("HandleCurrentTimeSliceColorNotesDidFinishTimeSlice")]
        private static void ProcessColorNotesInTimeRowPatch(BeatmapObjectsInTimeRowProcessor __instance, object currentTimeSlice)
        {
            IReadOnlyList<NoteData> colorNotesData = AccessContainerItems<NoteData>(currentTimeSlice);
            int customNoteCount = colorNotesData.Count;
            if (customNoteCount != 2)
            {
                return;
            }

            bool v2 = NoodleBeatmapObjectsInTimeRowProcessor.GetV2(__instance);

            float[] lineIndexes = new float[2];
            float[] lineLayers = new float[2];
            for (int i = 0; i < customNoteCount; i++)
            {
                if (colorNotesData[i] is not CustomNoteData noteData)
                {
                    continue;
                }

                CustomData customData = noteData.customData;
                IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : POSITION)?.ToList();
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
                    CustomData customData = noteData.customData;
                    customData[INTERNAL_FLIPLINEINDEX] = lineIndexes[1 - i];

                    float flipYSide = (lineIndexes[i] > lineIndexes[1 - i]) ? 1 : -1;
                    if ((lineIndexes[i] > lineIndexes[1 - i] &&
                         lineLayers[i] < lineLayers[1 - i]) ||
                        (lineIndexes[i] < lineIndexes[1 - i] &&
                         lineLayers[i] > lineLayers[1 - i]))
                    {
                        flipYSide *= -1f;
                    }

                    customData[INTERNAL_FLIPYSIDE] = flipYSide;
                }
            }
        }

        private static IReadOnlyList<T> AccessContainerItems<T>(object timeSliceContainer)
        {
            // ReSharper disable once InvertIf
            if (!_getItemsMethods.TryGetValue(typeof(T), out MethodInfo getItems))
            {
                Type genericType = Type.GetType("BeatmapObjectsInTimeRowProcessor+TimeSliceContainer`1,BeatmapCore")
                                   ?? throw new InvalidOperationException("Unable to resolve [TimeSliceContainer] type.");
                Type constructed = genericType.MakeGenericType(typeof(T));
                getItems = AccessTools.PropertyGetter(constructed, "items");
                _getItemsMethods[typeof(T)] = getItems;
            }

            return (IReadOnlyList<T>)getItems.Invoke(timeSliceContainer, null);
        }
    }
}
