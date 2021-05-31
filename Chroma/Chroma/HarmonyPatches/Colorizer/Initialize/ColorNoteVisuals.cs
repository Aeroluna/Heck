namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("Awake")]
    internal static class ColorNoteVisualsAwake
    {
        [HarmonyPriority(Priority.High)]
        private static void Prefix(ColorNoteVisuals __instance, NoteControllerBase ____noteController)
        {
            new NoteColorizer(__instance, ____noteController);
        }
    }

    [HarmonyPatch(typeof(ColorNoteVisuals))]
    [HarmonyPatch("OnDestroy")]
    internal static class ColorNoteVisualsOnDestroy
    {
        [HarmonyPriority(Priority.Low)]
        private static void Postfix(NoteControllerBase ____noteController)
        {
            NoteColorizer.Colorizers.Remove(____noteController);
        }
    }
}
