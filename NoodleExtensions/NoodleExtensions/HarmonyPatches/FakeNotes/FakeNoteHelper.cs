namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using HarmonyLib;
    using UnityEngine;
    using static NoodleExtensions.NoodleObjectDataManager;

    internal static class FakeNoteHelper
    {
        internal static readonly MethodInfo _boundsNullCheck = AccessTools.Method(typeof(FakeNoteHelper), nameof(BoundsNullCheck));

        private static readonly MethodInfo _intersectingObstaclesGetter = AccessTools.PropertyGetter(typeof(PlayerHeadAndObstacleInteraction), nameof(PlayerHeadAndObstacleInteraction.intersectingObstacles));
        private static readonly MethodInfo _obstacleFakeCheck = AccessTools.Method(typeof(FakeNoteHelper), nameof(ObstacleFakeCheck));

        internal static bool GetFakeNote(NoteController noteController)
        {
            NoodleNoteData? noodleData = TryGetObjectData<NoodleNoteData>(noteController.noteData);
            if (noodleData != null)
            {
                bool? fake = noodleData.Fake;
                if (fake.HasValue && fake.Value)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool GetCuttable(NoteData noteData)
        {
            NoodleNoteData? noodleData = TryGetObjectData<NoodleNoteData>(noteData);
            if (noodleData != null)
            {
                bool? cuttable = noodleData.Cuttable;
                if (cuttable.HasValue && !cuttable.Value)
                {
                    return false;
                }
            }

            return true;
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

        private static List<ObstacleController> ObstacleFakeCheck(List<ObstacleController> intersectingObstacles)
        {
            return intersectingObstacles.Where(n =>
            {
                NoodleObstacleData? noodleData = TryGetObjectData<NoodleObstacleData>(n.obstacleData);
                if (noodleData != null)
                {
                    bool? fake = noodleData.Fake;
                    if (fake.HasValue && fake.Value)
                    {
                        return false;
                    }
                }

                return true;
            }).ToList();
        }
    }
}
