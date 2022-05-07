using Heck;
using Heck.Animation;
using SiraUtil.Affinity;
using Zenject;

namespace NoodleExtensions.HarmonyPatches.ObjectProcessing
{
    internal class RemoveFromTrackObjects : IAffinity
    {
        private readonly DeserializedData _deserializedData;

        private RemoveFromTrackObjects([Inject(Id = NoodleController.ID)] DeserializedData deserializedData)
        {
            _deserializedData = deserializedData;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(BeatmapObjectManager), nameof(BeatmapObjectManager.HandleNoteControllerNoteDidFinishJump))]
        private void RemoveNoteObjects(NoteController noteController)
        {
            if (!_deserializedData.Resolve(noteController.noteData, out NoodleObjectData? noodleData) || noodleData.Track == null)
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
            if (!_deserializedData.Resolve(obstacleController.obstacleData, out NoodleObjectData? noodleData) || noodleData.Track == null)
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
