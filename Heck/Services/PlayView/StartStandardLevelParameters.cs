using System;
using JetBrains.Annotations;
#if LATEST
using Zenject;
#endif

namespace Heck.PlayView;

public class StartStandardLevelParameters
{
    [UsedImplicitly]
    public StartStandardLevelParameters(
        string gameMode,
#if LATEST
        in BeatmapKey beatmapKey,
        BeatmapLevel beatmapLevel,
#else
        IDifficultyBeatmap difficultyBeatmap,
        IPreviewBeatmapLevel previewBeatmapLevel,
#endif
        OverrideEnvironmentSettings? overrideEnvironmentSettings,
        ColorScheme? overrideColorScheme,
#if !V1_29_1
        ColorScheme? beatmapOverrideColorScheme,
#endif
        GameplayModifiers gameplayModifiers,
        PlayerSpecificSettings playerSpecificSettings,
        PracticeSettings? practiceSettings,
#if LATEST
        EnvironmentsListModel? environmentsListModel,
#endif
        string backButtonText,
        bool useTestNoteCutSoundEffects,
        bool startPaused,
        Action? beforeSceneSwitchCallback,
#if LATEST
        Action<DiContainer>? afterSceneSwitchCallback,
#endif
        Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelFinishedCallback,
#if !V1_29_1
        Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelRestartedCallback,
        RecordingToolManager.SetupData? recordingToolData)
#else
        Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? levelRestartedCallback)
#endif
    {
        GameMode = gameMode;
#if LATEST
        BeatmapKey = beatmapKey;
        BeatmapLevel = beatmapLevel;
#else
        DifficultyBeatmap = difficultyBeatmap;
        PreviewBeatmapLevel = previewBeatmapLevel;
#endif
        OverrideEnvironmentSettings = overrideEnvironmentSettings;
        OverrideColorScheme = overrideColorScheme;
#if !V1_29_1
        BeatmapOverrideColorScheme = beatmapOverrideColorScheme;
#endif
        GameplayModifiers = gameplayModifiers;
        PlayerSpecificSettings = playerSpecificSettings;
        PracticeSettings = practiceSettings;
#if LATEST
        EnvironmentsListModel = environmentsListModel;
#endif
        BackButtonText = backButtonText;
        UseTestNoteCutSoundEffects = useTestNoteCutSoundEffects;
        StartPaused = startPaused;
        BeforeSceneSwitchCallback = beforeSceneSwitchCallback;
        LevelFinishedCallback = levelFinishedCallback;
        LevelRestartedCallback = levelRestartedCallback;
#if !V1_29_1
        RecordingToolData = recordingToolData;
#endif
    }

    public StartStandardLevelParameters(StartStandardLevelParameters original)
    {
        GameMode = original.GameMode;
#if LATEST
        BeatmapKey = original.BeatmapKey;
        BeatmapLevel = original.BeatmapLevel;
#else
        DifficultyBeatmap = original.DifficultyBeatmap;
        PreviewBeatmapLevel = original.PreviewBeatmapLevel;
#endif
        OverrideEnvironmentSettings = original.OverrideEnvironmentSettings;
        OverrideColorScheme = original.OverrideColorScheme;
#if !V1_29_1
        BeatmapOverrideColorScheme = original.BeatmapOverrideColorScheme;
#endif
        GameplayModifiers = original.GameplayModifiers;
        PlayerSpecificSettings = original.PlayerSpecificSettings;
        PracticeSettings = original.PracticeSettings;
#if LATEST
        EnvironmentsListModel = original.EnvironmentsListModel;
#endif
        BackButtonText = original.BackButtonText;
        UseTestNoteCutSoundEffects = original.UseTestNoteCutSoundEffects;
        StartPaused = original.StartPaused;
        BeforeSceneSwitchCallback = original.BeforeSceneSwitchCallback;
        LevelFinishedCallback = original.LevelFinishedCallback;
        LevelRestartedCallback = original.LevelRestartedCallback;
#if !V1_29_1
        RecordingToolData = original.RecordingToolData;
#endif
    }

    public string GameMode { get; }

#if LATEST
    public BeatmapKey BeatmapKey { get; }

    public BeatmapLevel BeatmapLevel { get; }
#else
    public IDifficultyBeatmap DifficultyBeatmap { get; }

    public IPreviewBeatmapLevel PreviewBeatmapLevel { get; }
#endif

    public OverrideEnvironmentSettings? OverrideEnvironmentSettings { get; set; }

    public ColorScheme? OverrideColorScheme { get; set; }

#if !V1_29_1
    public ColorScheme? BeatmapOverrideColorScheme { get; }
#endif

    public GameplayModifiers GameplayModifiers { get; set; }

    public PlayerSpecificSettings PlayerSpecificSettings { get; set; }

    public PracticeSettings? PracticeSettings { get; }

#if LATEST
    public EnvironmentsListModel? EnvironmentsListModel { get; }
#endif

    public string BackButtonText { get; }

    public bool UseTestNoteCutSoundEffects { get; }

    public bool StartPaused { get; }

    public Action? BeforeSceneSwitchCallback { get; }

    public Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelFinishedCallback { get; }

    public Action<LevelScenesTransitionSetupDataSO, LevelCompletionResults>? LevelRestartedCallback { get; }

#if !V1_29_1
    public RecordingToolManager.SetupData? RecordingToolData { get; }
#endif

    public virtual StartStandardLevelParameters Copy()
    {
        return new StartStandardLevelParameters(this);
    }
}
