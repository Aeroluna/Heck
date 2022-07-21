using System;
using System.Collections.Generic;
using System.Linq;
using CustomJSONData.CustomBeatmap;
using Heck;
using Heck.Animation;
using Heck.Animation.Transform;
using IPA.Utilities;
using UnityEngine;
using static Heck.HeckController;
using static NoodleExtensions.NoodleController;

namespace NoodleExtensions
{
    internal class NoodleNoteData : NoodleBaseNoteData
    {
        private static readonly PropertyAccessor<NoteData, NoteData.ScoringType>.Setter _scoringTypeAccessor =
            PropertyAccessor<NoteData, NoteData.ScoringType>.GetSetter("scoringType");

        internal NoodleNoteData(
            NoteData noteData,
            CustomData customData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2,
            bool leftHanded)
            : base(noteData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
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

                if (Fake.GetValueOrDefault())
                {
                    _scoringTypeAccessor(ref noteData, NoteData.ScoringType.Ignore);
                }
            }
            catch (Exception e)
            {
                Log.Logger.LogFailure(e, noteData);
            }
        }
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
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2,
            bool leftHanded)
            : base(noteData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
        {
            try
            {
                InternalFlipYSide = customData.Get<float?>(INTERNAL_FLIPYSIDE);
                InternalFlipLineIndex = customData.Get<float?>(INTERNAL_FLIPLINEINDEX)?.Mirror(leftHanded);
                InternalStartNoteLineLayer = customData.Get<float?>(INTERNAL_STARTNOTELINELAYER) ?? 0;

                DisableGravity = customData.Get<bool?>(v2 ? V2_NOTE_GRAVITY_DISABLE : NOTE_GRAVITY_DISABLE) ?? false;
                DisableLook = customData.Get<bool?>(v2 ? V2_NOTE_LOOK_DISABLE : NOTE_LOOK_DISABLE) ?? false;

                StartX = StartX?.Mirror(leftHanded);
            }
            catch (Exception e)
            {
                Log.Logger.LogFailure(e, noteData);
            }
        }

        internal float? InternalFlipYSide { get; }

        internal float? InternalFlipLineIndex { get; }

        internal float InternalStartNoteLineLayer { get; }

        internal bool DisableGravity { get; }

        internal bool DisableLook { get; }

        internal float InternalEndRotation { get; set; }
    }

    internal class NoodleObstacleData : NoodleObjectData
    {
        internal NoodleObstacleData(
            ObstacleData obstacleData,
            CustomData customData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2,
            bool leftHanded)
            : base(obstacleData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
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
                    Width = (lineIndex + width) * -1;
                }
            }
            catch (Exception e)
            {
                Log.Logger.LogFailure(e, obstacleData);
            }
        }

        internal Vector3 InternalBoundsSize { get; set; }

        internal float? Width { get; }

        internal float? Height { get; }

        internal float? Length { get; }

        internal bool InternalDoUnhide { get; set; }
    }

    internal class NoodleSliderData : NoodleBaseNoteData, ICopyable<IObjectCustomData>
    {
        internal NoodleSliderData(
            BeatmapObjectData sliderData,
            CustomData customData,
            Dictionary<string, PointDefinition> pointDefinitions,
            Dictionary<string, Track> beatmapTracks,
            bool v2,
            bool leftHanded)
            : base(sliderData, customData, pointDefinitions, beatmapTracks, v2, leftHanded)
        {
            try
            {
                InternalTailStartNoteLineLayer = customData.Get<float?>(INTERNAL_TAILSTARTNOTELINELAYER) ?? 0;

                IEnumerable<float?>? position = customData.GetNullableFloats(TAIL_NOTE_OFFSET)?.ToList();
                TailStartX = position?.ElementAtOrDefault(0)?.Mirror(leftHanded);
                TailStartY = position?.ElementAtOrDefault(1);
            }
            catch (Exception e)
            {
                Log.Logger.LogFailure(e, sliderData);
            }
        }

        internal float? TailStartX { get; }

        internal float? TailStartY { get; }

        internal float InternalTailStartNoteLineLayer { get; }

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
        }

        internal NoodleObjectData(
            BeatmapObjectData beatmapObjectData,
            CustomData customData,
            Dictionary<string, PointDefinition> pointDefinitions,
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

                LocalRotationQuaternion = customData.GetQuaternion(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION)?.Mirror(leftHanded);

                Track = customData.GetNullableTrackArray(beatmapTracks, v2)?.ToList();

                CustomData? animationData = customData.Get<CustomData>(v2 ? V2_ANIMATION : ANIMATION);
                if (animationData != null)
                {
                    AnimationObject = new AnimationObjectData(
                        animationData.GetPointData(v2 ? V2_POSITION : OFFSET_POSITION, pointDefinitions),
                        animationData.GetPointData(v2 ? V2_ROTATION : OFFSET_ROTATION, pointDefinitions),
                        animationData.GetPointData(v2 ? V2_SCALE : SCALE, pointDefinitions),
                        animationData.GetPointData(v2 ? V2_LOCAL_ROTATION : LOCAL_ROTATION, pointDefinitions),
                        animationData.GetPointData(v2 ? V2_DISSOLVE : DISSOLVE, pointDefinitions),
                        animationData.GetPointData(v2 ? V2_DISSOLVE_ARROW : DISSOLVE_ARROW, pointDefinitions),
                        animationData.GetPointData(v2 ? V2_CUTTABLE : INTERACTABLE, pointDefinitions),
                        animationData.GetPointData(v2 ? V2_DEFINITE_POSITION : DEFINITE_POSITION, pointDefinitions));
                }

                Uninteractable = v2 ? !customData.Get<bool?>(V2_CUTTABLE) : customData.Get<bool?>(UNINTERACTABLE);

                IEnumerable<float?>? position = customData.GetNullableFloats(v2 ? V2_POSITION : NOTE_OFFSET)?.ToList();
                StartX = position?.ElementAtOrDefault(0);
                StartY = position?.ElementAtOrDefault(1);

                NJS = customData.Get<float?>(v2 ? V2_NOTE_JUMP_SPEED : NOTE_JUMP_SPEED);
                SpawnOffset = customData.Get<float?>(v2 ? V2_NOTE_SPAWN_OFFSET : NOTE_SPAWN_OFFSET);
            }
            catch (Exception e)
            {
                Log.Logger.LogFailure(e, beatmapObjectData);
            }
        }

        internal Vector3 InternalStartPos { get; set; }

        internal Vector3 InternalMidPos { get; set; }

        internal Vector3 InternalEndPos { get; set; }

        internal Quaternion? WorldRotationQuaternion { get; }

        internal Quaternion? LocalRotationQuaternion { get; }

        internal List<Track>? Track { get; }

        internal Quaternion InternalWorldRotation { get; set; }

        internal Quaternion InternalLocalRotation { get; set; }

        internal AnimationObjectData? AnimationObject { get; }

        internal Vector3 InternalNoteOffset { get; set; }

        internal bool? Uninteractable { get; }

        internal bool? Fake { get; }

        internal float? StartX { get; private protected set; }

        internal float? StartY { get; }

        internal float? NJS { get; }

        internal float? SpawnOffset { get; }

        internal float? InternalAheadTime { get; set; }

        internal float? GetTimeProperty()
        {
            return Track?.Select(n => n.GetLinearProperty(TIME)).FirstOrDefault(n => n.HasValue);
        }

        internal class AnimationObjectData
        {
            public AnimationObjectData(
                PointDefinition? localPosition,
                PointDefinition? localRotation,
                PointDefinition? localScale,
                PointDefinition? localLocalRotation,
                PointDefinition? localDissolve,
                PointDefinition? localDissolveArrow,
                PointDefinition? localCuttable,
                PointDefinition? localDefinitePosition)
            {
                LocalPosition = localPosition;
                LocalRotation = localRotation;
                LocalScale = localScale;
                LocalLocalRotation = localLocalRotation;
                LocalDissolve = localDissolve;
                LocalDissolveArrow = localDissolveArrow;
                LocalCuttable = localCuttable;
                LocalDefinitePosition = localDefinitePosition;
            }

            internal PointDefinition? LocalPosition { get; }

            internal PointDefinition? LocalRotation { get; }

            internal PointDefinition? LocalScale { get; }

            internal PointDefinition? LocalLocalRotation { get; }

            internal PointDefinition? LocalDissolve { get; }

            internal PointDefinition? LocalDissolveArrow { get; }

            internal PointDefinition? LocalCuttable { get; }

            internal PointDefinition? LocalDefinitePosition { get; }
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
        }

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

        internal Track ParentTrack { get; }

        internal List<Track> ChildrenTracks { get; }

        internal bool WorldPositionStays { get; }

        internal Vector3? OffsetPosition { get; }

        internal Quaternion? WorldRotation { get; }

        internal TransformData TransformData { get; }
    }
}
