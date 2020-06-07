using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.Plugin;
using static NoodleExtensions.HarmonyPatches.SpawnDataHelper.BeatmapObjectSpawnMovementDataVariables;

namespace NoodleExtensions.Animation
{
    internal static class AnimationHelper
    {
        private static Dictionary<string, Track> _tracks { get => ((CustomBeatmapData)AnimationController._customEventCallbackController._beatmapData).customData.tracks; }
        private static Dictionary<string, PointData> _pointDefinitions { get => Trees.at(((CustomBeatmapData)AnimationController._customEventCallbackController._beatmapData).customData, "pointDefinitions"); }

        private static BeatmapObjectManager _beatmapObjectManager;

        private static readonly Vector3 _vectorZero = Vector3.zero;

        private static BeatmapObjectManager beatmapObjectManager
        {
            get
            {
                if (_beatmapObjectManager == null) _beatmapObjectManager = Resources.FindObjectsOfTypeAll<BeatmapObjectManager>().First();
                return _beatmapObjectManager;
            }
        }

        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteAPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteAPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _noteBPoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_noteBPool");
        private static readonly FieldAccessor<BeatmapObjectManager, NoteController.Pool>.Accessor _bombNotePoolAccessor = FieldAccessor<BeatmapObjectManager, NoteController.Pool>.GetAccessor("_bombNotePool");
        private static readonly FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.Accessor _obstaclePoolAccessor = FieldAccessor<BeatmapObjectManager, ObstacleController.Pool>.GetAccessor("_obstaclePool");

        internal static void GetDefinitePosition(dynamic customData, Track track, out PointData position)
        {
            dynamic positionString = Trees.at(customData, DEFINITEPOSITION);

            TryGetPointData(_pointDefinitions, positionString, out position);
            position = position ?? track?.definitePosition;
        }

        internal static void GetObjectOffset(dynamic customData, Track track, float time, out Vector3? positionOffset, out Vector3? rotationOffset, out Vector3? scaleOffset, out Vector3? localRotationOffset)
        {
            // TODO: Recode this to cache the PointData if one is created
            AnimationHelper.GetPointData(customData, out PointData localPosition, out PointData localRotation, out PointData localScale, out PointData localLocalRotation);

            Vector3? pathPosition = localPosition?.Interpolate(time) ?? track.pathPosition?.Interpolate(time);
            Vector3? pathRotation = localRotation?.InterpolateAsQuaternion(time) ?? track.pathRotation?.InterpolateAsQuaternion(time);
            Vector3? pathScale = localScale?.Interpolate(time) ?? track.pathScale?.Interpolate(time);
            Vector3? pathLocalRotation = localLocalRotation?.InterpolateAsQuaternion(time) ?? track.pathLocalRotation?.InterpolateAsQuaternion(time);

            positionOffset = SumVectorNullables(pathPosition, track.position) * _noteLinesDistance;
            rotationOffset = SumVectorNullables(pathRotation, track.rotation);
            if (pathScale.HasValue) scaleOffset = Vector3.Scale(track.scale, pathScale.Value);
            else scaleOffset = track.scale;
            localRotationOffset = SumVectorNullables(pathLocalRotation, track.localRotation);
        }

        internal static void GetPointData(dynamic customData, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation)
        {
            dynamic positionString = Trees.at(customData, POSITION);
            dynamic rotationString = Trees.at(customData, ROTATION);
            dynamic scaleString = Trees.at(customData, SCALE);
            dynamic localRotationString = Trees.at(customData, LOCALROTATION);

            Dictionary<string, PointData> pointDefinitions = _pointDefinitions;

            TryGetPointData(pointDefinitions, positionString, out position);
            TryGetPointData(pointDefinitions, rotationString, out rotation);
            TryGetPointData(pointDefinitions, scaleString, out scale);
            TryGetPointData(pointDefinitions, localRotationString, out localRotation);
        }

        private static void TryGetPointData(Dictionary<string, PointData> pointDefinitions, dynamic pointString, out PointData pointData)
        {
            if (pointString is string)
            {
                if (!pointDefinitions.TryGetValue(pointString, out pointData))
                {
                    Logger.Log($"Could not find point definition {pointString}!", IPA.Logging.Logger.Level.Error);
                    pointData = null;
                }
            }
            else pointData = DynamicToPointData(pointString);
        }

        internal static PointData DynamicToPointData(dynamic dyn)
        {
            IEnumerable<IEnumerable<float>> points = ((IEnumerable<object>)dyn)
                        ?.Cast<IEnumerable<object>>()
                        .Select(n => n.Select(Convert.ToSingle));
            if (points == null) return null;

            PointData pointData = new PointData();
            foreach (IEnumerable<float> rawPoint in points)
            {
                pointData.Add(new Vector4(rawPoint.ElementAt(0), rawPoint.ElementAt(1), rawPoint.ElementAt(2), rawPoint.ElementAt(3)));
            }
            return pointData;
        }

        internal static Track GetTrack(dynamic customData)
        {
            string trackName = Trees.at(customData, TRACK);
            if (trackName == null) return null;
            if (_tracks.TryGetValue(trackName, out Track track))
            {
                return track;
            }
            else
            {
                Logger.Log($"Could not find track {trackName}!", IPA.Logging.Logger.Level.Error);
                return null;
            }
        }

        private static bool CompareTrack(BeatmapObjectData beatmapObjectData, Track track)
        {
            return Trees.at(((dynamic)beatmapObjectData).customData, "track") == track;
        }

        internal static IEnumerable<NoteController> GetActiveNotes(Track track = null)
        {
            BeatmapObjectManager objectManager = beatmapObjectManager;
            IEnumerable<NoteController> activeNotes = _noteAPoolAccessor(ref objectManager).activeItems
                .Union(_noteBPoolAccessor(ref objectManager).activeItems)
                .Union(_bombNotePoolAccessor(ref objectManager).activeItems);
            if (track != null) return activeNotes.Where(n => CompareTrack(n.noteData, track));
            return activeNotes;
        }

        internal static IEnumerable<ObstacleController> GetActiveObstacles(Track track = null)
        {
            BeatmapObjectManager objectManager = beatmapObjectManager;
            IEnumerable<ObstacleController> activeObstacles = _obstaclePoolAccessor(ref objectManager).activeItems;
            if (track != null) return activeObstacles.Where(n => CompareTrack(n.obstacleData, track));
            return activeObstacles;
        }

        internal static Vector3? SumVectorNullables(Vector3? vectorOne, Vector3? vectorTwo)
        {
            if (!vectorOne.HasValue && !vectorTwo.HasValue) return null;
            Vector3 total = _vectorZero;
            if (vectorOne.HasValue) total += vectorOne.Value;
            if (vectorTwo.HasValue) total += vectorTwo.Value;
            return total;
        }
    }
}