using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Modules;
using JetBrains.Annotations;

namespace Chroma.EnvironmentEnhancement;

internal enum LoadedEnvironmentType
{
    None,
    MapOverride,
    SavedOverride
}

internal class EnvironmentOverrideChecker
{
#if PRE_V1_37_1
    private readonly CustomLevelLoader _customLevelLoader;
#else
    private readonly EnvironmentsListModel _environmentsListModel;
#endif
    private readonly SavedEnvironmentLoader _savedEnvironmentLoader;
    private readonly GameplayCoreSceneSetupData _gameplayCoreSceneSetupData;
    private readonly GameScenesManager _gameScenesManager;
    private readonly EnvironmentModule.EnvironmentOverrideType _environmentOverrideType;

    private LoadedEnvironmentType? _loadedEnvironment;

    [UsedImplicitly]
    private EnvironmentOverrideChecker(
#if PRE_V1_37_1
        CustomLevelLoader customLevelLoader,
#else
        EnvironmentsListModel environmentsListModel,
#endif
        SavedEnvironmentLoader savedEnvironmentLoader,
        GameplayCoreSceneSetupData gameplayCoreSceneSetupData,
        GameScenesManager gameScenesManager,
        EnvironmentModule environmentModule)
    {
#if PRE_V1_37_1
        _customLevelLoader = customLevelLoader;
#else
        _environmentsListModel = environmentsListModel;
#endif
        _savedEnvironmentLoader = savedEnvironmentLoader;
        _gameplayCoreSceneSetupData = gameplayCoreSceneSetupData;
        _gameScenesManager = gameScenesManager;
        _environmentOverrideType = environmentModule.OverrideType;
    }

    internal LoadedEnvironmentType LoadedEnvironment
    {
        get
        {
            // ReSharper disable once InvertIf
            if (_loadedEnvironment == null)
            {
#if PRE_V1_37_1
                EnvironmentInfoSO? mapEnv =
                    _gameplayCoreSceneSetupData.difficultyBeatmap.GetEnvironmentInfo();
                EnvironmentInfoSO? savedEnv =
                    _customLevelLoader.LoadEnvironmentInfo(
                        _savedEnvironmentLoader.SavedEnvironment?.EnvironmentName,
                        mapEnv?.environmentType ?? false);
#else
                BeatmapKey beatmapKey = _gameplayCoreSceneSetupData.beatmapKey;
                EnvironmentName environmentName = _gameplayCoreSceneSetupData.beatmapLevel.GetEnvironmentName(
                    beatmapKey.beatmapCharacteristic,
                    beatmapKey.difficulty);
                EnvironmentInfoSO? mapEnv =
                    _environmentsListModel.GetEnvironmentInfoBySerializedName(environmentName);
                EnvironmentInfoSO? savedEnv =
                    _environmentsListModel.GetEnvironmentInfoBySerializedName(
                        _savedEnvironmentLoader.SavedEnvironment?.EnvironmentName!);
#endif

                _loadedEnvironment = _environmentOverrideType switch
                {
                    EnvironmentModule.EnvironmentOverrideType.MapOverride when IsEnvLoaded(mapEnv) =>
                        LoadedEnvironmentType.MapOverride,
                    EnvironmentModule.EnvironmentOverrideType.SavedOverride when IsEnvLoaded(savedEnv) =>
                        LoadedEnvironmentType.SavedOverride,
                    _ => LoadedEnvironmentType.None
                };
            }

            return _loadedEnvironment.Value;
        }
    }

    private bool IsEnvLoaded(EnvironmentInfoSO? environmentInfo)
    {
        return environmentInfo != null && _gameScenesManager.IsSceneInStack(environmentInfo.sceneInfo.sceneName);
    }
}
