namespace NoodleExtensions.Animation
{
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using static NoodleExtensions.NoodleCustomDataManager;
    using static NoodleExtensions.Plugin;

    internal static class AssignPlayerToTrack
    {
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
