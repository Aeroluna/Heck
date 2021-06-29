namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Heck;
    using static Chroma.ChromaObjectDataManager;

    [HeckPatch(typeof(BombNoteController))]
    [HeckPatch("Init")]
    internal static class BombNoteControllerInit
    {
        private static void Postfix(BombNoteController __instance, NoteData noteData)
        {
            // They said it couldn't be done, they called me a madman
            ChromaObjectData? chromaData = TryGetObjectData<ChromaObjectData>(noteData);
            if (chromaData == null)
            {
                return;
            }

            __instance.ColorizeBomb(chromaData.Color);
        }
    }
}
