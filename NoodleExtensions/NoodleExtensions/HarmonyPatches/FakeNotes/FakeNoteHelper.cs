namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using static NoodleExtensions.NoodleObjectDataManager;
    using static NoodleExtensions.Plugin;

    internal static class FakeNoteHelper
    {
        internal static readonly MethodInfo _boundsNullCheck = SymbolExtensions.GetMethodInfo(() => BoundsNullCheck(null));
        internal static readonly MethodInfo _obstacleFakeCheck = SymbolExtensions.GetMethodInfo(() => ObstacleFakeCheck(null));

        internal static bool GetFakeNote(NoteController noteController)
        {
            NoodleNoteData noodleData = TryGetObjectData<NoodleNoteData>(noteController.noteData);
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
            NoodleNoteData noodleData = TryGetObjectData<NoodleNoteData>(noteData);
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

        private static bool BoundsNullCheck(ObstacleController obstacleController)
        {
            return obstacleController.bounds.size == _vectorZero;
        }

        private static List<ObstacleController> ObstacleFakeCheck(List<ObstacleController> intersectingObstacles)
        {
            return intersectingObstacles.Where(n =>
            {
                NoodleObstacleData noodleData = TryGetObjectData<NoodleObstacleData>(n.obstacleData);
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
