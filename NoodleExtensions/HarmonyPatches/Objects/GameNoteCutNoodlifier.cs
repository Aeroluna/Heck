using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using Heck.Deserialize;
using SiraUtil.Affinity;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Objects;

internal class GameNoteCutNoodlifier : IAffinity, IDisposable
{
    private readonly DeserializedData _deserializedData;
    private readonly CodeInstruction _skipBadCuts;

    private GameNoteCutNoodlifier([Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
    {
        _deserializedData = deserializedData;
        _skipBadCuts =
            InstanceTranspilers.EmitInstanceDelegate<Func<GameNoteController, bool, bool, bool, bool>>(SkipBadCuts);
    }

    public void Dispose()
    {
        InstanceTranspilers.DisposeDelegate(_skipBadCuts);
    }

    [AffinityTranspiler]
    [AffinityPatch(typeof(GameNoteController), nameof(GameNoteController.HandleCut))]
    private IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return new CodeMatcher(instructions, generator)
            /*
             * ++ if (SkipBadCuts(this, flag, flag2, flag3)) {
             * ++     return;
             * ++ }
             * if ((!flag || !flag2 || !flag3) && !allowBadCut)
             */
            .MatchForward(false, new CodeMatch(OpCodes.Ldloc_1))
            .CreateLabel(out Label label)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_3),
                _skipBadCuts,
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ret))
            .InstructionEnumeration();
    }

    // ReSharper disable InconsistentNaming
    private bool SkipBadCuts(GameNoteController gameNoteController, bool directionOK, bool speedOK, bool saberTypeOK)
    {
        _deserializedData.Resolve(gameNoteController.noteData, out NoodleBaseNoteData? noodleData);

        return noodleData != null &&
               ((noodleData.DisableBadCutDirection && !directionOK) ||
                (noodleData.DisableBadCutSpeed && !speedOK) ||
                (noodleData.DisableBadCutSaberType && !saberTypeOK));
    }
}
