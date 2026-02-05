using System;
using JetBrains.Annotations;
#if !PRE_V1_37_1
using Zenject;
#endif

namespace Heck.PlayView;

public class StartStandardLevelParameters
{
    [UsedImplicitly]
    public StartStandardLevelParameters(
        string gameMode,
#if !PRE_V1_37_1
        in BeatmapKey beatmapKey,
        BeatmapLevel beatmapLevel,
#else
        IDifficultyBeatmap difficultyBeatmap,
        IPreviewBeatmapLevel previewBeatmapLevel,
#endif
        OverrideEnvironmentSettings? overrideEnvironmentSettings,
        ColorScheme? overrideColorScheme,
#if !PRE_V1_40_8
        bool playerOverrideLightshowColors,
#endif
#if !V1_29_1 && !LATEST
        ColorScheme? beatmapOverrideColorScheme,
#endif
        GameplayModifiers gameplayModifiers,
        PlayerSpecificSettings playerSpecificSettings,
        PracticeSettings? practiceSettings,
#if !PRE_V1_37_1
        EnvironmentsListModel environmentsListModel,
#endif
#if LATEST
        GameplayAdditionalInformation? gameplayAdditionalInformation,
        Action? beforeSceneSwitchToGameplayCallback,
        Action<DiContainer>? afterSceneSwitchToGameplayCallback,
#else
        string backButtonText,
        bool useTestNoteCutSoundEffects,
        bool startPaused,
        Action? beforeSceneSwitchCallback,
#endif
#if !PRE_V1_37_1 && !LATEST
        Action<DiContainer>? afterSceneSwitchCallback,
#endif
        Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelFinishedCallback,
#if !V1_29_1
        Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelRestartedCallback,
    #if LATEST
        IBeatmapLevelData? beatmapLevelData,
    #endif
        RecordingToolManager.SetupData? recordingToolData)
#else
        Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelRestartedCallback)
#endif
    {
        GameMode = gameMode;
#if !PRE_V1_37_1
        BeatmapKey = beatmapKey;
        BeatmapLevel = beatmapLevel;
#else
        DifficultyBeatmap = difficultyBeatmap;
        PreviewBeatmapLevel = previewBeatmapLevel;
#endif
        OverrideEnvironmentSettings = overrideEnvironmentSettings;
        OverrideColorScheme = overrideColorScheme;
#if !PRE_V1_40_8
        PlayerOverrideLightshowColors = playerOverrideLightshowColors;
#endif
#if !V1_29_1 && !LATEST
        BeatmapOverrideColorScheme = beatmapOverrideColorScheme;
#endif
        GameplayModifiers = gameplayModifiers;
        PlayerSpecificSettings = playerSpecificSettings;
        PracticeSettings = practiceSettings;
#if !PRE_V1_37_1
        EnvironmentsListModel = environmentsListModel;
#endif
#if LATEST
        GameplayAdditionalInformation = gameplayAdditionalInformation;
        BeforeSceneSwitchCallback = beforeSceneSwitchToGameplayCallback;
        AfterSceneSwitchCallback = afterSceneSwitchToGameplayCallback;
#else
        BackButtonText = backButtonText;
        UseTestNoteCutSoundEffects = useTestNoteCutSoundEffects;
        StartPaused = startPaused;
        BeforeSceneSwitchCallback = beforeSceneSwitchCallback;
#endif
        LevelFinishedCallback = levelFinishedCallback;
        LevelRestartedCallback = levelRestartedCallback;
#if !V1_29_1
        RecordingToolData = recordingToolData;
#endif
#if LATEST
        BeatmapLevelData = beatmapLevelData;
#endif
    }

    public StartStandardLevelParameters(StartStandardLevelParameters original)
    {
        GameMode = original.GameMode;
#if !PRE_V1_37_1
        BeatmapKey = original.BeatmapKey;
        BeatmapLevel = original.BeatmapLevel;
#else
        DifficultyBeatmap = original.DifficultyBeatmap;
        PreviewBeatmapLevel = original.PreviewBeatmapLevel;
#endif
        OverrideEnvironmentSettings = original.OverrideEnvironmentSettings;
        OverrideColorScheme = original.OverrideColorScheme;
#if !PRE_V1_40_8
        PlayerOverrideLightshowColors = original.PlayerOverrideLightshowColors;
#endif
#if !V1_29_1 && !LATEST
        BeatmapOverrideColorScheme = original.BeatmapOverrideColorScheme;
#endif
        GameplayModifiers = original.GameplayModifiers;
        PlayerSpecificSettings = original.PlayerSpecificSettings;
        PracticeSettings = original.PracticeSettings;
#if !PRE_V1_37_1
        EnvironmentsListModel = original.EnvironmentsListModel;
#endif
#if LATEST
        GameplayAdditionalInformation = original.GameplayAdditionalInformation;
        AfterSceneSwitchCallback = original.AfterSceneSwitchCallback;
#else
        BackButtonText = original.BackButtonText;
        UseTestNoteCutSoundEffects = original.UseTestNoteCutSoundEffects;
        StartPaused = original.StartPaused;
#endif
        BeforeSceneSwitchCallback = original.BeforeSceneSwitchCallback;
        LevelFinishedCallback = original.LevelFinishedCallback;
        LevelRestartedCallback = original.LevelRestartedCallback;
#if !V1_29_1
        RecordingToolData = original.RecordingToolData;
#endif
#if LATEST
        BeatmapLevelData = original.BeatmapLevelData;
#endif
    }

    public string GameMode { get; }

#if !PRE_V1_37_1
    public BeatmapKey BeatmapKey { get; }

    public BeatmapLevel BeatmapLevel { get; }
#else
    public IDifficultyBeatmap DifficultyBeatmap { get; }

    public IPreviewBeatmapLevel PreviewBeatmapLevel { get; }
#endif

    public OverrideEnvironmentSettings? OverrideEnvironmentSettings { get; set; }

    public ColorScheme? OverrideColorScheme { get; set; }

#if !PRE_V1_40_8
    public bool PlayerOverrideLightshowColors { get; }
#endif

#if !V1_29_1 && !LATEST
    public ColorScheme? BeatmapOverrideColorScheme { get; }
#endif

    public GameplayModifiers GameplayModifiers { get; set; }

    public PlayerSpecificSettings PlayerSpecificSettings { get; set; }

    public PracticeSettings? PracticeSettings { get; }

#if !PRE_V1_37_1
    public EnvironmentsListModel EnvironmentsListModel { get; }
#endif

#if LATEST
    public GameplayAdditionalInformation? GameplayAdditionalInformation { get; }
#else
    public string BackButtonText { get; }

    public bool UseTestNoteCutSoundEffects { get; }

    public bool StartPaused { get; }
#endif

    public Action? BeforeSceneSwitchCallback { get; }

#if LATEST
    public Action<DiContainer>? AfterSceneSwitchCallback { get; }
#endif

    public Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelFinishedCallback { get; }

    public Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelRestartedCallback { get; }

#if !V1_29_1
    public RecordingToolManager.SetupData? RecordingToolData { get; }
#endif

#if LATEST
    public IBeatmapLevelData? BeatmapLevelData { get; }
#endif

    public virtual StartStandardLevelParameters Copy()
    {
        return new StartStandardLevelParameters(this);
    }
}
