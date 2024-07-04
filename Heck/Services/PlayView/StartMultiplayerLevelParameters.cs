using System;
using JetBrains.Annotations;

namespace Heck.PlayView
{
    public class StartMultiplayerLevelParameters : StartStandardLevelParameters
    {
        [UsedImplicitly]
        public StartMultiplayerLevelParameters(
            string gameMode,
#if LATEST
            in BeatmapKey beatmapKey,
            BeatmapLevel beatmapLevel,
            IBeatmapLevelData beatmapLevelData,
#else
            IPreviewBeatmapLevel previewBeatmapLevel,
            BeatmapDifficulty beatmapDifficulty,
            BeatmapCharacteristicSO beatmapCharacteristic,
            IDifficultyBeatmap difficultyBeatmap,
#endif
            ColorScheme overrideColorScheme,
            GameplayModifiers gameplayModifiers,
            PlayerSpecificSettings playerSpecificSettings,
            PracticeSettings? practiceSettings,
            string backButtonText,
            bool useTestNoteCutSoundEffects,
            Action beforeSceneSwitchCallback,
            Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData> levelFinishedCallback,
            Action<DisconnectedReason> didDisconnectCallback)
            : base(
                gameMode,
#if LATEST
                in beatmapKey,
                beatmapLevel,
#else
                difficultyBeatmap,
                previewBeatmapLevel,
#endif
                null,
                overrideColorScheme,
#if !V1_29_1
                null,
#endif
                gameplayModifiers,
                playerSpecificSettings,
                practiceSettings,
#if LATEST
                null,
#endif
                backButtonText,
                useTestNoteCutSoundEffects,
                false,
                beforeSceneSwitchCallback,
#if LATEST
                null,
#endif
                null,
#if !V1_29_1
                null,
#endif
                null)
        {
#if LATEST
            BeatmapLevelData = beatmapLevelData;
#else
            BeatmapDifficulty = beatmapDifficulty;
            BeatmapCharacteristic = beatmapCharacteristic;
#endif
            MultiplayerLevelFinishedCallback = levelFinishedCallback;
            DidDisconnectCallback = didDisconnectCallback;
        }

        public StartMultiplayerLevelParameters(StartMultiplayerLevelParameters original)
            : base(original)
        {
#if LATEST
            BeatmapLevelData = original.BeatmapLevelData;
#else
            BeatmapDifficulty = original.BeatmapDifficulty;
            BeatmapCharacteristic = original.BeatmapCharacteristic;
#endif
            MultiplayerLevelFinishedCallback = original.MultiplayerLevelFinishedCallback;
            DidDisconnectCallback = original.DidDisconnectCallback;
        }

#if LATEST
        public IBeatmapLevelData BeatmapLevelData { get; }
#else
        public BeatmapDifficulty BeatmapDifficulty { get; }

        public BeatmapCharacteristicSO BeatmapCharacteristic { get; }
#endif

        public Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData>? MultiplayerLevelFinishedCallback { get; }

        public Action<DisconnectedReason>? DidDisconnectCallback { get; }

        public override StartStandardLevelParameters Copy()
        {
            return new StartMultiplayerLevelParameters(this);
        }
    }
}
