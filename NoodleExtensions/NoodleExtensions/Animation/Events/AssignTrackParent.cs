namespace NoodleExtensions.Animation
{
    using System.Collections.Generic;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using static NoodleExtensions.NoodleCustomDataManager;
    using static NoodleExtensions.Plugin;

    internal class AssignTrackParent
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == ASSIGNTRACKPARENT)
            {
                NoodleParentTrackEventData? noodleData = TryGetEventData<NoodleParentTrackEventData>(customEventData);
                if (noodleData != null)
                {
                    IEnumerable<Track> tracks = noodleData.ChildrenTracks;
                    Track parentTrack = noodleData.ParentTrack;
                    ParentObject.AssignTrack(tracks, parentTrack, noodleData.WorldPositionStays, noodleData.Position, noodleData.Rotation, noodleData.LocalRotation, noodleData.Scale);
                }
            }
        }
    }
}
