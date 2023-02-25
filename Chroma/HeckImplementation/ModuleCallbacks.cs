using System;
using System.Collections.Generic;
using System.Linq;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Lighting;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using IPA.Logging;
using JetBrains.Annotations;
using static Chroma.ChromaController;

namespace Chroma
{
    internal class ModuleCallbacks
    {
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
                Log.Logger.Log("Legacy Chroma Detected...", Logger.Level.Warning);
                Log.Logger.Log("Please do not use Legacy Chroma Lights for new maps as it is deprecated and its functionality in future versions of Chroma cannot be guaranteed", Logger.Level.Warning);
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

            CustomBeatmapSaveData? customBeatmapSaveData = difficultyBeatmap.GetBeatmapSaveData();
            if (!Config.Instance.EnvironmentEnhancementsDisabled &&
                customBeatmapSaveData != null &&
                ((customBeatmapSaveData.beatmapCustomData.Get<List<object>>(V2_ENVIRONMENT_REMOVAL)?.Any() ?? false) ||
                 (customBeatmapSaveData.customData.Get<List<object>>(V2_ENVIRONMENT)?.Any() ?? false) ||
                 (customBeatmapSaveData.customData.Get<List<object>>(ENVIRONMENT)?.Any() ?? false)))
            {
                // TODO: this logic should probably not be in the condition
                if (dependency)
                {
                    try
                    {
                        LightIDTableManager.SetEnvironment(environmentInfo.serializedName);
                        moduleArgs.OverrideEnvironmentSettings = null;

                        return true;
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Log(e, Logger.Level.Error);
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
