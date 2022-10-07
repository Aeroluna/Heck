using System;
using JetBrains.Annotations;

namespace Heck.SettingsSetter
{
    internal class StartMultiplayerLevelParameters : StartStandardLevelParameters
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
            : base(
                gameMode,
                difficultyBeatmap,
                previewBeatmapLevel,
                null,
                overrideColorScheme,
                gameplayModifiers,
                playerSpecificSettings,
                practiceSettings,
                backButtonText,
                useTestNoteCutSoundEffects,
                false,
                beforeSceneSwitchCallback,
                null,
                null)
        {
            BeatmapDifficulty = beatmapDifficulty;
            BeatmapCharacteristic = beatmapCharacteristic;
            MultiplayerLevelFinishedCallback = levelFinishedCallback;
            DidDisconnectCallback = didDisconnectCallback;
        }

        internal StartMultiplayerLevelParameters(StartMultiplayerLevelParameters original)
            : base(original)
        {
            BeatmapDifficulty = original.BeatmapDifficulty;
            BeatmapCharacteristic = original.BeatmapCharacteristic;
            MultiplayerLevelFinishedCallback = original.MultiplayerLevelFinishedCallback;
            DidDisconnectCallback = original.DidDisconnectCallback;
        }

        internal BeatmapDifficulty BeatmapDifficulty { get; }

        internal BeatmapCharacteristicSO BeatmapCharacteristic { get; }

        internal Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData>? MultiplayerLevelFinishedCallback { get; }

        internal Action<DisconnectedReason>? DidDisconnectCallback { get; }

        internal override StartStandardLevelParameters Copy()
        {
            return new StartMultiplayerLevelParameters(this);
        }
    }
}
