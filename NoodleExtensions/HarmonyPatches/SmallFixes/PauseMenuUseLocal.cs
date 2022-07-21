using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(PatchType.Features)]
    internal static class PauseMenuUseLocal
    {
        private static readonly MethodInfo _eulerAnglesSetter = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.eulerAngles));
        private static readonly MethodInfo _localEulerAnglesSetter = AccessTools.PropertySetter(typeof(Transform), nameof(Transform.localEulerAngles));

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PauseMenuManager), nameof(PauseMenuManager.ShowMenu))]
        private static IEnumerable<CodeInstruction> Replace(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _eulerAnglesSetter))
                .SetOperandAndAdvance(_localEulerAnglesSetter)
                .InstructionEnumeration();
        }
    }
}
