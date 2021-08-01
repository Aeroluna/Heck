namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using Heck;
    using IPA.Utilities;
    using static NoodleExtensions.Plugin;

    [HeckPatch(typeof(BeatmapDataTransformHelper))]
    [HeckPatch("CreateTransformedBeatmapData")]
    internal static class BeatmapDataTransformHelperCreateTransformedBeatmapData
    {
        private static readonly MethodInfo _reorderLineData = AccessTools.Method(typeof(BeatmapDataTransformHelperCreateTransformedBeatmapData), nameof(ReorderLineData));

        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

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
                float startHalfJumpDurationInBeats = 4;
                float maxHalfJumpDistance = 18;
                float moveDuration = 0.5f;

                for (int i = 0; i < customBeatmapData.beatmapLinesData.Count; i++)
                {
                    BeatmapLineData beatmapLineData = (BeatmapLineData)customBeatmapData.beatmapLinesData[i];
                    foreach (BeatmapObjectData beatmapObjectData in beatmapLineData.beatmapObjectsData)
                    {
                        Dictionary<string, object?> dynData = beatmapObjectData.GetDataForObject();

                        float noteJumpMovementSpeed = dynData.Get<float?>(NOTEJUMPSPEED) ?? GameplayCoreInstallerInstallBindings.CachedNoteJumpMovementSpeed;
                        float noteJumpStartBeatOffset = dynData.Get<float?>(NOTESPAWNOFFSET) ?? GameplayCoreInstallerInstallBindings.CachedNoteJumpStartBeatOffset;

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
                        .OrderBy(n => n.time - (float)(n.GetDataForObject()["aheadTime"] ?? throw new System.InvalidOperationException($"Could not get aheadTime for [{n.GetType().FullName}] at time [{n.time}].")))
                        .ToList();
                }

                return customBeatmapData;
            }

            Logger.Log("beatmapData was not CustomBeatmapData", IPA.Logging.Logger.Level.Error);
            return beatmapData;
        }

        private static void Postfix(IReadonlyBeatmapData __result)
        {
            // Skip if calling class is MultiplayerConnectPlayerInstaller
            StackTrace stackTrace = new StackTrace();
            if (!stackTrace.GetFrame(2).GetMethod().Name.Contains("MultiplayerConnectedPlayerInstaller"))
            {
                NoodleObjectDataManager.DeserializeBeatmapData(__result);
                Animation.NoodleEventDataManager.DeserializeBeatmapData(__result);
            }
        }
    }
}
