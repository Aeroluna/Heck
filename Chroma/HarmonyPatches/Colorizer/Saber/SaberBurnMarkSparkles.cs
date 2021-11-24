using Chroma.Colorizer;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer.Saber
{
    [HarmonyPatch(typeof(SaberBurnMarkSparkles))]
    [HarmonyPatch("Start")]
    internal static class SaberBurnMarkSparklesStart
    {
        private static ParticleSystem[]? _burnMarksPS;

        internal static void OnSaberColorChanged(SaberType saberType, Color color)
        {
            if (_burnMarksPS == null)
            {
                return;
            }

            ParticleSystem.MainModule main = _burnMarksPS[(int)saberType].main;
            main.startColor = color;
        }

        [UsedImplicitly]
        private static void Postfix(ParticleSystem[] ____burnMarksPS)
        {
            _burnMarksPS = ____burnMarksPS;
            SaberColorizer.SaberColorChanged += OnSaberColorChanged;
        }
    }

    [HarmonyPatch(typeof(SaberBurnMarkSparkles))]
    [HarmonyPatch("OnDestroy")]
    internal static class SaberBurnMarkSparklesOnDestroy
    {
        [UsedImplicitly]
        private static void Postfix()
        {
            SaberColorizer.SaberColorChanged -= SaberBurnMarkAreaStart.OnSaberColorChanged;
        }
    }
}
