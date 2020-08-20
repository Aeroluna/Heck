namespace Chroma.HarmonyPatches
{
    using Chroma.Extensions;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [ChromaPatch(typeof(NoteController))]
    [ChromaPatch("Update")]
    internal static class NoteControllerUpdate
    {
        private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        private static readonly FieldAccessor<NoteJump, AudioTimeSyncController>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, AudioTimeSyncController>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");
        private static readonly FieldAccessor<ColorNoteVisuals, float>.Accessor _arrowGlowIntensityAccessor = FieldAccessor<ColorNoteVisuals, float>.GetAccessor("_arrowGlowIntensity");
        private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _arrowGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_arrowGlowSpriteRenderer");
        private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _circleGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_circleGlowSpriteRenderer");
        private static readonly FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
        private static readonly int _colorID = Shader.PropertyToID("_Color");

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
        private static void Postfix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
        {
            if (Chroma.Plugin.NoodleExtensionsActive)
            {
                TrackColorize(__instance, ____noteData, ____noteMovement);
            }
        }

        private static void TrackColorize(NoteController instance, NoteData noteData, NoteMovement noteMovement)
        {
            if (noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                Track track = AnimationHelper.GetTrack(dynData);
                dynamic animationObject = Trees.at(dynData, "_animation");

                if (track != null || animationObject != null)
                {
                    NoteJump noteJump = _noteJumpAccessor(ref noteMovement);

                    float jumpDuration = _jumpDurationAccessor(ref noteJump);
                    float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (noteData.time - (jumpDuration * 0.5f));
                    float normalTime = elapsedTime / jumpDuration;

                    Chroma.AnimationHelper.GetColorOffset(animationObject, track, normalTime, out Color? colorOffset);

                    if (colorOffset.HasValue)
                    {
                        if (instance is BombNoteController bnc)
                        {
                            bnc.SetBombColor(colorOffset.Value);
                        }
                        else
                        {
                            instance.SetNoteColors(colorOffset.Value, colorOffset.Value);
                            instance.SetActiveColors();
                        }
                    }
                }
            }
        }
    }
}
