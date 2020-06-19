using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using NoodleExtensions.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions
{
    internal static class NoodleController
    {
        internal static float? ToNullableFloat(this object @this)
        {
            if (@this == null || @this == DBNull.Value) return null;
            return Convert.ToSingle(@this);
        }

        internal static void InitNoodlePatches()
        {
            if (NoodlePatches == null)
            {
                NoodlePatches = new List<NoodlePatchData>();
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    object[] noodleattributes = type.GetCustomAttributes(typeof(NoodlePatch), true);
                    if (noodleattributes.Length > 0)
                    {
                        Type declaringType = null;
                        List<string> methodNames = new List<string>();
                        foreach (NoodlePatch n in noodleattributes)
                        {
                            if (n.declaringType != null) declaringType = n.declaringType;
                            if (n.methodName != null) methodNames.Add(n.methodName);
                        }
                        if (declaringType == null || !methodNames.Any()) throw new ArgumentException("Type or Method Name not described");

                        MethodInfo prefix = AccessTools.Method(type, "Prefix");
                        MethodInfo postfix = AccessTools.Method(type, "Postfix");
                        MethodInfo transpiler = AccessTools.Method(type, "Transpiler");

                        methodNames.ForEach(n => NoodlePatches.Add(new NoodlePatchData(AccessTools.Method(declaringType, n), prefix, postfix, transpiler)));
                    }
                }
            }
        }

        private static List<NoodlePatchData> NoodlePatches;

        public static void ToggleNoodlePatches(bool value, BeatmapData beatmapData, float defaultNoteJumpMovementSpeed, float defaultNoteJumpStartBeatOffset)
        {
            if (value)
            {
                if (!Harmony.HasAnyPatches(HARMONYID))
                    NoodlePatches.ForEach(n => harmony.Patch(n.originalMethod,
                        n.prefix != null ? new HarmonyMethod(n.prefix) : null,
                        n.postfix != null ? new HarmonyMethod(n.postfix) : null,
                        n.transpiler != null ? new HarmonyMethod(n.transpiler) : null));

                // var njs/spawn offset stuff below

                // there is some ambiguity with these variables but who frikkin cares
                float _startHalfJumpDurationInBeats = 4;
                float _maxHalfJumpDistance = 18;
                float _moveDuration = 0.5f;

                foreach (BeatmapLineData beatmapLineData in beatmapData.beatmapLinesData)
                {
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        dynamic customData;
                        if (beatmapObjectData is CustomObstacleData || beatmapObjectData is CustomNoteData) customData = beatmapObjectData;
                        else return;
                        dynamic dynData = customData.customData;
                        float noteJumpMovementSpeed = (float?)Trees.at(dynData, NOTEJUMPSPEED) ?? defaultNoteJumpMovementSpeed;
                        float noteJumpStartBeatOffset = (float?)Trees.at(dynData, SPAWNOFFSET) ?? defaultNoteJumpStartBeatOffset;

                        // how do i not repeat this in a reasonable way
                        float num = 60f / (float)Trees.at(dynData, "bpm");
                        float num2 = _startHalfJumpDurationInBeats;
                        while (noteJumpMovementSpeed * num * num2 > _maxHalfJumpDistance)
                        {
                            num2 /= 2f;
                        }
                        num2 += noteJumpStartBeatOffset;
                        if (num2 < 1f) num2 = 1f;
                        float _jumpDuration = num * num2 * 2f;
                        dynData.aheadTime = _moveDuration + _jumpDuration * 0.5f;
                    }
                    beatmapLineData.beatmapObjectsData = beatmapLineData.beatmapObjectsData.OrderBy(n => n.time - (float)((dynamic)n).customData.aheadTime).ToArray();
                }

                if (beatmapData is CustomBeatmapData customBeatmapData)
                {
                    Dictionary<string, Track> tracks = Trees.at(customBeatmapData.customData, "tracks");
                    if (tracks != null)
                    {
                        foreach (KeyValuePair<string, Track> track in tracks)
                        {
                            track.Value.ResetVariables();
                        }
                    }
                }
            }
            else harmony.UnpatchAll(HARMONYID);
        }
    }
}
