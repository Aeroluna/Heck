namespace NoodleExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using Heck;
    using Heck.Animation;
    using UnityEngine;
    using static NoodleExtensions.Plugin;

    internal static class NoodleObjectDataManager
    {
        private static Dictionary<BeatmapObjectData, NoodleObjectData> _noodleObjectDatas;

        internal static T TryGetObjectData<T>(BeatmapObjectData beatmapObjectData)
        {
            if (_noodleObjectDatas.TryGetValue(beatmapObjectData, out NoodleObjectData noodleObjectData))
            {
                if (noodleObjectData is T t)
                {
                    return t;
                }
                else
                {
                    throw new InvalidOperationException($"NoodleObjectData was not of correct type. Expected: {typeof(T).Name}, was: {noodleObjectData.GetType().Name}");
                }
            }

            return default;
        }

        internal static void DeserializeBeatmapData(IReadonlyBeatmapData beatmapData)
        {
            _noodleObjectDatas = new Dictionary<BeatmapObjectData, NoodleObjectData>();
            foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectsData)
            {
                try
                {
                    NoodleObjectData noodleObjectData;

                    dynamic customData;

                    switch (beatmapObjectData)
                    {
                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            noodleObjectData = ProcessCustomNote(customData);
                            break;

                        case CustomObstacleData customObstacleData:
                            customData = customObstacleData.customData;
                            noodleObjectData = ProcessCustomObstacle(customData);
                            break;

                        case CustomWaypointData customWaypointData:
                            customData = customWaypointData.customData;
                            noodleObjectData = new NoodleObjectData();
                            break;

                        default:
                            continue;
                    }

                    if (noodleObjectData != null)
                    {
                        FinalizeCustomObject(customData, noodleObjectData, beatmapData);
                        _noodleObjectDatas.Add(beatmapObjectData, noodleObjectData);
                    }
                }
                catch (Exception e)
                {
                    Plugin.Logger.Log($"Could not create NoodleObjectData for object {beatmapObjectData.GetType().Name} at {beatmapObjectData.time}", IPA.Logging.Logger.Level.Error);
                    Plugin.Logger.Log(e, IPA.Logging.Logger.Level.Error);
                }
            }
        }

        private static void FinalizeCustomObject(dynamic dynData, NoodleObjectData noodleObjectData, IReadonlyBeatmapData beatmapData)
        {
            dynamic rotation = Trees.at(dynData, ROTATION);
            if (rotation != null)
            {
                if (rotation is List<object> list)
                {
                    IEnumerable<float> rot = list?.Select(n => Convert.ToSingle(n));
                    noodleObjectData.WorldRotationQuaternion = Quaternion.Euler(rot.ElementAt(0), rot.ElementAt(1), rot.ElementAt(2));
                }
                else
                {
                    noodleObjectData.WorldRotationQuaternion = Quaternion.Euler(0, (float)rotation, 0);
                }
            }

            IEnumerable<float> localrot = ((List<object>)Trees.at(dynData, LOCALROTATION))?.Select(n => Convert.ToSingle(n));
            if (localrot != null)
            {
                noodleObjectData.LocalRotationQuaternion = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
            }

            noodleObjectData.Track = AnimationHelper.GetTrackPreload(dynData, beatmapData);

            dynamic animationObjectDyn = Trees.at(dynData, "_animation");
            Dictionary<string, PointDefinition> pointDefinitions = Trees.at(((CustomBeatmapData)beatmapData).customData, "pointDefinitions");
            Animation.AnimationHelper.GetAllPointData(
                animationObjectDyn,
                pointDefinitions,
                out PointDefinition localPosition,
                out PointDefinition localRotation,
                out PointDefinition localScale,
                out PointDefinition localLocalRotation,
                out PointDefinition localDissolve,
                out PointDefinition localDissolveArrow,
                out PointDefinition localCuttable,
                out PointDefinition localDefinitePosition);
            NoodleObjectData.AnimationObjectData animationObjectData = new NoodleObjectData.AnimationObjectData
            {
                LocalPosition = localPosition,
                LocalRotation = localRotation,
                LocalScale = localScale,
                LocalLocalRotation = localLocalRotation,
                LocalDissolve = localDissolve,
                LocalDissolveArrow = localDissolveArrow,
                LocalCuttable = localCuttable,
                LocalDefinitePosition = localDefinitePosition,
            };
            noodleObjectData.AnimationObject = animationObjectData;

            noodleObjectData.Cuttable = Trees.at(dynData, CUTTABLE);
            noodleObjectData.Fake = Trees.at(dynData, FAKENOTE);

            IEnumerable<float?> position = ((List<object>)Trees.at(dynData, POSITION))?.Select(n => n.ToNullableFloat());
            noodleObjectData.StartX = position?.ElementAtOrDefault(0);
            noodleObjectData.StartY = position?.ElementAtOrDefault(1);

            noodleObjectData.NJS = (float?)Trees.at(dynData, NOTEJUMPSPEED);
            noodleObjectData.SpawnOffset = (float?)Trees.at(dynData, NOTESPAWNOFFSET);
            noodleObjectData.AheadTimeInternal = (float?)Trees.at(dynData, "aheadTime");
        }

        private static NoodleNoteData ProcessCustomNote(dynamic dynData)
        {
            NoodleNoteData noodleNoteData = new NoodleNoteData();

            float? cutDir = (float?)Trees.at(dynData, CUTDIRECTION);
            if (cutDir.HasValue)
            {
                noodleNoteData.CutQuaternion = Quaternion.Euler(0, 0, cutDir.Value);
            }

            noodleNoteData.FlipYSideInternal = (float?)Trees.at(dynData, "flipYSide");
            noodleNoteData.FlipLineIndexInternal = (float?)Trees.at(dynData, "flipLineIndex");

            noodleNoteData.StartNoteLineLayerInternal = (float?)Trees.at(dynData, "startNoteLineLayer");

            noodleNoteData.DisableGravity = (bool?)Trees.at(dynData, NOTEGRAVITYDISABLE) ?? false;
            noodleNoteData.DisableLook = (bool?)Trees.at(dynData, NOTELOOKDISABLE) ?? false;

            return noodleNoteData;
        }

        private static NoodleObstacleData ProcessCustomObstacle(dynamic dynData)
        {
            NoodleObstacleData noodleObstacleData = new NoodleObstacleData();

            IEnumerable<float?> scale = ((List<object>)Trees.at(dynData, SCALE))?.Select(n => n.ToNullableFloat());
            noodleObstacleData.Width = scale?.ElementAtOrDefault(0);
            noodleObstacleData.Height = scale?.ElementAtOrDefault(1);
            noodleObstacleData.Length = scale?.ElementAtOrDefault(2);

            return noodleObstacleData;
        }
    }

    internal class NoodleNoteData : NoodleObjectData
    {
        internal Quaternion? CutQuaternion { get; set; }

        internal Vector3 MoveStartPos { get; set; }

        internal Vector3 MoveEndPos { get; set; }

        internal Vector3 JumpEndPos { get; set; }

        internal float? FlipYSideInternal { get; set; }

        internal float? FlipLineIndexInternal { get; set; }

        internal float? StartNoteLineLayerInternal { get; set; }

        internal bool DisableGravity { get; set; }

        internal bool DisableLook { get; set; }

        internal float EndRotation { get; set; }
    }

    internal class NoodleObstacleData : NoodleObjectData
    {
        internal Vector3 StartPos { get; set; }

        internal Vector3 MidPos { get; set; }

        internal Vector3 EndPos { get; set; }

        internal Vector3 BoundsSize { get; set; }

        internal float? Width { get; set; }

        internal float? Height { get; set; }

        internal float? Length { get; set; }

        internal float XOffset { get; set; }

        internal bool DoUnhide { get; set; }
    }

    internal class NoodleObjectData
    {
        internal Quaternion? WorldRotationQuaternion { get; set; }

        internal Quaternion? LocalRotationQuaternion { get; set; }

        internal Track Track { get; set; }

        internal Quaternion WorldRotation { get; set; }

        internal Quaternion LocalRotation { get; set; }

        internal AnimationObjectData AnimationObject { get; set; }

        internal Vector3 NoteOffset { get; set; }

        internal bool? Cuttable { get; set; }

        internal bool? Fake { get; set; }

        internal float? StartX { get; set; }

        internal float? StartY { get; set; }

        internal float? NJS { get; set; }

        internal float? SpawnOffset { get; set; }

        internal float? AheadTimeInternal { get; set; }

        internal class AnimationObjectData
        {
            internal PointDefinition LocalPosition { get; set; }

            internal PointDefinition LocalRotation { get; set; }

            internal PointDefinition LocalScale { get; set; }

            internal PointDefinition LocalLocalRotation { get; set; }

            internal PointDefinition LocalDissolve { get; set; }

            internal PointDefinition LocalDissolveArrow { get; set; }

            internal PointDefinition LocalCuttable { get; set; }

            internal PointDefinition LocalDefinitePosition { get; set; }
        }
    }
}
