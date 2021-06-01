namespace Chroma.Colorizer
{
    using System.Collections.Generic;
    using IPA.Utilities;
    using UnityEngine;

    public class NoteColorizer : ObjectColorizer
    {
        private static readonly FieldAccessor<ColorNoteVisuals, ColorManager>.Accessor _colorManagerAccessor = FieldAccessor<ColorNoteVisuals, ColorManager>.GetAccessor("_colorManager");
        private static readonly FieldAccessor<ColorNoteVisuals, Color>.Accessor _noteColorAccessor = FieldAccessor<ColorNoteVisuals, Color>.GetAccessor("_noteColor");

        private static readonly FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
        private static readonly int _colorID = Shader.PropertyToID("_Color");

        private readonly MaterialPropertyBlockController[] _materialPropertyBlockControllers;
        private readonly ColorNoteVisuals _colorNoteVisuals;
        private readonly NoteControllerBase _noteController;

        internal NoteColorizer(ColorNoteVisuals colorNoteVisuals, NoteControllerBase noteController)
        {
            _colorNoteVisuals = colorNoteVisuals;
            _materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref colorNoteVisuals);
            _noteController = noteController;

            ColorManager colorManager = _colorManagerAccessor(ref colorNoteVisuals);
            OriginalColors[0] = colorManager.ColorForType(ColorType.ColorA);
            OriginalColors[1] = colorManager.ColorForType(ColorType.ColorB);

            Colorizers.Add(noteController, this);
        }

        public static Dictionary<NoteControllerBase, NoteColorizer> Colorizers { get; } = new Dictionary<NoteControllerBase, NoteColorizer>();

        public static Color?[] GlobalColor { get; private set; } = new Color?[2];

        public Color[] OriginalColors { get; private set; } = new Color[2];

        public ColorType ColorType
        {
            get
            {
                if (_noteController is GameNoteController gameNoteController)
                {
                    NoteData noteData = gameNoteController.noteData;
                    if (noteData != null)
                    {
                        return noteData.colorType;
                    }
                }

                return ColorType.ColorA;
            }
        }

        protected override Color? GlobalColorGetter => GlobalColor[(int)ColorType];

        protected override Color OriginalColorGetter => OriginalColors[(int)ColorType];

        public static void GlobalColorize(Color? color, ColorType colorType)
        {
            GlobalColor[(int)colorType] = color;
            foreach (KeyValuePair<NoteControllerBase, NoteColorizer> valuePair in Colorizers)
            {
                valuePair.Value.Refresh();
            }
        }

        internal static void ColorizeSaber(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (ChromaController.DoColorizerSabers)
            {
                NoteData noteData = noteController.noteData;
                SaberType saberType = noteCutInfo.saberType;
                if ((int)noteData.colorType == (int)saberType)
                {
                    if (noteController.TryGetNoteColorizer(out NoteColorizer noteColorizer))
                    {
                        saberType.ColorizeSaber(noteColorizer.Color);
                    }
                }
            }
        }

        protected override void Refresh()
        {
            Color color = Color;
            ColorNoteVisuals colorNoteVisuals = _colorNoteVisuals;
            if (color == _noteColorAccessor(ref colorNoteVisuals))
            {
                return;
            }

            _noteColorAccessor(ref colorNoteVisuals) = color;
            foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers)
            {
                materialPropertyBlockController.materialPropertyBlock.SetColor(_colorID, color);
                materialPropertyBlockController.ApplyChanges();
            }
        }
    }
}
