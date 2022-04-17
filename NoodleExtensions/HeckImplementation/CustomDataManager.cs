using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class CustomDataManager
    {
        [EarlyDeserializer]
        internal static void DeserializeEarly(TrackBuilder trackBuilder, List<CustomEventData> customEventDatas, bool v2)
        {
            foreach (CustomEventData customEventData in customEventDatas)
            {
                try
                {
                    switch (customEventData.eventType)
                    {
                        case ASSIGN_PLAYER_TO_TRACK:
                            trackBuilder.AddTrack(customEventData.customData.Get<string>(v2 ? V2_TRACK : TRACK)
                                                  ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            trackBuilder.AddTrack(customEventData.customData.Get<string>(v2 ? V2_PARENT_TRACK : PARENT_TRACK)
                                                  ?? throw new InvalidOperationException("Parent track was not defined."));
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
            IReadOnlyList<BeatmapObjectData> beatmapObjectsDatas,
            bool v2)
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
                            noodleObjectData = ProcessCustomObstacle(customData, v2);
                            break;

                        case CustomNoteData customNoteData:
                            customData = customNoteData.customData;
                            noodleObjectData = ProcessCustomNote(customNoteData, v2);
                            break;

                        default:
                            noodleObjectData = new NoodleObjectData();
                            continue;
                    }

                    FinalizeCustomObject(customData, noodleObjectData, pointDefinitions, tracks, v2);
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
            IReadOnlyList<CustomEventData> customEventDatas,
            bool v2)
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
                            Track track = customEventData.customData.GetTrack(tracks, v2);
                            dictionary.Add(customEventData, new NoodlePlayerTrackEventData(track));
                            break;

                        case ASSIGN_TRACK_PARENT:
                            dictionary.Add(customEventData, ProcessParentTrackEvent(data, tracks, v2));
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

        private static void FinalizeCustomObject(
            Dictionary<string, object?> dynData,
            NoodleObjectData noodleObjectData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2)
        {
            object? rotation = dynData.Get<object>(v2 ? V2_ROTATION : WORLD_ROTATION);
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

            Vector3? localrot = dynData.GetVector3(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION);
            if (localrot.HasValue)
            {
                noodleObjectData.LocalRotationQuaternion = Quaternion.Euler(localrot.Value);
            }

            noodleObjectData.Track = dynData.GetNullableTrackArray(beatmapTracks, v2)?.ToList();

            Dictionary<string, object?>? animationObjectDyn = dynData.Get<Dictionary<string, object?>>(v2 ? V2_ANIMATION : ANIMATION);
            if (animationObjectDyn != null)
            {
                NoodleObjectData.AnimationObjectData animationObjectData = new()
                {
                    LocalPosition = animationObjectDyn.GetPointData(v2 ? V2_POSITION : OFFSET_POSITION, pointDefinitions),
                    LocalRotation = animationObjectDyn.GetPointData(v2 ? V2_ROTATION : WORLD_ROTATION, pointDefinitions),
                    LocalScale = animationObjectDyn.GetPointData(v2 ? V2_SCALE : SCALE, pointDefinitions),
                    LocalLocalRotation = animationObjectDyn.GetPointData(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION, pointDefinitions),
                    LocalDissolve = animationObjectDyn.GetPointData(v2 ? V2_DISSOLVE : DISSOLVE, pointDefinitions),
                    LocalDissolveArrow = animationObjectDyn.GetPointData(v2 ? V2_DISSOLVE_ARROW : DISSOLVE_ARROW, pointDefinitions),
                    LocalCuttable = animationObjectDyn.GetPointData(v2 ? V2_CUTTABLE : INTERACTABLE, pointDefinitions),
                    LocalDefinitePosition = animationObjectDyn.GetPointData(v2 ? V2_DEFINITE_POSITION : DEFINITE_POSITION, pointDefinitions)
                };
                noodleObjectData.AnimationObject = animationObjectData;
            }

            if (v2)
            {
                noodleObjectData.Uninteractable = !dynData.Get<bool?>(V2_CUTTABLE);
            }
            else
            {
                noodleObjectData.Uninteractable = dynData.Get<bool?>(UNINTERACTABLE);
            }

            // TODO: handle fake shit
            noodleObjectData.Fake = dynData.Get<bool?>(v2 ? V2_FAKE_NOTE : FAKE_NOTE);

            IEnumerable<float?>? position = dynData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
            noodleObjectData.StartX = position?.ElementAtOrDefault(0);
            noodleObjectData.StartY = position?.ElementAtOrDefault(1);

            noodleObjectData.NJS = dynData.Get<float?>(v2 ? V2_NOTE_JUMP_SPEED : NOTE_JUMP_SPEED);
            noodleObjectData.SpawnOffset = dynData.Get<float?>(v2 ? V2_NOTE_SPAWN_OFFSET : NOTE_SPAWN_OFFSET);
        }

        private static NoodleNoteData ProcessCustomNote(CustomNoteData customNoteData, bool v2)
        {
            Dictionary<string, object?> customData = customNoteData.customData;
            NoodleNoteData noodleNoteData = new();

            if (v2)
            {
                float? cutDir = customData.Get<float?>(V2_CUT_DIRECTION);
                if (cutDir.HasValue)
                {
                    customNoteData.SetCutDirectionAngleOffset(cutDir.Value);
                    customNoteData.ChangeNoteCutDirection(NoteCutDirection.Down);
                }
            }

            noodleNoteData.FlipYSideInternal = customData.Get<float?>(INTERNAL_FLIPYSIDE);
            noodleNoteData.FlipLineIndexInternal = customData.Get<float?>(INTERNAL_FLIPLINEINDEX);

            noodleNoteData.StartNoteLineLayerInternal = customData.Get<float?>(INTERNAL_STARTNOTELINELAYER);

            noodleNoteData.DisableGravity = customData.Get<bool?>(v2 ? V2_NOTE_GRAVITY_DISABLE : NOTE_GRAVITY_DISABLE) ?? false;
            noodleNoteData.DisableLook = customData.Get<bool?>(v2 ? V2_NOTE_LOOK_DISABLE : NOTE_LOOK_DISABLE) ?? false;

            return noodleNoteData;
        }

        private static NoodleObstacleData ProcessCustomObstacle(Dictionary<string, object?> dynData, bool v2)
        {
            NoodleObstacleData noodleObstacleData = new();

            IEnumerable<float?>? scale = dynData.GetNullableFloats(v2 ? V2_SCALE : OBSTACLE_SIZE)?.ToList();
            noodleObstacleData.Width = scale?.ElementAtOrDefault(0);
            noodleObstacleData.Height = scale?.ElementAtOrDefault(1);
            noodleObstacleData.Length = scale?.ElementAtOrDefault(2);

            return noodleObstacleData;
        }

        private static NoodleParentTrackEventData ProcessParentTrackEvent(
            Dictionary<string, object?> customData,
            Dictionary<string, Track> beatmapTracks,
            bool v2)
        {
            static Quaternion? ToQuaternion(Vector3? vector)
            {
                if (vector.HasValue)
                {
                    return Quaternion.Euler(vector.Value);
                }

                return null;
            }

            Vector3? posVector = customData.GetVector3(v2 ? V2_POSITION : POSITION);
            Quaternion? rotQuaternion = ToQuaternion(customData.GetVector3(v2 ? V2_ROTATION : WORLD_ROTATION));
            Quaternion? localRotQuaternion = ToQuaternion(customData.GetVector3(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION));
            Vector3? scaleVector = customData.GetVector3(v2 ? V2_SCALE : SCALE);

            return new NoodleParentTrackEventData(
                customData.GetTrack(beatmapTracks, v2 ? V2_PARENT_TRACK : PARENT_TRACK),
                customData.GetTrackArray(beatmapTracks, v2 ? V2_CHILDREN_TRACKS : CHILDREN_TRACKS).ToList(),
                customData.Get<bool?>(v2 ? V2_WORLD_POSITION_STAYS : WORLD_POSITION_STAYS) ?? false,
                posVector,
                rotQuaternion,
                localRotQuaternion,
                scaleVector);
        }
    }
}
