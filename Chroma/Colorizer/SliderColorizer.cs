using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Chroma.Colorizer
{
    [UsedImplicitly]
    public class SliderColorizerManager
    {
        private readonly SliderColorizer.Factory _factory;

        internal SliderColorizerManager(
            SliderColorizer.Factory factory)
        {
            _factory = factory;
        }

        public Dictionary<SliderController, SliderColorizer> Colorizers { get; } = new();

        ////public Color? GlobalColor { get; private set; }

        public SliderColorizer GetColorizer(SliderController sliderController) => Colorizers[sliderController];

        public void Colorize(SliderController sliderController, Color? color) => GetColorizer(sliderController).Colorize(color);

        // Global coloring handled by NoteColorizerManager
        /*[PublicAPI]
        public void GlobalColorize(Color? color)
        {
            GlobalColor = color;
            foreach (KeyValuePair<SliderController, SliderColorizer> valuePair in Colorizers)
            {
                valuePair.Value.Refresh();
            }
        }*/

        internal void Create(SliderController sliderController)
        {
            Colorizers.Add(sliderController, _factory.Create(sliderController));
        }
    }

    [UsedImplicitly]
    public class SliderColorizer : ObjectColorizer
    {
        private readonly SliderController _sliderController;
        private readonly NoteColorizerManager _manager;
        private readonly ColorManager _colorManager;

        // Does not handle MirroredSliderController
        internal SliderColorizer(
            SliderController sliderController,
            NoteColorizerManager manager,
            ColorManager colorManager)
        {
            _sliderController = sliderController;
            _manager = manager;
            _colorManager = colorManager;
        }

        public ColorType ColorType => _sliderController.sliderData?.colorType ?? ColorType.ColorA;

        protected override Color? GlobalColorGetter => _manager.GlobalColor[(int)ColorType];

        protected override Color OriginalColorGetter => _colorManager.ColorForType(ColorType);

        internal override void Refresh()
        {
            SliderController sliderController = _sliderController;
            _sliderController._initColor = Color;
        }

        [UsedImplicitly]
        internal class Factory : PlaceholderFactory<SliderController, SliderColorizer>
        {
        }
    }
}
