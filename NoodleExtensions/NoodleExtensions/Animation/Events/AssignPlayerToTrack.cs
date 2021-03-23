namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using static NoodleExtensions.Animation.NoodleEventDataManager;
    using static NoodleExtensions.Plugin;

    internal static class AssignPlayerToTrack
    {
        internal static void OnTrackManagerCreated(object trackManager, CustomBeatmapData customBeatmapData)
        {
            List<CustomEventData> customEventsData = customBeatmapData.customEventsData;
            foreach (CustomEventData customEventData in customEventsData)
            {
                if (customEventData.type == ASSIGNPLAYERTOTRACK)
                {
                    string trackName = Trees.at(customEventData.data, TRACK);
                    ((TrackManager)trackManager).AddTrack(trackName);
                }
            }
        }

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == ASSIGNPLAYERTOTRACK)
            {
                NoodlePlayerTrackEventData noodleData = (NoodlePlayerTrackEventData)NoodleEventDatas[customEventData];
                Track track = noodleData.Track;
                if (track != null)
                {
                    PlayerTrack.AssignTrack(track);
                }
            }
        }
    }
}
