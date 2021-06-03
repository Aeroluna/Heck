namespace NoodleExtensions.HarmonyPatches
{
    using Heck;
    using UnityEngine;

    internal static class MirroredNoteControllerHelper
    {
        internal static bool CheckSkip(Transform noteTransform, Transform followedNoteTransform)
        {
            if (followedNoteTransform.position.y < 0)
            {
                if (noteTransform.gameObject.activeInHierarchy)
                {
                    noteTransform.gameObject.SetActive(false);
                }

                return false;
            }

            if (!noteTransform.gameObject.activeInHierarchy)
            {
                noteTransform.gameObject.SetActive(true);
            }

            return true;
        }

        internal static void UpdateMirror(Transform objectTransform, Transform noteTransform, Transform followedObjectTransform, Transform followedNoteTransform, NoteControllerBase noteController, NoteControllerBase followedNote)
        {
            if (objectTransform.localScale != followedObjectTransform.localScale)
            {
                objectTransform.localScale = followedObjectTransform.localScale;
            }

            if (noteTransform.localScale != followedNoteTransform.localScale)
            {
                noteTransform.localScale = followedNoteTransform.localScale;
            }

            if (CutoutManager.NoteCutoutEffects.TryGetValue(noteController, out CutoutEffectWrapper cutoutEffect))
            {
                if (CutoutManager.NoteCutoutEffects.TryGetValue(followedNote, out CutoutEffectWrapper followedCutoutEffect))
                {
                    cutoutEffect.SetCutout(followedCutoutEffect.Cutout);
                }
            }

            if (CutoutManager.NoteDisappearingArrowWrappers.TryGetValue(noteController, out DisappearingArrowWrapper disappearingArrow))
            {
                if (CutoutManager.NoteDisappearingArrowWrappers.TryGetValue(followedNote, out DisappearingArrowWrapper followedDisappearingArrow))
                {
                    disappearingArrow.SetCutout(followedDisappearingArrow.Cutout);
                }
            }
        }
    }

    // Fuck generics
    [HeckPatch(typeof(MirroredNoteController<INoteMirrorable>))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredNoteControllerINoteMirrorableUpdatePositionAndRotation
    {
        private static bool Prefix(Transform ____noteTransform, Transform ____followedNoteTransform)
        {
            return MirroredNoteControllerHelper.CheckSkip(____noteTransform, ____followedNoteTransform);
        }

        private static void Postfix(MirroredNoteController<INoteMirrorable> __instance, INoteMirrorable ___followedNote, Transform ____objectTransform, Transform ____noteTransform, Transform ____followedObjectTransform, Transform ____followedNoteTransform)
        {
            MirroredNoteControllerHelper.UpdateMirror(____objectTransform, ____noteTransform, ____followedObjectTransform, ____followedNoteTransform, __instance, (NoteControllerBase)___followedNote);
        }
    }

    [HeckPatch(typeof(MirroredNoteController<ICubeNoteMirrorable>))]
    [HeckPatch("UpdatePositionAndRotation")]
    internal static class MirroredNoteControllerICubeNoteMirrorableUpdatePositionAndRotation
    {
        private static bool Prefix(Transform ____noteTransform, Transform ____followedNoteTransform)
        {
            return MirroredNoteControllerHelper.CheckSkip(____noteTransform, ____followedNoteTransform);
        }

        private static void Postfix(MirroredNoteController<ICubeNoteMirrorable> __instance, ICubeNoteMirrorable ___followedNote, Transform ____objectTransform, Transform ____noteTransform, Transform ____followedObjectTransform, Transform ____followedNoteTransform)
        {
            MirroredNoteControllerHelper.UpdateMirror(____objectTransform, ____noteTransform, ____followedObjectTransform, ____followedNoteTransform, __instance, (NoteControllerBase)___followedNote);
        }
    }
}
