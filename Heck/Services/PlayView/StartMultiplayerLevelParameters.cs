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
#if !PRE_V1_44_1
        EnvironmentsListModel environmentsListModel,
#endif
        PracticeSettings? practiceSettings,
        string backButtonText,
        bool useTestNoteCutSoundEffects,
        Action beforeSceneSwitchCallback,
#if LATEST
        Action<MultiplayerLevelScenesTransitionSetupData, MultiplayerResultsData> levelFinishedCallback,
#else
        Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData> levelFinishedCallback,
#endif
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
#if !V1_29_1 && PRE_V1_44_1
            null,
#endif
            gameplayModifiers,
            playerSpecificSettings,
            practiceSettings,
#if !PRE_V1_44_1
            environmentsListModel,
#endif
#if !PRE_V1_37_1
            null,
#endif
#if !PRE_V1_44_1
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
#if LATEST
            null)
#else
    #if !V1_29_1
            null,
    #endif
            null)
#endif
    {
#if PRE_V1_44_1
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
#if PRE_V1_44_1
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

#if PRE_V1_44_1
    #if !PRE_V1_37_1
    public IBeatmapLevelData? BeatmapLevelData { get; }
    #else
    public BeatmapDifficulty BeatmapDifficulty { get; }

    public BeatmapCharacteristicSO BeatmapCharacteristic { get; }
    #endif
#endif

#if LATEST
    public Action<MultiplayerLevelScenesTransitionSetupData, MultiplayerResultsData>? MultiplayerLevelFinishedCallback
    {
        get;
    }
#else
    public Action<MultiplayerLevelScenesTransitionSetupDataSO, MultiplayerResultsData>? MultiplayerLevelFinishedCallback
    {
        get;
    }
#endif

    public Action<DisconnectedReason>? DidDisconnectCallback { get; }

    public override StartStandardLevelParameters Copy()
    {
        return new StartMultiplayerLevelParameters(this);
    }
}
