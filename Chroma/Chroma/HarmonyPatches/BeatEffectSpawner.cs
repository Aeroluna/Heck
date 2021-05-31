namespace Chroma.HarmonyPatches
{
    using HarmonyLib;
    using Heck;
    using static ChromaObjectDataManager;

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
