namespace NoodleExtensions.HarmonyPatches
{
    using Heck;
    using Heck.Animation;
    using static NoodleExtensions.NoodleObjectDataManager;

    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("HandleNoteControllerNoteDidFinishJump")]
    internal static class BeatmapObjectManagerHandleNoteControllerNoteDidFinishJump
    {
        private static void Prefix(NoteController noteController)
        {
            NoodleObjectData? noodleData = TryGetObjectData<NoodleObjectData>(noteController.noteData);
            if (noodleData?.Track != null)
            {
                foreach (Track track in noodleData.Track)
                {
                    track.RemoveGameObject(noteController.gameObject);
                }
            }
        }
    }

    [HeckPatch(typeof(BeatmapObjectManager))]
    [HeckPatch("HandleObstacleFinishedMovement")]
    internal static class BeatmapObjectManagerHandleObstacleFinishedMovement
    {
        private static void Prefix(ObstacleController obstacleController)
        {
            NoodleObjectData? noodleData = TryGetObjectData<NoodleObjectData>(obstacleController.obstacleData);
            if (noodleData?.Track != null)
            {
                foreach (Track track in noodleData.Track)
                {
                    track.RemoveGameObject(obstacleController.gameObject);
                }
            }
        }
    }
}
