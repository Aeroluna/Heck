using System;
using JetBrains.Annotations;

namespace Heck.PlayView
{
    public class StartStandardLevelParameters
    {
        [UsedImplicitly]
        public StartStandardLevelParameters(
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
            Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelFinishedCallback,
            Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelRestartedCallback)
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
            LevelRestartedCallback = levelRestartedCallback;
        }

        public StartStandardLevelParameters(StartStandardLevelParameters original)
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
            LevelRestartedCallback = original.LevelRestartedCallback;
        }

        public string GameMode { get; }

        public IDifficultyBeatmap DifficultyBeatmap { get; }

        public IPreviewBeatmapLevel PreviewBeatmapLevel { get; }

        public OverrideEnvironmentSettings? OverrideEnvironmentSettings { get; set; }

        public ColorScheme? OverrideColorScheme { get; set; }

        public GameplayModifiers GameplayModifiers { get; set; }

        public PlayerSpecificSettings PlayerSpecificSettings { get; set; }

        public PracticeSettings PracticeSettings { get; }

        public string BackButtonText { get; }

        public bool UseTestNoteCutSoundEffects { get; }

        public bool StartPaused { get; }

        public Action? BeforeSceneSwitchCallback { get; }

        public Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelFinishedCallback { get; }

        public Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelRestartedCallback { get; }

        public virtual StartStandardLevelParameters Copy()
        {
            return new StartStandardLevelParameters(this);
        }
    }
}
