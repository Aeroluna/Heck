namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Heck;
    using static Chroma.ChromaCustomDataManager;

    [HeckPatch(typeof(ColorNoteVisuals))]
    [HeckPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInit
    {
        private static void Postfix(NoteControllerBase noteController)
        {
            if (!Settings.ChromaConfig.Instance.NoteColoringDisabled && noteController is NoteController)
            {
                ChromaNoteData? chromaData = TryGetObjectData<ChromaNoteData>(noteController.noteData);
                if (chromaData != null)
                {
                    noteController.ColorizeNote(chromaData.Color);
                }
            }

            return;
        }
    }
}
