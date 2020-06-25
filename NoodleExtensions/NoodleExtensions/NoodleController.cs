namespace NoodleExtensions
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

    public static class NoodleController
    {
        private static List<NoodlePatchData> _noodlePatches;

        public static void ToggleNoodlePatches(bool value, BeatmapData beatmapData, float defaultNoteJumpMovementSpeed, float defaultNoteJumpStartBeatOffset)
        {
            if (value)
            {
                if (!Harmony.HasAnyPatches(HARMONYID))
                {
                    _noodlePatches.ForEach(n => _harmonyInstance.Patch(
                        n.OriginalMethod,
                        n.Prefix != null ? new HarmonyMethod(n.Prefix) : null,
                        n.Postfix != null ? new HarmonyMethod(n.Postfix) : null,
                        n.Transpiler != null ? new HarmonyMethod(n.Transpiler) : null));
                }

                // var njs/spawn offset stuff below

                // there is some ambiguity with these variables but who frikkin cares
                float startHalfJumpDurationInBeats = 4;
                float maxHalfJumpDistance = 18;
                float moveDuration = 0.5f;

                foreach (BeatmapLineData beatmapLineData in beatmapData.beatmapLinesData)
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
                            return;
                        }

                        dynamic dynData = customData.customData;
                        float noteJumpMovementSpeed = (float?)Trees.at(dynData, NOTEJUMPSPEED) ?? defaultNoteJumpMovementSpeed;
                        float noteJumpStartBeatOffset = (float?)Trees.at(dynData, SPAWNOFFSET) ?? defaultNoteJumpStartBeatOffset;

                        // how do i not repeat this in a reasonable way
                        float num = 60f / (float)Trees.at(dynData, "bpm");
                        float num2 = startHalfJumpDurationInBeats;
                        while (noteJumpMovementSpeed * num * num2 > maxHalfJumpDistance)
                        {
                            num2 /= 2f;
                        }

                        num2 += noteJumpStartBeatOffset;
                        if (num2 < 1f)
                        {
                            num2 = 1f;
                        }

                        float jumpDuration = num * num2 * 2f;
                        dynData.aheadTime = moveDuration + (jumpDuration * 0.5f);
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
            else
            {
                _harmonyInstance.UnpatchAll(HARMONYID);
            }
        }

        internal static void InitNoodlePatches()
        {
            if (_noodlePatches == null)
            {
                _noodlePatches = new List<NoodlePatchData>();
                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    object[] noodleattributes = type.GetCustomAttributes(typeof(NoodlePatch), true);
                    if (noodleattributes.Length > 0)
                    {
                        Type declaringType = null;
                        List<string> methodNames = new List<string>();
                        foreach (NoodlePatch n in noodleattributes)
                        {
                            if (n.DeclaringType != null)
                            {
                                declaringType = n.DeclaringType;
                            }

                            if (n.MethodName != null)
                            {
                                methodNames.Add(n.MethodName);
                            }
                        }

                        if (declaringType == null || !methodNames.Any())
                        {
                            throw new ArgumentException("Type or Method Name not described");
                        }

                        MethodInfo prefix = AccessTools.Method(type, "Prefix");
                        MethodInfo postfix = AccessTools.Method(type, "Postfix");
                        MethodInfo transpiler = AccessTools.Method(type, "Transpiler");

                        methodNames.ForEach(n => _noodlePatches.Add(new NoodlePatchData(AccessTools.Method(declaringType, n), prefix, postfix, transpiler)));
                    }
                }
            }
        }
    }
}
