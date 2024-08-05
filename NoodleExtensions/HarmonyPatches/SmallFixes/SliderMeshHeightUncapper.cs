using HarmonyLib;
using Heck;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.SmallFixes;

[HeckPatch(PatchType.Features)]
[HarmonyPatch(typeof(EnvironmentSceneSetup))]
internal static class SliderMeshHeightUncapper
{
    private static readonly int _trackLaneYPositionPropertyId = Shader.PropertyToID("_TrackLaneYPosition");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnvironmentSceneSetup.InstallBindings))]
    private static void ShaderFloatSet()
    {
        Shader.SetGlobalFloat(_trackLaneYPositionPropertyId, -1000000f);
    }
}
