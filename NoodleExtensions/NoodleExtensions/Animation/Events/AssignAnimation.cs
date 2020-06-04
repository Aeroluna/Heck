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
                    string positionString = (string)Trees.at(customEventData.data, "_position");
                    string rotationString = (string)Trees.at(customEventData.data, "_rotation");
                    string scaleString = (string)Trees.at(customEventData.data, "_scale");
                    string localRotationString = (string)Trees.at(customEventData.data, "_localRotation");

                    Dictionary<string, PointData> pointDefintions = ((CustomBeatmapData)_customEventCallbackController._beatmapData).customData.pointDefinitions;

                    if (positionString != null && pointDefintions.TryGetValue(positionString, out PointData position)) track.definePosition = position;
                    if (rotationString != null && pointDefintions.TryGetValue(rotationString, out PointData rotation)) track.defineRotation = rotation;
                    if (scaleString != null && pointDefintions.TryGetValue(scaleString, out PointData scale)) track.defineScale = scale;
                    if (localRotationString != null && pointDefintions.TryGetValue(localRotationString, out PointData localRotation)) track.defineLocalRotation = localRotation;
                }
            }
        }
    }
}
