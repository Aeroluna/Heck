using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using NoodleExtensions.Managers;
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

        private static readonly Type _sliderTailDataType = Type.GetType("BeatmapObjectsInTimeRowProcessor+SliderTailData,BeatmapCore")
                                                           ?? throw new InvalidOperationException();

        private static readonly Func<IEnumerable, IEnumerable> _getSliderTailDatas = (Func<IEnumerable, IEnumerable>)AccessTools.Method(typeof(Enumerable), nameof(Enumerable.OfType))
            .MakeGenericMethod(_sliderTailDataType)
            .CreateDelegate(typeof(Func<IEnumerable, IEnumerable>));

        private static readonly FieldInfo _sliderField = AccessTools.Field(_sliderTailDataType, "slider");

        [HarmonyTranspiler]
        [HarmonyPatch("HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice")]
        private static IEnumerable<CodeInstruction> ProcessColorNotesInTimeRowTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                // clamp
                .MatchForward(
                    true,
                    new CodeMatch(OpCodes.Ldloc_S),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Ldelem_Ref))
                .Insert(
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Ldc_I4_3),
                    new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => Mathf.Clamp(0, 0, 0))))

                // yeet slider processing
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldloc_1),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Stloc_S))
                .Insert(
                    new CodeInstruction(OpCodes.Ret))

                .InstructionEnumeration();
        }

        [HarmonyPostfix]
        [HarmonyPatch("HandleCurrentTimeSliceAllNotesAndSlidersDidFinishTimeSlice")]
        private static void ProcessAllNotesInTimeRowPatch(BeatmapObjectsInTimeRowProcessor __instance, int ____numberOfLines, object allObjectsTimeSlice)
        {
            float offset = ____numberOfLines / 2f;
            bool v2 = NoodleBeatmapObjectsInTimeRowProcessor.GetV2(__instance);
            IReadOnlyList<BeatmapDataItem> containerItems = AccessContainerItems<BeatmapDataItem>(allObjectsTimeSlice);
            IEnumerable<CustomNoteData> notesInTimeRow = containerItems.OfType<CustomNoteData>().ToArray();
            Dictionary<float, List<CustomNoteData>> notesInColumns = new();
            foreach (CustomNoteData noteData in notesInTimeRow)
            {
                CustomData customData = noteData.customData;

                IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                float lineIndex = position?.ElementAtOrDefault(0) + offset ?? noteData.lineIndex;
                float lineLayer = position?.ElementAtOrDefault(1) ?? (float)noteData.noteLineLayer;

                if (!notesInColumns.TryGetValue(lineIndex, out List<CustomNoteData> list))
                {
                    list = new List<CustomNoteData>(1);
                    notesInColumns.Add(lineIndex, list);
                }

                bool flag = false;
                for (int k = 0; k < list.Count; k++)
                {
                    IEnumerable<float?>? listPosition = list[k].customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET);
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
                float? flipX = flip?.ElementAtOrDefault(0) + offset;
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
                    customData[INTERNAL_FLIPLINEINDEX] = lineIndex;
                    customData[INTERNAL_FLIPYSIDE] = 0;
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

            IEnumerable<CustomSliderData> slidersInTimeRow = containerItems.OfType<CustomSliderData>().ToArray();
            foreach (CustomSliderData sliderData in slidersInTimeRow)
            {
                IEnumerable<float?>? headPosition = sliderData.customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                float headX = headPosition?.ElementAtOrDefault(0) + offset ?? sliderData.headLineIndex;
                float headY = headPosition?.ElementAtOrDefault(1) ?? (float)sliderData.headLineLayer;
                IEnumerable<float?>? tailPosition = sliderData.customData.GetNullableFloats(TAIL_NOTE_OFFSET)?.ToList();
                float tailX = tailPosition?.ElementAtOrDefault(0) + offset ?? sliderData.tailLineIndex;
                float tailY = tailPosition?.ElementAtOrDefault(1) ?? (float)sliderData.tailLineLayer;

                foreach (CustomNoteData noteData in notesInTimeRow)
                {
                    IEnumerable<float?>? notePosition = noteData.customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                    float noteX = notePosition?.ElementAtOrDefault(0) + offset ?? noteData.lineIndex;
                    float noteY = notePosition?.ElementAtOrDefault(1) ?? (float)noteData.noteLineLayer;

                    if (!Mathf.Approximately(headX, noteX) || !Mathf.Approximately(headY, noteY))
                    {
                        continue;
                    }

                    sliderData.SetHasHeadNote(true);
                    sliderData.customData[INTERNAL_STARTNOTELINELAYER] = noteData.customData[INTERNAL_STARTNOTELINELAYER];
                    if (sliderData.sliderType == SliderData.Type.Burst)
                    {
                        noteData.ChangeToBurstSliderHead();
                        if (noteData.cutDirection != sliderData.tailCutDirection)
                        {
                            continue;
                        }

                        Vector2 line = SpawnDataManager.Get2DNoteOffset(noteX, ____numberOfLines, noteY) -
                                       SpawnDataManager.Get2DNoteOffset(tailX, ____numberOfLines, tailY);
                        float num = noteData.cutDirection.Direction().SignedAngleToLine(line);
                        if (!(Mathf.Abs(num) <= 40f))
                        {
                            continue;
                        }

                        noteData.SetCutDirectionAngleOffset(num);
                        sliderData.SetCutDirectionAngleOffset(num, num);
                    }
                    else
                    {
                        noteData.ChangeToSliderHead();
                    }
                }

                if (!sliderData.customData.ContainsKey(INTERNAL_STARTNOTELINELAYER))
                {
                    sliderData.customData[INTERNAL_STARTNOTELINELAYER] = headY;
                }
            }

            foreach (object sliderTailData in _getSliderTailDatas(containerItems))
            {
                CustomSliderData sliderData = (CustomSliderData)_sliderField.GetValue(sliderTailData);
                IEnumerable<float?>? tailPosition = sliderData.customData.GetNullableFloats(TAIL_NOTE_OFFSET)?.ToList();
                float tailX = tailPosition?.ElementAtOrDefault(0) + offset ?? sliderData.tailLineIndex;
                float tailY = tailPosition?.ElementAtOrDefault(1) ?? (float)sliderData.tailLineLayer;
                foreach (CustomNoteData noteData in notesInTimeRow)
                {
                    IEnumerable<float?>? notePosition = noteData.customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                    float noteX = notePosition?.ElementAtOrDefault(0) + offset ?? noteData.lineIndex;
                    float noteY = notePosition?.ElementAtOrDefault(1) ?? (float)noteData.noteLineLayer;

                    if (Mathf.Approximately(tailX, noteX) && Mathf.Approximately(tailY, noteY))
                    {
                        continue;
                    }

                    sliderData.SetHasTailNote(true);
                    sliderData.customData[INTERNAL_TAILSTARTNOTELINELAYER] = noteData.customData[INTERNAL_STARTNOTELINELAYER];
                    sliderData.SetTailBeforeJumpLineLayer(noteData.beforeJumpNoteLineLayer);
                    noteData.ChangeToSliderTail();
                }

                if (!sliderData.customData.ContainsKey(INTERNAL_TAILSTARTNOTELINELAYER))
                {
                    sliderData.customData[INTERNAL_TAILSTARTNOTELINELAYER] = tailY;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("HandleCurrentTimeSliceColorNotesDidFinishTimeSlice")]
        private static void ProcessColorNotesInTimeRowPatch(BeatmapObjectsInTimeRowProcessor __instance, int ____numberOfLines, object currentTimeSlice)
        {
            float offset = ____numberOfLines / 2f;
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
                IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                lineIndexes[i] = position?.ElementAtOrDefault(0) + offset ?? colorNotesData[i].lineIndex;
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
