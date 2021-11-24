using HarmonyLib;
using Heck;
using static Chroma.ChromaCustomDataManager;

namespace Chroma.HarmonyPatches
{
    [HeckPatch(typeof(BeatEffectSpawner))]
    [HeckPatch("HandleNoteDidStartJump")]
    internal static class BeatEffectSpawnerHandleNoteDidStartJump
    {
        [HarmonyPriority(Priority.High)]
        private static bool Prefix(NoteController noteController)
        {
            ChromaNoteData? chromaData = TryGetObjectData<ChromaNoteData>(noteController.noteData);
            bool? disable = chromaData?.DisableSpawnEffect;
            return disable is not true;
        }
    }
}
