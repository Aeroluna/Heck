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

    internal static class BeatmapDataLoaderProcessNotesInTimeRow
    {
        private static readonly Dictionary<float, float> _numberOfNotesInLines = new Dictionary<float, float>();

        internal static void PatchBeatmapDataLoader(Harmony harmony)
        {
            Type notesInTimeRowProcessor = Type.GetType("BeatmapDataLoader+NotesInTimeRowProcessor,Main");
            MethodInfo basicoriginal = AccessTools.Method(notesInTimeRowProcessor, "ProcessBasicNotesInTimeRow");
            MethodInfo basicpostfix = SymbolExtensions.GetMethodInfo(() => ProcessBasicNotesInTimeRow(null));
            harmony.Patch(basicoriginal, postfix: new HarmonyMethod(basicpostfix));

            MethodInfo original = AccessTools.Method(notesInTimeRowProcessor, "ProcessNotesInTimeRow");
            MethodInfo postfix = SymbolExtensions.GetMethodInfo(() => ProcessNotesInTimeRow(null));
            harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }

        private static void ProcessFlipData(List<CustomNoteData> customNotes, bool defaultFlip = true)
        {
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

            if (defaultFlip)
            {
                customNotes.ForEach(c => c.customData.flipYSide = 0);
            }
        }

        private static void ProcessBasicNotesInTimeRow(List<NoteData> basicNotes)
        {
            List<CustomNoteData> customNotes = basicNotes.Cast<CustomNoteData>().ToList();

            ProcessFlipData(customNotes);

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

                if (customNotes[0].noteType != customNotes[1].noteType && ((customNotes[0].noteType == NoteType.NoteA && lineIndexes[0] > lineIndexes[1]) ||
                    (customNotes[0].noteType == NoteType.NoteB && lineIndexes[0] < lineIndexes[1])))
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

        private static void ProcessNotesInTimeRow(List<NoteData> notes)
        {
            List<CustomNoteData> customNotes = notes.Cast<CustomNoteData>().ToList();

            ProcessFlipData(customNotes, false);

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

    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromBeatmapSaveData")]
    internal static class BeatmapDataLoaderGetBeatmapDataFromBeatmapSaveData
    {
        // TODO: account for base game bpm changes
#pragma warning disable SA1313
        private static void Postfix(BeatmapData __result, float startBPM)
#pragma warning restore SA1313
        {
            if (__result == null)
            {
                return;
            }

            if (__result is CustomBeatmapData customBeatmapData)
            {
                TrackManager trackManager = new TrackManager(customBeatmapData);
                foreach (BeatmapLineData beatmapLineData in customBeatmapData.beatmapLinesData)
                {
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        dynamic customData;
                        if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData)
                        {
                            customData = beatmapObjectData;
                        }
                        else
                        {
                            continue;
                        }

                        dynamic dynData = customData.customData;

                        // for per object njs and spawn offset
                        float bpm = startBPM;
                        dynData.bpm = bpm;

                        // for epic tracks thing
                        string trackName = Trees.at(dynData, TRACK);
                        if (trackName != null)
                        {
                            dynData.track = trackManager.AddTrack(trackName);
                        }
                    }
                }

                customBeatmapData.customData.tracks = trackManager.Tracks;

                IEnumerable<dynamic> pointDefinitions = (IEnumerable<dynamic>)Trees.at(customBeatmapData.customData, POINTDEFINITIONS);
                if (pointDefinitions == null)
                {
                    return;
                }

                PointDefinitionManager pointDataManager = new PointDefinitionManager();
                foreach (dynamic pointDefintion in pointDefinitions)
                {
                    string pointName = Trees.at(pointDefintion, NAME);
                    PointDefinition pointData = PointDefinition.DynamicToPointData(Trees.at(pointDefintion, POINTS));
                    pointDataManager.AddPoint(pointName, pointData);
                }

                customBeatmapData.customData.pointDefinitions = pointDataManager.PointData;
            }
        }
    }
}
