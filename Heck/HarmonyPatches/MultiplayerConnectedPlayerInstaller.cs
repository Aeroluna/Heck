namespace Heck.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using static Heck.Plugin;

    [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller))]
    [HarmonyPatch("InstallBindings")]
    internal static class MultiplayerConnectedPlayerInstallerInstallBindings
    {
        private static readonly MethodInfo _exclude = SymbolExtensions.GetMethodInfo(() => Exclude(null));

        private static readonly FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.Accessor _beatmapObjectsDataAccessor = FieldAccessor<BeatmapLineData, List<BeatmapObjectData>>.GetAccessor("_beatmapObjectsData");

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

                    instructionList.Insert(i + 1, new CodeInstruction(OpCodes.Call, _exclude));
                }
            }

            if (!foundBeatmapData)
            {
                Logger.Log("Failed to find Call to CreateTransformedBeatmapData!", IPA.Logging.Logger.Level.Error);
            }

            return instructionList.AsEnumerable();
        }

        private static IReadonlyBeatmapData Exclude(IReadonlyBeatmapData result)
        {
            if (result is CustomBeatmapData customBeatmapData)
            {
                string[] excludedTypes = new string[]
                {
                    ANIMATETRACK,
                    ASSIGNPATHANIMATION,
                };

                customBeatmapData.customEventsData.RemoveAll(n => excludedTypes.Contains(n.type));

                customBeatmapData.customData.isMultiplayer = true;
            }

            return result;
        }
    }
}
