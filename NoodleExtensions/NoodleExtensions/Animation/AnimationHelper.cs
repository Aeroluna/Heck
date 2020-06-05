using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static NoodleExtensions.Plugin;

namespace NoodleExtensions.Animation
{
    internal static class AnimationHelper
    {
        private static Dictionary<string, Track> _tracks { get => ((CustomBeatmapData)AnimationController._customEventCallbackController._beatmapData).customData.tracks; }

        private static BeatmapObjectManager _beatmapObjectManager;

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

        internal static void GetPointData(CustomEventData customEventData, out PointData position, out PointData rotation, out PointData scale, out PointData localRotation)
        {
            dynamic positionString = Trees.at(customEventData.data, POSITION);
            dynamic rotationString = Trees.at(customEventData.data, ROTATION);
            dynamic scaleString = Trees.at(customEventData.data, SCALE);
            dynamic localRotationString = Trees.at(customEventData.data, LOCALROTATION);

            Dictionary<string, PointData> pointDefinitions = Trees.at(((CustomBeatmapData)AnimationController._customEventCallbackController._beatmapData).customData, "pointDefinitions");

            if (positionString is string) TryGetPointData(pointDefinitions, positionString, out position);
            else position = DynamicToPointData(positionString);
            if (rotationString is string) TryGetPointData(pointDefinitions, rotationString, out rotation);
            else rotation = DynamicToPointData(rotationString);
            if (scaleString is string) TryGetPointData(pointDefinitions, scaleString, out scale);
            else scale = DynamicToPointData(scaleString);
            if (localRotationString is string) TryGetPointData(pointDefinitions, localRotationString, out localRotation);
            else localRotation = DynamicToPointData(localRotationString);
        }

        private static void TryGetPointData(Dictionary<string, PointData> pointDefinitions, string pointString, out PointData pointData)
        {
            if (!pointDefinitions.TryGetValue(pointString, out pointData))
            {
                Logger.Log($"Could not find point definition {pointString}!", IPA.Logging.Logger.Level.Error);
                pointData = null;
            }
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

        internal static Track GetTrack(CustomEventData customEventData)
        {
            string trackName = Trees.at(customEventData.data, TRACK);
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
    }
}