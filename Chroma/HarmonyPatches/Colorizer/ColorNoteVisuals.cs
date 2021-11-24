using Chroma.Colorizer;
using Chroma.Settings;
using Heck;
using JetBrains.Annotations;
using static Chroma.ChromaCustomDataManager;

namespace Chroma.HarmonyPatches.Colorizer
{
    [HeckPatch(typeof(ColorNoteVisuals))]
    [HeckPatch("HandleNoteControllerDidInit")]
    internal static class ColorNoteVisualsHandleNoteControllerDidInit
    {
        [UsedImplicitly]
        private static void Postfix(NoteControllerBase noteController)
        {
            if (ChromaConfig.Instance.NoteColoringDisabled || noteController is not NoteController)
            {
                return;
            }

            ChromaNoteData? chromaData = TryGetObjectData<ChromaNoteData>(noteController.noteData);
            if (chromaData != null)
            {
                noteController.ColorizeNote(chromaData.Color);
            }
        }
    }
}
