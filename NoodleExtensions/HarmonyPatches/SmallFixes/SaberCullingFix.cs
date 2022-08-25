using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes
{
    [HeckPatch(PatchType.Features)]
    [HarmonyPatch(typeof(SaberTrailRenderer))]
    internal static class SaberCullingFix
    {
        private static readonly MethodInfo _boundsSetter = AccessTools.PropertySetter(typeof(Mesh), nameof(Mesh.bounds));
        private static readonly Bounds _bounds = new(Vector3.zero, Vector3.positiveInfinity);

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(SaberTrailRenderer.UpdateMesh))]
        private static IEnumerable<CodeInstruction> PreventBoundsUpdate(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                /*
                 * this._mesh.triangles = this._indices;
                 * -- this._mesh.bounds = SaberTrailRenderer._bounds;
                 */
                .MatchForward(false, new CodeMatch(OpCodes.Callvirt, _boundsSetter))
                .Advance(-3)
                .RemoveInstructions(4)
                .InstructionEnumeration();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaberTrailRenderer), nameof(SaberTrailRenderer.UpdateMesh))]
        private static void InfiniteBounds(Mesh ____mesh)
        {
            ____mesh.bounds = _bounds;
        }
    }
}
