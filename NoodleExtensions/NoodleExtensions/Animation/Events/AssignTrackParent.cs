namespace NoodleExtensions.Animation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static NoodleExtensions.Animation.AnimationHelper;
    using static NoodleExtensions.Plugin;

    internal class AssignTrackParent
    {
        internal static void OnTrackManagerCreated(object trackManager, CustomBeatmapData customBeatmapData)
        {
            CustomEventData[] customEventDatas = customBeatmapData.customEventData;
            foreach (CustomEventData customEventData in customEventDatas)
            {
                if (customEventData.type == "AssignTrackParent")
                {
                    string trackName = Trees.at(customEventData.data, "_parentTrack");
                    ((TrackManager)trackManager).AddTrack(trackName);
                }
            }
        }

        internal static void Callback(CustomEventData customEventData)
        {
            if (customEventData.type == "AssignTrackParent")
            {
                Track parentTrack = GetTrack(customEventData.data, "_parentTrack");
                IEnumerable<Track> tracks = GetTrackArray(customEventData.data, "_childrenTracks");

                IEnumerable<float> position = ((List<object>)Trees.at(customEventData.data, POSITION))?.Select(n => Convert.ToSingle(n));
                Vector3? posVector = null;
                if (position != null)
                {
                    posVector = new Vector3(position.ElementAt(0), position.ElementAt(1), position.ElementAt(2));
                }

                IEnumerable<float> rotation = ((List<object>)Trees.at(customEventData.data, ROTATION))?.Select(n => Convert.ToSingle(n));
                Quaternion? rotQuaternion = null;
                if (rotation != null)
                {
                    rotQuaternion = Quaternion.Euler(rotation.ElementAt(0), rotation.ElementAt(1), rotation.ElementAt(2));
                }

                IEnumerable<float> localrot = ((List<object>)Trees.at(customEventData.data, LOCALROTATION))?.Select(n => Convert.ToSingle(n));
                Quaternion? localRotQuaternion = null;
                if (localrot != null)
                {
                    localRotQuaternion = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
                }

                IEnumerable<float> scale = ((List<object>)Trees.at(customEventData.data, SCALE))?.Select(n => Convert.ToSingle(n));
                Vector3? scaleVector = null;
                if (scale != null)
                {
                    scaleVector = new Vector3(scale.ElementAt(0), scale.ElementAt(1), scale.ElementAt(2));
                }

                if (tracks != null && parentTrack != null)
                {
                    ParentObject.AssignTrack(tracks, parentTrack, posVector, rotQuaternion, localRotQuaternion, scaleVector);
                }
            }
        }
    }
}
