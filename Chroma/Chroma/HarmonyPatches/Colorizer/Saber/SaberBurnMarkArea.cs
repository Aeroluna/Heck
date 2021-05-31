namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal static class SaberBurnMarkAreaStart
    {
        private static LineRenderer[] _lineRenderers;

        internal static void OnSaberColorChanged(SaberType saberType, Color color)
        {
            int intType = (int)saberType;
            _lineRenderers[intType].startColor = color;
            _lineRenderers[intType].endColor = color;
        }

        private static void Postfix(LineRenderer[] ____lineRenderers)
        {
            _lineRenderers = ____lineRenderers;
            SaberColorizer.SaberColorChanged += OnSaberColorChanged;
        }
    }

    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("OnDestroy")]
    internal static class SaberBurnMarkAreaOnDestroy
    {
        private static void Postfix()
        {
            SaberColorizer.SaberColorChanged -= SaberBurnMarkAreaStart.OnSaberColorChanged;
        }
    }
}
