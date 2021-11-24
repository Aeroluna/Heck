using System.Collections.Generic;
using Chroma.Colorizer;
using Chroma.Settings;
using Heck;
using Heck.Animation;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using static Chroma.ChromaCustomDataManager;

namespace Chroma.HarmonyPatches.Colorizer
{
    [HeckPatch(typeof(NoteController))]
    [HeckPatch("Update")]
    internal static class NoteControllerUpdate
    {
        private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        private static readonly FieldAccessor<NoteJump, IAudioTimeSource>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, IAudioTimeSource>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        [UsedImplicitly]
        private static void Postfix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            if (ChromaConfig.Instance.NoteColoringDisabled)
            {
                return;
            }

            ChromaObjectData? chromaData = TryGetObjectData<ChromaObjectData>(____noteData);
            if (chromaData == null)
            {
                return;
            }

            List<Track>? tracks = chromaData.Track;
            PointDefinition? pathPointDefinition = chromaData.LocalPathColor;
            if (tracks == null && pathPointDefinition == null)
            {
                return;
            }

            NoteJump noteJump = _noteJumpAccessor(ref ____noteMovement);

            float jumpDuration = _jumpDurationAccessor(ref noteJump);
            float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (____noteData.time - (jumpDuration * 0.5f));
            float normalTime = elapsedTime / jumpDuration;

            AnimationHelper.GetColorOffset(pathPointDefinition, tracks, normalTime, out Color? colorOffset);

            if (!colorOffset.HasValue)
            {
                return;
            }

            Color color = colorOffset.Value;
            if (__instance is BombNoteController)
            {
                __instance.ColorizeBomb(color);
            }
            else
            {
                __instance.ColorizeNote(color);
            }
        }
    }
}
