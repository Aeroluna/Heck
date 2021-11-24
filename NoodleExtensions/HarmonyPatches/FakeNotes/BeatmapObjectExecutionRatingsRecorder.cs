using System.Collections.Generic;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(typeof(BeatmapObjectExecutionRatingsRecorder))]
    [HeckPatch("Update")]
    internal static class BeatmapObjectExecutionRatingsRecorderUpdate
    {
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return FakeNoteHelper.ObstaclesTranspiler(instructions);
        }
    }
}
