namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using Chroma;
    using HarmonyLib;

    // We do all this because we only want this to run on the local player
    [HarmonyPatch(typeof(GameplayCoreInstaller))]
    [HarmonyPatch("InstallBindings")]
    internal static class GameplayCoreInstallerInstallBindings
    {
        private static readonly MethodInfo _readCustomData = SymbolExtensions.GetMethodInfo(() => ReadCustomData(null));

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

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _readCustomData));
                }
            }

            if (!foundBeatmapData)
            {
                ChromaLogger.Log("Failed to find Call to CreateTransformedBeatmapData!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static IReadonlyBeatmapData ReadCustomData(IReadonlyBeatmapData result)
        {
            ChromaObjectDataManager.DeserializeBeatmapData(result);
            ChromaEventDataManager.DeserializeBeatmapData(result);
            return result;
        }
    }
}
