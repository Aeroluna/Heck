using System;
using JetBrains.Annotations;

namespace Heck.PlayView
{
    public class StartMultiplayerLevelParameters : StartStandardLevelParameters
    {
        [UsedImplicitly]
        public StartMultiplayerLevelParameters(
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
#if LATEST
                null,
#endif
                gameplayModifiers,
                playerSpecificSettings,
                practiceSettings,
                backButtonText,
                useTestNoteCutSoundEffects,
                false,
                beforeSceneSwitchCallback,
                null,
#if LATEST
                null,
#endif
                null)
        {
            BeatmapDifficulty = beatmapDifficulty;
            BeatmapCharacteristic = beatmapCharacteristic;
            MultiplayerLevelFinishedCallback = levelFinishedCallback;
            DidDisconnectCallback = didDisconnectCallback;
        }

        public StartMultiplayerLevelParameters(StartMultiplayerLevelParameters original)
            : base(original)
        {
            BeatmapDifficulty = original.BeatmapDifficulty;
            BeatmapCharacteristic = original.BeatmapCharacteristic;
            MultiplayerLevelFinishedCallback = original.MultiplayerLevelFinishedCallback;
            DidDisconnectCallback = original.DidDisconnectCallback;
        }

        public BeatmapDifficulty BeatmapDifficulty { get; }

        public BeatmapCharacteristicSO BeatmapCharacteristic { get; }

        public Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData>? MultiplayerLevelFinishedCallback { get; }

        public Action<DisconnectedReason>? DidDisconnectCallback { get; }

        public override StartStandardLevelParameters Copy()
        {
            return new StartMultiplayerLevelParameters(this);
        }
    }
}
