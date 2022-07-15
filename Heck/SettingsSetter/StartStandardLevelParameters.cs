using System;
using JetBrains.Annotations;

namespace Heck.SettingsSetter
{
    internal class StartStandardLevelParameters
    {
        [UsedImplicitly]
        internal StartStandardLevelParameters(
            string gameMode,
            IDifficultyBeatmap difficultyBeatmap,
            IPreviewBeatmapLevel previewBeatmapLevel,
            OverrideEnvironmentSettings? overrideEnvironmentSettings,
            ColorScheme overrideColorScheme,
            GameplayModifiers gameplayModifiers,
            PlayerSpecificSettings playerSpecificSettings,
            PracticeSettings practiceSettings,
            string backButtonText,
            bool useTestNoteCutSoundEffects,
            bool startPaused,
            Action beforeSceneSwitchCallback,
            Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelFinishedCallback)
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
            StartPaused = startPaused;
            BeforeSceneSwitchCallback = beforeSceneSwitchCallback;
            LevelFinishedCallback = levelFinishedCallback;
        }

        internal StartStandardLevelParameters(StartStandardLevelParameters original)
        {
            GameMode = original.GameMode;
            DifficultyBeatmap = original.DifficultyBeatmap;
            PreviewBeatmapLevel = original.PreviewBeatmapLevel;
            OverrideEnvironmentSettings = original.OverrideEnvironmentSettings;
            OverrideColorScheme = original.OverrideColorScheme;
            GameplayModifiers = original.GameplayModifiers;
            PlayerSpecificSettings = original.PlayerSpecificSettings;
            PracticeSettings = original.PracticeSettings;
            BackButtonText = original.BackButtonText;
            UseTestNoteCutSoundEffects = original.UseTestNoteCutSoundEffects;
            StartPaused = original.StartPaused;
            BeforeSceneSwitchCallback = original.BeforeSceneSwitchCallback;
            LevelFinishedCallback = original.LevelFinishedCallback;
        }

        internal string GameMode { get; }

        internal IDifficultyBeatmap DifficultyBeatmap { get; }

        internal IPreviewBeatmapLevel PreviewBeatmapLevel { get; }

        internal OverrideEnvironmentSettings? OverrideEnvironmentSettings { get; set; }

        internal ColorScheme? OverrideColorScheme { get; set; }

        internal GameplayModifiers GameplayModifiers { get; set; }

        internal PlayerSpecificSettings PlayerSpecificSettings { get; set; }

        internal PracticeSettings PracticeSettings { get; }

        internal string BackButtonText { get; }

        internal bool UseTestNoteCutSoundEffects { get; }

        internal bool StartPaused { get; }

        internal Action? BeforeSceneSwitchCallback { get; }

        internal Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelFinishedCallback { get; }

        internal virtual StartStandardLevelParameters Copy()
        {
            return new StartStandardLevelParameters(this);
        }
    }
}
