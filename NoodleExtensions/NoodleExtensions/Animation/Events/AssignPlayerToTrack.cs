namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using static NoodleExtensions.NoodleCustomDataManager;
    using static NoodleExtensions.Plugin;

    internal static class AssignPlayerToTrack
    {
        internal static void OnTrackManagerCreated(TrackBuilder trackManager, CustomBeatmapData customBeatmapData)
        {
            List<CustomEventData> customEventsData = customBeatmapData.customEventsData;
            foreach (CustomEventData customEventData in customEventsData)
            {
                if (customEventData.type == ASSIGNPLAYERTOTRACK)
                {
                    string? trackName = customEventData.data.Get<string>(TRACK);
                    if (trackName != null)
                    {
                        trackManager.AddTrack(trackName);
                    }
                }
            }
        }

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == ASSIGNPLAYERTOTRACK)
            {
                NoodlePlayerTrackEventData? noodleData = TryGetEventData<NoodlePlayerTrackEventData>(customEventData);
                if (noodleData != null)
                {
                    Track track = noodleData.Track;
                    if (track != null)
                    {
                        PlayerTrack.AssignTrack(track);
                    }
                }
            }
        }
    }
}
