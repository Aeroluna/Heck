using System;
using System.Diagnostics;
using HarmonyLib;
using Heck.SettingsSetter;
using IPA.Logging;
using JetBrains.Annotations;

namespace Heck.HarmonyPatches
{
    [HarmonyPatch(typeof(MenuTransitionsHelper))]
    [HarmonyPatch(new[]
    {
        typeof(string),
        typeof(IDifficultyBeatmap),
        typeof(IPreviewBeatmapLevel),
        typeof(OverrideEnvironmentSettings),
        typeof(ColorScheme),
        typeof(GameplayModifiers),
        typeof(PlayerSpecificSettings),
        typeof(PracticeSettings),
        typeof(string),
        typeof(bool),
        typeof(Action),
        typeof(Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>)
    })]
    [HarmonyPatch("StartStandardLevel")]
    internal static class MenuTransitionsHelperStartStandardLevel
    {
        [UsedImplicitly]
        private static bool Prefix(
            MenuTransitionsHelper __instance,
            string gameMode,
            IDifficultyBeatmap difficultyBeatmap,
            IPreviewBeatmapLevel previewBeatmapLevel,
            OverrideEnvironmentSettings overrideEnvironmentSettings,
            ColorScheme overrideColorScheme,
            GameplayModifiers gameplayModifiers,
            PlayerSpecificSettings playerSpecificSettings,
            PracticeSettings practiceSettings,
            string backButtonText,
            bool useTestNoteCutSoundEffects,
            Action beforeSceneSwitchCallback,
            Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> levelFinishedCallback)
        {
            // In a perfect world I would patch SingePlayerLevelSelectionFlowCoordinator instead, but im a lazy mf
            // SO WEIRD STACK TRACE JANKINESS WE GO!!!
            StackTrace stackTrace = new();
            if (stackTrace.GetFrame(2).GetMethod().Name.Contains("SinglePlayerLevelSelectionFlowCoordinator"))
            {
                SettingsSetterViewController.StartStandardLevelParameters startStandardLevelParameters = new(
                    gameMode,
                    difficultyBeatmap,
                    previewBeatmapLevel,
                    overrideEnvironmentSettings,
                    overrideColorScheme,
                    gameplayModifiers,
                    playerSpecificSettings,
                    practiceSettings,
                    backButtonText,
                    useTestNoteCutSoundEffects,
                    beforeSceneSwitchCallback,
                    levelFinishedCallback);

                SettingsSetterViewController.Instance.Init(startStandardLevelParameters, __instance);
                return !SettingsSetterViewController.Instance.DoPresent;
            }

            Log.Logger.Log("Level started outside of SinglePlayerLevelSelectionFlowCoordinator, skipping settable settings.", Logger.Level.Trace);

            return true;
        }
    }

    [HarmonyPatch(typeof(MenuTransitionsHelper))]
    [HarmonyPatch("HandleMainGameSceneDidFinish")]
    internal static class MenuTransitionsHelperHandleMainGameSceneDidFinish
    {
        [UsedImplicitly]
        private static void Prefix(LevelCompletionResults levelCompletionResults)
        {
            if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.Restart)
            {
                SettingsSetterViewController.Instance.RestoreCached();
            }
        }
    }
}
