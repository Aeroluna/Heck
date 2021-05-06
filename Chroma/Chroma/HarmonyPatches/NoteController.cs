namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using Heck;
    using Heck.Animation;
    using IPA.Utilities;
    using UnityEngine;
    using static ChromaObjectDataManager;

    [HeckPatch(typeof(NoteController))]
    [HeckPatch("Update")]
    internal static class NoteControllerUpdate
    {
        private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        private static readonly FieldAccessor<NoteJump, IAudioTimeSource>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, IAudioTimeSource>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        private static void Postfix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            ChromaObjectData chromaData = TryGetObjectData<ChromaObjectData>(____noteData);
            if (chromaData == null)
            {
                return;
            }

            Track track = chromaData.Track;
            PointDefinition pathPointDefinition = chromaData.LocalPathColor;
            if (track != null || pathPointDefinition != null)
            {
                NoteJump noteJump = _noteJumpAccessor(ref ____noteMovement);

                float jumpDuration = _jumpDurationAccessor(ref noteJump);
                float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (____noteData.time - (jumpDuration * 0.5f));
                float normalTime = elapsedTime / jumpDuration;

                Chroma.AnimationHelper.GetColorOffset(pathPointDefinition, track, normalTime, out Color? colorOffset);

                if (colorOffset.HasValue)
                {
                    Color color = colorOffset.Value;
                    if (__instance is BombNoteController bnc)
                    {
                        bnc.SetBombColor(color);
                    }
                    else
                    {
                        __instance.SetNoteColors(color, color);
                        __instance.SetActiveColors();
                    }
                }
            }
        }
    }
}
