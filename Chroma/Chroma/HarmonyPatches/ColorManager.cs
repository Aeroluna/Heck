namespace Chroma.HarmonyPatches
{
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForNoteType")]
    internal class ColorManagerColorForNoteType
    {
#pragma warning disable SA1313
        private static bool Prefix(ref Color __result)
#pragma warning restore SA1313
        {
            Color? color = NoteColorManager.NoteColorOverride;
            if (color.HasValue)
            {
                __result = color.Value;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForSaberType")]
    internal class ColorManagerColorForSaberType
    {
#pragma warning disable SA1313
        private static bool Prefix(SaberType type, ref Color __result)
#pragma warning restore SA1313
        {
            bool warm = type == SaberType.SaberA;

            Color? color = warm ? Extensions.SaberColorizer.CurrentAColor : Extensions.SaberColorizer.CurrentBColor;
            if (color.HasValue)
            {
                __result = color.Value;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("EffectsColorForSaberType")]
    internal class ColorManagerEffectsColorForSaberType
    {
#pragma warning disable SA1313
        private static bool Prefix(ColorManager __instance, SaberType type, ref Color __result)
#pragma warning restore SA1313
        {
            Color rgbColor = __instance.ColorForSaberType(type);
            Color.RGBToHSV(rgbColor, out float h, out float s, out float v);
            v = 1f;
            __result = Color.HSVToRGB(h, s, v);
            return false;
        }
    }
}
