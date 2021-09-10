namespace Heck.HarmonyPatches
{
    using System;
    using HarmonyLib;
    using Heck.SettingsSetter;

    [HarmonyPatch(typeof(MenuTransitionsHelper))]
    [HarmonyPatch(new Type[]
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
        typeof(Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>),
    })]
    [HarmonyPatch("StartStandardLevel")]
    internal static class MenuTransitionsHelperStartStandardLevel
    {
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
            SettingsSetterViewController.StartStandardLevelParameters startStandardLevelParameters = new SettingsSetterViewController.StartStandardLevelParameters(
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
            if (SettingsSetterViewController.Instance.DoPresent)
            {
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MenuTransitionsHelper))]
    [HarmonyPatch("HandleMainGameSceneDidFinish")]
    internal static class MenuTransitionsHelperHandleMainGameSceneDidFinish
    {
        private static void Prefix(LevelCompletionResults levelCompletionResults)
        {
            if (levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.Restart)
            {
                SettingsSetterViewController.Instance.RestoreCached();
            }
        }
    }
}
