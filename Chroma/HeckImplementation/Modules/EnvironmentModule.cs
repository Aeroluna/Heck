using System;
using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Lighting;
using Chroma.Settings;
using Heck;
using Heck.Module;
using SiraUtil.Logging;
using static Chroma.ChromaController;
#if !PRE_V1_37_1
using _EnvironmentType = EnvironmentType;
#else
using System.Collections.Generic;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using _EnvironmentType = EnvironmentTypeSO;
#endif

namespace Chroma.Modules;

[Module("ChromaEnvironment", 2, LoadType.Active, ["ChromaColorizer"])]
[ModulePatcher(HARMONY_ID + "Environment", PatchType.Environment)]
internal class EnvironmentModule : IModule
{
    // if there is a better way to detect v3 lights, i would love to know it
    // blacklist because likely this list will never need to be updated
    private static readonly string[] _basicEnvironments =
    [
        "DefaultEnvironment",
        "TriangleEnvironment",
        "NiceEnvironment",
        "BigMirrorEnvironment",
        "KDAEnvironment",
        "MonstercatEnvironment",
        "CrabRaveEnvironment",
        "DragonsEnvironment",
        "OriginsEnvironment",
        "PanicEnvironment",
        "RocketEnvironment",
        "GreenDayEnvironment",
        "GreenDayGrenadeEnvironment",
        "TimbalandEnvironment",
        "FitBeatEnvironment",
        "LinkinParkEnvironment",
        "BTSEnvironment",
        "KaleidoscopeEnvironment",
        "InterscopeEnvironment",
        "SkrillexEnvironment",
        "BillieEnvironment",
        "HalloweenEnvironment",
        "GagaEnvironment"
    ];

    private readonly SiraLog _log;
    private readonly Config _config;
    private readonly SavedEnvironmentLoader _savedEnvironmentLoader;
#if !PRE_V1_37_1
    private readonly EnvironmentsListModel _environmentsListModel;
#else
    private readonly CustomLevelLoader _customLevelLoader;
#endif

    private EnvironmentModule(
        SiraLog log,
        Config config,
        SavedEnvironmentLoader savedEnvironmentLoader,
#if !PRE_V1_37_1
        EnvironmentsListModel environmentsListModel)
#else
        CustomLevelLoader customLevelLoader)
#endif
    {
        _log = log;
        _config = config;
        _savedEnvironmentLoader = savedEnvironmentLoader;
#if !PRE_V1_37_1
        _environmentsListModel = environmentsListModel;
#else
        _customLevelLoader = customLevelLoader;
#endif
    }

    internal bool Active { get; private set; }

    [ModuleCallback]
    private void Callback(bool value)
    {
        Active = value;
    }

    [ModuleCondition]
    private bool ConditionEnvironment(
#if !PRE_V1_37_1
        BeatmapKey beatmapKey,
        BeatmapLevel beatmapLevel,
#else
        IDifficultyBeatmap difficultyBeatmap,
#endif
        ModuleManager.ModuleArgs moduleArgs,
        bool dependency)
    {
#if !PRE_V1_37_1
        EnvironmentName environmentName = beatmapLevel.GetEnvironmentName(
            beatmapKey.beatmapCharacteristic,
            beatmapKey.difficulty);
        EnvironmentInfoSO environmentInfo =
            _environmentsListModel.GetEnvironmentInfoBySerializedNameSafe(environmentName);
#else
        EnvironmentInfoSO environmentInfo = difficultyBeatmap.GetEnvironmentInfo();
#endif
        _EnvironmentType type = environmentInfo.environmentType;

        bool settingForce = (_config.ForceMapEnvironmentWhenChroma && dependency) ||
                            (_config.ForceMapEnvironmentWhenV3 &&
                             !_basicEnvironments.Contains(environmentInfo._serializedName));

#if PRE_V1_37_1
        Version3CustomBeatmapSaveData? customBeatmapSaveData = difficultyBeatmap.GetBeatmapSaveData();
#endif
        if (settingForce ||
            (!_config.EnvironmentEnhancementsDisabled &&
#if !PRE_V1_37_1
             // cant conditionally enable environment module without reading customdata
             dependency))
#else
             customBeatmapSaveData != null &&
             ((customBeatmapSaveData.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Count ?? 0) > 0 ||
              (customBeatmapSaveData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Count ?? 0) > 0 ||
              (customBeatmapSaveData.customData.Get<List<object>>(ENVIRONMENT)?.Count ?? 0) > 0)))
#endif
        {
            // TODO: this logic should probably not be in the condition
            if (settingForce || dependency)
            {
                try
                {
                    LightIDTableManager.SetEnvironment(environmentInfo.serializedName);
                    moduleArgs.OverrideEnvironmentSettings = null;

                    return true;
                }
                catch (Exception e)
                {
                    _log.Error(e);
                }
            }
        }
        else if (moduleArgs.OverrideEnvironmentSettings != null)
        {
            SavedEnvironment? savedEnvironment = _savedEnvironmentLoader.SavedEnvironment;
            if (_config.CustomEnvironmentEnabled && savedEnvironment != null)
            {
#if !PRE_V1_37_1
                EnvironmentInfoSO overrideEnv =
                    _environmentsListModel.GetEnvironmentInfoBySerializedNameSafe(savedEnvironment.EnvironmentName);
#else
                EnvironmentInfoSO overrideEnv =
                    _customLevelLoader.LoadEnvironmentInfo(savedEnvironment.EnvironmentName, type);
#endif
                LightIDTableManager.SetEnvironment(overrideEnv.serializedName);
                OverrideEnvironmentSettings newSettings = new()
                {
                    overrideEnvironments = true
                };
                newSettings.SetEnvironmentInfoForType(type, overrideEnv);
                moduleArgs.OverrideEnvironmentSettings = newSettings;
                return true;
            }
        }

        OverrideEnvironmentSettings? environmentSettings = moduleArgs.OverrideEnvironmentSettings;
        LightIDTableManager.SetEnvironment(
            environmentSettings is { overrideEnvironments: true }
                ? environmentSettings.GetOverrideEnvironmentInfoForType(type).serializedName
                : environmentInfo.serializedName);

        return dependency;
    }
}
