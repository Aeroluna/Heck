using System;
using JetBrains.Annotations;

namespace Heck.SettingsSetter
{
    internal struct StartStandardLevelParameters
    {
        [UsedImplicitly]
        internal StartStandardLevelParameters(
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
            GameMode = gameMode;
            DifficultyBeatmap = difficultyBeatmap;
            PreviewBeatmapLevel = previewBeatmapLevel;
            OverrideEnvironmentSettings = overrideEnvironmentSettings;
            OverrideColorScheme = overrideColorScheme;
            GameplayModifiers = gameplayModifiers;
            PlayerSpecificSettings = playerSpecificSettings;
            PracticeSettings = practiceSettings;
            BackButtonText = backButtonText;
            UseTestNoteCutSoundEffects = useTestNoteCutSoundEffects;
            BeforeSceneSwitchCallback = beforeSceneSwitchCallback;
            LevelFinishedCallback = levelFinishedCallback;
        }

        internal string GameMode { get; }

        internal IDifficultyBeatmap DifficultyBeatmap { get; }

        internal IPreviewBeatmapLevel PreviewBeatmapLevel { get; }

        internal OverrideEnvironmentSettings OverrideEnvironmentSettings { get; set; }

        internal ColorScheme? OverrideColorScheme { get; set; }

        internal GameplayModifiers GameplayModifiers { get; set; }

        internal PlayerSpecificSettings PlayerSpecificSettings { get; set; }

        internal PracticeSettings PracticeSettings { get; }

        internal string BackButtonText { get; }

        internal bool UseTestNoteCutSoundEffects { get; }

        internal Action? BeforeSceneSwitchCallback { get; }

        internal Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelFinishedCallback { get; }
    }
}
