using HarmonyLib;
using Heck;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;

namespace NoodleExtensions.HarmonyPatches.Mirror
{
    [HeckPatch(PatchType.Features)]
    internal class MirroredNoteNoodleTracker : IAffinity
    {
        private readonly CutoutManager _cutoutManager;

        private MirroredNoteNoodleTracker(CutoutManager cutoutManager)
        {
            _cutoutManager = cutoutManager;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MirroredNoteController<INoteMirrorable>), "UpdatePositionAndRotation")]
        private static bool INoteMirrorableUpdateUpdatePositionAndRotationPrefix(Transform ____noteTransform, Transform ____followedNoteTransform)
        {
            return CheckSkip(____noteTransform, ____followedNoteTransform);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MirroredNoteController<IGameNoteMirrorable>), "UpdatePositionAndRotation")]
        private static bool ICubeNoteMirrorableUpdateUpdatePositionAndRotationPrefix(Transform ____noteTransform, Transform ____followedNoteTransform)
        {
            return CheckSkip(____noteTransform, ____followedNoteTransform);
        }

        private static bool CheckSkip(Transform noteTransform, Transform followedNoteTransform)
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

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<INoteMirrorable>), "UpdatePositionAndRotation")]
        private void INoteMirrorableUpdateUpdatePositionAndRotationPostfix(
            MirroredNoteController<INoteMirrorable> __instance,
            INoteMirrorable ___followedNote,
            Transform ____noteTransform,
            Transform ____followedNoteTransform)
        {
            UpdateMirror(
                ____noteTransform,
                __instance,
                (NoteControllerBase)___followedNote);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<IGameNoteMirrorable>), "UpdatePositionAndRotation")]
        private void ICubeNoteMirrorableUpdateUpdatePositionAndRotationPostfix(
            MirroredNoteController<IGameNoteMirrorable> __instance,
            INoteMirrorable ___followedNote,
            Transform ____noteTransform,
            Transform ____followedNoteTransform)
        {
            UpdateMirror(
                ____noteTransform,
                __instance,
                (NoteControllerBase)___followedNote);
        }

        private void UpdateMirror(Transform noteTransform, NoteControllerBase noteController, NoteControllerBase followedNote)
        {
            noteTransform.localScale = followedNote.transform.localScale;

            _cutoutManager.NoteCutoutEffects[noteController].SetCutout(_cutoutManager.NoteCutoutEffects[followedNote].Cutout);
            if (followedNote is IGameNoteMirrorable)
            {
                _cutoutManager.NoteDisappearingArrowWrappers[noteController].SetCutout(_cutoutManager.NoteDisappearingArrowWrappers[followedNote].Cutout);
            }
        }
    }
}
