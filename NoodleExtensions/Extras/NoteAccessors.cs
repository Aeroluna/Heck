using IPA.Utilities;
using UnityEngine;

namespace NoodleExtensions.Extras
{
    internal static class NoteAccessors
    {
        internal static FieldAccessor<NoteMovement, NoteJump>.Accessor NoteJumpAccessor { get; } = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");

        internal static FieldAccessor<NoteMovement, NoteFloorMovement>.Accessor NoteFloorMovementAccessor { get; } = FieldAccessor<NoteMovement, NoteFloorMovement>.GetAccessor("_floorMovement");

        internal static FieldAccessor<NoteMovement, float>.Accessor ZOffsetAccessor { get; } = FieldAccessor<NoteMovement, float>.GetAccessor("_zOffset");

        internal static FieldAccessor<NoteFloorMovement, Quaternion>.Accessor WorldRotationFloorAccessor { get; } = FieldAccessor<NoteFloorMovement, Quaternion>.GetAccessor("_worldRotation");

        internal static FieldAccessor<NoteFloorMovement, Quaternion>.Accessor InverseWorldRotationFloorAccessor { get; } = FieldAccessor<NoteFloorMovement, Quaternion>.GetAccessor("_inverseWorldRotation");

        internal static FieldAccessor<NoteJump, Quaternion>.Accessor WorldRotationJumpAccessor { get; } = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_worldRotation");

        internal static FieldAccessor<NoteJump, Quaternion>.Accessor InverseWorldRotationJumpAccessor { get; } = FieldAccessor<NoteJump, Quaternion>.GetAccessor("_inverseWorldRotation");

        internal static FieldAccessor<NoteFloorMovement, Vector3>.Accessor FloorEndPosAccessor { get; } = FieldAccessor<NoteFloorMovement, Vector3>.GetAccessor("_endPos");
    }
}
