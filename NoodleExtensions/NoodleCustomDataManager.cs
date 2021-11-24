using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using UnityEngine;
using static Heck.Animation.AnimationHelper;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal static class NoodleCustomDataManager
    {
        private static Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData> _noodleObjectDatas = new();
        private static Dictionary<CustomEventData, ICustomEventCustomData> _noodleEventDatas = new();

        internal static T? TryGetObjectData<T>(BeatmapObjectData beatmapObjectData)
            where T : NoodleObjectData
        {
            return _noodleObjectDatas.TryGetCustomData<T>(beatmapObjectData);
        }

        internal static T? TryGetEventData<T>(CustomEventData customEventData)
            where T : ICustomEventCustomData
        {
            return _noodleEventDatas.TryGetCustomData<T>(customEventData);
        }

        internal static void OnBuildTracks(CustomDataDeserializer.DeserializeBeatmapEventArgs eventArgs)
        {
            TrackBuilder trackBuilder = eventArgs.TrackBuilder;
            foreach (CustomEventData customEventData in eventArgs.CustomEventDatas)
            {
                try
                {
                    switch (customEventData.type)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            trackBuilder.AddTrack(customEventData.data.Get<string>(TRACK) ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            trackBuilder.AddTrack(customEventData.data.Get<string>(PARENT_TRACK) ?? throw new InvalidOperationException("Parent track was not defined."));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Log.Logger, e, customEventData);
                }
            }
        }

        internal static void OnDeserializeBeatmapData(CustomDataDeserializer.DeserializeBeatmapEventArgs eventArgs)
        {
            if (eventArgs.IsMultiplayer)
            {
                return;
            }

            CustomBeatmapData beatmapData = eventArgs.BeatmapData;
            _noodleObjectDatas = new Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData>();
            Dictionary<string, PointDefinition> pointDefinitions = beatmapData.GetBeatmapPointDefinitions();
            Dictionary<string, Track> beatmapTracks = beatmapData.GetBeatmapTracks();
            foreach (BeatmapObjectData beatmapObjectData in eventArgs.BeatmapObjectDatas)
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

                    FinalizeCustomObject(customData, noodleObjectData, pointDefinitions, beatmapTracks);
                    _noodleObjectDatas.Add(beatmapObjectData, noodleObjectData);
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Log.Logger, e, beatmapObjectData);
                }
            }

            _noodleEventDatas = new Dictionary<CustomEventData, ICustomEventCustomData>();
            foreach (CustomEventData customEventData in eventArgs.CustomEventDatas)
            {
                try
                {
                    ICustomEventCustomData noodleEventData;

                    Dictionary<string, object?> data = customEventData.data;
                    switch (customEventData.type)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            noodleEventData = new NoodlePlayerTrackEventData(GetTrack(customEventData.data, beatmapTracks) ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            noodleEventData = ProcessParentTrackEvent(data, beatmapTracks);
                            break;

                        default:
                            continue;
                    }

                    _noodleEventDatas.Add(customEventData, noodleEventData);
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Log.Logger, e, customEventData);
                }
            }
        }

        private static void FinalizeCustomObject(Dictionary<string, object?> dynData, NoodleObjectData noodleObjectData, Dictionary<string, PointDefinition> pointDefinitions, Dictionary<string, Track> beatmapTracks)
        {
            object? rotation = dynData.Get<object>(ROTATION);
            if (rotation != null)
            {
                if (rotation is List<object> list)
                {
                    List<float> rot = list.Select(Convert.ToSingle).ToList();
                    noodleObjectData.WorldRotationQuaternion = Quaternion.Euler(rot[0], rot[1], rot[2]);
                }
                else
                {
                    noodleObjectData.WorldRotationQuaternion = Quaternion.Euler(0, Convert.ToSingle(rotation), 0);
                }
            }

            Vector3? localrot = dynData.GetVector3(LOCAL_ROTATION);
            if (localrot.HasValue)
            {
                noodleObjectData.LocalRotationQuaternion = Quaternion.Euler(localrot.Value);
            }

            noodleObjectData.Track = GetTrackArray(dynData, beatmapTracks)?.ToList();

            Dictionary<string, object?>? animationObjectDyn = dynData.Get<Dictionary<string, object?>>(ANIMATION);
            if (animationObjectDyn != null)
            {
                NoodleObjectData.AnimationObjectData animationObjectData = new()
                {
                    LocalPosition = TryGetPointData(animationObjectDyn, POSITION, pointDefinitions),
                    LocalRotation = TryGetPointData(animationObjectDyn, ROTATION, pointDefinitions),
                    LocalScale = TryGetPointData(animationObjectDyn, SCALE, pointDefinitions),
                    LocalLocalRotation = TryGetPointData(animationObjectDyn, LOCAL_ROTATION, pointDefinitions),
                    LocalDissolve = TryGetPointData(animationObjectDyn, DISSOLVE, pointDefinitions),
                    LocalDissolveArrow = TryGetPointData(animationObjectDyn, DISSOLVE_ARROW, pointDefinitions),
                    LocalCuttable = TryGetPointData(animationObjectDyn, CUTTABLE, pointDefinitions),
                    LocalDefinitePosition = TryGetPointData(animationObjectDyn, DEFINITE_POSITION, pointDefinitions)
                };
                noodleObjectData.AnimationObject = animationObjectData;
            }

            noodleObjectData.Cuttable = dynData.Get<bool?>(CUTTABLE);
            noodleObjectData.Fake = dynData.Get<bool?>(FAKE_NOTE);

            IEnumerable<float?>? position = dynData.GetNullableFloats(POSITION)?.ToList();
            noodleObjectData.StartX = position?.ElementAtOrDefault(0);
            noodleObjectData.StartY = position?.ElementAtOrDefault(1);

            noodleObjectData.NJS = dynData.Get<float?>(NOTE_JUMP_SPEED);
            noodleObjectData.SpawnOffset = dynData.Get<float?>(NOTE_SPAWN_OFFSET);
            noodleObjectData.AheadTimeInternal = dynData.Get<float?>("aheadTime");
        }

        private static NoodleNoteData ProcessCustomNote(Dictionary<string, object?> dynData)
        {
            NoodleNoteData noodleNoteData = new();

            float? cutDir = dynData.Get<float?>(CUT_DIRECTION);
            if (cutDir.HasValue)
            {
                noodleNoteData.CutQuaternion = Quaternion.Euler(0, 0, cutDir.Value);
            }

            noodleNoteData.FlipYSideInternal = dynData.Get<float?>("flipYSide");
            noodleNoteData.FlipLineIndexInternal = dynData.Get<float?>("flipLineIndex");

            noodleNoteData.StartNoteLineLayerInternal = dynData.Get<float?>("startNoteLineLayer");

            noodleNoteData.DisableGravity = dynData.Get<bool?>(NOTE_GRAVITY_DISABLE) ?? false;
            noodleNoteData.DisableLook = dynData.Get<bool?>(NOTE_LOOK_DISABLE) ?? false;

            return noodleNoteData;
        }

        private static NoodleObstacleData ProcessCustomObstacle(Dictionary<string, object?> dynData)
        {
            NoodleObstacleData noodleObstacleData = new();

            IEnumerable<float?>? scale = dynData.GetNullableFloats(SCALE)?.ToList();
            noodleObstacleData.Width = scale?.ElementAtOrDefault(0);
            noodleObstacleData.Height = scale?.ElementAtOrDefault(1);
            noodleObstacleData.Length = scale?.ElementAtOrDefault(2);

            return noodleObstacleData;
        }

        private static NoodleParentTrackEventData ProcessParentTrackEvent(Dictionary<string, object?> customData, Dictionary<string, Track> beatmapTracks)
        {
            static Quaternion? ToQuaternion(Vector3? vector)
            {
                if (vector.HasValue)
                {
                    return Quaternion.Euler(vector.Value);
                }

                return null;
            }

            Vector3? posVector = customData.GetVector3(POSITION);
            Quaternion? rotQuaternion = ToQuaternion(customData.GetVector3(ROTATION));
            Quaternion? localRotQuaternion = ToQuaternion(customData.GetVector3(LOCAL_ROTATION));
            Vector3? scaleVector = customData.GetVector3(SCALE);

            return new NoodleParentTrackEventData(
                GetTrack(customData, beatmapTracks, PARENT_TRACK) ?? throw new InvalidOperationException("Parent track was not defined."),
                GetTrackArray(customData, beatmapTracks, CHILDREN_TRACKS)?.ToList() ?? throw new InvalidOperationException("Children track was not defined."),
                customData.Get<bool?>(WORLD_POSITION_STAYS) ?? false,
                posVector,
                rotQuaternion,
                localRotQuaternion,
                scaleVector);
        }
    }
}
