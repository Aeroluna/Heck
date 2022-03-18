using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using UnityEngine;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class CustomDataManager
    {
        [EarlyDeserializer]
        internal static void DeserializeEarly(TrackBuilder trackBuilder, List<CustomEventData> customEventDatas)
        {
            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            trackBuilder.AddTrack(customEventData.customData.Get<string>(TRACK) ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            trackBuilder.AddTrack(customEventData.customData.Get<string>(PARENT_TRACK) ?? throw new InvalidOperationException("Parent track was not defined."));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, customEventData);
                }
            }
        }

        [ObjectsDeserializer]
        private static Dictionary<BeatmapObjectData, IObjectCustomData> DeserializeObjects(
            CustomBeatmapData beatmapData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> tracks,
            IReadOnlyList<BeatmapObjectData> beatmapObjectsDatas)
        {
            Dictionary<BeatmapObjectData, IObjectCustomData> dictionary = new();
            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectsDatas)
            {
                try
                {
                    Dictionary<string, object?> customData;
                    NoodleObjectData noodleObjectData;
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

                        default:
                            noodleObjectData = new NoodleObjectData();
                            continue;
                    }

                    FinalizeCustomObject(customData, noodleObjectData, pointDefinitions, tracks);
                    dictionary.Add(beatmapObjectData, noodleObjectData);
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, beatmapObjectData);
                }
            }

            return dictionary;
        }

        [CustomEventsDeserializer]
        private static Dictionary<CustomEventData, ICustomEventCustomData> DeserializeCustomEvents(
            CustomBeatmapData beatmapData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> tracks,
            IReadOnlyList<CustomEventData> customEventDatas)
        {
            Dictionary<CustomEventData, ICustomEventCustomData> dictionary = new();
            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    Dictionary<string, object?> data = customEventData.customData;
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            Track track = customEventData.customData.GetTrack(tracks) ?? throw new InvalidOperationException("Track was not defined.");
                            dictionary.Add(customEventData, new NoodlePlayerTrackEventData(track));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            dictionary.Add(customEventData, ProcessParentTrackEvent(data, tracks));
                            break;

                        default:
                            continue;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.LogFailure(e, customEventData);
                }
            }

            return dictionary;
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

            noodleObjectData.Track = dynData.GetTrackArray(beatmapTracks)?.ToList();

            Dictionary<string, object?>? animationObjectDyn = dynData.Get<Dictionary<string, object?>>(ANIMATION);
            if (animationObjectDyn != null)
            {
                NoodleObjectData.AnimationObjectData animationObjectData = new()
                {
                    LocalPosition = animationObjectDyn.GetPointData(POSITION, pointDefinitions),
                    LocalRotation = animationObjectDyn.GetPointData(ROTATION, pointDefinitions),
                    LocalScale = animationObjectDyn.GetPointData(SCALE, pointDefinitions),
                    LocalLocalRotation = animationObjectDyn.GetPointData(LOCAL_ROTATION, pointDefinitions),
                    LocalDissolve = animationObjectDyn.GetPointData(DISSOLVE, pointDefinitions),
                    LocalDissolveArrow = animationObjectDyn.GetPointData(DISSOLVE_ARROW, pointDefinitions),
                    LocalCuttable = animationObjectDyn.GetPointData(CUTTABLE, pointDefinitions),
                    LocalDefinitePosition = animationObjectDyn.GetPointData(DEFINITE_POSITION, pointDefinitions)
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
        }

        private static NoodleNoteData ProcessCustomNote(Dictionary<string, object?> dynData)
        {
            NoodleNoteData noodleNoteData = new();

            float? cutDir = dynData.Get<float?>(CUT_DIRECTION);
            if (cutDir.HasValue)
            {
                noodleNoteData.CutDirectionAngle = cutDir.Value;
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
                customData.GetTrack(beatmapTracks, PARENT_TRACK) ?? throw new InvalidOperationException("Parent track was not defined."),
                customData.GetTrackArray(beatmapTracks, CHILDREN_TRACKS)?.ToList() ?? throw new InvalidOperationException("Children track was not defined."),
                customData.Get<bool?>(WORLD_POSITION_STAYS) ?? false,
                posVector,
                rotQuaternion,
                localRotQuaternion,
                scaleVector);
        }
    }
}
