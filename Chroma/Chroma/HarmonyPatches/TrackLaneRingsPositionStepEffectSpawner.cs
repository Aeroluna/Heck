namespace Chroma.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma;
    using Heck;
    using HarmonyLib;
    using static ChromaEventDataManager;

    [HeckPatch(typeof(TrackLaneRingsPositionStepEffectSpawner))]
    [HeckPatch("HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger")]
    internal static class TrackLaneRingsPositionStepEffectSpawnerHandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger
    {
        private static readonly MethodInfo _getPrecisionStep = SymbolExtensions.GetMethodInfo(() => GetPrecisionStep(0, null));

        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundStLoc = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundStLoc &&
                    instructionList[i].opcode == OpCodes.Stloc_0)
                {
                    foundStLoc = true;

                    instructionList[i].opcode = OpCodes.Ldarg_1;
                    CodeInstruction[] codeInstructions = new CodeInstruction[]
                    {
                        new CodeInstruction(OpCodes.Call, _getPrecisionStep),
                        new CodeInstruction(OpCodes.Stloc_0),
                    };
                    instructionList.InsertRange(i + 1, codeInstructions);
                }
            }

            if (!foundStLoc)
            {
                Plugin.Logger.Log("Failed to find stloc.0!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static float GetPrecisionStep(float @default, BeatmapEventData beatmapEventData)
        {
            ChromaRingStepEventData chromaData = TryGetEventData<ChromaRingStepEventData>(beatmapEventData);
            if (chromaData != null)
            {
                if (chromaData.Step.HasValue)
                {
                    return chromaData.Step.Value;
                }
            }

            return @default;
        }
    }
}
