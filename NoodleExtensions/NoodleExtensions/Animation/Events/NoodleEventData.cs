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

    internal static class NoodleEventDataManager
    {
        internal static Dictionary<CustomEventData, NoodleEventData> NoodleEventDatas { get; private set; }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            NoodleEventDatas = new Dictionary<CustomEventData, NoodleEventData>();
            foreach (CustomEventData customEventData in ((CustomBeatmapData)beatmapData).customEventsData)
            {
                NoodleEventData noodleEventData;

                switch (customEventData.type)
                {
                    case ANIMATETRACK:
                    case ASSIGNPATHANIMATION:
                        noodleEventData = ProcessCoroutineEvent(customEventData, beatmapData);
                        break;

                    case ASSIGNPLAYERTOTRACK:
                        noodleEventData = new NoodlePlayerTrackEventData()
                        {
                            Track = GetTrackPreload(customEventData.data, beatmapData),
                        };
                        break;

                    case ASSIGNTRACKPARENT:
                        noodleEventData = ProcessParentTrackEvent(customEventData.data, beatmapData);
                        break;

                    default:
                        continue;
                }

                NoodleEventDatas.Add(customEventData, noodleEventData);
            }
        }

        private static NoodleCoroutineEventData ProcessCoroutineEvent(CustomEventData customEventData, IReadonlyBeatmapData beatmapData)
        {
            NoodleCoroutineEventData noodleEventData = new NoodleCoroutineEventData();

            string easingString = (string)Trees.at(customEventData.data, EASING);
            noodleEventData.Easing = Functions.easeLinear;
            if (easingString != null)
            {
                noodleEventData.Easing = (Functions)Enum.Parse(typeof(Functions), easingString);
            }

            noodleEventData.Duration = (float?)Trees.at(customEventData.data, DURATION) ?? 0f;

            Track track = GetTrackPreload(customEventData.data, beatmapData);
            if (track == null)
            {
                return null;
            }

            EventType eventType;
            switch (customEventData.type)
            {
                case ANIMATETRACK:
                    eventType = EventType.AnimateTrack;
                    break;

                case ASSIGNPATHANIMATION:
                    eventType = EventType.AssignPathAnimation;
                    break;

                default:
                    return null;
            }

            List<string> excludedStrings = new List<string> { TRACK, DURATION, EASING };
            IDictionary<string, object> eventData = new Dictionary<string, object>(customEventData.data as IDictionary<string, object>); // Shallow copy
            IDictionary<string, Property> properties = null;
            switch (eventType)
            {
                case EventType.AnimateTrack:
                    properties = track.Properties;
                    break;

                case EventType.AssignPathAnimation:
                    properties = track.PathProperties;
                    break;
            }

            foreach (KeyValuePair<string, object> valuePair in eventData)
            {
                if (!excludedStrings.Any(n => n == valuePair.Key))
                {
                    if (!properties.TryGetValue(valuePair.Key, out Property property))
                    {
                        NoodleLogger.Log($"Could not find property {valuePair.Key}!", IPA.Logging.Logger.Level.Error);
                        continue;
                    }

                    Dictionary<string, PointDefinition> pointDefinitions = Trees.at(((CustomBeatmapData)beatmapData).customData, "pointDefinitions");
                    TryGetPointData(customEventData.data, valuePair.Key, out PointDefinition pointData, pointDefinitions);

                    NoodleCoroutineEventData.CoroutineInfo coroutineInfo = new NoodleCoroutineEventData.CoroutineInfo()
                    {
                        PointDefinition = pointData,
                        Property = property,
                    };

                    noodleEventData.CoroutineInfos.Add(coroutineInfo);
                }
            }

            return noodleEventData;
        }

        private static NoodleParentTrackEventData ProcessParentTrackEvent(dynamic customData, IReadonlyBeatmapData beatmapData)
        {
            IEnumerable<float> position = ((List<object>)Trees.at(customData, POSITION))?.Select(n => Convert.ToSingle(n));
            Vector3? posVector = null;
            if (position != null)
            {
                posVector = new Vector3(position.ElementAt(0), position.ElementAt(1), position.ElementAt(2));
            }

            IEnumerable<float> rotation = ((List<object>)Trees.at(customData, ROTATION))?.Select(n => Convert.ToSingle(n));
            Quaternion? rotQuaternion = null;
            if (rotation != null)
            {
                rotQuaternion = Quaternion.Euler(rotation.ElementAt(0), rotation.ElementAt(1), rotation.ElementAt(2));
            }

            IEnumerable<float> localrot = ((List<object>)Trees.at(customData, LOCALROTATION))?.Select(n => Convert.ToSingle(n));
            Quaternion? localRotQuaternion = null;
            if (localrot != null)
            {
                localRotQuaternion = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
            }

            IEnumerable<float> scale = ((List<object>)Trees.at(customData, SCALE))?.Select(n => Convert.ToSingle(n));
            Vector3? scaleVector = null;
            if (scale != null)
            {
                scaleVector = new Vector3(scale.ElementAt(0), scale.ElementAt(1), scale.ElementAt(2));
            }

            return new NoodleParentTrackEventData()
            {
                ParentTrack = GetTrackPreload(customData, beatmapData, "_parentTrack"),
                ChildrenTracks = GetTrackArrayPreload(customData, beatmapData, "_childrenTracks"),
                Position = posVector,
                Rotation = rotQuaternion,
                LocalRotation = localRotQuaternion,
                Scale = scaleVector,
            };
        }
    }

    internal class NoodleCoroutineEventData : NoodleEventData
    {
        internal float Duration { get; set; }

        internal Functions Easing { get; set; }

        internal List<CoroutineInfo> CoroutineInfos { get; set; } = new List<CoroutineInfo>();

        internal class CoroutineInfo
        {
            internal PointDefinition PointDefinition { get; set; }

            internal Property Property { get; set; }
        }
    }

    internal class NoodlePlayerTrackEventData : NoodleEventData
    {
        internal Track Track { get; set; }
    }

    internal class NoodleParentTrackEventData : NoodleEventData
    {
        internal Track ParentTrack { get; set; }

        internal IEnumerable<Track> ChildrenTracks { get; set; }

        internal Vector3? Position { get; set; }

        internal Quaternion? Rotation { get; set; }

        internal Quaternion? LocalRotation { get; set; }

        internal Vector3? Scale { get; set; }
    }

    internal class NoodleEventData
    {
    }
}
