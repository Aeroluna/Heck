using CustomJSONData.CustomBeatmap;
using Heck.Animation;
using static NoodleExtensions.NoodleController;
using static NoodleExtensions.NoodleCustomDataManager;

namespace NoodleExtensions.Animation.Events
{
    internal static class AssignPlayerToTrack
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type != ASSIGN_PLAYER_TO_TRACK)
            {
                return;
            }

            NoodlePlayerTrackEventData? noodleData = TryGetEventData<NoodlePlayerTrackEventData>(customEventData);
            Track? track = noodleData?.Track;
            if (track != null)
            {
                PlayerTrack.AssignTrack(track);
            }
        }
    }
}
