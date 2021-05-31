namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Heck;
    using static ChromaObjectDataManager;

    [HeckPatch(typeof(ColorNoteVisuals))]
    [HeckPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInit
    {
        private static void Postfix(NoteController noteController)
        {
            ChromaNoteData chromaData = TryGetObjectData<ChromaNoteData>(noteController.noteData);
            if (chromaData == null)
            {
                return;
            }

            noteController.ColorizeNote(chromaData.Color);
        }
    }
}
