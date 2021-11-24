using System.Collections.Generic;
using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using static NoodleExtensions.NoodleController;
using static NoodleExtensions.NoodleCustomDataManager;

namespace NoodleExtensions.Animation.Events
{
    internal static class AssignTrackParent
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type != ASSIGN_TRACK_PARENT)
            {
                return;
            }

            NoodleParentTrackEventData? noodleData = TryGetEventData<NoodleParentTrackEventData>(customEventData);
            if (noodleData == null)
            {
                return;
            }

            List<Track> tracks = noodleData.ChildrenTracks;
            Track parentTrack = noodleData.ParentTrack;
            ParentObject.AssignTrack(tracks, parentTrack, noodleData.WorldPositionStays, noodleData.Position, noodleData.Rotation, noodleData.LocalRotation, noodleData.Scale);
        }
    }
}
