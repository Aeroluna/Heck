using System.Collections.Generic;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Logger = IPA.Logging.Logger;

namespace Chroma.Colorizer
{
    public class NoteColorizer : ObjectColorizer
    {
        private static readonly FieldAccessor<ColorNoteVisuals, ColorManager>.Accessor _colorManagerAccessor = FieldAccessor<ColorNoteVisuals, ColorManager>.GetAccessor("_colorManager");
        private static readonly FieldAccessor<ColorNoteVisuals, Color>.Accessor _noteColorAccessor = FieldAccessor<ColorNoteVisuals, Color>.GetAccessor("_noteColor");

        private static readonly FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.Accessor _materialPropertyBlockControllersAccessor = FieldAccessor<ColorNoteVisuals, MaterialPropertyBlockController[]>.GetAccessor("_materialPropertyBlockControllers");
        private static readonly int _colorID = Shader.PropertyToID("_Color");

        private readonly NoteControllerBase _noteController;

        // ColorNoteVisuals is not grabbed up front because whatever method that Custom Notes uses to replace ColorNoteVisuals causes wild inconsistency
        // seriously, its annoying. whatever saber factory does is much more consistent
        private ColorNoteVisuals? _colorNoteVisuals;
        private MaterialPropertyBlockController[]? _materialPropertyBlockControllers;
        private Color[]? _originalColors;

        private NoteColorizer(NoteControllerBase noteController)
        {
            _noteController = noteController;
        }

        public static Dictionary<NoteControllerBase, NoteColorizer> Colorizers { get; } = new();

        public static Color?[] GlobalColor { get; } = new Color?[2];

        public Color[] OriginalColors
        {
            get
            {
                if (_originalColors != null)
                {
                    return _originalColors;
                }

                ColorNoteVisuals colorNoteVisuals = ColorNoteVisuals;
                ColorManager colorManager = _colorManagerAccessor(ref colorNoteVisuals);
                if (colorManager != null)
                {
                    _originalColors = new[]
                    {
                        colorManager.ColorForType(ColorType.ColorA),
                        colorManager.ColorForType(ColorType.ColorB)
                    };
                }
                else
                {
                    Log.Logger.Log("_colorManager was null, defaulting to red/blue", Logger.Level.Warning);
                    _originalColors = new Color[]
                    {
                        new(0.784f, 0.078f, 0.078f),
                        new(0, 0.463f, 0.823f)
                    };
                }

                return _originalColors;
            }
        }

        public ColorType ColorType
        {
            get
            {
                if (_noteController is not GameNoteController gameNoteController)
                {
                    return ColorType.ColorA;
                }

                NoteData noteData = gameNoteController.noteData;
                return noteData?.colorType ?? ColorType.ColorA;
            }
        }

        protected override Color? GlobalColorGetter => GlobalColor[(int)ColorType];

        protected override Color OriginalColorGetter => OriginalColors[(int)ColorType];

        private ColorNoteVisuals ColorNoteVisuals
        {
            get
            {
                // Retrieve colornotevisuals on the fly
                if (_colorNoteVisuals != null)
                {
                    return _colorNoteVisuals;
                }

                ColorNoteVisuals colorNoteVisuals = _noteController.GetComponent<ColorNoteVisuals>();
                _colorNoteVisuals = colorNoteVisuals;

                _materialPropertyBlockControllers = _materialPropertyBlockControllersAccessor(ref colorNoteVisuals);

                return _colorNoteVisuals;
            }
        }

        [PublicAPI]
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

        internal static void Create(NoteControllerBase noteController)
        {
            Colorizers.Add(noteController, new NoteColorizer(noteController));
        }

        internal static void ColorizeSaber(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (!ChromaController.DoColorizerSabers)
            {
                return;
            }

            NoteData noteData = noteController.noteData;
            SaberType saberType = noteCutInfo.saberType;
            if ((int)noteData.colorType == (int)saberType)
            {
                saberType.ColorizeSaber(noteController.GetNoteColorizer().Color);
            }
        }

        protected override void Refresh()
        {
            Color color = Color;
            ColorNoteVisuals colorNoteVisuals = ColorNoteVisuals;
            if (color == _noteColorAccessor(ref colorNoteVisuals))
            {
                return;
            }

            _noteColorAccessor(ref colorNoteVisuals) = color;
            foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers!)
            {
                MaterialPropertyBlock propertyBlock = materialPropertyBlockController.materialPropertyBlock;
                Color original = propertyBlock.GetColor(_colorID);
                propertyBlock.SetColor(_colorID, color.ColorWithAlpha(original.a));
                materialPropertyBlockController.ApplyChanges();
            }
        }
    }
}
