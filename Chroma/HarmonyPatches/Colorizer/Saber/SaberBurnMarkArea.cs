using Chroma.Colorizer;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer.Saber
{
    [HarmonyPatch(typeof(SaberBurnMarkArea))]
    [HarmonyPatch("Start")]
    internal static class SaberBurnMarkAreaStart
    {
        private static LineRenderer[]? _lineRenderers;

        internal static void OnSaberColorChanged(SaberType saberType, Color color)
        {
            if (_lineRenderers == null)
            {
                return;
            }

            int intType = (int)saberType;
            _lineRenderers[intType].startColor = color;
            _lineRenderers[intType].endColor = color;
        }

        [UsedImplicitly]
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
        [UsedImplicitly]
        private static void Postfix()
        {
            SaberColorizer.SaberColorChanged -= SaberBurnMarkAreaStart.OnSaberColorChanged;
        }
    }
}
