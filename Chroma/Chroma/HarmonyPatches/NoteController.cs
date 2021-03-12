namespace Chroma.HarmonyPatches
{
    using Chroma.Colorizer;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;
    using static Plugin;

    [ChromaPatch(typeof(NoteController))]
    [ChromaPatch("Update")]
    internal static class NoteControllerUpdate
    {
        private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        private static readonly FieldAccessor<NoteJump, IAudioTimeSource>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, IAudioTimeSource>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");
        private static readonly FieldAccessor<ColorNoteVisuals, float>.Accessor _arrowGlowIntensityAccessor = FieldAccessor<ColorNoteVisuals, float>.GetAccessor("_arrowGlowIntensity");
        private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _arrowGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_arrowGlowSpriteRenderer");
        private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _circleGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_circleGlowSpriteRenderer");
        private static readonly FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
        private static readonly int _colorID = Shader.PropertyToID("_Color");

        private static void Postfix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
        {
            if (Chroma.Plugin.NoodleExtensionsInstalled)
            {
                TrackColorize(__instance, ____noteData, ____noteMovement);
            }
        }

        private static void TrackColorize(NoteController instance, NoteData noteData, NoteMovement noteMovement)
        {
            if (NoodleExtensions.NoodleController.NoodleExtensionsActive && noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;
                Track track = AnimationHelper.GetTrack(dynData);
                dynamic animationObject = Trees.at(dynData, ANIMATION);

                if (track != null || animationObject != null)
                {
                    NoteJump noteJump = _noteJumpAccessor(ref noteMovement);

                    float jumpDuration = _jumpDurationAccessor(ref noteJump);
                    float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (noteData.time - (jumpDuration * 0.5f));
                    float normalTime = elapsedTime / jumpDuration;

                    Chroma.AnimationHelper.GetColorOffset(animationObject, track, normalTime, out Color? colorOffset);

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
                            dynData.color0 = color;
                            dynData.color1 = color;
                            instance.SetActiveColors();
                        }
                    }
                }
            }
        }
    }
}
