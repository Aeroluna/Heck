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
        private static Dictionary<BeatmapObjectData, NoodleObjectData> _noodleObjectDatas = new Dictionary<BeatmapObjectData, NoodleObjectData>();

        internal static T? TryGetObjectData<T>(BeatmapObjectData beatmapObjectData)
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

                    Dictionary<string, object?> customData;

                    switch (beatmapObjectData)
                    {
                        case CustomObstacleData customObstacleData:
                            customData = customObstacleData.customData;
                            noodleObjectData = ProcessCustomObstacle(customData);
                            break;

                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            noodleObjectData = ProcessCustomNote(customData);
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

        private static void FinalizeCustomObject(Dictionary<string, object?> dynData, NoodleObjectData noodleObjectData, IReadonlyBeatmapData beatmapData)
        {
            object? rotation = dynData.Get<object>(ROTATION);
            if (rotation != null)
            {
                if (rotation is List<object> list)
                {
                    IEnumerable<float> rot = list.Select(n => Convert.ToSingle(n));
                    noodleObjectData.WorldRotationQuaternion = Quaternion.Euler(rot.ElementAt(0), rot.ElementAt(1), rot.ElementAt(2));
                }
                else
                {
                    noodleObjectData.WorldRotationQuaternion = Quaternion.Euler(0, Convert.ToSingle(rotation), 0);
                }
            }

            IEnumerable<float>? localrot = dynData.Get<List<object>>(LOCALROTATION)?.Select(n => Convert.ToSingle(n));
            if (localrot != null)
            {
                noodleObjectData.LocalRotationQuaternion = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
            }

            noodleObjectData.Track = AnimationHelper.GetTrackArray(dynData, beatmapData);

            Dictionary<string, object?>? animationObjectDyn = dynData.Get<Dictionary<string, object?>>("_animation");
            if (animationObjectDyn != null)
            {
                Dictionary<string, PointDefinition>? pointDefinitions = ((CustomBeatmapData)beatmapData).customData.Get<Dictionary<string, PointDefinition>>("pointDefinitions")
                    ?? throw new InvalidOperationException("Could not retrieve point definitions.");
                Animation.AnimationHelper.GetAllPointData(
                    animationObjectDyn,
                    pointDefinitions,
                    out PointDefinition? localPosition,
                    out PointDefinition? localRotation,
                    out PointDefinition? localScale,
                    out PointDefinition? localLocalRotation,
                    out PointDefinition? localDissolve,
                    out PointDefinition? localDissolveArrow,
                    out PointDefinition? localCuttable,
                    out PointDefinition? localDefinitePosition);
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
            }

            noodleObjectData.Cuttable = dynData.Get<bool?>(CUTTABLE);
            noodleObjectData.Fake = dynData.Get<bool?>(FAKENOTE);

            IEnumerable<float?>? position = dynData.GetNullableFloats(POSITION);
            noodleObjectData.StartX = position?.ElementAtOrDefault(0);
            noodleObjectData.StartY = position?.ElementAtOrDefault(1);

            noodleObjectData.NJS = dynData.Get<float?>(NOTEJUMPSPEED);
            noodleObjectData.SpawnOffset = dynData.Get<float?>(NOTESPAWNOFFSET);
            noodleObjectData.AheadTimeInternal = dynData.Get<float?>("aheadTime");
        }

        private static NoodleNoteData ProcessCustomNote(Dictionary<string, object?> dynData)
        {
            NoodleNoteData noodleNoteData = new NoodleNoteData();

            float? cutDir = dynData.Get<float?>(CUTDIRECTION);
            if (cutDir.HasValue)
            {
                noodleNoteData.CutQuaternion = Quaternion.Euler(0, 0, cutDir.Value);
            }

            noodleNoteData.FlipYSideInternal = dynData.Get<float?>("flipYSide");
            noodleNoteData.FlipLineIndexInternal = dynData.Get<float?>("flipLineIndex");

            noodleNoteData.StartNoteLineLayerInternal = dynData.Get<float?>("startNoteLineLayer");

            noodleNoteData.DisableGravity = dynData.Get<bool?>(NOTEGRAVITYDISABLE) ?? false;
            noodleNoteData.DisableLook = dynData.Get<bool?>(NOTELOOKDISABLE) ?? false;

            return noodleNoteData;
        }

        private static NoodleObstacleData ProcessCustomObstacle(Dictionary<string, object?> dynData)
        {
            NoodleObstacleData noodleObstacleData = new NoodleObstacleData();

            IEnumerable<float?>? scale = dynData.GetNullableFloats(SCALE);
            noodleObstacleData.Width = scale?.ElementAtOrDefault(0);
            noodleObstacleData.Height = scale?.ElementAtOrDefault(1);
            noodleObstacleData.Length = scale?.ElementAtOrDefault(2);

            return noodleObstacleData;
        }
    }

    internal record NoodleNoteData : NoodleObjectData
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

    internal record NoodleObstacleData : NoodleObjectData
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

    internal record NoodleObjectData
    {
        internal Quaternion? WorldRotationQuaternion { get; set; }

        internal Quaternion? LocalRotationQuaternion { get; set; }

        internal IEnumerable<Track>? Track { get; set; }

        internal Quaternion WorldRotation { get; set; }

        internal Quaternion LocalRotation { get; set; }

        internal AnimationObjectData? AnimationObject { get; set; }

        internal Vector3 NoteOffset { get; set; }

        internal bool? Cuttable { get; set; }

        internal bool? Fake { get; set; }

        internal float? StartX { get; set; }

        internal float? StartY { get; set; }

        internal float? NJS { get; set; }

        internal float? SpawnOffset { get; set; }

        internal float? AheadTimeInternal { get; set; }

        internal record AnimationObjectData
        {
            internal PointDefinition? LocalPosition { get; set; }

            internal PointDefinition? LocalRotation { get; set; }

            internal PointDefinition? LocalScale { get; set; }

            internal PointDefinition? LocalLocalRotation { get; set; }

            internal PointDefinition? LocalDissolve { get; set; }

            internal PointDefinition? LocalDissolveArrow { get; set; }

            internal PointDefinition? LocalCuttable { get; set; }

            internal PointDefinition? LocalDefinitePosition { get; set; }
        }
    }
}
