using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using UnityEngine;
using static Heck.HeckController;
using Logger = IPA.Logging.Logger;

namespace Heck.Animation
{
    public static class AnimationExtensions
    {
        public static float? GetLinearPathProperty(this Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            return pointDataInterpolation?.InterpolateLinear(time);
        }

        public static Quaternion? GetQuaternionPathProperty(this Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            return pointDataInterpolation?.InterpolateQuaternion(time);
        }

        public static Vector3? GetVector3PathProperty(this Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            return pointDataInterpolation?.Interpolate(time);
        }

        public static Vector4? GetVector4PathProperty(this Track? track, string propertyName, float time)
        {
            PointDefinitionInterpolation? pointDataInterpolation = GetPathInterpolation(track, propertyName);

            return pointDataInterpolation?.InterpolateVector4(time);
        }

        public static T? GetProperty<T>(this Track? track, string propertyName)
        {
            Property? property = null;
            track?.Properties.TryGetValue(propertyName, out property);
            return (T?)property?.Value;
        }

        public static PointDefinition? GetPointData(this CustomData customData, string pointName, Dictionary<string, PointDefinition> pointDefinitions)
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

                    Log.Logger.Log($"Could not find point definition [{castedString}].", Logger.Level.Error);
                    return null;

                case List<object> list:
                    return PointDefinition.ListToPointDefinition(list);

                default:
                    throw new InvalidOperationException($"Point was not a valid type. Got [{pointString.GetType().FullName}].");
            }
        }

        public static Track GetTrack(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetTrack(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        public static Track GetTrack(this CustomData customData, Dictionary<string, Track> beatmapTracks, string name)
        {
            return GetNullableTrack(customData, beatmapTracks, name) ?? throw new InvalidOperationException($"{name} was not defined.");
        }

        public static Track? GetNullableTrack(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetNullableTrack(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        public static IEnumerable<Track> GetTrackArray(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetTrackArray(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        public static IEnumerable<Track> GetTrackArray(this CustomData customData, Dictionary<string, Track> beatmapTracks, string name)
        {
            return GetNullableTrackArray(customData, beatmapTracks, name) ?? throw new InvalidOperationException($"{name} was not defined.");
        }

        public static IEnumerable<Track>? GetNullableTrackArray(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetNullableTrackArray(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        public static Track? GetNullableTrack(this CustomData customData, Dictionary<string, Track> beatmapTracks, string name)
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

            throw new InvalidOperationException($"Could not find track [{trackName}].");
        }

        public static IEnumerable<Track>? GetNullableTrackArray(this CustomData customData, Dictionary<string, Track> beatmapTracks, string name)
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
                trackNames = new[] { (string)trackNameRaw };
            }

            HashSet<Track> result = new();
            foreach (string trackName in trackNames)
            {
                if (beatmapTracks.TryGetValue(trackName, out Track track))
                {
                    result.Add(track);
                }
                else
                {
                    throw new InvalidOperationException($"Could not find track [{trackName}].");
                }
            }

            return result;
        }

        private static PointDefinitionInterpolation? GetPathInterpolation(Track? track, string propertyName)
        {
            Property? pathProperty = null;
            track?.PathProperties.TryGetValue(propertyName, out pathProperty);
            return ((PathProperty?)pathProperty)?.Interpolation;
        }
    }
}
