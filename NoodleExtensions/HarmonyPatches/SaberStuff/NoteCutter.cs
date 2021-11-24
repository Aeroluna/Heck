using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using JetBrains.Annotations;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SaberStuff
{
    [HeckPatch(typeof(NoteCutter))]
    [HeckPatch("Cut")]
    internal static class NoteCutterCut
    {
        private static readonly MethodInfo _convertToWorld = AccessTools.Method(typeof(NoteCutterCut), nameof(ConvertToWorld));

        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Stloc_3))
                .Advance(1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldarg_1),
                    new CodeInstruction(OpCodes.Ldloca_S, 2),
                    new CodeInstruction(OpCodes.Ldloca_S, 3),
                    new CodeInstruction(OpCodes.Call, _convertToWorld))
                .InstructionEnumeration();
        }

        private static void ConvertToWorld(Saber saber, ref Vector3 topPos, ref Vector3 bottomPos)
        {
            Transform playerTransform = saber.transform.parent.parent;
            topPos = playerTransform.TransformPoint(topPos);
            bottomPos = playerTransform.TransformPoint(bottomPos);
        }
    }
}
