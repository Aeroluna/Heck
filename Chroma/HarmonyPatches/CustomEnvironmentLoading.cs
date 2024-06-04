using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BeatmapSaveDataVersion3;
using Chroma.EnvironmentEnhancement.Saved;
using Chroma.Settings;
using CustomJSONData.CustomBeatmap;
using HarmonyLib;
using Heck;
using SiraUtil.Affinity;
using static Chroma.ChromaController;

namespace Chroma.HarmonyPatches
{
    internal class CustomEnvironmentLoading : IAffinity, IDisposable
    {
        private readonly Config _config;
        private readonly SavedEnvironmentLoader _savedEnvironmentLoader;

        private readonly CodeInstruction _changeFilterPreset;

        private CustomEnvironmentLoading(
            Config config,
            SavedEnvironmentLoader savedEnvironmentLoader)
        {
            _config = config;
            _savedEnvironmentLoader = savedEnvironmentLoader;
            _changeFilterPreset = InstanceTranspilers.EmitInstanceDelegate<Func<bool, BeatmapSaveData, bool>>(ChangeFilterPreset);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_changeFilterPreset);
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
        [AffinityPatch(typeof(BeatmapDataLoader), nameof(BeatmapDataLoader.GetBeatmapDataFromBeatmapSaveData))]
        private IEnumerable<CodeInstruction> ChangeFilterPreset(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- bool flag3 = flag && flag2;
                 * ++ bool flag3 = ChangeFilterPreset(flag && flag2, beatmapSaveData);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_1))
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_0),
                    _changeFilterPreset)
                .InstructionEnumeration();
        }

        private bool ChangeFilterPreset(bool original, BeatmapSaveData saveData)
        {
            if (!_config.CustomEnvironmentEnabled ||
                (saveData is CustomBeatmapSaveData customSaveData &&
                (!_config.EnvironmentEnhancementsDisabled &&
                 (Any(customSaveData.beatmapCustomData, V2_ENVIRONMENT_REMOVAL) ||
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

        [AffinityPrefix]
        [AffinityPatch(typeof(DefaultEnvironmentEventsFactory), nameof(DefaultEnvironmentEventsFactory.InsertDefaultEnvironmentEvents))]
        private bool Prefix(BeatmapData beatmapData)
        {
            if (!_config.CustomEnvironmentEnabled ||
                (beatmapData is CustomBeatmapData customBeatmapData &&
                (!_config.EnvironmentEnhancementsDisabled &&
                 (Any(customBeatmapData.beatmapCustomData, V2_ENVIRONMENT_REMOVAL) ||
                  Any(customBeatmapData.customData, V2_ENVIRONMENT) ||
                  Any(customBeatmapData.customData, ENVIRONMENT)))))
            {
                return true;
            }

            List<CustomBeatmapSaveData.BasicEventData>? basicEventDatas = _savedEnvironmentLoader.SavedEnvironment?.Features.BasicEventDatas;
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
                false)));

            return false;
        }
    }
}
