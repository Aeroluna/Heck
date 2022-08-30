using HarmonyLib;
using Heck;
using UnityEngine;

namespace Chroma.HarmonyPatches.EnvironmentComponent
{
    [HeckPatch(PatchType.Environment)]
    [HarmonyPatch(typeof(BloomFogEnvironment))]
    internal static class BloomFogParamsInstantiator
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BloomFogEnvironment.OnEnable))]
        private static void Prefix(ref BloomFogEnvironmentParams ____fogParams)
        {
            ____fogParams = Object.Instantiate(____fogParams);
        }
    }
}
