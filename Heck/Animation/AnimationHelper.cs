namespace Heck.Animation
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static Heck.Plugin;

    public static class AnimationHelper
    {
        public static bool LeftHandedMode { get; internal set; }

        public static float? TryGetLinearPathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateLinear(time);
            }

            return null;
        }

        public static Quaternion? TryGetQuaternionPathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateQuaternion(time);
            }

            return null;
        }

        public static Vector3? TryGetVector3PathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.Interpolate(time);
            }

            return null;
        }

        public static Vector4? TryGetVector4PathProperty(Track track, string propertyName, float time)
        {
            PointDefinitionInterpolation pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateVector4(time);
            }

            return null;
        }

        public static object TryGetProperty(Track track, string propertyName)
        {
            Property property = null;
            track?.Properties.TryGetValue(propertyName, out property);
            return property?.Value;
        }

        public static void TryGetPointData(Dictionary<string, object> customData, string pointName, out PointDefinition pointData, Dictionary<string, PointDefinition> pointDefinitions)
        {
            object pointString = customData.Get<object>(pointName);
            switch (pointString)
            {
                case null:
                    pointData = null;
                    break;

                case string castedString:
                    if (!pointDefinitions.TryGetValue(castedString, out pointData))
                    {
                        Plugin.Logger.Log($"Could not find point definition {castedString}!", IPA.Logging.Logger.Level.Error);
                        pointData = null;
                    }

                    break;

                case List<object> list:
                    pointData = PointDefinition.ListToPointData(list);

                    break;

                default:
                    throw new System.ArgumentException($"Point was not a valid type. Got {pointString.GetType().FullName}");
            }
        }

        public static Track GetTrack(Dictionary<string, object> customData, IReadonlyBeatmapData beatmapData, string name = TRACK)
        {
            string trackName = customData.Get<string>(name);
            if (trackName == null)
            {
                return null;
            }

            if (((Dictionary<string, Track>)((CustomBeatmapData)beatmapData).customData["tracks"]).TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                Plugin.Logger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                return null;
            }
        }

        public static IEnumerable<Track> GetTrackArray(Dictionary<string, object> customData, IReadonlyBeatmapData beatmapData, string name = TRACK)
        {
            IEnumerable<string> trackNames = customData.Get<List<object>>(name).Cast<string>();
            if (trackNames == null)
            {
                return null;
            }

            HashSet<Track> tracks = new HashSet<Track>();
            foreach (string trackName in trackNames)
            {
                if (((Dictionary<string, Track>)((CustomBeatmapData)beatmapData).customData["tracks"]).TryGetValue(trackName, out Track track))
                {
                    tracks.Add(track);
                }
                else
                {
                    Plugin.Logger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                }
            }

            return tracks;
        }

        private static PointDefinitionInterpolation GetPathInterpolation(Track track, string propertyName)
        {
            Property pathProperty = null;
            track?.PathProperties.TryGetValue(propertyName, out pathProperty);
            if (pathProperty != null)
            {
                PointDefinitionInterpolation pointDataInterpolation = (PointDefinitionInterpolation)pathProperty.Value;

                return pointDataInterpolation;
            }

            return null;
        }
    }
}
