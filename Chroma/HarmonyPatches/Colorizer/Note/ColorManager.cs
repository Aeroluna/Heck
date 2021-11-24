using Chroma.Colorizer;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace Chroma.HarmonyPatches.Colorizer.Note
{
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

        [UsedImplicitly]
        private static bool Prefix(ref Color __result)
        {
            Color? color = _noteColorOverride;
            if (!color.HasValue)
            {
                return true;
            }

            __result = color.Value;
            return false;
        }
    }
}
