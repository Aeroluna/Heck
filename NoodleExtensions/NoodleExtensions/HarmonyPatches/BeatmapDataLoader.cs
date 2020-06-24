using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using NoodleExtensions.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.HarmonyPatches
{
    internal class BeatmapDataLoaderProcessNotesInTimeRow
    {
        internal static void PatchBeatmapDataLoader(Harmony harmony)
        {
            Type NotesInTimeRowProcessor = Type.GetType("BeatmapDataLoader+NotesInTimeRowProcessor,Main");
            MethodInfo basicoriginal = AccessTools.Method(NotesInTimeRowProcessor, "ProcessBasicNotesInTimeRow");
            MethodInfo basicpostfix = SymbolExtensions.GetMethodInfo(() => ProcessBasicNotesInTimeRow(null));
            harmony.Patch(basicoriginal, postfix: new HarmonyMethod(basicpostfix));

            MethodInfo original = AccessTools.Method(NotesInTimeRowProcessor, "ProcessNotesInTimeRow");
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
                    if (flipX.HasValue) dynData.flipLineIndex = flipX.Value;
                    if (flipY.HasValue) dynData.flipYSide = flipY.Value;
                    customNotes.Remove(customNotes[i]);
                }
            }
            if (defaultFlip) customNotes.ForEach(c => c.customData.flipYSide = 0);
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
                    IEnumerable<float?> _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
                    float? startRow = _position?.ElementAtOrDefault(0);
                    float? startHeight = _position?.ElementAtOrDefault(1);

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
        }
    }

    [HarmonyPatch(typeof(BeatmapDataLoader))]
    [HarmonyPatch("GetBeatmapDataFromBeatmapSaveData")]
    internal class BeatmapDataLoaderGetBeatmapDataFromBeatmapSaveData
    {
        // TODO: account for base game bpm changes
        private static void Postfix(BeatmapData __result, float startBPM)
        {
            if (__result == null) return;

            if (__result is CustomBeatmapData customBeatmapData)
            {
                TrackManager trackManager = new TrackManager();
                foreach (BeatmapLineData beatmapLineData in __result.beatmapLinesData)
                {
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        dynamic customData;
                        if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData) customData = beatmapObjectData;
                        else continue;
                        dynamic dynData = customData.customData;
                        // for per object njs and spawn offset
                        float bpm = startBPM;
                        dynData.bpm = bpm;

                        // for epic tracks thing
                        string trackName = Trees.at(dynData, TRACK);
                        if (trackName != null) dynData.track = trackManager.AddToTrack(trackName);
                    }
                }
                customBeatmapData.customData.tracks = trackManager._tracks;

                IEnumerable<dynamic> pointDefinitions = (IEnumerable<dynamic>)Trees.at(customBeatmapData.customData, POINTDEFINITIONS);
                if (pointDefinitions == null) return;
                PointDataManager pointDataManager = new PointDataManager();
                foreach (dynamic pointDefintion in pointDefinitions)
                {
                    string pointName = Trees.at(pointDefintion, NAME);
                    PointData pointData = PointData.DynamicToPointData(Trees.at(pointDefintion, POINTS));
                    pointDataManager.AddPoint(pointName, pointData);
                }
                customBeatmapData.customData.pointDefinitions = pointDataManager._pointData;
            }
        }
    }
}
