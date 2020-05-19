using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
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
            for (int i = customNotes.Count - 1; i >= 0; i--)
            {
                dynamic dynData = customNotes[i].customData;
                IEnumerable<float?> _flip = ((List<object>)Trees.at(dynData, FLIP))?.Select(n => n.ToNullableFloat());
                float? _flipX = _flip?.ElementAtOrDefault(0);
                float? _flipY = _flip?.ElementAtOrDefault(1);
                if (_flipX.HasValue || _flipY.HasValue)
                {
                    if (_flipX.HasValue) dynData.flipLineIndex = _flipX.Value;
                    if (_flipY.HasValue) dynData.flipYSide = _flipY.Value;
                    customNotes.Remove(customNotes[i]);
                }
            }
            if (defaultFlip) customNotes.ForEach(c => c.customData.flipYSide = 0);
        }

        private static void ProcessBasicNotesInTimeRow(List<NoteData> basicNotes)
        {
            List<CustomNoteData> customNotes = basicNotes.Cast<CustomNoteData>().ToList();

            ProcessFlipData(customNotes);

            if (customNotes.Count == 2)
            {
                float[] lineIndexes = new float[2];
                float[] lineLayers = new float[2];
                for (int i = 0; i < customNotes.Count; i++)
                {
                    dynamic dynData = customNotes[i].customData;
                    IEnumerable<float?> _position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
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

        private static void ProcessNotesInTimeRow(List<NoteData> notes)
        {
            List<CustomNoteData> customNotes = notes.Cast<CustomNoteData>().ToList();
            ProcessFlipData(customNotes, false);
        }
    }

    // TODO: THIS IS JANK
    [HarmonyPatch(typeof(CustomLevelLoader))]
    [HarmonyPatch("LoadBeatmapDataBeatmapData")]
    internal class BeatmapDataLoaderGetBeatmapDataFromBeatmapSaveData
    {
        private static void Postfix(BeatmapData __result, StandardLevelInfoSaveData standardLevelInfoSaveData)
        {
            foreach (BeatmapLineData beatmapLineData in __result.beatmapLinesData)
            {
                foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                {
                    dynamic customData;
                    if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData) customData = beatmapObjectData;
                    else continue;
                    dynamic dynData = customData.customData;
                    // JANK JANK JANK
                    // TODO: REWRITE CJD SO I DONT HAVE TO DO THIS JANK
                    float bpm = standardLevelInfoSaveData.beatsPerMinute;
                    dynData.bpm = bpm;

                    // VARIABLE FUN FUN TIME YAY SO FUN YAYYYY
                    List<object> varPosition = Trees.at(dynData, VARIABLEPOSITION);
                    if (varPosition != null)
                    {
                        List<PositionData> positionData = new List<PositionData>();
                        foreach (object n in varPosition)
                        {
                            IDictionary<string, object> dictData = n as IDictionary<string, object>;

                            IEnumerable<float> startpos = ((List<object>)Trees.at(dictData, VARIABLESTARTPOS))?.Select(Convert.ToSingle);
                            IEnumerable<float> endpos = ((List<object>)Trees.at(dictData, VARIABLEENDPOS))?.Select(Convert.ToSingle);

                            float time = GetRealTimeFromBPMTime((float)Trees.at(dictData, VARIABLETIME), bpm);
                            float duration = (float)Trees.at(dictData, VARIABLEDURATION);
                            string easing = (string)Trees.at(dictData, VARIABLEEASING);
                            bool? relative = (bool?)Trees.at(dictData, VARIABLERELATIVE);
                            positionData.Add(new PositionData(time, duration, startpos, endpos, easing, relative));
                        }
                        dynData.varPosition = positionData;
                    }

                    RotationData.savedRotation = Quaternion.identity;

                    List<object> varRotation = Trees.at(dynData, VARIABLEROTATION);
                    if (varRotation != null)
                    {
                        List<RotationData> rotationData = new List<RotationData>();
                        foreach (object n in varRotation)
                        {
                            IDictionary<string, object> dictData = n as IDictionary<string, object>;

                            IEnumerable<float> startrot = ((List<object>)Trees.at(dictData, VARIABLESTARTROT))?.Select(Convert.ToSingle);
                            IEnumerable<float> endrot = ((List<object>)Trees.at(dictData, VARIABLEENDROT))?.Select(Convert.ToSingle);

                            float time = GetRealTimeFromBPMTime((float)Trees.at(dictData, VARIABLETIME), bpm);
                            float duration = (float)Trees.at(dictData, VARIABLEDURATION);
                            string easing = (string)Trees.at(dictData, VARIABLEEASING);
                            rotationData.Add(new RotationData(time, duration, startrot, endrot, easing));
                        }
                        dynData.varRotation = rotationData;
                    }

                    RotationData.savedRotation = Quaternion.identity;

                    List<object> varLocalRotation = Trees.at(dynData, VARIABLELOCALROTATION);
                    if (varLocalRotation != null)
                    {
                        List<RotationData> rotationData = new List<RotationData>();
                        foreach (object n in varLocalRotation)
                        {
                            IDictionary<string, object> dictData = n as IDictionary<string, object>;

                            IEnumerable<float> startrot = ((List<object>)Trees.at(dictData, VARIABLESTARTROT))?.Select(Convert.ToSingle);
                            IEnumerable<float> endrot = ((List<object>)Trees.at(dictData, VARIABLEENDROT))?.Select(Convert.ToSingle);

                            float time = GetRealTimeFromBPMTime((float)Trees.at(dictData, VARIABLETIME), bpm);
                            float duration = (float)Trees.at(dictData, VARIABLEDURATION);
                            string easing = (string)Trees.at(dictData, VARIABLEEASING);
                            rotationData.Add(new RotationData(time, duration, startrot, endrot, easing));
                        }
                        dynData.varLocalRotation = rotationData;
                    }
                }
            }
        }

        // TODO: use base game method
        private static float GetRealTimeFromBPMTime(float bpmTime, float bpm)
        {
            return bpmTime / bpm * 60f;
        }
    }
}