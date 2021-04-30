namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using static NoodleExtensions.Animation.NoodleEventDataManager;
    using static NoodleExtensions.Plugin;

    internal class AssignTrackParent
    {
        internal static void OnTrackManagerCreated(object trackManager, CustomBeatmapData customBeatmapData)
        {
            List<CustomEventData> customEventsData = customBeatmapData.customEventsData;
            foreach (CustomEventData customEventData in customEventsData)
            {
                if (customEventData.type == ASSIGNTRACKPARENT)
                {
                    string trackName = Trees.at(customEventData.data, "_parentTrack");
                    ((TrackManager)trackManager).AddTrack(trackName);
                }
            }
        }

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == ASSIGNTRACKPARENT)
            {
                NoodleParentTrackEventData noodleData = TryGetEventData<NoodleParentTrackEventData>(customEventData);
                if (noodleData != null)
                {
                    IEnumerable<Track> tracks = noodleData.ChildrenTracks;
                    Track parentTrack = noodleData.ParentTrack;
                    if (tracks != null && parentTrack != null)
                    {
                        ParentObject.AssignTrack(tracks, parentTrack, noodleData.Position, noodleData.Rotation, noodleData.LocalRotation, noodleData.Scale);
                    }
                    else
                    {
                        Logger.Log($"Missing _parentTrack or _childrenTracks!", IPA.Logging.Logger.Level.Error);
                    }
                }
            }
        }
    }
}
