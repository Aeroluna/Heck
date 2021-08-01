namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(GameplayCoreInstaller))]
    [HeckPatch("InstallBindings")]
    internal static class GameplayCoreInstallerInstallBindings
    {
        private static readonly MethodInfo _createTransformedBeatmapData = AccessTools.Method(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData));

        private static readonly MethodInfo _cacheNoteJumpValues = AccessTools.Method(typeof(GameplayCoreInstallerInstallBindings), nameof(CacheNoteJumpValues));

        internal static float CachedNoteJumpMovementSpeed { get; private set; }

        internal static float CachedNoteJumpStartBeatOffset { get; private set; }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createTransformedBeatmapData))
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, 9),
                    new CodeInstruction(OpCodes.Ldloc_S, 10),
                    new CodeInstruction(OpCodes.Call, _cacheNoteJumpValues))
                .InstructionEnumeration();
        }

        private static void CacheNoteJumpValues(float defaultNoteJumpMovementSpeed, float defaultNoteJumpStartBeatOffset)
        {
            CachedNoteJumpMovementSpeed = defaultNoteJumpMovementSpeed;
            CachedNoteJumpStartBeatOffset = defaultNoteJumpStartBeatOffset;
        }
    }
}
