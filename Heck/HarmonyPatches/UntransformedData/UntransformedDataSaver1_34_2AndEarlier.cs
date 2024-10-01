#if PRE_V1_37_1
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;

namespace Heck.HarmonyPatches.UntransformedData;

// whatever mess they created with the beatmapdatas in the base class is real stinky
[HeckPatch]
public class HeckGameplayCoreSceneSetupData : GameplayCoreSceneSetupData
{
    private static readonly ConstructorInfo _hecked =
        AccessTools.FirstConstructor(typeof(HeckGameplayCoreSceneSetupData), _ => true);

    private static readonly ConstructorInfo _original =
        AccessTools.FirstConstructor(typeof(GameplayCoreSceneSetupData), _ => true);

    private static readonly MethodInfo _heckType =
        AccessTools.Method(typeof(HeckGameplayCoreSceneSetupData), nameof(HeckGetType));

    private IReadonlyBeatmapData? _untransformedBeatmapData;

    public HeckGameplayCoreSceneSetupData(
        IDifficultyBeatmap difficultyBeatmap,
        IPreviewBeatmapLevel previewBeatmapLevel,
        GameplayModifiers gameplayModifiers,
        PlayerSpecificSettings playerSpecificSettings,
        PracticeSettings practiceSettings,
        bool useTestNoteCutSoundEffects,
        EnvironmentInfoSO environmentInfo,
        ColorScheme colorScheme,
        MainSettingsModelSO mainSettingsModel,
#if !V1_29_1
            BeatmapDataCache? beatmapDataCache = null,
            RecordingToolManager.SetupData? recordingToolData = null)
#else
        BeatmapDataCache? beatmapDataCache = null)
#endif
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
#if !V1_29_1
                beatmapDataCache,
                recordingToolData)
#else
            beatmapDataCache)
#endif
    {
    }

    public IReadonlyBeatmapData UntransformedBeatmapData =>
        _untransformedBeatmapData ??
        throw new InvalidOperationException($"[{nameof(_untransformedBeatmapData)}] was null.");

    private static Type HeckGetType(Type original)
    {
        return original == typeof(HeckGameplayCoreSceneSetupData) ? typeof(GameplayCoreSceneSetupData) : original;
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

    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(GameplayCoreSceneSetupData),
        nameof(GetTransformedBeatmapDataAsync))]
    private static bool OverrideGetTransformedBeatmapDataAsync(
        GameplayCoreSceneSetupData __instance,
        ref Task<IReadonlyBeatmapData?> __result)
    {
        if (__instance is not HeckGameplayCoreSceneSetupData hecked)
        {
            return true;
        }

        __result = hecked.GetUntransformedAndTransformedBeatmapDataAsync();
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
             * ++ base.gameplayCoreSceneSetupData = new HeckGameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, practiceSettings, useTestNoteCutSoundEffects, this.environmentInfo, this.colorScheme, this._mainSettingsModel);
             */
            .MatchForward(false, new CodeMatch(OpCodes.Newobj, _original))
            .SetOperandAndAdvance(_hecked)
            .InstructionEnumeration();
    }

    // override to store the untransformed beatmapdata
    private async Task<IReadonlyBeatmapData?> GetUntransformedAndTransformedBeatmapDataAsync()
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
            difficultyBeatmap.difficulty == BeatmapDifficulty.ExpertPlus
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
#endif
