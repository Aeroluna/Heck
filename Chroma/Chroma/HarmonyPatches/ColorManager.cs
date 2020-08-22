namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForNoteType")]
    internal static class ColorManagerColorForNoteType
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(ref Color __result, NoteType type)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (type == NoteType.NoteA || type == NoteType.NoteB)
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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(SaberType type, ref Color __result)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static bool Prefix(ColorManager __instance, SaberType type, ref Color __result)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            Color rgbColor = __instance.ColorForSaberType(type);
            Color.RGBToHSV(rgbColor, out float h, out float s, out _);
            __result = Color.HSVToRGB(h, s, 1);
            return false;
        }
    }
}
