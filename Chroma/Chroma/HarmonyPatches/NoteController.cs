namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static ChromaObjectDataManager;
    using static Plugin;

    [ChromaPatch(typeof(NoteController))]
    [ChromaPatch("Update")]
    internal static class NoteControllerUpdate
    {
        private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        private static readonly FieldAccessor<NoteJump, IAudioTimeSource>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, IAudioTimeSource>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");

        private static void Postfix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            if (!(__instance is MultiplayerConnectedPlayerNoteController) && NoodleExtensionsInstalled)
            {
                TrackColorize(__instance, ____noteData, ____noteMovement);
            }
        }

        private static void TrackColorize(NoteController instance, NoteData noteData, NoteMovement noteMovement)
        {
            if (NoodleExtensions.NoodleController.NoodleExtensionsActive)
            {
                ChromaNoodleData chromaData = ChromaNoodleDatas[noteData];

                Track track = chromaData.Track;
                PointDefinition pathPointDefinition = chromaData.LocalPathColor;
                if (track != null || pathPointDefinition != null)
                {
                    NoteJump noteJump = _noteJumpAccessor(ref noteMovement);

                    float jumpDuration = _jumpDurationAccessor(ref noteJump);
                    float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (noteData.time - (jumpDuration * 0.5f));
                    float normalTime = elapsedTime / jumpDuration;

                    Chroma.AnimationHelper.GetColorOffset(pathPointDefinition, track, normalTime, out Color? colorOffset);

                    if (colorOffset.HasValue)
                    {
                        Color color = colorOffset.Value;
                        if (instance is BombNoteController bnc)
                        {
                            bnc.SetBombColor(color);
                        }
                        else
                        {
                            instance.SetNoteColors(color, color);
                            instance.SetActiveColors();
                        }
                    }
                }
            }
        }
    }
}
