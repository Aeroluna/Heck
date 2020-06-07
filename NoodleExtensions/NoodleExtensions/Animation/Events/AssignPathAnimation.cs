using CustomJSONData.CustomBeatmap;
using static NoodleExtensions.Animation.AnimationHelper;

namespace NoodleExtensions.Animation
{
    internal class AssignPathAnimation
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AssignPathAnimation")
            {
                Track track = GetTrack(customEventData.data);
                if (track != null)
                {
                    GetPointData(customEventData.data, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation);

                    if (position != null) track.pathPosition = position;
                    if (rotation != null) track.pathRotation = rotation;
                    if (scale != null) track.pathScale = scale;
                    if (localRotation != null) track.pathLocalRotation = localRotation;
                }
            }
        }
    }
}