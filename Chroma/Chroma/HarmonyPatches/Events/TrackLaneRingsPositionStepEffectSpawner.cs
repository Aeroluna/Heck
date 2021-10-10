namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma;
    using HarmonyLib;
    using Heck;
    using static Chroma.ChromaEventDataManager;

    [HeckPatch(typeof(TrackLaneRingsPositionStepEffectSpawner))]
    [HeckPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class TrackLaneRingsPositionStepEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        private static readonly MethodInfo _getPrecisionStep = AccessTools.Method(typeof(TrackLaneRingsPositionStepEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger), nameof(GetPrecisionStep));
        private static readonly FieldInfo _moveSpeedField = AccessTools.Field(typeof(TrackLaneRingsPositionStepEffectSpawner), "_moveSpeed");
        private static readonly MethodInfo _getPrecisionSpeed = AccessTools.Method(typeof(TrackLaneRingsPositionStepEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger), nameof(GetPrecisionSpeed));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
                .SetOpcodeAndAdvance(OpCodes.Ldarg_1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Call, _getPrecisionStep),
                    new CodeInstruction(OpCodes.Stloc_0))
                .MatchForward(false, new CodeMatch(OpCodes.Ldfld, _moveSpeedField))
                .Advance(1)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Call, _getPrecisionSpeed))
                .InstructionEnumeration();
        }

        private static float GetPrecisionStep(float @default, BeatmapEventData beatmapEventData)
        {
            ChromaEventData? chromaData = TryGetEventData(beatmapEventData);
            if (chromaData != null && chromaData.Step.HasValue)
            {
                return chromaData.Step.Value;
            }

            return @default;
        }

        private static float GetPrecisionSpeed(float @default, BeatmapEventData beatmapEventData)
        {
            ChromaEventData? chromaData = TryGetEventData(beatmapEventData);
            if (chromaData != null && chromaData.Speed.HasValue)
            {
                return chromaData.Speed.Value;
            }

            return @default;
        }
    }
}
