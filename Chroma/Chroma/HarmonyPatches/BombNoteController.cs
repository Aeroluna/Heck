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
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(BombNoteController __instance)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            BombColorizer.BNCStart(__instance);
        }
    }

    [ChromaPatch(typeof(BombNoteController))]
    [ChromaPatch("Init")]
    internal static class BombNoteControllerInit
    {
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Prefix(BombNoteController __instance, NoteData noteData)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
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
