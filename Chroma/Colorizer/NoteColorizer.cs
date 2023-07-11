using System;
using System.Collections.Generic;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    public sealed class NoteColorizerManager : IDisposable
    {
        private readonly NoteColorizer.Factory _factory;
        private readonly BeatmapObjectManager _beatmapObjectManager;
        private readonly SaberColorizerManager _saberManager;
        private readonly SliderColorizerManager _sliderManager;

        private NoteColorizerManager(
            NoteColorizer.Factory factory,
            BeatmapObjectManager beatmapObjectManager,
            SaberColorizerManager saberManager,
            SliderColorizerManager sliderManager,
            [Inject(Optional = true, Id = "dontColorizeSabers")] bool dontColorizeSabers)
        {
            _factory = factory;
            _beatmapObjectManager = beatmapObjectManager;
            _saberManager = saberManager;
            _sliderManager = sliderManager;

            if (!dontColorizeSabers)
            {
                beatmapObjectManager.noteWasCutEvent += ColorizeSaber;
            }
        }

        public Dictionary<NoteControllerBase, NoteColorizer> Colorizers { get; } = new();

        public Color?[] GlobalColor { get; } = new Color?[2];

        public void Dispose()
        {
            _beatmapObjectManager.noteWasCutEvent -= ColorizeSaber;
        }

        public NoteColorizer GetColorizer(NoteControllerBase noteController) => Colorizers[noteController];

        public void Colorize(NoteControllerBase noteController, Color? color) => GetColorizer(noteController).Colorize(color);

        [PublicAPI]
        public void GlobalColorize(Color? color, ColorType colorType)
        {
            GlobalColor[(int)colorType] = color;
            foreach (KeyValuePair<NoteControllerBase, NoteColorizer> valuePair in Colorizers)
            {
                valuePair.Value.Refresh();
            }

            foreach (KeyValuePair<SliderController, SliderColorizer> valuePair in _sliderManager.Colorizers)
            {
                valuePair.Value.Refresh();
            }
        }

        internal void Create(NoteControllerBase noteController)
        {
            Colorizers.Add(noteController, _factory.Create(noteController));
        }

        private void ColorizeSaber(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            NoteData noteData = noteController.noteData;
            SaberType saberType = noteCutInfo.saberType;
            if ((int)noteData.colorType == (int)saberType)
            {
                _saberManager.Colorize(saberType, GetColorizer(noteController).Color);
            }
        }
    }

    [UsedImplicitly]
    public class NoteColorizer : ObjectColorizer
    {
        private static readonly int _colorID = Shader.PropertyToID("_Color");

        private readonly NoteControllerBase _noteController;
        private readonly NoteColorizerManager _manager;
        private readonly ColorManager _colorManager;

        private readonly MaterialPropertyBlockController[] _materialPropertyBlockControllers;
        private readonly ColorNoteVisuals _colorNoteVisuals;

        internal NoteColorizer(
            NoteControllerBase noteController,
            NoteColorizerManager manager,
            ColorManager colorManager)
        {
            _noteController = noteController;
            _colorNoteVisuals = _noteController.GetComponent<ColorNoteVisuals>();
            _materialPropertyBlockControllers = _colorNoteVisuals._materialPropertyBlockControllers;
            _manager = manager;
            _colorManager = colorManager;
        }

        public ColorType ColorType => _noteController.noteData?.colorType ?? ColorType.ColorA;

        protected override Color? GlobalColorGetter
        {
            get
            {
                ColorType colorType = ColorType;
                return colorType == ColorType.None ? null : _manager.GlobalColor[(int)colorType];
            }
        }

        protected override Color OriginalColorGetter => _colorManager.ColorForType(ColorType);

        internal override void Refresh()
        {
            if (!_colorNoteVisuals.isActiveAndEnabled)
            {
                return;
            }

            Color color = Color;
            if (color == _colorNoteVisuals._noteColor)
            {
                return;
            }

            _colorNoteVisuals._noteColor = color;

            foreach (MaterialPropertyBlockController materialPropertyBlockController in _materialPropertyBlockControllers)
            {
                MaterialPropertyBlock propertyBlock = materialPropertyBlockController.materialPropertyBlock;
                Color original = propertyBlock.GetColor(_colorID);
                propertyBlock.SetColor(_colorID, color.ColorWithAlpha(original.a));
                materialPropertyBlockController.ApplyChanges();
            }
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<NoteControllerBase, NoteColorizer>
        {
        }
    }
}
