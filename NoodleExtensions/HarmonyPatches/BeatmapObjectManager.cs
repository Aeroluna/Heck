using Heck;
using Heck.Animation;
using JetBrains.Annotations;
using static NoodleExtensions.NoodleCustomDataManager;

namespace NoodleExtensions.HarmonyPatches
{
    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("HandleNoteControllerNoteDidFinishJump")]
    internal static class BeatmapObjectManagerHandleNoteControllerNoteDidFinishJump
    {
        [UsedImplicitly]
        private static void Prefix(NoteController noteController)
        {
            NoodleObjectData? noodleData = TryGetObjectData<NoodleObjectData>(noteController.noteData);
            if (noodleData?.Track == null)
            {
                return;
            }

            foreach (Track track in noodleData.Track)
            {
                track.RemoveGameObject(noteController.gameObject);
            }
        }
    }

    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("HandleObstacleFinishedMovement")]
    internal static class BeatmapObjectManagerHandleObstacleFinishedMovement
    {
        [UsedImplicitly]
        private static void Prefix(ObstacleController obstacleController)
        {
            NoodleObjectData? noodleData = TryGetObjectData<NoodleObjectData>(obstacleController.obstacleData);
            if (noodleData?.Track == null)
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
