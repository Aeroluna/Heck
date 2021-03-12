namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForType")]
    internal static class ColorManagerColorForType
    {
        private static bool Prefix(ref Color __result, ColorType type)
        {
            if (type == ColorType.ColorA || type == ColorType.ColorB)
            {
                Color? color = NoteColorizer.NoteColorOverride[(int)type];
                if (color.HasValue)
                {
                    __result = color.Value;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForSaberType")]
    internal static class ColorManagerColorForSaberType
    {
        private static bool Prefix(SaberType type, ref Color __result)
        {
            Color? color = SaberColorizer.SaberColorOverride[(int)type];
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
    internal static class ColorManagerEffectsColorForSaberType
    {
        private static bool Prefix(ColorManager __instance, SaberType type, ref Color __result)
        {
            Color rgbColor = __instance.ColorForSaberType(type);
            Color.RGBToHSV(rgbColor, out float h, out float s, out _);
            __result = Color.HSVToRGB(h, s, 1);
            return false;
        }
    }
}
