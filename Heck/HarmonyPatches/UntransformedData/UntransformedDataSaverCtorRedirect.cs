#if LATEST
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Heck.HarmonyPatches.UntransformedData;

// whatever mess they created with the beatmapdatas in the base class is real stinky
[HeckPatch]
public class UntransformedDataSaveCtorRedirect
{
    private static readonly ConstructorInfo _hecked = AccessTools.FirstConstructor(
        typeof(HeckGameplayCoreSceneSetupData),
        _ => true);

    [UsedImplicitly]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Replace(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            /*
             * -- base.gameplayCoreSceneSetupData = new GameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, practiceSettings, useTestNoteCutSoundEffects, this.environmentInfo, this.colorScheme, this._mainSettingsModel);
             * ++ base.gameplayCoreSceneSetupData = new HeckinGameplayCoreSceneSetupData(difficultyBeatmap, previewBeatmapLevel, gameplayModifiers, playerSpecificSettings, practiceSettings, useTestNoteCutSoundEffects, this.environmentInfo, this.colorScheme, this._mainSettingsModel);
             */
            .MatchForward(
                false,
                new CodeMatch(
                    n => n.opcode == OpCodes.Newobj &&
                         ((ConstructorInfo)n.operand).DeclaringType == typeof(GameplayCoreSceneSetupData)))
            .Repeat(
                n => n
                    .Advance(1)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Newobj, _hecked)))
            .InstructionEnumeration();
    }

    [UsedImplicitly]
    [HarmonyTargetMethod]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        MethodInfo[] standard = typeof(StandardLevelScenesTransitionSetupDataSO).GetMethods();
        MethodInfo[] multiplayer = typeof(MultiplayerLevelScenesTransitionSetupDataSO).GetMethods();
        MethodInfo[] mission = typeof(MissionLevelScenesTransitionSetupDataSO).GetMethods();
        return standard.Concat(multiplayer).Concat(mission).Where(n => n.Name == "Init");
    }
}
#endif
