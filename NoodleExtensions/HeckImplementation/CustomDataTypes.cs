using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using Heck.Deserialize;
using NoodleExtensions.Managers;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions;

internal class NoodleNoteData : NoodleBaseNoteData
{
    internal NoodleNoteData(
        NoteData noteData,
        CustomData customData,
        float bpm,
        Dictionary<string, List<object>> pointDefinitions,
        Dictionary<string, Track> beatmapTracks,
        bool v2,
        bool leftHanded)
        : base(noteData, customData, bpm, pointDefinitions, beatmapTracks, v2, leftHanded)
    {
        try
        {
            if (v2)
            {
                float? cutDir = customData.Get<float?>(V2_CUT_DIRECTION);
                if (cutDir.HasValue)
                {
                    noteData.SetCutDirectionAngleOffset(cutDir.Value.Mirror(leftHanded));
                    if (noteData.cutDirection != NoteCutDirection.Any)
                    {
                        noteData.ChangeNoteCutDirection(NoteCutDirection.Down);
                    }
                }
            }
            else
            {
                Link = customData.Get<string?>(LINK);
            }

            if (Fake.GetValueOrDefault())
            {
                noteData.scoringType = NoteData.ScoringType.Ignore;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.DeserializeFailure(e, noteData, bpm);
        }
    }

    internal string? Link { get; }
}

internal class NoodleBaseNoteData : NoodleObjectData
{
    internal NoodleBaseNoteData(NoodleBaseNoteData original)
        : base(original)
    {
        DisableGravity = original.DisableGravity;
        DisableLook = original.DisableLook;
    }

    internal NoodleBaseNoteData(
        BeatmapObjectData noteData,
        CustomData customData,
        float bpm,
        Dictionary<string, List<object>> pointDefinitions,
        Dictionary<string, Track> beatmapTracks,
        bool v2,
        bool leftHanded)
        : base(noteData, customData, bpm, pointDefinitions, beatmapTracks, v2, leftHanded)
    {
        try
        {
            if (!v2)
            {
                DisableBadCutDirection = customData.Get<bool?>(NOTE_BADCUT_DIRECTION_DISABLE) ?? false;
                DisableBadCutSpeed = customData.Get<bool?>(NOTE_BADCUT_SPEED_DISABLE) ?? false;
                DisableBadCutSaberType = customData.Get<bool?>(NOTE_BADCUT_SABERTYPE_DISABLE) ?? false;
            }

            InternalFlipYSide = customData.Get<float?>(INTERNAL_FLIPYSIDE);
            InternalFlipLineIndex = customData.Get<float?>(INTERNAL_FLIPLINEINDEX);
            InternalStartNoteLineLayer = customData.Get<float?>(INTERNAL_STARTNOTELINELAYER) ?? 0;

            DisableGravity = customData.Get<bool?>(v2 ? V2_NOTE_GRAVITY_DISABLE : NOTE_GRAVITY_DISABLE) ?? false;
            DisableLook = customData.Get<bool?>(v2 ? V2_NOTE_LOOK_DISABLE : NOTE_LOOK_DISABLE) ?? false;
        }
        catch (Exception e)
        {
            Plugin.Log.DeserializeFailure(e, noteData, bpm);
        }
    }

    internal bool DisableBadCutDirection { get; }

    internal bool DisableBadCutSaberType { get; }

    internal bool DisableBadCutSpeed { get; }

    internal bool DisableGravity { get; }

    internal bool DisableLook { get; }

    internal float? InternalFlipLineIndex { get; }

    internal float? InternalFlipYSide { get; }

    internal float InternalStartNoteLineLayer { get; }

    internal float InternalEndRotation { get; set; }
}

internal class NoodleObstacleData : NoodleObjectData
{
    internal NoodleObstacleData(
        ObstacleData obstacleData,
        CustomData customData,
        float bpm,
        Dictionary<string, List<object>> pointDefinitions,
        Dictionary<string, Track> beatmapTracks,
        bool v2,
        bool leftHanded)
        : base(obstacleData, customData, bpm, pointDefinitions, beatmapTracks, v2, leftHanded)
    {
        try
        {
            IEnumerable<float?>? scale = customData.GetNullableFloats(v2 ? V2_SCALE : OBSTACLE_SIZE)?.ToList();
            Width = scale?.ElementAtOrDefault(0);
            Height = scale?.ElementAtOrDefault(1);
            Length = scale?.ElementAtOrDefault(2);

            if (!leftHanded)
            {
                return;
            }

            float width = Width ?? obstacleData.width;
            if (StartX.HasValue)
            {
                StartX = (StartX.Value + width) * -1;
            }
            else if (Width.HasValue)
            {
                float lineIndex = obstacleData.lineIndex - 2;
                StartX = lineIndex - width;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.DeserializeFailure(e, obstacleData, bpm);
        }
    }

    internal float? Height { get; }

    internal float? Length { get; }

    internal float? Width { get; }

    internal Vector3 InternalBoundsSize { get; set; }

    internal bool InternalDoUnhide { get; set; }
}

internal class NoodleSliderData : NoodleBaseNoteData, ICopyable<IObjectCustomData>
{
    internal NoodleSliderData(
        BeatmapObjectData sliderData,
        CustomData customData,
        float bpm,
        Dictionary<string, List<object>> pointDefinitions,
        Dictionary<string, Track> beatmapTracks,
        bool v2,
        bool leftHanded)
        : base(sliderData, customData, bpm, pointDefinitions, beatmapTracks, v2, leftHanded)
    {
        try
        {
            InternalTailStartNoteLineLayer = customData.Get<float?>(INTERNAL_TAILSTARTNOTELINELAYER) ?? 0;

            IEnumerable<float?>? position = customData.GetNullableFloats(TAIL_NOTE_OFFSET)?.ToList();
            TailStartX = position?.ElementAtOrDefault(0);
            TailStartY = position?.ElementAtOrDefault(1);
        }
        catch (Exception e)
        {
            Plugin.Log.DeserializeFailure(e, sliderData, bpm);
        }
    }

