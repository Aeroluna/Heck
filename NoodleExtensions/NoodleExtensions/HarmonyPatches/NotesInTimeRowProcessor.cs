namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using static NoodleExtensions.Plugin;

    [HarmonyPatch(typeof(NotesInTimeRowProcessor))]
    [HarmonyPatch("ProcessAllNotesInTimeRow")]
    internal static class NotesInTimeRowProcessorProcessAllNotesInTimeRow
    {
        private static readonly Dictionary<float, List<CustomNoteData>> _notesInColumns = new Dictionary<float, List<CustomNoteData>>();

        private static void Postfix(List<NoteData> notes)
        {
            List<CustomNoteData> customNotes = notes.Cast<CustomNoteData>().ToList();

            _notesInColumns.Clear();
            for (int i = 0; i < customNotes.Count; i++)
            {
                CustomNoteData noteData = customNotes[i];

                IEnumerable<float?> position = ((List<object>)Trees.at(noteData.customData, POSITION))?.Select(n => n.ToNullableFloat());
                float lineIndex = position?.ElementAtOrDefault(0) ?? (noteData.lineIndex - 2);
                float lineLayer = position?.ElementAtOrDefault(1) ?? (float)noteData.noteLineLayer;

                if (!_notesInColumns.TryGetValue(lineIndex, out List<CustomNoteData> list))
                {
                    list = new List<CustomNoteData>();
                    _notesInColumns.Add(lineIndex, list);
                }

                bool flag = false;
                for (int k = 0; k < list.Count; k++)
                {
                    IEnumerable<float?> listPosition = ((List<object>)Trees.at(list[k].customData, POSITION))?.Select(n => n.ToNullableFloat());
                    float listLineLayer = listPosition?.ElementAtOrDefault(1) ?? (float)list[k].noteLineLayer;
                    if (listLineLayer > lineLayer)
                    {
                        list.Insert(k, noteData);
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    list.Add(noteData);
                }
            }

            foreach (KeyValuePair<float, List<CustomNoteData>> keyValue in _notesInColumns)
            {
                List<CustomNoteData> list2 = keyValue.Value;
                for (int m = 0; m < list2.Count; m++)
                {
                    list2[m].customData.startNoteLineLayer = m;
                }
            }

            // Process flip data
            List<CustomNoteData> flipNotes = new List<CustomNoteData>(customNotes);
            for (int i = flipNotes.Count - 1; i >= 0; i--)
            {
                dynamic dynData = flipNotes[i].customData;
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

                    flipNotes.Remove(flipNotes[i]);
                }
            }

            flipNotes.ForEach(c => c.customData.flipYSide = 0);
        }
    }

    [HarmonyPatch(typeof(NotesInTimeRowProcessor))]
    [HarmonyPatch("ProcessColorNotesInTimeRow")]
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
                    lineIndexes[i] = position?.ElementAtOrDefault(0) ?? (customNotes[i].lineIndex - 2);
                    lineLayers[i] = position?.ElementAtOrDefault(1) ?? (float)customNotes[i].noteLineLayer;
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
