using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Lighting;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using JetBrains.Annotations;
using static Chroma.ChromaController;

namespace Chroma
{
    internal class ModuleCallbacks
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

        [ModuleCallback(PatchType.Colorizer)]
        private static void ToggleColorizer(bool value)
        {
            Deserializer.Enabled = value;
            ColorizerPatcher.Enabled = value;
        }

        [ModuleCondition(PatchType.Features)]
        private static bool ConditionFeatures(
            Capabilities capabilities,
            IDifficultyBeatmap difficultyBeatmap)
        {
            bool chromaRequirement = capabilities.Requirements.Contains(CAPABILITY) || capabilities.Suggestions.Contains(CAPABILITY);

            // please let me remove this shit
            bool legacyOverride = difficultyBeatmap is CustomDifficultyBeatmap { beatmapSaveData: CustomBeatmapSaveData customBeatmapSaveData }
                                  && customBeatmapSaveData.basicBeatmapEvents.Any(n => n.value >= LegacyLightHelper.RGB_INT_OFFSET);

            bool customEnvironment = Config.Instance.CustomEnvironmentEnabled && (SavedEnvironmentLoader.Instance.SavedEnvironment?.Features.UseChromaEvents ?? false);

            // ReSharper disable once InvertIf
            if (legacyOverride)
            {
                Plugin.Log.Warn("Legacy Chroma Detected...");
                Plugin.Log.Warn("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed");
            }

            return (chromaRequirement || legacyOverride || customEnvironment) && !Config.Instance.ChromaEventsDisabled;
        }

        [ModuleCallback(PatchType.Features)]
        private static void ToggleFeatures(
            bool value,
            IDifficultyBeatmap difficultyBeatmap,
            ModuleManager.ModuleArgs moduleArgs)
        {
            FeaturesPatcher.Enabled = value;
        }

        [ModuleCondition(PatchType.Environment)]
        private static bool ConditionEnvironment(
            IDifficultyBeatmap difficultyBeatmap,
            ModuleManager.ModuleArgs moduleArgs,
            bool dependency)
        {
            EnvironmentInfoSO environmentInfo = difficultyBeatmap.GetEnvironmentInfo();
            EnvironmentTypeSO type = environmentInfo.environmentType;

            bool settingForce = (Config.Instance.ForceMapEnvironmentWhenChroma && dependency) ||
                                (Config.Instance.ForceMapEnvironmentWhenV3 && !_basicEnvironments.Contains(environmentInfo._serializedName));

            CustomBeatmapSaveData? customBeatmapSaveData = difficultyBeatmap.GetBeatmapSaveData();
            if (settingForce ||
                (!Config.Instance.EnvironmentEnhancementsDisabled &&
                customBeatmapSaveData != null &&
                ((customBeatmapSaveData.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Any() ?? false) ||
                 (customBeatmapSaveData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Any() ?? false) ||
                 (customBeatmapSaveData.customData.Get<List<object>>(ENVIRONMENT)?.Any() ?? false))))
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
                        Plugin.Log.Error(e);
                    }
                }
            }
            else
            {
                SavedEnvironment? savedEnvironment = SavedEnvironmentLoader.Instance.SavedEnvironment;
                if (Config.Instance.CustomEnvironmentEnabled && savedEnvironment != null)
                {
                    EnvironmentInfoSO overrideEnv = CustomLevelLoaderExposer.CustomLevelLoader.LoadEnvironmentInfo(savedEnvironment.EnvironmentName, type);
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

        [ModuleCallback(PatchType.Environment)]
        private static void ToggleEnvironment(bool value)
        {
            EnvironmentPatcher.Enabled = value;
        }
    }

    internal class CustomLevelLoaderExposer
    {
        [UsedImplicitly]
        private CustomLevelLoaderExposer(CustomLevelLoader customLevelLoader)
        {
            CustomLevelLoader = customLevelLoader;
        }

        // im lazy
        internal static CustomLevelLoader CustomLevelLoader { get; private set; } = null!;
    }
}
