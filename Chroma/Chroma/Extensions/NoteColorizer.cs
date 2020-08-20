namespace Chroma.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using UnityEngine;

    internal static class NoteColorizer
    {
        private static readonly HashSet<CNVColorManager> _cnvColorManagers = new HashSet<CNVColorManager>();

        internal static void ClearCNVColorManagers()
        {
            _cnvColorManagers.Clear();
        }

        internal static void Reset(this NoteController nc)
        {
            CNVColorManager.GetCNVColorManager(nc)?.Reset();
        }

        internal static void ResetAllNotesColors()
        {
            foreach (CNVColorManager cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.Reset();
            }
        }

        internal static void SetNoteColors(this NoteController cnv, Color? color0, Color? color1)
        {
            CNVColorManager.GetCNVColorManager(cnv)?.SetNoteColors(color0, color1);
        }

        internal static void SetAllNoteColors(Color? color0, Color? color1)
        {
            foreach (CNVColorManager cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.SetNoteColors(color0, color1);
            }
        }

        internal static void SetActiveColors(this NoteController nc)
        {
            CNVColorManager.GetCNVColorManager(nc).SetActiveColors();
        }

        internal static void SetAllActiveColors()
        {
            foreach (CNVColorManager cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.SetActiveColors();
            }
        }

        /*
         * CNV ColorSO holders
         */

        internal static void CNVStart(ColorNoteVisuals cnv, NoteController nc)
        {
            CNVColorManager.CreateCNVColorManager(cnv, nc);
        }

        private class CNVColorManager
        {
            private static readonly FieldAccessor<NoteMovement, NoteJump>.Accessor _noteJumpAccessor = FieldAccessor<NoteMovement, NoteJump>.GetAccessor("_jump");
            private static readonly FieldAccessor<NoteJump, AudioTimeSyncController>.Accessor _audioTimeSyncControllerAccessor = FieldAccessor<NoteJump, AudioTimeSyncController>.GetAccessor("_audioTimeSyncController");
            private static readonly FieldAccessor<NoteJump, float>.Accessor _jumpDurationAccessor = FieldAccessor<NoteJump, float>.GetAccessor("_jumpDuration");
            private static readonly FieldAccessor<ColorNoteVisuals, float>.Accessor _arrowGlowIntensityAccessor = FieldAccessor<ColorNoteVisuals, float>.GetAccessor("_arrowGlowIntensity");
            private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _arrowGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_arrowGlowSpriteRenderer");
            private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _circleGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_circleGlowSpriteRenderer");
            private static readonly FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
            private static readonly int _colorID = Shader.PropertyToID("_Color");

            private readonly ColorNoteVisuals _cnv;
            private readonly NoteController _nc;
            private readonly CustomNoteData _noteData;

            private CNVColorManager(ColorNoteVisuals cnv, NoteController nc)
            {
                _cnv = cnv;
                _nc = nc;
                if (nc.noteData is CustomNoteData)
                {
                    _noteData = _nc.noteData as CustomNoteData;
                }
            }

            internal static CNVColorManager GetCNVColorManager(NoteController nc)
            {
                return _cnvColorManagers.FirstOrDefault(n => n._nc == nc);
            }

            internal static CNVColorManager CreateCNVColorManager(ColorNoteVisuals cnv, NoteController nc)
            {
                if (GetCNVColorManager(nc) != null)
                {
                    return null;
                }

                CNVColorManager cnvcm;
                cnvcm = new CNVColorManager(cnv, nc);
                _cnvColorManagers.Add(cnvcm);
                return cnvcm;
            }

            internal void Reset()
            {
                _noteData.customData._color0 = null;
                _noteData.customData._color1 = null;
            }

            internal void SetNoteColors(Color? color0, Color? color1)
            {
                if (color0.HasValue)
                {
                    _noteData.customData._color0 = color0;
                }

                if (color1.HasValue)
                {
                    _noteData.customData._color1 = color1;
                }
            }

            internal void SetActiveColors()
            {
                ColorNoteVisuals colorNoteVisuals = _cnv;

                Color noteColor = Trees.at(_noteData.customData, "_color" + (int)_noteData.noteType);

                SpriteRenderer arrowGlowSpriteRenderer = _arrowGlowSpriteRendererAccessor(ref colorNoteVisuals);
                SpriteRenderer circleGlowSpriteRenderer = _circleGlowSpriteRendererAccessor(ref colorNoteVisuals);
                arrowGlowSpriteRenderer.color = noteColor.ColorWithAlpha(arrowGlowSpriteRenderer.color.a);
                circleGlowSpriteRenderer.color = noteColor.ColorWithAlpha(circleGlowSpriteRenderer.color.a);
                MaterialPropertyBlockController[] materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref colorNoteVisuals);
                foreach (MaterialPropertyBlockController materialPropertyBlockController in materialPropertyBlockControllers)
                {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(_colorID, noteColor);
                    materialPropertyBlockController.ApplyChanges();
                }
            }
        }
    }
}
