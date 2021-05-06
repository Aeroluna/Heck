namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using IPA.Utilities;
    using UnityEngine;
    using static ChromaObjectDataManager;

    // TODO: jesus christ i need to rewrite all the colorizer stuff
    public static class NoteColorizer
    {
        private static readonly Dictionary<NoteController, CNVColorManager> _cnvColorManagers = new Dictionary<NoteController, CNVColorManager>();

        internal static Color?[] NoteColorOverride { get; } = new Color?[2] { null, null };

        public static void Reset(this NoteController nc)
        {
            CNVColorManager.GetCNVColorManager(nc)?.Reset();
        }

        public static void ResetAllNotesColors()
        {
            CNVColorManager.ResetGlobal();

            foreach (KeyValuePair<NoteController, CNVColorManager> cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.Value.Reset();
            }
        }

        public static void SetNoteColors(this NoteController cnv, Color? color0, Color? color1)
        {
            CNVColorManager.GetCNVColorManager(cnv)?.SetNoteColors(color0, color1);
        }

        public static void SetAllNoteColors(Color? color0, Color? color1)
        {
            CNVColorManager.SetGlobalNoteColors(color0, color1);

            foreach (KeyValuePair<NoteController, CNVColorManager> cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.Value.Reset();
            }
        }

        public static void SetActiveColors(this NoteController nc)
        {
            CNVColorManager.GetCNVColorManager(nc).SetActiveColors();
        }

        public static void SetAllActiveColors()
        {
            foreach (KeyValuePair<NoteController, CNVColorManager> cnvColorManager in _cnvColorManagers)
            {
                cnvColorManager.Value.SetActiveColors();
            }
        }

        internal static void ClearCNVColorManagers()
        {
            ResetAllNotesColors();
            _cnvColorManagers.Clear();
        }

        internal static void EnableNoteColorOverride(NoteController noteController)
        {
            ChromaNoteData chromaData = TryGetObjectData<ChromaNoteData>(noteController.noteData);
            if (chromaData != null)
            {
                NoteColorOverride[0] = chromaData.InternalColor ?? CNVColorManager.GlobalColor[0];
                NoteColorOverride[1] = chromaData.InternalColor ?? CNVColorManager.GlobalColor[1];
            }
        }

        internal static void DisableNoteColorOverride()
        {
            NoteColorOverride[0] = null;
            NoteColorOverride[1] = null;
        }

        internal static void ColorizeSaber(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (ChromaController.DoColorizerSabers)
            {
                NoteData noteData = noteController.noteData;
                SaberType saberType = noteCutInfo.saberType;
                if ((int)noteData.colorType == (int)saberType)
                {
                    Color? color = CNVColorManager.GetCNVColorManager(noteController)?.ColorForCNVManager();

                    if (color.HasValue)
                    {
                        SaberColorizer.SetSaberColor(saberType, color.Value);
                    }
                }
            }
        }

        /*
         * CNV ColorSO holders
         */

        internal static void CNVStart(ColorNoteVisuals cnv, NoteController nc)
        {
            ColorType noteType = nc.noteData.colorType;
            if (noteType == ColorType.ColorA || noteType == ColorType.ColorB)
            {
                CNVColorManager.CreateCNVColorManager(cnv, nc);
            }
        }

        private class CNVColorManager
        {
            internal static readonly Color?[] GlobalColor = new Color?[2] { null, null };

            private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _arrowGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_arrowGlowSpriteRenderer");
            private static readonly FieldAccessor<ColorNoteVisuals, SpriteRenderer>.Accessor _circleGlowSpriteRendererAccessor = FieldAccessor<ColorNoteVisuals, SpriteRenderer>.GetAccessor("_circleGlowSpriteRenderer");
            private static readonly FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
            private static readonly int _colorID = Shader.PropertyToID("_Color");

            private static readonly FieldAccessor<ColorNoteVisuals, ColorManager>.Accessor _colorManagerAccessor = FieldAccessor<ColorNoteVisuals, ColorManager>.GetAccessor("_colorManager");
            private static readonly FieldAccessor<ColorNoteVisuals, Color>.Accessor _noteColorAccessor = FieldAccessor<ColorNoteVisuals, Color>.GetAccessor("_noteColor");

            private readonly ColorNoteVisuals _cnv;
            private readonly NoteController _nc;
            private readonly ColorManager _colorManager;
            private ChromaNoteData _chromaData;
            private NoteData _noteData;

            private CNVColorManager(ColorNoteVisuals cnv, NoteController nc, NoteData noteData, ChromaNoteData chromaNoteData)
            {
                _cnv = cnv;
                _nc = nc;
                _colorManager = _colorManagerAccessor(ref cnv);
                _chromaData = chromaNoteData;
                _noteData = noteData;
            }

            internal static CNVColorManager GetCNVColorManager(NoteController nc)
            {
                if (_cnvColorManagers.TryGetValue(nc, out CNVColorManager colorManager))
                {
                    return colorManager;
                }

                return null;
            }

            internal static CNVColorManager CreateCNVColorManager(ColorNoteVisuals cnv, NoteController nc)
            {
                CNVColorManager cnvColorManager = GetCNVColorManager(nc);
                NoteData noteData = nc.noteData;
                ChromaNoteData chromaData = TryGetObjectData<ChromaNoteData>(noteData);
                if (chromaData != null)
                {
                    if (cnvColorManager != null)
                    {
                        cnvColorManager._noteData = noteData;
                        cnvColorManager._chromaData = chromaData;
                        cnvColorManager.Reset();

                        return null;
                    }

                    CNVColorManager cnvcm;
                    cnvcm = new CNVColorManager(cnv, nc, noteData, chromaData);
                    _cnvColorManagers.Add(nc, cnvcm);
                    return cnvcm;
                }

                return null;
            }

            internal static void SetGlobalNoteColors(Color? color0, Color? color1)
            {
                if (color0.HasValue)
                {
                    GlobalColor[0] = color0.Value;
                }

                if (color1.HasValue)
                {
                    GlobalColor[1] = color1.Value;
                }
            }

            internal static void ResetGlobal()
            {
                GlobalColor[0] = null;
                GlobalColor[1] = null;
            }

            internal void Reset()
            {
                _chromaData.InternalColor = null;
            }

            internal void SetNoteColors(Color? color0, Color? color1)
            {
                switch (_noteData.colorType)
                {
                    case ColorType.ColorA:
                        _chromaData.InternalColor = color0.Value;
                        break;

                    case ColorType.ColorB:
                        _chromaData.InternalColor = color1.Value;
                        break;
                }
            }

            internal Color ColorForCNVManager()
            {
                EnableNoteColorOverride(_nc);
                Color noteColor = _colorManager.ColorForType(_noteData.colorType);
                DisableNoteColorOverride();
                return noteColor;
            }

            internal void SetActiveColors()
            {
                ColorNoteVisuals colorNoteVisuals = _cnv;

                Color noteColor = ColorForCNVManager();

                if (noteColor == _noteColorAccessor(ref colorNoteVisuals))
                {
                    return;
                }

                _noteColorAccessor(ref colorNoteVisuals) = noteColor;
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
