namespace Chroma.HarmonyPatches
{
    [ChromaPatch(typeof(NoteCutEffectSpawner))]
    [ChromaPatch("SpawnNoteCutEffect")]
    internal class NoteCutEffectSpawnerSpawnNoteCutEffect
    {
        private static void Prefix(NoteController noteController)
        {
            NoteColorManager.EnableNoteColorOverride(noteController);
        }

        private static void Postfix()
        {
            NoteColorManager.DisableNoteColorOverride();
        }
    }
}
