using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Lighting;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using SiraUtil.Logging;
using static Chroma.ChromaController;

namespace Chroma.Modules
{
    [Module("ChromaEnvironment", 2, LoadType.Active, new[] { "ChromaColorizer" })]
    [ModulePatcher(HARMONY_ID + "Environment", PatchType.Environment)]
    internal class EnvironmentModule : IModule
    {
        // if there is a better way to detect v3 lights, i would love to know it
        // blacklist because likely this list will never need to be updated
        private static readonly string[] _basicEnvironments =
        {
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
        };

        private readonly SiraLog _log;
        private readonly Config _config;
        private readonly CustomLevelLoader _customLevelLoader;

        private EnvironmentModule(SiraLog log, Config config, CustomLevelLoader customLevelLoader)
        {
            _log = log;
            _config = config;
            _customLevelLoader = customLevelLoader;
        }

        internal bool Active { get; private set; }

        [ModuleCallback]
        private void Callback(bool value)
        {
            Active = value;
        }

        [ModuleCondition]
        private bool ConditionEnvironment(
            IDifficultyBeatmap difficultyBeatmap,
            ModuleManager.ModuleArgs moduleArgs,
            bool dependency)
        {
            EnvironmentInfoSO environmentInfo = difficultyBeatmap.GetEnvironmentInfo();
            EnvironmentTypeSO type = environmentInfo.environmentType;

            bool settingForce = (_config.ForceMapEnvironmentWhenChroma && dependency) ||
                                (_config.ForceMapEnvironmentWhenV3 && !_basicEnvironments.Contains(environmentInfo._serializedName));

            CustomBeatmapSaveData? customBeatmapSaveData = difficultyBeatmap.GetBeatmapSaveData();
            if (settingForce ||
                (!_config.EnvironmentEnhancementsDisabled &&
                customBeatmapSaveData != null &&
                ((customBeatmapSaveData.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Count ?? 0) > 0 ||
                 (customBeatmapSaveData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Count ?? 0) > 0 ||
                 (customBeatmapSaveData.customData.Get<List<object>>(ENVIRONMENT)?.Count ?? 0) > 0)))
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
                SavedEnvironment? savedEnvironment = SavedEnvironmentLoader.Instance.SavedEnvironment;
                if (_config.CustomEnvironmentEnabled && savedEnvironment != null)
                {
                    EnvironmentInfoSO overrideEnv = _customLevelLoader.LoadEnvironmentInfo(savedEnvironment.EnvironmentName, type);
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
            LightIDTableManager.SetEnvironment(environmentSettings is { overrideEnvironments: true }
                ? environmentSettings.GetOverrideEnvironmentInfoForType(type).serializedName
                : environmentInfo.serializedName);

            return dependency;
        }
    }
}
