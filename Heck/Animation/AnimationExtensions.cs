using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using JetBrains.Annotations;
using UnityEngine;
using static Heck.HeckController;

namespace Heck.Animation
{
    public static class AnimationExtensions
    {
        [Pure]
        public static T? GetProperty<T>(this Track? track, string propertyName)
            where T : struct
        {
            return track?.FindProperty<T>(propertyName)?.Value;
        }

        [Pure]
        public static float? GetLinearPathProperty(this Track? track, string propertyName, float time)
        {
            return GetPathInterpolation<float>(track, propertyName)?.Interpolate(time);
        }

        [Pure]
        public static Vector3? GetVector3PathProperty(this Track? track, string propertyName, float time)
        {
            return GetPathInterpolation<Vector3>(track, propertyName)?.Interpolate(time);
        }

        [Pure]
        public static Vector4? GetVector4PathProperty(this Track? track, string propertyName, float time)
        {
            return GetPathInterpolation<Vector4>(track, propertyName)?.Interpolate(time);
        }

        [Pure]
        public static Quaternion? GetQuaternionPathProperty(this Track? track, string propertyName, float time)
        {
            return GetPathInterpolation<Quaternion>(track, propertyName)?.Interpolate(time);
        }

        [Pure]
        public static PointDefinition<T>? GetPointData<T>(this CustomData customData, string pointName, Dictionary<string, List<object>> pointDefinitions)
            where T : struct
        {
            object? pointString = customData.Get<object>(pointName);
            switch (pointString)
            {
                case null:
                    return null;

                case string castedString:
                    if (pointDefinitions.TryGetValue(castedString, out List<object> pointData))
                    {
                        return pointData.ToPointDefinition<T>();
                    }

                    Plugin.Log.Error($"Could not find point definition [{castedString}]");
                    return null;

                case List<object> list:
                    return list.ToPointDefinition<T>();

                default:
                    throw new InvalidOperationException($"Point was not a valid type. Got [{pointString.GetType().FullName}].");
            }
        }

        [Pure]
        public static Track GetTrack(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetTrack(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        [Pure]
        public static Track GetTrack(this CustomData customData, Dictionary<string, Track> beatmapTracks, string name)
        {
            return GetNullableTrack(customData, beatmapTracks, name) ?? throw new JsonNotDefinedException(name);
        }

        [Pure]
        [PublicAPI]
        public static Track? GetNullableTrack(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetNullableTrack(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        [Pure]
        public static IEnumerable<Track> GetTrackArray(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetTrackArray(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        [Pure]
        public static IEnumerable<Track> GetTrackArray(this CustomData customData, Dictionary<string, Track> beatmapTracks, string name)
        {
            return GetNullableTrackArray(customData, beatmapTracks, name) ?? throw new JsonNotDefinedException(name);
        }

        [Pure]
        public static IEnumerable<Track>? GetNullableTrackArray(this CustomData customData, Dictionary<string, Track> beatmapTracks, bool v2)
        {
            return GetNullableTrackArray(customData, beatmapTracks, v2 ? V2_TRACK : TRACK);
        }

        [Pure]
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

        [Pure]
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

        private static PointDefinitionInterpolation<T>? GetPathInterpolation<T>(Track? track, string propertyName)
            where T : struct
        {
            return track?.FindPathProperty<T>(propertyName)?.Interpolation;
        }
    }
}
