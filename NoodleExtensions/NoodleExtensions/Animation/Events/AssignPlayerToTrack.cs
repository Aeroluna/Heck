namespace NoodleExtensions.Animation
{
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.Plugin;

    internal static class AssignPlayerToTrack
    {
        internal static void OnTrackManagerCreated(object trackManager, CustomBeatmapData customBeatmapData)
        {
            CustomEventData[] customEventDatas = customBeatmapData.customEventData;
            foreach (CustomEventData customEventData in customEventDatas)
            {
                if (customEventData.type == "AssignPlayerToTrack")
                {
                    string trackName = Trees.at(customEventData.data, TRACK);
                    ((TrackManager)trackManager).AddTrack(trackName);
                }
            }
        }

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AssignPlayerToTrack")
            {
                Track track = GetTrack(customEventData.data);
                if (track != null)
                {
                    PlayerTrack.AssignTrack(track);
                }
            }
        }
    }
}
