namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using NoodleExtensions.Animation;
    using static NoodleExtensions.Plugin;

    internal static class NotesInTimeRowProcessorProcessAllNotesInTimeRow
    {
        private static readonly Dictionary<float, float> _numberOfNotesInLines = new Dictionary<float, float>();

        private static void Postfix(List<NoteData> notes)
        {
            List<CustomNoteData> customNotes = notes.Cast<CustomNoteData>().ToList();

            // Process flip data
            int customNoteCount = customNotes.Count;
            for (int i = customNoteCount - 1; i >= 0; i--)
            {
                dynamic dynData = customNotes[i].customData;
                IEnumerable<float?> flip = ((List<object>)Trees.at(dynData, FLIP))?.Select(n => n.ToNullableFloat());
                float? flipX = flip?.ElementAtOrDefault(0);
                float? flipY = flip?.ElementAtOrDefault(1);
                if (flipX.HasValue || flipY.HasValue)
                {
                    if (flipX.HasValue)
                    {
                        dynData.flipLineIndex = flipX.Value;
                    }

                    if (flipY.HasValue)
                    {
                        dynData.flipYSide = flipY.Value;
                    }

                    customNotes.Remove(customNotes[i]);
                }
            }

            customNotes.ForEach(c => c.customData.flipYSide = 0);

            _numberOfNotesInLines.Clear();
            for (int i = 0; i < customNotes.Count; i++)
            {
                CustomNoteData noteData = customNotes[i];
                dynamic dynData = noteData.customData;

                IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                float lineIndex = position?.ElementAt(0) ?? noteData.lineIndex;
                float lineLayer = position?.ElementAt(1) ?? (float)noteData.noteLineLayer;
                if (_numberOfNotesInLines.TryGetValue(lineIndex, out float num))
                {
                    Dictionary<float, float> numberOfNotesInLines = _numberOfNotesInLines;
                    float num2 = Math.Max(numberOfNotesInLines[lineIndex], 0) + Math.Min(lineLayer, 1);
                    dynData.startNoteLineLayer = num2;
                    numberOfNotesInLines[lineIndex] = num2;
                }
                else
                {
                    float startLineLayer = Math.Min(lineLayer, 0);
                    _numberOfNotesInLines[lineIndex] = startLineLayer;
                    dynData.startNoteLineLayer = startLineLayer;
                }
            }
        }
    }

    internal static class NotesInTimeRowProcessorProcessColorNotesInTimeRow
    {
        private static void ProcessBasicNotesInTimeRow(List<NoteData> basicNotes)
        {
            List<CustomNoteData> customNotes = basicNotes.Cast<CustomNoteData>().ToList();

            int customNoteCount = customNotes.Count;
            if (customNoteCount == 2)
            {
                float[] lineIndexes = new float[2];
                float[] lineLayers = new float[2];
                for (int i = 0; i < customNoteCount; i++)
                {
                    dynamic dynData = customNotes[i].customData;
                    IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                    float? startRow = position?.ElementAtOrDefault(0);
                    float? startHeight = position?.ElementAtOrDefault(1);

                    lineIndexes[i] = startRow.GetValueOrDefault(customNotes[i].lineIndex - 2);
                    lineLayers[i] = startHeight.GetValueOrDefault((float)customNotes[i].noteLineLayer);
                }

                if (customNotes[0].colorType != customNotes[1].colorType && ((customNotes[0].colorType == ColorType.ColorA && lineIndexes[0] > lineIndexes[1]) ||
                    (customNotes[0].colorType == ColorType.ColorB && lineIndexes[0] < lineIndexes[1])))
                {
                    for (int i = 0; i < customNoteCount; i++)
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
