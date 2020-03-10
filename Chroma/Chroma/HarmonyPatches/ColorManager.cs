using Harmony;
using UnityEngine;

namespace Chroma.HarmonyPatches
{
    [HarmonyPriority(Priority.Low)]
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForNoteType")]
    internal class ColorManagerColorForNoteType
    {
        public static bool Prefix(ref Color __result, ref NoteType type)
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

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForSaberType")]
    internal class ColorManagerColorForSaberType
    {
        public static bool Prefix(ref Saber.SaberType type, ref Color __result)
        {
            bool warm = type == Saber.SaberType.SaberA;

            Color? color = warm ? Extensions.SaberColourizer.currentAColor : Extensions.SaberColourizer.currentBColor;
            if (color == null) color = warm ? ColourManager.A : ColourManager.B;
            if (color.HasValue)
            {
                __result = color.Value;
                return false;
            }

            return true;
        }
    }

    [HarmonyPriority(Priority.High)]
    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("EffectsColorForSaberType")]
    internal class ColorManagerEffectsColorForSaberType
    {
        public static bool Prefix(ColorManager __instance, ref Saber.SaberType type, ref Color __result)
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