using Chroma.Settings;
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
            if (c != null)
            {
                __result = (Color)c;
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

            if (ColourManager.TechnicolourSabers)
            {
                if (ChromaConfig.TechnicolourSabersStyle != ColourManager.TechnicolourStyle.GRADIENT)
                {
                    __result = ColourManager.GetTechnicolour(warm, Time.time, ChromaConfig.TechnicolourSabersStyle);
                    return false;
                }
                else
                {
                    __result = (Color)VFX.TechnicolourController.Instance.rainbowSaberColours[type == Saber.SaberType.SaberA ? 0 : 1];
                }
            }

            Color? color = warm ? Extensions.SaberColourizer.currentAColor : Extensions.SaberColourizer.currentBColor;
            if (color == null) color = warm ? ColourManager.A : ColourManager.B;
            if (color != null)
            {
                __result = (Color)color;
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