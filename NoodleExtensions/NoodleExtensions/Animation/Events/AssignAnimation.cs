using CustomJSONData.CustomBeatmap;
using static NoodleExtensions.Animation.AnimationHelper;

namespace NoodleExtensions.Animation
{
    internal class AssignAnimation
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AssignAnimation")
            {
                Track track = GetTrack(customEventData.data);
                if (track != null)
                {
                    GetPointData(customEventData.data, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation);

                    if (position != null) track.definePosition = position;
                    if (rotation != null) track.defineRotation = rotation;
                    if (scale != null) track.defineScale = scale;
                    if (localRotation != null) track.defineLocalRotation = localRotation;
                }
            }
        }
    }
}