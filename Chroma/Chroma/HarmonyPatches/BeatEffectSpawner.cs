namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;

    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJump")]
    internal static class HandleNoteDidStartJumpColorizer
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController)
        {
            NoteColorizer.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorizer.DisableNoteColorOverride();
        }
    }

    [ChromaPatch(typeof(BeatEffectSpawner))]
    [ChromaPatch("HandleNoteDidStartJump")]
    internal static class HandleNoteDidStartJump
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            if (noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                bool? disable = Trees.at(dynData, "_disableSpawnEffect");
                if (disable.HasValue && disable == true)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
