namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(SaberClashEffect))]
    [HarmonyPatch("Start")]
    internal static class SaberClashEffectStart
    {
        private static void Postfix(SaberClashEffect __instance, ParticleSystem ____sparkleParticleSystem, ParticleSystem ____glowParticleSystem, ColorManager ____colorManager)
        {
            __instance.gameObject.AddComponent<ChromaClashEffectController>().Init(____sparkleParticleSystem, ____glowParticleSystem, ____colorManager);
        }
    }
}
