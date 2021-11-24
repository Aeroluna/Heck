using Chroma.Colorizer.Monobehaviours;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer.Saber
{
    [HarmonyPatch(typeof(SaberClashEffect))]
    [HarmonyPatch("Start")]
    internal static class SaberClashEffectStart
    {
        [UsedImplicitly]
        private static void Postfix(SaberClashEffect __instance, ParticleSystem ____sparkleParticleSystem, ParticleSystem ____glowParticleSystem, ColorManager ____colorManager)
        {
            __instance.gameObject.AddComponent<ChromaClashEffectController>().Init(____sparkleParticleSystem, ____glowParticleSystem, ____colorManager);
        }
    }
}
