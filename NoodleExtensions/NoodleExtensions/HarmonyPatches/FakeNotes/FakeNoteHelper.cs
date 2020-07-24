namespace NoodleExtensions.HarmonyPatches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using static NoodleExtensions.Plugin;

    internal static class FakeNoteHelper
    {
        internal static readonly MethodInfo _boundsNullCheck = SymbolExtensions.GetMethodInfo(() => BoundsNullCheck(null));
        internal static readonly MethodInfo _obstacleFakeCheck = SymbolExtensions.GetMethodInfo(() => ObstacleFakeCheck(null));

        internal static bool GetFakeNote(INoteController noteController)
        {
            if (noteController.noteData is CustomNoteData customNoteData)
            {
                dynamic dynData = customNoteData.customData;
                bool? fake = Trees.at(dynData, FAKENOTE);
                if (fake.HasValue && fake.Value)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool GetCuttable(NoteData noteData)
        {
            if (noteData is CustomNoteData customNoteData)
            {
                dynamic dynData = customNoteData.customData;
                bool? cuttable = Trees.at(dynData, CUTTABLE);
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
                if (n.obstacleData is CustomObstacleData customObstacleData)
                {
                    dynamic dynData = customObstacleData.customData;
                    bool? fake = Trees.at(dynData, FAKENOTE);
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
