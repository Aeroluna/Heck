using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Settings;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using static Chroma.ChromaController;

namespace Chroma.HarmonyPatches
{
    internal class SavedEnvironmentLoading : IAffinity, IDisposable
    {
        private readonly Config _config;
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

        private readonly CodeInstruction _changeFilterPresetV3;
#if LATEST
        private readonly CodeInstruction _changeFilterPresetV2;
#endif

        private SavedEnvironmentLoading(
            Config config,
            SavedEnvironmentLoader savedEnvironmentLoader)
        {
            _config = config;
            _savedEnvironmentLoader = savedEnvironmentLoader;
            _changeFilterPresetV3 = InstanceTranspilers.EmitInstanceDelegate<Func<bool, BeatmapSaveDataVersion3.BeatmapSaveData, bool>>(ChangeFilterPresetV3);
#if LATEST
            _changeFilterPresetV2 = InstanceTranspilers.EmitInstanceDelegate<Func<bool, BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData, bool>>(ChangeFilterPresetV2);
#endif
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_changeFilterPresetV3);
#if LATEST
            InstanceTranspilers.DisposeDelegate(_changeFilterPresetV2);
#endif
        }

        private static bool Any(CustomData customData, string key)
        {
            List<object>? list = customData.Get<List<object>>(key);
            if (list == null)
            {
                return false;
            }

            return list.Count != 0;
        }

        [AffinityTranspiler]
#if LATEST
        [AffinityPatch(typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader), nameof(BeatmapDataLoaderVersion3.BeatmapDataLoader.GetBeatmapDataFromSaveData))]
#else
        [AffinityPatch(typeof(BeatmapDataLoader), nameof(BeatmapDataLoader.GetBeatmapDataFromBeatmapSaveData))]
#endif
        private IEnumerable<CodeInstruction> ChangeFilterPresetV3Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- bool flag3 = flag && flag2;
                 * ++ bool flag3 = ChangeFilterPreset(flag && flag2, beatmapSaveData);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_1))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    _changeFilterPresetV3)
                .InstructionEnumeration();
        }

        private bool ChangeFilterPresetV3(bool original, BeatmapSaveDataVersion3.BeatmapSaveData saveData)
        {
            if (!_config.CustomEnvironmentEnabled ||
                (saveData is Version3CustomBeatmapSaveData customSaveData &&
                (!_config.EnvironmentEnhancementsDisabled &&
#if LATEST
                 (
#else
                 (Any(customSaveData.beatmapCustomData, V2_ENVIRONMENT_REMOVAL) ||
#endif
                  Any(customSaveData.customData, V2_ENVIRONMENT) ||
                  Any(customSaveData.customData, ENVIRONMENT)))))
            {
                return original;
            }

            EnvironmentEffectsFilterPreset? forcedPreset = _savedEnvironmentLoader.SavedEnvironment?.Features.ForcedPreset;
            if (forcedPreset != null)
            {
                return original && forcedPreset.Value != EnvironmentEffectsFilterPreset.NoEffects;
            }

            return original;
        }

#if LATEST
        [AffinityTranspiler]
        [AffinityPatch(typeof(BeatmapDataLoaderVersion3.BeatmapDataLoader), nameof(BeatmapDataLoaderVersion2_6_0AndEarlier.BeatmapDataLoader.GetBeatmapDataFromSaveData))]
        private IEnumerable<CodeInstruction> ChangeFilterPresetV2Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- bool flag3 = flag && flag2;
                 * ++ bool flag3 = ChangeFilterPreset(flag && flag2, beatmapSaveData);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_1))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    _changeFilterPresetV2)
                .InstructionEnumeration();
        }

        private bool ChangeFilterPresetV2(bool original, BeatmapSaveDataVersion2_6_0AndEarlier.BeatmapSaveData saveData)
        {
            if (!_config.CustomEnvironmentEnabled ||
                (saveData is Version2_6_0AndEarlierCustomBeatmapSaveData customSaveData &&
                 (!_config.EnvironmentEnhancementsDisabled &&
                    (Any(customSaveData.customData, V2_ENVIRONMENT) ||
                    Any(customSaveData.customData, ENVIRONMENT)))))
            {
                return original;
            }

            EnvironmentEffectsFilterPreset? forcedPreset = _savedEnvironmentLoader.SavedEnvironment?.Features.ForcedPreset;
            if (forcedPreset != null)
            {
                return original && forcedPreset.Value != EnvironmentEffectsFilterPreset.NoEffects;
            }

            return original;
        }
#endif

        [AffinityPrefix]
#if LATEST
        [AffinityPatch(typeof(DefaultEnvironmentEventsFactory), nameof(DefaultEnvironmentEventsFactory.InsertDefaultEvents))]
#else
        [AffinityPatch(typeof(DefaultEnvironmentEventsFactory), nameof(DefaultEnvironmentEventsFactory.InsertDefaultEnvironmentEvents))]
#endif
        private bool Prefix(BeatmapData beatmapData)
        {
            if (!_config.CustomEnvironmentEnabled ||
                (beatmapData is CustomBeatmapData customBeatmapData &&
                (!_config.EnvironmentEnhancementsDisabled &&
#if LATEST
                 (
#else
                 (Any(customBeatmapData.beatmapCustomData, V2_ENVIRONMENT_REMOVAL) ||
#endif
                  Any(customBeatmapData.customData, V2_ENVIRONMENT) ||
                  Any(customBeatmapData.customData, ENVIRONMENT)))))
            {
                return true;
            }

            List<Version3CustomBeatmapSaveData.BasicEventSaveData>? basicEventDatas = _savedEnvironmentLoader.SavedEnvironment?.Features.BasicEventDatas;
            if (basicEventDatas == null)
            {
                return true;
            }

            basicEventDatas.ForEach(n => beatmapData.InsertBeatmapEventData(new CustomBasicBeatmapEventData(
                0,
                (BasicBeatmapEventType)n.eventType,
                n.value,
                n.floatValue,
                n.customData,
                VersionExtensions.version3)));

            return false;
        }
    }
}
