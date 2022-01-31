using Heck;
using Heck.Animation;
using SiraUtil.Affinity;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    internal class RemoveFromTrackObjects : IAffinity
    {
        private readonly CustomData _customData;

        private RemoveFromTrackObjects([Inject(Id = NoodleController.ID)] CustomData customData)
        {
            _customData = customData;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.HandleNoteControllerNoteDidFinishJump))]
        private void RemoveNoteObjects(NoteController noteController)
        {
            if (!_customData.Resolve(noteController.noteData, out NoodleObjectData? noodleData) || noodleData.Track == null)
            {
                return;
            }

            foreach (Track track in noodleData.Track)
            {
                track.RemoveGameObject(noteController.gameObject);
            }
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectManager), "HandleObstacleFinishedMovement")]
        private void RemoveObstacleObjects(ObstacleController obstacleController)
        {
            if (!_customData.Resolve(obstacleController.obstacleData, out NoodleObjectData? noodleData) || noodleData.Track == null)
            {
                return;
            }

            foreach (Track track in noodleData.Track)
            {
                track.RemoveGameObject(obstacleController.gameObject);
            }
        }
    }
}
