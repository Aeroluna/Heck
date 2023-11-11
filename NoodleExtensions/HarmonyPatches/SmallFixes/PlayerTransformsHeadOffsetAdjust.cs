using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(PlayerTransforms))]
    internal static class PlayerTransformsUseParent
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PlayerTransforms.Awake))]
        private static void Postfix(
            ref bool ____useOriginParentTransformForPseudoLocalCalculations)
        {
            ____useOriginParentTransformForPseudoLocalCalculations = false;
        }
    }

    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(PlayerTransforms))]
    internal static class PlayerTransformsHeadOffsetAdjust
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PlayerTransforms.HeadOffsetZ))]
        private static bool Prefix(
            Quaternion noteInverseWorldRotation,
            Vector3 ____headPseudoLocalPos,
            Transform ____originParentTransform,
            ref float __result)
        {
            // get magnitude in direction we care about rather than just z
            __result = Vector3.Dot(noteInverseWorldRotation * ____headPseudoLocalPos, ____originParentTransform.forward);
            return false;
        }
    }
}
