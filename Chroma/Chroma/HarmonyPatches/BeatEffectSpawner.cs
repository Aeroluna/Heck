namespace Chroma.HarmonyPatches
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;

    [ChromaPatch(typeof(BeatEffectSpawner))]
    [ChromaPatch("HandleNoteDidStartJumpEvent")]
    internal static class HandleNoteDidStartJumpEvent
    {
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

            NoteColorManager.EnableNoteColorOverride(noteController);
            return true;
        }

        private static void Postfix()
        {
            NoteColorManager.DisableNoteColorOverride();
        }
    }
}
