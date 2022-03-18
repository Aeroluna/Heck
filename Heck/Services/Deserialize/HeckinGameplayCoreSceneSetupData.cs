using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;

namespace Heck
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
            MainSettingsModelSO mainSettingsModel)
            : base(
                difficultyBeatmap,
                previewBeatmapLevel,
                gameplayModifiers,
                playerSpecificSettings,
                practiceSettings,
                useTestNoteCutSoundEffects,
                environmentInfo,
                colorScheme,
                mainSettingsModel)
        {
        }

        public IReadonlyBeatmapData UntransformedBeatmapData =>
            _untransformedBeatmapData
            ?? throw new InvalidOperationException($"[{nameof(_untransformedBeatmapData)}] was null.");

        public override async Task LoadTransformedBeatmapDataAsync()
        {
            _transformedBeatmapData = await GetTransformedBeatmapDataAsync();
        }

        // override to store the untransformed beatmapdata
        public override async Task<IReadonlyBeatmapData> GetTransformedBeatmapDataAsync()
        {
            IReadonlyBeatmapData beatmapData = await difficultyBeatmap.GetBeatmapDataAsync(environmentInfo);
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

        [HarmonyTranspiler]
        [HarmonyPatch("Init")]
        [HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO))]
        private static IEnumerable<CodeInstruction> Replace(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
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
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, _heckType))
                .InstructionEnumeration();
        }

        private static Type HeckGetType(Type original)
        {
            return original == typeof(HeckinGameplayCoreSceneSetupData) ? typeof(GameplayCoreSceneSetupData) : original;
        }
    }
}
