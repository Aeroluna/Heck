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

        public static void TryGetPointData(Dictionary<string, object?> customData, string pointName, out PointDefinition? pointData, Dictionary<string, PointDefinition> pointDefinitions)
        {
            object? pointString = customData.Get<object>(pointName);
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
                    pointData = PointDefinition.ListToPointDefinition(list);

                    break;

                default:
                    throw new InvalidOperationException($"Point was not a valid type. Got {pointString.GetType().FullName}");
            }
        }

        public static Track? GetTrack(Dictionary<string, object?> customData, IReadonlyBeatmapData beatmapData, string name = TRACK)
        {
            string? trackName = customData.Get<string>(name);
            if (trackName == null)
            {
                return null;
            }

            if (((Dictionary<string, Track>)(((CustomBeatmapData)beatmapData).customData["tracks"] ?? throw new InvalidOperationException("Could not find tracks in BeatmapData."))).TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                throw new InvalidOperationException($"Could not find track {trackName}.");
            }
        }

        public static IEnumerable<Track>? GetTrackArray(Dictionary<string, object?> customData, IReadonlyBeatmapData beatmapData, string name = TRACK)
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

            HashSet<Track> tracks = new HashSet<Track>();
            Dictionary<string, Track> beatmapTracks = (Dictionary<string, Track>)(((CustomBeatmapData)beatmapData).customData["tracks"] ?? throw new InvalidOperationException("Could not find tracks in BeatmapData."));
            foreach (string trackName in trackNames)
            {
                if (beatmapTracks.TryGetValue(trackName, out Track track))
                {
                    tracks.Add(track);
                }
                else
                {
                    throw new InvalidOperationException($"Could not find track {trackName}.");
                }
            }

            return tracks;
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
