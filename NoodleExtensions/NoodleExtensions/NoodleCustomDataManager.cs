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
    using static Heck.Animation.AnimationHelper;
    using static NoodleExtensions.Plugin;

    internal static class NoodleCustomDataManager
    {
        private static Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData> _noodleObjectDatas = new Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData>();
        private static Dictionary<CustomEventData, ICustomEventCustomData> _noodleEventDatas = new Dictionary<CustomEventData, ICustomEventCustomData>();

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

        internal static void DeserializeBeatmapObjects(bool isMultiplayer, IEnumerable<BeatmapObjectData> beatmapObjectsData, CustomBeatmapData beatmapData)
        {
            if (isMultiplayer)
            {
                return;
            }

            _noodleObjectDatas = new Dictionary<BeatmapObjectData, IBeatmapObjectDataCustomData>();
            Dictionary<string, PointDefinition> pointDefinitions = beatmapData.GetBeatmapPointDefinitions();
            Dictionary<string, Track> beatmapTracks = beatmapData.GetBeatmapTracks();
            foreach (BeatmapObjectData beatmapObjectData in beatmapObjectsData)
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
                        FinalizeCustomObject(customData, noodleObjectData, pointDefinitions, beatmapTracks);
                        _noodleObjectDatas.Add(beatmapObjectData, noodleObjectData);
                    }
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Plugin.Logger, e, beatmapObjectData);
                }
            }
        }

        internal static void DeserializeCustomEvents(bool isMultiplayer, IEnumerable<CustomEventData> customEventsData, CustomBeatmapData beatmapData)
        {
            if (isMultiplayer)
            {
                return;
            }

            _noodleEventDatas = new Dictionary<CustomEventData, ICustomEventCustomData>();
            Dictionary<string, Track> beatmapTracks = beatmapData.GetBeatmapTracks();
            foreach (CustomEventData customEventData in customEventsData)
            {
                try
                {
                    ICustomEventCustomData noodleEventData;

                    switch (customEventData.type)
                    {
                        case ASSIGNPLAYERTOTRACK:
                            noodleEventData = new NoodlePlayerTrackEventData(GetTrack(customEventData.data, beatmapTracks) ?? throw new InvalidOperationException("Track was not defined."));
                            break;

                        case ASSIGNTRACKPARENT:
                            noodleEventData = ProcessParentTrackEvent(customEventData.data, beatmapTracks);
                            break;

                        default:
                            continue;
                    }

                    if (noodleEventData != null)
                    {
                        _noodleEventDatas.Add(customEventData, noodleEventData);
                    }
                }
                catch (Exception e)
                {
                    CustomDataDeserializer.LogFailure(Plugin.Logger, e, customEventData);
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

            noodleObjectData.Track = GetTrackArray(dynData, beatmapTracks);

            Dictionary<string, object?>? animationObjectDyn = dynData.Get<Dictionary<string, object?>>("_animation");
            if (animationObjectDyn != null)
            {
                NoodleObjectData.AnimationObjectData animationObjectData = new NoodleObjectData.AnimationObjectData
                {
                    LocalPosition = TryGetPointData(animationObjectDyn, POSITION, pointDefinitions),
                    LocalRotation = TryGetPointData(animationObjectDyn, ROTATION, pointDefinitions),
                    LocalScale = TryGetPointData(animationObjectDyn, SCALE, pointDefinitions),
                    LocalLocalRotation = TryGetPointData(animationObjectDyn, LOCALROTATION, pointDefinitions),
                    LocalDissolve = TryGetPointData(animationObjectDyn, DISSOLVE, pointDefinitions),
                    LocalDissolveArrow = TryGetPointData(animationObjectDyn, DISSOLVEARROW, pointDefinitions),
                    LocalCuttable = TryGetPointData(animationObjectDyn, CUTTABLE, pointDefinitions),
                    LocalDefinitePosition = TryGetPointData(animationObjectDyn, DEFINITEPOSITION, pointDefinitions),
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

        private static NoodleParentTrackEventData ProcessParentTrackEvent(Dictionary<string, object?> customData, Dictionary<string, Track> beatmapTracks)
        {
            IEnumerable<float>? position = customData.Get<List<object>>(POSITION)?.Select(n => Convert.ToSingle(n));
            Vector3? posVector = null;
            if (position != null)
            {
                posVector = new Vector3(position.ElementAt(0), position.ElementAt(1), position.ElementAt(2));
            }

            IEnumerable<float>? rotation = customData.Get<List<object>>(ROTATION)?.Select(n => Convert.ToSingle(n));
            Quaternion? rotQuaternion = null;
            if (rotation != null)
            {
                rotQuaternion = Quaternion.Euler(rotation.ElementAt(0), rotation.ElementAt(1), rotation.ElementAt(2));
            }

            IEnumerable<float>? localrot = customData.Get<List<object>>(LOCALROTATION)?.Select(n => Convert.ToSingle(n));
            Quaternion? localRotQuaternion = null;
            if (localrot != null)
            {
                localRotQuaternion = Quaternion.Euler(localrot.ElementAt(0), localrot.ElementAt(1), localrot.ElementAt(2));
            }

            IEnumerable<float>? scale = customData.Get<List<object>>(SCALE)?.Select(n => Convert.ToSingle(n));
            Vector3? scaleVector = null;
            if (scale != null)
            {
                scaleVector = new Vector3(scale.ElementAt(0), scale.ElementAt(1), scale.ElementAt(2));
            }

            return new NoodleParentTrackEventData(
                GetTrack(customData, beatmapTracks, "_parentTrack") ?? throw new InvalidOperationException("Parent track was not defined."),
                GetTrackArray(customData, beatmapTracks, "_childrenTracks") ?? throw new InvalidOperationException("Children track was not defined."),
                posVector,
                rotQuaternion,
                localRotQuaternion,
                scaleVector);
        }
    }
}
