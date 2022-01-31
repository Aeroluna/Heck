using System.Collections.Generic;
using HarmonyLib;
using Heck;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    [HeckPatch(PatchType.Features)]
    internal class MiscFakePatches : IAffinity
    {
        private readonly FakePatchesManager _fakePatchesManager;

        private MiscFakePatches(FakePatchesManager fakePatchesManager)
        {
            _fakePatchesManager = fakePatchesManager;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ObstacleSaberSparkleEffectManager), nameof(ObstacleSaberSparkleEffectManager.Update))]
        private static IEnumerable<CodeInstruction> ObstacleSaberSparkleBoundsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return FakePatchesManager.BoundsNullCheckTranspiler(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlayerHeadAndObstacleInteraction), nameof(PlayerHeadAndObstacleInteraction.GetObstaclesContainingPoint))]
        private static IEnumerable<CodeInstruction> PlayerObstacleBoundsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return FakePatchesManager.BoundsNullCheckTranspiler(instructions);
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BombNoteController), nameof(BombNoteController.Init))]
        private void BombNoteCuttable(NoteData noteData, CuttableBySaber ____cuttableBySaber)
        {
            if (!_fakePatchesManager.GetCuttable(noteData))
            {
                ____cuttableBySaber.canBeCut = false;
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(GameNoteController), "NoteDidStartJump")]
        private bool NoteStartJumpCuttable(GameNoteController __instance)
        {
            return _fakePatchesManager.GetCuttable(__instance.noteData);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(GameEnergyCounter), nameof(GameEnergyCounter.LateUpdate))]
        private IEnumerable<CodeInstruction> ObstacleEnergyTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return _fakePatchesManager.ObstaclesTranspiler(instructions);
        }

        [AffinityTranspiler]
        [AffinityPatch(typeof(BeatmapObjectExecutionRatingsRecorder), nameof(BeatmapObjectExecutionRatingsRecorder.Update))]
        private IEnumerable<CodeInstruction> RatingsRecorderTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return _fakePatchesManager.ObstaclesTranspiler(instructions);
        }
    }
}
