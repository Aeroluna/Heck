namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Chroma.Utils;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using UnityEngine;

    [HarmonyPatch(typeof(BombNoteController))]
    [HarmonyPatch("Init")]
    internal static class BombNoteControllerInitColorizer
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(BombNoteController __instance)
        {
            BombColorizer.BNCStart(__instance);
        }
    }

    [ChromaPatch(typeof(BombNoteController))]
    [ChromaPatch("Init")]
    internal static class BombNoteControllerInit
    {
        private static void Prefix(BombNoteController __instance, NoteData noteData)
        {
            // They said it couldn't be done, they called me a madman
            Color? color = null;

            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                color = ChromaUtils.GetColorFromData(dynData) ?? color;
            }

            if (color.HasValue)
            {
                __instance.SetBombColor(color);
            }
            else
            {
                __instance.Reset();
            }
        }
    }
}
