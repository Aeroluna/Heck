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
#if LATEST
        EnvironmentsListModel environmentsListModel,
#endif
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
#if !PRE_V1_40_8
            false,
#endif
#if !V1_29_1 && !LATEST
            null,
#endif
            gameplayModifiers,
            playerSpecificSettings,
            practiceSettings,
#if LATEST
            environmentsListModel,
#endif
#if !PRE_V1_37_1
            null,
#endif
#if LATEST
            beforeSceneSwitchCallback,
            null,
#else
            backButtonText,
            useTestNoteCutSoundEffects,
            false,
            beforeSceneSwitchCallback,
#endif
#if !PRE_V1_37_1
            null,
#endif
            null,
#if !V1_29_1
            null,
#endif
            null)
    {
#if !LATEST
    #if !PRE_V1_37_1
        BeatmapLevelData = beatmapLevelData;
    #else
        BeatmapDifficulty = beatmapDifficulty;
        BeatmapCharacteristic = beatmapCharacteristic;
    #endif
#endif
        MultiplayerLevelFinishedCallback = levelFinishedCallback;
        DidDisconnectCallback = didDisconnectCallback;
    }

    public StartMultiplayerLevelParameters(StartMultiplayerLevelParameters original)
        : base(original)
    {
#if !LATEST
    #if !PRE_V1_37_1
        BeatmapLevelData = original.BeatmapLevelData;
    #else
        BeatmapDifficulty = original.BeatmapDifficulty;
        BeatmapCharacteristic = original.BeatmapCharacteristic;
    #endif
#endif
        MultiplayerLevelFinishedCallback = original.MultiplayerLevelFinishedCallback;
        DidDisconnectCallback = original.DidDisconnectCallback;
    }

#if !LATEST
    #if !PRE_V1_37_1
    public IBeatmapLevelData BeatmapLevelData { get; }
    #else
    public BeatmapDifficulty BeatmapDifficulty { get; }

    public BeatmapCharacteristicSO BeatmapCharacteristic { get; }
    #endif
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
