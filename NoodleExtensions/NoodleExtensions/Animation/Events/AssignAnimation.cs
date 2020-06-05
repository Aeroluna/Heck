using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using static NoodleExtensions.Animation.AnimationController;

namespace NoodleExtensions.Animation
{
    internal class AssignAnimation
    {
        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AssignAnimation")
            {
                Track track = GetTrack(customEventData);
                if (track != null)
                {
                    dynamic positionString = Trees.at(customEventData.data, "_position");
                    dynamic rotationString = Trees.at(customEventData.data, "_rotation");
                    dynamic scaleString = Trees.at(customEventData.data, "_scale");
                    dynamic localRotationString = Trees.at(customEventData.data, "_localRotation");

                    Dictionary<string, PointData> pointDefintions = Trees.at(((CustomBeatmapData)_customEventCallbackController._beatmapData).customData, "pointDefinitions");

                    PointData position;
                    PointData rotation;
                    PointData scale;
                    PointData localRotation;

                    if (positionString is string) pointDefintions.TryGetValue(positionString, out position);
                    else position = DynamicToPointData(positionString);
                    if (rotationString is string) pointDefintions.TryGetValue(rotationString, out rotation);
                    else rotation = DynamicToPointData(rotationString);
                    if (scaleString is string) pointDefintions.TryGetValue(scaleString, out scale);
                    else scale = DynamicToPointData(scaleString);
                    if (localRotationString is string) pointDefintions.TryGetValue(localRotationString, out localRotation);
                    else localRotation = DynamicToPointData(localRotationString);

                    if (positionString != null) track.definePosition = position;
                    if (rotationString != null) track.defineRotation = rotation;
                    if (scaleString != null) track.defineScale = scale;
                    if (localRotationString != null) track.defineLocalRotation = localRotation;
                }
            }
        }
    }
}
