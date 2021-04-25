namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using UnityEngine;
    using static ChromaObjectDataManager;

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
            ChromaObjectData chromaData = TryGetObjectData<ChromaObjectData>(noteData);
            if (chromaData == null)
            {
                return;
            }

            Color? color = chromaData.Color;

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
