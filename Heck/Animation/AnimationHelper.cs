namespace Heck.Animation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using UnityEngine;
    using static Heck.Plugin;

    public static class AnimationHelper
    {
        public static bool LeftHandedMode { get; internal set; }

        public static float? TryGetLinearPathProperty(Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateLinear(time);
            }

            return null;
        }

        public static Quaternion? TryGetQuaternionPathProperty(Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateQuaternion(time);
            }

            return null;
        }

        public static Vector3? TryGetVector3PathProperty(Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.Interpolate(time);
            }

            return null;
        }

        public static Vector4? TryGetVector4PathProperty(Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            if (pointDataInterpolation != null)
            {
                return pointDataInterpolation.InterpolateVector4(time);
            }

            return null;
        }

        public static T? TryGetProperty<T>(Track? track, string propertyName)
        {
            Property? property = null;
            track?.Properties.TryGetValue(propertyName, out property);
            return (T?)property?.Value;
        }

        public static PointDefinition? TryGetPointData(Dictionary<string, object?> customData, string pointName, CustomBeatmapData customBeatmapData)
        {
            return TryGetPointData(customData, pointName, customBeatmapData.GetBeatmapPointDefinitions());
        }

        public static PointDefinition? TryGetPointData(Dictionary<string, object?> customData, string pointName, Dictionary<string, PointDefinition> pointDefinitions)
        {
            object? pointString = customData.Get<object>(pointName);
            switch (pointString)
            {
                case null:
                    return null;

                case string castedString:
                    if (pointDefinitions.TryGetValue(castedString, out PointDefinition pointData))
                    {
                        return pointData;
                    }

                    Plugin.Logger.Log($"Could not find point definition {castedString}!", IPA.Logging.Logger.Level.Error);
                    return null;

                case List<object> list:
                    return PointDefinition.ListToPointDefinition(list);

                default:
                    throw new InvalidOperationException($"Point was not a valid type. Got {pointString.GetType().FullName}");
            }
        }

        public static Track? GetTrack(Dictionary<string, object?> customData, CustomBeatmapData customBeatmapData, string name = TRACK)
        {
            return GetTrack(customData, customBeatmapData.GetBeatmapTracks(), name);
        }

        public static Track? GetTrack(Dictionary<string, object?> customData, Dictionary<string, Track> beatmapTracks, string name = TRACK)
        {
            string? trackName = customData.Get<string>(name);
            if (trackName == null)
            {
                return null;
            }

            if (beatmapTracks.TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                throw new InvalidOperationException($"Could not find track {trackName}.");
            }
        }

        public static IEnumerable<Track>? GetTrackArray(Dictionary<string, object?> customData, CustomBeatmapData customBeatmapData, string name = TRACK)
        {
            return GetTrackArray(customData, customBeatmapData.GetBeatmapTracks(), name);
        }

        public static IEnumerable<Track>? GetTrackArray(Dictionary<string, object?> customData, Dictionary<string, Track> beatmapTracks, string name = TRACK)
        {
            object? trackNameRaw = customData.Get<object>(name);
            if (trackNameRaw == null)
            {
                return null;
            }

            IEnumerable<string> trackNames;
            if (trackNameRaw is List<object> listTrack)
            {
                trackNames = listTrack.Cast<string>();
            }
            else
            {
                trackNames = new string[] { (string)trackNameRaw };
            }

            HashSet<Track> result = new HashSet<Track>();
            foreach (string trackName in trackNames)
            {
                if (beatmapTracks.TryGetValue(trackName, out Track track))
                {
                    result.Add(track);
                }
                else
                {
                    throw new InvalidOperationException($"Could not find track {trackName}.");
                }
            }

            return result;
        }

        public static Dictionary<string, PointDefinition> GetBeatmapPointDefinitions(this CustomBeatmapData customBeatmapData)
        {
            return (Dictionary<string, PointDefinition>)(customBeatmapData.customData["pointDefinitions"] ?? throw new InvalidOperationException("Could not find point definitions in BeatmapData."));
        }

        public static Dictionary<string, Track> GetBeatmapTracks(this CustomBeatmapData customBeatmapData)
        {
            return (Dictionary<string, Track>)(customBeatmapData.customData["tracks"] ?? throw new InvalidOperationException("Could not find tracks in BeatmapData."));
        }

        private static PointDefinitionInterpolation? GetPathInterpolation(Track? track, string propertyName)
        {
            Property? pathProperty = null;
            track?.PathProperties.TryGetValue(propertyName, out pathProperty);
            if (pathProperty != null)
            {
                return ((PathProperty)pathProperty).Interpolation;
            }

            return null;
        }
    }
}
