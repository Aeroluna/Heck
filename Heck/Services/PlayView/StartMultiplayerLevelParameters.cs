using System;
using JetBrains.Annotations;

namespace Heck.PlayView;

public class StartMultiplayerLevelParameters : StartStandardLevelParameters
{
    [UsedImplicitly]
    public StartMultiplayerLevelParameters(
        string gameMode,
#if !PRE_V1_37_1
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
#if !PRE_V1_37_1
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
#if !PRE_V1_37_1
            null,
#endif
            backButtonText,
            useTestNoteCutSoundEffects,
            false,
            beforeSceneSwitchCallback,
#if !PRE_V1_37_1
            null,
#endif
            null,
#if !V1_29_1
            null,
#endif
            null)
    {
#if !PRE_V1_37_1
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
#if !PRE_V1_37_1
        BeatmapLevelData = original.BeatmapLevelData;
#else
        BeatmapDifficulty = original.BeatmapDifficulty;
        BeatmapCharacteristic = original.BeatmapCharacteristic;
#endif
        MultiplayerLevelFinishedCallback = original.MultiplayerLevelFinishedCallback;
        DidDisconnectCallback = original.DidDisconnectCallback;
    }

#if !PRE_V1_37_1
    public IBeatmapLevelData BeatmapLevelData { get; }
#else
    public BeatmapDifficulty BeatmapDifficulty { get; }

    public BeatmapCharacteristicSO BeatmapCharacteristic { get; }
#endif

    public Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData>? MultiplayerLevelFinishedCallback
    {
        get;
    }

    public Action<DisconnectedReason>? DidDisconnectCallback { get; }

    public override StartStandardLevelParameters Copy()
    {
        return new StartMultiplayerLevelParameters(this);
    }
}