    internal float InternalTailStartNoteLineLayer { get; }

    internal float? TailStartX { get; }

    internal float? TailStartY { get; }

    public IObjectCustomData Copy()
    {
        return new NoodleBaseNoteData(this);
    }
}

internal class NoodleObjectData : IObjectCustomData
{
    internal NoodleObjectData(NoodleObjectData original)
    {
        WorldRotationQuaternion = original.WorldRotationQuaternion;
        LocalRotationQuaternion = original.LocalRotationQuaternion;
        Track = original.Track;
        AnimationObject = original.AnimationObject;
        Uninteractable = original.Uninteractable;
        Fake = original.Fake;
        Njs = original.Njs;
        SpawnOffset = original.SpawnOffset;
    }

    internal NoodleObjectData(
        BeatmapObjectData beatmapObjectData,
        CustomData customData,
        float bpm,
        Dictionary<string, List<object>> pointDefinitions,
        Dictionary<string, Track> beatmapTracks,
        bool v2,
        bool leftHanded)
    {
        try
        {
            object? rotation = customData.Get<object>(v2 ? V2_ROTATION : WORLD_ROTATION);
            if (rotation != null)
            {
                if (rotation is List<object> list)
                {
                    List<float> rot = list.Select(Convert.ToSingle).ToList();
                    WorldRotationQuaternion = Quaternion.Euler(rot[0], rot[1], rot[2]).Mirror(leftHanded);
                }
                else
                {
                    WorldRotationQuaternion = Quaternion.Euler(0, Convert.ToSingle(rotation), 0).Mirror(leftHanded);
                }
            }

            Fake = customData.Get<bool?>(v2 ? V2_FAKE_NOTE : INTERNAL_FAKE_NOTE);

            LocalRotationQuaternion =
                customData.GetQuaternion(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION)?.Mirror(leftHanded);

            Track = customData.GetNullableTrackArray(beatmapTracks, v2)?.ToList();

            CustomData? animationData = customData.Get<CustomData>(v2 ? V2_ANIMATION : ANIMATION);
            if (animationData != null)
            {
                AnimationObject = new AnimationObjectData(
                    animationData.GetPointData<Vector3>(v2 ? V2_POSITION : OFFSET_POSITION, pointDefinitions),
                    animationData.GetPointData<Quaternion>(v2 ? V2_ROTATION : OFFSET_ROTATION, pointDefinitions),
                    animationData.GetPointData<Vector3>(v2 ? V2_SCALE : SCALE, pointDefinitions),
                    animationData.GetPointData<Quaternion>(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION, pointDefinitions),
                    animationData.GetPointData<float>(v2 ? V2_DISSOLVE : DISSOLVE, pointDefinitions),
                    animationData.GetPointData<float>(v2 ? V2_DISSOLVE_ARROW : DISSOLVE_ARROW, pointDefinitions),
                    animationData.GetPointData<float>(v2 ? V2_CUTTABLE : INTERACTABLE, pointDefinitions),
                    animationData.GetPointData<Vector3>(
                        v2 ? V2_DEFINITE_POSITION : DEFINITE_POSITION,
                        pointDefinitions));
            }

            Uninteractable = v2 ? !customData.Get<bool?>(V2_CUTTABLE) : customData.Get<bool?>(UNINTERACTABLE);

            IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
            StartX = position?.ElementAtOrDefault(0);
            StartY = position?.ElementAtOrDefault(1);

            if (!v2)
            {
                IEnumerable<float?>? scale = customData.GetNullableFloats(SCALE)?.ToList();
                ScaleX = scale?.ElementAtOrDefault(0);
                ScaleY = scale?.ElementAtOrDefault(1);
                ScaleZ = scale?.ElementAtOrDefault(2);
            }

            Njs = customData.Get<float?>(v2 ? V2_NOTE_JUMP_SPEED : NOTE_JUMP_SPEED);
            SpawnOffset = customData.Get<float?>(v2 ? V2_NOTE_SPAWN_OFFSET : NOTE_SPAWN_OFFSET);
        }
        catch (Exception e)
        {
            Plugin.Log.DeserializeFailure(e, beatmapObjectData, bpm);
        }
    }

