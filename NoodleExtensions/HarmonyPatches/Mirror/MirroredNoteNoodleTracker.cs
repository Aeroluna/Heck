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
                ____followedNoteTransform,
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
                ____followedNoteTransform,
                __instance,
                (NoteControllerBase)___followedNote);
        }

        private void UpdateMirror(Transform noteTransform, Transform followedNoteTransform, NoteControllerBase noteController, NoteControllerBase followedNote)
        {
            if (noteTransform.localScale != followedNoteTransform.localScale)
            {
                noteTransform.localScale = followedNoteTransform.localScale;
            }

            _cutoutManager.NoteCutoutEffects[noteController].SetCutout(_cutoutManager.NoteCutoutEffects[followedNote].Cutout);
            if (followedNote is IGameNoteMirrorable)
            {
                _cutoutManager.NoteDisappearingArrowWrappers[noteController].SetCutout(_cutoutManager.NoteDisappearingArrowWrappers[followedNote].Cutout);
            }
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<INoteMirrorable>), "Mirror")]
        private void INoteMirrorableMirror(MirroredNoteController<INoteMirrorable> __instance)
        {
            AddToTrack(__instance.noteData, __instance.gameObject);
        }

        [AffinityPostfix]
        [AffinityPatch(typeof(MirroredNoteController<IGameNoteMirrorable>), "Mirror")]
        private void ICubeNoteMirrorableMirror(MirroredNoteController<IGameNoteMirrorable> __instance)
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
