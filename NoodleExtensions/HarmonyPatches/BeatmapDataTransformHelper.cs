using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using IPA.Utilities;
using JetBrains.Annotations;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions.HarmonyPatches
{
    [HeckPatch(typeof(BeatmapDataTransformHelper))]
    [HeckPatch("CreateTransformedBeatmapData")]
    internal static class BeatmapDataTransformHelperCreateTransformedBeatmapData
    {
        private static readonly MethodInfo _reorderLineData = AccessTools.Method(typeof(BeatmapDataTransformHelperCreateTransformedBeatmapData), nameof(ReorderLineData));

        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Bne_Un))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, _reorderLineData),
                    new CodeInstruction(OpCodes.Stloc_0),
                    new CodeInstruction(OpCodes.Ldloc_0)) // Replace the opcode we replace
                .InstructionEnumeration();
        }

        private static IReadonlyBeatmapData ReorderLineData(IReadonlyBeatmapData beatmapData)
        {
            if (beatmapData is CustomBeatmapData)
            {
                CustomBeatmapData customBeatmapData = (CustomBeatmapData)beatmapData.GetCopy();

                // there is some ambiguity with these variables but who frikkin cares
                const float startHalfJumpDurationInBeats = 4;
                const float maxHalfJumpDistance = 18;
                const float moveDuration = 0.5f;

                foreach (IReadonlyBeatmapLineData t in customBeatmapData.beatmapLinesData)
                {
                    BeatmapLineData beatmapLineData = (BeatmapLineData)t;
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        Dictionary<string, object?> dynData = beatmapObjectData.GetDataForObject();

                        float noteJumpMovementSpeed = dynData.Get<float?>(NOTE_JUMP_SPEED) ?? GameplayCoreInstallerInstallBindings.CachedNoteJumpMovementSpeed;
                        float noteJumpStartBeatOffset = dynData.Get<float?>(NOTE_SPAWN_OFFSET) ?? GameplayCoreInstallerInstallBindings.CachedNoteJumpStartBeatOffset;

                        // how do i not repeat this in a reasonable way
                        float num = 60f / dynData.Get<float>("bpm");
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
                        dynData["aheadTime"] = moveDuration + (jumpDuration * 0.5f);
                    }

                    _beatmapObjectsDataAccessor(ref beatmapLineData) = beatmapLineData.beatmapObjectsData
                        .OrderBy(n => n.time - (float)(n.GetDataForObject()["aheadTime"] ?? throw new InvalidOperationException($"Could not get aheadTime for [{n.GetType().FullName}] at time [{n.time}].")))
                        .ToList();
                }

                return customBeatmapData;
            }

            Log.Logger.Log("beatmapData was not CustomBeatmapData", IPA.Logging.Logger.Level.Error);
            return beatmapData;
        }
    }
}
