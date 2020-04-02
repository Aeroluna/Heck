using HarmonyLib;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForNoteType")]
    internal class ColorManagerColorForNoteType
    {
        private static bool Prefix(ref Color __result, NoteType type)
        {
            Color? c = ColourManager.GetNoteTypeColourOverride(type);
            if (c.HasValue)
            {
                __result = c.Value;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForSaberType")]
    internal class ColorManagerColorForSaberType
    {
        private static bool Prefix(SaberType type, ref Color __result)
        {
            bool warm = type == SaberType.SaberA;

            Color? color = warm ? Extensions.SaberColourizer.currentAColor : Extensions.SaberColourizer.currentBColor;
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
        private static bool Prefix(ColorManager __instance, SaberType type, ref Color __result)
        {
            Color rgbColor = __instance.ColorForSaberType(type);
            float h;
            float s;
            float v;
            Color.RGBToHSV(rgbColor, out h, out s, out v);
            v = 1f;
            __result = Color.HSVToRGB(h, s, v);
            return false;
        }
    }
}