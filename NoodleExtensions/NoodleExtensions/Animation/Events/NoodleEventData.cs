namespace NoodleExtensions.Animation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck.Animation;
    using UnityEngine;
    using static Heck.Animation.AnimationHelper;
    using static NoodleExtensions.Plugin;

    internal static class NoodleEventDataManager
    {
        private static Dictionary<CustomEventData, NoodleEventData> _noodleEventDatas;

        internal static T TryGetEventData<T>(CustomEventData customEventData)
        {
            if (_noodleEventDatas.TryGetValue(customEventData, out NoodleEventData noodleEventData))
            {
                if (noodleEventData is T t)
                {
                    return t;
                }
                else
                {
                    throw new InvalidOperationException($"NoodleEventData was not of correct type. Expected: {typeof(T).Name}, was: {noodleEventData.GetType().Name}");
                }
            }

            return default;
        }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            _noodleEventDatas = new Dictionary<CustomEventData, NoodleEventData>();
            foreach (CustomEventData customEventData in ((CustomBeatmapData)beatmapData).customEventsData)
            {
                try
                {
                    NoodleEventData noodleEventData;

                    switch (customEventData.type)
                    {
                        case ASSIGNPLAYERTOTRACK:
                            noodleEventData = new NoodlePlayerTrackEventData()
                            {
                                Track = GetTrack(customEventData.data, beatmapData),
                            };
                            break;

                        case ASSIGNTRACKPARENT:
                            noodleEventData = ProcessParentTrackEvent(customEventData.data, beatmapData);
                            break;

                        default:
                            continue;
                    }

                    if (noodleEventData != null)
                    {
                        _noodleEventDatas.Add(customEventData, noodleEventData);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Logger.Log($"Could not create NoodleEventData for event {customEventData.type} at {customEventData.time}", IPA.Logging.Logger.Level.Error);
                    Plugin.Logger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }

        private static NoodleParentTrackEventData ProcessParentTrackEvent(Dictionary<string, object> customData, IReadonlyBeatmapData beatmapData)
        {
            IEnumerable<float> position = customData.Get<List<object>>(POSITION)?.Select(n => Convert.ToSingle(n));
            Vector3? posVector = null;
            if (position != null)
            {
                posVector = new Vector3(position.ElementAt(0), position.ElementAt(1), position.ElementAt(2));
            }

            IEnumerable<float> rotation = customData.Get<List<object>>(ROTATION)?.Select(n => Convert.ToSingle(n));
            Quaternion? rotQuaternion = null;
            if (rotation != null)
            {
                rotQuaternion = Quaternion.Euler(rotation.ElementAt(0), rotation.ElementAt(1), rotation.ElementAt(2));
            }

            IEnumerable<float> localrot = customData.Get<List<object>>(LOCALROTATION)?.Select(n => Convert.ToSingle(n));
            Quaternion? localRotQuaternion = null;
            if (localrot != null)
            {
                localRotQuaternion = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
            }

            IEnumerable<float> scale = customData.Get<List<object>>(SCALE)?.Select(n => Convert.ToSingle(n));
            Vector3? scaleVector = null;
            if (scale != null)
            {
                scaleVector = new Vector3(scale.ElementAt(0), scale.ElementAt(1), scale.ElementAt(2));
            }

            return new NoodleParentTrackEventData()
            {
                ParentTrack = GetTrack(customData, beatmapData, "_parentTrack"),
                ChildrenTracks = GetTrackArray(customData, beatmapData, "_childrenTracks"),
                Position = posVector,
                Rotation = rotQuaternion,
                LocalRotation = localRotQuaternion,
                Scale = scaleVector,
            };
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
