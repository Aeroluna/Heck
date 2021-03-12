﻿namespace NoodleExtensions.HarmonyPatches
{
    using System;
    using HarmonyLib;

    [HarmonyPatch(
        typeof(MissionLevelScenesTransitionSetupDataSO),
        new Type[] { typeof(string), typeof(IDifficultyBeatmap), typeof(IPreviewBeatmapLevel), typeof(MissionObjective[]), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(string) })]
    [HarmonyPatch("Init")]
    internal static class MissionLevelScenesTransitionSetupDataSOInit
    {
        private static void Postfix(IDifficultyBeatmap difficultyBeatmap, PlayerSpecificSettings playerSpecificSettings)
        {
            SceneTransitionHelper.Patch(difficultyBeatmap, playerSpecificSettings);
        }
    }
}
