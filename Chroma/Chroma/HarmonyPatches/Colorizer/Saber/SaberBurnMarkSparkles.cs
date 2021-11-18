namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(SaberBurnMarkSparkles))]
    [HarmonyPatch("Start")]
    internal static class SaberBurnMarkSparklesStart
    {
        private static ParticleSystem[]? _burnMarksPS;

        internal static void OnSaberColorChanged(SaberType saberType, Color color)
        {
            if (_burnMarksPS != null)
            {
                ParticleSystem.MainModule main = _burnMarksPS[(int)saberType].main;
                main.startColor = color;
            }
        }

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
        private static void Postfix()
        {
            SaberColorizer.SaberColorChanged -= SaberBurnMarkAreaStart.OnSaberColorChanged;
        }
    }
}
