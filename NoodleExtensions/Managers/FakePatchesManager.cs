using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.Managers
{
    internal class FakePatchesManager : IDisposable
    {
        private static readonly MethodInfo _intersectingObstaclesGetter = AccessTools.PropertyGetter(typeof(PlayerHeadAndObstacleInteraction), nameof(PlayerHeadAndObstacleInteraction.intersectingObstacles));

        private static readonly MethodInfo _currentGetter = AccessTools.PropertyGetter(typeof(List<ObstacleController>.Enumerator), nameof(List<ObstacleController>.Enumerator.Current));
        private static readonly MethodInfo _boundsNullCheck = AccessTools.Method(typeof(FakePatchesManager), nameof(BoundsNullCheck));

        private readonly CodeInstruction _obstacleFakeCheck;
        private readonly CustomData _customData;

        private FakePatchesManager([Inject(Id = NoodleController.ID)] CustomData customData)
        {
            _customData = customData;
            _obstacleFakeCheck = InstanceTranspilers.EmitInstanceDelegate<Func<IEnumerable<ObstacleController>, List<ObstacleController>>>(ObstacleFakeCheck);
        }

        public void Dispose()
        {
            InstanceTranspilers.DisposeDelegate(_obstacleFakeCheck);
        }

        internal static bool BoundsNullCheck(ObstacleController obstacleController)
        {
            return obstacleController.bounds.size == Vector3.zero;
        }

        internal static IEnumerable<CodeInstruction> BoundsNullCheckTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Br));

            object label = codeMatcher.Operand;

            return codeMatcher
                .MatchForward(false, new CodeMatch(OpCodes.Call, _currentGetter))
                .Advance(2)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Call, _boundsNullCheck),
                    new CodeInstruction(OpCodes.Brtrue_S, label))
                .InstructionEnumeration();
        }

        internal bool GetFakeNote(NoteController noteController)
        {
            _customData.Resolve(noteController.noteData, out NoodleNoteData? noodleData);
            return noodleData?.Fake is not true;
        }

        internal bool GetCuttable(NoteData noteData)
        {
            _customData.Resolve(noteData, out NoodleNoteData? noodleData);
            bool? cuttable = noodleData?.Cuttable;
            return !cuttable.HasValue || cuttable.Value;
        }

        internal IEnumerable<CodeInstruction> ObstaclesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _intersectingObstaclesGetter))
                .Advance(1)
                .Insert(_obstacleFakeCheck)
                .InstructionEnumeration();
        }

        private List<ObstacleController> ObstacleFakeCheck(IEnumerable<ObstacleController> intersectingObstacles)
        {
            return intersectingObstacles.Where(n =>
            {
                _customData.Resolve(n.obstacleData, out NoodleObstacleData? noodleData);
                return noodleData?.Fake is true;
            }).ToList();
        }
    }
}
