namespace Heck.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using static Heck.Plugin;

    [HarmonyPatch(typeof(MultiplayerConnectedPlayerInstaller))]
    [HarmonyPatch("InstallBindings")]
    internal static class MultiplayerConnectedPlayerInstallerInstallBindings
    {
        private static readonly MethodInfo _createTransformedBeatmapData = AccessTools.Method(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData));

        private static readonly MethodInfo _exclude = AccessTools.Method(typeof(MultiplayerConnectedPlayerInstallerInstallBindings), nameof(Exclude));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Call, _createTransformedBeatmapData))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Call, _exclude))
                .InstructionEnumeration();
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

                customBeatmapData.customData["isMultiplayer"] = true;
            }

            return result;
        }
    }
}
