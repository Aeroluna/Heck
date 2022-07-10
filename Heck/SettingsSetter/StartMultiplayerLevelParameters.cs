using JetBrains.Annotations;
using System;

namespace Heck.SettingsSetter
{
    internal struct StartMultiplayerLevelParameters
    {
        [UsedImplicitly]
        internal StartMultiplayerLevelParameters(
            string gameMode,
            IPreviewBeatmapLevel previewBeatmapLevel,
            BeatmapDifficulty beatmapDifficulty,
            BeatmapCharacteristicSO beatmapCharacteristic,
            IDifficultyBeatmap difficultyBeatmap,
            ColorScheme overrideColorScheme,
            GameplayModifiers gameplayModifiers,
            PlayerSpecificSettings playerSpecificSettings,
            PracticeSettings practiceSettings,
            string backButtonText,
            bool useTestNoteCutSoundEffects,
            Action beforeSceneSwitchCallback,
            Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData> levelFinishedCallback,
            Action<DisconnectedReason> didDisconnectCallback)
        {
            GameMode = gameMode;
            PreviewBeatmapLevel = previewBeatmapLevel;
            BeatmapDifficulty = beatmapDifficulty;
            BeatmapCharacteristic = beatmapCharacteristic;
            DifficultyBeatmap = difficultyBeatmap;
            OverrideColorScheme = overrideColorScheme;
            GameplayModifiers = gameplayModifiers;
            PlayerSpecificSettings = playerSpecificSettings;
            PracticeSettings = practiceSettings;
            BackButtonText = backButtonText;
            UseTestNoteCutSoundEffects = useTestNoteCutSoundEffects;
            BeforeSceneSwitchCallback = beforeSceneSwitchCallback;
            LevelFinishedCallback = levelFinishedCallback;
            DidDisconnectCallback = didDisconnectCallback;
        }

        internal string GameMode { get; }

        internal IPreviewBeatmapLevel PreviewBeatmapLevel { get; }

        internal BeatmapDifficulty BeatmapDifficulty { get; }

        internal BeatmapCharacteristicSO BeatmapCharacteristic { get; }

        internal IDifficultyBeatmap DifficultyBeatmap { get; }

        internal ColorScheme? OverrideColorScheme { get; set; }

        internal GameplayModifiers GameplayModifiers { get; set; }

        internal PlayerSpecificSettings PlayerSpecificSettings { get; set; }

        internal PracticeSettings PracticeSettings { get; }

        internal string BackButtonText { get; }

        internal bool UseTestNoteCutSoundEffects { get; }

        internal Action? BeforeSceneSwitchCallback { get; }

        internal Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData>? LevelFinishedCallback { get; }

        internal Action<DisconnectedReason>? DidDisconnectCallback { get; }
    }
}
