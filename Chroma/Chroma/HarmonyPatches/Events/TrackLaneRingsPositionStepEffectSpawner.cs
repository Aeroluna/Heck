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

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_0))
                .SetOpcodeAndAdvance(OpCodes.Ldarg_0)
                .Insert(
                    new CodeInstruction(OpCodes.Call, _getPrecisionStep),
                    new CodeInstruction(OpCodes.Stloc_0))
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
    }
}