    internal AnimationObjectData? AnimationObject { get; }

    internal bool? Fake { get; }

    internal Quaternion? LocalRotationQuaternion { get; }

    internal float? Njs { get; }

    internal float? SpawnOffset { get; }

    internal float? StartY { get; }

    internal IReadOnlyList<Track>? Track { get; }

    internal bool? Uninteractable { get; }

    internal Quaternion? WorldRotationQuaternion { get; }

    internal float? ScaleX { get; }

    internal float? ScaleY { get; }

    internal float? ScaleZ { get; }

    internal float? InternalAheadTime { get; set; }

    internal Vector3 InternalEndPos { get; set; }

    internal Quaternion InternalLocalRotation { get; set; }

    internal Vector3 InternalScale { get; set; }

    internal Vector3 InternalMidPos { get; set; }

    internal Vector3 InternalNoteOffset { get; set; }

    internal Vector3 InternalStartPos { get; set; }

    internal Quaternion InternalWorldRotation { get; set; }

    internal float? StartX { get; private protected set; }

    // This method runs on each frame in the game scene, so avoid allocations (do NOT use Linq).
    internal float? GetTimeProperty()
    {
        IReadOnlyList<Track>? tracks = Track;

        // ReSharper disable once InvertIf
        if (tracks != null)
        {
            // foreach causes an allocation, so we'll use a for loop as this method is called extremely often
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < tracks.Count; i++)
            {
                float? time = tracks[i].GetProperty<float>(TIME);
                if (time.HasValue)
                {
                    return time;
                }
            }
        }

        return null;
    }

    internal class AnimationObjectData(
        PointDefinition<Vector3>? localPosition,
        PointDefinition<Quaternion>? localRotation,
        PointDefinition<Vector3>? localScale,
        PointDefinition<Quaternion>? localLocalRotation,
        PointDefinition<float>? localDissolve,
        PointDefinition<float>? localDissolveArrow,
        PointDefinition<float>? localCuttable,
        PointDefinition<Vector3>? localDefinitePosition)
    {
        internal PointDefinition<float>? LocalCuttable { get; } = localCuttable;

        internal PointDefinition<Vector3>? LocalDefinitePosition { get; } = localDefinitePosition;

        internal PointDefinition<float>? LocalDissolve { get; } = localDissolve;

        internal PointDefinition<float>? LocalDissolveArrow { get; } = localDissolveArrow;

        internal PointDefinition<Quaternion>? LocalLocalRotation { get; } = localLocalRotation;

        internal PointDefinition<Vector3>? LocalPosition { get; } = localPosition;

        internal PointDefinition<Quaternion>? LocalRotation { get; } = localRotation;

        internal PointDefinition<Vector3>? LocalScale { get; } = localScale;
    }
}

internal class NoodlePlayerTrackEventData : ICustomEventCustomData
{
    internal NoodlePlayerTrackEventData(
        CustomData customData,
        Dictionary<string, Track> tracks,
        bool v2)
    {
        Track track = customData.GetTrack(tracks, v2);
        Track = track;

        // DEFAULT TO PLAYER IF NOT SPECIFIED
        PlayerObject =
            customData.GetStringToEnum<PlayerObject?>(v2 ? V2_PLAYER_TRACK_OBJECT : PLAYER_TRACK_OBJECT) ??
            PlayerObject.Root;
    }

    internal PlayerObject PlayerObject { get; }

    internal Track Track { get; }
}

internal class NoodleParentTrackEventData : ICustomEventCustomData
{
    internal NoodleParentTrackEventData(
        CustomData customData,
        Dictionary<string, Track> beatmapTracks,
        bool v2)
    {
        ParentTrack = customData.GetTrack(beatmapTracks, v2 ? V2_PARENT_TRACK : PARENT_TRACK);
        ChildrenTracks = customData.GetTrackArray(beatmapTracks, v2 ? V2_CHILDREN_TRACKS : CHILDREN_TRACKS).ToList();
        WorldPositionStays = customData.Get<bool?>(v2 ? V2_WORLD_POSITION_STAYS : WORLD_POSITION_STAYS) ?? false;
        OffsetPosition = customData.GetVector3(v2 ? V2_POSITION : OFFSET_POSITION);
        WorldRotation = customData.GetQuaternion(v2 ? V2_ROTATION : WORLD_ROTATION);
        TransformData = new TransformData(customData, v2);
    }

    internal IReadOnlyList<Track> ChildrenTracks { get; }

    internal Vector3? OffsetPosition { get; }

    internal Track ParentTrack { get; }

    internal TransformData TransformData { get; }

    internal bool WorldPositionStays { get; }

    internal Quaternion? WorldRotation { get; }
}
