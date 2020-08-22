namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomJSONData;
    using CustomJSONData.CustomBeatmap;
    using IPA.Utilities;
    using UnityEngine;

    public static class NoteColorizer
    {
        private static readonly HashSet<CNVColorManager> _cnvColorManagers = new HashSet<CNVColorManager>();

        internal static Color?[] NoteColorOverride { get; } = new Color?[2] { null, null };

        public static void Reset(this NoteController nc)
        {
            CNVColorManager.GetCNVColorManager(nc)?.Reset();
        }

        public static void ResetAllNotesColors()
        {
            CNVColorManager.ResetGlobal();

            foreach (CNVColorManager cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.Reset();
            }
        }

        public static void SetNoteColors(this NoteController cnv, Color? color0, Color? color1)
        {
            CNVColorManager.GetCNVColorManager(cnv)?.SetNoteColors(color0, color1);
        }

        public static void SetAllNoteColors(Color? color0, Color? color1)
        {
            CNVColorManager.SetGlobalNoteColors(color0, color1);

            foreach (CNVColorManager cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.Reset();
            }
        }

        public static void SetActiveColors(this NoteController nc)
        {
            CNVColorManager.GetCNVColorManager(nc).SetActiveColors();
        }

        public static void SetAllActiveColors()
        {
            foreach (CNVColorManager cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.SetActiveColors();
            }
        }

        internal static void ClearCNVColorManagers()
        {
            ResetAllNotesColors();
            _cnvColorManagers.Clear();
        }

        internal static void EnableNoteColorOverride(NoteController noteController)
        {
            if (noteController.noteData is CustomNoteData customData)
            {
                dynamic dynData = customData.customData;

                NoteColorOverride[0] = Trees.at(dynData, "color0");
                NoteColorOverride[1] = Trees.at(dynData, "color1");
            }
        }

        internal static void DisableNoteColorOverride()
        {
            NoteColorOverride[0] = null;
            NoteColorOverride[1] = null;
        }

        internal static void ColorizeSaber(INoteController noteController, NoteCutInfo noteCutInfo)
        {
            if (ChromaController.DoColorizerSabers)
            {
                NoteData noteData = noteController.noteData;
                SaberType saberType = noteCutInfo.saberType;
                if ((int)noteData.noteType == (int)saberType)
                {
                    Color color = CNVColorManager.GetCNVColorManager((NoteController)noteController).ColorForCNVManager();

                    SaberColorizer.SetSaberColor(saberType, color);
                }
            }
        }

        /*
         * CNV ColorSO holders
         */

        internal static void CNVStart(ColorNoteVisuals cnv, NoteController nc)
        {
            NoteType noteType = nc.noteData.noteType;
            if (noteType == NoteType.NoteA || noteType == NoteType.NoteB)
            {
                CNVColorManager.CreateCNVColorManager(cnv, nc);
            }
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

            private static readonly FieldAccessor<ColorNoteVisuals, ColorManager>.Accessor _colorManagerAccessor = FieldAccessor<ColorNoteVisuals, ColorManager>.GetAccessor("_colorManager");

            private static readonly Color?[] _globalColor = new Color?[2] { null, null };

            private readonly ColorNoteVisuals _cnv;
            private readonly NoteController _nc;
            private readonly ColorManager _colorManager;
            private CustomNoteData _noteData;

            private CNVColorManager(ColorNoteVisuals cnv, NoteController nc)
            {
                _cnv = cnv;
                _nc = nc;
                _colorManager = _colorManagerAccessor(ref cnv);
                if (nc.noteData is CustomNoteData customNoteData)
                {
                    _noteData = customNoteData;
                }
            }

            internal static CNVColorManager GetCNVColorManager(NoteController nc)
            {
                return _cnvColorManagers.FirstOrDefault(n => n._nc == nc);
            }

            internal static CNVColorManager CreateCNVColorManager(ColorNoteVisuals cnv, NoteController nc)
            {
                CNVColorManager cnvColorManager = GetCNVColorManager(nc);
                if (cnvColorManager != null)
                {
                    if (nc.noteData is CustomNoteData customNoteData)
                    {
                        cnvColorManager._noteData = customNoteData;
                        customNoteData.customData._color0 = _globalColor[0];
                        customNoteData.customData._color1 = _globalColor[1];
                    }

                    return null;
                }

                CNVColorManager cnvcm;
                cnvcm = new CNVColorManager(cnv, nc);
                _cnvColorManagers.Add(cnvcm);
                return cnvcm;
            }

            internal static void SetGlobalNoteColors(Color? color0, Color? color1)
            {
                if (color0.HasValue)
                {
                    _globalColor[0] = color0.Value;
                }

                if (color1.HasValue)
                {
                    _globalColor[1] = color1.Value;
                }
            }

            internal static void ResetGlobal()
            {
                _globalColor[0] = null;
                _globalColor[1] = null;
            }

            internal void Reset()
            {
                _noteData.customData.color0 = _globalColor[0];
                _noteData.customData.color1 = _globalColor[1];
            }

            internal void SetNoteColors(Color? color0, Color? color1)
            {
                if (color0.HasValue)
                {
                    _noteData.customData.color0 = color0.Value;
                }

                if (color1.HasValue)
                {
                    _noteData.customData.color1 = color1.Value;
                }
            }

            internal Color ColorForCNVManager()
            {
                EnableNoteColorOverride(_nc);
                Color noteColor = _colorManager.ColorForNoteType(_noteData.noteType);
                DisableNoteColorOverride();
                return noteColor;
            }

            internal void SetActiveColors()
            {
                ColorNoteVisuals colorNoteVisuals = _cnv;

                Color noteColor = ColorForCNVManager();

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
