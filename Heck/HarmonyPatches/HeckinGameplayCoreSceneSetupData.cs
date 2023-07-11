using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;

namespace Heck.HarmonyPatches
{
    // whatever mess they created with the beatmapdatas in the base class is real stinky
    [HeckPatch]
    public class HeckinGameplayCoreSceneSetupData : GameplayCoreSceneSetupData
    {
        private static readonly ConstructorInfo _original = AccessTools.FirstConstructor(typeof(GameplayCoreSceneSetupData), _ => true);
        private static readonly ConstructorInfo _hecked = AccessTools.FirstConstructor(typeof(HeckinGameplayCoreSceneSetupData), _ => true);

        private static readonly MethodInfo _heckType = AccessTools.Method(typeof(HeckinGameplayCoreSceneSetupData), nameof(HeckGetType));

        private IReadonlyBeatmapData? _untransformedBeatmapData;

        public HeckinGameplayCoreSceneSetupData(
            IDifficultyBeatmap difficultyBeatmap,
            IPreviewBeatmapLevel previewBeatmapLevel,
            GameplayModifiers gameplayModifiers,
            PlayerSpecificSettings playerSpecificSettings,
            PracticeSettings practiceSettings,
            bool useTestNoteCutSoundEffects,
            EnvironmentInfoSO environmentInfo,
            ColorScheme colorScheme,
            MainSettingsModelSO mainSettingsModel,
            BeatmapDataCache? beatmapDataCache = null)
            : base(
                difficultyBeatmap,
                previewBeatmapLevel,
                gameplayModifiers,
                playerSpecificSettings,
                practiceSettings,
                useTestNoteCutSoundEffects,
                environmentInfo,
                colorScheme,
                mainSettingsModel,
                beatmapDataCache)
        {
        }

        public IReadonlyBeatmapData UntransformedBeatmapData =>
            _untransformedBeatmapData
            ?? throw new InvalidOperationException($"[{nameof(_untransformedBeatmapData)}] was null.");

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameplayCoreSceneSetupData), nameof(GameplayCoreSceneSetupData.GetTransformedBeatmapDataAsync))]
        private static bool OverrideGetTransformedBeatmapDataAsync(GameplayCoreSceneSetupData __instance, ref Task<IReadonlyBeatmapData?> __result)
        {
            if (__instance is not HeckinGameplayCoreSceneSetupData hecked)
            {
                return true;
            }

            __result = hecked.GetTransformedBeatmapDataAsync();
            return false;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), "Init")]
        [HarmonyPatch(typeof(MultiplayerLevelScenesTransitionSetupDataSO), "Init")]
        [HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), "Init")]
        private static IEnumerable<CodeInstruction> Replace(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- base.gameplayCoreSceneSetupData = new GameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, practiceSettings, useTestNoteCutSoundEffects, this.environmentInfo, this.colorScheme, this._mainSettingsModel);
                 * ++ base.gameplayCoreSceneSetupData = new HeckinGameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, practiceSettings, useTestNoteCutSoundEffects, this.environmentInfo, this.colorScheme, this._mainSettingsModel);
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Newobj, _original))
                .SetOperandAndAdvance(_hecked)
                .InstructionEnumeration();
        }

        // i hate gettype i hate gettype i hate gettype
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ScenesTransitionSetupDataSO), nameof(ScenesTransitionSetupDataSO.InstallBindings))]
        private static IEnumerable<CodeInstruction> HeckOff(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * -- Type type = sceneSetupData.GetType();
                 * ++ Type type = HeckGetType(sceneSetupData.GetType());
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, _heckType))
                .InstructionEnumeration();
        }

        private static Type HeckGetType(Type original)
        {
            return original == typeof(HeckinGameplayCoreSceneSetupData) ? typeof(GameplayCoreSceneSetupData) : original;
        }

        // override to store the untransformed beatmapdata
        private new async Task<IReadonlyBeatmapData?> GetTransformedBeatmapDataAsync()
        {
            if (difficultyBeatmap == null)
            {
                return null;
            }

            IReadonlyBeatmapData beatmapData = beatmapDataCache != null
                ? await beatmapDataCache.GetBeatmapData(difficultyBeatmap, environmentInfo, playerSpecificSettings)
                : await difficultyBeatmap.GetBeatmapDataAsync(environmentInfo, playerSpecificSettings);

            _untransformedBeatmapData = beatmapData;
            EnvironmentEffectsFilterPreset environmentEffectsFilterPreset =
                (difficultyBeatmap.difficulty == BeatmapDifficulty.ExpertPlus)
                    ? playerSpecificSettings.environmentEffectsFilterExpertPlusPreset
                    : playerSpecificSettings.environmentEffectsFilterDefaultPreset;
            return BeatmapDataTransformHelper.CreateTransformedBeatmapData(
                beatmapData,
                previewBeatmapLevel,
                gameplayModifiers,
                playerSpecificSettings.leftHanded,
                environmentEffectsFilterPreset,
                environmentInfo.environmentIntensityReductionOptions,
                mainSettingsModel);
        }
    }
}
