namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using HarmonyLib;
    using static ChromaObjectDataManager;

    [HarmonyPatch(typeof(BeatEffectSpawner))]
    [HarmonyPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJumpColorizer
    {
        [HarmonyPriority(Priority.Low)]
        private static void Prefix(NoteController noteController)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController))
            {
                NoteColorizer.EnableNoteColorOverride(noteController);
            }
        }

        private static void Postfix()
        {
            NoteColorizer.DisableNoteColorOverride();
        }
    }

    [ChromaPatch(typeof(BeatEffectSpawner))]
    [ChromaPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJump
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            if (!(noteController is MultiplayerConnectedPlayerNoteController))
            {
                ChromaNoteData chromaData = (ChromaNoteData)ChromaObjectDatas[noteController.noteData];
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
