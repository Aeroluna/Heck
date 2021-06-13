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

            // Custom notes means there is a chance that _colorManager is null
            ColorManager colorManager = _colorManagerAccessor(ref colorNoteVisuals);
            if (colorManager != null)
            {
                OriginalColors[0] = colorManager.ColorForType(ColorType.ColorA);
                OriginalColors[1] = colorManager.ColorForType(ColorType.ColorB);
            }
            else
            {
                Plugin.Logger.Log("_colorManager was null, defaulting to red/blue", IPA.Logging.Logger.Level.Warning);
            }

            // Override old key because custom notes creates 2 colornotevisuals for every notecontroller
            Colorizers[noteController] = this;
        }

        public static Dictionary<NoteControllerBase, NoteColorizer> Colorizers { get; } = new Dictionary<NoteControllerBase, NoteColorizer>();

        public static Color?[] GlobalColor { get; } = new Color?[2];

        public Color[] OriginalColors { get; } = new Color[2]
        {
            new Color(0.784f, 0.078f, 0.078f),
            new Color(0, 0.463f, 0.823f),
        };

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

        internal static void Reset()
        {
            GlobalColor[0] = null;
            GlobalColor[1] = null;
        }

        internal static void ColorizeSaber(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (ChromaController.DoColorizerSabers)
            {
                NoteData noteData = noteController.noteData;
                SaberType saberType = noteCutInfo.saberType;
                if ((int)noteData.colorType == (int)saberType)
                {
                    saberType.ColorizeSaber(noteController.GetNoteColorizer().Color);
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
