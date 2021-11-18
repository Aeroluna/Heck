namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(ColorManager))]
    [HarmonyPatch("ColorForType")]
    internal static class ColorManagerColorForType
    {
        private static Color? _noteColorOverride;

        internal static void EnableColorOverride(NoteControllerBase noteController)
        {
            _noteColorOverride = noteController.GetNoteColorizer().Color;
        }

        internal static void DisableColorOverride()
        {
            _noteColorOverride = null;
        }

        private static bool Prefix(ref Color __result)
        {
            Color? color = _noteColorOverride;
            if (color.HasValue)
            {
                __result = color.Value;
                return false;
            }

            return true;
        }
    }
}
