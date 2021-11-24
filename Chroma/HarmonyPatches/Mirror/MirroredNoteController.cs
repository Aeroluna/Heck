using Chroma.Colorizer;
using HarmonyLib;
using JetBrains.Annotations;

namespace Chroma.HarmonyPatches.Mirror
{
    internal static class MirroredNoteControllerHelper
    {
        internal static void UpdateMirror(NoteControllerBase noteController, NoteControllerBase followedNote)
        {
            if (noteController is MirroredBombNoteController)
            {
                noteController.ColorizeBomb(followedNote.GetBombColorizer().Color);
            }
            else
            {
                noteController.ColorizeNote(followedNote.GetNoteColorizer().Color);
            }
        }
    }

    // Fuck generics
    [HarmonyPatch(typeof(MirroredNoteController<INoteMirrorable>))]
    [HarmonyPatch("UpdatePositionAndRotation")]
    internal static class MirroredNoteControllerINoteMirrorableUpdatePositionAndRotation
    {
        [UsedImplicitly]
        private static void Postfix(MirroredNoteController<INoteMirrorable> __instance, INoteMirrorable ___followedNote)
        {
            MirroredNoteControllerHelper.UpdateMirror(__instance, (NoteControllerBase)___followedNote);
        }
    }

    [HarmonyPatch(typeof(MirroredNoteController<ICubeNoteMirrorable>))]
    [HarmonyPatch("UpdatePositionAndRotation")]
    internal static class MirroredNoteControllerICubeNoteMirrorableUpdatePositionAndRotation
    {
        [UsedImplicitly]
        private static void Postfix(MirroredNoteController<ICubeNoteMirrorable> __instance, ICubeNoteMirrorable ___followedNote)
        {
            MirroredNoteControllerHelper.UpdateMirror(__instance, (NoteControllerBase)___followedNote);
        }
    }
}
