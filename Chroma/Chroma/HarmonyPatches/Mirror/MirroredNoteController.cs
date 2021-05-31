namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Heck;

    internal static class MirroredNoteControllerHelper
    {
        internal static void UpdateMirror(NoteControllerBase noteController, NoteControllerBase followedNote)
        {
            if (noteController is MirroredBombNoteController)
            {
                if (followedNote.TryGetBombColorizer(out BombColorizer bombColorizer))
                {
                    noteController.ColorizeBomb(bombColorizer.Color);
                }
            }
            else
            {
                if (followedNote.TryGetNoteColorizer(out NoteColorizer noteColorizer))
                {
                    noteController.ColorizeNote(noteColorizer.Color);
                }
            }
        }
    }

    // Fuck generics
    [HeckPatch(typeof(MirroredNoteController<INoteMirrorable>))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredNoteControllerINoteMirrorableUpdatePositionAndRotation
    {
        private static void Postfix(MirroredNoteController<INoteMirrorable> __instance, INoteMirrorable ___followedNote)
        {
            MirroredNoteControllerHelper.UpdateMirror(__instance, (NoteControllerBase)___followedNote);
        }
    }

    [HeckPatch(typeof(MirroredNoteController<ICubeNoteMirrorable>))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredNoteControllerICubeNoteMirrorableUpdatePositionAndRotation
    {
        private static void Postfix(MirroredNoteController<ICubeNoteMirrorable> __instance, ICubeNoteMirrorable ___followedNote)
        {
            MirroredNoteControllerHelper.UpdateMirror(__instance, (NoteControllerBase)___followedNote);
        }
    }
}
