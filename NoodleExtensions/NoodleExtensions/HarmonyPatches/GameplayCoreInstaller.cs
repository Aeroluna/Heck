namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Heck;

    [HeckPatch(typeof(GameplayCoreInstaller))]
    [HeckPatch("InstallBindings")]
    internal static class GameplayCoreInstallerInstallBindings
    {
        private static readonly MethodInfo _cacheNoteJumpValues = SymbolExtensions.GetMethodInfo(() => CacheNoteJumpValues(0, 0));

        internal static float CachedNoteJumpMovementSpeed { get; private set; }

        internal static float CachedNoteJumpStartBeatOffset { get; private set; }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = instructions.ToList();
            bool foundBeatmapData = false;
            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!foundBeatmapData &&
                    instructionList[i].opcode == OpCodes.Call &&
                    ((MethodInfo)instructionList[i].operand).Name == "CreateTransformedBeatmapData")
                {
                    foundBeatmapData = true;

                    instructionList.Insert(i - 8, new CodeInstruction(OpCodes.Ldloc_S, 9));
                    instructionList.Insert(i - 7, new CodeInstruction(OpCodes.Ldloc_S, 10));
                    instructionList.Insert(i - 6, new CodeInstruction(OpCodes.Call, _cacheNoteJumpValues));
                }
            }

            if (!foundBeatmapData)
            {
                Plugin.Logger.Log("Failed to find Call to CreateTransformedBeatmapData!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static void CacheNoteJumpValues(float defaultNoteJumpMovementSpeed, float defaultNoteJumpStartBeatOffset)
        {
            CachedNoteJumpMovementSpeed = defaultNoteJumpMovementSpeed;
            CachedNoteJumpStartBeatOffset = defaultNoteJumpStartBeatOffset;
        }
    }
}
