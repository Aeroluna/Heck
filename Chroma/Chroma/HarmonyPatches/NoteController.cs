namespace Chroma.HarmonyPatches
{
    using Chroma.Utils;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using HarmonyLib;
    using IPA.Utilities;
    using NoodleExtensions.Animation;
    using UnityEngine;

    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Init")]
    internal class NoteControllerInit
    {
#pragma warning disable SA1313
        private static void Prefix(NoteController __instance, NoteData noteData)
#pragma warning restore SA1313
        {
            // They said it couldn't be done, they called me a madman
            if (noteData.noteType == NoteType.Bomb)
            {
                Color? c = null;

                // CustomJSONData _customData individual scale override
                if (noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered)
                {
                    dynamic dynData = customData.customData;

                    c = ChromaUtils.GetColorFromData(dynData, false) ?? c;
                }

                if (!c.HasValue)
                {
                    // I shouldn't hard code this... but i can't be bothered to not atm
                    c = new Color(0.251f, 0.251f, 0.251f, 0);
                }

                Material mat = __instance.noteTransform.gameObject.GetComponent<Renderer>().material;
                mat.SetColor("_SimpleColor", c.Value);
            }
        }
    }

    [HarmonyPatch(typeof(NoteController))]
    [HarmonyPatch("Update")]
    internal class NoteControllerUpdate
    {
        private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
        private static readonly FieldAccessor<NoteJump, AudioTimeSyncController>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, AudioTimeSyncController>.GetAccessor("_audioTimeSyncController");
        private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");
        private static readonly FieldAccessor<ColorNoteVisuals, float>.Accessor _arrowGlowIntensityAccessor = FieldAccessor<ColorNoteVisuals, float>.GetAccessor("_arrowGlowIntensity");
        private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _arrowGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_arrowGlowSpriteRenderer");
        private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _circleGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_circleGlowSpriteRenderer");
        private static readonly FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
        private static readonly int _colorID = Shader.PropertyToID("_Color");

#pragma warning disable SA1313
        private static void Postfix(NoteController __instance, NoteData ____noteData, NoteMovement ____noteMovement)
#pragma warning restore SA1313
        {
            if (____noteData is CustomNoteData customData && ChromaBehaviour.LightingRegistered)
            {
                dynamic dynData = customData.customData;
                Track track = AnimationHelper.GetTrack(dynData);
                dynamic animationObject = Trees.at(dynData, "_animation");

                if (track != null || animationObject != null)
                {
                    NoteJump noteJump = _noteJumpAccessor(ref ____noteMovement);

                    float jumpDuration = _jumpDurationAccessor(ref noteJump);
                    float elapsedTime = _audioTimeSyncControllerAccessor(ref noteJump).songTime - (____noteData.time - (jumpDuration * 0.5f));
                    float normalTime = elapsedTime / jumpDuration;

                    Chroma.AnimationHelper.GetColorOffset(animationObject, track, normalTime, out Color? colorOffset);

                    if (____noteData.noteType == NoteType.Bomb)
                    {
                        Color? c = colorOffset;
                        if (!c.HasValue)
                        {
                            // I shouldn't hard code this... but i can't be bothered to not atm
                            c = new Color(0.251f, 0.251f, 0.251f, 0);
                        }

                        Material mat = __instance.noteTransform.gameObject.GetComponent<Renderer>().material;
                        mat.SetColor("_SimpleColor", c.Value);
                    }
                    else
                    {
                        ColorNoteVisuals colorNoteVisuals = Trees.at(dynData, "colorNoteVisuals");
                        if (colorNoteVisuals == null)
                        {
                            colorNoteVisuals = __instance.gameObject.GetComponent<ColorNoteVisuals>();
                            dynData.colorNoteVisuals = colorNoteVisuals;
                        }

                        if (colorOffset.HasValue)
                        {
                            Color noteColor = colorOffset.Value;

                            ChromaColorManager.SetNoteTypeColourOverride(____noteData.noteType, noteColor);

                            _arrowGlowSpriteRendererAccessor(ref colorNoteVisuals).color = noteColor.ColorWithAlpha(noteColor.a * _arrowGlowIntensityAccessor(ref colorNoteVisuals));
                            _circleGlowSpriteRendererAccessor(ref colorNoteVisuals).color = noteColor;
                            MaterialPropertyBlockController[] materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref colorNoteVisuals);
                            foreach (MaterialPropertyBlockController materialPropertyBlockController in materialPropertyBlockControllers)
                            {
                                materialPropertyBlockController.materialPropertyBlock.SetColor(_colorID, noteColor);
                                materialPropertyBlockController.ApplyChanges();
                            }

                            Events.ChromaNoteColorEvent.SavedNoteColours[__instance] = noteColor;

                            bool? isSubscribed = Trees.at(dynData, "subscribed");
                            if (!isSubscribed.HasValue)
                            {
                                __instance.noteWasCutEvent += Events.ChromaNoteColorEvent.SaberColour;
                                dynData.isSubscribed = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
