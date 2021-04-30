namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using Heck;
    using static ChromaObjectDataManager;

    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJumpColorizer
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

    [HeckPatch(typeof(BeatEffectSpawner))]
    [HeckPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJump
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            ChromaNoteData chromaData = TryGetObjectData<ChromaNoteData>(noteController.noteData);
            if (chromaData != null)
            {
                bool? disable = chromaData.DisableSpawnEffect;
                if (disable.HasValue && disable == true)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
