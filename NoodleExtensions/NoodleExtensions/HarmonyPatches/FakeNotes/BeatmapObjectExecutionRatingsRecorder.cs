namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(BeatmapObjectExecutionRatingsRecorder))]
    [HeckPatch("Update")]
    internal static class BeatmapObjectExecutionRatingsRecorderUpdate
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return FakeNoteHelper.ObstaclesTranspiler(instructions);
        }
    }
}
