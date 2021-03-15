namespace Chroma.HarmonyPatches
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;

    [HarmonyPatch(
        typeof(MissionLevelScenesTransitionSetupDataSO),
        new Type[] { typeof(string), typeof(IDifficultyBeatmap), typeof(IPreviewBeatmapLevel), typeof(MissionObjective[]), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(string) })]
    [HarmonyPatch("Init")]
    internal static class MissionLevelScenesTransitionSetupDataSOInit
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return SceneTransitionHelper.Transpiler(instructions);
        }

        private static void Postfix(IDifficultyBeatmap difficultyBeatmap)
        {
            SceneTransitionHelper.Patch(difficultyBeatmap);
        }
    }
}
