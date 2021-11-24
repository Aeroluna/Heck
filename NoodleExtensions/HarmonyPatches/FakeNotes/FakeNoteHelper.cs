using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using static NoodleExtensions.NoodleCustomDataManager;

namespace NoodleExtensions.HarmonyPatches.FakeNotes
{
    internal static class FakeNoteHelper
    {
        internal static readonly MethodInfo _boundsNullCheck = AccessTools.Method(typeof(FakeNoteHelper), nameof(BoundsNullCheck));

        private static readonly MethodInfo _intersectingObstaclesGetter = AccessTools.PropertyGetter(typeof(PlayerHeadAndObstacleInteraction), nameof(PlayerHeadAndObstacleInteraction.intersectingObstacles));
        private static readonly MethodInfo _obstacleFakeCheck = AccessTools.Method(typeof(FakeNoteHelper), nameof(ObstacleFakeCheck));

        internal static bool GetFakeNote(NoteController noteController)
        {
            NoodleNoteData? noodleData = TryGetObjectData<NoodleNoteData>(noteController.noteData);
            bool? fake = noodleData?.Fake;
            return fake is not true;
        }

        internal static bool GetCuttable(NoteData noteData)
        {
            NoodleNoteData? noodleData = TryGetObjectData<NoodleNoteData>(noteData);
            bool? cuttable = noodleData?.Cuttable;
            return !cuttable.HasValue || cuttable.Value;
        }

        internal static IEnumerable<CodeInstruction> ObstaclesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _intersectingObstaclesGetter))
                .Advance(1)
                .Insert(new CodeInstruction(OpCodes.Call, _obstacleFakeCheck))
                .InstructionEnumeration();
        }

        private static bool BoundsNullCheck(ObstacleController obstacleController)
        {
            return obstacleController.bounds.size == Vector3.zero;
        }

        private static List<ObstacleController> ObstacleFakeCheck(IEnumerable<ObstacleController> intersectingObstacles)
        {
            return intersectingObstacles.Where(n =>
            {
                NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(n.obstacleData);
                bool? fake = noodleData?.Fake;
                return fake is true;
            }).ToList();
        }
    }
}
