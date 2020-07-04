namespace NoodleExtensions.Animation
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.Plugin;

    internal static class AssignPlayerToTrack
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AssignPlayerToTrack")
            {
                Track track = GetTrack(customEventData.data);
                if (track == null)
                {
                    NoodleLogger.Log("Creating track", IPA.Logging.Logger.Level.Info);
                    string trackName = Trees.at(customEventData.data, TRACK);
                    if (trackName != null)
                    {
                        track = TrackManager.Instance.AddTrack(trackName);
                    }
                }

                PlayerTrack.SpawnComponent(track);

                return;
            }
        }
    }
}
