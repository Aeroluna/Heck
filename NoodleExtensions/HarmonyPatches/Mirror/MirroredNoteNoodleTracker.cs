using HarmonyLib;
using Heck;
using Heck.Animation;
using NoodleExtensions.Managers;
using SiraUtil.Affinity;
using UnityEngine;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.Mirror
{
    [HeckPatch(PatchType.Features)]
    internal class MirroredNoteNoodleTracker : IAffinity
    {
        private readonly CustomData _customData;
        private readonly CutoutManager _cutoutManager;

        private MirroredNoteNoodleTracker([Inject(Id = NoodleController.ID)] CustomData customData, CutoutManager cutoutManager)
        {
            _customData = customData;
            _cutoutManager = cutoutManager;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MirroredNoteController<INoteMirrorable>), "UpdatePositionAndRotation")]
        [HarmonyPatch(typeof(MirroredNoteController<ICubeNoteMirrorable>), "UpdatePositionAndRotation")]
        private static bool UpdatePositionAndRotationPrefix(Transform ____noteTransform, Transform ____followedNoteTransform)
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
            Transform ____objectTransform,
            Transform ____noteTransform,
            Transform ____followedObjectTransform,
            Transform ____followedNoteTransform)
        {
            UpdateMirror(
                ____objectTransform,
                ____noteTransform,
                ____followedObjectTransform,
                ____followedNoteTransform,
                __instance,
                (NoteControllerBase)___followedNote);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<ICubeNoteMirrorable>), "UpdatePositionAndRotation")]
        private void ICubeNoteMirrorableUpdateUpdatePositionAndRotationPostfix(
            MirroredNoteController<ICubeNoteMirrorable> __instance,
            INoteMirrorable ___followedNote,
            Transform ____objectTransform,
            Transform ____noteTransform,
            Transform ____followedObjectTransform,
            Transform ____followedNoteTransform)
        {
            UpdateMirror(
                ____objectTransform,
                ____noteTransform,
                ____followedObjectTransform,
                ____followedNoteTransform,
                __instance,
                (NoteControllerBase)___followedNote);
        }

        private void UpdateMirror(Transform objectTransform, Transform noteTransform, Transform followedObjectTransform, Transform followedNoteTransform, NoteControllerBase noteController, NoteControllerBase followedNote)
        {
            if (objectTransform.localScale != followedObjectTransform.localScale)
            {
                objectTransform.localScale = followedObjectTransform.localScale;
            }

            if (noteTransform.localScale != followedNoteTransform.localScale)
            {
                noteTransform.localScale = followedNoteTransform.localScale;
            }

            if (_cutoutManager.NoteCutoutEffects.TryGetValue(noteController, out CutoutEffectWrapper cutoutEffect))
            {
                if (_cutoutManager.NoteCutoutEffects.TryGetValue(followedNote, out CutoutEffectWrapper followedCutoutEffect))
                {
                    cutoutEffect.SetCutout(followedCutoutEffect.Cutout);
                }
            }

            if (!_cutoutManager.NoteDisappearingArrowWrappers.TryGetValue(noteController, out DisappearingArrowWrapper disappearingArrow))
            {
                return;
            }

            if (_cutoutManager.NoteDisappearingArrowWrappers.TryGetValue(followedNote, out DisappearingArrowWrapper followedDisappearingArrow))
            {
                disappearingArrow.SetCutout(followedDisappearingArrow.Cutout);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<INoteMirrorable>), "Mirror")]
        private void INoteMirrorableMirror(MirroredNoteController<INoteMirrorable> __instance)
        {
            AddToTrack(__instance.noteData, __instance.gameObject);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<ICubeNoteMirrorable>), "Mirror")]
        private void ICubeNoteMirrorableMirror(MirroredNoteController<ICubeNoteMirrorable> __instance)
        {
            AddToTrack(__instance.noteData, __instance.gameObject);
        }

        private void AddToTrack(NoteData noteData, GameObject gameObject)
        {
            if (!_customData.Resolve(noteData, out NoodleNoteData? noodleData) || noodleData.Track == null)
            {
                return;
            }

            foreach (Track track in noodleData.Track)
            {
                // add to gameobjects
                track.AddGameObject(gameObject);
            }
        }
    }
}
