namespace NoodleExtensions
{
    using System.Collections.Generic;
    using Heck;
    using Heck.Animation;
    using UnityEngine;

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

    internal record NoodleObjectData : IBeatmapObjectDataCustomData
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

    internal record NoodlePlayerTrackEventData : ICustomEventCustomData
    {
        internal NoodlePlayerTrackEventData(Track track)
        {
            Track = track;
        }

        internal Track Track { get; set; }
    }

    internal record NoodleParentTrackEventData : ICustomEventCustomData
    {
        internal NoodleParentTrackEventData(Track parentTrack, IEnumerable<Track> childrenTracks, bool worldPositionStays, Vector3? position, Quaternion? rotation, Quaternion? localRotation, Vector3? scale)
        {
            ParentTrack = parentTrack;
            ChildrenTracks = childrenTracks;
            WorldPositionStays = worldPositionStays;
            Position = position;
            Rotation = rotation;
            LocalRotation = localRotation;
            Scale = scale;
        }

        internal Track ParentTrack { get; }

        internal IEnumerable<Track> ChildrenTracks { get; }

        internal bool WorldPositionStays { get; }

        internal Vector3? Position { get; }

        internal Quaternion? Rotation { get; }

        internal Quaternion? LocalRotation { get; }

        internal Vector3? Scale { get; }
    }
}
